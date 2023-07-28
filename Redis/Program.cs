var redisConfiguration = ConfigurationOptions.Parse("localhost:6379"); // 指定 Redis 服务器的地址和端口
var redisConnection = ConnectionMultiplexer.Connect(redisConfiguration);
var redisDatabase = redisConnection.GetDatabase();

// 设置字符串、哈希、列表、集合和有序集合类型的值
redisDatabase.StringSet("name", "John Doe", TimeSpan.FromDays(1));
redisDatabase.HashSet("user:1", new HashEntry[] {
    new HashEntry("name", "John Doe"),
    new HashEntry("age", 30),
    new HashEntry("email", "johndoe@example.com")
});
redisDatabase.ListRightPush("list", "item1");
redisDatabase.SetAdd("set", "item1");
redisDatabase.SortedSetAdd("sortedset", "item1", 1);
await redisDatabase.KeyExpireAsync("name", TimeSpan.FromDays(1));
await redisDatabase.KeyExpireTimeAsync("name");

// 获取所有键
RedisKey[] keys = redisConnection.GetServer(redisConfiguration.EndPoints[0]).Keys().ToArray();

// 获取所有键对应的值
foreach (RedisKey key in keys)
{
    RedisType type = redisDatabase.KeyType(key);
    switch (type)
    {
        case RedisType.String:
            RedisValue stringValue = redisDatabase.StringGet(key);
            Console.WriteLine(key + ": " + stringValue);
            break;
        case RedisType.Hash:
            HashEntry[] hashEntries = redisDatabase.HashGetAll(key);
            Console.WriteLine(key + ":");
            foreach (HashEntry entry in hashEntries)
            {
                Console.WriteLine("  " + entry.Name + ": " + entry.Value);
            }
            break;
        case RedisType.List:
            RedisValue[] listValues = redisDatabase.ListRange(key);
            Console.WriteLine(key + ":");
            foreach (RedisValue value in listValues)
            {
                Console.WriteLine("  " + value);
            }
            break;
        case RedisType.Set:
            RedisValue[] setValues = redisDatabase.SetMembers(key);
            Console.WriteLine(key + ":");
            foreach (RedisValue value in setValues)
            {
                Console.WriteLine("  " + value);
            }
            break;
        case RedisType.SortedSet:
            SortedSetEntry[] sortedSetEntries = redisDatabase.SortedSetRangeByRankWithScores(key);
            Console.WriteLine(key + ":");
            foreach (SortedSetEntry entry in sortedSetEntries)
            {
                Console.WriteLine("  " + entry.Element + ": " + entry.Score);
            }
            break;
        default:
            Console.WriteLine(key + ": unknown type");
            break;
    }
}

// 删除所有键
redisDatabase.KeyDelete(keys);



redisConnection.Dispose();
