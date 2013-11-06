<StorageAndMappings>
  <Schema Namespace="NorthwindModel.Store" Alias="Self" Provider="System.Data.SqlClient" ProviderManifestToken="2005" xmlns:store="http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator" xmlns="http://schemas.microsoft.com/ado/2006/04/edm/ssdl">
  <EntityContainer Name="NorthwindModelStoreContainer">
    <EntitySet Name="Categories" EntityType="NorthwindModel.Store.Categories" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="CustomerDemographics" EntityType="NorthwindModel.Store.CustomerDemographics" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="CustomerHistory" EntityType="NorthwindModel.Store.CustomerHistory" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="Customers" EntityType="NorthwindModel.Store.Customers" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="DocumentStorage" EntityType="NorthwindModel.Store.DocumentStorage" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="Employees" EntityType="NorthwindModel.Store.Employees" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="Order_Details" EntityType="NorthwindModel.Store.Order_Details" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="OrderHistory" EntityType="NorthwindModel.Store.OrderHistory" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="Orders" EntityType="NorthwindModel.Store.Orders" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="Products" EntityType="NorthwindModel.Store.Products" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="Region" EntityType="NorthwindModel.Store.Region" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="Shippers" EntityType="NorthwindModel.Store.Shippers" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="Suppliers" EntityType="NorthwindModel.Store.Suppliers" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="Territories" EntityType="NorthwindModel.Store.Territories" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="CustomerCustomerDemo" EntityType="NorthwindModel.Store.CustomerCustomerDemo" store:Type="Tables" Schema="dbo" />
    <EntitySet Name="EmployeeTerritories" EntityType="NorthwindModel.Store.EmployeeTerritories" store:Type="Tables" Schema="dbo" />
    <AssociationSet Name="FK_Products_Categories" Association="NorthwindModel.Store.FK_Products_Categories">
      <End Role="Categories" EntitySet="Categories" />
      <End Role="Products" EntitySet="Products" />
    </AssociationSet>
    <AssociationSet Name="FK_Orders_Customers" Association="NorthwindModel.Store.FK_Orders_Customers">
      <End Role="Customers" EntitySet="Customers" />
      <End Role="Orders" EntitySet="Orders" />
    </AssociationSet>
    <AssociationSet Name="FK_Employees_Employees" Association="NorthwindModel.Store.FK_Employees_Employees">
      <End Role="Employees" EntitySet="Employees" />
      <End Role="Employees1" EntitySet="Employees" />
    </AssociationSet>
    <AssociationSet Name="FK_Orders_Employees" Association="NorthwindModel.Store.FK_Orders_Employees">
      <End Role="Employees" EntitySet="Employees" />
      <End Role="Orders" EntitySet="Orders" />
    </AssociationSet>
    <AssociationSet Name="FK_Order_Details_Orders" Association="NorthwindModel.Store.FK_Order_Details_Orders">
      <End Role="Orders" EntitySet="Orders" />
      <End Role="Order_Details" EntitySet="Order_Details" />
    </AssociationSet>
    <AssociationSet Name="FK_Order_Details_Products" Association="NorthwindModel.Store.FK_Order_Details_Products">
      <End Role="Products" EntitySet="Products" />
      <End Role="Order_Details" EntitySet="Order_Details" />
    </AssociationSet>
    <AssociationSet Name="FK_Orders_Shippers" Association="NorthwindModel.Store.FK_Orders_Shippers">
      <End Role="Shippers" EntitySet="Shippers" />
      <End Role="Orders" EntitySet="Orders" />
    </AssociationSet>
    <AssociationSet Name="FK_Products_Suppliers" Association="NorthwindModel.Store.FK_Products_Suppliers">
      <End Role="Suppliers" EntitySet="Suppliers" />
      <End Role="Products" EntitySet="Products" />
    </AssociationSet>
    <AssociationSet Name="FK_Territories_Region" Association="NorthwindModel.Store.FK_Territories_Region">
      <End Role="Region" EntitySet="Region" />
      <End Role="Territories" EntitySet="Territories" />
    </AssociationSet>
    <AssociationSet Name="FK_CustomerCustomerDemo_CustomerDemographics" Association="NorthwindModel.Store.FK_CustomerCustomerDemo_CustomerDemographics">
      <End Role="CustomerDemographics" EntitySet="CustomerDemographics" />
      <End Role="CustomerCustomerDemo" EntitySet="CustomerCustomerDemo" />
    </AssociationSet>
    <AssociationSet Name="FK_CustomerCustomerDemo_Customers" Association="NorthwindModel.Store.FK_CustomerCustomerDemo_Customers">
      <End Role="Customers" EntitySet="Customers" />
      <End Role="CustomerCustomerDemo" EntitySet="CustomerCustomerDemo" />
    </AssociationSet>
    <AssociationSet Name="FK_EmployeeTerritories_Employees" Association="NorthwindModel.Store.FK_EmployeeTerritories_Employees">
      <End Role="Employees" EntitySet="Employees" />
      <End Role="EmployeeTerritories" EntitySet="EmployeeTerritories" />
    </AssociationSet>
    <AssociationSet Name="FK_EmployeeTerritories_Territories" Association="NorthwindModel.Store.FK_EmployeeTerritories_Territories">
      <End Role="Territories" EntitySet="Territories" />
      <End Role="EmployeeTerritories" EntitySet="EmployeeTerritories" />
    </AssociationSet>
  </EntityContainer>
  <EntityType Name="Categories">
    <Key>
      <PropertyRef Name="CategoryID" />
    </Key>
    <Property Name="CategoryID" Type="int" Nullable="false" />
    <Property Name="CategoryName" Type="nvarchar" Nullable="false" MaxLength="15" />
    <Property Name="Description" Type="nvarchar(max)" Nullable="true" />
    <Property Name="Picture" Type="varbinary(max)" Nullable="true" />
  </EntityType>
  <EntityType Name="CustomerDemographics">
    <Key>
      <PropertyRef Name="CustomerTypeID" />
    </Key>
    <Property Name="CustomerTypeID" Type="nchar" Nullable="false" MaxLength="10" />
    <Property Name="CustomerDesc" Type="nvarchar(max)" Nullable="true" />
  </EntityType>
  <EntityType Name="CustomerHistory">
    <Key>
      <PropertyRef Name="CustomerID" />
    </Key>
    <Property Name="CustomerID" Type="nchar" Nullable="false" MaxLength="5" />
    <Property Name="CompanyName" Type="nvarchar" Nullable="false" MaxLength="40" />
    <Property Name="ContactName" Type="nvarchar" Nullable="true" MaxLength="30" />
    <Property Name="ContactTitle" Type="nvarchar" Nullable="true" MaxLength="30" />
    <Property Name="Address" Type="nvarchar" Nullable="true" MaxLength="60" />
    <Property Name="City" Type="nvarchar" Nullable="true" MaxLength="15" />
    <Property Name="Region" Type="nvarchar" Nullable="true" MaxLength="15" />
    <Property Name="PostalCode" Type="nvarchar" Nullable="true" MaxLength="10" />
    <Property Name="Country" Type="nvarchar" Nullable="true" MaxLength="15" />
    <Property Name="Phone" Type="nvarchar" Nullable="true" MaxLength="24" />
    <Property Name="Fax" Type="nvarchar" Nullable="true" MaxLength="24" />
  </EntityType>
  <EntityType Name="Customers">
    <Key>
      <PropertyRef Name="CustomerID" />
    </Key>
    <Property Name="CustomerID" Type="nchar" Nullable="false" MaxLength="5" />
    <Property Name="CompanyName" Type="nvarchar" Nullable="false" MaxLength="40" />
    <Property Name="ContactName" Type="nvarchar" Nullable="true" MaxLength="30" />
    <Property Name="ContactTitle" Type="nvarchar" Nullable="true" MaxLength="30" />
    <Property Name="Address" Type="nvarchar" Nullable="true" MaxLength="60" />
    <Property Name="City" Type="nvarchar" Nullable="true" MaxLength="15" />
    <Property Name="Region" Type="nvarchar" Nullable="true" MaxLength="15" />
    <Property Name="PostalCode" Type="nvarchar" Nullable="true" MaxLength="10" />
    <Property Name="Country" Type="nvarchar" Nullable="true" MaxLength="15" />
    <Property Name="Phone" Type="nvarchar" Nullable="true" MaxLength="24" />
    <Property Name="Fax" Type="nvarchar" Nullable="true" MaxLength="24" />
  </EntityType>
  <EntityType Name="DocumentStorage">
    <Key>
      <PropertyRef Name="DocumentID" />
      <PropertyRef Name="FileName" />
    </Key>
    <Property Name="DocumentID" Type="int" Nullable="false" />
    <Property Name="FileName" Type="nvarchar" Nullable="false" MaxLength="255" />
    <Property Name="DocumentFile" Type="varbinary(max)" Nullable="false" />
  </EntityType>
  <EntityType Name="Employees">
    <Key>
      <PropertyRef Name="EmployeeID" />
    </Key>
    <Property Name="EmployeeID" Type="int" Nullable="false" />
    <Property Name="LastName" Type="nvarchar" Nullable="false" MaxLength="20" />
    <Property Name="FirstName" Type="nvarchar" Nullable="false" MaxLength="10" />
    <Property Name="Title" Type="nvarchar" Nullable="true" MaxLength="30" />
    <Property Name="TitleOfCourtesy" Type="nvarchar" Nullable="true" MaxLength="25" />
    <Property Name="BirthDate" Type="datetime" Nullable="true" />
    <Property Name="HireDate" Type="datetime" Nullable="true" />
    <Property Name="Address" Type="nvarchar" Nullable="true" MaxLength="60" />
    <Property Name="City" Type="nvarchar" Nullable="true" MaxLength="15" />
    <Property Name="Region" Type="nvarchar" Nullable="true" MaxLength="15" />
    <Property Name="PostalCode" Type="nvarchar" Nullable="true" MaxLength="10" />
    <Property Name="Country" Type="nvarchar" Nullable="true" MaxLength="15" />
    <Property Name="HomePhone" Type="nvarchar" Nullable="true" MaxLength="24" />
    <Property Name="Extension" Type="nvarchar" Nullable="true" MaxLength="4" />
    <Property Name="Photo" Type="varbinary(max)" Nullable="true" />
    <Property Name="Notes" Type="nvarchar(max)" Nullable="true" />
    <Property Name="PhotoPath" Type="nvarchar" Nullable="true" MaxLength="255" />
    <Property Name="Employees2_EmployeeID" Type="int" Nullable="true" />
  </EntityType>
  <EntityType Name="Order_Details">
    <Key>
      <PropertyRef Name="OrderID" />
      <PropertyRef Name="ProductID" />
    </Key>
    <Property Name="OrderID" Type="int" Nullable="false" />
    <Property Name="ProductID" Type="int" Nullable="false" />
    <Property Name="UnitPrice" Type="decimal" Nullable="false" Precision="19" Scale="4" />
    <Property Name="Quantity" Type="smallint" Nullable="false" />
    <Property Name="Discount" Type="real" Nullable="false" />
  </EntityType>
  <EntityType Name="OrderHistory">
    <Key>
      <PropertyRef Name="OrderID" />
    </Key>
    <Property Name="OrderID" Type="int" Nullable="false" />
    <Property Name="CustomerID" Type="nchar" Nullable="true" MaxLength="5" />
    <Property Name="EmployeeID" Type="int" Nullable="true" />
    <Property Name="OrderDate" Type="datetime" Nullable="true" />
    <Property Name="RequiredDate" Type="datetime" Nullable="true" />
    <Property Name="ShippedDate" Type="datetime" Nullable="true" />
    <Property Name="ShipVia" Type="int" Nullable="true" />
    <Property Name="Freight" Type="decimal" Nullable="true" Precision="19" Scale="4" />
    <Property Name="ShipName" Type="nvarchar" Nullable="true" MaxLength="40" />
    <Property Name="ShipAddress" Type="nvarchar" Nullable="true" MaxLength="60" />
    <Property Name="ShipCity" Type="nvarchar" Nullable="true" MaxLength="15" />
    <Property Name="ShipRegion" Type="nvarchar" Nullable="true" MaxLength="15" />
    <Property Name="ShipPostalCode" Type="nvarchar" Nullable="true" MaxLength="10" />
    <Property Name="ShipCountry" Type="nvarchar" Nullable="true" MaxLength="15" />
  </EntityType>
  <EntityType Name="Orders">
    <Key>
      <PropertyRef Name="OrderID" />
    </Key>
    <Property Name="OrderID" Type="int" Nullable="false" />
    <Property Name="OrderDate" Type="datetime" Nullable="true" />
    <Property Name="RequiredDate" Type="datetime" Nullable="true" />
    <Property Name="ShippedDate" Type="datetime" Nullable="true" />
    <Property Name="Freight" Type="decimal" Nullable="true" Precision="19" Scale="4" />
    <Property Name="ShipName" Type="nvarchar" Nullable="true" MaxLength="40" />
    <Property Name="ShipAddress" Type="nvarchar" Nullable="true" MaxLength="60" />
    <Property Name="ShipCity" Type="nvarchar" Nullable="true" MaxLength="15" />
    <Property Name="ShipRegion" Type="nvarchar" Nullable="true" MaxLength="15" />
    <Property Name="ShipPostalCode" Type="nvarchar" Nullable="true" MaxLength="10" />
    <Property Name="ShipCountry" Type="nvarchar" Nullable="true" MaxLength="15" />
    <Property Name="Customers_CustomerID" Type="nchar" MaxLength="5" Nullable="true" />
    <Property Name="Employees_EmployeeID" Type="int" Nullable="true" />
    <Property Name="Shippers_ShipperID" Type="int" Nullable="true" />
  </EntityType>
  <EntityType Name="Products">
    <Key>
      <PropertyRef Name="ProductID" />
    </Key>
    <Property Name="ProductID" Type="int" Nullable="false" />
    <Property Name="ProductName" Type="nvarchar" Nullable="false" MaxLength="40" />
    <Property Name="QuantityPerUnit" Type="nvarchar" Nullable="true" MaxLength="20" />
    <Property Name="UnitPrice" Type="decimal" Nullable="true" Precision="19" Scale="4" />
    <Property Name="UnitsInStock" Type="smallint" Nullable="true" />
    <Property Name="UnitsOnOrder" Type="smallint" Nullable="true" />
    <Property Name="ReorderLevel" Type="smallint" Nullable="true" />
    <Property Name="Discontinued" Type="bit" Nullable="false" />
    <Property Name="Categories_CategoryID" Type="int" Nullable="true" />
    <Property Name="Suppliers_SupplierID" Type="int" Nullable="true" />
  </EntityType>
  <EntityType Name="Region">
    <Key>
      <PropertyRef Name="RegionID" />
    </Key>
    <Property Name="RegionID" Type="int" Nullable="false" />
    <Property Name="RegionDescription" Type="nchar" Nullable="false" MaxLength="50" />
  </EntityType>
  <EntityType Name="Shippers">
    <Key>
      <PropertyRef Name="ShipperID" />
    </Key>
    <Property Name="ShipperID" Type="int" Nullable="false" />
    <Property Name="CompanyName" Type="nvarchar" Nullable="false" MaxLength="40" />
    <Property Name="Phone" Type="nvarchar" Nullable="true" MaxLength="24" />
  </EntityType>
  <EntityType Name="Suppliers">
    <Key>
      <PropertyRef Name="SupplierID" />
    </Key>
    <Property Name="SupplierID" Type="int" Nullable="false" />
    <Property Name="CompanyName" Type="nvarchar" Nullable="false" MaxLength="40" />
    <Property Name="ContactName" Type="nvarchar" Nullable="true" MaxLength="30" />
    <Property Name="ContactTitle" Type="nvarchar" Nullable="true" MaxLength="30" />
    <Property Name="Address" Type="nvarchar" Nullable="true" MaxLength="60" />
    <Property Name="City" Type="nvarchar" Nullable="true" MaxLength="15" />
    <Property Name="Region" Type="nvarchar" Nullable="true" MaxLength="15" />
    <Property Name="PostalCode" Type="nvarchar" Nullable="true" MaxLength="10" />
    <Property Name="Country" Type="nvarchar" Nullable="true" MaxLength="15" />
    <Property Name="Phone" Type="nvarchar" Nullable="true" MaxLength="24" />
    <Property Name="Fax" Type="nvarchar" Nullable="true" MaxLength="24" />
    <Property Name="HomePage" Type="nvarchar(max)" Nullable="true" />
  </EntityType>
  <EntityType Name="Territories">
    <Key>
      <PropertyRef Name="TerritoryID" />
    </Key>
    <Property Name="TerritoryID" Type="nvarchar" Nullable="false" MaxLength="20" />
    <Property Name="TerritoryDescription" Type="nchar" Nullable="false" MaxLength="50" />
    <Property Name="Region_RegionID" Type="int" Nullable="false" />
  </EntityType>
  <EntityType Name="CustomerCustomerDemo">
    <Key>
      <PropertyRef Name="CustomerDemographics_CustomerTypeID" />
      <PropertyRef Name="Customers_CustomerID" />
    </Key>
    <Property Name="CustomerDemographics_CustomerTypeID" Type="nchar" Nullable="false" MaxLength="10" />
    <Property Name="Customers_CustomerID" Type="nchar" Nullable="false" MaxLength="5" />
  </EntityType>
  <EntityType Name="EmployeeTerritories">
    <Key>
      <PropertyRef Name="Employees_EmployeeID" />
      <PropertyRef Name="Territories_TerritoryID" />
    </Key>
    <Property Name="Employees_EmployeeID" Type="int" Nullable="false" />
    <Property Name="Territories_TerritoryID" Type="nvarchar" Nullable="false" MaxLength="20" />
  </EntityType>
  <Association Name="FK_Products_Categories">
    <End Role="Categories" Type="NorthwindModel.Store.Categories" Multiplicity="0..1" />
    <End Role="Products" Type="NorthwindModel.Store.Products" Multiplicity="*" />
    <ReferentialConstraint>
      <Principal Role="Categories">
        <PropertyRef Name="CategoryID" />
      </Principal>
      <Dependent Role="Products">
        <PropertyRef Name="Categories_CategoryID" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="FK_Orders_Customers">
    <End Role="Customers" Type="NorthwindModel.Store.Customers" Multiplicity="0..1" />
    <End Role="Orders" Type="NorthwindModel.Store.Orders" Multiplicity="*" />
    <ReferentialConstraint>
      <Principal Role="Customers">
        <PropertyRef Name="CustomerID" />
      </Principal>
      <Dependent Role="Orders">
        <PropertyRef Name="Customers_CustomerID" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="FK_Employees_Employees">
    <End Role="Employees" Type="NorthwindModel.Store.Employees" Multiplicity="0..1" />
    <End Role="Employees1" Type="NorthwindModel.Store.Employees" Multiplicity="*" />
    <ReferentialConstraint>
      <Principal Role="Employees">
        <PropertyRef Name="EmployeeID" />
      </Principal>
      <Dependent Role="Employees1">
        <PropertyRef Name="Employees2_EmployeeID" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="FK_Orders_Employees">
    <End Role="Employees" Type="NorthwindModel.Store.Employees" Multiplicity="0..1" />
    <End Role="Orders" Type="NorthwindModel.Store.Orders" Multiplicity="*" />
    <ReferentialConstraint>
      <Principal Role="Employees">
        <PropertyRef Name="EmployeeID" />
      </Principal>
      <Dependent Role="Orders">
        <PropertyRef Name="Employees_EmployeeID" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="FK_Order_Details_Orders">
    <End Role="Orders" Type="NorthwindModel.Store.Orders" Multiplicity="1" />
    <End Role="Order_Details" Type="NorthwindModel.Store.Order_Details" Multiplicity="*" />
    <ReferentialConstraint>
      <Principal Role="Orders">
        <PropertyRef Name="OrderID" />
      </Principal>
      <Dependent Role="Order_Details">
        <PropertyRef Name="OrderID" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="FK_Order_Details_Products">
    <End Role="Products" Type="NorthwindModel.Store.Products" Multiplicity="1" />
    <End Role="Order_Details" Type="NorthwindModel.Store.Order_Details" Multiplicity="*" />
    <ReferentialConstraint>
      <Principal Role="Products">
        <PropertyRef Name="ProductID" />
      </Principal>
      <Dependent Role="Order_Details">
        <PropertyRef Name="ProductID" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="FK_Orders_Shippers">
    <End Role="Shippers" Type="NorthwindModel.Store.Shippers" Multiplicity="0..1" />
    <End Role="Orders" Type="NorthwindModel.Store.Orders" Multiplicity="*" />
    <ReferentialConstraint>
      <Principal Role="Shippers">
        <PropertyRef Name="ShipperID" />
      </Principal>
      <Dependent Role="Orders">
        <PropertyRef Name="Shippers_ShipperID" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="FK_Products_Suppliers">
    <End Role="Suppliers" Type="NorthwindModel.Store.Suppliers" Multiplicity="0..1" />
    <End Role="Products" Type="NorthwindModel.Store.Products" Multiplicity="*" />
    <ReferentialConstraint>
      <Principal Role="Suppliers">
        <PropertyRef Name="SupplierID" />
      </Principal>
      <Dependent Role="Products">
        <PropertyRef Name="Suppliers_SupplierID" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="FK_Territories_Region">
    <End Role="Region" Type="NorthwindModel.Store.Region" Multiplicity="1" />
    <End Role="Territories" Type="NorthwindModel.Store.Territories" Multiplicity="*" />
    <ReferentialConstraint>
      <Principal Role="Region">
        <PropertyRef Name="RegionID" />
      </Principal>
      <Dependent Role="Territories">
        <PropertyRef Name="Region_RegionID" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="FK_CustomerCustomerDemo_CustomerDemographics">
    <End Role="CustomerDemographics" Type="NorthwindModel.Store.CustomerDemographics" Multiplicity="1" />
    <End Role="CustomerCustomerDemo" Type="NorthwindModel.Store.CustomerCustomerDemo" Multiplicity="*" />
    <ReferentialConstraint>
      <Principal Role="CustomerDemographics">
        <PropertyRef Name="CustomerTypeID" />
      </Principal>
      <Dependent Role="CustomerCustomerDemo">
        <PropertyRef Name="CustomerDemographics_CustomerTypeID" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="FK_CustomerCustomerDemo_Customers">
    <End Role="CustomerCustomerDemo" Type="NorthwindModel.Store.CustomerCustomerDemo" Multiplicity="*" />
    <End Role="Customers" Type="NorthwindModel.Store.Customers" Multiplicity="1" />
    <ReferentialConstraint>
      <Principal Role="Customers">
        <PropertyRef Name="CustomerID" />
      </Principal>
      <Dependent Role="CustomerCustomerDemo">
        <PropertyRef Name="Customers_CustomerID" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="FK_EmployeeTerritories_Employees">
    <End Role="Employees" Type="NorthwindModel.Store.Employees" Multiplicity="1" />
    <End Role="EmployeeTerritories" Type="NorthwindModel.Store.EmployeeTerritories" Multiplicity="*" />
    <ReferentialConstraint>
      <Principal Role="Employees">
        <PropertyRef Name="EmployeeID" />
      </Principal>
      <Dependent Role="EmployeeTerritories">
        <PropertyRef Name="Employees_EmployeeID" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name="FK_EmployeeTerritories_Territories">
    <End Role="EmployeeTerritories" Type="NorthwindModel.Store.EmployeeTerritories" Multiplicity="*" />
    <End Role="Territories" Type="NorthwindModel.Store.Territories" Multiplicity="1" />
    <ReferentialConstraint>
      <Principal Role="Territories">
        <PropertyRef Name="TerritoryID" />
      </Principal>
      <Dependent Role="EmployeeTerritories">
        <PropertyRef Name="Territories_TerritoryID" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
</Schema>

<!--Finished generating the storage layer. Here are the mappings:-->

<Mapping Space="C-S" xmlns="urn:schemas-microsoft-com:windows:storage:mapping:CS">
  <EntityContainerMapping StorageEntityContainer="NorthwindModelStoreContainer" CdmEntityContainer="NorthwindEntities1">
    <EntitySetMapping Name="Categories">
      <EntityTypeMapping TypeName="IsTypeOf(NorthwindModel.Categories)">
        <MappingFragment StoreEntitySet="Categories">
          <ScalarProperty Name="CategoryID" ColumnName="CategoryID" />
          <ScalarProperty Name="CategoryName" ColumnName="CategoryName" />
          <ScalarProperty Name="Description" ColumnName="Description" />
          <ScalarProperty Name="Picture" ColumnName="Picture" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name="CustomerDemographics">
      <EntityTypeMapping TypeName="IsTypeOf(NorthwindModel.CustomerDemographics)">
        <MappingFragment StoreEntitySet="CustomerDemographics">
          <ScalarProperty Name="CustomerTypeID" ColumnName="CustomerTypeID" />
          <ScalarProperty Name="CustomerDesc" ColumnName="CustomerDesc" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name="CustomerHistory">
      <EntityTypeMapping TypeName="IsTypeOf(NorthwindModel.CustomerHistory)">
        <MappingFragment StoreEntitySet="CustomerHistory">
          <ScalarProperty Name="CustomerID" ColumnName="CustomerID" />
          <ScalarProperty Name="CompanyName" ColumnName="CompanyName" />
          <ScalarProperty Name="ContactName" ColumnName="ContactName" />
          <ScalarProperty Name="ContactTitle" ColumnName="ContactTitle" />
          <ScalarProperty Name="Address" ColumnName="Address" />
          <ScalarProperty Name="City" ColumnName="City" />
          <ScalarProperty Name="Region" ColumnName="Region" />
          <ScalarProperty Name="PostalCode" ColumnName="PostalCode" />
          <ScalarProperty Name="Country" ColumnName="Country" />
          <ScalarProperty Name="Phone" ColumnName="Phone" />
          <ScalarProperty Name="Fax" ColumnName="Fax" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name="Customers">
      <EntityTypeMapping TypeName="IsTypeOf(NorthwindModel.Customers)">
        <MappingFragment StoreEntitySet="Customers">
          <ScalarProperty Name="CustomerID" ColumnName="CustomerID" />
          <ScalarProperty Name="CompanyName" ColumnName="CompanyName" />
          <ScalarProperty Name="ContactName" ColumnName="ContactName" />
          <ScalarProperty Name="ContactTitle" ColumnName="ContactTitle" />
          <ScalarProperty Name="Address" ColumnName="Address" />
          <ScalarProperty Name="City" ColumnName="City" />
          <ScalarProperty Name="Region" ColumnName="Region" />
          <ScalarProperty Name="PostalCode" ColumnName="PostalCode" />
          <ScalarProperty Name="Country" ColumnName="Country" />
          <ScalarProperty Name="Phone" ColumnName="Phone" />
          <ScalarProperty Name="Fax" ColumnName="Fax" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name="DocumentStorage">
      <EntityTypeMapping TypeName="IsTypeOf(NorthwindModel.DocumentStorage)">
        <MappingFragment StoreEntitySet="DocumentStorage">
          <ScalarProperty Name="DocumentID" ColumnName="DocumentID" />
          <ScalarProperty Name="FileName" ColumnName="FileName" />
          <ScalarProperty Name="DocumentFile" ColumnName="DocumentFile" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name="Employees">
      <EntityTypeMapping TypeName="IsTypeOf(NorthwindModel.Employees)">
        <MappingFragment StoreEntitySet="Employees">
          <ScalarProperty Name="EmployeeID" ColumnName="EmployeeID" />
          <ScalarProperty Name="LastName" ColumnName="LastName" />
          <ScalarProperty Name="FirstName" ColumnName="FirstName" />
          <ScalarProperty Name="Title" ColumnName="Title" />
          <ScalarProperty Name="TitleOfCourtesy" ColumnName="TitleOfCourtesy" />
          <ScalarProperty Name="BirthDate" ColumnName="BirthDate" />
          <ScalarProperty Name="HireDate" ColumnName="HireDate" />
          <ScalarProperty Name="Address" ColumnName="Address" />
          <ScalarProperty Name="City" ColumnName="City" />
          <ScalarProperty Name="Region" ColumnName="Region" />
          <ScalarProperty Name="PostalCode" ColumnName="PostalCode" />
          <ScalarProperty Name="Country" ColumnName="Country" />
          <ScalarProperty Name="HomePhone" ColumnName="HomePhone" />
          <ScalarProperty Name="Extension" ColumnName="Extension" />
          <ScalarProperty Name="Photo" ColumnName="Photo" />
          <ScalarProperty Name="Notes" ColumnName="Notes" />
          <ScalarProperty Name="PhotoPath" ColumnName="PhotoPath" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name="Order_Details">
      <EntityTypeMapping TypeName="IsTypeOf(NorthwindModel.Order_Details)">
        <MappingFragment StoreEntitySet="Order_Details">
          <ScalarProperty Name="OrderID" ColumnName="OrderID" />
          <ScalarProperty Name="ProductID" ColumnName="ProductID" />
          <ScalarProperty Name="UnitPrice" ColumnName="UnitPrice" />
          <ScalarProperty Name="Quantity" ColumnName="Quantity" />
          <ScalarProperty Name="Discount" ColumnName="Discount" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name="OrderHistory">
      <EntityTypeMapping TypeName="IsTypeOf(NorthwindModel.OrderHistory)">
        <MappingFragment StoreEntitySet="OrderHistory">
          <ScalarProperty Name="OrderID" ColumnName="OrderID" />
          <ScalarProperty Name="CustomerID" ColumnName="CustomerID" />
          <ScalarProperty Name="EmployeeID" ColumnName="EmployeeID" />
          <ScalarProperty Name="OrderDate" ColumnName="OrderDate" />
          <ScalarProperty Name="RequiredDate" ColumnName="RequiredDate" />
          <ScalarProperty Name="ShippedDate" ColumnName="ShippedDate" />
          <ScalarProperty Name="ShipVia" ColumnName="ShipVia" />
          <ScalarProperty Name="Freight" ColumnName="Freight" />
          <ScalarProperty Name="ShipName" ColumnName="ShipName" />
          <ScalarProperty Name="ShipAddress" ColumnName="ShipAddress" />
          <ScalarProperty Name="ShipCity" ColumnName="ShipCity" />
          <ScalarProperty Name="ShipRegion" ColumnName="ShipRegion" />
          <ScalarProperty Name="ShipPostalCode" ColumnName="ShipPostalCode" />
          <ScalarProperty Name="ShipCountry" ColumnName="ShipCountry" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name="Orders">
      <EntityTypeMapping TypeName="IsTypeOf(NorthwindModel.Orders)">
        <MappingFragment StoreEntitySet="Orders">
          <ScalarProperty Name="OrderID" ColumnName="OrderID" />
          <ScalarProperty Name="OrderDate" ColumnName="OrderDate" />
          <ScalarProperty Name="RequiredDate" ColumnName="RequiredDate" />
          <ScalarProperty Name="ShippedDate" ColumnName="ShippedDate" />
          <ScalarProperty Name="Freight" ColumnName="Freight" />
          <ScalarProperty Name="ShipName" ColumnName="ShipName" />
          <ScalarProperty Name="ShipAddress" ColumnName="ShipAddress" />
          <ScalarProperty Name="ShipCity" ColumnName="ShipCity" />
          <ScalarProperty Name="ShipRegion" ColumnName="ShipRegion" />
          <ScalarProperty Name="ShipPostalCode" ColumnName="ShipPostalCode" />
          <ScalarProperty Name="ShipCountry" ColumnName="ShipCountry" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name="Products">
      <EntityTypeMapping TypeName="IsTypeOf(NorthwindModel.Products)">
        <MappingFragment StoreEntitySet="Products">
          <ScalarProperty Name="ProductID" ColumnName="ProductID" />
          <ScalarProperty Name="ProductName" ColumnName="ProductName" />
          <ScalarProperty Name="QuantityPerUnit" ColumnName="QuantityPerUnit" />
          <ScalarProperty Name="UnitPrice" ColumnName="UnitPrice" />
          <ScalarProperty Name="UnitsInStock" ColumnName="UnitsInStock" />
          <ScalarProperty Name="UnitsOnOrder" ColumnName="UnitsOnOrder" />
          <ScalarProperty Name="ReorderLevel" ColumnName="ReorderLevel" />
          <ScalarProperty Name="Discontinued" ColumnName="Discontinued" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name="Region">
      <EntityTypeMapping TypeName="IsTypeOf(NorthwindModel.Region)">
        <MappingFragment StoreEntitySet="Region">
          <ScalarProperty Name="RegionID" ColumnName="RegionID" />
          <ScalarProperty Name="RegionDescription" ColumnName="RegionDescription" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name="Shippers">
      <EntityTypeMapping TypeName="IsTypeOf(NorthwindModel.Shippers)">
        <MappingFragment StoreEntitySet="Shippers">
          <ScalarProperty Name="ShipperID" ColumnName="ShipperID" />
          <ScalarProperty Name="CompanyName" ColumnName="CompanyName" />
          <ScalarProperty Name="Phone" ColumnName="Phone" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name="Suppliers">
      <EntityTypeMapping TypeName="IsTypeOf(NorthwindModel.Suppliers)">
        <MappingFragment StoreEntitySet="Suppliers">
          <ScalarProperty Name="SupplierID" ColumnName="SupplierID" />
          <ScalarProperty Name="CompanyName" ColumnName="CompanyName" />
          <ScalarProperty Name="ContactName" ColumnName="ContactName" />
          <ScalarProperty Name="ContactTitle" ColumnName="ContactTitle" />
          <ScalarProperty Name="Address" ColumnName="Address" />
          <ScalarProperty Name="City" ColumnName="City" />
          <ScalarProperty Name="Region" ColumnName="Region" />
          <ScalarProperty Name="PostalCode" ColumnName="PostalCode" />
          <ScalarProperty Name="Country" ColumnName="Country" />
          <ScalarProperty Name="Phone" ColumnName="Phone" />
          <ScalarProperty Name="Fax" ColumnName="Fax" />
          <ScalarProperty Name="HomePage" ColumnName="HomePage" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name="Territories">
      <EntityTypeMapping TypeName="IsTypeOf(NorthwindModel.Territories)">
        <MappingFragment StoreEntitySet="Territories">
          <ScalarProperty Name="TerritoryID" ColumnName="TerritoryID" />
          <ScalarProperty Name="TerritoryDescription" ColumnName="TerritoryDescription" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <AssociationSetMapping Name="FK_Products_Categories" TypeName="NorthwindModel.FK_Products_Categories" StoreEntitySet="Products">
      <EndProperty Name="Categories">
        <ScalarProperty Name="CategoryID" ColumnName="Categories_CategoryID" />
      </EndProperty>
      <EndProperty Name="Products">
        <ScalarProperty Name="ProductID" ColumnName="ProductID" />
      </EndProperty>
      <Condition ColumnName="Categories_CategoryID" IsNull="false" />
    </AssociationSetMapping>
    <AssociationSetMapping Name="FK_Orders_Customers" TypeName="NorthwindModel.FK_Orders_Customers" StoreEntitySet="Orders">
      <EndProperty Name="Customers">
        <ScalarProperty Name="CustomerID" ColumnName="Customers_CustomerID" />
      </EndProperty>
      <EndProperty Name="Orders">
        <ScalarProperty Name="OrderID" ColumnName="OrderID" />
      </EndProperty>
      <Condition ColumnName="Customers_CustomerID" IsNull="false" />
    </AssociationSetMapping>
    <AssociationSetMapping Name="FK_Employees_Employees" TypeName="NorthwindModel.FK_Employees_Employees" StoreEntitySet="Employees">
      <EndProperty Name="Employees">
        <ScalarProperty Name="EmployeeID" ColumnName="Employees2_EmployeeID" />
      </EndProperty>
      <EndProperty Name="Employees1">
        <ScalarProperty Name="EmployeeID" ColumnName="EmployeeID" />
      </EndProperty>
      <Condition ColumnName="Employees2_EmployeeID" IsNull="false" />
    </AssociationSetMapping>
    <AssociationSetMapping Name="FK_Orders_Employees" TypeName="NorthwindModel.FK_Orders_Employees" StoreEntitySet="Orders">
      <EndProperty Name="Employees">
        <ScalarProperty Name="EmployeeID" ColumnName="Employees_EmployeeID" />
      </EndProperty>
      <EndProperty Name="Orders">
        <ScalarProperty Name="OrderID" ColumnName="OrderID" />
      </EndProperty>
      <Condition ColumnName="Employees_EmployeeID" IsNull="false" />
    </AssociationSetMapping>
    <AssociationSetMapping Name="FK_Order_Details_Orders" TypeName="NorthwindModel.FK_Order_Details_Orders" StoreEntitySet="Order_Details">
      <EndProperty Name="Orders">
        <ScalarProperty Name="OrderID" ColumnName="OrderID" />
      </EndProperty>
      <EndProperty Name="Order_Details">
        <ScalarProperty Name="OrderID" ColumnName="OrderID" />
        <ScalarProperty Name="ProductID" ColumnName="ProductID" />
      </EndProperty>
    </AssociationSetMapping>
    <AssociationSetMapping Name="FK_Order_Details_Products" TypeName="NorthwindModel.FK_Order_Details_Products" StoreEntitySet="Order_Details">
      <EndProperty Name="Products">
        <ScalarProperty Name="ProductID" ColumnName="ProductID" />
      </EndProperty>
      <EndProperty Name="Order_Details">
        <ScalarProperty Name="OrderID" ColumnName="OrderID" />
        <ScalarProperty Name="ProductID" ColumnName="ProductID" />
      </EndProperty>
    </AssociationSetMapping>
    <AssociationSetMapping Name="FK_Orders_Shippers" TypeName="NorthwindModel.FK_Orders_Shippers" StoreEntitySet="Orders">
      <EndProperty Name="Shippers">
        <ScalarProperty Name="ShipperID" ColumnName="Shippers_ShipperID" />
      </EndProperty>
      <EndProperty Name="Orders">
        <ScalarProperty Name="OrderID" ColumnName="OrderID" />
      </EndProperty>
      <Condition ColumnName="Shippers_ShipperID" IsNull="false" />
    </AssociationSetMapping>
    <AssociationSetMapping Name="FK_Products_Suppliers" TypeName="NorthwindModel.FK_Products_Suppliers" StoreEntitySet="Products">
      <EndProperty Name="Suppliers">
        <ScalarProperty Name="SupplierID" ColumnName="Suppliers_SupplierID" />
      </EndProperty>
      <EndProperty Name="Products">
        <ScalarProperty Name="ProductID" ColumnName="ProductID" />
      </EndProperty>
      <Condition ColumnName="Suppliers_SupplierID" IsNull="false" />
    </AssociationSetMapping>
    <AssociationSetMapping Name="FK_Territories_Region" TypeName="NorthwindModel.FK_Territories_Region" StoreEntitySet="Territories">
      <EndProperty Name="Region">
        <ScalarProperty Name="RegionID" ColumnName="Region_RegionID" />
      </EndProperty>
      <EndProperty Name="Territories">
        <ScalarProperty Name="TerritoryID" ColumnName="TerritoryID" />
      </EndProperty>
    </AssociationSetMapping>
    <AssociationSetMapping Name="CustomerCustomerDemo" TypeName="NorthwindModel.CustomerCustomerDemo" StoreEntitySet="CustomerCustomerDemo">
      <EndProperty Name="CustomerDemographics">
        <ScalarProperty Name="CustomerTypeID" ColumnName="CustomerDemographics_CustomerTypeID" />
      </EndProperty>
      <EndProperty Name="Customers">
        <ScalarProperty Name="CustomerID" ColumnName="Customers_CustomerID" />
      </EndProperty>
    </AssociationSetMapping>
    <AssociationSetMapping Name="EmployeeTerritories" TypeName="NorthwindModel.EmployeeTerritories" StoreEntitySet="EmployeeTerritories">
      <EndProperty Name="Employees">
        <ScalarProperty Name="EmployeeID" ColumnName="Employees_EmployeeID" />
      </EndProperty>
      <EndProperty Name="Territories">
        <ScalarProperty Name="TerritoryID" ColumnName="Territories_TerritoryID" />
      </EndProperty>
    </AssociationSetMapping>
  </EntityContainerMapping>
</Mapping></StorageAndMappings>

The generated DDL:
SET QUOTED_IDENTIFIER OFF;
GO
USE [TestDb];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------


-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Categories'
CREATE TABLE [dbo].[Categories] (
    [CategoryID] int  NOT NULL,
    [CategoryName] nvarchar(15)  NOT NULL,
    [Description] nvarchar(max)  NULL,
    [Picture] varbinary(max)  NULL
);
GO

-- Creating table 'CustomerDemographics'
CREATE TABLE [dbo].[CustomerDemographics] (
    [CustomerTypeID] nchar(10)  NOT NULL,
    [CustomerDesc] nvarchar(max)  NULL
);
GO

-- Creating table 'CustomerHistory'
CREATE TABLE [dbo].[CustomerHistory] (
    [CustomerID] nchar(5)  NOT NULL,
    [CompanyName] nvarchar(40)  NOT NULL,
    [ContactName] nvarchar(30)  NULL,
    [ContactTitle] nvarchar(30)  NULL,
    [Address] nvarchar(60)  NULL,
    [City] nvarchar(15)  NULL,
    [Region] nvarchar(15)  NULL,
    [PostalCode] nvarchar(10)  NULL,
    [Country] nvarchar(15)  NULL,
    [Phone] nvarchar(24)  NULL,
    [Fax] nvarchar(24)  NULL
);
GO

-- Creating table 'Customers'
CREATE TABLE [dbo].[Customers] (
    [CustomerID] nchar(5)  NOT NULL,
    [CompanyName] nvarchar(40)  NOT NULL,
    [ContactName] nvarchar(30)  NULL,
    [ContactTitle] nvarchar(30)  NULL,
    [Address] nvarchar(60)  NULL,
    [City] nvarchar(15)  NULL,
    [Region] nvarchar(15)  NULL,
    [PostalCode] nvarchar(10)  NULL,
    [Country] nvarchar(15)  NULL,
    [Phone] nvarchar(24)  NULL,
    [Fax] nvarchar(24)  NULL
);
GO

-- Creating table 'DocumentStorage'
CREATE TABLE [dbo].[DocumentStorage] (
    [DocumentID] int  NOT NULL,
    [FileName] nvarchar(255)  NOT NULL,
    [DocumentFile] varbinary(max)  NOT NULL
);
GO

-- Creating table 'Employees'
CREATE TABLE [dbo].[Employees] (
    [EmployeeID] int  NOT NULL,
    [LastName] nvarchar(20)  NOT NULL,
    [FirstName] nvarchar(10)  NOT NULL,
    [Title] nvarchar(30)  NULL,
    [TitleOfCourtesy] nvarchar(25)  NULL,
    [BirthDate] datetime  NULL,
    [HireDate] datetime  NULL,
    [Address] nvarchar(60)  NULL,
    [City] nvarchar(15)  NULL,
    [Region] nvarchar(15)  NULL,
    [PostalCode] nvarchar(10)  NULL,
    [Country] nvarchar(15)  NULL,
    [HomePhone] nvarchar(24)  NULL,
    [Extension] nvarchar(4)  NULL,
    [Photo] varbinary(max)  NULL,
    [Notes] nvarchar(max)  NULL,
    [PhotoPath] nvarchar(255)  NULL,
    [Employees2_EmployeeID] int  NULL
);
GO

-- Creating table 'Order_Details'
CREATE TABLE [dbo].[Order_Details] (
    [OrderID] int  NOT NULL,
    [ProductID] int  NOT NULL,
    [UnitPrice] decimal(19,4)  NOT NULL,
    [Quantity] smallint  NOT NULL,
    [Discount] real  NOT NULL
);
GO

-- Creating table 'OrderHistory'
CREATE TABLE [dbo].[OrderHistory] (
    [OrderID] int  NOT NULL,
    [CustomerID] nchar(5)  NULL,
    [EmployeeID] int  NULL,
    [OrderDate] datetime  NULL,
    [RequiredDate] datetime  NULL,
    [ShippedDate] datetime  NULL,
    [ShipVia] int  NULL,
    [Freight] decimal(19,4)  NULL,
    [ShipName] nvarchar(40)  NULL,
    [ShipAddress] nvarchar(60)  NULL,
    [ShipCity] nvarchar(15)  NULL,
    [ShipRegion] nvarchar(15)  NULL,
    [ShipPostalCode] nvarchar(10)  NULL,
    [ShipCountry] nvarchar(15)  NULL
);
GO

-- Creating table 'Orders'
CREATE TABLE [dbo].[Orders] (
    [OrderID] int  NOT NULL,
    [OrderDate] datetime  NULL,
    [RequiredDate] datetime  NULL,
    [ShippedDate] datetime  NULL,
    [Freight] decimal(19,4)  NULL,
    [ShipName] nvarchar(40)  NULL,
    [ShipAddress] nvarchar(60)  NULL,
    [ShipCity] nvarchar(15)  NULL,
    [ShipRegion] nvarchar(15)  NULL,
    [ShipPostalCode] nvarchar(10)  NULL,
    [ShipCountry] nvarchar(15)  NULL,
    [Customers_CustomerID] nchar(5)  NULL,
    [Employees_EmployeeID] int  NULL,
    [Shippers_ShipperID] int  NULL
);
GO

-- Creating table 'Products'
CREATE TABLE [dbo].[Products] (
    [ProductID] int  NOT NULL,
    [ProductName] nvarchar(40)  NOT NULL,
    [QuantityPerUnit] nvarchar(20)  NULL,
    [UnitPrice] decimal(19,4)  NULL,
    [UnitsInStock] smallint  NULL,
    [UnitsOnOrder] smallint  NULL,
    [ReorderLevel] smallint  NULL,
    [Discontinued] bit  NOT NULL,
    [Categories_CategoryID] int  NULL,
    [Suppliers_SupplierID] int  NULL
);
GO

-- Creating table 'Region'
CREATE TABLE [dbo].[Region] (
    [RegionID] int  NOT NULL,
    [RegionDescription] nchar(50)  NOT NULL
);
GO

-- Creating table 'Shippers'
CREATE TABLE [dbo].[Shippers] (
    [ShipperID] int  NOT NULL,
    [CompanyName] nvarchar(40)  NOT NULL,
    [Phone] nvarchar(24)  NULL
);
GO

-- Creating table 'Suppliers'
CREATE TABLE [dbo].[Suppliers] (
    [SupplierID] int  NOT NULL,
    [CompanyName] nvarchar(40)  NOT NULL,
    [ContactName] nvarchar(30)  NULL,
    [ContactTitle] nvarchar(30)  NULL,
    [Address] nvarchar(60)  NULL,
    [City] nvarchar(15)  NULL,
    [Region] nvarchar(15)  NULL,
    [PostalCode] nvarchar(10)  NULL,
    [Country] nvarchar(15)  NULL,
    [Phone] nvarchar(24)  NULL,
    [Fax] nvarchar(24)  NULL,
    [HomePage] nvarchar(max)  NULL
);
GO

-- Creating table 'Territories'
CREATE TABLE [dbo].[Territories] (
    [TerritoryID] nvarchar(20)  NOT NULL,
    [TerritoryDescription] nchar(50)  NOT NULL,
    [Region_RegionID] int  NOT NULL
);
GO

-- Creating table 'CustomerCustomerDemo'
CREATE TABLE [dbo].[CustomerCustomerDemo] (
    [CustomerDemographics_CustomerTypeID] nchar(10)  NOT NULL,
    [Customers_CustomerID] nchar(5)  NOT NULL
);
GO

-- Creating table 'EmployeeTerritories'
CREATE TABLE [dbo].[EmployeeTerritories] (
    [Employees_EmployeeID] int  NOT NULL,
    [Territories_TerritoryID] nvarchar(20)  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [CategoryID] in table 'Categories'
ALTER TABLE [dbo].[Categories]
ADD CONSTRAINT [PK_Categories]
    PRIMARY KEY CLUSTERED ([CategoryID] ASC);
GO

-- Creating primary key on [CustomerTypeID] in table 'CustomerDemographics'
ALTER TABLE [dbo].[CustomerDemographics]
ADD CONSTRAINT [PK_CustomerDemographics]
    PRIMARY KEY CLUSTERED ([CustomerTypeID] ASC);
GO

-- Creating primary key on [CustomerID] in table 'CustomerHistory'
ALTER TABLE [dbo].[CustomerHistory]
ADD CONSTRAINT [PK_CustomerHistory]
    PRIMARY KEY CLUSTERED ([CustomerID] ASC);
GO

-- Creating primary key on [CustomerID] in table 'Customers'
ALTER TABLE [dbo].[Customers]
ADD CONSTRAINT [PK_Customers]
    PRIMARY KEY CLUSTERED ([CustomerID] ASC);
GO

-- Creating primary key on [DocumentID], [FileName] in table 'DocumentStorage'
ALTER TABLE [dbo].[DocumentStorage]
ADD CONSTRAINT [PK_DocumentStorage]
    PRIMARY KEY CLUSTERED ([DocumentID], [FileName] ASC);
GO

-- Creating primary key on [EmployeeID] in table 'Employees'
ALTER TABLE [dbo].[Employees]
ADD CONSTRAINT [PK_Employees]
    PRIMARY KEY CLUSTERED ([EmployeeID] ASC);
GO

-- Creating primary key on [OrderID], [ProductID] in table 'Order_Details'
ALTER TABLE [dbo].[Order_Details]
ADD CONSTRAINT [PK_Order_Details]
    PRIMARY KEY CLUSTERED ([OrderID], [ProductID] ASC);
GO

-- Creating primary key on [OrderID] in table 'OrderHistory'
ALTER TABLE [dbo].[OrderHistory]
ADD CONSTRAINT [PK_OrderHistory]
    PRIMARY KEY CLUSTERED ([OrderID] ASC);
GO

-- Creating primary key on [OrderID] in table 'Orders'
ALTER TABLE [dbo].[Orders]
ADD CONSTRAINT [PK_Orders]
    PRIMARY KEY CLUSTERED ([OrderID] ASC);
GO

-- Creating primary key on [ProductID] in table 'Products'
ALTER TABLE [dbo].[Products]
ADD CONSTRAINT [PK_Products]
    PRIMARY KEY CLUSTERED ([ProductID] ASC);
GO

-- Creating primary key on [RegionID] in table 'Region'
ALTER TABLE [dbo].[Region]
ADD CONSTRAINT [PK_Region]
    PRIMARY KEY CLUSTERED ([RegionID] ASC);
GO

-- Creating primary key on [ShipperID] in table 'Shippers'
ALTER TABLE [dbo].[Shippers]
ADD CONSTRAINT [PK_Shippers]
    PRIMARY KEY CLUSTERED ([ShipperID] ASC);
GO

-- Creating primary key on [SupplierID] in table 'Suppliers'
ALTER TABLE [dbo].[Suppliers]
ADD CONSTRAINT [PK_Suppliers]
    PRIMARY KEY CLUSTERED ([SupplierID] ASC);
GO

-- Creating primary key on [TerritoryID] in table 'Territories'
ALTER TABLE [dbo].[Territories]
ADD CONSTRAINT [PK_Territories]
    PRIMARY KEY CLUSTERED ([TerritoryID] ASC);
GO

-- Creating primary key on [CustomerDemographics_CustomerTypeID], [Customers_CustomerID] in table 'CustomerCustomerDemo'
ALTER TABLE [dbo].[CustomerCustomerDemo]
ADD CONSTRAINT [PK_CustomerCustomerDemo]
    PRIMARY KEY CLUSTERED ([CustomerDemographics_CustomerTypeID], [Customers_CustomerID] ASC);
GO

-- Creating primary key on [Employees_EmployeeID], [Territories_TerritoryID] in table 'EmployeeTerritories'
ALTER TABLE [dbo].[EmployeeTerritories]
ADD CONSTRAINT [PK_EmployeeTerritories]
    PRIMARY KEY CLUSTERED ([Employees_EmployeeID], [Territories_TerritoryID] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [Categories_CategoryID] in table 'Products'
ALTER TABLE [dbo].[Products]
ADD CONSTRAINT [FK_Products_Categories]
    FOREIGN KEY ([Categories_CategoryID])
    REFERENCES [dbo].[Categories]
        ([CategoryID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Products_Categories'
CREATE INDEX [IX_FK_Products_Categories]
ON [dbo].[Products]
    ([Categories_CategoryID]);
GO

-- Creating foreign key on [Customers_CustomerID] in table 'Orders'
ALTER TABLE [dbo].[Orders]
ADD CONSTRAINT [FK_Orders_Customers]
    FOREIGN KEY ([Customers_CustomerID])
    REFERENCES [dbo].[Customers]
        ([CustomerID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Orders_Customers'
CREATE INDEX [IX_FK_Orders_Customers]
ON [dbo].[Orders]
    ([Customers_CustomerID]);
GO

-- Creating foreign key on [Employees2_EmployeeID] in table 'Employees'
ALTER TABLE [dbo].[Employees]
ADD CONSTRAINT [FK_Employees_Employees]
    FOREIGN KEY ([Employees2_EmployeeID])
    REFERENCES [dbo].[Employees]
        ([EmployeeID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Employees_Employees'
CREATE INDEX [IX_FK_Employees_Employees]
ON [dbo].[Employees]
    ([Employees2_EmployeeID]);
GO

-- Creating foreign key on [Employees_EmployeeID] in table 'Orders'
ALTER TABLE [dbo].[Orders]
ADD CONSTRAINT [FK_Orders_Employees]
    FOREIGN KEY ([Employees_EmployeeID])
    REFERENCES [dbo].[Employees]
        ([EmployeeID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Orders_Employees'
CREATE INDEX [IX_FK_Orders_Employees]
ON [dbo].[Orders]
    ([Employees_EmployeeID]);
GO

-- Creating foreign key on [OrderID] in table 'Order_Details'
ALTER TABLE [dbo].[Order_Details]
ADD CONSTRAINT [FK_Order_Details_Orders]
    FOREIGN KEY ([OrderID])
    REFERENCES [dbo].[Orders]
        ([OrderID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [ProductID] in table 'Order_Details'
ALTER TABLE [dbo].[Order_Details]
ADD CONSTRAINT [FK_Order_Details_Products]
    FOREIGN KEY ([ProductID])
    REFERENCES [dbo].[Products]
        ([ProductID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Order_Details_Products'
CREATE INDEX [IX_FK_Order_Details_Products]
ON [dbo].[Order_Details]
    ([ProductID]);
GO

-- Creating foreign key on [Shippers_ShipperID] in table 'Orders'
ALTER TABLE [dbo].[Orders]
ADD CONSTRAINT [FK_Orders_Shippers]
    FOREIGN KEY ([Shippers_ShipperID])
    REFERENCES [dbo].[Shippers]
        ([ShipperID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Orders_Shippers'
CREATE INDEX [IX_FK_Orders_Shippers]
ON [dbo].[Orders]
    ([Shippers_ShipperID]);
GO

-- Creating foreign key on [Suppliers_SupplierID] in table 'Products'
ALTER TABLE [dbo].[Products]
ADD CONSTRAINT [FK_Products_Suppliers]
    FOREIGN KEY ([Suppliers_SupplierID])
    REFERENCES [dbo].[Suppliers]
        ([SupplierID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Products_Suppliers'
CREATE INDEX [IX_FK_Products_Suppliers]
ON [dbo].[Products]
    ([Suppliers_SupplierID]);
GO

-- Creating foreign key on [Region_RegionID] in table 'Territories'
ALTER TABLE [dbo].[Territories]
ADD CONSTRAINT [FK_Territories_Region]
    FOREIGN KEY ([Region_RegionID])
    REFERENCES [dbo].[Region]
        ([RegionID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Territories_Region'
CREATE INDEX [IX_FK_Territories_Region]
ON [dbo].[Territories]
    ([Region_RegionID]);
GO

-- Creating foreign key on [CustomerDemographics_CustomerTypeID] in table 'CustomerCustomerDemo'
ALTER TABLE [dbo].[CustomerCustomerDemo]
ADD CONSTRAINT [FK_CustomerCustomerDemo_CustomerDemographics]
    FOREIGN KEY ([CustomerDemographics_CustomerTypeID])
    REFERENCES [dbo].[CustomerDemographics]
        ([CustomerTypeID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Customers_CustomerID] in table 'CustomerCustomerDemo'
ALTER TABLE [dbo].[CustomerCustomerDemo]
ADD CONSTRAINT [FK_CustomerCustomerDemo_Customers]
    FOREIGN KEY ([Customers_CustomerID])
    REFERENCES [dbo].[Customers]
        ([CustomerID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_CustomerCustomerDemo_Customers'
CREATE INDEX [IX_FK_CustomerCustomerDemo_Customers]
ON [dbo].[CustomerCustomerDemo]
    ([Customers_CustomerID]);
GO

-- Creating foreign key on [Employees_EmployeeID] in table 'EmployeeTerritories'
ALTER TABLE [dbo].[EmployeeTerritories]
ADD CONSTRAINT [FK_EmployeeTerritories_Employees]
    FOREIGN KEY ([Employees_EmployeeID])
    REFERENCES [dbo].[Employees]
        ([EmployeeID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Territories_TerritoryID] in table 'EmployeeTerritories'
ALTER TABLE [dbo].[EmployeeTerritories]
ADD CONSTRAINT [FK_EmployeeTerritories_Territories]
    FOREIGN KEY ([Territories_TerritoryID])
    REFERENCES [dbo].[Territories]
        ([TerritoryID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_EmployeeTerritories_Territories'
CREATE INDEX [IX_FK_EmployeeTerritories_Territories]
ON [dbo].[EmployeeTerritories]
    ([Territories_TerritoryID]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------
