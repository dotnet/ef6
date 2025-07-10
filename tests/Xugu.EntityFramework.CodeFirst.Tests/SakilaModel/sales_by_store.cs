using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xugu.EntityFramework.CodeFirst.Tests
{
  [Table("sales_by_store")]
  public partial class sales_by_store
  {
    [Key]
    [Column(Order = 0)]
    [StringLength(101)]
    public string store { get; set; }

    [Key]
    [Column(Order = 1)]
    [StringLength(91)]
    public string manager { get; set; }

    public decimal? total_sales { get; set; }
  }
}
