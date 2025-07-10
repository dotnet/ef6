using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xugu.EntityFramework.CodeFirst.Tests
{
    [Table("film_category")]
    public partial class film_category
    {
        [Key, Column(Order = 0)]
        //[Column(Order = 0, TypeName = "usmallint")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int film_id { get; set; }

        [Key]
        [Column(Order = 1)]
        public int category_id { get; set; }

        [Column(TypeName = "timestamp")]
        public DateTime last_update { get; set; }

        public virtual category category { get; set; }

        public virtual film film { get; set; }
    }
}
