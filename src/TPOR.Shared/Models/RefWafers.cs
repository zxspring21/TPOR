using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPOR.Shared.Models;

[Table("RefWafers")]
public class RefWafer : BaseEntity
{
    [MaxLength(50)]
    public string WaferCode { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? Type { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
}
