using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xugu.EntityFramework.CodeFirst.Tests
{
    [Table("language")]
    public partial class language
    {
        public language()
        {
            films = new HashSet<film>();
            films1 = new HashSet<film>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int language_id { get; set; }

        [Column(TypeName = "char")]
        [Required]
        [StringLength(20)]
        public string name { get; set; }

        [Column(TypeName = "timestamp")]
        public DateTime last_update { get; set; }

        public virtual ICollection<film> films { get; set; }

        public virtual ICollection<film> films1 { get; set; }
    }
}
