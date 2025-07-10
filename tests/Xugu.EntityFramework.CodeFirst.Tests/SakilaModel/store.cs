using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xugu.EntityFramework.CodeFirst.Tests
{
    [Table("store")]
    public partial class store
    {
        public store()
        {
            customers = new HashSet<customer>();
            inventories = new HashSet<inventory>();
            staffs = new HashSet<staff>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int store_id { get; set; }

        public int manager_staff_id { get; set; }

        //[Column(TypeName = "usmallint")]
        public int address_id { get; set; }

        [Column(TypeName = "timestamp")]
        public DateTime last_update { get; set; }

        public virtual address address { get; set; }

        public virtual ICollection<customer> customers { get; set; }

        public virtual ICollection<inventory> inventories { get; set; }

        public virtual ICollection<staff> staffs { get; set; }

        public virtual staff staff { get; set; }
    }
}
