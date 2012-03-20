namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm;
    using System.Data.Entity.Resources;
    using System.Linq;

    /// <summary>
    ///     Convention to detect primary key properties. 
    ///     Recognized naming patterns in order of precedence are:
    ///     1. 'Id'
    ///     2. [type name]Id
    ///     Primary key detection is case insensitive.
    /// </summary>
    public sealed class IdKeyDiscoveryConvention : IEdmConvention<EdmEntityType>
    {
        private readonly IEdmConvention<EdmEntityType> _impl = new IdKeyDiscoveryConventionImpl();

        internal IdKeyDiscoveryConvention()
        {
        }

        void IEdmConvention<EdmEntityType>.Apply(EdmEntityType entityType, EdmModel model)
        {
            _impl.Apply(entityType, model);
        }

        // Nested impl. because KeyDiscoveryConvention needs to be internal for now
        private sealed class IdKeyDiscoveryConventionImpl : KeyDiscoveryConvention
        {
            private const string Id = "Id";

            protected override EdmProperty MatchKeyProperty(EdmEntityType entityType, IEnumerable<EdmProperty> primitiveProperties)
            {
                var matches = primitiveProperties
                    .Where(p => Id.Equals(p.Name, StringComparison.OrdinalIgnoreCase));
                
                if (!matches.Any())
                {
                    matches = primitiveProperties
                    .Where(p => (entityType.Name + Id).Equals(p.Name, StringComparison.OrdinalIgnoreCase));
                }

                // If the number of matches is more than one, then multiple properties matched differing only by
                // case--for example, "Id" and "ID". In such as case we throw and point the developer to using
                // data annotations or the fluent API to disambiguate.
                if (matches.Count() > 1)
                {
                    throw Error.MultiplePropertiesMatchedAsKeys(matches.First().Name, entityType.Name);
                }

                return matches.SingleOrDefault();
            }
        }
    }
}