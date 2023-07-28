namespace ClearLogicalDirectoriesFIleSystem.Services;

public class MyDbContext : DbContext
{
    public string ConnectionString { get; set; }

    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {

    }


    public DbSet<FileItem> FileItems { get; set; }
    public DbSet<FileItemBak> FileItemsBak { get; set; }

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



