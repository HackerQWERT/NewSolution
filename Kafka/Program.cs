

// if (args.Length != 1)
// {
//     Console.WriteLine("Please provide the configuration file path as a command line argument");
//     Console.WriteLine("Please provide the configuration file path as a command line argument");
// }

// IConfiguration configuration = new ConfigurationBuilder()
//     .AddIniFile(args[0])
//     .Build();


// await Task.WhenAll(RunProducer(), RunConsumer());


// async Task RunProducer()
// {

//     const string topic = "purchases";

//     string[] users = { "eabara", "jsmith", "sgarcia", "jbernard", "htanaka", "awalther" };
//     string[] items = { "book", "alarm clock", "t-shirts", "gift card", "batteries" };

//     using (var producer = new ProducerBuilder<string, string>(
//         configuration.AsEnumerable()).Build())
//     {
//         var numProduced = 0;
//         Random rnd = new();
//         const int numMessages = 10;
//         for (int i = 0; i < numMessages; ++i)
//         {
//             var user = users[rnd.Next(users.Length)];
//             var item = items[rnd.Next(items.Length)];

//             producer.Produce(topic, new Message<string, string> { Key = user, Value = item },
//                 (deliveryReport) =>
//                 {
//                     if (deliveryReport.Error.Code != ErrorCode.NoError)
//                     {
//                         Console.WriteLine($"Failed to deliver message: {deliveryReport.Error.Reason}");
//                     }
//                     else
//                     {
//                         Console.WriteLine($"Produced event to topic {topic}: key = {user,-10} value = {item}");
//                         numProduced += 1;
//                     }
//                 });
//         }

//         producer.Flush(TimeSpan.FromSeconds(10));
//         Console.WriteLine($"{numProduced} messages were produced to topic {topic}");
//     }
// }



// async Task RunConsumer()
// {
//     configuration["group.id"] = "kafka-dotnet-getting-started";
//     configuration["auto.offset.reset"] = "earliest";
//     const string topic = "purchases";

//     CancellationTokenSource cts = new CancellationTokenSource();
//     Console.CancelKeyPress += (_, e) =>
//     {
//         e.Cancel = true; // prevent the process from terminating.
//         cts.Cancel();
//     };

//     using (var consumer = new ConsumerBuilder<string, string>(
//         configuration.AsEnumerable()).Build())
//     {
//         consumer.Subscribe(topic);
//         try
//         {
//             while (true)
//             {
//                 var cr = consumer.Consume(cts.Token);
//                 Console.WriteLine($"Consumed event from topic {topic} with key {cr.Message.Key,-10} and value {cr.Message.Value}");
//             }
//         }
//         catch (OperationCanceledException)
//         {
//             // Ctrl-C was pressed.
//         }
//         finally
//         {
//             consumer.Close();
//         }
//     }
// }