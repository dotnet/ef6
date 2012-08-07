// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.ComponentModel;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     Configures a many:many relationship.
    ///     This configuration functionality is available via the Code First Fluent API, see <see cref="DbModelBuilder" />.
    /// </summary>
    public class ManyToManyNavigationPropertyConfiguration
    {
        private readonly NavigationPropertyConfiguration _navigationPropertyConfiguration;

        internal ManyToManyNavigationPropertyConfiguration(
            NavigationPropertyConfiguration navigationPropertyConfiguration)
        {
            Contract.Requires(navigationPropertyConfiguration != null);

            _navigationPropertyConfiguration = navigationPropertyConfiguration;
        }

        /// <summary>
        ///     Configures the foreign key column(s) and table used to store the relationship.
        /// </summary>
        /// <param name="configurationAction"> Action that configures the foreign key column(s) and table. </param>
        public void Map(Action<ManyToManyAssociationMappingConfiguration> configurationAction)
        {
            Contract.Requires(configurationAction != null);

            var manyToManyMappingConfiguration = new ManyToManyAssociationMappingConfiguration();

            configurationAction(manyToManyMappingConfiguration);

            _navigationPropertyConfiguration.AssociationMappingConfiguration = manyToManyMappingConfiguration;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }
    }
}
