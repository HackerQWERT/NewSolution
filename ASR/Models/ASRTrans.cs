namespace ASR.Models;

[Table("asr_trans_log")]
public class ASRTrans
{
    [Column("id")]
    [Key]
    public Int64 Id { get; set; }

    [Column("task_id")]
    public Int64 TaskId { get; set; }

    [Column("lang")]
    public string Language { get; set; }

    [Column("path")]
    public string Path { get; set; }

    [Column("create_time")]
    public DateTime CreateTime { get; set; }

    [Column("start_time")]
    public DateTime StartTime { get; set; }

    [Column("end_time")]
    public DateTime EndTime { get; set; }

    [Column("result")]
    public string Result { get; set; } = string.Empty;
    [Column("manifest")]
    public string Manifest { get; set; } = string.Empty;
    [Column("trans_result")]
    public string TransResult { get; set; } = string.Empty;

}
