using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xugu.EntityFramework.CodeFirst.Tests
{
  [Table("customer_list")]
  public partial class customer_list
  {
    [Key,Column(Order = 0)]
    //[Column(Order = 0, TypeName = "usmallint")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int ID { get; set; }

    [Key]
    [Column(Order = 1)]
    [StringLength(91)]
    public string name { get; set; }

    [Key]
    [Column(Order = 2)]
    [StringLength(50)]
    public string address { get; set; }

    //[Column("zip code")]
    [StringLength(10)]
    public string zip_code { get; set; }

    [Key]
    [Column(Order = 3)]
    [StringLength(20)]
    public string phone { get; set; }

    [Key]
    [Column(Order = 4)]
    [StringLength(50)]
    public string city { get; set; }

    [Key]
    [Column(Order = 5)]
    [StringLength(50)]
    public string country { get; set; }

    [Key]
    [Column(Order = 6)]
    [StringLength(6)]
    public string notes { get; set; }

    [Key]
    [Column(Order = 7)]
    public int SID { get; set; }
  }
}
