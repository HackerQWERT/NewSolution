using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Play.Services;

public class MysqlDbContext : DbContext
{

    public DbSet<MemoryItem> MemoryItems { get; set; }



    public MysqlDbContext()
    {

    }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // 配置数据库连接
            var connectionString = "Database=DataCentre;Data Source=192.168.0.243;User Id=test;Password=esontest2019;CharSet=utf8mb4;port=3306;Pooling=true;Max Pool Size=100;Min Pool Size=5;oldguids=True;";
            var serverVersion = ServerVersion.AutoDetect(connectionString);
            optionsBuilder.UseMySql(connectionString, serverVersion);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

    }



}




[Table("MemoryItem")]
public class MemoryItem
{
    [Column("Id")]
    public Int64 Id { get; set; }

    [Column("MemoryLibId")]
    public Int64 MemoryLibId { get; set; }

    [Column("SrcContent")]
    public string SrcContent { get; set; }

    [Column("SrcContentMD5")]
    public string SrcContentMD5 { get; set; }

    [Column("TgtContent")]
    public string TgtContent { get; set; }

    [Column("TgtContentMD5")]
    public string TgtContentMD5 { get; set; }

    [Column("ReleaseNumber")]
    public Int64 State { get; set; }

    [Column("Matching")]
    public Int32 Matching { get; set; }

    [Column("SrcLanguage")]
    public Int32 SrcLanguage { get; set; }

    [Column("TgtLanguage")]
    public Int32 TgtLanguage { get; set; }

    [Column("Seq")]
    public Int32 Seq { get; set; }

    [Column("CreateTime")]
    public DateTime CreateTime { get; set; }

    [Column("UpdateTime")]
    public DateTime UpdateTime { get; set; }

    [Column("Status")]
    public bool Status { get; set; }

    [Column("EditStatus")]
    public bool EditStatus { get; set; }

    [Column("MemoryBatchId")]
    public Int64? MemoryBatchId { get; set; }

}