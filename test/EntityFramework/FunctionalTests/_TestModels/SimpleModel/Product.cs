namespace SimpleModel
{
    public class Product : ProductBase
    {
        public string CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
