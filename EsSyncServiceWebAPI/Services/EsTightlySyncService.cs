

namespace EsSyncService.Services;

public class EsTightlySyncService : BackgroundService

{
    private IServiceProvider ServiceProvider { get; init; }
    private ILogger Logger { get; init; }

    private DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;

    private IConfiguration Configuration { get; set; }

    public EsTightlySyncService(IServiceProvider serviceProvider, IConfiguration configuration)
    {

        this.ServiceProvider = serviceProvider;

        this.Configuration = configuration;

        this.Logger = serviceProvider.GetRequiredService<ILogger>();



    }

    /// <summary>
    /// 重写启动方法
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        while (!stoppingToken.IsCancellationRequested)
        {

            try
            {
                using var scope = ServiceProvider.CreateAsyncScope();

                var settings = new ConnectionSettings(new Uri(Configuration.GetValue<string>("Elasticsearch:Url")!))
                           .DefaultIndex(Configuration.GetValue<string>("Elasticsearch:Index"))
                           .BasicAuthentication(Configuration.GetValue<string>("Elasticsearch:Username"), Configuration.GetValue<string>("Elasticsearch:Password"))
                           .CertificateFingerprint(Configuration.GetValue<string>("Elasticsearch:CertificateFingerprint"));

                // settings.EnableApiVersioningHeader(); // enable ES 7.x compatibility on ES 8.x servers

                var elasticClient = new ElasticClient(settings);

                MysqlDbContext mysqlDbContext = scope.ServiceProvider.GetRequiredService<MysqlDbContext>();
                var isExited = await IndexExistsAsync(Configuration.GetValue<string>("Elasticsearch:Index")!, elasticClient);
                if (!isExited)
                    await CreateIndexAsync(Configuration.GetValue<string>("Elasticsearch:Index")!, elasticClient);

                await UpdateEsDataAsync(Configuration.GetValue<string>("Elasticsearch:Index")!, mysqlDbContext, elasticClient);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }


            await Task.Delay(TimeSpan.FromSeconds(Configuration.GetValue<int>("EsTightlySyncServiceSeconds")!));
        }
    }




    /// <summary>
    /// 重写停止方法
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
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
            try
            {
                var updatedData = await mysqlDbContext.MemoryItems
                    .Where(x => x.UpdateTime > LastUpdateTime)
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

            finally
            {


            }



        }

    }




    /// <summary>
    /// 更新es中的数据
    /// </summary>
    /// <returns></returns>
    public async Task UpdateEsDataAsync(string indexName, MysqlDbContext mysqlDbContext, ElasticClient elasticClient)
    {
        List<Task> tasks = new List<Task>();
        LastUpdateTime = await ReadLastUpdateTimeInEsAsync(Configuration.GetValue<string>("Elasticsearch:LastUpdateTimeIndex")!, elasticClient);
        LastUpdateTime = LastUpdateTime == default ? LastUpdateTime : LastUpdateTime -= TimeSpan.FromDays(1);
        var runTime = DateTime.UtcNow;
        int failedCount = 0;
        int totalCount = 0;
        SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);

        await foreach (var updatedData in GetUpdatedDataFromMysqlAsync(mysqlDbContext))
        {
            if (updatedData.Count == 0)
                Logger.Information("没有数据");
            var tempUpdatedData = updatedData;

            totalCount += tempUpdatedData.Count;

            tasks.Add(
                Task.Run(async () =>
                {
                    var bulkResponse = await elasticClient.BulkAsync(b => b
                        .Index(indexName)
                        .UpdateMany<MemoryItem>(tempUpdatedData, (u, d) => u
                            .Id(d.Id)
                            .DocAsUpsert(true)
                            .Doc(d)
                        )
                    );
                    bulkResponse.Items.ToList().ForEach(async x =>
                    {
                        if (x.Result == "updated")
                            Logger.Information($"更新成功，Id:{x.Id}");
                        else if (x.Result == "created")
                            Logger.Information($"创建成功，Id:{x.Id}");
                        else if (x.Result == "deleted")
                            Logger.Information($"删除成功，Id:{x.Id}");
                        else if (x.Result == "noop")
                            Logger.Information($"无需更新，Id:{x.Id}");
                        else if (x.Result == "not_found")
                            Logger.Warning($"未找到，Id:{x.Id}");
                        else if (x.Result == "error")
                        {
                            Logger.Error($"更新/创建失败，Id:{x.Id}");
                            Logger.Error(x.Error.Reason);
                            try
                            {
                                await semaphoreSlim.WaitAsync(TimeSpan.FromSeconds(10));
                                failedCount++;

                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex.Message);
                            }
                            finally
                            {
                                semaphoreSlim.Release();
                            }

                        }
                    });
                }));
            // bulkResponse.ItemsWithErrors
            //                 .ToList()
            //                 .ForEach(x =>
            //                 {
            //                     System.Console.WriteLine($"更新/创建失败，Id:{x.Id}");
            //                     System.Console.WriteLine(x.Error.Reason);
            //                 });


            // foreach (var document in updatedData)
            // {
            //     var tempDocument = document;
            //     tasks.Add(Task.Run(async () =>
            //     {
            //         var updateResponse = await NestClient.UpdateAsync<MemoryItem>(tempDocument.Id, u => u
            //             .Index(indexName)
            //             .DocAsUpsert(true)
            //             .Doc(tempDocument)
            //         );

            //         if (updateResponse.Result == Nest.Result.Updated)
            //             System.Console.WriteLine($"更新成功，Id:{tempDocument.Id}");
            //         else if (updateResponse.Result == Nest.Result.Created)
            //             System.Console.WriteLine($"创建成功，Id:{tempDocument.Id}");
            //         else if (updateResponse.Result == Nest.Result.Noop)
            //             System.Console.WriteLine($"无需更新，Id:{tempDocument.Id}");
            //         else if (updateResponse.Result == Nest.Result.NotFound)
            //             System.Console.WriteLine($"未找到，Id:{tempDocument.Id}");
            //         else if (updateResponse.Result == Nest.Result.Error)
            //         {
            //             System.Console.WriteLine($"更新/创建失败，Id:{tempDocument.Id}");
            //             System.Console.WriteLine(updateResponse.DebugInformation);
            //         }
            //     }));
            // }


        }
        await Task.WhenAll(tasks);


        Logger.Information($"更新最新同步时间到ES成功，开始时间：{runTime}");
        Logger.Information($"本次耗时：{DateTime.UtcNow - runTime}");

        Logger.Information($"一共处理{totalCount}条数据");
        Logger.Warning($"失败更新{failedCount}条数据");
        LastUpdateTime = runTime;
        await UpdateLastUpdateTimeInEsAsync(Configuration["Elasticsearch:LastUpdateTimeIndex"]!, LastUpdateTime, elasticClient);

    }




    /// <summary>
    /// 判断索引是否存在
    /// </summary>
    /// <param name="indexName"></param>
    /// <returns></returns>
    private async Task<bool> IndexExistsAsync(string indexName, ElasticClient elasticClient)
    {
        var indexExistsResponse = await elasticClient.Indices.ExistsAsync(indexName);

        return indexExistsResponse.Exists;
    }




    /// <summary>
    /// 创建数据索引，默认standard分析器
    /// </summary>
    /// <param name="indexName">索引名</param>
    /// <returns></returns>
    private async Task CreateIndexAsync(string indexName, ElasticClient elasticClient)
    {
        var createIndexResponse = await elasticClient.Indices.CreateAsync(indexName, c => c
            .Map(m => m.AutoMap())
        );
        if (createIndexResponse.IsValid)
            Logger.Information("创建索引成功");
        else
        {
            Logger.Fatal("创建索引失败");
            Logger.Fatal(createIndexResponse.DebugInformation);
        }
    }


    /// <summary>
    /// 从ES中读取最新同步时间
    /// </summary>
    /// <returns></returns>
    private async Task<DateTime> ReadLastUpdateTimeInEsAsync(string indexName, ElasticClient elasticClient)
    {
        var isIndexExist = await IndexExistsAsync(indexName, elasticClient);
        if (!isIndexExist)
            await CreateIndexAsync(indexName, elasticClient);

        var searchResponse = await elasticClient.SearchAsync<LastUpdateTimeIndexDocument>(s => s
            .Index(indexName)
            .Query(q => q
                .MatchAll()
            )
        );

        // 检查是否有匹配的结果
        var lastUpdateTimeIndexDocument = searchResponse.Documents.FirstOrDefault();
        // System.Console.WriteLine(lastUpdateTimeIndexDocument.LastUpdateTime);

        return lastUpdateTimeIndexDocument?.LastUpdateTime ?? default;

        // 获取第一个匹配的文档
        // 输出文档的内容
    }




    /// <summary>
    /// 更新最新同步时间到ES
    /// </summary>
    /// <param name="indexName"></param>
    /// <param name="lastUpdateTime"></param>
    /// <returns></returns>
    private async Task UpdateLastUpdateTimeInEsAsync(string indexName, DateTime lastUpdateTime, ElasticClient elasticClient)
    {
        var isIndexExist = await IndexExistsAsync(indexName, elasticClient);
        if (!isIndexExist)
            await CreateIndexAsync(indexName, elasticClient);

        var updateResponse = await elasticClient.UpdateAsync<LastUpdateTimeIndexDocument>(1, u => u
            .Index(indexName)
            .DocAsUpsert(true)
            .Doc(new LastUpdateTimeIndexDocument
            {
                Id = 1,
                LastUpdateTime = lastUpdateTime
            })
        );

    }

}
