namespace UnSpecifiedOrderingModel
{
    using System.ComponentModel.DataAnnotations;

    public class CompositeKeyEntityWithNoOrdering
    {
        [Key]
        public int intKey { get; set; }

        [Key]
        public float floatKey { get; set; }

        [Key]
        public byte[] binaryKey { get; set; }
    }
}
