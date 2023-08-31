namespace EsSyncServiceWebAPI;

public class EsLooseSyncService : BackgroundService
{

    private IServiceProvider ServiceProvider { get; init; }

    public IElasticClient NestClient { get; set; }


    private IConfiguration Configuration { get; set; }

    public EsLooseSyncService(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        this.ServiceProvider = serviceProvider;

        this.Configuration = configuration;

        var settings = new ConnectionSettings(new Uri(Configuration.GetValue<string>("Elasticsearch:Url")!))
                   .DefaultIndex(Configuration.GetValue<string>("Elasticsearch:Index"))
                   .BasicAuthentication(Configuration.GetValue<string>("Elasticsearch:Username"), Configuration.GetValue<string>("Elasticsearch:Password"))
                   .CertificateFingerprint(Configuration.GetValue<string>("Elasticsearch:CertificateFingerprint"));

        NestClient = new ElasticClient(settings);

    }

    /// <summary>
    /// 重写启动方法
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = ServiceProvider.CreateAsyncScope();
        MysqlDbContext mysqlDbContext = scope.ServiceProvider.GetRequiredService<MysqlDbContext>();

        var isExited = await IndexExistsAsync(Configuration.GetValue<string>("Elasticsearch:Index")!);
        if (!isExited)
            await CreateIndexAsync(Configuration.GetValue<string>("Elasticsearch:Index")!);

        while (!stoppingToken.IsCancellationRequested)
        {
            System.Console.WriteLine($"开始修补同步数据到ES，时间：{DateTime.UtcNow}");
            int totalCount = 0;
            int successCount = 0;
            DateTime startTime = DateTime.UtcNow;

            using var transaction = await mysqlDbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            var pageSize = 1000;
            var pageIndex = 0;
            List<Task> tasks = new();

            while (!stoppingToken.IsCancellationRequested)
            {

                var data = await mysqlDbContext.MemoryItems
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
                    var tempItem = item;
                    tasks.Add(Task.Run(async () =>
                    {

                        var isInEs = await IsMysqlSingleDataInESAsync(Configuration.GetValue<string>("Elasticsearch:Index")!, item.Id);
                        if (!isInEs)
                        {
                            totalCount++;
                            var indexResponse = await NestClient.IndexDocumentAsync(item);
                            if (indexResponse.IsValid)
                            {
                                successCount++;

                                System.Console.WriteLine($"索引{Configuration.GetValue<string>("Elasticsearch:Index")!}修补数据成功，id为{item.Id}");
                            }
                            else
                            {
                                System.Console.WriteLine($"索引{Configuration.GetValue<string>("Elasticsearch:Index")!}修补数据失败，id为{item.Id}\nReason: {indexResponse.DebugInformation}");

                            }
                        }
                    }));
                }

            }
            await Task.WhenAll(tasks);
            DateTime endTime = DateTime.UtcNow;
            System.Console.WriteLine($"结束修补同步数据到ES，时间：{endTime} 耗时：{(endTime - startTime).TotalSeconds}秒");
            System.Console.WriteLine($"共修补{totalCount}条数据，成功{successCount}条，失败{totalCount - successCount}条");
            await Task.Delay(TimeSpan.FromHours(Configuration.GetValue<int>("EsLooseSyncServiceHours")));

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
    /// 判断单条数据是否在ES中
    /// </summary>
    /// <param name="indexName"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    private async Task<bool> IsMysqlSingleDataInESAsync(string indexName, Int64 id)
    {
        var searchResponse = await NestClient.SearchAsync<MemoryItem>(s => s
            .Query(q => q
                .Term(t => t
                    .Field(f => f.Id)
                    .Value(id)
                )
            )
        );

        // 检查是否有匹配的结果
        if (searchResponse.IsValid && searchResponse.Documents.Any())
            return true;
        else
            return false;

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
            System.Console.WriteLine("创建索引成功");
        else
            System.Console.WriteLine("创建索引失败");
    }


}
