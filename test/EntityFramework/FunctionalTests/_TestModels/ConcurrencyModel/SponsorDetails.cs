namespace ConcurrencyModel
{
    using System.ComponentModel.DataAnnotations.Schema;

    [ComplexType]
    public class SponsorDetails
    {
        public int Days { get; set; }
        public decimal Space { get; set; }
    }
}