var services = new ServiceCollection();

string path;
DirectoryInfo directoryInfo = new(Directory.GetCurrentDirectory());

if (directoryInfo.Name == "ElasticSearch")

    path = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "log.txt");
else
    path = Path.Combine(Directory.GetCurrentDirectory(), "../../../Logs", "log.txt");
System.Console.WriteLine(path);

Serilog.Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(path, rollingInterval: RollingInterval.Day)
    .CreateLogger();

services.AddSingleton<ILogger>(Serilog.Log.Logger);

// 创建服务提供程序
var serviceProvider = services.BuildServiceProvider(true);

var scope = serviceProvider.CreateAsyncScope();

var logger = scope.ServiceProvider.GetRequiredService<ILogger>();

var settings = new ConnectionSettings(new Uri("https://localhost:9200"))
    .BasicAuthentication("elastic", "eGIutE2ZGircY53s30tf")
    .DefaultIndex("index"); // Replace with your credentials

var client = new ElasticClient(settings);


CancellationToken cancellationToken = new();

while (!cancellationToken.IsCancellationRequested)
{

    var searchResponse = client.Search<object>(s => s
        .Index("index")
        .Query(q => q
            .Match(m => m
                .Field("content")
                .Query("？")
            )
        )
        .Highlight(h => h
            .PreTags("<tag1>", "<tag2>")
            .PostTags("</tag1>", "</tag2>")
            .Fields(f => f
                .Field("content")
            )
        )
    // .Size(10) // Number of documents to retrieve
    );
    //Natürlich, hier ist ein kurzer Satz auf Deutsch:
    //The enigmatic artifact perplexed the intrepid archaeologist
    if (searchResponse.IsValid)
    {
        foreach (var hit in searchResponse.Hits)
        {
            //Process the search results here
            // hit.Source;
            var source = hit.Source as Dictionary<string, object>;

            logger.Information($"Document ID: {hit.Id}");
            logger.Information($"Source: {source["content"]}");
            logger.Information($"Highlight: {string.Join(", ", hit.Highlight.SelectMany(kv => kv.Value))}\n");
        }
    }
    else
    {
        logger.Error($"Search failed: {searchResponse.DebugInformation}");
    }

    await Task.Delay(1000);
}
