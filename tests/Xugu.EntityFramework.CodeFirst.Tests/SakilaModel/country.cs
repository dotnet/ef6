using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xugu.EntityFramework.CodeFirst.Tests
{
    [Table("country")]
    public partial class country
    {
        public country()
        {
            cities = new HashSet<city>();
        }

        [Key]
        //[Column(TypeName = "usmallint")]
        public int country_id { get; set; }

        [Column("country")]
        [Required]
        [StringLength(50)]
        public string country1 { get; set; }

        [Column(TypeName = "timestamp")]
        public DateTime last_update { get; set; }

        public virtual ICollection<city> cities { get; set; }
    }
}
