namespace AdvancedPatternsModel
{
    using System;
    using System.Collections.Generic;

    public class Office : UnMappedOfficeBase
    {
        public Office()
        {
            this.WhiteBoards = new List<Whiteboard>();
        }

        public Guid BuildingId { get; set; }
        public Building Building { get; set; }
        public IList<Whiteboard> WhiteBoards { get; set; }
    }
}
