using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xugu.EntityFramework.CodeFirst.Tests
{
    [Table("film")]
    public partial class film
    {
        public film()
        {
            film_actor = new HashSet<film_actor>();
            film_category = new HashSet<film_category>();
            inventories = new HashSet<inventory>();
        }

        [Key]
        //[Column(TypeName = "usmallint")]
        public int film_id { get; set; }

        [Required]
        [StringLength(255)]
        public string title { get; set; }

        [Column(TypeName = "clob")]
        //[StringLength(65535)]
        public string description { get; set; }

        //[Column(TypeName = "year")]
        public int? release_year { get; set; }

        public int language_id { get; set; }

        public int? original_language_id { get; set; }

        public int rental_duration { get; set; }

        public decimal rental_rate { get; set; }

        //[Column(TypeName = "usmallint")]
        public int? length { get; set; }

        public decimal replacement_cost { get; set; }

        //[Column(TypeName = "enum")]
        [StringLength(59999)]
        public string rating { get; set; }

        //[Column(TypeName = "set")]
        [StringLength(59999)]
        public string special_features { get; set; }

        [Column(TypeName = "timestamp")]
        public DateTime last_update { get; set; }

        public virtual ICollection<film_actor> film_actor { get; set; }

        public virtual ICollection<film_category> film_category { get; set; }

        public virtual language language { get; set; }

        public virtual language language1 { get; set; }

        public virtual ICollection<inventory> inventories { get; set; }
    }
}
