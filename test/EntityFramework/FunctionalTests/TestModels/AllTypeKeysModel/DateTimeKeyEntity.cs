namespace AllTypeKeysModel
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class DateTimeKeyEntity
    {
        [Key]
        public DateTime key { get; set; }
        public string Description { get; set; }
    }
}
