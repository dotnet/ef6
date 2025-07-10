using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xugu.EntityFramework.CodeFirst.Tests
{
    [Table("actor")]
    public partial class actor
    {
        public actor()
        {
            film_actor = new HashSet<film_actor>();
        }

        [Key]
        //[Column(TypeName = "smallint")]
        public int actor_id { get; set; }

        [Required]
        [StringLength(45)]
        public string first_name { get; set; }

        [Required]
        [StringLength(45)]
        public string last_name { get; set; }

        [Column(TypeName = "timestamp")]
        public DateTime last_update { get; set; }

        public virtual ICollection<film_actor> film_actor { get; set; }
    }
}
