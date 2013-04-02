// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Services;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    ///     Used to configure a navigation property.
    /// </summary>
    public class NavigationPropertyConfiguration : PropertyConfiguration
    {
        private readonly PropertyInfo _navigationProperty;
        private RelationshipMultiplicity? _endKind;
        private PropertyInfo _inverseNavigationProperty;
        private RelationshipMultiplicity? _inverseEndKind;
        private ConstraintConfiguration _constraint;
        private AssociationMappingConfiguration _associationMappingConfiguration;
        private ModificationFunctionsConfiguration _modificationFunctionsConfiguration;

        internal NavigationPropertyConfiguration(PropertyInfo navigationProperty)
        {
            DebugCheck.NotNull(navigationProperty);

            _navigationProperty = navigationProperty;
        }

        private NavigationPropertyConfiguration(NavigationPropertyConfiguration source)
        {
            DebugCheck.NotNull(source);

            _navigationProperty = source._navigationProperty;
            _endKind = source._endKind;
            _inverseNavigationProperty = source._inverseNavigationProperty;
            _inverseEndKind = source._inverseEndKind;

            _constraint = source._constraint == null
                              ? null
                              : source._constraint.Clone();

            _associationMappingConfiguration
                = source._associationMappingConfiguration == null
                      ? null
                      : source._associationMappingConfiguration.Clone();

            DeleteAction = source.DeleteAction;
            IsNavigationPropertyDeclaringTypePrincipal = source.IsNavigationPropertyDeclaringTypePrincipal;

            _modificationFunctionsConfiguration
                = source._modificationFunctionsConfiguration == null
                      ? null
                      : source._modificationFunctionsConfiguration.Clone();
        }

        internal virtual NavigationPropertyConfiguration Clone()
        {
            return new NavigationPropertyConfiguration(this);
        }

        /// <summary>
        ///     Gets or sets the action to take when a delete operation is attempted.
        /// </summary>
        public OperationAction? DeleteAction { get; set; }

        internal PropertyInfo NavigationProperty
        {
            get { return _navigationProperty; }
        }

        /// <summary>
        ///     Gets or sets the multiplicity of this end of the navigation property.
        /// </summary>
        public RelationshipMultiplicity? RelationshipMultiplicity
        {
            get { return _endKind; }
            set
            {
                Check.NotNull(value, "value");

                _endKind = value;
            }
        }

        internal PropertyInfo InverseNavigationProperty
        {
            get { return _inverseNavigationProperty; }
            set
            {
                DebugCheck.NotNull(value);

                if (value == _navigationProperty)
                {
                    throw Error.NavigationInverseItself(value.Name, value.ReflectedType);
                }

                _inverseNavigationProperty = value;
            }
        }

        internal RelationshipMultiplicity? InverseEndKind
        {
            get { return _inverseEndKind; }
            set
            {
                DebugCheck.NotNull(value);

                _inverseEndKind = value;
            }
        }

        /// <summary>
        ///     Gets or sets the constraint associated with the navigation property.
        /// </summary>
        /// <remarks>
        ///     This property uses <see cref="ForeignKeyConstraintConfiguration" /> for
        ///     foreign key constraints and <see cref="IndependentConstraintConfiguration" />
        ///     for independent constraints.
        /// </remarks>
        public ConstraintConfiguration Constraint
        {
            get { return _constraint; }
            set
            {
                Check.NotNull(value, "value");

                _constraint = value;
            }
        }

        /// <summary>
        ///     True if the NavigationProperty's declaring type is the principal end, false if it is not, null if it is not known
        /// </summary>
        internal bool? IsNavigationPropertyDeclaringTypePrincipal { get; set; }

        internal AssociationMappingConfiguration AssociationMappingConfiguration
        {
            get { return _associationMappingConfiguration; }
            set
            {
                DebugCheck.NotNull(value);

                _associationMappingConfiguration = value;
            }
        }

        internal ModificationFunctionsConfiguration ModificationFunctionsConfiguration
        {
            get { return _modificationFunctionsConfiguration; }
            set
            {
                DebugCheck.NotNull(value);

                _modificationFunctionsConfiguration = value;
            }
        }

        internal void Configure(
            NavigationProperty navigationProperty, EdmModel model, EntityTypeConfiguration entityTypeConfiguration)
        {
            DebugCheck.NotNull(navigationProperty);
            DebugCheck.NotNull(model);
            DebugCheck.NotNull(entityTypeConfiguration);

            navigationProperty.SetConfiguration(this);

            var associationType = navigationProperty.Association;
            var configuration = associationType.GetConfiguration() as NavigationPropertyConfiguration;

            if (configuration == null)
            {
                associationType.SetConfiguration(this);
            }
            else
            {
                ValidateConsistency(configuration);
            }

            ConfigureInverse(associationType, model);
            ConfigureEndKinds(associationType, configuration);
            ConfigureDependentBehavior(associationType, model, entityTypeConfiguration);
        }

        internal void Configure(
            StorageAssociationSetMapping associationSetMapping,
            DbDatabaseMapping databaseMapping,
            DbProviderManifest providerManifest)
        {
            DebugCheck.NotNull(associationSetMapping);
            DebugCheck.NotNull(databaseMapping);
            DebugCheck.NotNull(providerManifest);

            // We may apply configuration twice from two different NavigationPropertyConfiguration objects,
            // but that should be okay since they were validated as consistent above.
            // We still apply twice because each object may have different pieces of the full configuration.
            if (AssociationMappingConfiguration != null)
            {
                // This may replace a configuration previously set, but that's okay since we validated
                // consistency when processing the configuration above.
                associationSetMapping.SetConfiguration(this);

                AssociationMappingConfiguration
                    .Configure(associationSetMapping, databaseMapping.Database, _navigationProperty);
            }

            if (_modificationFunctionsConfiguration != null)
            {
                if (associationSetMapping.ModificationFunctionMapping == null)
                {
                    new ModificationFunctionMappingGenerator(providerManifest)
                        .Generate(associationSetMapping, databaseMapping);
                }

                _modificationFunctionsConfiguration.Configure(associationSetMapping.ModificationFunctionMapping);
            }
        }

        private void ConfigureInverse(AssociationType associationType, EdmModel model)
        {
            DebugCheck.NotNull(associationType);
            DebugCheck.NotNull(model);

            if (_inverseNavigationProperty == null)
            {
                return;
            }

            var inverseNavigationProperty
                = model.GetNavigationProperty(_inverseNavigationProperty);

            if ((inverseNavigationProperty != null)
                && (inverseNavigationProperty.Association != associationType))
            {
                associationType.SourceEnd.RelationshipMultiplicity
                    = inverseNavigationProperty.Association.TargetEnd.RelationshipMultiplicity;

                if ((associationType.Constraint == null)
                    && (_constraint == null)
                    && (inverseNavigationProperty.Association.Constraint != null))
                {
                    associationType.Constraint = inverseNavigationProperty.Association.Constraint;
                    associationType.Constraint.FromRole = associationType.SourceEnd;
                    associationType.Constraint.ToRole = associationType.TargetEnd;
                }

                model.RemoveAssociationType(inverseNavigationProperty.Association);

                inverseNavigationProperty.RelationshipType = associationType;
                inverseNavigationProperty.FromEndMember = associationType.TargetEnd;
                inverseNavigationProperty.ToEndMember = associationType.SourceEnd;
            }
        }

        private void ConfigureEndKinds(
            AssociationType associationType, NavigationPropertyConfiguration configuration)
        {
            DebugCheck.NotNull(associationType);

            var sourceEnd = associationType.SourceEnd;
            var targetEnd = associationType.TargetEnd;

            if ((configuration != null)
                && (configuration.InverseNavigationProperty != null))
            {
                sourceEnd = associationType.TargetEnd;
                targetEnd = associationType.SourceEnd;
            }

            if (_inverseEndKind != null)
            {
                sourceEnd.RelationshipMultiplicity = _inverseEndKind.Value;
            }

            if (_endKind != null)
            {
                targetEnd.RelationshipMultiplicity = _endKind.Value;
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void ValidateConsistency(NavigationPropertyConfiguration navigationPropertyConfiguration)
        {
            DebugCheck.NotNull(navigationPropertyConfiguration);

            if ((navigationPropertyConfiguration.InverseEndKind != null)
                && (RelationshipMultiplicity != null)
                && (navigationPropertyConfiguration.InverseEndKind != RelationshipMultiplicity))
            {
                throw Error.ConflictingMultiplicities(
                    NavigationProperty.Name, NavigationProperty.ReflectedType);
            }

            if ((navigationPropertyConfiguration.RelationshipMultiplicity != null)
                && (InverseEndKind != null)
                && (navigationPropertyConfiguration.RelationshipMultiplicity != InverseEndKind))
            {
                if (InverseNavigationProperty == null)
                {
                    // InverseNavigationProperty may be null if the association is bi-directional and is configured
                    // from both sides but on one side the navigation property is not specified in the configuration.
                    // See Dev11 330745.
                    // In this case we use the navigation property that we do know about in the exception message.
                    throw Error.ConflictingMultiplicities(
                        NavigationProperty.Name, NavigationProperty.ReflectedType);
                }
                throw Error.ConflictingMultiplicities(
                    InverseNavigationProperty.Name, InverseNavigationProperty.ReflectedType);
            }

            if ((navigationPropertyConfiguration.DeleteAction != null)
                && (DeleteAction != null)
                && (navigationPropertyConfiguration.DeleteAction != DeleteAction))
            {
                throw Error.ConflictingCascadeDeleteOperation(
                    NavigationProperty.Name, NavigationProperty.ReflectedType);
            }

            if ((navigationPropertyConfiguration.Constraint != null)
                && (Constraint != null)
                && !Equals(navigationPropertyConfiguration.Constraint, Constraint))
            {
                throw Error.ConflictingConstraint(
                    NavigationProperty.Name, NavigationProperty.ReflectedType);
            }

            if ((navigationPropertyConfiguration.IsNavigationPropertyDeclaringTypePrincipal != null)
                && (IsNavigationPropertyDeclaringTypePrincipal != null)
                && navigationPropertyConfiguration.IsNavigationPropertyDeclaringTypePrincipal
                == IsNavigationPropertyDeclaringTypePrincipal)
            {
                throw Error.ConflictingConstraint(
                    NavigationProperty.Name, NavigationProperty.ReflectedType);
            }

            if ((navigationPropertyConfiguration.AssociationMappingConfiguration != null)
                && (AssociationMappingConfiguration != null)
                && !Equals(
                    navigationPropertyConfiguration.AssociationMappingConfiguration, AssociationMappingConfiguration))
            {
                throw Error.ConflictingMapping(
                    NavigationProperty.Name, NavigationProperty.ReflectedType);
            }

            if ((navigationPropertyConfiguration.ModificationFunctionsConfiguration != null)
                && (ModificationFunctionsConfiguration != null)
                && !navigationPropertyConfiguration.ModificationFunctionsConfiguration.IsCompatibleWith(ModificationFunctionsConfiguration))
            {
                throw Error.ConflictingFunctionsMapping(
                    NavigationProperty.Name, NavigationProperty.ReflectedType);
            }
        }

        private void ConfigureDependentBehavior(
            AssociationType associationType, EdmModel model, EntityTypeConfiguration entityTypeConfiguration)
        {
            DebugCheck.NotNull(associationType);
            DebugCheck.NotNull(model);
            DebugCheck.NotNull(entityTypeConfiguration);

            AssociationEndMember principalEnd;
            AssociationEndMember dependentEnd;

            if (!associationType.TryGuessPrincipalAndDependentEnds(out principalEnd, out dependentEnd))
            {
                if (IsNavigationPropertyDeclaringTypePrincipal.HasValue)
                {
                    associationType.MarkPrincipalConfigured();

                    var navProp = model.EntityTypes
                                       .SelectMany(et => et.DeclaredNavigationProperties)
                                       .Single(np => np.GetClrPropertyInfo().IsSameAs(NavigationProperty));

                    principalEnd = IsNavigationPropertyDeclaringTypePrincipal.Value
                                       ? associationType.GetOtherEnd(navProp.ResultEnd)
                                       : navProp.ResultEnd;

                    dependentEnd = associationType.GetOtherEnd(principalEnd);

                    if (associationType.SourceEnd != principalEnd)
                    {
                        // need to move around source to be principal, target to be dependent so Edm services will use the correct
                        // principal and dependent ends. The Edm default Db + mapping service tries to guess principal/dependent
                        // based on multiplicities, but if it can't figure it out, it will use source as principal and target as dependent
                        associationType.SourceEnd = principalEnd;
                        associationType.TargetEnd = dependentEnd;

                        var associationSet
                            = model.Containers
                                   .SelectMany(ct => ct.AssociationSets)
                                   .Single(aset => aset.ElementType == associationType);

                        var sourceSet = associationSet.SourceSet;

                        associationSet.SourceSet = associationSet.TargetSet;
                        associationSet.TargetSet = sourceSet;
                    }
                }

                if (principalEnd == null)
                {
                    dependentEnd = associationType.TargetEnd;
                }
            }

            ConfigureConstraint(associationType, dependentEnd, entityTypeConfiguration);
            ConfigureDeleteAction(associationType.GetOtherEnd(dependentEnd));
        }

        private void ConfigureConstraint(
            AssociationType associationType,
            AssociationEndMember dependentEnd,
            EntityTypeConfiguration entityTypeConfiguration)
        {
            DebugCheck.NotNull(associationType);
            DebugCheck.NotNull(dependentEnd);
            DebugCheck.NotNull(entityTypeConfiguration);

            if (_constraint != null)
            {
                _constraint.Configure(associationType, dependentEnd, entityTypeConfiguration);

                var associationConstraint = associationType.Constraint;

                if ((associationConstraint != null)
                    && associationConstraint.ToProperties
                                            .SequenceEqual(associationConstraint.ToRole.GetEntityType().KeyProperties))
                {
                    // The dependent FK is also the PK. We need to adjust the multiplicity
                    // when it has not been explicity configured because the default is *:0..1

                    if ((_inverseEndKind == null)
                        && associationType.SourceEnd.IsMany())
                    {
                        associationType.SourceEnd.RelationshipMultiplicity = Core.Metadata.Edm.RelationshipMultiplicity.ZeroOrOne;
                        associationType.TargetEnd.RelationshipMultiplicity = Core.Metadata.Edm.RelationshipMultiplicity.One;
                    }
                }
            }
        }

        private void ConfigureDeleteAction(AssociationEndMember principalEnd)
        {
            DebugCheck.NotNull(principalEnd);

            if (DeleteAction != null)
            {
                principalEnd.DeleteBehavior = DeleteAction.Value;
            }
        }

        internal void Reset()
        {
            _endKind = null;
            _inverseNavigationProperty = null;
            _inverseEndKind = null;
            _constraint = null;
            _associationMappingConfiguration = null;
        }
    }
}
