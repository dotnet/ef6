namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    [ContractClass(typeof(IDbMappingConventionContracts))]
    internal interface IDbMappingConvention : IConvention
    {
        void Apply(DbDatabaseMapping databaseMapping);
    }

    #region Interface Member Contracts

    [ContractClassFor(typeof(IDbMappingConvention))]
    internal abstract class IDbMappingConventionContracts : IDbMappingConvention
    {
        void IDbMappingConvention.Apply(DbDatabaseMapping databaseMapping)
        {
            Contract.Requires(databaseMapping != null);
        }
    }

    #endregion
}
