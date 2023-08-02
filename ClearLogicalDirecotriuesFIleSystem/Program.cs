// 注册服务
var services = new ServiceCollection();
Serilog.Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs\\log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

services.AddSingleton<ILogger>(Serilog.Log.Logger);

// 创建服务提供程序
var serviceProvider = services.BuildServiceProvider(true);


var scope = serviceProvider.CreateAsyncScope();

// 获取服务
var logger = scope.ServiceProvider.GetRequiredService<ILogger>();

string connectionString = "Server=localhost;Port=3306;Database=A;Uid=root;Pwd=1234;";

DbContextOptions<MyDbContext> dbContextOptions = new DbContextOptionsBuilder<MyDbContext>()
    .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
    // .LogTo(Console.WriteLine)
    .Options;

MyDbContext dbContext = new(dbContextOptions);


CancellationToken stoppingToken = new CancellationToken();

// 每次查询的跨度个目录
int onceQueryCounts = 1000;


//记录一次遍历删除的目录数
int totalDeleteCounts = 0;


//记录一次查询删除的目录数
int deleteCounts = 0;

//一次查1000次，一共查询次数
int page = 1;
bool isFirstTraverse = true;
HashSet<Int64> matchedDirectoriesIdHashSet = new();
while (!stoppingToken.IsCancellationRequested)
{

    //查询onceQueryCounts条目录
    var twoDaysAgo = DateTime.Now.AddDays(-2);
    var directories = dbContext.FileItems.Where(x => x.IsDirectory && x.SpaceId == 1 && x.CreationTime < twoDaysAgo).OrderBy(x => x._Id).Skip(onceQueryCounts * page - totalDeleteCounts).Take(onceQueryCounts).ToList();
    //遍历目录
    foreach (var directory in directories)
    {
        // 查询是否有子目录或文件
        var haSubdirectories = await dbContext.FileItems.AnyAsync(x => x.ParentId == directory.Id);
        if (haSubdirectories)
            continue;
        //没有子目录或文件，直接删
        else
        {
            //检查是否为根目录，是直接删
            if (directory.ParentId == 0)
            {
                // 创建一个新的FileItemBak对象
                var fileItemBak = CreateFileItemBak(directory);

                //  使用事务 
                await DeleteAndBackup(dbContext, fileItemBak, directory, logger);
                deleteCounts++;
                matchedDirectoriesIdHashSet.Remove(directory.Id);
            }
            //不是根目录,检查根目录是否为202304格式,是则删
            else
            {
                if (matchedDirectoriesIdHashSet.Contains(directory.Id))
                {
                    //直接删，并弹出matchedDirectoriesIdHashSet

                    // 创建一个新的FileItemBak对象
                    var fileItemBak = CreateFileItemBak(directory);
                    //  使用事务删除和备份
                    await DeleteAndBackup(dbContext, fileItemBak, directory, logger);
                    deleteCounts++;
                    matchedDirectoriesIdHashSet.Remove(directory.Id);
                }
                else if (isFirstTraverse)
                {
                    //进行正常遍历，检查根目录是否为202304格式并判断是否将父节点存入matchedDirectoriesIdHashSet,如果是，直接删
                    bool isMatch = false;
                    var parentId = directory.ParentId;
                    //临时Id列表
                    List<Int64> ids = new();
                    // 循环查询根目录情况，直到查到根目录
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var parent = await dbContext.FileItems.FirstOrDefaultAsync(x => x.Id == parentId);
                        ids.Add(parent!.Id);
                        //如果是根目录
                        if (parent!.ParentId == 0)
                        {
                            string str = parent.Name.Trim();
                            isMatch = Regex.IsMatch(str, @"^\d{6}$");
                            //如果匹配202304
                            if (isMatch)
                            {
                                // 创建一个新的FileItemBak对象
                                var fileItemBak = CreateFileItemBak(directory);
                                //  使用事务删除和备份
                                await DeleteAndBackup(dbContext, fileItemBak, directory, logger);

                                deleteCounts++;
                                matchedDirectoriesIdHashSet.UnionWith(ids);
                            }
                            //已经查到根目录，跳出循环，查询下一个空目录的根目录
                            break;
                        }
                        //如果不是根目录
                        else
                        {
                            parentId = parent.ParentId;
                        }
                    }
                }
                else
                {
                    //说明不根目录不是202304格式，不用删，直接跳过
                    continue;
                }
            }
        }

    }
    // 保存更改到数据库中
    await dbContext.SaveChangesAsync();
    totalDeleteCounts += deleteCounts;
    deleteCounts = 0;
    page++;

    //查到最后一次，如果不足1000条，说明遍历结束
    if (directories.Count < onceQueryCounts)
    {
        isFirstTraverse = false;
        //如果没有空目录，遍历结束
        if (totalDeleteCounts == 0)
        {
            logger!.Information($"遍历结束，没有空目录");
            break;
        }
        //如果有删除空目录，需要再遍历一次
        else
        {
            page = 1;
            logger!.Information($"遍历结束，共删除{totalDeleteCounts}个空目录,重新开始遍历");
            totalDeleteCounts = 0;
        }

    }

}

static async Task DeleteAndBackup(MyDbContext dbContext, FileItemBak fileItemBak, FileItem directory, ILogger logger)
{
    using (var transaction = dbContext.Database.BeginTransaction())
    {
        try
        {
            // 插入到备份表
            await dbContext.FileItemsBak.AddAsync(fileItemBak);

            // 删除目录
            await dbContext.FileItems.Where(x => x.Id == directory.Id).ExecuteDeleteAsync();

            // 提交事务
            // await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            // 回滚事务
            transaction.Rollback();
            logger.Error(ex, "删除空目录出错");
        }
    }
    logger!.Warning($"删除空目录： SpaceId: {directory.SpaceId}\tID: {directory.Id}\tParentId: {directory.ParentId}\tName: {directory.Name}");

}

static FileItemBak CreateFileItemBak(FileItem directory)
{
    var fileItemBak = new FileItemBak
    {
        Id = directory.Id,
        Name = directory.Name,
        IsDirectory = directory.IsDirectory,
        SpaceId = directory.SpaceId,
        ParentId = directory.ParentId,
        _Id = directory._Id,
        State = directory.State,
        Size = directory.Size,
        Hash = directory.Hash,
        CreationTime = directory.CreationTime,
        UpdateTime = directory.UpdateTime,
        LastUseTime = directory.LastUseTime,
        DeleteTime = DateTime.Now
    };
    return fileItemBak;
}


