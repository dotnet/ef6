using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xugu.EntityFramework.CodeFirst.Tests
{
  [Table("actor_info")]
  public partial class actor_info
  {
    [Key,Column(Order = 0)]
    //[Column(Order = 0, TypeName = "usmallint")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int actor_id { get; set; }

    [Key]
    [Column(Order = 1)]
    [StringLength(45)]
    public string first_name { get; set; }

    [Key]
    [Column(Order = 2)]
    [StringLength(45)]
    public string last_name { get; set; }

    [Column(TypeName = "clob")]
    //[StringLength(65535)]
    public string film_info { get; set; }
  }
}
