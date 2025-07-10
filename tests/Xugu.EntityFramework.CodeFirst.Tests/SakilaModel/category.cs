using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xugu.EntityFramework.CodeFirst.Tests
{
    [Table("category")]
    public partial class category
    {
        public category()
        {
            film_category = new HashSet<film_category>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int category_id { get; set; }

        [Required]
        [StringLength(25)]
        public string name { get; set; }

        [Column(TypeName = "timestamp")]
        public DateTime last_update { get; set; }

        public virtual ICollection<film_category> film_category { get; set; }
    }
}
