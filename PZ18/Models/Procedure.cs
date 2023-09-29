using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DbTools;
using MySqlConnector;

namespace PZ17.Models; 

[Table("procedures")]
public class Procedure {
    [Key]
    [Column("procedure_id")]
    [DbType(MySqlDbType.Int32)]
    public int ProcedureId { get; set; }

    [Column("procedure_name")]
    [DbType(MySqlDbType.VarChar)]
    public string ProcedureName { get; set; } = string.Empty;
    [Column("base_price")]
    [DbType(MySqlDbType.Decimal)]
    public decimal BasePrice { get; set; }
}