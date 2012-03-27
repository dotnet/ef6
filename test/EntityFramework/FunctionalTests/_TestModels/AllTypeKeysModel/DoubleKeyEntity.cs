namespace AllTypeKeysModel
{
    using System.ComponentModel.DataAnnotations;

    public class DoubleKeyEntity
    {
        [Key]
        public double key { get; set; }
        public string Description { get; set; }
    }
}
