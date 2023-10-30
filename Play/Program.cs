
MyUdpClient myUdpClient = new MyUdpClient("127.0.0.1", 1111);
myUdpClient.OnReceiveMessage += async (sender, e) =>
{
    Console.WriteLine($"接收到消息：{e.Message}");
    await myUdpClient.SendAsync("我收到了你的消息");
};

await myUdpClient.RunAsync();
