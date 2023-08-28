using ElasticSearch.Models;
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
        // var settings = new ConnectionSettings(new Uri("https://localhost:9200"))
        //     .DefaultIndex("Index")
        //     .BasicAuthentication("elastic", "J_jUO3nGCy4afiLNJjyX")
        //     .CertificateFingerprint("6f9c780fb45d1888525aa29677e5cbbe71d7302538fff6063cc40a13c55bd8c4");

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


    /// <summary>
    /// 读取测试数据
    /// </summary>
    /// <param name="fileInfo"></param>
    /// <returns></returns>
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
                    await TestSingleLineAsync(tempTestLine, indexName, "custom_phrase_analyzer");
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
        System.Console.WriteLine($"****************************************************************************************************************************************************");
        System.Console.WriteLine($"Complete TestEsTokenizePerformanceAsync: {stopwatch.ElapsedMilliseconds}ms");
        System.Console.WriteLine($"Total: {testLines.Length}");
        System.Console.WriteLine($"Success: {testLines.Length - failCount}");
        System.Console.WriteLine($"Fail: {failCount}");
        // logger.Information("");
        // logger.Information($"****************************************************************************************************************************************************");
        // logger.Information($"Complete TestEsTokenizePerformanceAsync: {stopwatch.ElapsedMilliseconds}ms");
        // logger.Information($"Total: {testLines.Length}");
        // logger.Information($"Success: {testLines.Length - failCount}");
        // logger.Warning($"Fail: {failCount}");

    }



    /// <summary>
    /// 创建索引
    /// </summary>
    /// <param name="indexName">创建的索引名字</param>
    /// <param name="analyzerName">创建索引时使用的默认分词器名字，要在此分词器上扩展新功能</param>
    /// <returns></returns>
    private async Task CreateIndexAsync(string indexName, string analyzerName = "ik_smart")
    {
        // logger.Information("****************************************************************************************************************************************************");
        // logger.Information($"Start CreateIndexAsync: {indexName}");
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

        var createResponse = await nestClient.Indices.CreateAsync(createIndexRequest);

        // if (createResponse.IsValid)
        //     // await client.IndexAsync("indexName");
        //     logger.Information($"Success CreateIndexAsync: {indexName}");
        // else
        //     logger.Error($"Fail CreateIndexAsync: {indexName}");
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


        // await nestClient.IndexAsync(new { text = testLine }, i => i
        //     .Index(indexName)
        // );

        var analyzeResponse = await nestClient.Indices.AnalyzeAsync(a => a
            .Index(indexName)
            .Analyzer(analyzerName)
            .Text(testLine)
        );
        stopwatch.Stop();
        // logger.Information($"Complete single line test: {testLine}\tCost: {stopwatch.ElapsedTicks * 1000000000 / Stopwatch.Frequency}ns");

        // foreach (var item in analyzeResponse.Tokens)
        // {
        //     // System.Console.WriteLine($"Token: {item.Token}\tStartOffset: {item.StartOffset}\tEndOffset: {item.EndOffset}\tType: {item.Type}\n");
        //     logger.Information($"Token: {item.Token}\tStartOffset: {item.StartOffset}\tEndOffset: {item.EndOffset}\tType: {item.Type}\n");
        // }

        // logger.Information($"Complete single line test: {indexDocument.Id}\tCost: {stopwatch.ElapsedTicks * 1000000000 / Stopwatch.Frequency}ns");

    }



    // private class IndexDocument
    // {
    //     public string Id { get; set; }
    //     public string Message { get; set; }
    // }


    //TODO 一个用户两个索引，一个分词索引返回token，一个数据索引存入token对应的翻译
    /// <summary>
    /// 创建用户分词索引
    /// </summary>
    /// <param name="userAnalyzerIndexName">用户分词索引名</param>
    /// <param name="tokenizer">分词器</param>
    /// <param name="mappingsPath">短语格式文件映射路径</param>
    /// <returns></returns>
    private async Task CreateAnalyzerIndexAsync(string userAnalyzerIndexName, string mappingsPath, string tokenizer = "ik_smart", string analyzerName = "custom_phrase_analyzer")
    {
        var createIndexRequest = new CreateIndexRequest(userAnalyzerIndexName)
        {
            Settings = new IndexSettings
            {
                Analysis = new Nest.Analysis
                {
                    CharFilters = new Nest.CharFilters
                    {
                        { "replaceSpaceTo_", new MappingCharFilter
                            {
                                MappingsPath = mappingsPath
                            }
                        }
                    },
                    Analyzers = new Nest.Analyzers
                    {
                        { analyzerName, new CustomAnalyzer
                            {
                                Tokenizer = tokenizer,
                                CharFilter = new List<string> { "replaceSpaceTo_" }
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
                            Analyzer = analyzerName
                        }
                    }
                }
            }
        };
        var createIndexResponse = await nestClient.Indices.CreateAsync(createIndexRequest);


        if (createIndexResponse.IsValid)
            // await client.IndexAsync("indexName");
            logger.Information($"Success CreateIndexAsync: {userAnalyzerIndexName}");
        else
            logger.Error($"Fail CreateIndexAsync: {userAnalyzerIndexName}");
    }



    /// <summary>
    /// 删除用户分词索引
    /// </summary>
    /// <param name="userAnalyzerIndexName">用户分词索引名</param>
    /// <returns></returns>
    private async Task DeleteAnalyzerIndexAsync(string userAnalyzerIndexName)
    {

        var deleteIndexResponse = await nestClient.Indices.DeleteAsync(userAnalyzerIndexName);
        if (deleteIndexResponse.IsValid)
            // await client.IndexAsync("indexName");
            logger.Information($"Success CreateIndexAsync: {userAnalyzerIndexName}");
        else
            logger.Error($"Fail CreateIndexAsync: {userAnalyzerIndexName}");

    }



    /// <summary>
    /// 获取分词结果
    /// </summary>
    /// <param name="userAnalyzerIndexName"></param>
    /// <param name="texts"></param>
    /// <param name="analyzerName"></param>
    /// <returns></returns>
    private async Task<IReadOnlyCollection<AnalyzeToken>> GetAnalyzeTokensAsync(string userAnalyzerIndexName, string texts, string analyzerName = "custom_phrase_analyzer")
    {
        var analyzeResponse = await nestClient.Indices.AnalyzeAsync(a => a
            .Index(userAnalyzerIndexName)
            .Analyzer(analyzerName)
            .Text(texts)
        );
        // foreach (var item in analyzeResponse.Tokens)
        // {
        //     // System.Console.WriteLine($"Token: {item.Token}\tStartOffset: {item.StartOffset}\tEndOffset: {item.EndOffset}\tType: {item.Type}\n");
        //     logger.Information($"Token: {item.Token}\tStartOffset: {item.StartOffset}\tEndOffset: {item.EndOffset}\tType: {item.Type}\n");
        // }
        return analyzeResponse.Tokens;
    }



    /// <summary>
    /// 创建用户数据索引，默认standard分析器
    /// </summary>
    /// <param name="userDataIndexName">用户数据索引名</param>
    /// <returns></returns>
    private async Task CreateUserDataIndexAsync(string userDataIndexName)
    {
        var createIndexResponse = await nestClient.Indices.CreateAsync(userDataIndexName, c => c
            .Map(m => m.AutoMap())
        );
        if (createIndexResponse.IsValid)
            // await client.IndexAsync("indexName");
            logger.Information($"Success CreateIndexAsync: {userDataIndexName}");
        else
            logger.Error($"Fail CreateIndexAsync: {userDataIndexName}");
    }



    /// <summary>
    /// 删除用户数据索引
    /// </summary>
    /// <param name="userDataIndexName"></param>
    /// <returns></returns>
    private async Task DeleteUserDataIndexAsync(string userDataIndexName)
    {

        var deleteIndexResponse = await nestClient.Indices.DeleteAsync(userDataIndexName);
        if (deleteIndexResponse.IsValid)
            // await client.IndexAsync("indexName");
            logger.Information($"Success CreateIndexAsync: {userDataIndexName}");
        else
            logger.Error($"Fail CreateIndexAsync: {userDataIndexName}");
    }



    /// <summary>
    /// 插入用户数据索引
    /// </summary>
    /// <param name="userDocument"></param>
    /// <param name="userIndexName"></param>
    /// <returns></returns>
    private async Task InsertUserDataIndexAsync(UserDocument userDocument, string userIndexName)
    {
        var indexResponse = await nestClient.IndexAsync(userDocument, i => i
            .Index(userIndexName)
        );
        if (indexResponse.IsValid)
            // await client.IndexAsync("indexName");
            logger.Information($"Success CreateIndexAsync: {userIndexName}");
        else
            logger.Error($"Fail CreateIndexAsync: {userIndexName}");
    }



    /// <summary>
    /// 更新用户数据索引
    /// </summary>
    /// <param name="userDocument"></param>
    /// <param name="userIndexName"></param>
    /// <returns></returns>
    private async Task UpdateUserDataIndexAsync(UserDocument userDocument, string userIndexName)
    {
        var updateResponse = await nestClient.UpdateAsync<UserDocument>(userDocument.Term, u => u
            .Index(userIndexName)
            .Doc(userDocument)
        );
        if (updateResponse.IsValid)
            // await client.IndexAsync("indexName");
            logger.Information($"Success CreateIndexAsync: {userIndexName}");
        else
            logger.Error($"Fail CreateIndexAsync: {userIndexName}");
    }



    /// <summary>
    /// 搜索用户数据索引
    /// </summary>
    /// <param name="userIndexName">用户索引名</param>
    /// <param name="termId">术语ID</param>
    /// <returns></returns>
    private async Task<IReadOnlyCollection<UserDocument>> SearchUserDataIndexAsync(string userIndexName, string termId)
    {
        var searchResponse = await nestClient.SearchAsync<UserDocument>(s => s
            .Index(userIndexName)
            .Query(q => q
                .Match(m => m
                    .Field(f => f.TermId)
                    .Query(termId)
                )
            )
        );
        if (searchResponse.IsValid)
        {
            // logger.Information($"Success CreateIndexAsync: {userIndexName}");

            return searchResponse.Documents;
        }
        // await client.IndexAsync("indexName");
        else
        {
            // logger.Error($"Fail CreateIndexAsync: {userIndexName}");

            return null;

        }
    }



    /// <summary>
    /// 更新英文映射文件
    /// </summary>
    /// <param name="updateText">新映射</param>
    /// <param name="filePath">映射路径</param>
    /// <returns></returns>
    private async Task UpdateEnglishMappingFileAsync(string[] updateText, string filePath)
    {
        await System.IO.File.WriteAllLinesAsync(filePath, updateText, Encoding.UTF8);
    }


}



