using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DbTools;
using MySqlConnector;

namespace PZ18.Models; 

[Table("procedures_clients")]
public class ProcedureClient {
    [Key]
    [Column("id")]
    [DbType(MySqlDbType.Int32)]
    public int Id { get; set; }
    // Использует название столбца из другой таблицы
    [ForeignKey("procedures.procedure_id")]
    [Column("procedure_id")]
    [DbType(MySqlDbType.Int32)]
    public int ProcedureId { get; set; }

    public Procedure? Procedure { get; set; }
    [ForeignKey("clients.client_id")]
    [Column("client_id")]
    [DbType(MySqlDbType.Int32)]
    public int ClientId { get; set; }
    public Client? Client { get; set; }
    [Column("price")] 
    [DbType(MySqlDbType.Decimal)]
    public decimal Price { get; set; }
    [Column("date")] 
    [DbType(MySqlDbType.DateTime)]
    public DateTime Date { get; set; }
}