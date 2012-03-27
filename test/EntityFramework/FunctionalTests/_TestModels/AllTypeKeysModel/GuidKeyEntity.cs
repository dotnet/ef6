namespace AllTypeKeysModel
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class GuidKeyEntity
    {
        [Key]
        public Guid key { get; set; }
        public string Description { get; set; }
    }
}
