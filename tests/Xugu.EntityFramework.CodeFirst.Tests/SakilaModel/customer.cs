using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xugu.EntityFramework.CodeFirst.Tests
{
    [Table("customer")]
    public partial class customer
    {
        public customer()
        {
            payments = new HashSet<payment>();
            rentals = new HashSet<rental>();
        }

        [Key]
        //[Column(TypeName = "usmallint")]
        public int customer_id { get; set; }

        public int store_id { get; set; }

        [Required]
        [StringLength(45)]
        public string first_name { get; set; }

        [Required]
        [StringLength(45)]
        public string last_name { get; set; }

        [StringLength(50)]
        public string email { get; set; }

        //[Column(TypeName = "usmallint")]
        public int address_id { get; set; }

        public bool active { get; set; }

        public DateTime create_date { get; set; }

        [Column(TypeName = "timestamp")]
        public DateTime last_update { get; set; }

        public virtual address address { get; set; }

        public virtual store store { get; set; }

        public virtual ICollection<payment> payments { get; set; }

        public virtual ICollection<rental> rentals { get; set; }
    }
}
