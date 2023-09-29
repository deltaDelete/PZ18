using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DbTools;
using MySqlConnector;

namespace PZ17.Models;

[Table("genders")]
public class Gender {
    [Key]
    [Column("gender_id")]
    [DbType(MySqlDbType.Int32)]
    public int GenderId { get; set; }

    [Column("name")]
    [DbType(MySqlDbType.VarChar)]
    public string Name { get; set; } = string.Empty;
}