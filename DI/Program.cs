using System;
using DI;
using Microsoft.Extensions.DependencyInjection;

// 设置依赖注入容器
var serviceProvider = new ServiceCollection()
    .AddScoped<IMessageService, EmailService>() // 注册 IMessageService 接口和 EmailService 实现类
                                                // .AddScoped<IMessageService, SmsService>()   // 如果想用 SmsService 替换 EmailService，取消注释此行
    .AddScoped<NotifierService>() // 注册 NotifierService 类
    .BuildServiceProvider();

// 获取 NotifierService 实例

var notifierService = serviceProvider.GetRequiredService<NotifierService>();
// 使用 NotifierService 实例发送消息
notifierService.Notify("Hello, Dependency Injection!");




