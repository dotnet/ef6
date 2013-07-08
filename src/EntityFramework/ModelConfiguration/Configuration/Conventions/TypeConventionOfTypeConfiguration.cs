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
    ///     the entity types in a model that inherit from a common, specified type.
    /// </summary>
    /// <typeparam name="T"> The common type of the entity types that this convention applies to. </typeparam>
    public class TypeConventionOfTypeConfiguration<T>
        where T : class
    {
        private readonly ConventionsConfiguration _conventionsConfiguration;
        private readonly IEnumerable<Func<Type, bool>> _predicates;

        internal TypeConventionOfTypeConfiguration(ConventionsConfiguration conventionsConfiguration)
            : this(conventionsConfiguration, Enumerable.Empty<Func<Type, bool>>())
        {
            DebugCheck.NotNull(conventionsConfiguration);
        }

        private TypeConventionOfTypeConfiguration(
            ConventionsConfiguration conventionsConfiguration,
            IEnumerable<Func<Type, bool>> predicates)
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
        /// <returns>
        ///     An <see cref="TypeConventionOfTypeConfiguration{T}" /> instance so that multiple calls can be chained.
        /// </returns>
        public TypeConventionOfTypeConfiguration<T> Where(Func<Type, bool> predicate)
        {
            Check.NotNull(predicate, "predicate");

            return new TypeConventionOfTypeConfiguration<T>(
                _conventionsConfiguration,
                _predicates.Append(predicate));
        }

        /// <summary>
        ///     Filters the entity types that this convention applies to based on a predicate
        ///     while capturing a value to use later during configuration.
        /// </summary>
        /// <typeparam name="TValue"> Type of the captured value. </typeparam>
        /// <param name="capturingPredicate">
        ///     A function to capture a value for each entity type. If the value is null, the
        ///     entity type will be filtered out.
        /// </param>
        /// <returns>
        ///     An <see cref="TypeConventionOfTypeWithHavingConfiguration{T,TValue}" /> instance so that multiple calls can be chained.
        /// </returns>
        public TypeConventionOfTypeWithHavingConfiguration<T, TValue> Having<TValue>(Func<Type, TValue> capturingPredicate)
            where TValue : class
        {
            Check.NotNull(capturingPredicate, "capturingPredicate");

            return new TypeConventionOfTypeWithHavingConfiguration<T, TValue>(
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
        ///         cref="LightweightTypeConfiguration{T}" />
        ///     .
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public void Configure(Action<LightweightTypeConfiguration<T>> entityConfigurationAction)
        {
            Check.NotNull(entityConfigurationAction, "entityConfigurationAction");

            _conventionsConfiguration.Add(new TypeConventionOfType<T>(_predicates, entityConfigurationAction));
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

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }
    }
}
