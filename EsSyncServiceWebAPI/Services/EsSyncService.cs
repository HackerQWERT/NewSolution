using System.Data;
using EsSyncService.Models;
using Microsoft.EntityFrameworkCore;
using Nest;

namespace EsSyncService.Services;

public class EsSyncService : BackgroundService

{
    private IServiceProvider ServiceProvider { get; init; }

    public IElasticClient NestClient { get; set; }

    private DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;

    private IConfiguration Configuration { get; set; }

    public EsSyncService(IServiceProvider serviceProvider, IConfiguration configuration)
    {

        this.ServiceProvider = serviceProvider;

        this.Configuration = configuration;


        var settings = new ConnectionSettings(new Uri(Configuration.GetValue<string>("Elasticsearch:Url")!))
                   .DefaultIndex(Configuration.GetValue<string>("Elasticsearch:Index"))
                   .BasicAuthentication(Configuration.GetValue<string>("Elasticsearch:Username"), Configuration.GetValue<string>("Elasticsearch:Password"))
                   .CertificateFingerprint(Configuration.GetValue<string>("Elasticsearch:CertificateFingerprint"));

        NestClient = new ElasticClient(settings);

    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var scope = ServiceProvider.CreateAsyncScope();
        MysqlDbContext mysqlDbContext = scope.ServiceProvider.GetRequiredService<MysqlDbContext>();

        var isExited = await IndexExistsAsync(Configuration.GetValue<string>("Elasticsearch:Index")!);
        if (!isExited)
            await CreateIndexAsync(Configuration.GetValue<string>("Elasticsearch:Index")!);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdateEsDataAsync(Configuration.GetValue<string>("Elasticsearch:Index")!, mysqlDbContext);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

            await Task.Delay(TimeSpan.FromSeconds(10));
        }
    }
    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        await base.StopAsync(stoppingToken);
    }



    /// <summary>
    /// 从mysql中获取更新的或插入的数据
    /// </summary>
    /// <returns></returns>
    public async IAsyncEnumerable<List<Models.MemoryItem>> GetUpdatedDataFromMysqlAsync(MysqlDbContext mysqlDbContext)
    {
        using var transaction = await mysqlDbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        var pageSize = 1000;
        var pageIndex = 0;

        while (true)
        {
            System.Console.WriteLine(LastUpdateTime);

            var updatedData = await mysqlDbContext.MemoryItems
                .Where(x => x.UpdateTime > LastUpdateTime && x.UpdateTime < DateTime.UtcNow)
                .OrderBy(x => x.Id)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (updatedData.Count == 0)
            {
                break;
            }

            yield return updatedData;

            pageIndex++;
        }

    }




    /// <summary>
    /// 更新es中的数据
    /// </summary>
    /// <returns></returns>
    public async Task UpdateEsDataAsync(string indexName, MysqlDbContext mysqlDbContext)
    {

        List<Task> tasks = new List<Task>();
        var runTime = DateTime.UtcNow;

        await foreach (var updatedData in GetUpdatedDataFromMysqlAsync(mysqlDbContext))
        {
            foreach (var document in updatedData)
            {
                var tempDocument = document;
                tasks.Add(Task.Run(async () =>
                {
                    var updateResponse = await NestClient.UpdateAsync<MemoryItem>(tempDocument.Id, u => u
                        .Index(indexName)
                        .DocAsUpsert(true)
                        .Doc(tempDocument)
                    );

                    if (updateResponse.Result == Nest.Result.Updated)
                        System.Console.WriteLine($"更新成功，Id:{tempDocument.Id}");
                    else if (updateResponse.Result == Nest.Result.Created)
                        System.Console.WriteLine($"创建成功，Id:{tempDocument.Id}");
                    else if (updateResponse.Result == Nest.Result.Noop)
                        System.Console.WriteLine($"无需更新，Id:{tempDocument.Id}");
                    else if (updateResponse.Result == Nest.Result.NotFound)
                        System.Console.WriteLine($"未找到，Id:{tempDocument.Id}");
                    else if (updateResponse.Result == Nest.Result.Error)
                    {
                        System.Console.WriteLine($"更新/创建失败，Id:{tempDocument.Id}");
                        System.Console.WriteLine(updateResponse.DebugInformation);
                    }
                }));
            }
        }
        await Task.WhenAll(tasks);
        LastUpdateTime = runTime;
    }




    /// <summary>
    /// 判断索引是否存在
    /// </summary>
    /// <param name="indexName"></param>
    /// <returns></returns>
    private async Task<bool> IndexExistsAsync(string indexName)
    {
        var indexExistsResponse = await NestClient.Indices.ExistsAsync(indexName);

        return indexExistsResponse.Exists;
    }



    /// <summary>
    /// 创建数据索引，默认standard分析器
    /// </summary>
    /// <param name="indexName">索引名</param>
    /// <returns></returns>
    private async Task CreateIndexAsync(string indexName)
    {
        var createIndexResponse = await NestClient.Indices.CreateAsync(indexName, c => c
            .Map(m => m.AutoMap())
        );
        if (createIndexResponse.IsValid)
            System.Console.WriteLine("创建索引成功");
        else
            System.Console.WriteLine("创建索引失败");
    }





}


