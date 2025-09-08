using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPOR.Shared.Models;

[Table("BucketObjectLogs")]
public class BucketObjectLog : BaseEntity
{
    [MaxLength(500)]
    public string ObjectName { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string BucketName { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;
    
    public long? FileSize { get; set; }
    
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }
}
