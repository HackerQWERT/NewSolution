namespace JaegerWebAPI.Middleware;
public class Middleware2
{
    private readonly RequestDelegate _next;

    public Middleware2(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 执行请求前的逻辑
        Console.WriteLine("Before Request");

        // 传递请求给下一个 Middleware
        await _next(context);

        // 执行请求后的逻辑
        Console.WriteLine("After Request");
    }
}
