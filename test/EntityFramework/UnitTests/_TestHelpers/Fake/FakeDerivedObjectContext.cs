namespace System.Data.Entity.ModelConfiguration.Internal.UnitTests
{
    using System.Data.Objects;

    public class FakeDerivedObjectContext : ObjectContext
    {
        public FakeDerivedObjectContext()
            : base("")
        {
        }
    }
}