
CancellationToken cancellationToken = new();
var factory = new ConnectionFactory()
{
    HostName = "localhost", // RabbitMQ服务器的主机名
    Port = 5672, // 默认端口号
    UserName = "root", // 默认用户名
    Password = "1234" // 默认密码
};

var producerTask = Task.Run(() => RunProducerAsync(), cancellationToken);
var consumerTask = Task.Run(() => RunConsumerAsync(), cancellationToken);

await Task.WhenAll(producerTask, consumerTask);




async Task RunProducerAsync()
{
    using var connection = factory.CreateConnection();

    while (!cancellationToken.IsCancellationRequested)
    {
        await Task.Delay(1000, cancellationToken);

        using var channel = connection.CreateModel();
        // 声明一个队列
        channel.QueueDeclare(queue: "myQueue",
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        // 在此处执行发送消息和接收消息的操作
        // 要发送的消息
        string message = "Hello RabbitMQ!";

        var body = Encoding.UTF8.GetBytes(message);

        // 将消息发布到队列
        channel.BasicPublish(exchange: "",
                             routingKey: "myQueue",
                             basicProperties: null,
                             body: body);

    }
}

async Task RunConsumerAsync()
{

    await Task.Delay(1000, cancellationToken);
    using var connection = factory!.CreateConnection();
    using var channel = connection.CreateModel();
    // channel.QueueDeclare(queue: "myQueue",
    //                      durable: false,
    //                      exclusive: false,
    //                      autoDelete: false,
    //                      arguments: null);
    // 创建一个消费者
    var consumer = new EventingBasicConsumer(channel);

    // 注册接收消息的事件处理程序
    consumer.Received += (model, ea) =>
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        Console.WriteLine("收到消息：{0}", message);
    };

    // 启动消费者，等待接收消息
    channel.BasicConsume(queue: "myQueue",
                         autoAck: true,
                         consumer: consumer);
    await Task.Delay(-1, cancellationToken);
}