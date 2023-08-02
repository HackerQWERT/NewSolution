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
var serviceProvider = services.BuildServiceProvider();

// await TestASRApis.StartAsync(serviceProvider);
Stopwatch stopwatch = new();
stopwatch.Start();
await UpdateASRZsTransContent.StartAsync(serviceProvider);
stopwatch.Stop();
System.Console.WriteLine(stopwatch.ElapsedMilliseconds);
//1023ms
//5944ms