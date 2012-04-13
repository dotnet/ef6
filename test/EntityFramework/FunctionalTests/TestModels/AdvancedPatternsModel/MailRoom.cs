namespace AdvancedPatternsModel
{
    using System;

    public class MailRoom
    {
        public int id { get; set; }
        public Building Building { get; set; }
        public Guid BuildingId { get; set; }
    }
}
