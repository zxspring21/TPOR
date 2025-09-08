using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPOR.Shared.Models;

[Table("RefTesters")]
public class RefTester : BaseEntity
{
    [MaxLength(50)]
    public string TesterCode { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? Model { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
}
