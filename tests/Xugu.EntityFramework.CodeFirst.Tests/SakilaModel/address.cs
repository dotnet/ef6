using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xugu.EntityFramework.CodeFirst.Tests
{
  [Table("address")]
  public partial class address
  {
    public address()
    {
      customers = new HashSet<customer>();
      staffs = new HashSet<staff>();
      stores = new HashSet<store>();
    }

    [Key]
    //[Column(TypeName = "usmallint")]
    public int address_id { get; set; }

    [Column("address")]
    [Required]
    [StringLength(50)]
    public string address1 { get; set; }

    [StringLength(50)]
    public string address2 { get; set; }

    [Required]
    [StringLength(20)]
    public string district { get; set; }

    //[Column(TypeName = "usmallint")]
    public int city_id { get; set; }

    [StringLength(10)]
    public string postal_code { get; set; }

    [Required]
    [StringLength(20)]
    public string phone { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime last_update { get; set; }

    public virtual city city { get; set; }

    public virtual ICollection<customer> customers { get; set; }

    public virtual ICollection<staff> staffs { get; set; }

    public virtual ICollection<store> stores { get; set; }
  }
}
