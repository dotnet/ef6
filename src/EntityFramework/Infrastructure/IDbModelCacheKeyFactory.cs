namespace System.Data.Entity.Infrastructure
{
    using System.Diagnostics.Contracts;

    [ContractClass(typeof(IDbModelCacheKeyFactoryContracts))]
    public interface IDbModelCacheKeyFactory
    {
        IDbModelCacheKey Create(DbContext context);
    }
    
    #region Interface Member Contracts

    [ContractClassFor(typeof(IDbModelCacheKeyFactory))]
    internal abstract class IDbModelCacheKeyFactoryContracts : IDbModelCacheKeyFactory
    {
        public IDbModelCacheKey Create(DbContext context)
        {
            Contract.Requires(context != null);

            throw new NotImplementedException();
        }
    }

    #endregion
}
