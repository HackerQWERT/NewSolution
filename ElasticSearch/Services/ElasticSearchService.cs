using Elasticsearch.Net;
using Nest;
using CustomAnalyzer = Nest.CustomAnalyzer;
using MappingCharFilter = Nest.MappingCharFilter;

namespace ElasticSearch.Services;

public class ElasticSearchService<T>
{
    private readonly ILogger logger;
    // private readonly ElasticsearchClient client;
    private readonly IElasticClient nestClient;

    public ElasticSearchService(ILogger logger)
    {
        this.logger = logger;
        // var settings = new ElasticsearchClientSettings(new Uri("https://localhost:9200"))
        //     .CertificateFingerprint("bb7754cc33e56594e4009d67086864064d25310d7d265ba2941b8fd46101288d")
        //     .DefaultIndex("Index")
        //     .Authentication(new BasicAuthentication("elastic", "eGIutE2ZGircY53s30tf"));
        // client = new ElasticsearchClient(settings);
        var settings = new ConnectionSettings(new Uri("https://localhost:9200"))
            .DefaultIndex("Index")
            .BasicAuthentication("elastic", "eGIutE2ZGircY53s30tf")
            .CertificateFingerprint("bb7754cc33e56594e4009d67086864064d25310d7d265ba2941b8fd46101288d");

        nestClient = new ElasticClient(settings);
    }

    /// <summary>
    /// 启动测试
    /// </summary>
    /// <param name="fileInfo">测试文件信息</param>
    /// <param name="indexName">测试所用索引</param>
    /// <returns></returns>
    public static async Task StartAsync(IServiceProvider sp, FileInfo fileInfo, string indexName = "my_custom_index")
    {
        using var scope = sp.CreateAsyncScope();
        var serviceProvider = scope.ServiceProvider;
        ElasticSearchService<T> elasticSearch = serviceProvider.GetRequiredService<ElasticSearchService<T>>();
        try
        {
            var lines = await elasticSearch.ReadTestDataAsync(fileInfo);
            await elasticSearch.StartTestEsTokenizePerformanceAsync(lines, indexName);
        }
        catch (Exception ex)
        {
            elasticSearch.logger.Error(ex.Message);
        }
    }

    private async Task<string[]> ReadTestDataAsync(FileInfo fileInfo)
    {
        string[] lines = await System.IO.File.ReadAllLinesAsync(fileInfo.FullName, Encoding.UTF8);
        return lines;
    }


    /// <summary>
    /// 测试数据
    /// </summary>
    /// <param name="testLines">测试数据</param>
    /// <param name="indexName">测试所用索引</param>
    /// <returns></returns>
    private async Task StartTestEsTokenizePerformanceAsync(string[] testLines, string indexName)
    {
        List<Task> tasks = new();
        Stopwatch stopwatch = new();
        stopwatch.Start();
        int failCount = 0;
        // if (!client.Indices.ExistsAsync(indexName).Result.Exists)
        //     await CreateIndexAsync(indexName);
        if (!nestClient.Indices.Exists(indexName).Exists)
            await CreateIndexAsync(indexName, "ik_smart");

        SemaphoreSlim semaphoreSlim = new(1);
        foreach (var testLine in testLines)
        {
            var tempTestLine = testLine;
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    await TestSingleLineAsync(tempTestLine, "my_custom_index", "custom_phrase_analyzer");
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    try
                    {
                        await semaphoreSlim.WaitAsync();
                        failCount++;

                    }
                    catch (Exception exception)
                    {
                        logger.Error(exception.Message);
                    }
                    finally
                    {
                        semaphoreSlim.Release();
                    }
                }
            }));
        }
        await Task.WhenAll(tasks);
        stopwatch.Stop();
        logger.Information("");
        logger.Information($"****************************************************************************************************************************************************");
        logger.Information($"Complete TestEsTokenizePerformanceAsync: {stopwatch.ElapsedMilliseconds}ms");
        logger.Information($"Total: {testLines.Length}");
        logger.Information($"Success: {testLines.Length - failCount}");
        logger.Warning($"Fail: {failCount}");

    }

    /// <summary>
    /// 创建索引
    /// </summary>
    /// <param name="indexName">创建的索引名字</param>
    /// <param name="analyzerName">创建索引时使用的默认分词器名字，要在此分词器上扩展新功能</param>
    /// <returns></returns>
    private async Task CreateIndexAsync(string indexName, string analyzerName = "ik_smart")
    {
        logger.Information("****************************************************************************************************************************************************");
        logger.Information($"Start CreateIndexAsync: {indexName}");
        var createIndexRequest = new CreateIndexRequest(indexName)
        {
            Settings = new IndexSettings
            {
                Analysis = new Nest.Analysis
                {
                    CharFilters = new Nest.CharFilters
                    {
                        { "replace_hello_world", new MappingCharFilter
                            {
                                MappingsPath = "analysis/A.txt"
                            }
                        }
                    },
                    Analyzers = new Nest.Analyzers
                    {
                        { "custom_phrase_analyzer", new CustomAnalyzer
                            {
                                Tokenizer = analyzerName,
                                CharFilter = new List<string> { "replace_hello_world" }
                            }
                        }
                    }
                }
            },
            Mappings = new TypeMapping
            {
                Properties = new Properties
                {
                    { "text", new TextProperty
                        {
                            Analyzer = "custom_phrase_analyzer"
                        }
                    }
                }
            }
        };

        await nestClient.Indices.CreateAsync(createIndexRequest);

        // await client.IndexAsync("indexName");
        logger.Information($"Complete CreateIndexAsync: {indexName}");
    }



    /// <summary>
    /// 测试单条数据
    /// </summary>
    /// <param name="testLine">单条数据</param>
    /// <param name="indexName">分析时使用的索引</param>
    /// <param name="analyzerName">分析时使用的分析器</param>
    /// <returns></returns>
    private async Task TestSingleLineAsync(string testLine, string indexName = "my_custom_index", string analyzerName = "custom_phrase_analyzer")
    {
        // IndexDocument indexDocument = new();
        // indexDocument.Id = Guid.NewGuid().ToString();
        // indexDocument.Message = testLine;
        Stopwatch stopwatch = new();
        stopwatch.Start();
        // await client.IndexAsync<IndexDocument>(indexDocument);
        // await nestClient.IndexDocumentAsync(new { text = testLine });
        // await nestClient.IndexAsync(new { text = testLine }, i => i.Index(indexName));

        var analyzeResponse = await nestClient.Indices.AnalyzeAsync(a => a
            .Index(indexName)
            .Analyzer(analyzerName)
            .Text(testLine)
        );
        stopwatch.Stop();
        logger.Information($"Complete single line test: {testLine}\tCost: {stopwatch.ElapsedTicks * 1000000000 / Stopwatch.Frequency}ns");
        foreach (var item in analyzeResponse.Tokens)
        {
            // System.Console.WriteLine($"Token: {item.Token}\tStartOffset: {item.StartOffset}\tEndOffset: {item.EndOffset}\tType: {item.Type}\n");
            logger.Information($"Token: {item.Token}\tStartOffset: {item.StartOffset}\tEndOffset: {item.EndOffset}\tType: {item.Type}\n");
        }
        // logger.Information($"Complete single line test: {indexDocument.Id}\tCost: {stopwatch.ElapsedTicks * 1000000000 / Stopwatch.Frequency}ns");

    }

    // private class IndexDocument
    // {
    //     public string Id { get; set; }
    //     public string Message { get; set; }
    // }

}
