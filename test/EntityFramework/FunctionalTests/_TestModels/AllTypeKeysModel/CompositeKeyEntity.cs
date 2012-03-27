namespace AllTypeKeysModel
{
    using System.ComponentModel.DataAnnotations;

    public class CompositeKeyEntity
    {
        [Key]
        public int intKey { get; set; }

        [Key]
        public string stringKey { get; set; }

        [Key]
        public byte[] binaryKey { get; set; }

        public string Details { get; set; }
    }
}