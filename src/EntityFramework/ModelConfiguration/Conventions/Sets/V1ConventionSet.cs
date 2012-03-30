namespace System.Data.Entity.ModelConfiguration.Conventions.Sets
{
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal static class V1ConventionSet
    {
        public static readonly IConvention[] Conventions
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
                      new ColumnTypeCasingConvention(),
                      new SqlCePropertyMaxLengthConvention(),
                      new PropertyMaxLengthConvention(),
                      new DecimalPropertyConvention(),
                      new ManyToManyCascadeDeleteConvention(),
                      new MappingInheritedPropertiesSupportConvention()
                  };
    }
}
