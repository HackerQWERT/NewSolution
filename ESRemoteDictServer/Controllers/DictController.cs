using EsRemoteDictServer.Servers;

namespace EsRemoteDictServer.Controllers;

[ApiController]
[Route("API/[Controller]/[Action]/")]
public class DictController : ControllerBase
{

    private readonly ILogger<DictController> logger;
    public DictController(ILogger<DictController> logger)
    {
        this.logger = logger;
    }


    [HttpGet]
    [HttpHead]
    public async Task<IActionResult> GetDictAsync()
    {
        Response.Headers.Add("Last-Modified", DateTime.UtcNow.ToString("R"));
        Response.Headers.Add("ETag", Guid.NewGuid().ToString());
        //读本地txt文件
        // System.Console.WriteLine(Value.i);
        // if (Request.Method is "GET")
        //     Value.i += 1;
        // string txt = Value.i % 2 == 0 ? "cn-99999.txt" : "test.txt";
        // logger.LogInformation(txt + ":\n");
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Dicts", "cn-99999.txt");


        // var lines = await System.IO.File.ReadAllLinesAsync(path);
        // string updateText = string.Join("\n", lines);

        var updateText = await System.IO.File.ReadAllTextAsync(path, Encoding.UTF8);

        return Ok(updateText);
    }

    [HttpPost]
    public async Task<IActionResult> UploadFileAsync(IFormFile file)
    {
        return Ok("File uploaded successfully");
    }





    //"D:\C#\NewSolution\ESRemoteDictServer\Dicts\cn-99999.txt"
    //"d:\C#\NewSolution\ESRemoteDictServer\Dicts\cn-99999.txt"
}