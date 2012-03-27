namespace AllTypeKeysModel
{
    using System.ComponentModel.DataAnnotations;

    public class FloatKeyEntity
    {
        [Key]
        public float key { get; set; }
        public string Description { get; set; }
    }
}
