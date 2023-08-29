using System.Data;
using System.Diagnostics;
using EsSyncService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using Nest;

namespace EsSyncService.Services;

public class EsSyncService : BackgroundService

{
    private IServiceProvider ServiceProvider { get; init; }

    public IElasticClient NestClient { get; set; }

    private DateTime LastUpdateTime { get; set; } = default;

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

        var isExited = await IndexExistsAsync(Configuration.GetValue<string>("Elasticsearch:Index")!);
        if (!isExited)
            await CreateIndexAsync(Configuration.GetValue<string>("Elasticsearch:Index")!);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdateEsDataAsync(Configuration.GetValue<string>("Elasticsearch:Index")!);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

            await Task.Delay(TimeSpan.FromMinutes(60));
        }
    }

    /// <summary>
    /// 从mysql中获取更新的或插入的数据
    /// </summary>
    /// <returns></returns>
    public async IAsyncEnumerable<List<Models.MemoryItem>> GetUpdatedDataFromMysqlAsync()
    {
        var scope = ServiceProvider.CreateAsyncScope();

        var mysqlDbContext = scope.ServiceProvider.GetRequiredService<MysqlDbContext>();

        using var transaction = await mysqlDbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        var pageSize = 1000;
        var pageIndex = 0;

        while (true)
        {
            var updatedData = await mysqlDbContext.MemoryItems
                .Where(x => x.UpdateTime > LastUpdateTime && x.UpdateTime < DateTime.Now)
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
    public async Task UpdateEsDataAsync(string indexName)
    {
        var scope = ServiceProvider.CreateAsyncScope();

        List<Task> tasks = new List<Task>();
        var runTime = DateTime.Now;

        await foreach (var updatedData in GetUpdatedDataFromMysqlAsync())
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
                    else
                        System.Console.WriteLine($"更新失败，Id:{tempDocument.Id}");
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


