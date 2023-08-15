using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<MyDbContext>(options =>
            options.UseMySql(MyDbContext.ConnectionString, ServerVersion.AutoDetect(MyDbContext.ConnectionString))

        );


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
    });



// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// builder.Services.AddScoped<ITracer>(serviceProvider =>
// {
//     // 使用 serviceProvider 来获取所需的依赖项
//     var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
//     var sampler = new ConstSampler(true);

//     // 创建并返回 Jaeger.Tracer 类的实例
//     var tracer = new Tracer.Builder("my-service-name")
//         .WithLoggerFactory(loggerFactory)
//         .WithSampler(sampler)
//         .Build();

//     return tracer;
// });

builder.Services.AddScoped<ITracer, TracerService>();

var app = builder.Build();



app.UseMiddleware<SimpleMiddleware>();
app.UseMiddleware<Middleware2>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
