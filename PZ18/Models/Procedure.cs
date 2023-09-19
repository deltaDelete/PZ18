using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PZ17.Models; 

[Table("procedures")]
public class Procedure {
    [Key]
    [Column("procedure_id")]
    public int ProcedureId { get; set; }
    [Column("procedure_name")]
    public string ProcedureName { get; set; }
    [Column("base_price")]
    public decimal BasePrice { get; set; }
}