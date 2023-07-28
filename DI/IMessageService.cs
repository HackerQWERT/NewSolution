namespace DI;

// 自定义接口
public interface IMessageService
{
    void SendMessage(string message);
}

// 自定义实现类
public class EmailService : IMessageService
{
    public void SendMessage(string message)
    {
        Console.WriteLine("Sending email: " + message);
    }
}

public class SmsService : IMessageService
{
    public void SendMessage(string message)
    {
        Console.WriteLine("Sending SMS: " + message);
    }
}

// 需要依赖注入的类
public class NotifierService
{
    private readonly IMessageService _messageService;

    // 构造函数依赖注入
    public NotifierService(IMessageService messageService)
    {
        _messageService = messageService;
    }

    public void Notify(string message)
    {
        _messageService.SendMessage(message);
    }
}