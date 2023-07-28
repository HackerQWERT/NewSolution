using System.ComponentModel.DataAnnotations;

namespace ASR.Models;


[Table("asr_test")]
public class ASRTest
{
    [Column("id")]
    [Key]
    public Int64 Id { get; set; }

    [Column("language")]
    public string Language { get; set; }

    [Column("path")]
    public string Path { get; set; }

    [Column("start_time")]
    public DateTime StartTime { get; set; }

    [Column("end_time")]
    public DateTime EndTime { get; set; }

    [Column("cost_time")]
    public TimeSpan CostTime { get; set; }

    [Column("cost_nanos")]
    public string CostNanoTime { get; set; }

    [Column("status")]
    public string Status { get; set; }

    [Column("content")]
    public string Content { get; set; }
}

