// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    ///     Allows configuration to be performed for a lightweight convention based on
    ///     the entity types in a model.
    /// </summary>
    public class EntityConventionConfiguration
    {
        private readonly ConventionsConfiguration _conventionsConfiguration;
        private readonly IEnumerable<Func<Type, bool>> _predicates;

        internal EntityConventionConfiguration(ConventionsConfiguration conventionsConfiguration)
            : this(conventionsConfiguration, Enumerable.Empty<Func<Type, bool>>())
        {
            DebugCheck.NotNull(conventionsConfiguration);
        }

        private EntityConventionConfiguration(ConventionsConfiguration conventionsConfiguration, IEnumerable<Func<Type, bool>> predicates)
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

        internal IEnumerable<Func<Type, bool>> Predicates
        {
            get { return _predicates; }
        }

        /// <summary>
        ///     Filters the entity types that this convention applies to based on a
        ///     predicate.
        /// </summary>
        /// <param name="predicate"> A function to test each entity type for a condition. </param>
        /// <returns> An <see cref="EntityConventionConfiguration" /> instance so that multiple calls can be chained. </returns>
        public EntityConventionConfiguration Where(Func<Type, bool> predicate)
        {
            Check.NotNull(predicate, "predicate");

            return new EntityConventionConfiguration(_conventionsConfiguration, _predicates.Append(predicate));
        }

        /// <summary>
        ///     Filters the entity types that this convention applies to based on a predicate
        ///     while capturing a value to use later during configuration.
        /// </summary>
        /// <typeparam name="T"> Type of the captured value. </typeparam>
        /// <param name="capturingPredicate">
        ///     A function to capture a value for each entity type. If the value is null, the
        ///     entity type will be filtered out.
        ///</param>
        /// <returns> An <see cref="EntityConventionWithHavingConfiguration{T}" /> instance so that multiple calls can be chained. </returns>
        public EntityConventionWithHavingConfiguration<T> Having<T>(Func<Type, T> capturingPredicate)
            where T : class
        {
            Check.NotNull(capturingPredicate, "capturingPredicate");

            return new EntityConventionWithHavingConfiguration<T>(
                _conventionsConfiguration,
                _predicates,
                capturingPredicate);
        }

        /// <summary>
        ///     Allows configuration of the entity types that this convention applies to.
        /// </summary>
        /// <param name="entityConfigurationAction">
        ///     An action that performs configuration against a
        ///     <see
        ///         cref="LightweightEntityConfiguration" />
        ///     .
        /// </param>
        public void Configure(Action<LightweightEntityConfiguration> entityConfigurationAction)
        {
            Check.NotNull(entityConfigurationAction, "entityConfigurationAction");

            _conventionsConfiguration.Add(new EntityConvention(_predicates, entityConfigurationAction));
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
