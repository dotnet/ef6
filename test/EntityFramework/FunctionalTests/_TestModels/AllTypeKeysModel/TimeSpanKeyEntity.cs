namespace AllTypeKeysModel
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class TimeSpanKeyEntity
    {
        [Key]
        public TimeSpan key { get; set; }
        public string Description { get; set; }
    }
}
