// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Allows configuration to be performed for a lightweight convention based on
    ///     the entity types in a model that inherit from a common, specified type and a
    ///     captured value.
    /// </summary>
    /// <typeparam name="T"> The common type of the entity types that this convention applies to. </typeparam>
    /// <typeparam name="TValue"> Type of the captured value. </typeparam>
    public class TypeConventionOfTypeWithHavingConfiguration<T, TValue>
        where T : class
        where TValue : class
    {
        private readonly ConventionsConfiguration _conventionsConfiguration;
        private readonly IEnumerable<Func<Type, bool>> _predicates;
        private readonly Func<Type, TValue> _capturingPredicate;

        internal TypeConventionOfTypeWithHavingConfiguration(
            ConventionsConfiguration conventionsConfiguration,
            IEnumerable<Func<Type, bool>> predicates,
            Func<Type, TValue> capturingPredicate)
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

        internal IEnumerable<Func<Type, bool>> Predicates
        {
            get { return _predicates; }
        }

        internal Func<Type, TValue> CapturingPredicate
        {
            get { return _capturingPredicate; }
        }

        /// <summary>
        ///     Allows configuration of the entity types that this convention applies to.
        /// </summary>
        /// <param name="entityConfigurationAction">
        ///     An action that performs configuration against a <see cref="LightweightTypeConfiguration{T}" />
        ///     using a captured value.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public void Configure(Action<LightweightTypeConfiguration<T>, TValue> entityConfigurationAction)
        {
            Check.NotNull(entityConfigurationAction, "entityConfigurationAction");

            _conventionsConfiguration.Add(
                new TypeConventionOfTypeWithHaving<T, TValue>(
                    _predicates,
                    _capturingPredicate,
                    entityConfigurationAction));
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
