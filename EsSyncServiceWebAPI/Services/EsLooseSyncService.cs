namespace EsSyncServiceWebAPI;

public class EsLooseSyncService : BackgroundService
{

    private IServiceProvider ServiceProvider { get; init; }

    public IElasticClient NestClient { get; set; }

    private DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;

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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        var scope = ServiceProvider.CreateAsyncScope();
        MysqlDbContext mysqlDbContext = scope.ServiceProvider.GetRequiredService<MysqlDbContext>();

        var isExited = await IndexExistsAsync(Configuration.GetValue<string>("Elasticsearch:Index")!);
        if (!isExited)
            await CreateIndexAsync(Configuration.GetValue<string>("Elasticsearch:Index")!);

        while (!stoppingToken.IsCancellationRequested)
        {
            using var transaction = await mysqlDbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            var pageSize = 1000;
            var pageIndex = 0;
            List<Task> tasks = new();

            while (true)
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
                            var indexResponse = await NestClient.IndexDocumentAsync(item);
                            if (indexResponse.IsValid)
                            {
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

            await Task.Delay(TimeSpan.FromHours(Configuration.GetValue<int>("EsLooseSyncServiceHours")));

        }
    }


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
