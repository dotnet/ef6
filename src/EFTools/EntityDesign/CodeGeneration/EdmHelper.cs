// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// Helper methods for analyzing a model.
    /// </summary>
    public class EdmHelper
    {
        private static readonly IEnumerable<INavigationPropertyConfigurationDiscoverer> _navigationPropertyConfigurationDiscoverers =
            new INavigationPropertyConfigurationDiscoverer[]
                {
                    new ForeignKeyDiscoverer(),
                    new JoinTableDiscoverer(),
                    new CascadeDeleteDiscoverer()
                };

        private readonly IEnumerable<ITypeConfigurationDiscoverer> _typeConfigurationDiscoverers;
        private readonly IEnumerable<IPropertyConfigurationDiscoverer> _propertyConfigurationDiscoverers;

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmHelper"/> class.
        /// </summary>
        /// <param name="code">The helper used to generate code.</param>
        public EdmHelper(CodeHelper code)
        {
            _typeConfigurationDiscoverers = new ITypeConfigurationDiscoverer[]
                {
                    new KeyDiscoverer(),
                    new TableDiscoverer(code)
                };

            _propertyConfigurationDiscoverers = new IPropertyConfigurationDiscoverer[]
                {
                    new KeyPropertyDiscoverer(),                    
                    new ColumnDiscoverer(code),
                    new DatabaseGeneratedDiscoverer(),
                    new PrecisionDateTimeDiscoverer(),
                    new PrecisionDecimalDiscoverer(),
                    new RequiredDiscoverer(),
                    new MaxLengthDiscoverer(),
                    new TimestampDiscoverer(),                    
                    new FixedLengthDiscoverer(),
                    new NonUnicodeDiscoverer()
                };
        }

        /// <summary>
        /// Gets the model configurations to apply to the specified entity type.
        /// </summary>
        /// <param name="entitySet">The set of the entity type.</param>
        /// <param name="model">The model.</param>
        /// <returns>The configurations.</returns>
        public IEnumerable<IConfiguration> GetConfigurations(EntitySet entitySet, DbModel model)
        {
            return _typeConfigurationDiscoverers.Select(f => f.Discover(entitySet, model)).Where(c => c != null);
        }

        /// <summary>
        /// Gets the model configurations to apply to the specified property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="model">The model.</param>
        /// <returns>The configurations.</returns>
        public IEnumerable<IConfiguration> GetConfigurations(EdmProperty property, DbModel model)
        {
            return _propertyConfigurationDiscoverers.Select(f => f.Discover(property, model)).Where(c => c != null);
        }

        /// <summary>
        /// Gets the multiplicity model configuration to apply to the specified navigation property.
        /// </summary>
        /// <param name="navigationProperty">The navigation property.</param>
        /// <param name="isDefault">A value indicating whether the configuration will be applied by default.</param>
        /// <returns>The configuration.</returns>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public IFluentConfiguration GetMultiplicityConfiguration(
            NavigationProperty navigationProperty,
            out bool isDefault)
        {
            return MultiplicityDiscoverer.Discover(navigationProperty, out isDefault);
        }

        /// <summary>
        /// Gets the model configurations to apply to the specified navigation property.
        /// </summary>
        /// <param name="navigationProperty">The navigation property.</param>
        /// <param name="model">The model.</param>
        /// <returns>The configurations.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public IEnumerable<IFluentConfiguration> GetConfigurations(
            NavigationProperty navigationProperty,
            DbModel model)
        {
            return _navigationPropertyConfigurationDiscoverers.Select(f => f.Discover(navigationProperty, model))
                .Where(c => c != null);
        }
    }
}
