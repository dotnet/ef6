namespace AllTypeKeysModel
{
    using System.ComponentModel.DataAnnotations;

    public class BoolKeyEntity
    {
        [Key]
        public bool key { get; set; }
        public string Description { get; set; }
    }
}
