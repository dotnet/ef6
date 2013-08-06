// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    ///     Allows configuration to be performed for a lightweight convention based on
    ///     the properties in a model.
    /// </summary>
    public class PropertyConventionConfiguration
    {
        private readonly ConventionsConfiguration _conventionsConfiguration;
        private readonly IEnumerable<Func<PropertyInfo, bool>> _predicates;

        internal PropertyConventionConfiguration(ConventionsConfiguration conventionsConfiguration)
            : this(conventionsConfiguration, Enumerable.Empty<Func<PropertyInfo, bool>>())
        {
            DebugCheck.NotNull(conventionsConfiguration);
        }

        private PropertyConventionConfiguration(
            ConventionsConfiguration conventionsConfiguration,
            IEnumerable<Func<PropertyInfo, bool>> predicates)
        {
            DebugCheck.NotNull(conventionsConfiguration);
            DebugCheck.NotNull(predicates);

            _conventionsConfiguration = conventionsConfiguration;
            _predicates = predicates;
        }

        internal ConventionsConfiguration ConventionsConfiguration
        {
            get { return _conventionsConfiguration; }
        }

        internal IEnumerable<Func<PropertyInfo, bool>> Predicates
        {
            get { return _predicates; }
        }

        /// <summary>
        ///     Filters the properties that this convention applies to based on a predicate.
        /// </summary>
        /// <param name="predicate"> A function to test each property for a condition. </param>
        /// <returns>
        ///     A <see cref="PropertyConventionConfiguration" /> instance so that multiple calls can be chained.
        /// </returns>
        public PropertyConventionConfiguration Where(Func<PropertyInfo, bool> predicate)
        {
            Check.NotNull(predicate, "predicate");

            return new PropertyConventionConfiguration(_conventionsConfiguration, _predicates.Append(predicate));
        }

        /// <summary>
        ///     Filters the properties that this convention applies to based on a predicate
        ///     while capturing a value to use later during configuration.
        /// </summary>
        /// <typeparam name="T"> Type of the captured value. </typeparam>
        /// <param name="capturingPredicate">
        ///     A function to capture a value for each property. If the value is null, the
        ///     property will be filtered out.
        /// </param>
        /// <returns>
        ///     A <see cref="PropertyConventionWithHavingConfiguration{T}" /> instance so that multiple calls can be chained.
        /// </returns>
        public PropertyConventionWithHavingConfiguration<T> Having<T>(
            Func<PropertyInfo, T> capturingPredicate)
            where T : class
        {
            Check.NotNull(capturingPredicate, "capturingPredicate");

            return new PropertyConventionWithHavingConfiguration<T>(
                _conventionsConfiguration,
                _predicates,
                capturingPredicate);
        }

        /// <summary>
        ///     Allows configuration of the properties that this convention applies to.
        /// </summary>
        /// <param name="propertyConfigurationAction">
        ///     An action that performs configuration against a
        ///     <see
        ///         cref="ConventionPrimitivePropertyConfiguration" />
        ///     .
        /// </param>
        public void Configure(Action<ConventionPrimitivePropertyConfiguration> propertyConfigurationAction)
        {
            Check.NotNull(propertyConfigurationAction, "propertyConfigurationAction");

            _conventionsConfiguration.Add(
                new PropertyConvention(
                    _predicates,
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
