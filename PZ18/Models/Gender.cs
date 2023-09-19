using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PZ17.Models;

[Table("gender")]
public class Gender {
    [Key] [Column("gender_id")] public int GenderId { get; set; }

    [Column("name")] public string Name { get; set; } = string.Empty;
}