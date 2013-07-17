// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation
{
    using System.ComponentModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    ///     Used to create a convention that configures navigation properties.
    /// </summary>
    internal class ConventionNavigationPropertyConfiguration
    {
        private readonly NavigationPropertyConfiguration _configuration;
        private readonly ModelConfiguration _modelConfiguration;

        internal ConventionNavigationPropertyConfiguration(
            NavigationPropertyConfiguration configuration, ModelConfiguration modelConfiguration)
        {
            _configuration = configuration;
            _modelConfiguration = modelConfiguration;
        }

        /// <summary>
        ///     Gets the <see cref="PropertyInfo" /> for this property.
        /// </summary>
        public virtual PropertyInfo ClrPropertyInfo
        {
            get
            {
                return _configuration != null
                           ? _configuration.NavigationProperty
                           : null;
            }
        }

        internal NavigationPropertyConfiguration Configuration
        {
            get { return _configuration; }
        }

        /// <summary>
        ///     Configures the constraint associated with the navigation property.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of constraint configuration.
        /// <see cref="ForeignKeyConstraintConfiguration" /> for
        ///     foreign key constraints and <see cref="IndependentConstraintConfiguration" />
        ///     for independent constraints.
        /// </typeparam>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public virtual void HasConstraint<T>()
            where T : ConstraintConfiguration
        {
            HasConstraintInternal<T>(null);
        }

        /// <summary>
        ///     Configures the constraint associated with the navigation property.
        /// </summary>
        /// <param name="constraintConfigurationAction"> Constraint configuration to be applied. </param>
        /// <typeparam name="T">
        ///     The type of constraint configuration.
        /// <see cref="ForeignKeyConstraintConfiguration" /> for
        ///     foreign key constraints and <see cref="IndependentConstraintConfiguration" />
        ///     for independent constraints.
        /// </typeparam>
        public virtual void HasConstraint<T>(Action<T> constraintConfigurationAction)
            where T : ConstraintConfiguration
        {
            Check.NotNull(constraintConfigurationAction, "constraintConfigurationAction");

            HasConstraintInternal(constraintConfigurationAction);
        }

        private void HasConstraintInternal<T>(Action<T> constraintConfigurationAction)
            where T : ConstraintConfiguration
        {
            if (_configuration != null
                && !HasConfiguredConstraint())
            {
                var constraintType = typeof(T);
                if (_configuration.Constraint == null)
                {
                    if (constraintType == typeof(IndependentConstraintConfiguration))
                    {
                        _configuration.Constraint = IndependentConstraintConfiguration.Instance;
                    }
                    else
                    {
                        _configuration.Constraint = (ConstraintConfiguration)Activator.CreateInstance(constraintType);
                    }
                }
                else if (_configuration.Constraint.GetType() != constraintType)
                {
                    return;
                }

                if (constraintConfigurationAction != null)
                {
                    constraintConfigurationAction((T)_configuration.Constraint);
                }
            }
        }

        private bool HasConfiguredConstraint()
        {
            if (_configuration != null
                && _configuration.Constraint != null
                && _configuration.Constraint.IsFullySpecified)
            {
                return true;
            }

            if (_configuration != null
                && _configuration.InverseNavigationProperty != null)
            {
                var targetType = _configuration.NavigationProperty.PropertyType.GetTargetType();
                if (_modelConfiguration.Entities.Contains(targetType))
                {
                    var entityConfiguration = _modelConfiguration.Entity(targetType);
                    if (entityConfiguration.IsNavigationPropertyConfigured(_configuration.InverseNavigationProperty))
                    {
                        return entityConfiguration.Navigation(_configuration.InverseNavigationProperty)
                                   .Constraint != null;
                    }
                }
            }
            return false;
        }

        /// <summary>
        ///     Sets the inverse navigation property.
        /// </summary>
        public virtual ConventionNavigationPropertyConfiguration HasInverseNavigationProperty(
            Func<PropertyInfo, PropertyInfo> inverseNavigationPropertyGetter)
        {
            Check.NotNull(inverseNavigationPropertyGetter, "inverseNavigationPropertyGetter");

            if (_configuration != null
                && _configuration.InverseNavigationProperty == null)
            {
                var inverseNavigationProperty = inverseNavigationPropertyGetter(ClrPropertyInfo);
                Check.NotNull(inverseNavigationProperty, "inverseNavigationProperty");

                if (!inverseNavigationProperty.IsValidEdmNavigationProperty())
                {
                    throw new InvalidOperationException(
                        Strings.LightweightEntityConfiguration_InvalidNavigationProperty(inverseNavigationProperty.Name));
                }

                if (_configuration.NavigationProperty.PropertyType.GetTargetType() != inverseNavigationProperty.DeclaringType)
                {
                    throw new InvalidOperationException(
                        Strings.LightweightEntityConfiguration_MismatchedInverseNavigationProperty(
                            _configuration.NavigationProperty.PropertyType.GetTargetType(), _configuration.NavigationProperty.Name,
                            inverseNavigationProperty.DeclaringType, inverseNavigationProperty.Name));
                }

                if (inverseNavigationProperty.PropertyType.GetTargetType() != _configuration.NavigationProperty.DeclaringType)
                {
                    throw new InvalidOperationException(
                        Strings.LightweightEntityConfiguration_InvalidInverseNavigationProperty(
                            _configuration.NavigationProperty.DeclaringType, _configuration.NavigationProperty.Name,
                            inverseNavigationProperty.PropertyType.GetTargetType(), inverseNavigationProperty.Name));
                }

                if (_configuration.InverseEndKind.HasValue)
                {
                    VerifyMultiplicityCompatibility(_configuration.InverseEndKind.Value, inverseNavigationProperty);
                }

                _modelConfiguration
                    .Entity(_configuration.NavigationProperty.PropertyType.GetTargetType())
                    .Navigation(inverseNavigationProperty);

                _configuration.InverseNavigationProperty = inverseNavigationProperty;
            }

            return this;
        }

        /// <summary>
        ///     Sets the inverse end multiplicity.
        /// </summary>
        public virtual ConventionNavigationPropertyConfiguration HasInverseEndMultiplicity(RelationshipMultiplicity multiplicity)
        {
            if (_configuration != null
                && _configuration.InverseEndKind == null)
            {
                if (_configuration.InverseNavigationProperty != null)
                {
                    VerifyMultiplicityCompatibility(multiplicity, _configuration.InverseNavigationProperty);
                }

                _configuration.InverseEndKind = multiplicity;
            }

            return this;
        }

        /// <summary>
        ///     True if the navigation property's declaring type is the principal end, false if it is not
        /// </summary>
        public virtual ConventionNavigationPropertyConfiguration IsDeclaringTypePrincipal(bool isPrincipal)
        {
            if (_configuration != null
                && _configuration.IsNavigationPropertyDeclaringTypePrincipal == null)
            {
                _configuration.IsNavigationPropertyDeclaringTypePrincipal = isPrincipal;
            }

            return this;
        }

        /// <summary>
        ///     Sets the action to take when a delete operation is attempted.
        /// </summary>
        public virtual ConventionNavigationPropertyConfiguration HasDeleteAction(OperationAction deleteAction)
        {
            if (_configuration != null
                && _configuration.DeleteAction == null)
            {
                _configuration.DeleteAction = deleteAction;
            }

            return this;
        }

        /// <summary>
        ///     Sets the multiplicity of this end of the navigation property.
        /// </summary>
        public virtual ConventionNavigationPropertyConfiguration HasRelationshipMultiplicity(RelationshipMultiplicity multiplicity)
        {
            if (_configuration != null
                && _configuration.RelationshipMultiplicity == null)
            {
                VerifyMultiplicityCompatibility(multiplicity, _configuration.NavigationProperty);

                _configuration.RelationshipMultiplicity = multiplicity;
            }

            return this;
        }

        private static void VerifyMultiplicityCompatibility(RelationshipMultiplicity multiplicity, PropertyInfo propertyInfo)
        {
            var isCompatible = true;
            switch (multiplicity)
            {
                case RelationshipMultiplicity.Many:
                    isCompatible = propertyInfo.PropertyType.IsCollection();
                    break;
                case RelationshipMultiplicity.One:
                case RelationshipMultiplicity.ZeroOrOne:
                    isCompatible = !propertyInfo.PropertyType.IsCollection();
                    break;
                default:
                    throw new InvalidOperationException(Strings.LightweightNavigationPropertyConfiguration_InvalidMultiplicity(multiplicity));
            }

            if (!isCompatible)
            {
                throw new InvalidOperationException(
                    Strings.LightweightNavigationPropertyConfiguration_IncompatibleMultiplicity(
                        RelationshipMultiplicityConverter.MultiplicityToString(multiplicity),
                        propertyInfo.DeclaringType + "." + propertyInfo.Name,
                        propertyInfo.PropertyType));
            }
        }

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }
    }
}
