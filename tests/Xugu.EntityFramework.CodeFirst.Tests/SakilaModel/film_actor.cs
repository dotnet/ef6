using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xugu.EntityFramework.CodeFirst.Tests
{
    [Table("film_actor")]
    public partial class film_actor
    {
        [Key, Column(Order = 0)]
        //[Column(Order = 0, TypeName = "usmallint")]
        //[DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int actor_id { get; set; }

        [Key, Column(Order = 1)]
        //[Column(Order = 1, TypeName = "usmallint")]
        //[DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int film_id { get; set; }

        [Column(TypeName = "timestamp")]
        public DateTime last_update { get; set; }

        public virtual actor actor { get; set; }

        public virtual film film { get; set; }
    }
}
