namespace AllTypeKeysModel
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class DateTimeOffSetKeyEntity
    {
        [Key]
        public DateTimeOffset key { get; set; }
        public string Description { get; set; }
    }
}
