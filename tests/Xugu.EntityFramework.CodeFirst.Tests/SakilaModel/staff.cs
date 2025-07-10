using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xugu.EntityFramework.CodeFirst.Tests
{
    [Table("staff")]
    public partial class staff
    {
        public staff()
        {
            payments = new HashSet<payment>();
            rentals = new HashSet<rental>();
            stores = new HashSet<store>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int staff_id { get; set; }

        [Required]
        [StringLength(45)]
        public string first_name { get; set; }

        [Required]
        [StringLength(45)]
        public string last_name { get; set; }

        //[Column(TypeName = "usmallint")]
        public int address_id { get; set; }

        [Column(TypeName = "blob")]
        public int[] picture { get; set; }

        [StringLength(50)]
        public string email { get; set; }

        public int store_id { get; set; }

        public bool active { get; set; }

        [Required]
        [StringLength(16)]
        public string username { get; set; }

        [StringLength(40)]
        public string password { get; set; }

        [Column(TypeName = "timestamp")]
        public DateTime last_update { get; set; }

        public virtual address address { get; set; }

        public virtual ICollection<payment> payments { get; set; }

        public virtual ICollection<rental> rentals { get; set; }

        public virtual store store { get; set; }

        public virtual ICollection<store> stores { get; set; }
    }
}
