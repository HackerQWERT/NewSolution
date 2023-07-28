using Confluent.Kafka;
using System;

class Program
{
    static void Main(string[] args)
    {
        var config = new ProducerConfig { BootstrapServers = "localhost:9092" };

        using (var producer = new ProducerBuilder<Null, string>(config).Build())
        {
            string topic = "your_topic_name";
            string message = "Hello, Kafka!";
            var deliveryReport = producer.ProduceAsync(topic, new Message<Null, string> { Value = message });
            deliveryReport.ContinueWith(task =>
            {
                Console.WriteLine($"Message delivered: {task.Result.TopicPartitionOffset}");
            });
        }
    }
}
