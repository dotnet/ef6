// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions.Sets
{
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal static class V1ConventionSet
    {
        private static readonly IConvention[] _conventions
            = new IConvention[]
                {
                    // Type Configuration
                    new NotMappedTypeAttributeConvention(),
                    new ComplexTypeAttributeConvention(),
                    new TableAttributeConvention(),
                    // Property Configuration
                    new NotMappedPropertyAttributeConvention(),
                    new KeyAttributeConvention(),
                    new RequiredPrimitivePropertyAttributeConvention(),
                    new RequiredNavigationPropertyAttributeConvention(),
                    new TimestampAttributeConvention(),
                    new ConcurrencyCheckAttributeConvention(),
                    new DatabaseGeneratedAttributeConvention(),
                    new MaxLengthAttributeConvention(),
                    new StringLengthAttributeConvention(),
                    new ColumnAttributeConvention(),
                    new InversePropertyAttributeConvention(),
                    new ForeignKeyPrimitivePropertyAttributeConvention(),
                    // EDM
                    new IdKeyDiscoveryConvention(),
                    new AssociationInverseDiscoveryConvention(),
                    new ForeignKeyNavigationPropertyAttributeConvention(),
                    new OneToOneConstraintIntroductionConvention(),
                    new NavigationPropertyNameForeignKeyDiscoveryConvention(),
                    new PrimaryKeyNameForeignKeyDiscoveryConvention(),
                    new TypeNameForeignKeyDiscoveryConvention(),
                    new ForeignKeyAssociationMultiplicityConvention(),
                    new OneToManyCascadeDeleteConvention(),
                    new ComplexTypeDiscoveryConvention(),
                    new StoreGeneratedIdentityKeyConvention(),
                    new PluralizingEntitySetNameConvention(),
                    new DeclaredPropertyOrderingConvention(),
                    new PluralizingTableNameConvention(),
                    new ColumnOrderingConvention(),
                    new SqlCePropertyMaxLengthConvention(),
                    new PropertyMaxLengthConvention(),
                    new DecimalPropertyConvention(),
                    new ManyToManyCascadeDeleteConvention(),
                    new MappingInheritedPropertiesSupportConvention()
                };

        public static IConvention[] Conventions
        {
            get { return _conventions; }
        }
    }
}
