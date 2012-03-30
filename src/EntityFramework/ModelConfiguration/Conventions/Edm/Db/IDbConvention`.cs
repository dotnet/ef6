namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Edm.Db;
    using System.Diagnostics.Contracts;

    [ContractClass(typeof(IDbConventionContracts<>))]
    internal interface IDbConvention<TDbDataModelItem> : IConvention
        where TDbDataModelItem : DbDataModelItem
    {
        void Apply(TDbDataModelItem dbDataModelItem, DbDatabaseMetadata database);
    }

    #region Interface Member Contracts

    [ContractClassFor(typeof(IDbConvention<>))]
    internal abstract class IDbConventionContracts<TDbDataModelItem> : IDbConvention<TDbDataModelItem>
        where TDbDataModelItem : DbDataModelItem
    {
        void IDbConvention<TDbDataModelItem>.Apply(TDbDataModelItem dbDataModelItem, DbDatabaseMetadata database)
        {
            Contract.Requires(dbDataModelItem != null);
            Contract.Requires(database != null);
        }
    }

    #endregion
}
