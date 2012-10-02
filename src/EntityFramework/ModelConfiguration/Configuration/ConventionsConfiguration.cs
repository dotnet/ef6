// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Data.Entity.ModelConfiguration.Conventions.Sets;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    ///     Allows the conventions used by a <see cref="DbModelBuilder" /> instance to be customized.
    ///     Currently removal of one or more default conventions is the only supported operation.
    ///     The default conventions can be found in the System.Data.Entity.ModelConfiguration.Conventions namespace.
    /// </summary>
    public partial class ConventionsConfiguration : ConfigurationBase, IEnumerable<IConvention>
    {
        private readonly List<IConvention> _conventions = new List<IConvention>();

        internal ConventionsConfiguration()
            : this(V2ConventionSet.Conventions)
        {
        }

        internal ConventionsConfiguration(IEnumerable<IConvention> conventionSet)
        {
            Contract.Requires(conventionSet != null);
            Contract.Assert(conventionSet.All(c => c != null));

            _conventions.AddRange(conventionSet);
        }

        private ConventionsConfiguration(ConventionsConfiguration source)
        {
            Contract.Requires(source != null);

            _conventions.AddRange(source._conventions);
        }

        internal virtual ConventionsConfiguration Clone()
        {
            return new ConventionsConfiguration(this);
        }

        public void Add(params IConvention[] conventions)
        {
            Contract.Requires(conventions != null);
            Contract.Assert(conventions.All(c => c != null));

            conventions.Each(c => _conventions.Add(c));
        }

        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public void Add<TConvention>()
            where TConvention : IConvention, new()
        {
            Add(new TConvention());
        }

        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public void AddAfter<TExistingConvention>(IConvention newConvention)
            where TExistingConvention : IConvention
        {
            Contract.Requires(newConvention != null);

            var index = IndexOf<TExistingConvention>();

            if (index < 0)
            {
                throw Error.ConventionNotFound(newConvention.GetType(), typeof(TExistingConvention));
            }

            _conventions.Insert(index + 1, newConvention);
        }

        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public void AddBefore<TExistingConvention>(IConvention newConvention)
            where TExistingConvention : IConvention
        {
            Contract.Requires(newConvention != null);

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

        public void Remove(params IConvention[] conventions)
        {
            Contract.Requires(conventions != null);

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
            Contract.Requires(model != null);

            foreach (var convention in _conventions)
            {
                new EdmConventionDispatcher(convention, model).Dispatch();
            }
        }

        internal void ApplyDatabase(DbDatabaseMetadata database)
        {
            Contract.Requires(database != null);

            foreach (var convention in _conventions)
            {
                new DatabaseConventionDispatcher(convention, database).Dispatch();
            }
        }

        public void ApplyMapping(DbDatabaseMapping databaseMapping)
        {
            Contract.Requires(databaseMapping != null);

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
            Contract.Requires(modelConfiguration != null);

            foreach (var convention in _conventions.OfType<IConfigurationConvention>())
            {
                convention.Apply(modelConfiguration);
            }
        }

        internal void ApplyModelConfiguration(Type type, ModelConfiguration modelConfiguration)
        {
            Contract.Requires(type != null);
            Contract.Requires(modelConfiguration != null);

            foreach (var convention in _conventions.OfType<IConfigurationConvention<Type, ModelConfiguration>>())
            {
                convention.Apply(type, () => modelConfiguration);
            }
        }

        internal void ApplyTypeConfiguration<TStructuralTypeConfiguration>(
            Type type, Func<TStructuralTypeConfiguration> structuralTypeConfiguration)
            where TStructuralTypeConfiguration : StructuralTypeConfiguration
        {
            Contract.Requires(type != null);
            Contract.Requires(structuralTypeConfiguration != null);

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
            Contract.Requires(propertyInfo != null);
            Contract.Requires(modelConfiguration != null);

            foreach (var convention in _conventions.OfType<IConfigurationConvention<PropertyInfo, ModelConfiguration>>()
                )
            {
                convention.Apply(propertyInfo, () => modelConfiguration);
            }
        }

        internal void ApplyPropertyConfiguration(
            PropertyInfo propertyInfo, Func<PropertyConfiguration> propertyConfiguration)
        {
            Contract.Requires(propertyInfo != null);
            Contract.Requires(propertyConfiguration != null);

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
            Contract.Requires(propertyInfo != null);
            Contract.Requires(structuralTypeConfiguration != null);

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

        internal void ApplyPluralizingTableNameConvention(DbDatabaseMetadata database)
        {
            if (database == null)
            {
                throw new ArgumentNullException("database");
            }

            foreach (var convention in _conventions.Where(c => c is PluralizingTableNameConvention))
            {
                new DatabaseConventionDispatcher(convention, database).Dispatch();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IEnumerator<IConvention> IEnumerable<IConvention>.GetEnumerator()
        {
            return _conventions.GetEnumerator();
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _conventions.GetEnumerator();
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
