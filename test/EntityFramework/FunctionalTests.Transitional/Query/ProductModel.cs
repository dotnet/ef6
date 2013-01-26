// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query
{
    public static class ProductModel
    {
        public static readonly string ssdl =
            @"<?xml version=""1.0"" encoding=""utf-8""?>
<Schema Namespace=""ProductStore"" Alias=""Self"" Provider=""System.Data.SqlClient"" ProviderManifestToken=""2008"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm/ssdl"">
  <EntityContainer Name=""ProductContainer_Store"">
    <EntitySet Name=""Products"" EntityType=""Self.Product"" Schema=""dbo"" Table=""Products"" />
    <EntitySet Name=""Customers"" EntityType=""Self.Customer"" Schema=""dbo"" Table=""Customers"" />
  </EntityContainer>
  <EntityType Name=""Product"">
    <Key>
      <PropertyRef Name=""ProductID"" />
    </Key>
    <Property Name=""ProductID"" Type=""int"" Nullable=""false"" />
    <Property Name=""ProductName"" Type=""nvarchar"" MaxLength=""40"" />
    <Property Name=""ReorderLevel"" Type=""smallint"" />
    <Property Name=""Discontinued"" Type=""bit"" />
  </EntityType>
  <EntityType Name=""Customer"">
    <Key>
      <PropertyRef Name=""CustomerID"" />
    </Key>
    <Property Name=""CustomerID"" Type=""nvarchar"" Nullable=""false"" MaxLength=""5""/>
    <Property Name=""HomeAddress"" Type=""nvarchar"" MaxLength=""60"" />
    <Property Name=""City"" Type=""nvarchar"" MaxLength=""15"" />
    <Property Name=""Region"" Type=""nvarchar"" MaxLength=""15"" />
    <Property Name=""PostalCode"" Type=""nvarchar"" MaxLength=""10"" />
    <Property Name=""Country"" Type=""nvarchar"" MaxLength=""15"" />
    <Property Name=""Phone"" Type=""nvarchar"" MaxLength=""24"" />
    <Property Name=""Fax"" Type=""nvarchar"" MaxLength=""24"" />
  </EntityType>
</Schema>";

        public static readonly string msl =
            @"<?xml version=""1.0"" encoding=""utf-8""?>
<Mapping xmlns=""http://schemas.microsoft.com/ado/2009/11/mapping/cs"" Space=""C-S"">
  <EntityContainerMapping CdmEntityContainer=""ProductContainer"" StorageEntityContainer=""ProductContainer_Store"">
    <EntitySetMapping Name=""Products"">
      <EntityTypeMapping TypeName=""ProductModel.Product"">
        <MappingFragment StoreEntitySet=""Products"">
          <ScalarProperty Name=""ProductID"" ColumnName=""ProductID"" />
          <ScalarProperty Name=""ProductName"" ColumnName=""ProductName"" />
          <ScalarProperty Name=""ReorderLevel"" ColumnName=""ReorderLevel"" />
          <Condition Value=""false"" ColumnName=""Discontinued"" />
        </MappingFragment>
      </EntityTypeMapping>
      <EntityTypeMapping TypeName=""ProductModel.DiscontinuedProduct"">
        <MappingFragment StoreEntitySet=""Products"">
          <ScalarProperty Name=""ProductID"" ColumnName=""ProductID"" />
          <ScalarProperty Name=""ProductName"" ColumnName=""ProductName"" />
          <ScalarProperty Name=""ReorderLevel"" ColumnName=""ReorderLevel"" />
          <Condition Value=""true"" ColumnName=""Discontinued"" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name=""Customers"">
      <EntityTypeMapping TypeName=""ProductModel.Customer"">
        <MappingFragment StoreEntitySet=""Customers"">
          <ScalarProperty Name=""CustomerID"" ColumnName=""CustomerID"" />
          <ComplexProperty Name=""Address"" TypeName=""ProductModel.Address"">
            <ScalarProperty Name=""HomeAddress"" ColumnName=""HomeAddress"" />
            <ScalarProperty Name=""City"" ColumnName=""City"" />
            <ScalarProperty Name=""Region"" ColumnName=""Region"" />
            <ScalarProperty Name=""PostalCode"" ColumnName=""PostalCode"" />
            <ScalarProperty Name=""Country"" ColumnName=""Country"" />
          </ComplexProperty>
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
</EntityContainerMapping>
</Mapping>";

        private static readonly string csdlTemplate =
            @"<?xml version=""1.0"" encoding=""utf-8""?>
<Schema Namespace=""ProductModel"" Alias=""Self"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"">
  <EntityContainer Name=""ProductContainer"">
    <EntitySet Name=""Products"" EntityType=""Self.Product"" />
    <EntitySet Name=""Customers"" EntityType=""Self.Customer"" />
  </EntityContainer>
  <EntityType Name=""Product"">
    <Key>
      <PropertyRef Name=""ProductID"" />
    </Key>
    <Property Name=""ProductID"" Type=""Int32"" Nullable=""false"" />
    <Property Name=""ProductName"" Type=""String"" MaxLength=""40"" />
    <Property Name=""ReorderLevel"" Type=""Int16"" />
  </EntityType>
  <EntityType Name=""DiscontinuedProduct"" BaseType=""Self.Product"" />
  <EntityType Name=""Customer"">
    <Key>
      <PropertyRef Name=""CustomerID"" />
    </Key>
    <Property Name=""CustomerID"" Type=""String"" Nullable=""false"" />
    <Property Name=""Address"" Type=""Self.Address"" Nullable=""false"" />
  </EntityType>
  <ComplexType Name=""Address"">
    <Property Name=""HomeAddress"" Type=""String"" />
    <Property Name=""City"" Type=""String"" />
    <Property Name=""Region"" Type=""String"" />
    <Property Name=""PostalCode"" Type=""String"" />
    <Property Name=""Country"" Type=""String"" />
  </ComplexType>
  <Function Name=""F_CountProducts"" ReturnType=""Int32"">
    <Parameter Name=""products"" Type=""Collection(Self.Product)""/>
    <DefiningExpression>
      count(select value 1 from products)
    </DefiningExpression>
  </Function>
  {0}
</Schema>";

        private static readonly string modelDefinedFunctions =
            @" <Function Name=""F_NoBody"" ReturnType=""Int32"" />
  <Function Name=""F_A"" ReturnType=""Int32"">
    <DefiningExpression>
      using ProductModel;
      F_B() - F_C()
    </DefiningExpression>
  </Function>
  <!-- positive: test inline functions in the function definition -->
  <Function Name=""F_B"" ReturnType=""Int32"">
    <DefiningExpression>
      using ProductModel;
      function F_B() as (1)
      function F_A() as (F_B())
      F_A()
    </DefiningExpression>
  </Function>
  <!-- positive: test backward references -->
  <Function Name=""F_C"" ReturnType=""Int32"">
    <DefiningExpression>
      using ProductModel;
      F_B() + 2
    </DefiningExpression>
  </Function>
  <!-- negative: test self reference -->
  <Function Name=""F_D"" ReturnType=""Int32"">
    <DefiningExpression>
      using ProductModel;
      F_D() + 1
    </DefiningExpression>
  </Function>
  <!-- negative: test recursion thru another function -->
  <Function Name=""F_E"" ReturnType=""Int32"">
    <DefiningExpression>
      using ProductModel;
      F_F() - F_C()
    </DefiningExpression>
  </Function>
  <!-- negative: test recursion thru another function -->
  <Function Name=""F_F"" ReturnType=""Int32"">
    <DefiningExpression>
      using ProductModel;
      F_E() + 1
    </DefiningExpression>
  </Function>
  <!-- negative: when used with a query containing an inline function F_H() should still fail because 
       model functions can not reference query functions-->
  <Function Name=""F_G"" ReturnType=""Int32"">
    <DefiningExpression>
      using ProductModel;
      F_H() + 1
    </DefiningExpression>
  </Function>
  <!-- negative: function definition has different type than the declaration -->
  <Function Name=""F_I"" ReturnType=""Int32"">
    <Parameter Name=""p"" Type=""Int64""/>
    <DefiningExpression>
      CAST(p AS Int16)
    </DefiningExpression>
  </Function>
  <!-- positive: identity function -->
  <Function Name=""F_J"" ReturnType=""Int32"">
    <Parameter Name=""p"" Type=""Int32""/>
    <DefiningExpression>
      p
    </DefiningExpression>
  </Function>
  <!-- positive: function returns a scalar type -->
  <Function Name=""F_Ret_ST"" ReturnType=""Int32"">
    <DefiningExpression>
      using ProductModel;
      Count(select value 3 from ProductContainer.Products as p)
    </DefiningExpression>
  </Function>
  <!-- positive: function returns a collection of scalars type -->
  <Function Name=""F_Ret_ColST"" ReturnType=""Collection(Int32)"">
    <DefiningExpression>
      using ProductModel;
      select value p.ProductID - 3 from ProductContainer.Products as p
    </DefiningExpression>
  </Function>
  <!-- positive: function returns an entity type -->
  <Function Name=""F_Ret_ET"" ReturnType=""Self.Product"">
    <DefiningExpression>
      using ProductModel;
      anyelement(select value top(1) p from ProductContainer.Products as p order by p.ProductID)
    </DefiningExpression>
  </Function>
  <!-- positive: function returns a collection of entities type -->
  <Function Name=""F_Ret_ColET"" ReturnType=""Collection(Self.Product)"">
    <DefiningExpression>
      using ProductModel;
      select value top(5) p from ProductContainer.Products as p order by p.ProductID
    </DefiningExpression>
  </Function>
  <!-- positive: function takes an entity type returns an entity type -->
  <Function Name=""F_In_ET_Ret_BaseET"" ReturnType=""Self.Product"">
    <Parameter Name=""Prod"" Type=""Self.Product""/>
    <DefiningExpression>
      using ProductModel;
      Prod
    </DefiningExpression>
  </Function>
  <!-- positive: function takes an entity type returns an entity type -->
  <Function Name=""F_In_ET_Ret_DerivedET"" ReturnType=""Self.DiscontinuedProduct"">
    <Parameter Name=""Prod"" Type=""Self.DiscontinuedProduct""/>
    <DefiningExpression>
      using ProductModel;
      Prod
    </DefiningExpression>
  </Function>
  <!-- positive: function returns a complex type -->
  <Function Name=""F_Ret_CT"" ReturnType=""Self.Address"">
    <DefiningExpression>
      using ProductModel;
      anyelement(select value top(1) c.Address from ProductContainer.Customers as c order by c.CustomerID)
    </DefiningExpression>
  </Function>
  <!-- positive: function returns a collection of complex type elements -->
  <Function Name=""F_Ret_ColCT"" ReturnType=""Collection(Self.Address)"">
    <DefiningExpression>
      using ProductModel;
      select value top(5) c.Address from ProductContainer.Customers as c order by c.CustomerID
    </DefiningExpression>
  </Function>
  <!-- positive: function takes collection of scalars returns a collection of entity type -->
  <Function Name=""F_In_ColST_Ret_ColET"" ReturnType=""Collection(Self.Customer)"">
    <Parameter Name=""IDs"" Type=""Collection(String)""/>
    <DefiningExpression>
      using ProductModel;
      select value c from ProductContainer.Customers as c where c.CustomerID in IDs
    </DefiningExpression>
  </Function>
  <!-- positive: function takes collection of entities returns a collection of complex type -->
  <Function Name=""F_In_ColET_Ret_ColCT"" ReturnType=""Collection(Self.Address)"">
    <Parameter Name=""pCustomers"" Type=""Collection(Self.Customer)""/>
    <DefiningExpression>
      using ProductModel;
      select value c.Address from pCustomers as c where c.CustomerID = 'BONAP'
    </DefiningExpression>
  </Function>
  <!-- positive: function takes collection of complex type elements returns a scalar -->
  <Function Name=""F_In_ColCT_Ret_ST"" ReturnType=""String"">
    <Parameter Name=""pAddresses"" Type=""Collection(Self.Address)""/>
    <DefiningExpression>
      using ProductModel;
      Min(select value a.City from pAddresses as a where a.Country = 'Mexico')
    </DefiningExpression>
  </Function>
  <!-- positive: function takes collection of complex type elements returns a collection of scalar types -->
  <Function Name=""F_In_ColCT_Ret_ColST"" ReturnType=""Collection(String)"">
    <Parameter Name=""pAddresses"" Type=""Collection(Self.Address)""/>
    <DefiningExpression>
      using ProductModel;
      select value a.City from pAddresses as a where a.Country = 'Mexico'
    </DefiningExpression>
  </Function>
  <!-- positive: function takes collection of Customers entities returns a collection of entities -->
  <Function Name=""F_In_ColET_Ret_ColET"" ReturnType=""Collection(Self.Customer)"">
    <Parameter Name=""pCustomers"" Type=""Collection(Self.Customer)""/>
    <DefiningExpression>
      using ProductModel;
      select value c from pCustomers as c where c.Address.Country = 'Mexico'
    </DefiningExpression>
  </Function>
  <!-- positive: function takes collection of complex type elements returns a collection of complex type elements -->
  <Function Name=""F_In_ColCT_Ret_ColCT"" ReturnType=""Collection(Self.Address)"">
    <Parameter Name=""pAddresses"" Type=""Collection(Self.Address)""/>
    <DefiningExpression>
      using ProductModel;
      select value p from pAddresses as p where p.Country = 'Mexico'
    </DefiningExpression>
  </Function>
  <!-- positive: function takes collection of scalars returns a collection of scalars -->
  <Function Name=""F_In_ColST_Ret_ColST"" ReturnType=""Collection(String)"">
    <Parameter Name=""pStrings"" Type=""Collection(String)""/>
    <DefiningExpression>
      using ProductModel;
      pStrings intersect {'a', 'b', 'c'}
    </DefiningExpression>
  </Function>
  <!-- positive: function takes collection of scalars returns a collection of scalars -->
  <Function Name=""F_In_ColST_Ret_NonEmptyColST"" ReturnType=""Collection(String)"">
    <Parameter Name=""pStrings"" Type=""Collection(String)""/>
    <DefiningExpression>
      using ProductModel;
      pStrings
    </DefiningExpression>
  </Function>
  <!-- positive: function takes a row returns a row-->
  <Function Name=""F_In_RT_Ret_RT"">
    <ReturnType>
      <RowType>
        <Property Name=""X"" Type=""Int32""/>
        <Property Name=""Y"" Type=""String""/>
        <Property Name=""AA"" Type=""Self.Address""/>
      </RowType>
    </ReturnType>
    <Parameter Name=""pRow"">
      <RowType>
        <Property Name=""Cust"" Type=""Self.Customer""/>
      </RowType>
    </Parameter>
    <DefiningExpression>
      using ProductModel;
      row(Length(pRow.Cust.CustomerID) as X, pRow.Cust.Address.City as Y, pRow.Cust.Address as AA)
    </DefiningExpression>
  </Function>
  <!-- positive: function takes a scalar returns a row-->
  <Function Name=""F_In_ST_Ret_RT"">
    <ReturnType>
      <RowType>
        <Property Name=""X"" Type=""Int32""/>
        <Property Name=""Y"" Type=""String""/>
      </RowType>
    </ReturnType>
    <Parameter Name=""p"" Type=""Int32""/>
    <DefiningExpression>
      using ProductModel;
      row(p + 3 as X, 'alltypes' as Y)
    </DefiningExpression>
  </Function>
  <!-- positive: function takes a row(Int, String) returns a scalar-->
  <Function Name=""F_In_RT_Int_String_Ret_ST"" ReturnType=""Int32"">
    <Parameter Name=""pRow"">
      <RowType>
        <Property Name=""X"" Type=""Int32""/>
        <Property Name=""Y"" Type=""String""/>
      </RowType>
    </Parameter>
    <DefiningExpression>
      using ProductModel;
      pRow.X
    </DefiningExpression>
  </Function>
  <!-- positive: function takes a row(String, Int) returns a scalar-->
  <Function Name=""F_In_RT_String_Int_Ret_ST"" ReturnType=""Int32"">
    <Parameter Name=""pRow"">
      <RowType>
        <Property Name=""X"" Type=""String""/>
        <Property Name=""Y"" Type=""Int32""/>
      </RowType>
    </Parameter>
    <DefiningExpression>
      using ProductModel;
      pRow.Y+3
    </DefiningExpression>
  </Function>
  <!-- positive: function takes a collection of rows returns a collection of rows -->
  <Function Name=""F_In_ColRT_Ret_ColRT"">
    <ReturnType>
      <CollectionType>
        <RowType>
          <Property Name=""X"" Type=""Int32""/>
          <Property Name=""Y"" Type=""String""/>
          <Property Name=""AA"" Type=""Self.Address""/>
        </RowType>
      </CollectionType>
    </ReturnType>
    <Parameter Name=""pRows"">
      <CollectionType>
        <RowType>
          <Property Name=""Cust"" Type=""Self.Customer""/>
        </RowType>
      </CollectionType>
    </Parameter>
    <DefiningExpression>
      using ProductModel;
      select LENGTH(row.Cust.CustomerID) as X, row.Cust.Address.City as Y, row.Cust.Address as AA
      from pRows as row
    </DefiningExpression>
  </Function>
  <!-- positive: function takes a Entity Type returns a reference -->
  <Function Name=""F_In_ET_Ret_RefET"">
    <ReturnType>
      <ReferenceType Type=""Self.Product""/>
    </ReturnType>
    <Parameter Name=""productArg"" Type=""Self.Product""/>
    <DefiningExpression>
      using ProductModel;
      REF(productArg)
    </DefiningExpression>
  </Function>
  <!-- positive: function takes a Entity Type returns a reference -->
  <Function Name=""F_In_ET_Ret_RefET"">
    <ReturnType>
      <ReferenceType Type=""Self.Customer""/>
    </ReturnType>
    <Parameter Name=""custArg"" Type=""Self.Customer""/>
    <DefiningExpression>
      using ProductModel;
      REF(custArg)
    </DefiningExpression>
  </Function>
  <!-- positive: function takes a collection of references returns a collection of references -->
  <Function Name=""F_In_ColRefET_Ret_ColRefET"">
    <ReturnType>
      <CollectionType>
        <ReferenceType Type=""Self.Customer""/>
      </CollectionType>
    </ReturnType>
    <Parameter Name=""customerRefs"">
      <CollectionType>
        <ReferenceType Type=""Self.Customer""/>
      </CollectionType>
    </Parameter>
    <DefiningExpression>
      using ProductModel;
      select value r from customerRefs as r
    </DefiningExpression>
  </Function>
  <!-- positive: make sure that row field names are ignored during function resolution and type comparisons within the function definition-->
  <Function Name=""F_RowFieldNamessIgnored"">
    <ReturnType>
      <CollectionType>
        <RowType>
          <Property Name=""A"" Type=""Int32""/>
          <Property Name=""B"" Type=""String""/>
        </RowType>
      </CollectionType>
    </ReturnType>
    <Parameter Name=""pRows"">
      <CollectionType>
        <RowType>
          <Property Name=""E"" Type=""Int32""/>
          <Property Name=""F"" Type=""String""/>
        </RowType>
      </CollectionType>
    </Parameter>
    <DefiningExpression>
      select
      r.E + 1 as k,
      r.F + '1' as l
      from pRows as r
      where
      r.E = 1 and
      r.F = '1'
    </DefiningExpression>
  </Function>
  <!-- overload resolution: primitive promotion/ranking -->
  <Function Name=""F_In_Number"" ReturnType=""String"">
    <Parameter Name=""p"" Type=""Int16""/>
    <DefiningExpression>
      '16'
    </DefiningExpression>
  </Function>
  <Function Name=""F_In_Number"" ReturnType=""String"">
    <Parameter Name=""p"" Type=""Int32""/>
    <DefiningExpression>
      '32'
    </DefiningExpression>
  </Function>
  <Function Name=""F_In_Number"" ReturnType=""String"">
    <Parameter Name=""p"" Type=""Decimal""/>
    <DefiningExpression>
      'Decimal'
    </DefiningExpression>
  </Function>
  <!--  overload resolution: Primitive types promotion/ranking -->
  <Function Name=""F_In_ST_1"" ReturnType=""Int32"">
    <Parameter Name=""p1"" Type=""Int16""/>
    <Parameter Name=""p2"" Type=""Single""/>
    <DefiningExpression>
      1
    </DefiningExpression>
  </Function>
  <!--  overload resolution: Primitive types promotion/ranking -->
  <Function Name=""F_In_ST_1"" ReturnType=""Int32"">
    <Parameter Name=""p1"" Type=""Int64""/>
    <Parameter Name=""p2"" Type=""Int64""/>
    <DefiningExpression>
      2
    </DefiningExpression>
  </Function>
  <!--  overload resolution: Primitive types promotion/ranking -->
  <Function Name=""F_In_ST_1"" ReturnType=""Int32"">
    <Parameter Name=""p1"" Type=""Single""/>
    <Parameter Name=""p2"" Type=""Int16""/>
    <DefiningExpression>
      3
    </DefiningExpression>
  </Function>
  <Function Name=""F_In_ST_2"" ReturnType=""Int32"">
    <Parameter Name=""p1"" Type=""Byte""/>
    <Parameter Name=""p2"" Type=""Int64""/>
    <DefiningExpression>
      1
    </DefiningExpression>
  </Function>
  <!--  overload resolution: Primitive types promotion/ranking -->
  <Function Name=""F_In_ST_2"" ReturnType=""Int32"">
    <Parameter Name=""p1"" Type=""Int32""/>
    <Parameter Name=""p2"" Type=""Double""/>
    <DefiningExpression>
      2
    </DefiningExpression>
  </Function>
  <!-- overload resolution: entity_inheritance promotion/ranking -->
  <Function Name=""F_In_Entity"" ReturnType=""String"">
    <Parameter Name=""p"" Type=""Self.Product""/>
    <DefiningExpression>
      'Product'
    </DefiningExpression>
  </Function>
  <Function Name=""F_In_Entity"" ReturnType=""String"">
    <Parameter Name=""p"" Type=""Self.DiscontinuedProduct""/>
    <DefiningExpression>
      'DiscontinuedProduct'
    </DefiningExpression>
  </Function>
  <Function Name=""F_In_Entity2"" ReturnType=""String"">
    <Parameter Name=""p"" Type=""Self.Product""/>
    <DefiningExpression>
      'Product'
    </DefiningExpression>
  </Function>
  <!-- overload resolution: primitive/entity_inheritance promotion/ranking -->
  <Function Name=""F_In_ProdNumber"" ReturnType=""String"">
    <Parameter Name=""p1"" Type=""Self.Product""/>
    <Parameter Name=""p2"" Type=""Int32""/>
    <DefiningExpression>
      'Prod-32'
    </DefiningExpression>
  </Function>
  <Function Name=""F_In_ProdNumber"" ReturnType=""String"">
    <Parameter Name=""p1"" Type=""Self.DiscontinuedProduct""/>
    <Parameter Name=""p2"" Type=""Int16""/>
    <DefiningExpression>
      'DiscProd-16'
    </DefiningExpression>
  </Function>
  <Function Name=""F_In_ProdNumber2"" ReturnType=""String"">
    <Parameter Name=""p1"" Type=""Self.DiscontinuedProduct""/>
    <Parameter Name=""p2"" Type=""Decimal""/>
    <DefiningExpression>
      'DiscProd-Decimal'
    </DefiningExpression>
  </Function>
  <Function Name=""F_In_ProdNumber2"" ReturnType=""String"">
    <Parameter Name=""p1"" Type=""Self.Product""/>
    <Parameter Name=""p2"" Type=""Int32""/>
    <DefiningExpression>
      'Prod-32'
    </DefiningExpression>
  </Function>
  <Function Name=""F_In_ProdNumber3"" ReturnType=""String"">
    <Parameter Name=""p1"" Type=""Self.Product""/>
    <Parameter Name=""p2"" Type=""Int32""/>
    <DefiningExpression>
      'Prod-32'
    </DefiningExpression>
  </Function>
  <!-- overload resolution: ref/entity_inheritance promotion/ranking -->
  <Function Name=""F_In_Ref"" ReturnType=""String"">
    <Parameter Name=""p"">
      <ReferenceType Type=""Self.Product""/>
    </Parameter>
    <DefiningExpression>
      'Ref(Product)'
    </DefiningExpression>
  </Function>
  <Function Name=""F_In_Ref"" ReturnType=""String"">
    <Parameter Name=""p"">
      <ReferenceType Type=""Self.DiscontinuedProduct""/>
    </Parameter>
    <DefiningExpression>
      'Ref(DiscontinuedProduct)'
    </DefiningExpression>
  </Function>
  <Function Name=""F_In_Ref2"" ReturnType=""String"">
    <Parameter Name=""p"">
      <ReferenceType Type=""Self.Product""/>
    </Parameter>
    <DefiningExpression>
      'Ref(Product)'
    </DefiningExpression>
  </Function>
  <!-- overload resolution: row promotion/ranking -->
  <Function Name=""F_In_Row"" ReturnType=""String"">
    <Parameter Name=""p"">
      <RowType>
        <Property Name=""p1"" Type=""Self.DiscontinuedProduct""/>
        <Property Name=""p2"" Type=""Decimal""/>
      </RowType>
    </Parameter>
    <DefiningExpression>
      'Row(DiscProd,Decimal)'
    </DefiningExpression>
  </Function>
  <Function Name=""F_In_Row"" ReturnType=""String"">
    <Parameter Name=""p"">
      <RowType>
        <Property Name=""p1"" Type=""Self.Product""/>
        <Property Name=""p2"" Type=""Int32""/>
      </RowType>
    </Parameter>
    <DefiningExpression>
      'Row(Prod,32)'
    </DefiningExpression>
  </Function>
  <Function Name=""F_In_Row2"" ReturnType=""String"">
    <Parameter Name=""p1"" Type=""Int32""/>
    <Parameter Name=""p2"">
      <RowType>
        <Property Name=""p1"" Type=""Self.Product""/>
        <Property Name=""p2"" Type=""Int32""/>
      </RowType>
    </Parameter>
    <Parameter Name=""p3"" Type=""Int32""/>
    <DefiningExpression>
      '32-r-32'
    </DefiningExpression>
  </Function>
  <Function Name=""F_In_Row2"" ReturnType=""String"">
    <Parameter Name=""p1"" Type=""Decimal""/>
    <Parameter Name=""p2"">
      <RowType>
        <Property Name=""p1"" Type=""Self.Product""/>
        <Property Name=""p2"" Type=""Int32""/>
      </RowType>
    </Parameter>
    <Parameter Name=""p3"" Type=""Decimal""/>
    <DefiningExpression>
      'Decimal-r-Decimal'
    </DefiningExpression>
  </Function>
  <!-- overload resolution: col(row) promotion/ranking -->
  <Function Name=""F_In_ColRow"" ReturnType=""String"">
    <Parameter Name=""p"">
      <CollectionType>
        <RowType>
          <Property Name=""p1"" Type=""Self.DiscontinuedProduct""/>
          <Property Name=""p2"" Type=""Decimal""/>
        </RowType>
      </CollectionType>
    </Parameter>
    <DefiningExpression>
      'Col(Row(DiscProd,Decimal))'
    </DefiningExpression>
  </Function>
  <Function Name=""F_In_ColRow"" ReturnType=""String"">
    <Parameter Name=""p"">
      <CollectionType>
        <RowType>
          <Property Name=""p1"" Type=""Self.Product""/>
          <Property Name=""p2"" Type=""Int32""/>
        </RowType>
      </CollectionType>
    </Parameter>
    <DefiningExpression>
      'Col(Row(Prod,32))'
    </DefiningExpression>
  </Function>
  <Function Name=""F_In_ColRow2"" ReturnType=""String"">
    <Parameter Name=""p1"" Type=""Decimal""/>
    <Parameter Name=""p2"">
      <CollectionType>
        <RowType>
          <Property Name=""p1"" Type=""Self.Product""/>
          <Property Name=""p2"" Type=""Int32""/>
        </RowType>
      </CollectionType>
    </Parameter>
    <Parameter Name=""p3"" Type=""Decimal""/>
    <DefiningExpression>
      'Decimal-c(r)-Decimal'
    </DefiningExpression>
  </Function>
  <!-- positive(regression): function returns a boolean value -->
  <Function Name=""F_Ret_SBool"" ReturnType=""Boolean"">
    <DefiningExpression>
      <![CDATA[10 > 20]]>
    </DefiningExpression>
  </Function>";

        public static readonly string csdl = string.Format(csdlTemplate, "");

        public static readonly string csdlWithFunctions = string.Format(csdlTemplate, modelDefinedFunctions);
    }
}
