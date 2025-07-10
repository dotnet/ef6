using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xugu.EntityFramework.CodeFirst.Tests
{
    [Table("city")]
    public partial class city
    {
        public city()
        {
            addresses = new HashSet<address>();
        }

        [Key]
        //[Column(TypeName = "usmallint")]
        public int city_id { get; set; }

        [Column("city")]
        [Required]
        [StringLength(50)]
        public string city1 { get; set; }

        //[Column(TypeName = "usmallint")]
        public int country_id { get; set; }

        [Column(TypeName = "timestamp")]
        public DateTime last_update { get; set; }

        public virtual ICollection<address> addresses { get; set; }

        public virtual country country { get; set; }
    }
}
