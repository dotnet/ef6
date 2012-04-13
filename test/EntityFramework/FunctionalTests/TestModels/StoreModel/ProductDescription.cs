namespace FunctionalTests.Model
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class ProductDescription
    {
        public virtual int ProductDescriptionID { get; set; }
        public virtual string Description { get; set; }

        [Required]
        public virtual RowDetails RowDetails { get; set; }

        public virtual ICollection<ProductModelProductDescriptionCulture> ProductModelProductDescriptionCultures { get; set; }
    }
}