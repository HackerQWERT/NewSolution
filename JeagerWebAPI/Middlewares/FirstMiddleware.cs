namespace JaegerWebAPI.Middleware;
public class FirstMiddleware
{
    private readonly RequestDelegate _next;

    public FirstMiddleware(RequestDelegate next)
    {
        _next = next;
        Console.WriteLine("FirstMiddleware Ctor");

    }

    ~FirstMiddleware()
    {
        Console.WriteLine("FirstMiddleware Dtor");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 执行请求前的逻辑
        Console.WriteLine("Before Request in FirstMiddleware");

        // 传递请求给下一个 Middleware
        await _next(context);

        // 执行请求后的逻辑
        Console.WriteLine("After Request in FirstMiddleware");
    }
}
