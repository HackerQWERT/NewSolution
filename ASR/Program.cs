// 注册服务

var services = new ServiceCollection();

Serilog.Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(@"D:\C#\NewSolution\ASR\Logs\log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

services.AddSingleton<ILogger>(Serilog.Log.Logger);

services.AddDbContext<MyDbContext>(options =>
            options.UseMySql(MyDbContext.ConnectionString, ServerVersion.AutoDetect(MyDbContext.ConnectionString))
        );

// 创建服务提供程序
var serviceProvider = services.BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILogger>();


var myDbContext = serviceProvider.GetRequiredService<MyDbContext>();



string directoryPath = "D:/ASR";
//获得一级路径 ：韩语
var languageDirectories = Directory.GetDirectories(directoryPath).ToList();

List<Task> taskLists = new();
bool isJoined = false;

foreach (string languageDirectory in languageDirectories)
{
    try
    {
        // 创建 DirectoryInfo 对象
        DirectoryInfo languageDirectoryInfo = new(languageDirectory);

        // 判断文件夹是否为压缩文件夹，如果是则跳过
        if (languageDirectoryInfo.Attributes.HasFlag(FileAttributes.Compressed))
        {
            continue;
        }

        // 获取文件夹信息
        string languageDirectoryInfoName = languageDirectoryInfo.Name;
        // 在这里可以对文件夹信息进行处理，例如打印、存储等
        logger.Information($"文件夹名：{languageDirectoryInfoName}");
        logger.Information("---------------------------------");

        //获得二级路径：04
        var serialNumberDirectoriesPath = languageDirectoryInfo.FullName;
        var serialNumberDirectories = Directory.GetDirectories(serialNumberDirectoriesPath).ToList();


        foreach (var serialNumberDirectory in serialNumberDirectories)
        {
            // 创建 DirectoryInfo 对象
            DirectoryInfo serialNumberDirectoryInfo = new(serialNumberDirectory);
            // 判断文件夹是否为压缩文件夹，如果是则跳过
            if (serialNumberDirectoryInfo.Attributes.HasFlag(FileAttributes.Compressed))
            {
                continue;
            }

            // 获取文件夹信息
            string serialNumberDirectoryInfoName = serialNumberDirectoryInfo.Name;

            // 在这里可以对文件夹信息进行处理，例如打印、存储等
            logger.Information($"文件夹名：{serialNumberDirectoryInfoName}");
            logger.Information("---------------------------------");

            //获得三级路径：160428944.wav
            var musicFilePath = serialNumberDirectoryInfo.FullName;
            var musicFiles = Directory.GetFiles(musicFilePath).ToList();
            foreach (var musicFile in musicFiles)
            {
                DirectoryInfo musicFileInfo = new(musicFile);

                if (!isJoined)
                {
                    if (musicFileInfo.FullName == @"D:\ASR\汉语普通话\19\1555637128.wav")
                    {
                        isJoined = true;
                        await TestASRApisAsync(musicFileInfo);
                        // taskLists.Add(Task.Run(async () =>
                        // {
                        // }));
                    }
                    else
                    {
                        continue;

                    }
                }
                else
                {

                    await TestASRApisAsync(musicFileInfo);

                }


            }
        }

        await Task.WhenAll(taskLists);
    }
    catch (Exception ex)
    {
        // 处理异常
        Console.WriteLine($"无法获取文件夹信息：{ex.Message}");
    }
}

async Task TestASRApisAsync(DirectoryInfo musicFileInfo)
{

    // 设置请求的 URL
    // string url = "http://192.168.0.118:30839/swagger#/Endpoints/asr_asr_post";
    string url = "http://192.168.0.118:30839/asr";

    // string url = "http://192.168.1.172:9812/swagger";
    // string url = "http://192.168.1.172:9812/asr";

    try
    {

        string audioFilePath = musicFileInfo.FullName;

        Dictionary<string, string> languageMap = new()
        {
            { "法语", "fr" },
            { "韩语", "ko" },
            { "汉语普通话", "zs" },
            { "美式英语", "en" },
            { "日语", "ja" },
            { "英式英语", "en" },
         };

        // 构建请求参数
        PostParameters postParameters = new()
        {
            ModelType = "base",
            Task = "transcribe",
            Language = languageMap[musicFileInfo!.Parent!.Parent!.Name],
            OutPut = "json",
            InitialPrompt = "",
            Encode = true,
            WordTimestamps = false,
        };
        var serializedData = JsonSerializer.Serialize(postParameters);

        var stringContent = new StringContent(serializedData, System.Text.Encoding.UTF8, "application/json");


        using var multipartFormDataContent = new MultipartFormDataContent();
        using var audioStream = File.Open(audioFilePath, FileMode.Open);

        var streamContent = new StreamContent(audioStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");

        multipartFormDataContent.Add(stringContent, "parameters");
        multipartFormDataContent.Add(streamContent, "audio_file", Path.GetFileName(audioFilePath));
        multipartFormDataContent.Headers.ContentType.MediaType = "multipart/form-data";

        using var httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


        Stopwatch stopwatch = new();
        stopwatch.Start();
        DateTime startTime = DateTime.Now;

        ASRTest aSRTest = new()
        {
            Language = postParameters.Language,
            Path = audioFilePath,
            StartTime = startTime,
        };

        logger.Information("开始请求ASR接口");
        var response = await httpClient.PostAsync(url, multipartFormDataContent);



        stopwatch.Stop();
        DateTime endTime = DateTime.Now;

        logger.Information("结束请求ASR接口");

        long elapsedNanoseconds = stopwatch.ElapsedTicks * (1000000000L / Stopwatch.Frequency);

        // 写入 language path start_time  end_time   cost_time  cost_nanoseconds 
        aSRTest.EndTime = endTime;
        aSRTest.CostTime = aSRTest.EndTime - aSRTest.StartTime;
        aSRTest.CostNanoTime = elapsedNanoseconds.ToString();
        aSRTest.Status = response.StatusCode.ToString();

        logger.Information($"请求ASR接口耗时：  {elapsedNanoseconds} ns");

        var responseBody = await response.Content.ReadAsStringAsync();
        aSRTest.Content = responseBody;

        logger.Information(responseBody);

        await myDbContext.ASRTest.AddAsync(aSRTest);
        await myDbContext.SaveChangesAsync();

    }


    catch (Exception ex)
    {

        logger.Error(ex.Message);
    }


}

