// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Collections.Generic;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     Allows configuration to be performed for a lightweight convention based on
    ///     the entity types in a model.
    /// </summary>
    public class EntityConventionConfiguration
    {
        private readonly List<Func<Type, bool>> _predicates = new List<Func<Type, bool>>();
        private Action<LightweightEntityConfiguration> _configurationAction;
        private PropertyConventionConfiguration _propertyConfiguration;

        internal EntityConventionConfiguration()
        {
        }

        internal Action<LightweightEntityConfiguration> ConfigurationAction
        {
            get { return _configurationAction; }
        }

        internal List<Func<Type, bool>> Predicates
        {
            get { return _predicates; }
        }

        internal PropertyConventionConfiguration PropertyConfiguration
        {
            get { return _propertyConfiguration; }
        }

        /// <summary>
        ///     Filters the entity types that this convention applies to based on a
        ///     predicate.
        /// </summary>
        /// <param name="predicate"> A function to test each entity type for a condition. </param>
        /// <returns> The same EntityConventionConfiguration instance so that multiple calls can be chained. </returns>
        public EntityConventionConfiguration Where(Func<Type, bool> predicate)
        {
            Contract.Requires(predicate != null);

            _predicates.Add(predicate);

            return this;
        }

        /// <summary>
        ///     Allows configuration of the entity types that this convention applies to.
        /// </summary>
        /// <param name="entityConfigurationAction"> An action that performs configuration against a <see
        ///      cref="LightweightEntityConfiguration" /> . </param>
        public void Configure(Action<LightweightEntityConfiguration> entityConfigurationAction)
        {
            Contract.Requires(entityConfigurationAction != null);

            _configurationAction = entityConfigurationAction;
        }

        /// <summary>
        ///     Allows further configuration of the convention based on the properties of
        ///     the entity types that this convention applies to.
        /// </summary>
        /// <returns> A configuration object that can be used to configure this convention based on properties. </returns>
        public PropertyConventionConfiguration Properties()
        {
            var propertyConfiguration = new PropertyConventionConfiguration();
            _propertyConfiguration = propertyConfiguration;

            return propertyConfiguration;
        }
    }
}
