namespace AdvancedPatternsModel
{
    using System;

    public class BuildingDetail
    {
        public Guid BuildingId { get; set; }
        public Building Building { get; set; }
        public string Details { get; set; }
    }
}
