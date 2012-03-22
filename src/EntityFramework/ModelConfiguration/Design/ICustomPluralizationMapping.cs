namespace System.Data.Entity.ModelConfiguration.Design.PluralizationServices
{
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pluralization")]
    [ContractClass(typeof(ICustomPluralizationMappingContracts))]
    internal interface ICustomPluralizationMapping
    {
        void AddWord(string singular, string plural);
    }

    [ContractClassFor(typeof(ICustomPluralizationMapping))]
    internal abstract class ICustomPluralizationMappingContracts : ICustomPluralizationMapping
    {
        void ICustomPluralizationMapping.AddWord(string singular, string plural)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(singular));
            Contract.Requires(!string.IsNullOrWhiteSpace(plural));

            throw new NotImplementedException();
        }
    }
}
