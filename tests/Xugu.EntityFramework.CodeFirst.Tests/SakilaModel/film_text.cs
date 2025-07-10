using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xugu.EntityFramework.CodeFirst.Tests
{
  [Table("film_text")]
  public partial class film_text
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public short film_id { get; set; }

    [Required]
    [StringLength(255)]
    public string title { get; set; }

    [Column(TypeName = "clob")]
    //[StringLength(65535)]
    public string description { get; set; }
  }
}
