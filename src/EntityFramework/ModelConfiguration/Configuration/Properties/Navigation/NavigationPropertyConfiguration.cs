namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Db.Mapping;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

    internal class NavigationPropertyConfiguration : PropertyConfiguration
    {
        private readonly PropertyInfo _navigationProperty;
        private EdmAssociationEndKind? _endKind;
        private PropertyInfo _inverseNavigationProperty;
        private EdmAssociationEndKind? _inverseEndKind;
        private ConstraintConfiguration _constraint;
        private AssociationMappingConfiguration _associationMappingConfiguration;

        internal NavigationPropertyConfiguration(PropertyInfo navigationProperty)
        {
            Contract.Requires(navigationProperty != null);

            _navigationProperty = navigationProperty;
        }

        private NavigationPropertyConfiguration(NavigationPropertyConfiguration source)
        {
            Contract.Requires(source != null);

            _navigationProperty = source._navigationProperty;
            _endKind = source._endKind;
            _inverseNavigationProperty = source._inverseNavigationProperty;
            _inverseEndKind = source._inverseEndKind;

            _constraint = source._constraint == null ? null : source._constraint.Clone();

            _associationMappingConfiguration = source._associationMappingConfiguration == null
                                                   ? null
                                                   : source._associationMappingConfiguration.Clone();

            DeleteAction = source.DeleteAction;
            IsNavigationPropertyDeclaringTypePrincipal = source.IsNavigationPropertyDeclaringTypePrincipal;
        }

        internal virtual NavigationPropertyConfiguration Clone()
        {
            return new NavigationPropertyConfiguration(this);
        }

        public EdmOperationAction? DeleteAction { get; set; }

        internal PropertyInfo NavigationProperty
        {
            get { return _navigationProperty; }
        }

        public EdmAssociationEndKind? EndKind
        {
            get { return _endKind; }
            set
            {
                Contract.Requires(value != null);

                _endKind = value;
            }
        }

        internal PropertyInfo InverseNavigationProperty
        {
            get { return _inverseNavigationProperty; }
            set
            {
                Contract.Requires(value != null);

                if (value == _navigationProperty)
                {
                    throw Error.NavigationInverseItself(value.Name, value.ReflectedType);
                }

                _inverseNavigationProperty = value;
            }
        }

        internal EdmAssociationEndKind? InverseEndKind
        {
            get { return _inverseEndKind; }
            set
            {
                Contract.Requires(value != null);

                _inverseEndKind = value;
            }
        }

        public ConstraintConfiguration Constraint
        {
            get { return _constraint; }
            set
            {
                Contract.Requires(value != null);

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
                Contract.Requires(value != null);

                _associationMappingConfiguration = value;
            }
        }

        internal void Configure(
            EdmNavigationProperty navigationProperty, EdmModel model, EntityTypeConfiguration entityTypeConfiguration)
        {
            Contract.Requires(navigationProperty != null);
            Contract.Requires(model != null);
            Contract.Requires(entityTypeConfiguration != null);

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

        internal void Configure(DbAssociationSetMapping associationSetMapping, DbDatabaseMapping databaseMapping)
        {
            Contract.Requires(associationSetMapping != null);
            Contract.Requires(databaseMapping != null);

            // We may apply configuration twice from two different NavigationPropertyConfiguration objects,
            // but that should be okay since they were validated as consistent above.
            // We still apply twice because each object may have different pieces of the full configuration.
            if (AssociationMappingConfiguration != null)
            {
                // This may replace a configuration previously set, but that's okay since we validated
                // consistency when processing the configuration above.
                associationSetMapping.SetConfiguration(this);

                AssociationMappingConfiguration.Configure(associationSetMapping, databaseMapping.Database);
            }
        }

        private void ConfigureInverse(EdmAssociationType associationType, EdmModel model)
        {
            Contract.Requires(associationType != null);
            Contract.Requires(model != null);

            if (_inverseNavigationProperty == null)
            {
                return;
            }

            var inverseNavigationProperty
                = model.GetNavigationProperty(_inverseNavigationProperty);

            if ((inverseNavigationProperty != null)
                && (inverseNavigationProperty.Association != associationType))
            {
                associationType.SourceEnd.EndKind = inverseNavigationProperty.Association.TargetEnd.EndKind;

                if ((associationType.Constraint == null)
                    && (_constraint == null)
                    && (inverseNavigationProperty.Association.Constraint != null))
                {
                    associationType.Constraint = inverseNavigationProperty.Association.Constraint;
                    associationType.Constraint.DependentEnd =
                        associationType.Constraint.DependentEnd.EntityType == associationType.SourceEnd.EntityType
                            ? associationType.SourceEnd
                            : associationType.TargetEnd;
                }

                model.RemoveAssociationType(inverseNavigationProperty.Association);
                inverseNavigationProperty.Association = associationType;
                inverseNavigationProperty.ResultEnd = associationType.SourceEnd;
            }
        }

        private void ConfigureEndKinds(
            EdmAssociationType associationType, NavigationPropertyConfiguration configuration)
        {
            Contract.Requires(associationType != null);

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
                sourceEnd.EndKind = _inverseEndKind.Value;
            }

            if (_endKind != null)
            {
                targetEnd.EndKind = _endKind.Value;
            }
        }

        private void ValidateConsistency(NavigationPropertyConfiguration navigationPropertyConfiguration)
        {
            Contract.Requires(navigationPropertyConfiguration != null);

            if ((navigationPropertyConfiguration.InverseEndKind != null)
                && (EndKind != null)
                && (navigationPropertyConfiguration.InverseEndKind != EndKind))
            {
                throw Error.ConflictingMultiplicities(
                    NavigationProperty.Name, NavigationProperty.ReflectedType);
            }

            if ((navigationPropertyConfiguration.EndKind != null)
                && (InverseEndKind != null)
                && (navigationPropertyConfiguration.EndKind != InverseEndKind))
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
                &&
                navigationPropertyConfiguration.IsNavigationPropertyDeclaringTypePrincipal
                == IsNavigationPropertyDeclaringTypePrincipal)
            {
                throw Error.ConflictingConstraint(
                    NavigationProperty.Name, NavigationProperty.ReflectedType);
            }

            if ((navigationPropertyConfiguration.AssociationMappingConfiguration != null)
                && (AssociationMappingConfiguration != null)
                &&
                !Equals(
                    navigationPropertyConfiguration.AssociationMappingConfiguration, AssociationMappingConfiguration))
            {
                throw Error.ConflictingMapping(
                    NavigationProperty.Name, NavigationProperty.ReflectedType);
            }
        }

        private void ConfigureDependentBehavior(
            EdmAssociationType associationType, EdmModel model, EntityTypeConfiguration entityTypeConfiguration)
        {
            Contract.Requires(associationType != null);
            Contract.Requires(model != null);
            Contract.Requires(entityTypeConfiguration != null);

            EdmAssociationEnd principalEnd;
            EdmAssociationEnd dependentEnd;

            if (!associationType.TryGuessPrincipalAndDependentEnds(out principalEnd, out dependentEnd))
            {
                if (IsNavigationPropertyDeclaringTypePrincipal.HasValue)
                {
                    associationType.MarkPrincipalConfigured();

                    var navProp = model.Namespaces
                        .SelectMany(ns => ns.EntityTypes)
                        .SelectMany(et => et.DeclaredNavigationProperties)
                        .Where(np => np.GetClrPropertyInfo().IsSameAs(NavigationProperty))
                        .Single();

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
                                .Where(aset => aset.ElementType == associationType).Single();

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
            EdmAssociationType associationType,
            EdmAssociationEnd dependentEnd,
            EntityTypeConfiguration entityTypeConfiguration)
        {
            Contract.Requires(associationType != null);
            Contract.Requires(dependentEnd != null);
            Contract.Requires(entityTypeConfiguration != null);

            if (_constraint != null)
            {
                _constraint.Configure(associationType, dependentEnd, entityTypeConfiguration);

                var associationConstraint = associationType.Constraint;

                if ((associationConstraint != null)
                    && associationConstraint.DependentProperties
                           .SequenceEqual(associationConstraint.DependentEnd.EntityType.DeclaredKeyProperties))
                {
                    // The dependent FK is also the PK. We need to adjust the multiplicity
                    // when it has not been explicity configured because the default is *:0..1

                    if ((_inverseEndKind == null)
                        && associationType.SourceEnd.IsMany())
                    {
                        associationType.SourceEnd.EndKind = EdmAssociationEndKind.Optional;
                        associationType.TargetEnd.EndKind = EdmAssociationEndKind.Required;
                    }
                }
            }
        }

        private void ConfigureDeleteAction(EdmAssociationEnd principalEnd)
        {
            Contract.Requires(principalEnd != null);

            if (DeleteAction != null)
            {
                principalEnd.DeleteAction = DeleteAction.Value;
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
