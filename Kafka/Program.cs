using Confluent.Kafka;

string brokerList = "localhost:9092";
string topicName = "my_topic";

var config = new ProducerConfig
{
    BootstrapServers = brokerList,
    SecurityProtocol = SecurityProtocol.Ssl,
    ApiVersionRequest = true,
};

using var producer = new ProducerBuilder<Null, string>(config).Build();

// Produce a message (change 'value' to your message content)
var value = "Hello, Kafka!";
var message = new Message<Null, string> { Value = value };

producer.Produce(topicName, message, (deliveryReport) =>
{
    if (deliveryReport.Error.Code != ErrorCode.NoError)
    {
        Console.WriteLine($"Failed to deliver message: {deliveryReport.Error.Reason}");
    }
});

// Wait for message delivery to complete.
producer.Flush(TimeSpan.FromSeconds(10));
