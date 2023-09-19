using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PZ17.Models; 

[Table("procedures_clients")]
public class ProcedureClient {
    [Key]
    [Column("id")]
    public int Id { get; set; }
    // Использует название столбца из другой таблицы
    [ForeignKey("procedures.procedure_id")]
    [Column("procedure_id")]
    public int ProcedureId { get; set; }

    public Procedure? Procedure { get; set; }
    [ForeignKey("clients.client_id")]
    [Column("client_id")]
    public int ClientId { get; set; }
    public Client? Client { get; set; }
    [Column("price")] 
    public decimal Price { get; set; }
    [Column("date")] 
    public DateTime Date { get; set; }
}