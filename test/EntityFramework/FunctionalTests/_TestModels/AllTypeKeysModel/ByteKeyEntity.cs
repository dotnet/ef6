namespace AllTypeKeysModel
{
    using System.ComponentModel.DataAnnotations;

    public class ByteKeyEntity
    {
        [Key]
        public byte key { get; set; }
        public string Description { get; set; }
    }
}
