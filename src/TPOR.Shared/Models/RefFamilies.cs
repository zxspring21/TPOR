using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPOR.Shared.Models;

[Table("RefFamilies")]
public class RefFamily : BaseEntity
{
    [MaxLength(50)]
    public string FamilyCode { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
}
