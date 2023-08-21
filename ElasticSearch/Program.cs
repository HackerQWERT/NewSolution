var services = new ServiceCollection();

string path, englishFilePath, englishTestPath, chineseTestPath;

DirectoryInfo directoryInfo = new(Directory.GetCurrentDirectory());

if (directoryInfo.Name == "ElasticSearch")
{
    path = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "log.txt");
    englishFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Dicts", "en-99999.txt");
    englishTestPath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "Book17_en.txt");
    chineseTestPath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "中文30w字.txt");

}
else
{
    path = Path.Combine(Directory.GetCurrentDirectory(), "../../../Logs", "log.txt");
    englishFilePath = Path.Combine(Directory.GetCurrentDirectory(), "../../../Dicts", "en-99999.txt");
    englishTestPath = Path.Combine(Directory.GetCurrentDirectory(), "../../../TestData", "Book17_en.txt");
    chineseTestPath = Path.Combine(Directory.GetCurrentDirectory(), "../../../TestData", "中文30w字.txt");
}


System.Console.WriteLine(path);

Serilog.Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(path, rollingInterval: RollingInterval.Day)
    .CreateLogger();

services.AddSingleton<ILogger>(Serilog.Log.Logger);
services.AddScoped<FileStyleConverter>();
services.AddScoped<ElasticSearchService<string>>();
// 创建服务提供程序
var serviceProvider = services.BuildServiceProvider(true);

List<Task> tasks = new(){

    // FileStyleConverter.StartAsync(serviceProvider, new FileInfo(englishFilePath)),
    // ElasticSearchService<string>.StartAsync(serviceProvider, new FileInfo(englishTestPath),"my_custom_index"),
    ElasticSearchService<string>.StartAsync(serviceProvider, new FileInfo(chineseTestPath),"my_custom_index")
};


await Task.WhenAll(tasks);

