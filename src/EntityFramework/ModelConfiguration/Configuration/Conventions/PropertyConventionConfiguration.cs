// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Collections.Generic;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Diagnostics.Contracts;
    using System.Reflection;

    /// <summary>
    /// Allows configuration to be performed for a lightweight convention based on
    /// the properties of entity types in a model.
    /// </summary>
    public class PropertyConventionConfiguration
    {
        private readonly List<Func<PropertyInfo, bool>> _predicates = new List<Func<PropertyInfo, bool>>();
        private Action<LightweightPropertyConfiguration> _configurationAction;

        internal PropertyConventionConfiguration()
        {
        }

        internal List<Func<PropertyInfo, bool>> Predicates
        {
            get { return _predicates; }
        }

        internal Action<LightweightPropertyConfiguration> ConfigurationAction
        {
            get { return _configurationAction; }
        }

        /// <summary>
        /// Filters the properties that this convention applies to based on a predicate.
        /// </summary>
        /// <param name="predicate">>A function to test each property for a condition.</param>
        /// <returns>
        /// The same PropertyConventionConfiguration instance so that multiple calls can
        /// be chained.
        /// </returns>
        public PropertyConventionConfiguration Where(Func<PropertyInfo, bool> predicate)
        {
            Contract.Requires(predicate != null);

            _predicates.Add(predicate);

            return this;
        }

        /// <summary>
        /// Allows configuration of the properties that this convention applies to.
        /// </summary>
        /// <param name="propertyConfigurationAction">
        /// An action that performs configuration against a <see cref="LightweightPropertyConfiguration" />.
        /// </param>
        public void Configure(Action<LightweightPropertyConfiguration> propertyConfigurationAction)
        {
            Contract.Requires(propertyConfigurationAction != null);

            _configurationAction = propertyConfigurationAction;
        }
    }
}
