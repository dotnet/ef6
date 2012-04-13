namespace AllTypeKeysModel
{
    using System.ComponentModel.DataAnnotations;

    public class ShortKeyEntity
    {
        [Key]
        public short key { get; set; }
        public string Description { get; set; }
    }
}
