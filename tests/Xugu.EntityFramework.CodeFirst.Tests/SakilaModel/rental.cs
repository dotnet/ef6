using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xugu.EntityFramework.CodeFirst.Tests
{
    [Table("rental")]
    public partial class rental
    {
        public rental()
        {
            payments = new HashSet<payment>();
        }

        [Key]
        public int rental_id { get; set; }

        public DateTime rental_date { get; set; }

        //[Column(TypeName = "umediumint")]
        public int inventory_id { get; set; }

        //[Column(TypeName = "usmallint")]
        public int customer_id { get; set; }

        public DateTime? return_date { get; set; }

        public int staff_id { get; set; }

        [Column(TypeName = "timestamp")]
        public DateTime last_update { get; set; }

        public virtual customer customer { get; set; }

        public virtual inventory inventory { get; set; }

        public virtual ICollection<payment> payments { get; set; }

        public virtual staff staff { get; set; }
    }
}
