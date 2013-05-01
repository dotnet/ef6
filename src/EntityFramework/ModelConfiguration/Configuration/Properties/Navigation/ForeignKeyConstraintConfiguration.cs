// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    ///     Used to configure a foreign key constraint on a navigation property.
    /// </summary>
    public class ForeignKeyConstraintConfiguration : ConstraintConfiguration
    {
        private readonly List<PropertyInfo> _dependentProperties = new List<PropertyInfo>();
        private readonly bool _isFullySpecified;

        /// <summary>
        ///     Initializes a new instance of the ForeignKeyConstraintConfiguration class.
        /// </summary>
        public ForeignKeyConstraintConfiguration()
        {
        }

        internal ForeignKeyConstraintConfiguration(IEnumerable<PropertyInfo> dependentProperties)
        {
            DebugCheck.NotNull(dependentProperties);
            Debug.Assert(dependentProperties.Any());
            Debug.Assert(!dependentProperties.Any(p => p == null));

            _dependentProperties.AddRange(dependentProperties);

            _isFullySpecified = true;
        }

        private ForeignKeyConstraintConfiguration(ForeignKeyConstraintConfiguration source)
        {
            DebugCheck.NotNull(source);

            _dependentProperties.AddRange(source._dependentProperties);
            _isFullySpecified = source._isFullySpecified;
        }

        internal override ConstraintConfiguration Clone()
        {
            return new ForeignKeyConstraintConfiguration(this);
        }

        /// <inheritdoc />
        public override bool IsFullySpecified
        {
            get { return _isFullySpecified; }
        }

        internal IEnumerable<PropertyInfo> ToProperties
        {
            get { return _dependentProperties; }
        }

        /// <summary>
        ///     Configures the foreign key property(s) for this end of the navigation property.
        /// </summary>
        /// <param name="propertyInfo"> The property to be used as the foreign key. If the foreign key is made up of multiple properties, call this method once for each of them. </param>
        public void AddColumn(PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, "propertyInfo");

            // DevDiv #324763 (DbModelBuilder.Build is not idempotent):  If build is called twice when foreign keys are 
            // configured via attributes, we need to check whether the key has already been included.
            if (!_dependentProperties.ContainsSame(propertyInfo))
            {
                _dependentProperties.Add(propertyInfo);
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        internal override void Configure(
            AssociationType associationType,
            AssociationEndMember dependentEnd,
            EntityTypeConfiguration entityTypeConfiguration)
        {
            DebugCheck.NotNull(associationType);
            DebugCheck.NotNull(dependentEnd);
            DebugCheck.NotNull(entityTypeConfiguration);

            if (!_dependentProperties.Any())
            {
                return;
            }

            var dependentPropertInfos = _dependentProperties.AsEnumerable();

            if (!IsFullySpecified)
            {
                var foreignKeys
                    = from p in _dependentProperties
                      select new
                          {
                              PropertyInfo = p,
                              entityTypeConfiguration.Property(new PropertyPath(p)).ColumnOrder
                          };

                if ((_dependentProperties.Count > 1)
                    && foreignKeys.Any(p => !p.ColumnOrder.HasValue))
                {
                    var dependentKeys = dependentEnd.GetEntityType().KeyProperties;

                    if ((dependentKeys.Count == _dependentProperties.Count)
                        && foreignKeys.All(fk => dependentKeys.Any(p => p.GetClrPropertyInfo().IsSameAs(fk.PropertyInfo))))
                    {
                        // The FK and PK sets are equal, we know the order
                        dependentPropertInfos = dependentKeys.Select(p => p.GetClrPropertyInfo());
                    }
                    else
                    {
                        throw Error.ForeignKeyAttributeConvention_OrderRequired(entityTypeConfiguration.ClrType);
                    }
                }
                else
                {
                    dependentPropertInfos = foreignKeys.OrderBy(p => p.ColumnOrder).Select(p => p.PropertyInfo);
                }
            }

            var dependentProperties = new List<EdmProperty>();

            foreach (var dependentProperty in dependentPropertInfos)
            {
                var property
                    = dependentEnd.GetEntityType()
                        .GetDeclaredPrimitiveProperty(dependentProperty);

                if (property == null)
                {
                    throw Error.ForeignKeyPropertyNotFound(
                        dependentProperty.Name, dependentEnd.GetEntityType().Name);
                }

                dependentProperties.Add(property);
            }

            var principalEnd = associationType.GetOtherEnd(dependentEnd);

            var associationConstraint
                = new ReferentialConstraint(
                    principalEnd,
                    dependentEnd,
                    principalEnd.GetEntityType().KeyProperties,
                    dependentProperties);

            if (principalEnd.IsRequired())
            {
                associationConstraint.ToProperties.Each(p => p.Nullable = false);
            }

            associationType.Constraint = associationConstraint;
        }

        public bool Equals(ForeignKeyConstraintConfiguration other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return other.ToProperties
                .SequenceEqual(
                    ToProperties,
                    new DynamicEqualityComparer<PropertyInfo>((p1, p2) => p1.IsSameAs(p2)));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType()
                != typeof(ForeignKeyConstraintConfiguration))
            {
                return false;
            }

            return Equals((ForeignKeyConstraintConfiguration)obj);
        }

        public override int GetHashCode()
        {
            return ToProperties.Aggregate(0, (t, p) => t + p.GetHashCode());
        }
    }
}
