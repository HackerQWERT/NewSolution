namespace ASR.Services;

public class MyDbContext : DbContext
{
    public static string ConnectionString { get; set; } = "Server=localhost;Port=3306;Database=ASR;Uid=root;Pwd=1234;";

    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {

    }


    public DbSet<ASRTest> ASRTest { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        if (!optionsBuilder.IsConfigured) optionsBuilder.UseMySql(ConnectionString, new MySqlServerVersion(new Version(8, 0, 20)));
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


    }

}



