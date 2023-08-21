
using System.Text.Unicode;

namespace ElasticSearch.Services;

public class FileStyleConverter
{
    private readonly ILogger logger;


    public FileStyleConverter(ILogger logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// 启动转换文件格式
    /// </summary>
    /// <param name="sp"></param>
    /// <param name="fileInfo"></param>
    /// <returns></returns>
    public static async Task StartAsync(IServiceProvider sp, FileInfo fileInfo)
    {
        using var scope = sp.CreateAsyncScope();
        var serviceProvider = scope.ServiceProvider;
        FileStyleConverter fileStyleConverter = serviceProvider.GetRequiredService<FileStyleConverter>();
        await fileStyleConverter.DeduplicateDictsAsync(fileInfo);

        await fileStyleConverter.ConvertFileStyle(fileInfo);
    }

    /// <summary>
    /// 字典去重(先Trim再去重)
    /// </summary>
    /// <param name="fileInfo"></param>
    /// <returns></returns>
    private async Task DeduplicateDictsAsync(FileInfo fileInfo)
    {
        logger.Information($"Start DeduplicateDictsAsync: {fileInfo.FullName}");
        Stopwatch stopwatch = new();
        stopwatch.Start();

        var lines = await System.IO.File.ReadAllLinesAsync(fileInfo.FullName, Encoding.UTF8);
        string[] newLines = new string[lines.Length];
        for (int i = 0; i < lines.Length; i++)
        {
            newLines[i] = lines[i].Trim();
        }
        HashSet<string> deduplicatedStrings = new HashSet<string>(newLines);
        await File.WriteAllTextAsync(fileInfo.FullName, string.Join("\n", deduplicatedStrings), Encoding.UTF8);
        stopwatch.Stop();
        logger.Information($"Complete DeduplicateDictsAsync: {stopwatch.ElapsedMilliseconds}ms");

    }

    /// <summary>
    /// 转换文件格式(已经去重)
    /// </summary>
    /// <param name="fileInfo"></param>
    /// <returns></returns>
    private async Task ConvertFileStyle(FileInfo fileInfo)
    {
        logger.Information($"Start ConvertFileStyle: {fileInfo.FullName}");
        Stopwatch stopwatch = new();
        stopwatch.Start();
        var lines = await System.IO.File.ReadAllLinesAsync(fileInfo.FullName, Encoding.UTF8);
        List<StringBuilder> newLines = new();
        // List<StringBuilder> stringBuilderList = new();
        // stringBuilderList.Join("\n");
        foreach (var line in lines)
        {
            if (!line.Contains(" "))
                continue;
            StringBuilder stringBuilder = new(line);
            stringBuilder.Append("=>");

            StringBuilder targetStringBuilder = new StringBuilder(line);
            targetStringBuilder.Replace(" ", "_");

            stringBuilder.Append(targetStringBuilder);
            newLines.Add(stringBuilder);
        }
        await File.WriteAllTextAsync(fileInfo.FullName, string.Join("\n", newLines), Encoding.UTF8);
        stopwatch.Stop();
        logger.Information($"Complete ConvertFileStyle: {stopwatch.ElapsedMilliseconds}ms");
    }

}
