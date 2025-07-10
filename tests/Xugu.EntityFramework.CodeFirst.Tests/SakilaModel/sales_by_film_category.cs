using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xugu.EntityFramework.CodeFirst.Tests
{
  [Table("sales_by_film_category")]
  public partial class sales_by_film_category
  {
    [Key]
    [StringLength(25)]
    public string category { get; set; }

    public decimal? total_sales { get; set; }
  }
}
