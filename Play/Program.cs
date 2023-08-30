using System.Data;
using Microsoft.EntityFrameworkCore;
using Play.Services;

MysqlDbContext mysqlDbContext = new();
while (true)
{
    MemoryItem memoryItem = new()
    {
        Id = 111111111111111111 + Random.Shared.Next(),
        MemoryLibId = 1,
        SrcContent = Random.Shared.Next().ToString(),
        SrcContentMD5 = Random.Shared.Next().ToString(),
        TgtContent = Random.Shared.Next().ToString(),
        TgtContentMD5 = Random.Shared.Next().ToString(),
        State = 1,
        Matching = 1,
        SrcLanguage = 1,
        TgtLanguage = 1,
        Seq = 1,
        CreateTime = DateTime.UtcNow,
        UpdateTime = DateTime.UtcNow,
        Status = true,
        EditStatus = true,
        MemoryBatchId = 1
    };
    await mysqlDbContext.MemoryItems.AddAsync(memoryItem);

    var s = await mysqlDbContext.SaveChangesAsync();

    System.Console.WriteLine(DateTime.UtcNow.ToString() + "\t" + s + "\t" + memoryItem.Id);

    await Task.Delay(5000);
}
