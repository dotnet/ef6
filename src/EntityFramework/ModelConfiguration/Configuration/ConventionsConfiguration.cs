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
        private readonly List<IConvention> _configurationConventions = new List<IConvention>();
        private readonly List<IConvention> _conceptualModelConventions = new List<IConvention>();
        private readonly List<IConvention> _conceptualToStoreMappingConventions = new List<IConvention>();
        private readonly List<IConvention> _storeModelConventions = new List<IConvention>();

        internal ConventionsConfiguration()
            : this(V2ConventionSet.Conventions)
        {
        }

        internal ConventionsConfiguration(ConventionSet conventionSet)
        {
            DebugCheck.NotNull(conventionSet);
            Debug.Assert(conventionSet.ConfigurationConventions.All(c => c != null && !IsModelConvention(c.GetType())));
            Debug.Assert(conventionSet.ConceptualModelConventions.All(c => c != null && IsModelConvention(c.GetType())));
            Debug.Assert(conventionSet.ConceptualToStoreMappingConventions.All(c => c != null && !IsModelConvention(c.GetType())));
            Debug.Assert(conventionSet.StoreModelConventions.All(c => c != null && IsModelConvention(c.GetType())));

            _configurationConventions.AddRange(conventionSet.ConfigurationConventions);
            _conceptualModelConventions.AddRange(conventionSet.ConceptualModelConventions);
            _conceptualToStoreMappingConventions.AddRange(conventionSet.ConceptualToStoreMappingConventions);
            _storeModelConventions.AddRange(conventionSet.StoreModelConventions);
        }

        private ConventionsConfiguration(ConventionsConfiguration source)
        {
            DebugCheck.NotNull(source);

            _configurationConventions.AddRange(source._configurationConventions);
            _conceptualModelConventions.AddRange(source._conceptualModelConventions);
            _conceptualToStoreMappingConventions.AddRange(source._conceptualToStoreMappingConventions);
            _storeModelConventions.AddRange(source._storeModelConventions);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        internal IEnumerable<IConvention> ConfigurationConventions
        {
            get { return _configurationConventions; }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        internal IEnumerable<IConvention> ConceptualModelConventions
        {
            get { return _conceptualModelConventions; }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        internal IEnumerable<IConvention> ConceptualToStoreMappingConventions
        {
            get { return _conceptualToStoreMappingConventions; }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        internal IEnumerable<IConvention> StoreModelConventions
        {
            get { return _storeModelConventions; }
        }

        internal virtual ConventionsConfiguration Clone()
        {
            return new ConventionsConfiguration(this);
        }

        /// <summary>
        ///     Enables one or more configuration conventions for the <see cref="DbModelBuilder" />.
        /// </summary>
        /// <param name="conventions"> The conventions to be enabled. </param>
        public void Add(params IConvention[] conventions)
        {
            Check.NotNull(conventions, "conventions");
            Debug.Assert(conventions.All(c => c != null));
            var modelConvention = conventions.FirstOrDefault(
                c => !IsConfigurationConvention(c.GetType()));
            if (modelConvention != null)
            {
                throw new InvalidOperationException(Strings.ConventionsConfiguration_NotConfigurationConvention(modelConvention.GetType()));
            }

            conventions.Each(c => _configurationConventions.Add(c));
        }

        /// <summary>
        ///     Enables one or more model conventions for the <see cref="DbModelBuilder" />.
        /// </summary>
        /// <param name="dataSpace"> The data space that the convention affects. </param>
        /// <param name="conventions"> The conventions to be enabled. </param>
        public void Add(DataSpace dataSpace, params IConvention[] conventions)
        {
            Check.NotNull(conventions, "conventions");
            Debug.Assert(conventions.All(c => c != null));
            var configurationConvention = conventions.FirstOrDefault(
                c => !IsModelConvention(c.GetType()));
            if (configurationConvention != null)
            {
                throw new InvalidOperationException(Strings.ConventionsConfiguration_NotModelConvention(configurationConvention.GetType()));
            }

            if (dataSpace == DataSpace.CSpace)
            {
                conventions.Each(c => _conceptualModelConventions.Add(c));
            }
            else if (dataSpace == DataSpace.SSpace)
            {
                conventions.Each(c => _storeModelConventions.Add(c));
            }
            else
            {
                throw new InvalidOperationException(Strings.ConventionsConfiguration_InvalidDataSpace(dataSpace));
            }
        }

        /// <summary>
        ///     Enables a convention for the <see cref="DbModelBuilder" />.
        /// </summary>
        /// <typeparam name="TConvention"> The type of the convention to be enabled. </typeparam>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public void Add<TConvention>()
            where TConvention : IConvention, new()
        {
            if (!IsConfigurationConvention(typeof(TConvention)))
            {
                throw new InvalidOperationException(Strings.ConventionsConfiguration_NotConfigurationConvention(typeof(TConvention)));
            }

            Add(new TConvention());
        }

        /// <summary>
        ///     Enables a configuration convention for the <see cref="DbModelBuilder" />.
        /// </summary>
        /// <param name="dataSpace"> The data space that the convention affects. </param>
        /// <typeparam name="TConvention"> The type of the convention to be enabled. </typeparam>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public void Add<TConvention>(DataSpace dataSpace)
            where TConvention : IConvention, new()
        {
            if (!IsModelConvention(typeof(TConvention)))
            {
                throw new InvalidOperationException(Strings.ConventionsConfiguration_NotModelConvention(typeof(TConvention)));
            }

            Add(dataSpace, new TConvention());
        }

        /// <summary>
        ///     Enables a configuration convention for the <see cref="DbModelBuilder" />. This convention
        ///     will run after the one specified.
        /// </summary>
        /// <typeparam name="TExistingConvention"> The type of the convention after which the enabled one will run. </typeparam>
        /// <param name="newConvention"> The convention to enable. </param>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public void AddAfter<TExistingConvention>(IConvention newConvention)
            where TExistingConvention : IConvention
        {
            Check.NotNull(newConvention, "newConvention");
            if (!IsConfigurationConvention(newConvention.GetType()))
            {
                throw new InvalidOperationException(Strings.ConventionsConfiguration_NotConfigurationConvention(newConvention.GetType()));
            }
            if (!IsConfigurationConvention(typeof(TExistingConvention)))
            {
                throw new InvalidOperationException(
                    Strings.ConventionsConfiguration_NotConfigurationConvention(typeof(TExistingConvention)));
            }

            var index = IndexOf<TExistingConvention>();

            if (index < 0)
            {
                throw Error.ConventionNotFound(newConvention.GetType(), typeof(TExistingConvention));
            }

            _configurationConventions.Insert(index + 1, newConvention);
        }

        /// <summary>
        ///     Enables a model convention for the <see cref="DbModelBuilder" />. This convention
        ///     will run after the one specified.
        /// </summary>
        /// <param name="dataSpace"> The data space that the convention affects. </param>
        /// <typeparam name="TExistingConvention"> The type of the convention after which the enabled one will run. </typeparam>
        /// <param name="newConvention"> The convention to enable. </param>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public void AddAfter<TExistingConvention>(DataSpace dataSpace, IConvention newConvention)
            where TExistingConvention : IConvention
        {
            Check.NotNull(newConvention, "newConvention");
            if (!IsModelConvention(newConvention.GetType()))
            {
                throw new InvalidOperationException(Strings.ConventionsConfiguration_NotModelConvention(newConvention.GetType()));
            }
            if (!IsModelConvention(typeof(TExistingConvention)))
            {
                throw new InvalidOperationException(Strings.ConventionsConfiguration_NotModelConvention(typeof(TExistingConvention)));
            }

            var index = IndexOf<TExistingConvention>(dataSpace);

            if (index < 0)
            {
                throw Error.ConventionNotFound(newConvention.GetType(), typeof(TExistingConvention));
            }

            if (dataSpace == DataSpace.CSpace)
            {
                _conceptualModelConventions.Insert(index + 1, newConvention);
            }
            else if (dataSpace == DataSpace.SSpace)
            {
                _storeModelConventions.Insert(index + 1, newConvention);
            }
        }

        /// <summary>
        ///     Enables a configuration convention for the <see cref="DbModelBuilder" />. This convention
        ///     will run before the one specified.
        /// </summary>
        /// <typeparam name="TExistingConvention"> The type of the convention before which the enabled one will run. </typeparam>
        /// <param name="newConvention"> The convention to enable. </param>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public void AddBefore<TExistingConvention>(IConvention newConvention)
            where TExistingConvention : IConvention
        {
            Check.NotNull(newConvention, "newConvention");
            if (!IsConfigurationConvention(newConvention.GetType()))
            {
                throw new InvalidOperationException(Strings.ConventionsConfiguration_NotConfigurationConvention(newConvention.GetType()));
            }
            if (!IsConfigurationConvention(typeof(TExistingConvention)))
            {
                throw new InvalidOperationException(
                    Strings.ConventionsConfiguration_NotConfigurationConvention(typeof(TExistingConvention)));
            }

            var index = IndexOf<TExistingConvention>();

            if (index < 0)
            {
                throw Error.ConventionNotFound(newConvention.GetType(), typeof(TExistingConvention));
            }

            _configurationConventions.Insert(index, newConvention);
        }

        /// <summary>
        ///     Enables a model convention for the <see cref="DbModelBuilder" />. This convention
        ///     will run before the one specified.
        /// </summary>
        /// <typeparam name="TExistingConvention"> The type of the convention before which the enabled one will run. </typeparam>
        /// <param name="newConvention"> The convention to enable. </param>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public void AddBefore<TExistingConvention>(DataSpace dataSpace, IConvention newConvention)
            where TExistingConvention : IConvention
        {
            Check.NotNull(newConvention, "newConvention");
            if (!IsModelConvention(newConvention.GetType()))
            {
                throw new InvalidOperationException(Strings.ConventionsConfiguration_NotModelConvention(newConvention.GetType()));
            }
            if (!IsModelConvention(typeof(TExistingConvention)))
            {
                throw new InvalidOperationException(Strings.ConventionsConfiguration_NotModelConvention(typeof(TExistingConvention)));
            }

            var index = IndexOf<TExistingConvention>(dataSpace);

            if (index < 0)
            {
                throw Error.ConventionNotFound(newConvention.GetType(), typeof(TExistingConvention));
            }

            if (dataSpace == DataSpace.CSpace)
            {
                _conceptualModelConventions.Insert(index, newConvention);
            }
            else if (dataSpace == DataSpace.SSpace)
            {
                _storeModelConventions.Insert(index, newConvention);
            }
        }

        private int IndexOf<TConvention>()
        {
            var index = 0;

            foreach (var c in _configurationConventions)
            {
                if (c.GetType() == typeof(TConvention))
                {
                    return index;
                }

                index++;
            }

            return -1;
        }

        private int IndexOf<TConvention>(DataSpace dataSpace)
        {
            List<IConvention> conventions;
            if (dataSpace == DataSpace.CSpace)
            {
                conventions = _conceptualModelConventions;
            }
            else if (dataSpace == DataSpace.SSpace)
            {
                conventions = _storeModelConventions;
            }
            else
            {
                throw new InvalidOperationException(Strings.ConventionsConfiguration_InvalidDataSpace(dataSpace));
            }

            var index = 0;
            foreach (var c in conventions)
            {
                if (c.GetType() == typeof(TConvention))
                {
                    return index;
                }

                index++;
            }

            return -1;
        }

        /// <summary>
        ///     Disables one or more configuration conventions for the <see cref="DbModelBuilder" />.
        /// </summary>
        /// <param name="conventions"> The conventions to be disabled. </param>
        public void Remove(params IConvention[] conventions)
        {
            Check.NotNull(conventions, "conventions");
            var modelConvention = conventions.FirstOrDefault(
                c => IsModelConvention(c.GetType()));
            if (modelConvention != null)
            {
                throw new InvalidOperationException(Strings.ConventionsConfiguration_NotConfigurationConvention(modelConvention.GetType()));
            }

            conventions.Each(c => _configurationConventions.Remove(c));
        }

        /// <summary>
        ///     Disables one or more model conventions for the <see cref="DbModelBuilder" />.
        /// </summary>
        /// <param name="dataSpace"> The data space that the convention affects. </param>
        /// <param name="conventions"> The conventions to be disabled. </param>
        public void Remove(DataSpace dataSpace, params IConvention[] conventions)
        {
            Check.NotNull(conventions, "conventions");
            var configurationConvention = conventions.FirstOrDefault(
                c => !IsModelConvention(c.GetType()));
            if (configurationConvention != null)
            {
                throw new InvalidOperationException(Strings.ConventionsConfiguration_NotModelConvention(configurationConvention.GetType()));
            }

            if (dataSpace == DataSpace.CSpace)
            {
                conventions.Each(c => _conceptualModelConventions.Remove(c));
            }
            else if (dataSpace == DataSpace.SSpace)
            {
                conventions.Each(c => _storeModelConventions.Remove(c));
            }
            else
            {
                throw new InvalidOperationException(Strings.ConventionsConfiguration_InvalidDataSpace(dataSpace));
            }
        }

        /// <summary>
        ///     Disables a configuration convention for the <see cref="DbModelBuilder" />.
        ///     The default conventions that are available for removal can be found in the System.Data.Entity.ModelConfiguration.Conventions namespace.
        /// </summary>
        /// <typeparam name="TConvention"> The type of the convention to be disabled. </typeparam>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public void Remove<TConvention>()
            where TConvention : IConvention
        {
            if (!IsConfigurationConvention(typeof(TConvention)))
            {
                throw new InvalidOperationException(Strings.ConventionsConfiguration_NotConfigurationConvention(typeof(TConvention)));
            }

            _configurationConventions.RemoveAll(c => c.GetType() == typeof(TConvention));
        }

        /// <summary>
        ///     Disables a model convention for the <see cref="DbModelBuilder" />.
        ///     The default conventions that are available for removal can be found in the System.Data.Entity.ModelConfiguration.Conventions namespace.
        /// </summary>
        /// <param name="dataSpace"> The data space that the convention affects. </param>
        /// <typeparam name="TConvention"> The type of the convention to be disabled. </typeparam>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public void Remove<TConvention>(DataSpace dataSpace)
            where TConvention : IConvention
        {
            if (!IsModelConvention(typeof(TConvention)))
            {
                throw new InvalidOperationException(Strings.ConventionsConfiguration_NotModelConvention(typeof(TConvention)));
            }

            if (dataSpace == DataSpace.CSpace)
            {
                _conceptualModelConventions.RemoveAll(c => c.GetType() == typeof(TConvention));
            }
            else if (dataSpace == DataSpace.SSpace)
            {
                _storeModelConventions.RemoveAll(c => c.GetType() == typeof(TConvention));
            }
            else
            {
                throw new InvalidOperationException(Strings.ConventionsConfiguration_InvalidDataSpace(dataSpace));
            }
        }

        internal void ApplyModel(EdmModel model)
        {
            DebugCheck.NotNull(model);

            foreach (var convention in _conceptualModelConventions)
            {
                new ModelConventionDispatcher(convention, model).Dispatch();
            }
        }

        internal void ApplyDatabase(EdmModel database)
        {
            foreach (var convention in _storeModelConventions)
            {
                new ModelConventionDispatcher(convention, database).Dispatch();
            }
        }

        internal void ApplyPluralizingTableNameConvention(EdmModel database)
        {
            DebugCheck.NotNull(database);

            foreach (var convention in _storeModelConventions.Where(c => c is PluralizingTableNameConvention))
            {
                new ModelConventionDispatcher(convention, database).Dispatch();
            }
        }

        internal void ApplyMapping(DbDatabaseMapping databaseMapping)
        {
            DebugCheck.NotNull(databaseMapping);

            foreach (var convention in _conceptualToStoreMappingConventions)
            {
                var mappingConvention = convention as IDbMappingConvention;

                if (mappingConvention != null)
                {
                    mappingConvention.Apply(databaseMapping);
                }
            }
        }

        internal virtual void ApplyModelConfiguration(ModelConfiguration modelConfiguration)
        {
            DebugCheck.NotNull(modelConfiguration);

            foreach (var convention in _configurationConventions)
            {
                var configurationConvention
                    = convention as IConfigurationConvention;

                if (configurationConvention != null)
                {
                    configurationConvention.Apply(modelConfiguration);
                }

                var lightweightConfigurationConvention
                    = convention as Convention;

                if (lightweightConfigurationConvention != null)
                {
                    lightweightConfigurationConvention.ApplyModelConfiguration(modelConfiguration);
                }
            }
        }

        internal virtual void ApplyModelConfiguration(Type type, ModelConfiguration modelConfiguration)
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotNull(modelConfiguration);

            foreach (var convention in _configurationConventions)
            {
                var modelConfigurationConvention
                    = convention as IConfigurationConvention<Type>;

                if (modelConfigurationConvention != null)
                {
                    modelConfigurationConvention.Apply(type, modelConfiguration);
                }

                var lightweightConfigurationConvention
                    = convention as Convention;

                if (lightweightConfigurationConvention != null)
                {
                    lightweightConfigurationConvention.ApplyModelConfiguration(type, modelConfiguration);
                }
            }
        }

        internal virtual void ApplyTypeConfiguration<TStructuralTypeConfiguration>(
            Type type,
            Func<TStructuralTypeConfiguration> structuralTypeConfiguration,
            ModelConfiguration modelConfiguration)
            where TStructuralTypeConfiguration : StructuralTypeConfiguration
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotNull(structuralTypeConfiguration);

            foreach (var convention in _configurationConventions)
            {
                var propertyTypeConfigurationConvention
                    = convention as IConfigurationConvention<Type, TStructuralTypeConfiguration>;

                if (propertyTypeConfigurationConvention != null)
                {
                    propertyTypeConfigurationConvention.Apply(type, structuralTypeConfiguration, modelConfiguration);
                }

                var structuralTypeConfigurationConvention
                    = convention as IConfigurationConvention<Type, StructuralTypeConfiguration>;

                if (structuralTypeConfigurationConvention != null)
                {
                    structuralTypeConfigurationConvention.Apply(type, structuralTypeConfiguration, modelConfiguration);
                }

                var lightweightConfigurationConvention
                    = convention as Convention;

                if (lightweightConfigurationConvention != null)
                {
                    lightweightConfigurationConvention.ApplyTypeConfiguration(type, structuralTypeConfiguration, modelConfiguration);
                }
            }
        }

        internal virtual void ApplyPropertyConfiguration(PropertyInfo propertyInfo, ModelConfiguration modelConfiguration)
        {
            DebugCheck.NotNull(propertyInfo);
            DebugCheck.NotNull(modelConfiguration);

            foreach (var convention in _configurationConventions)
            {
                var propertyConfigurationConvention
                    = convention as IConfigurationConvention<PropertyInfo>;

                if (propertyConfigurationConvention != null)
                {
                    propertyConfigurationConvention.Apply(propertyInfo, modelConfiguration);
                }

                var lightweightConfigurationConvention
                    = convention as Convention;

                if (lightweightConfigurationConvention != null)
                {
                    lightweightConfigurationConvention.ApplyPropertyConfiguration(propertyInfo, modelConfiguration);
                }
            }
        }

        internal virtual void ApplyPropertyConfiguration(
            PropertyInfo propertyInfo, Func<PropertyConfiguration> propertyConfiguration, ModelConfiguration modelConfiguration)
        {
            DebugCheck.NotNull(propertyInfo);
            DebugCheck.NotNull(propertyConfiguration);

            var propertyConfigurationType
                = StructuralTypeConfiguration.GetPropertyConfigurationType(propertyInfo.PropertyType);

            foreach (var convention in _configurationConventions)
            {
                new PropertyConfigurationConventionDispatcher(
                    convention, propertyConfigurationType, propertyInfo, propertyConfiguration, modelConfiguration)
                    .Dispatch();

                var lightweightConfigurationConvention
                    = convention as Convention;

                if (lightweightConfigurationConvention != null)
                {
                    lightweightConfigurationConvention.ApplyPropertyConfiguration(propertyInfo, propertyConfiguration, modelConfiguration);
                }
            }
        }

        internal virtual void ApplyPropertyTypeConfiguration<TStructuralTypeConfiguration>(
            PropertyInfo propertyInfo,
            Func<TStructuralTypeConfiguration> structuralTypeConfiguration,
            ModelConfiguration modelConfiguration)
            where TStructuralTypeConfiguration : StructuralTypeConfiguration
        {
            DebugCheck.NotNull(propertyInfo);
            DebugCheck.NotNull(structuralTypeConfiguration);

            foreach (var convention in _configurationConventions)
            {
                var propertyTypeConfigurationConvention
                    = convention as IConfigurationConvention<PropertyInfo, TStructuralTypeConfiguration>;

                if (propertyTypeConfigurationConvention != null)
                {
                    propertyTypeConfigurationConvention.Apply(propertyInfo, structuralTypeConfiguration, modelConfiguration);
                }

                var structuralTypeConfigurationConvention
                    = convention as IConfigurationConvention<PropertyInfo, StructuralTypeConfiguration>;

                if (structuralTypeConfigurationConvention != null)
                {
                    structuralTypeConfigurationConvention.Apply(propertyInfo, structuralTypeConfiguration, modelConfiguration);
                }

                var lightweightConfigurationConvention
                    = convention as Convention;

                if (lightweightConfigurationConvention != null)
                {
                    lightweightConfigurationConvention.ApplyPropertyTypeConfiguration(
                        propertyInfo, structuralTypeConfiguration, modelConfiguration);
                }
            }
        }

        private static bool IsConfigurationConvention(Type conventionType)
        {
            return typeof(IConfigurationConvention).IsAssignableFrom(conventionType)
                   || typeof(Convention).IsAssignableFrom(conventionType)
                   || conventionType.GetGenericTypeImplementations(typeof(IConfigurationConvention<>)).Any()
                   || conventionType.GetGenericTypeImplementations(typeof(IConfigurationConvention<,>)).Any();
        }

        private static bool IsModelConvention(Type conventionType)
        {
            return typeof(IModelConvention).IsAssignableFrom(conventionType)
                   || conventionType.GetGenericTypeImplementations(
                       typeof(IModelConvention<>)).Any();
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
