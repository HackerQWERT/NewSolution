using System.IO.Compression;
using Microsoft.EntityFrameworkCore.Query;

namespace ASR.Services;

public class UpdateASRZsTransContent
{

    public static async Task StartAsync(ServiceProvider serviceProvider)
    {
        await UpdateASRZsTransContentAsync(serviceProvider);
    }
    public static async Task UpdateASRZsTransContentAsync(ServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger>();
        var myDbContext = serviceProvider.GetRequiredService<MyDbContext>();

        // var transContent = myDbContext.ASRZsTest.Where(x => x.Language == "zs").Select(x => x.TransContent.ToString());
        // await transContent.ExecuteUpdateAsync(setters => setters.SetProperty(x => x, x => ConvertJsonToString(x)));

        //读出所有的asr_zs_test表中的数据
        var ASRZsTest = myDbContext.ASRZsTest.Where(x => x.Language == "zs");

        await ASRZsTest.ForEachAsync(x =>
        {
            x.TransContent = ConvertJsonToString(x.TransContent);
        });

        await myDbContext.SaveChangesAsync();
        // var ASRZsTest = myDbContext.ASRZsTest.Where(x => x.Language == "zs").Select(x => new { x.Id, x.TransContent });

        // foreach (var item in await ASRZsTest.ToListAsync())
        // {
        //     var newValue = ConvertJsonToString(item.TransContent);
        //     await myDbContext.ASRZsTest.Where(x => x.Id == item.Id).ExecuteUpdateAsync(setters => setters.SetProperty(x => x.TransContent, newValue));
        // }


    }


    static string ConvertJsonToString(string json)
    {
        var transContentJson = JsonSerializer.Deserialize<TransContentJson>(json);
        StringBuilder stringBuilder = new();
        transContentJson?.Sentences.ForEach(x =>
        {
            stringBuilder.Append(x.Text);
        });
        var x1 = stringBuilder.ToString();
        Console.WriteLine(x1);
        return x1;
    }


}
