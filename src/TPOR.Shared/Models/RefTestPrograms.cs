using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPOR.Shared.Models;

[Table("RefTestPrograms")]
public class RefTestProgram : BaseEntity
{
    [MaxLength(50)]
    public string TestProgramCode { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? Version { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
}
