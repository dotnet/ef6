namespace System.Data.Entity
{
    public static class DbContextExtensions
    {
        public static TypeAssertion<TStructuralType> Assert<TStructuralType>(this DbContext context)
            where TStructuralType : class
        {
            return new TypeAssertion<TStructuralType>(context);
        }
    }
}