using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPOR.Shared.Models;

[Table("DataLotAttributes")]
public class DataLotAttribute : BaseEntity
{
    [MaxLength(50)]
    public string LotCode { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string AttributeName { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? AttributeValue { get; set; }
    
    [MaxLength(50)]
    public string? DataType { get; set; }
}
