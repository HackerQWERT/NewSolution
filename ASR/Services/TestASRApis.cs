namespace ASR.Services;

public class TestASRApis
{

    public TestASRApis()
    {


    }

    public static async Task StartAsync(ServiceProvider serviceProvider)
    {
        await TestASRApisAsync(serviceProvider);
    }

    private static async Task TestASRApisAsync(ServiceProvider serviceProvider)
    {

        var logger = serviceProvider.GetRequiredService<ILogger>();


        var myDbContext = serviceProvider.GetRequiredService<MyDbContext>();

        string directoryPath = "D:/ASR";
        //获得一级路径 ：韩语
        var languageDirectories = Directory.GetDirectories(directoryPath).ToList();

        List<Task> taskLists = new();
        //为true则不进行顶点搜索
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

                if (languageDirectoryInfoName != "汉语普通话")
                {
                    continue;
                }
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
                        FileInfo musicFileInfo = new(musicFile);
                        // DirectoryInfo musicFileInfo = new(musicFile);
                        if (!isJoined)
                        {
                            if (musicFileInfo.FullName == @"D:\ASR\汉语普通话\15\1555326153.wav")
                            {
                                isJoined = true;
                                //测试三种模型
                                List<string> modelTypes = new() { "base", "small", "medium" };
                                modelTypes.ForEach(async (modelType) =>
                                {
                                    try
                                    {
                                        await TestASRApisAsync(musicFileInfo, modelType);
                                    }
                                    catch (Exception ex)
                                    {
                                        // 处理异常
                                        logger.Error(ex.Message);
                                    }
                                });
                            }
                            // taskLists.Add(Task.Run(async () =>
                            // {
                            // }));
                            else
                            {
                                continue;

                            }
                        }
                        else
                        {
                            try
                            {
                                await TestASRApisAsync(musicFileInfo);
                            }
                            catch (Exception ex)
                            {
                                // 处理异常
                                logger.Error(ex.Message);
                            }
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

        async Task TestASRApisAsync(FileInfo musicFileInfo, string task = "transcribe", string outPut = "json", string initialPrompt = "", bool encode = true, bool wordTimestamps = false)
        {

            //测试三种模型
            List<string> modelTypes = new() { "base", "small", "medium" };
            ASRZsTest aSRZsTest = new();

            foreach (var modelType in modelTypes)
            {
                // 设置请求的 URL
                // string url = "http://192.168.0.118:30839/swagger#/Endpoints/asr_asr_post";
                string url = "http://192.168.0.118:30839/asr";

                // string url = "http://192.168.1.172:9812/swagger";
                // string url = "http://192.168.1.172:9812/asr";


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
                    ModelType = modelType,
                    Task = task,
                    Language = languageMap[musicFileInfo!.Directory!
                    .Parent!.Name],
                    OutPut = outPut,
                    InitialPrompt = initialPrompt,
                    Encode = encode,
                    WordTimestamps = wordTimestamps,
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

                aSRZsTest.Language = postParameters.Language;
                aSRZsTest.Path = audioFilePath;

                Stopwatch stopwatch = new();
                stopwatch.Start();
                DateTime startTime = DateTime.Now;
                logger.Information($"开始请求ASR接口，使用模型：{postParameters.ModelType}，语言：{postParameters.Language}，音频路径：{audioFilePath}");
                var response = await httpClient.PostAsync(url, multipartFormDataContent);

                stopwatch.Stop();
                DateTime endTime = DateTime.Now;
                aSRZsTest.StartTime = startTime;
                logger.Information("结束请求ASR接口");

                long elapsedNanoseconds = stopwatch.ElapsedTicks * (1000000000L / Stopwatch.Frequency);


                var responseBody = await response.Content.ReadAsStringAsync();

                switch (modelType)
                {
                    case "base":
                        aSRZsTest.BaseCostNanoseconds = elapsedNanoseconds;
                        aSRZsTest.BaseContent = responseBody;
                        break;
                    case "small":
                        aSRZsTest.SmallCostNanoseconds = elapsedNanoseconds;
                        aSRZsTest.SmallContent = responseBody;
                        break;
                    case "medium":
                        aSRZsTest.MediumCostNanoseconds = elapsedNanoseconds;
                        aSRZsTest.MediumContent = responseBody;
                        break;
                    default:
                        logger.Error($"模型类型错误：{modelType}");
                        break;
                }

                aSRZsTest.Status = response.StatusCode.ToString();
                logger.Information($"请求ASR接口耗时：  {elapsedNanoseconds} ns");
                logger.Information(responseBody);

            }
            // 写入 language path start_time    cost_time  cost_nanoseconds 
            await myDbContext.ASRZsTest.AddAsync(aSRZsTest);
            await myDbContext.SaveChangesAsync();
        }



    }




}