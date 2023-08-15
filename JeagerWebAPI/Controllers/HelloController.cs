namespace JaegerWebAPI.Controllers;

[ValidateModel]
[ApiController]
[Route("API/[Controller]/[Action]/")]
public class HelloController : ControllerBase
{
    private readonly ITracer tracer;

    public HelloController(ITracer tracer)
    {
        this.tracer = tracer;
    }


    [HttpGet(Name = "Fuck")]
    public async Task<IActionResult> Get(int name)
    {

        string message = $"Hello, {name}!";
        // 在这里可以使用 _tracer 来创建和记录 span
        ISpanBuilder spanBuilder = tracer.BuildSpan("HelloController.Get");
        ISpan span = spanBuilder.Start();

        // 执行你的业务逻辑

        // 记录 span 结束
        span.Finish();

        return Ok("Hello, World!" + message);
    }

    [HttpPost]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        // 在这里可以使用 _tracer 来创建和记录 span
        ISpanBuilder spanBuilder = tracer.BuildSpan("HelloController.UploadFile");
        ISpan span = spanBuilder.Start();

        // 执行你的业务逻辑
        using (var stream = file.OpenReadStream())
        {
            // Process the file stream here
        }


        // 记录 span 结束
        span.Finish();

        return Ok("File uploaded successfully");
    }

}