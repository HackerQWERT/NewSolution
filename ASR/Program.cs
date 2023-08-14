// 注册服务
var services = new ServiceCollection();

Serilog.Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(@"D:\C#\NewSolution\ASR\Logs\log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

services.AddSingleton<ILogger>(Serilog.Log.Logger);

services.AddDbContext<MyDbContext>(options =>
            options.UseMySql(MyDbContext.ConnectionString, ServerVersion.AutoDetect(MyDbContext.ConnectionString))
        );
// 创建服务提供程序
var serviceProvider = services.BuildServiceProvider(true);


await TestASRApis.StartAsync(serviceProvider);

await UpdateASRZsTransContent.StartAsync(serviceProvider);
