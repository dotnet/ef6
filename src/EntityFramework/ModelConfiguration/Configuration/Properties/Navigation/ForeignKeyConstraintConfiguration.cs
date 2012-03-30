namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Resources;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

    internal class ForeignKeyConstraintConfiguration : ConstraintConfiguration
    {
        private readonly List<PropertyInfo> _dependentProperties = new List<PropertyInfo>();
        private readonly bool _isFullySpecified;

        internal ForeignKeyConstraintConfiguration()
        {
        }

        internal ForeignKeyConstraintConfiguration(IEnumerable<PropertyInfo> dependentProperties)
        {
            Contract.Requires(dependentProperties != null);
            Contract.Assert(dependentProperties.Any());

            _dependentProperties.AddRange(dependentProperties);

            _isFullySpecified = true;
        }

        private ForeignKeyConstraintConfiguration(ForeignKeyConstraintConfiguration source)
        {
            Contract.Requires(source != null);

            _dependentProperties.AddRange(source._dependentProperties);
            _isFullySpecified = source._isFullySpecified;
        }

        internal override ConstraintConfiguration Clone()
        {
            return new ForeignKeyConstraintConfiguration(this);
        }

        internal override bool IsFullySpecified
        {
            get { return _isFullySpecified; }
        }

        internal IEnumerable<PropertyInfo> DependentProperties
        {
            get { return _dependentProperties; }
        }

        internal void AddColumn(PropertyInfo propertyInfo)
        {
            Contract.Requires(propertyInfo != null);

            // DevDiv #324763 (DbModelBuilder.Build is not idempotent):  If build is called twice when foreign keys are 
            // configured via attributes, we need to check whether the key has already been included.
            if (!_dependentProperties.ContainsSame(propertyInfo))
            {
                _dependentProperties.Add(propertyInfo);
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        internal override void Configure(
            EdmAssociationType associationType, EdmAssociationEnd dependentEnd,
            EntityTypeConfiguration entityTypeConfiguration)
        {
            if (!_dependentProperties.Any())
            {
                return;
            }

            var associationConstraint
                = new EdmAssociationConstraint
                      {
                          DependentEnd = dependentEnd
                      };

            var dependentProperties = Enumerable.AsEnumerable(_dependentProperties);

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
                    var dependentKeys = dependentEnd.EntityType.DeclaredKeyProperties;

                    if ((dependentKeys.Count == _dependentProperties.Count)
                        &&
                        foreignKeys.All(fk => dependentKeys.Any(p => p.GetClrPropertyInfo().IsSameAs(fk.PropertyInfo))))
                    {
                        // The FK and PK sets are equal, we know the order
                        dependentProperties = dependentKeys.Select(p => p.GetClrPropertyInfo());
                    }
                    else
                    {
                        throw Error.ForeignKeyAttributeConvention_OrderRequired(entityTypeConfiguration.ClrType);
                    }
                }
                else
                {
                    dependentProperties = foreignKeys.OrderBy(p => p.ColumnOrder).Select(p => p.PropertyInfo);
                }
            }

            foreach (var dependentProperty in dependentProperties)
            {
                var property
                    = associationConstraint
                        .DependentEnd
                        .EntityType
                        .GetDeclaredPrimitiveProperty(dependentProperty);

                if (property == null)
                {
                    throw Error.ForeignKeyPropertyNotFound(
                        dependentProperty.Name, associationConstraint.DependentEnd.EntityType.Name);
                }

                associationConstraint.DependentProperties.Add(property);
            }

            associationType.Constraint = associationConstraint;

            var principalEnd = associationType.GetOtherEnd(dependentEnd);

            if (principalEnd.IsRequired())
            {
                associationType.Constraint.DependentProperties.Each(p => p.PropertyType.IsNullable = false);
            }
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

            return other.DependentProperties
                .SequenceEqual(
                    DependentProperties,
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
            return DependentProperties.Aggregate(0, (t, p) => t + p.GetHashCode());
        }
    }
}
