// IHostBuilder hostBuilder = StandardServiceHost.CreateDefaultBuilder(args, "EsSyncService")
//                                                 .AddStandardACCConfiguration("Default", optional: true)
//                                                 // .AddConfigurationDirectory("config") //非k8s时加载 config 目录下的配置
//                                                 .UseShutdownTimeout(TimeSpan.FromMinutes(5)); ;


// hostBuilder.ConfigureWebHostDefaults(webBuilder =>
//         {
//             webBuilder.ConfigureServices(services =>
//             {
//                 services.AddControllers();
//                 // Add services to the container.
//                 // Learn more about configuring Swagger / OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//                 services.AddEndpointsApiExplorer();
//                 services.AddSwaggerGen();

//                 services.AddDbContext<MysqlDbContext>();
//                 services.AddHostedService<EsTightlySyncService>();
//                 services.AddHostedService<EsLooseSyncService>();
//             });
//             webBuilder.Configure(app =>
//             {

//                 var environment = app.ApplicationServices.GetRequiredService<IHostEnvironment>();
//                 if (environment.IsDevelopment())
//                 {
//                     app.UseSwagger();
//                     app.UseSwaggerUI();
//                 }
//                 app.UseHttpsRedirection();

//             });
//         });


// hostBuilder.Build().RunWithExceptionLoged();






var builder = WebApplication.CreateBuilder();

builder.WebHost.UseShutdownTimeout(TimeSpan.FromMinutes(5));

// builder.Host.ConfigureDefaults(args).WiredAsStandardService("EsSyncService")
//                .AddStandardACCConfiguration("Default", optional: true);



builder.Services.AddControllers();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

Serilog.Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Services.AddSingleton<ILogger>(Serilog.Log.Logger);
builder.Services.AddDbContext<MysqlDbContext>();
// builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddHostedService<EsTightlySyncService>();
// builder.Services.AddHostedService<EsLooseSyncService>();





var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();




app.Run();

