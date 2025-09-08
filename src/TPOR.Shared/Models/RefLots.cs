using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPOR.Shared.Models;

[Table("RefLots")]
public class RefLot : BaseEntity
{
    [MaxLength(50)]
    public string LotCode { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? BatchNumber { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
}
