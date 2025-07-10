using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xugu.EntityFramework.CodeFirst.Tests
{
  [Table("nicer_but_slower_film_list")]
  public partial class nicer_but_slower_film_list
  {
    //[Column(TypeName = "usmallint")]
    public int? FID { get; set; }

    [StringLength(255)]
    public string title { get; set; }

    [Column(TypeName = "clob")]
    //[StringLength(65535)]
    public string description { get; set; }

    [Key]
    [StringLength(25)]
    public string category { get; set; }

    public decimal? price { get; set; }

    //[Column(TypeName = "usmallint")]
    public int? length { get; set; }

    //[Column(TypeName = "enum")]
    [StringLength(59999)]
    public string rating { get; set; }

    [Column(TypeName = "clob")]
    //[StringLength(65535)]
    public string actors { get; set; }
  }
}
