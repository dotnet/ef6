namespace ConcurrencyModel
{
    using System.ComponentModel.DataAnnotations;

    public class Location
    {
        [ConcurrencyCheck]
        public double Latitude { get; set; }

        [ConcurrencyCheck]
        public double Longitude { get; set; }
    }
}
