namespace ASR.Models;



[Table("asr_zs_test")]
public class ASRZsTest
{
    private const string? V = default;

    [Column("id")]
    [Key]
    public Int64 Id { get; set; }

    [Column("language")]
    public string Language { get; set; }

    [Column("path")]
    public string Path { get; set; }

    [Column("start_time")]
    public DateTime StartTime { get; set; }


    [Column("base_cost_nanoseconds")]
    public Int64 BaseCostNanoseconds { get; set; }

    [Column("small_cost_nanoseconds")]
    public Int64 SmallCostNanoseconds { get; set; }
    [Column("medium_cost_nanoseconds")]
    public Int64 MediumCostNanoseconds { get; set; }


    [Column("status")]
    public string Status { get; set; }

    [Column("base_content")]
    public string BaseContent { get; set; } = string.Empty;
    [Column("small_content")]
    public string SmallContent { get; set; } = string.Empty;
    [Column("medium_content")]
    public string MediumContent { get; set; } = string.Empty;
}
