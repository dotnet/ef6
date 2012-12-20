// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Data.Entity.ModelConfiguration.Conventions.Sets;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    ///     Allows the conventions used by a <see cref="DbModelBuilder" /> instance to be customized.
    ///     The default conventions can be found in the System.Data.Entity.ModelConfiguration.Conventions namespace.
    /// </summary>
    public partial class ConventionsConfiguration
    {
        private readonly List<IConvention> _conventions = new List<IConvention>();

        internal ConventionsConfiguration()
            : this(V2ConventionSet.Conventions)
        {
        }

        internal ConventionsConfiguration(IEnumerable<IConvention> conventionSet)
        {
            DebugCheck.NotNull(conventionSet);
            Debug.Assert(conventionSet.All(c => c != null));

            _conventions.AddRange(conventionSet);
        }

        private ConventionsConfiguration(ConventionsConfiguration source)
        {
            DebugCheck.NotNull(source);

            _conventions.AddRange(source._conventions);
        }

        internal virtual ConventionsConfiguration Clone()
        {
            return new ConventionsConfiguration(this);
        }

        /// <summary>
        ///     Enables one or more conventions for the <see cref="DbModelBuilder" />.
        /// </summary>
        /// <param name="conventions"> The conventions to be enabled. </param>
        public void Add(params IConvention[] conventions)
        {
            Check.NotNull(conventions, "conventions");
            Debug.Assert(conventions.All(c => c != null));

            conventions.Each(c => _conventions.Add(c));
        }

        /// <summary>
        ///     Enables a convention for the <see cref="DbModelBuilder" />.
        /// </summary>
        /// <typeparam name="TConvention"> The type of the convention to be enabled. </typeparam>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public void Add<TConvention>()
            where TConvention : IConvention, new()
        {
            Add(new TConvention());
        }

        /// <summary>
        ///     Enables a convention for the <see cref="DbModelBuilder" />. This convention
        ///     will run after the one specified.
        /// </summary>
        /// <typeparam name="TExistingConvention"> The type of the convention after which the enabled one will run. </typeparam>
        /// <param name="newConvention"> The convention to enable. </param>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public void AddAfter<TExistingConvention>(IConvention newConvention)
            where TExistingConvention : IConvention
        {
            Check.NotNull(newConvention, "newConvention");

            var index = IndexOf<TExistingConvention>();

            if (index < 0)
            {
                throw Error.ConventionNotFound(newConvention.GetType(), typeof(TExistingConvention));
            }

            _conventions.Insert(index + 1, newConvention);
        }

        /// <summary>
        ///     Enables a convention for the <see cref="DbModelBuilder" />. This convention
        ///     will run before the one specified.
        /// </summary>
        /// <typeparam name="TExistingConvention"> The type of the convention before which the enabled one will run. </typeparam>
        /// <param name="newConvention"> The convention to enable. </param>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public void AddBefore<TExistingConvention>(IConvention newConvention)
            where TExistingConvention : IConvention
        {
            Check.NotNull(newConvention, "newConvention");

            var index = IndexOf<TExistingConvention>();

            if (index < 0)
            {
                throw Error.ConventionNotFound(newConvention.GetType(), typeof(TExistingConvention));
            }

            _conventions.Insert(index, newConvention);
        }

        private int IndexOf<TConvention>()
        {
            var index = 0;

            foreach (var c in _conventions)
            {
                if (c.GetType()
                    == typeof(TConvention))
                {
                    return index;
                }

                index++;
            }

            return -1;
        }

        /// <summary>
        ///     Disables one or more conventions for the <see cref="DbModelBuilder" />.
        /// </summary>
        /// <param name="conventions"> The conventions to be disabled. </param>
        public void Remove(params IConvention[] conventions)
        {
            Check.NotNull(conventions, "conventions");

            conventions.Each(c => _conventions.Remove(c));
        }

        /// <summary>
        ///     Disables a convention for the <see cref="DbModelBuilder" />.
        ///     The default conventions that are available for removal can be found in the System.Data.Entity.ModelConfiguration.Conventions namespace.
        /// </summary>
        /// <typeparam name="TConvention"> The type of the convention to be disabled. </typeparam>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public void Remove<TConvention>()
            where TConvention : IConvention
        {
            _conventions.RemoveAll(c => c.GetType() == typeof(TConvention));
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        internal IEnumerable<IConvention> Conventions
        {
            get { return _conventions; }
        }

        internal void ApplyModel(EdmModel model)
        {
            DebugCheck.NotNull(model);

            foreach (var convention in _conventions)
            {
                new EdmConventionDispatcher(convention, model).Dispatch();
            }
        }

        internal void ApplyDatabase(EdmModel database)
        {
            foreach (var convention in _conventions)
            {
                new EdmConventionDispatcher(convention, database, DataSpace.SSpace).Dispatch();
            }
        }

        internal void ApplyMapping(DbDatabaseMapping databaseMapping)
        {
            DebugCheck.NotNull(databaseMapping);

            foreach (var convention in _conventions)
            {
                var mappingConvention = convention as IDbMappingConvention;

                if (mappingConvention != null)
                {
                    mappingConvention.Apply(databaseMapping);
                }
            }
        }

        internal void ApplyModelConfiguration(ModelConfiguration modelConfiguration)
        {
            DebugCheck.NotNull(modelConfiguration);

            foreach (var convention in _conventions.OfType<IConfigurationConvention>())
            {
                convention.Apply(modelConfiguration);
            }
        }

        internal void ApplyModelConfiguration(Type type, ModelConfiguration modelConfiguration)
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotNull(modelConfiguration);

            foreach (var convention in _conventions.OfType<IConfigurationConvention<Type, ModelConfiguration>>())
            {
                convention.Apply(type, () => modelConfiguration);
            }
        }

        internal void ApplyTypeConfiguration<TStructuralTypeConfiguration>(
            Type type, Func<TStructuralTypeConfiguration> structuralTypeConfiguration)
            where TStructuralTypeConfiguration : StructuralTypeConfiguration
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotNull(structuralTypeConfiguration);

            foreach (var convention in _conventions)
            {
                var propertyTypeConfigurationConvention
                    = convention as IConfigurationConvention<Type, TStructuralTypeConfiguration>;

                if (propertyTypeConfigurationConvention != null)
                {
                    propertyTypeConfigurationConvention.Apply(type, structuralTypeConfiguration);
                }

                var structuralTypeConfigurationConvention
                    = convention as IConfigurationConvention<Type, StructuralTypeConfiguration>;

                if (structuralTypeConfigurationConvention != null)
                {
                    structuralTypeConfigurationConvention.Apply(type, structuralTypeConfiguration);
                }
            }
        }

        internal void ApplyPropertyConfiguration(PropertyInfo propertyInfo, ModelConfiguration modelConfiguration)
        {
            DebugCheck.NotNull(propertyInfo);
            DebugCheck.NotNull(modelConfiguration);

            foreach (var convention in _conventions.OfType<IConfigurationConvention<PropertyInfo, ModelConfiguration>>()
                )
            {
                convention.Apply(propertyInfo, () => modelConfiguration);
            }
        }

        internal void ApplyPropertyConfiguration(
            PropertyInfo propertyInfo, Func<PropertyConfiguration> propertyConfiguration)
        {
            DebugCheck.NotNull(propertyInfo);
            DebugCheck.NotNull(propertyConfiguration);

            var propertyConfigurationType
                = StructuralTypeConfiguration.GetPropertyConfigurationType(propertyInfo.PropertyType);

            foreach (var convention in _conventions)
            {
                new PropertyConfigurationConventionDispatcher(
                    convention, propertyConfigurationType, propertyInfo, propertyConfiguration)
                    .Dispatch();
            }
        }

        internal void ApplyPropertyTypeConfiguration<TStructuralTypeConfiguration>(
            PropertyInfo propertyInfo, Func<TStructuralTypeConfiguration> structuralTypeConfiguration)
            where TStructuralTypeConfiguration : StructuralTypeConfiguration
        {
            DebugCheck.NotNull(propertyInfo);
            DebugCheck.NotNull(structuralTypeConfiguration);

            foreach (var convention in _conventions)
            {
                var propertyTypeConfigurationConvention
                    = convention as IConfigurationConvention<PropertyInfo, TStructuralTypeConfiguration>;

                if (propertyTypeConfigurationConvention != null)
                {
                    propertyTypeConfigurationConvention.Apply(propertyInfo, structuralTypeConfiguration);
                }

                var structuralTypeConfigurationConvention
                    = convention as IConfigurationConvention<PropertyInfo, StructuralTypeConfiguration>;

                if (structuralTypeConfigurationConvention != null)
                {
                    structuralTypeConfigurationConvention.Apply(propertyInfo, structuralTypeConfiguration);
                }
            }
        }

        internal void ApplyPluralizingTableNameConvention(EdmModel database)
        {
            if (database == null)
            {
                throw new ArgumentNullException("database");
            }

            foreach (var convention in _conventions.Where(c => c is PluralizingTableNameConvention))
            {
                new EdmConventionDispatcher(convention, database, DataSpace.SSpace).Dispatch();
            }
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
