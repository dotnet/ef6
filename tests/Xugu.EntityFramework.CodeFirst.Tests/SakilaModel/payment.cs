using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xugu.EntityFramework.CodeFirst.Tests
{
    [Table("payment")]
    public partial class payment
    {
        [Key]
        //[Column(TypeName = "usmallint")]
        public int payment_id { get; set; }

        //[Column(TypeName = "usmallint")]
        public int customer_id { get; set; }

        public int staff_id { get; set; }

        public int? rental_id { get; set; }

        public decimal amount { get; set; }

        public DateTime payment_date { get; set; }

        [Column(TypeName = "timestamp")]
        public DateTime last_update { get; set; }

        public virtual customer customer { get; set; }

        public virtual rental rental { get; set; }

        public virtual staff staff { get; set; }
    }
}
