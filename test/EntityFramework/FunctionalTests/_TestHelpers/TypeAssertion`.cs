namespace System.Data.Entity
{
    using System.Data.Metadata.Edm;
    using Xunit;

    public class TypeAssertion<TStructuralType> where TStructuralType : class
    {
        private readonly DbContext _context;

        public TypeAssertion(DbContext context)
        {
            _context = context;
        }

        public void IsNotInModel()
        {
            Assert.Null(ModelHelpers.GetEntitySetName(_context, typeof(TStructuralType)));
        }

        public void IsInModel()
        {
            Assert.NotNull(ModelHelpers.GetEntitySetName(_context, typeof(TStructuralType)));
        }

        public void IsComplexType()
        {
            Assert.NotNull(
                ModelHelpers.GetStructuralType<ComplexType>(TestBase.GetObjectContext(_context),
                                                                     typeof(TStructuralType)));
        }
    }
}