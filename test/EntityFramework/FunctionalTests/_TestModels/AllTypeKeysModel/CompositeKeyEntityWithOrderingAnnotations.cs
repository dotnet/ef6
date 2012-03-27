namespace AllTypeKeysModel
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class CompositeKeyEntityWithOrderingAnnotations
    {
        [Key]
        [Column(Order = 1)]
        public int intKey { get; set; }

        [Key]
        [Column(Order = 2)]
        public string stringKey { get; set; }

        [Key]
        [Column(Order = 3)]
        public byte[] binaryKey { get; set; }
    }
}
