using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xugu.EntityFramework.CodeFirst.Tests
{
    [Table("inventory")]
    public partial class inventory
    {
        public inventory()
        {
            rentals = new HashSet<rental>();
        }

        [Key]
        //[Column(TypeName = "umediumint")]
        public int inventory_id { get; set; }

        //[Column(TypeName = "smallint")]
        public int film_id { get; set; }

        public int store_id { get; set; }

        [Column(TypeName = "timestamp")]
        public DateTime last_update { get; set; }

        public virtual film film { get; set; }

        public virtual store store { get; set; }

        public virtual ICollection<rental> rentals { get; set; }
    }
}
