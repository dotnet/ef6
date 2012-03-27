namespace AllTypeKeysModel
{
    using System.ComponentModel.DataAnnotations;

    public class DecimalKeyEntity
    {
        [Key]
        public decimal key { get; set; }
        public string Description { get; set; }
    }
}
