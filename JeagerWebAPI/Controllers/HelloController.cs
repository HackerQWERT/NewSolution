namespace JaegerWebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class HelloController : ControllerBase
{
    private readonly ITracer tracer;

    public HelloController(ITracer tracer)
    {
        this.tracer = tracer;
    }

    [HttpGet()]
    public IActionResult Get()
    {

        // 在这里可以使用 _tracer 来创建和记录 span
        ISpanBuilder spanBuilder = tracer.BuildSpan("HelloController.Get");
        ISpan span = spanBuilder.Start();

        // 执行你的业务逻辑

        // 记录 span 结束
        span.Finish();

        return Ok("Hello, World!");
    }
}