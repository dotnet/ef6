namespace UnSpecifiedOrderingModel
{
    using System.Data.Entity;

    public class NoOrderingContext : DbContext
    {
        public DbSet<CompositeKeyEntityWithNoOrdering> CompositeKeyEntities { get; set; }
    }
}
