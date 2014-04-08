// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
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
        private readonly ConventionSet _initialConventionSet;

        internal ConventionsConfiguration()
            : this(V2ConventionSet.Conventions)
        {
        }

        internal ConventionsConfiguration(ConventionSet conventionSet)
        {
            DebugCheck.NotNull(conventionSet);
            Debug.Assert(
                conventionSet.ConfigurationConventions.All(c => c != null && ConventionsTypeFilter.IsConfigurationConvention(c.GetType())));
            Debug.Assert(
                conventionSet.ConceptualModelConventions.All(
                    c => c != null && ConventionsTypeFilter.IsConceptualModelConvention(c.GetType())));
            Debug.Assert(
                conventionSet.ConceptualToStoreMappingConventions.All(
                    c => c != null && ConventionsTypeFilter.IsConceptualToStoreMappingConvention(c.GetType())));
            Debug.Assert(
                conventionSet.StoreModelConventions.All(c => c != null && ConventionsTypeFilter.IsStoreModelConvention(c.GetType())));

            _configurationConventions.AddRange(conventionSet.ConfigurationConventions);
            _conceptualModelConventions.AddRange(conventionSet.ConceptualModelConventions);
            _conceptualToStoreMappingConventions.AddRange(conventionSet.ConceptualToStoreMappingConventions);
            _storeModelConventions.AddRange(conventionSet.StoreModelConventions);
            _initialConventionSet = conventionSet;
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
        ///     Discover all conventions in the given assembly and add them to the <see cref="DbModelBuilder" />.
        /// </summary>
        /// <remarks>
        ///     This method add all conventions ordered by type name. The order in which conventions are added
        ///     can have an impact on how they behave because it governs the order in which they are run.
        ///     All conventions found must have a parameterless public constructor.
        /// </remarks>
        /// <param name="assembly">The assembly containing conventions to be added.</param>
        public void AddFromAssembly(Assembly assembly)
        {
            Check.NotNull(assembly, "assembly");

            var types = assembly.GetAccessibleTypes()
                .OrderBy(type => type.Name);

            new ConventionsTypeFinder().AddConventions(types, convention => Add(convention));
        }

        /// <summary>
        ///     Enables one or more conventions for the <see cref="DbModelBuilder" />.
        /// </summary>
        /// <param name="conventions"> The conventions to be enabled. </param>
        public void Add(params IConvention[] conventions)
        {
            Check.NotNull(conventions, "conventions");
            Debug.Assert(conventions.All(c => c != null));

            foreach (var c in conventions)
            {
                var invalidType = true;

                if (ConventionsTypeFilter.IsConfigurationConvention(c.GetType()))
                {
                    invalidType = false;
                    var existingConventionIndex = _configurationConventions.FindIndex(
                        initialConvention => _initialConventionSet.ConfigurationConventions.Contains(initialConvention));
                    existingConventionIndex = existingConventionIndex == -1
                        ? _configurationConventions.Count
                        : existingConventionIndex;
                    _configurationConventions.Insert(existingConventionIndex, c);
                }

                if (ConventionsTypeFilter.IsConceptualModelConvention(c.GetType()))
                {
                    invalidType = false;
                    _conceptualModelConventions.Add(c);
                }

                if (ConventionsTypeFilter.IsStoreModelConvention(c.GetType()))
                {
                    invalidType = false;
                    _storeModelConventions.Add(c);
                }

                if (ConventionsTypeFilter.IsConceptualToStoreMappingConvention(c.GetType()))
                {
                    invalidType = false;
                    _conceptualToStoreMappingConventions.Add(c);
                }

                if (invalidType)
                {
                    throw new InvalidOperationException(
                        Strings.ConventionsConfiguration_InvalidConventionType(c.GetType()));
                }
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

            var typeMissmatch = true;

            if (ConventionsTypeFilter.IsConfigurationConvention(newConvention.GetType())
                && ConventionsTypeFilter.IsConfigurationConvention(typeof(TExistingConvention)))
            {
                typeMissmatch = false;
                Insert(typeof(TExistingConvention), 1, newConvention, _configurationConventions);
            }

            if (ConventionsTypeFilter.IsConceptualModelConvention(newConvention.GetType())
                && ConventionsTypeFilter.IsConceptualModelConvention(typeof(TExistingConvention)))
            {
                typeMissmatch = false;
                Insert(typeof(TExistingConvention), 1, newConvention, _conceptualModelConventions);
            }

            if (ConventionsTypeFilter.IsStoreModelConvention(newConvention.GetType())
                && ConventionsTypeFilter.IsStoreModelConvention(typeof(TExistingConvention)))
            {
                typeMissmatch = false;
                Insert(typeof(TExistingConvention), 1, newConvention, _storeModelConventions);
            }

            if (ConventionsTypeFilter.IsConceptualToStoreMappingConvention(newConvention.GetType())
                && ConventionsTypeFilter.IsConceptualToStoreMappingConvention(typeof(TExistingConvention)))
            {
                typeMissmatch = false;
                Insert(typeof(TExistingConvention), 1, newConvention, _conceptualToStoreMappingConventions);
            }

            if (typeMissmatch)
            {
                throw new InvalidOperationException(
                    Strings.ConventionsConfiguration_ConventionTypeMissmatch(
                        newConvention.GetType(), typeof(TExistingConvention)));
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

            var typeMissmatch = true;

            if (ConventionsTypeFilter.IsConfigurationConvention(newConvention.GetType())
                && ConventionsTypeFilter.IsConfigurationConvention(typeof(TExistingConvention)))
            {
                typeMissmatch = false;
                Insert(typeof(TExistingConvention), 0, newConvention, _configurationConventions);
            }

            if (ConventionsTypeFilter.IsConceptualModelConvention(newConvention.GetType())
                && ConventionsTypeFilter.IsConceptualModelConvention(typeof(TExistingConvention)))
            {
                typeMissmatch = false;
                Insert(typeof(TExistingConvention), 0, newConvention, _conceptualModelConventions);
            }

            if (ConventionsTypeFilter.IsStoreModelConvention(newConvention.GetType())
                && ConventionsTypeFilter.IsStoreModelConvention(typeof(TExistingConvention)))
            {
                typeMissmatch = false;
                Insert(typeof(TExistingConvention), 0, newConvention, _storeModelConventions);
            }

            if (ConventionsTypeFilter.IsConceptualToStoreMappingConvention(newConvention.GetType())
                && ConventionsTypeFilter.IsConceptualToStoreMappingConvention(typeof(TExistingConvention)))
            {
                typeMissmatch = false;
                Insert(typeof(TExistingConvention), 0, newConvention, _conceptualToStoreMappingConventions);
            }

            if (typeMissmatch)
            {
                throw new InvalidOperationException(
                    Strings.ConventionsConfiguration_ConventionTypeMissmatch(
                        newConvention.GetType(), typeof(TExistingConvention)));
            }
        }

        private static void Insert(Type existingConventionType, int offset, IConvention newConvention, IList<IConvention> conventions)
        {
            var index = IndexOf(existingConventionType, conventions);

            if (index < 0)
            {
                throw Error.ConventionNotFound(newConvention.GetType(), existingConventionType);
            }

            conventions.Insert(index + offset, newConvention);
        }

        private static int IndexOf(Type existingConventionType, IList<IConvention> conventions)
        {
            var index = 0;

            foreach (var c in conventions)
            {
                if (c.GetType() == existingConventionType)
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

            Check.NotNull(conventions, "conventions");
            Debug.Assert(conventions.All(c => c != null));

            foreach (var c in conventions)
            {
                if (ConventionsTypeFilter.IsConfigurationConvention(c.GetType()))
                {
                    _configurationConventions.Remove(c);
                }

                if (ConventionsTypeFilter.IsConceptualModelConvention(c.GetType()))
                {
                    _conceptualModelConventions.Remove(c);
                }

                if (ConventionsTypeFilter.IsStoreModelConvention(c.GetType()))
                {
                    _storeModelConventions.Remove(c);
                }

                if (ConventionsTypeFilter.IsConceptualToStoreMappingConvention(c.GetType()))
                {
                    _conceptualToStoreMappingConventions.Remove(c);
                }
            }
        }

        /// <summary>
        ///     Disables a convention for the <see cref="DbModelBuilder" />.
        ///     The default conventions that are available for removal can be found in the
        ///     System.Data.Entity.ModelConfiguration.Conventions namespace.
        /// </summary>
        /// <typeparam name="TConvention"> The type of the convention to be disabled. </typeparam>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public void Remove<TConvention>()
            where TConvention : IConvention
        {
            if (ConventionsTypeFilter.IsConfigurationConvention(typeof(TConvention)))
            {
                _configurationConventions.RemoveAll(c => c.GetType() == typeof(TConvention));
            }

            if (ConventionsTypeFilter.IsConceptualModelConvention(typeof(TConvention)))
            {
                _conceptualModelConventions.RemoveAll(c => c.GetType() == typeof(TConvention));
            }

            if (ConventionsTypeFilter.IsStoreModelConvention(typeof(TConvention)))
            {
                _storeModelConventions.RemoveAll(c => c.GetType() == typeof(TConvention));
            }

            if (ConventionsTypeFilter.IsConceptualToStoreMappingConvention(typeof(TConvention)))
            {
                _conceptualToStoreMappingConventions.RemoveAll(c => c.GetType() == typeof(TConvention));
            }
        }

        internal void ApplyConceptualModel(DbModel model)
        {
            DebugCheck.NotNull(model);

            foreach (var convention in _conceptualModelConventions)
            {
                new ModelConventionDispatcher(convention, model, DataSpace.CSpace).Dispatch();
            }
        }

        internal void ApplyStoreModel(DbModel model)
        {
            foreach (var convention in _storeModelConventions)
            {
                new ModelConventionDispatcher(convention, model, DataSpace.SSpace).Dispatch();
            }
        }

        internal void ApplyPluralizingTableNameConvention(DbModel model)
        {
            DebugCheck.NotNull(model);

            foreach (var convention in _storeModelConventions.Where(c => c is PluralizingTableNameConvention))
            {
                new ModelConventionDispatcher(convention, model, DataSpace.SSpace).Dispatch();
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

            foreach (var convention in ((IList<IConvention>)_configurationConventions).Reverse())
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

            foreach (var convention in ((IList<IConvention>)_configurationConventions).Reverse())
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

            foreach (var convention in ((IList<IConvention>)_configurationConventions).Reverse())
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

            foreach (var convention in ((IList<IConvention>)_configurationConventions).Reverse())
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

            foreach (var convention in ((IList<IConvention>)_configurationConventions).Reverse())
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

            foreach (var convention in ((IList<IConvention>)_configurationConventions).Reverse())
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
        ///     Gets the <see cref="Type" /> of the current instance.
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
