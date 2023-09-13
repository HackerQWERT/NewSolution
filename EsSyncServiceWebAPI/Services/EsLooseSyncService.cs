using System.Security.Cryptography.Xml;

namespace EsSyncService.Services;

public class EsLooseSyncService : BackgroundService
{

    private IServiceProvider ServiceProvider { get; init; }

    public IElasticClient NestClient { get; set; }

    private ILogger Logger { get; init; }
    private DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;

    private IConfiguration Configuration { get; set; }

    public EsLooseSyncService(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        this.ServiceProvider = serviceProvider;

        this.Configuration = configuration;

        this.Logger = serviceProvider.GetRequiredService<ILogger>();

        var settings = new ConnectionSettings(new Uri(Configuration.GetValue<string>("Elasticsearch:Url")!))
                   .DefaultIndex(Configuration.GetValue<string>("Elasticsearch:Index"))
                   .BasicAuthentication(Configuration.GetValue<string>("Elasticsearch:Username"), Configuration.GetValue<string>("Elasticsearch:Password"))
                   .CertificateFingerprint(Configuration.GetValue<string>("Elasticsearch:CertificateFingerprint"));
        settings.EnableApiVersioningHeader(); // enable ES 7.x compatibility on ES 8.x servers

        NestClient = new ElasticClient(settings);

    }

    /// <summary>
    /// 重写启动方法
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // await Task.Delay(TimeSpan.FromHours(Configuration.GetValue<int>("EsLooseSyncServiceSuccessfulHours")));
        while (!stoppingToken.IsCancellationRequested)
        {
            if (!NestClient.Ping().IsValid)
                Logger.Error("无法连接到Elasticsearch");
            else
                Logger.Information("连接到Elasticsearch成功");
            try
            {
                using var scope = ServiceProvider.CreateAsyncScope();

                LastUpdateTime = await ReadLastUpdateTimeInEsAsync(Configuration.GetValue<string>("Elasticsearch:LooseLastUpdateTimeIndex")!);

                LastUpdateTime = LastUpdateTime - TimeSpan.FromDays(1);


                MysqlDbContext mysqlDbContext = scope.ServiceProvider.GetRequiredService<MysqlDbContext>();

                var semaphoreSlim = new SemaphoreSlim(1, 1);

                var updateEsDataSemaphoreSlim = new SemaphoreSlim(5, 5);


                var isExited = await IndexExistsAsync(Configuration.GetValue<string>("Elasticsearch:Index")!);
                if (!isExited)
                    await CreateIndexAsync(Configuration.GetValue<string>("Elasticsearch:Index")!);

                Logger.Information($"开始修补同步数据到ES，时间：{DateTime.UtcNow}");
                int totalCount = 0;
                int failedCount = 0;
                DateTime startTime = DateTime.UtcNow;

                using var transaction = await mysqlDbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

                var pageSize = 1000;
                var pageIndex = 0;
                while (!stoppingToken.IsCancellationRequested)
                {
                    List<Task> tasks = new();

                    var data = await mysqlDbContext.MemoryItems
                        .Where(x => x.UpdateTime > LastUpdateTime)
                        .OrderBy(x => x.Id)
                        .Skip(pageIndex * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                    if (data.Count == 0)
                    {
                        break;
                    }
                    pageIndex++;

                    foreach (var item in data)
                    {
                        //14991142
                        var tempItem = item;
                        tasks.Add(Task.Run(async () =>
                        {
                            try
                            {

                            Start:
                                await updateEsDataSemaphoreSlim.WaitAsync();
                                //查询Mysql数据是否存在es
                                //var searchResponse = await IsMysqlSingleDataInESAsync(Configuration.GetValue<string>("Elasticsearch:Index")!, tempItem.Id);
                                var searchResponse = await NestClient.SearchAsync<MemoryItem>(s => s
                                    .Query(q => q
                                        .Term(t => t
                                            .Field(f => f.Id)
                                            .Value(tempItem.Id)
                                        )
                                    )
                                );
                                // 如果存在
                                if (searchResponse.IsValid && searchResponse.Documents.Any())
                                {
                                    Logger.Information(searchResponse.Documents.FirstOrDefault()!.UpdateTime.ToString() + "\t" + searchResponse.Documents.FirstOrDefault()!.Id);

                                    Logger.Information(tempItem.UpdateTime.ToString() + "\t" + tempItem.Id);
                                    //检查数据如果不是最新的
                                    if (searchResponse.Documents.FirstOrDefault()!.UpdateTime != tempItem.UpdateTime)
                                    {
                                        try
                                        {
                                            await semaphoreSlim.WaitAsync(TimeSpan.FromSeconds(10));
                                            totalCount++;
                                        }
                                        catch (Exception ex)
                                        {
                                            Logger.Error(ex.Message);
                                        }
                                        finally
                                        {
                                            semaphoreSlim.Release();
                                        }
                                        //更新数据到es
                                        var indexResponse = await NestClient.IndexDocumentAsync(tempItem);
                                        if (indexResponse.IsValid)
                                        {
                                            Logger.Information($"索引{Configuration.GetValue<string>("Elasticsearch:Index")!}修补更新数据成功，id为{tempItem.Id}");
                                        }
                                        else
                                        {
                                            Logger.Error(tempItem.UpdateTime.ToString() + "\t" + tempItem.Id + "\n" + indexResponse.DebugInformation);
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
                                            Logger.Information($"索引{Configuration.GetValue<string>("Elasticsearch:Index")!}修补更新数据失败，id为{tempItem.Id}\nReason: {indexResponse.DebugInformation}");

                                        }
                                    }
                                }
                                //如果无效查询
                                else if (!searchResponse.IsValid)
                                {
                                    Logger.Error(searchResponse.DebugInformation);
                                    updateEsDataSemaphoreSlim.Release();
                                    await Task.Delay(2000);
                                    goto Start;
                                }
                                //如果不存在
                                else
                                {
                                    try
                                    {
                                        await semaphoreSlim.WaitAsync(TimeSpan.FromSeconds(10));
                                        totalCount++;
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Console.WriteLine(ex.Message);
                                    }
                                    finally
                                    {
                                        semaphoreSlim.Release();
                                    }
                                    //插入数据到es
                                    var indexResponse = await NestClient.IndexDocumentAsync(tempItem);
                                    if (indexResponse.IsValid)
                                    {
                                        System.Console.WriteLine($"索引{Configuration.GetValue<string>("Elasticsearch:Index")!}修补插入数据成功，id为{tempItem.Id}");
                                    }
                                    else
                                    {
                                        Logger.Error(tempItem.UpdateTime.ToString() + "\t" + tempItem.Id + "\n" + indexResponse.DebugInformation);

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
                                        Logger.Error($"索引{Configuration.GetValue<string>("Elasticsearch:Index")!}修补插入数据失败，id为{tempItem.Id}\nReason: {indexResponse.DebugInformation}");
                                    }

                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex.Message);


                            }
                            finally
                            {
                                updateEsDataSemaphoreSlim.Release();
                            }


                        }));
                    }
                    await Task.WhenAll(tasks);
                }

                DateTime endTime = DateTime.UtcNow;
                Logger.Information($"结束修补同步数据到ES，时间：{endTime} 耗时：{(endTime - startTime).TotalSeconds}秒");
                Logger.Information($"共修补{totalCount}条数据，成功{totalCount - failedCount}条，失败{failedCount}条");
                if (failedCount == 0)
                {
                    await UpdateLastUpdateTimeInEsAsync(Configuration.GetValue<string>("LooseLastUpdateTimeIndex")!, startTime);
                    Logger.Information($"更新最新同步时间到ES成功，时间：{startTime}");
                    await Task.Delay(TimeSpan.FromHours(Configuration.GetValue<int>("EsLooseSyncServiceSuccessfulHours")));
                }
                else
                {
                    Logger.Error($"更新最新同步时间到ES失败，时间：{startTime}");
                    await Task.Delay(TimeSpan.FromHours(Configuration.GetValue<int>("EsLooseSyncServiceFailedHours")));
                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

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
    /// 更新最新同步时间到ES
    /// </summary>
    /// <param name="indexName"></param>
    /// <param name="lastUpdateTime"></param>
    /// <returns></returns>
    private async Task UpdateLastUpdateTimeInEsAsync(string indexName, DateTime lastUpdateTime)
    {
        var isIndexExist = await IndexExistsAsync(indexName);
        if (!isIndexExist)
            await CreateIndexAsync(indexName);

        var updateResponse = await NestClient.UpdateAsync<LastUpdateTimeIndexDocument>(1, u => u
            .Index(indexName)
            .DocAsUpsert(true)
            .Doc(new LastUpdateTimeIndexDocument
            {
                Id = 1,
                LastUpdateTime = lastUpdateTime
            })
        );

    }
    /// <summary>
    /// 判断单条数据是否在ES中
    /// </summary>
    /// <param name="indexName"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    private async Task<ISearchResponse<MemoryItem>> IsMysqlSingleDataInESAsync(string indexName, Int64 id)
    {
        var searchResponse = await NestClient.SearchAsync<MemoryItem>(s => s
            .Query(q => q
                .Term(t => t
                    .Field(f => f.Id)
                    .Value(id)
                )
            )
        );
        return searchResponse;
        // // 检查是否有匹配的结果
        // if (searchResponse.IsValid && searchResponse.Documents.Any())
        //     return true;
        // else
        //     return false;

    }


    /// <summary>
    /// 从ES中读取最新同步时间
    /// </summary>
    /// <returns></returns>
    private async Task<DateTime> ReadLastUpdateTimeInEsAsync(string indexName)
    {
        var isIndexExist = await IndexExistsAsync(indexName);
        if (!isIndexExist)
            await CreateIndexAsync(indexName);

        var searchResponse = await NestClient.SearchAsync<LastUpdateTimeIndexDocument>(s => s
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
    /// 判断索引是否存在
    /// </summary>
    /// 
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
            Logger.Information("创建索引成功");
        else
            Logger.Error("创建索引失败");
    }


}
