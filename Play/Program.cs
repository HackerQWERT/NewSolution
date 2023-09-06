
// using System.Data;
// using Microsoft.EntityFrameworkCore;
// using Play.Services;






List<int> ints = new() { 1, 5, 1, 4 };
ints.Sort((x, y) => 1);
ints.ForEach(x => System.Console.Write(x + "\t"));
System.Console.WriteLine();

ints.Sort((x, y) => -1);
ints.ForEach(x => System.Console.Write(x + "\t"));
System.Console.WriteLine();

ints.Sort();
ints.ForEach(x => System.Console.Write(x + "\t"));
System.Console.WriteLine();

ints.Sort((x, y) => y.CompareTo(x));
ints.ForEach(x => System.Console.Write(x + "\t"));





// MysqlDbContext mysqlDbContext = new();
// while (true)
// {
//     MemoryItem memoryItem = new()
//     {
//         Id = 111111111111111111 + Random.Shared.Next(),
//         MemoryLibId = 1,
//         SrcContent = Random.Shared.Next().ToString(),
//         SrcContentMD5 = Random.Shared.Next().ToString(),
//         TgtContent = Random.Shared.Next().ToString(),
//         TgtContentMD5 = Random.Shared.Next().ToString(),
//         State = 1,
//         Matching = 1,
//         SrcLanguage = 1,
//         TgtLanguage = 1,
//         Seq = 1,
//         CreateTime = DateTime.UtcNow,
//         UpdateTime = DateTime.UtcNow,
//         Status = true,
//         EditStatus = true,
//         MemoryBatchId = 1
//     };
//     await mysqlDbContext.MemoryItems.AddAsync(memoryItem);

//     var s = await mysqlDbContext.SaveChangesAsync();

//     System.Console.WriteLine(DateTime.UtcNow.ToString() + "\t" + s + "\t" + memoryItem.Id);

//     await Task.Delay(5000);
// }
