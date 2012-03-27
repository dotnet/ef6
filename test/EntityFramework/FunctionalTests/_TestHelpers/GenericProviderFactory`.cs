namespace System.Data.Entity
{
    using System.Data.Common;

    public class GenericProviderFactory<T> : DbProviderFactory
        where T : DbProviderFactory
    {
        public static GenericProviderFactory<T> Instance = new GenericProviderFactory<T>();

        public override DbConnection CreateConnection()
        {
            return new GenericConnection();
        }
    }
}