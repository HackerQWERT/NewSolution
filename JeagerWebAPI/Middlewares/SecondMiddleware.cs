namespace JaegerWebAPI.Middleware;
public class SecondMiddleware
{
    private readonly RequestDelegate _next;

    public SecondMiddleware(RequestDelegate next)
    {
        _next = next;
        Console.WriteLine("SecondMiddleware Ctor");

    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 执行请求前的逻辑
        Console.WriteLine("Before Request in SecondMiddleware");

        // 传递请求给下一个 Middleware
        await _next(context);

        // 执行请求后的逻辑
        Console.WriteLine("After Request in SecondMiddleware");
    }
}
