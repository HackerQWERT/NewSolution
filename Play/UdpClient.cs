using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

public class MyUdpClient
{
    private UdpClient udpClient;
    private IPEndPoint udpClientEP;
    private IPEndPoint udpRemoteEP;
    private CancellationTokenSource receiveMessageCancellationTokenSource;
    private Task receiveMessageTask;

    public MyUdpClient(string remoteIP, int remotePort)
    {
        // udpClient = new UdpClient(hostPort);
        // udpClientEP = new IPEndPoint(IPAddress.Any, hostPort);
        receiveMessageCancellationTokenSource = new CancellationTokenSource();
        udpRemoteEP = new IPEndPoint(IPAddress.Parse(remoteIP), remotePort);
    }
    ~MyUdpClient()
    {
        Dispose();
    }

    /// <summary>
    /// 开始接收消息
    /// </summary>
    /// <returns></returns>
    public async Task RunAsync()
    {
        if (receiveMessageTask.IsCompleted == false)
            return;
        receiveMessageTask = Task.Run(async () =>
        {
            while (receiveMessageCancellationTokenSource.IsCancellationRequested == false)
            {
                var message = await ReceiveAsync();
                await OnReceiveMessageAsync(new object(), new UdpClientEventArgs { Message = message });
            }
        });
    }

    /// <summary>
    /// 停止接收消息
    /// </summary>
    /// <returns></returns>
    public async Task StopAsync()
    {
        receiveMessageCancellationTokenSource.Cancel();
        await Task.WhenAny(receiveMessageTask, Task.Delay(5000));
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="remoteEP"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task SendAsync(string message)
    {
        ReadOnlyMemory<byte> datagram = Encoding.UTF8.GetBytes(message);
        await udpClient.SendAsync(datagram, udpRemoteEP);
    }

    /// <summary>
    /// 接收消息
    /// </summary>
    /// <returns></returns>
    private async Task<string> ReceiveAsync()
    {
        var message = await udpClient.ReceiveAsync();
        return Encoding.UTF8.GetString(message.Buffer, 0, message.Buffer.Length);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        udpClient.Dispose();
    }

    /// <summary>
    /// 接收消息事件参数
    /// </summary>
    public class UdpClientEventArgs : EventArgs
    {
        public string Message { get; set; }
    }

    /// <summary>
    /// 接收消息事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    public async Task OnReceiveMessageAsync(object sender, UdpClientEventArgs e)
    {
        Task r = Task.Run(() =>
        {
            OnReceiveMessage?.Invoke(sender, e);
        });
    }

    /// <summary>
    /// 接收消息事件
    /// </summary>
    public event UdpClientDelegate OnReceiveMessage;


    /// <summary>
    /// 接收消息事件委托
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void UdpClientDelegate(object sender, UdpClientEventArgs e);



}