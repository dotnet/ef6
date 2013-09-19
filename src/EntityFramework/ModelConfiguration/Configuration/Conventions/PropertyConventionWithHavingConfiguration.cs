// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    /// <summary>
    /// Allows configuration to be performed for a lightweight convention based on
    /// the properties of entity types in a model and a captured value.
    /// </summary>
    public class PropertyConventionWithHavingConfiguration<T>
        where T : class
    {
        private readonly ConventionsConfiguration _conventionsConfiguration;
        private readonly IEnumerable<Func<PropertyInfo, bool>> _predicates;
        private readonly Func<PropertyInfo, T> _capturingPredicate;

        internal PropertyConventionWithHavingConfiguration(
            ConventionsConfiguration conventionsConfiguration,
            IEnumerable<Func<PropertyInfo, bool>> predicates,
            Func<PropertyInfo, T> capturingPredicate)
        {
            DebugCheck.NotNull(conventionsConfiguration);
            DebugCheck.NotNull(predicates);
            DebugCheck.NotNull(capturingPredicate);

            _conventionsConfiguration = conventionsConfiguration;
            _predicates = predicates;
            _capturingPredicate = capturingPredicate;
        }

        internal ConventionsConfiguration ConventionsConfiguration
        {
            get { return _conventionsConfiguration; }
        }

        internal IEnumerable<Func<PropertyInfo, bool>> Predicates
        {
            get { return _predicates; }
        }

        internal Func<PropertyInfo, T> CapturingPredicate
        {
            get { return _capturingPredicate; }
        }

        /// <summary>
        /// Allows configuration of the properties that this convention applies to.
        /// </summary>
        /// <param name="propertyConfigurationAction">
        /// An action that performs configuration against a <see cref="ConventionPrimitivePropertyConfiguration" />
        /// using a captured value.
        /// </param>
        public void Configure(Action<ConventionPrimitivePropertyConfiguration, T> propertyConfigurationAction)
        {
            Check.NotNull(propertyConfigurationAction, "propertyConfigurationAction");

            _conventionsConfiguration.Add(
                new PropertyConventionWithHaving<T>(
                    _predicates,
                    _capturingPredicate,
                    propertyConfigurationAction));
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Gets the <see cref="Type" /> of the current instance.
        /// </summary>
        /// <returns>The exact runtime type of the current instance.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }
    }
}
