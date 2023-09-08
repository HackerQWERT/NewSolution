
namespace EsSyncService.Services;

public class MysqlDbContext : DbContext
{

    private readonly IConfiguration Configuration;
    public DbSet<MemoryItem> MemoryItems { get; set; }



    public MysqlDbContext(DbContextOptions<MysqlDbContext> options, IConfiguration configuration) : base(options)
    {
        this.Configuration = configuration;
    }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // 配置数据库连接
            var connectionString = Configuration.GetConnectionString("MysqlConnection");
            var serverVersion = ServerVersion.AutoDetect(connectionString);
            optionsBuilder
                        .UseMySql(connectionString, serverVersion)
                        .LogTo(_ => { });
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

    }



}
