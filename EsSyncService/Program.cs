using Microsoft.EntityFrameworkCore;


using Microsoft.Extensions.Configuration;


var builder = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

IConfigurationRoot configuration = builder.Build();


IServiceCollection servicesCollection = new ServiceCollection();

servicesCollection.AddHostedService<EsSyncService.Services.EsSyncService>();
servicesCollection.AddScoped<EsSyncService.Services.EsSyncService>();
servicesCollection.AddDbContext<MysqlDbContext>();
servicesCollection.AddSingleton<IConfiguration>(configuration);

var serviceProvider = servicesCollection.BuildServiceProvider(true);



await EsSyncService.Services.EsSyncService.StartAsync(serviceProvider);

await Task.Delay(-1);











