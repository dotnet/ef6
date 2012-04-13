namespace SimpleModel
{
    using System.Collections.Generic;

    public class Category
    {
        public Category() 
        {
            this.Products = new List<Product>();
        }

        public Category(string id) : this()
        { 
            Id = id; 
        }
        public string Id { get; set; }
        public ICollection<Product> Products { get; set; }
        public string DetailedDescription { get; set; }
    }
}
