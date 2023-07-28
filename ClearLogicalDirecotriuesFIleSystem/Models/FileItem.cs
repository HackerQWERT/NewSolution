
namespace ClearLogicalDirectoriesFIleSystem.Models;

[Table("file_item")]
public class FileItem
{
    [Column("_id")]
    public Int64 _Id { get; set; }

    [Column("id")]
    public Int64 Id { get; set; }

    [Column("space_id")]
    public Int64 SpaceId { get; set; }

    [Column("parent_id")]
    public Int64 ParentId { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("is_directory")]
    public bool IsDirectory { get; set; }

    [Column("state")]
    public Int16 State { get; set; }

    [Column("size")]
    public Int64 Size { get; set; }

    [Column("hash")]
    public string Hash { get; set; }

    [Column("creation_time")]
    public DateTime CreationTime { get; set; }

    [Column("update_time")]
    public DateTime UpdateTime { get; set; }

    [Column("last_use_time")]
    public DateTime LastUseTime { get; set; }
}