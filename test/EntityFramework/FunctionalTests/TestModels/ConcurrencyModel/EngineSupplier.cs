namespace ConcurrencyModel
{
    using System.Collections.Generic;

    public class EngineSupplier
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public virtual ICollection<Engine> Engines { get; set; }
    }
}
