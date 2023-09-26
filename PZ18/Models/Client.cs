using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MySqlConnector;

namespace PZ17.Models;

[Table("clients")]
public class Client {
    [Key]
    [Column("client_id")]
    [DbType(MySqlDbType.Int32)]
    public int ClientId { get; set; }
    [DbType(MySqlDbType.VarChar)]
    [Column("first_name")]
    [DisplayName("Имя")]
    public string FirstName { get; set; } = string.Empty;

    [Column("last_name")]
    [DbType(MySqlDbType.VarChar)]
    [DisplayName("Фамилия")]
    public string LastName { get; set; } = string.Empty;

    [Column("gender_id")]
    [DbType(MySqlDbType.Int32)]
    public int GenderId { get; set; }

    public Gender? Gender { get; set; }
}