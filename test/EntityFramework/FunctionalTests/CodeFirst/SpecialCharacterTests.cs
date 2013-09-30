// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.CodeFirst
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Xml.Linq;
    using Xunit;

    public class SpecialCharacterTests : FunctionalTestBase
    {
        [Fact]
        public void Properties_starting_with_underscores_are_massaged_to_allow_valid_EDM_and_database()
        {
            using (var context = new SpecialContext())
            {
                var special = context.Specials.Single();
                Assert.Equal("Superman!", special._Underpants);
                Assert.Equal("Supersuperman!", special.__DoubleUnderpants);

                var anon = context.Specials.Select(s => new { s._Underpants, s.__DoubleUnderpants }).Single();
                Assert.Equal("Superman!", anon._Underpants);
                Assert.Equal("Supersuperman!", anon.__DoubleUnderpants);

                var workspace = ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace;

                var cSpaceType= workspace.GetItemCollection(DataSpace.CSpace).OfType<EntityType>().Single();
                Assert.Equal(
                    new[] { "__DoubleUnderpants", "_Underpants", "Id" },
                    cSpaceType.Properties.Select(p => p.Name).OrderBy(n => n));

                var sSpaceType = workspace.GetItemCollection(DataSpace.SSpace).OfType<EntityType>().Single();
                Assert.Equal(
                    new[] { "__DoubleUnderpants", "_Underpants", "Id" },
                    sSpaceType.Properties.Select(p => p.Name).OrderBy(n => n));
            }
        }

        public class SpecialContext : DbContext
        {
            static SpecialContext()
            {
                Database.SetInitializer(new SpecialInitializer());
            }

            public DbSet<AreYouSpecial> Specials { get; set; }
        }

        public class AreYouSpecial
        {
            public int Id { get; set; }

            public string _Underpants { get; set; }
            public string __DoubleUnderpants { get; set; }
        }

        public class SpecialInitializer : DropCreateDatabaseAlways<SpecialContext>
        {
            protected override void Seed(SpecialContext context)
            {
                context.Specials.Add(new AreYouSpecial { _Underpants = "Superman!", __DoubleUnderpants = "Supersuperman!" });
            }
        }


        [Fact]
        public void Workspace_can_be_constructed_when_EDM_identifiers_start_with_underscores()
        {
            var edmItemCollection = new EdmItemCollection(new[] { XDocument.Parse(UnderpantsCsdl).CreateReader() });
            var storeItemCollection = new StoreItemCollection(new[] { XDocument.Parse(UnderpantsSsdl).CreateReader() });
            
            IList<EdmSchemaError> errors;
            var storageMappingItemCollection = StorageMappingItemCollection.Create(
                edmItemCollection, 
                storeItemCollection, 
                new[] { XDocument.Parse(UnderpantsMsl).CreateReader() }, 
                null, 
                out errors);

            Assert.Equal(0, errors.Count);
            Assert.NotNull(storageMappingItemCollection);

            var workspace = new MetadataWorkspace(
                () => edmItemCollection,
                () => storeItemCollection,
                () => storageMappingItemCollection);

            // Run enough initialization to get validation done
            using (var connection = new EntityConnection(workspace, new SqlConnection()))
            {
                Assert.Same(edmItemCollection, connection.GetMetadataWorkspace().GetItemCollection(DataSpace.CSpace));
                Assert.Same(storeItemCollection, connection.GetMetadataWorkspace().GetItemCollection(DataSpace.SSpace));
                Assert.Same(storageMappingItemCollection, connection.GetMetadataWorkspace().GetItemCollection(DataSpace.CSSpace));
            }
        }

        private const string UnderpantsCsdl = @"
<Schema Namespace=""_AdvancedPatternsModelFirst"" Alias=""Self"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"" xmlns:codegen=""http://schemas.microsoft.com/ado/2006/04/codegeneration"" xmlns:annotation=""http://schemas.microsoft.com/ado/2009/02/edm/annotation"">
  <ComplexType Name=""_AddressMf"" codegen:TypeAccess=""Public"">
    <Property Name=""_Street"" Type=""String"" Nullable=""true"" codegen:GetterAccess=""Public"" codegen:SetterAccess=""Public"" />
    <Property Name=""_City"" Type=""String"" Nullable=""true"" codegen:GetterAccess=""Private"" codegen:SetterAccess=""Public"" />
    <Property Name=""_State"" Type=""String"" Nullable=""true"" codegen:GetterAccess=""Internal"" codegen:SetterAccess=""Internal"" />
    <Property Name=""_ZipCode"" Type=""String"" Nullable=""true"" codegen:GetterAccess=""Internal"" codegen:SetterAccess=""Private"" />
    <Property Name=""_SiteInfo"" Type=""Self._SiteInfoMf"" Nullable=""false"" codegen:GetterAccess=""Internal"" codegen:SetterAccess=""Private"" />
  </ComplexType>
  <ComplexType Name=""_SiteInfoMf"" codegen:TypeAccess=""Internal"">
    <Property Name=""_Zone"" Type=""Int32"" Nullable=""true"" codegen:GetterAccess=""Protected"" codegen:SetterAccess=""Private"" />
    <Property Name=""_Environment"" Type=""String"" Nullable=""true"" />
  </ComplexType>
  <EntityType Name=""_EmployeeMf"" Abstract=""true"">
    <Key>
      <PropertyRef Name=""_EmployeeId"" />
    </Key>
    <Property Name=""_EmployeeId"" Type=""Int32"" Nullable=""false"" p6:StoreGeneratedPattern=""Identity"" xmlns:p6=""http://schemas.microsoft.com/ado/2009/02/edm/annotation"" />
    <Property Name=""_FirstName"" Type=""String"" FixedLength=""false"" Unicode=""true"" MaxLength=""4000"" Nullable=""true"" codegen:GetterAccess=""Public"" codegen:SetterAccess=""Private"" />
    <Property Name=""_LastName"" Type=""String"" FixedLength=""false"" Unicode=""true"" MaxLength=""4000"" Nullable=""true"" codegen:GetterAccess=""Private"" codegen:SetterAccess=""Internal"" />
  </EntityType>
  <EntityType Name=""_CurrentEmployeeMf"" BaseType=""Self._EmployeeMf"">
    <Property Name=""_LeaveBalance"" Type=""Decimal"" Nullable=""true"" />
    <NavigationProperty Name=""_Manager"" Relationship=""Self._CurrentEmployee_Manager"" FromRole=""_CurrentEmployee_Manager_Source"" ToRole=""_CurrentEmployee_Manager_Target"" />
    <NavigationProperty Name=""_Office"" Relationship=""Self._CurrentEmployee_Office"" FromRole=""_CurrentEmployee_Office_Source"" ToRole=""_CurrentEmployee_Office_Target"" />
  </EntityType>
  <EntityType Name=""_OfficeMf"">
    <Key>
      <PropertyRef Name=""_Number"" />
      <PropertyRef Name=""_BuildingId"" />
    </Key>
    <Property Name=""_Number"" Type=""String"" FixedLength=""false"" Unicode=""true"" MaxLength=""128"" Nullable=""false"" />
    <Property Name=""_BuildingId"" Type=""Guid"" Nullable=""false"" />
    <Property Name=""_Description"" Type=""String"" FixedLength=""false"" Unicode=""true"" MaxLength=""4000"" Nullable=""true"" codegen:GetterAccess=""Internal"" codegen:SetterAccess=""Private"" />
    <NavigationProperty Name=""_Building"" Relationship=""Self._Building_Offices"" FromRole=""_Building_Offices_Target"" ToRole=""_Building_Offices_Source"" codegen:GetterAccess=""Private"" codegen:SetterAccess=""Private"" />
    <NavigationProperty Name=""_WhiteBoards"" Relationship=""Self._Whiteboard_Office"" FromRole=""_Whiteboard_Office_Target"" ToRole=""_Whiteboard_Office_Source"" codegen:GetterAccess=""Internal"" codegen:SetterAccess=""Public"" />
  </EntityType>
  <EntityType Name=""_BuildingMf"">
    <Key>
      <PropertyRef Name=""_BuildingId"" />
    </Key>
    <Property Name=""_BuildingId"" Type=""Guid"" Nullable=""false"" />
    <Property Name=""_Name"" Type=""String"" FixedLength=""false"" Unicode=""true"" MaxLength=""4000"" Nullable=""true"" />
    <Property Name=""_Value"" Type=""Decimal"" Nullable=""false"" codegen:GetterAccess=""Internal"" codegen:SetterAccess=""Internal"" />
    <Property Name=""_Address"" Type=""Self._AddressMf"" Nullable=""false"" codegen:GetterAccess=""Internal"" codegen:SetterAccess=""Private"" />
    <NavigationProperty Name=""_Offices"" Relationship=""Self._Building_Offices"" FromRole=""_Building_Offices_Source"" ToRole=""_Building_Offices_Target"" />
    <NavigationProperty Name=""_MailRooms"" Relationship=""Self._MailRoom_Building"" FromRole=""_MailRoom_Building_Target"" ToRole=""_MailRoom_Building_Source"" codegen:GetterAccess=""Internal"" codegen:SetterAccess=""Private"" />
  </EntityType>
  <EntityType Name=""_MailRoomMf"" codegen:TypeAccess=""Internal"">
    <Key>
      <PropertyRef Name=""_id"" />
    </Key>
    <Property Name=""_id"" Type=""Int32"" Nullable=""false"" p6:StoreGeneratedPattern=""Identity"" xmlns:p6=""http://schemas.microsoft.com/ado/2009/02/edm/annotation"" />
    <Property Name=""_BuildingId"" Type=""Guid"" Nullable=""false"" />
    <NavigationProperty Name=""_Building"" Relationship=""Self._MailRoom_Building"" FromRole=""_MailRoom_Building_Source"" ToRole=""_MailRoom_Building_Target"" />
  </EntityType>
  <EntityType Name=""_WhiteboardMf"">
    <Key>
      <PropertyRef Name=""_iD"" />
    </Key>
    <Property Name=""_iD"" Type=""Binary"" FixedLength=""false"" MaxLength=""128"" Nullable=""false"" />
    <Property Name=""_AssetTag"" Type=""String"" FixedLength=""false"" Unicode=""true"" MaxLength=""4000"" Nullable=""true"" />
    <NavigationProperty Name=""_Office"" Relationship=""Self._Whiteboard_Office"" FromRole=""_Whiteboard_Office_Source"" ToRole=""_Whiteboard_Office_Target"" />
  </EntityType>
  <EntityType Name=""_PastEmployeeMf"" BaseType=""Self._EmployeeMf"">
    <Property Name=""_TerminationDate"" Type=""DateTime"" Nullable=""false"" />
  </EntityType>
  <EntityType Name=""_BuildingDetailMf"" codegen:TypeAccess=""Public"">
    <Key>
      <PropertyRef Name=""_BuildingId"" />
    </Key>
    <Property Name=""_BuildingId"" Type=""Guid"" Nullable=""false"" />
    <Property Name=""_Details"" Type=""String"" FixedLength=""false"" Unicode=""true"" MaxLength=""4000"" Nullable=""true"" />
    <NavigationProperty Name=""_Building"" Relationship=""Self._BuildingDetail_Building"" FromRole=""_BuildingDetail_Building_Source"" ToRole=""_BuildingDetail_Building_Target"" />
  </EntityType>
  <EntityType Name=""_WorkOrderMf"">
    <Key>
      <PropertyRef Name=""_WorkOrderId"" />
    </Key>
    <Property Name=""_WorkOrderId"" Type=""Int32"" Nullable=""false"" p6:StoreGeneratedPattern=""Identity"" xmlns:p6=""http://schemas.microsoft.com/ado/2009/02/edm/annotation"" />
    <Property Name=""_EmployeeId"" Type=""Int32"" Nullable=""false"" />
    <Property Name=""_Details"" Type=""String"" FixedLength=""false"" Unicode=""true"" MaxLength=""4000"" Nullable=""true"" />
    <NavigationProperty Name=""_Employee"" Relationship=""Self._WorkOrder_Employee"" FromRole=""_WorkOrder_Employee_Source"" ToRole=""_WorkOrder_Employee_Target"" codegen:GetterAccess=""Internal"" codegen:SetterAccess=""Internal"" />
  </EntityType>
  <Association Name=""_CurrentEmployee_Manager"">
    <End Role=""_CurrentEmployee_Manager_Source"" Type=""Self._CurrentEmployeeMf"" Multiplicity=""*"" />
    <End Role=""_CurrentEmployee_Manager_Target"" Type=""Self._CurrentEmployeeMf"" Multiplicity=""0..1"" />
  </Association>
  <Association Name=""_Building_Offices"">
    <End Role=""_Building_Offices_Source"" Type=""Self._BuildingMf"" Multiplicity=""1"">
      <OnDelete Action=""Cascade"" />
    </End>
    <End Role=""_Building_Offices_Target"" Type=""Self._OfficeMf"" Multiplicity=""*"" />
    <ReferentialConstraint>
      <Principal Role=""_Building_Offices_Source"">
        <PropertyRef Name=""_BuildingId"" />
      </Principal>
      <Dependent Role=""_Building_Offices_Target"">
        <PropertyRef Name=""_BuildingId"" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name=""_MailRoom_Building"">
    <End Role=""_MailRoom_Building_Source"" Type=""Self._MailRoomMf"" Multiplicity=""*"" />
    <End Role=""_MailRoom_Building_Target"" Type=""Self._BuildingMf"" Multiplicity=""1"">
      <OnDelete Action=""Cascade"" />
    </End>
    <ReferentialConstraint>
      <Principal Role=""_MailRoom_Building_Target"">
        <PropertyRef Name=""_BuildingId"" />
      </Principal>
      <Dependent Role=""_MailRoom_Building_Source"">
        <PropertyRef Name=""_BuildingId"" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name=""_Whiteboard_Office"">
    <End Role=""_Whiteboard_Office_Source"" Type=""Self._WhiteboardMf"" Multiplicity=""*"" />
    <End Role=""_Whiteboard_Office_Target"" Type=""Self._OfficeMf"" Multiplicity=""0..1"" />
  </Association>
  <Association Name=""_CurrentEmployee_Office"">
    <End Role=""_CurrentEmployee_Office_Source"" Type=""Self._CurrentEmployeeMf"" Multiplicity=""*"" />
    <End Role=""_CurrentEmployee_Office_Target"" Type=""Self._OfficeMf"" Multiplicity=""0..1"" />
  </Association>
  <Association Name=""_BuildingDetail_Building"">
    <End Role=""_BuildingDetail_Building_Source"" Type=""Self._BuildingDetailMf"" Multiplicity=""0..1"" />
    <End Role=""_BuildingDetail_Building_Target"" Type=""Self._BuildingMf"" Multiplicity=""1"" />
    <ReferentialConstraint>
      <Principal Role=""_BuildingDetail_Building_Target"">
        <PropertyRef Name=""_BuildingId"" />
      </Principal>
      <Dependent Role=""_BuildingDetail_Building_Source"">
        <PropertyRef Name=""_BuildingId"" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name=""_WorkOrder_Employee"">
    <End Role=""_WorkOrder_Employee_Source"" Type=""Self._WorkOrderMf"" Multiplicity=""*"" />
    <End Role=""_WorkOrder_Employee_Target"" Type=""Self._EmployeeMf"" Multiplicity=""1"">
      <OnDelete Action=""Cascade"" />
    </End>
    <ReferentialConstraint>
      <Principal Role=""_WorkOrder_Employee_Target"">
        <PropertyRef Name=""_EmployeeId"" />
      </Principal>
      <Dependent Role=""_WorkOrder_Employee_Source"">
        <PropertyRef Name=""_EmployeeId"" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <EntityContainer Name=""_AdvancedPatternsModelFirstContext"" codegen:TypeAccess=""Internal"" annotation:LazyLoadingEnabled=""false"">
    <EntitySet Name=""_Employees"" EntityType=""Self._EmployeeMf"" />
    <EntitySet Name=""_Offices"" EntityType=""Self._OfficeMf"" />
    <EntitySet Name=""_Buildings"" EntityType=""Self._BuildingMf"" />
    <EntitySet Name=""_MailRooms"" EntityType=""Self._MailRoomMf"" codegen:GetterAccess=""Internal"" />
    <EntitySet Name=""_Whiteboards"" EntityType=""Self._WhiteboardMf"" />
    <EntitySet Name=""_BuildingDetails"" EntityType=""Self._BuildingDetailMf"" />
    <EntitySet Name=""_WorkOrders"" EntityType=""Self._WorkOrderMf"" />
    <AssociationSet Name=""_CurrentEmployee_Manager"" Association=""Self._CurrentEmployee_Manager"">
      <End Role=""_CurrentEmployee_Manager_Source"" EntitySet=""_Employees"" />
      <End Role=""_CurrentEmployee_Manager_Target"" EntitySet=""_Employees"" />
    </AssociationSet>
    <AssociationSet Name=""_Building_Offices"" Association=""Self._Building_Offices"">
      <End Role=""_Building_Offices_Source"" EntitySet=""_Buildings"" />
      <End Role=""_Building_Offices_Target"" EntitySet=""_Offices"" />
    </AssociationSet>
    <AssociationSet Name=""_MailRoom_Building"" Association=""Self._MailRoom_Building"">
      <End Role=""_MailRoom_Building_Source"" EntitySet=""_MailRooms"" />
      <End Role=""_MailRoom_Building_Target"" EntitySet=""_Buildings"" />
    </AssociationSet>
    <AssociationSet Name=""_Whiteboard_Office"" Association=""Self._Whiteboard_Office"">
      <End Role=""_Whiteboard_Office_Source"" EntitySet=""_Whiteboards"" />
      <End Role=""_Whiteboard_Office_Target"" EntitySet=""_Offices"" />
    </AssociationSet>
    <AssociationSet Name=""_CurrentEmployee_Office"" Association=""Self._CurrentEmployee_Office"">
      <End Role=""_CurrentEmployee_Office_Source"" EntitySet=""_Employees"" />
      <End Role=""_CurrentEmployee_Office_Target"" EntitySet=""_Offices"" />
    </AssociationSet>
    <AssociationSet Name=""_BuildingDetail_Building"" Association=""Self._BuildingDetail_Building"">
      <End Role=""_BuildingDetail_Building_Source"" EntitySet=""_BuildingDetails"" />
      <End Role=""_BuildingDetail_Building_Target"" EntitySet=""_Buildings"" />
    </AssociationSet>
    <AssociationSet Name=""_WorkOrder_Employee"" Association=""Self._WorkOrder_Employee"">
      <End Role=""_WorkOrder_Employee_Source"" EntitySet=""_WorkOrders"" />
      <End Role=""_WorkOrder_Employee_Target"" EntitySet=""_Employees"" />
    </AssociationSet>
    <FunctionImport Name=""_AllOfficesStoredProc"" EntitySet=""_Offices"" ReturnType=""Collection(Self._OfficeMf)"" />
    <FunctionImport Name=""_EmployeeIdsInOfficeStoredProc"" ReturnType=""Collection(Int32)"">
      <Parameter Name=""_OfficeNumber"" Mode=""In"" Type=""String"" />
      <Parameter Name=""_BuildingId"" Mode=""In"" Type=""Guid"" />
    </FunctionImport>
    <FunctionImport Name=""_OfficesInBuildingStoredProc"" EntitySet=""_Offices"" ReturnType=""Collection(Self._OfficeMf)"">
      <Parameter Name=""_BuildingId"" Mode=""In"" Type=""Guid"" />
    </FunctionImport>
    <FunctionImport Name=""_SkimOffLeaveBalanceStoredProc"">
      <Parameter Name=""_First"" Mode=""In"" Type=""String"" />
      <Parameter Name=""_Last"" Mode=""In"" Type=""String"" />
    </FunctionImport>
    <FunctionImport Name=""_AllSiteInfoStoredProc"" ReturnType=""Collection(Self._SiteInfoMf)"" />
  </EntityContainer>
</Schema>";

        private const string UnderpantsSsdl = @"
<Schema Namespace=""_dboNamespace"" Provider=""System.Data.SqlClient"" ProviderManifestToken=""2008"" Alias=""Self"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm/ssdl"">
  <EntityType Name=""_Employees"">
    <Key>
      <PropertyRef Name=""_EmployeeId"" />
    </Key>
    <Property Name=""_EmployeeId"" Type=""int"" StoreGeneratedPattern=""Identity"" Nullable=""false"" />
    <Property Name=""_FirstName"" Type=""nvarchar"" MaxLength=""4000"" Nullable=""true"" />
    <Property Name=""_LastName"" Type=""nvarchar"" MaxLength=""4000"" Nullable=""true"" />
    <Property Name=""_LeaveBalance"" Type=""decimal"" Nullable=""true"" />
    <Property Name=""_TerminationDate"" Type=""datetime"" Nullable=""true"" />
    <Property Name=""_Discriminator"" Type=""nvarchar(max)"" Nullable=""false"" />
    <Property Name=""_CurrentEmployeeEmployeeId"" Type=""int"" Nullable=""true"" />
    <Property Name=""_OfficeNumber"" Type=""nvarchar"" MaxLength=""128"" Nullable=""true"" />
    <Property Name=""_OfficeBuildingId"" Type=""uniqueidentifier"" Nullable=""true"" />
  </EntityType>
  <EntityType Name=""_Offices"">
    <Key>
      <PropertyRef Name=""_Number"" />
      <PropertyRef Name=""_BuildingId"" />
    </Key>
    <Property Name=""_Number"" Type=""nvarchar"" MaxLength=""128"" Nullable=""false"" />
    <Property Name=""_BuildingId"" Type=""uniqueidentifier"" Nullable=""false"" />
    <Property Name=""_Description"" Type=""nvarchar"" MaxLength=""4000"" Nullable=""true"" />
  </EntityType>
  <EntityType Name=""_Buildings"">
    <Key>
      <PropertyRef Name=""_BuildingId"" />
    </Key>
    <Property Name=""_BuildingId"" Type=""uniqueidentifier"" Nullable=""false"" />
    <Property Name=""_Name"" Type=""nvarchar"" MaxLength=""4000"" Nullable=""true"" />
    <Property Name=""_Value"" Type=""decimal"" Nullable=""false"" />
    <Property Name=""_Street"" Type=""nvarchar(max)"" Nullable=""true"" />
    <Property Name=""_City"" Type=""nvarchar(max)"" Nullable=""true"" />
    <Property Name=""_State"" Type=""nvarchar(max)"" Nullable=""true"" />
    <Property Name=""_ZipCode"" Type=""nvarchar(max)"" Nullable=""true"" />
    <Property Name=""_Zone"" Type=""int"" Nullable=""true"" />
    <Property Name=""_Environment"" Type=""nvarchar(max)"" Nullable=""true"" />
  </EntityType>
  <EntityType Name=""_MailRooms"">
    <Key>
      <PropertyRef Name=""_id"" />
    </Key>
    <Property Name=""_id"" Type=""int"" StoreGeneratedPattern=""Identity"" Nullable=""false"" />
    <Property Name=""_BuildingId"" Type=""uniqueidentifier"" Nullable=""false"" />
  </EntityType>
  <EntityType Name=""_Whiteboards"">
    <Key>
      <PropertyRef Name=""_iD"" />
    </Key>
    <Property Name=""_iD"" Type=""varbinary"" MaxLength=""128"" Nullable=""false"" />
    <Property Name=""_AssetTag"" Type=""nvarchar"" MaxLength=""4000"" Nullable=""true"" />
    <Property Name=""_OfficeNumber"" Type=""nvarchar"" MaxLength=""128"" Nullable=""true"" />
    <Property Name=""_OfficeBuildingId"" Type=""uniqueidentifier"" Nullable=""true"" />
  </EntityType>
  <EntityType Name=""_BuildingDetails"">
    <Key>
      <PropertyRef Name=""_BuildingId"" />
    </Key>
    <Property Name=""_BuildingId"" Type=""uniqueidentifier"" Nullable=""false"" />
    <Property Name=""_Details"" Type=""nvarchar"" MaxLength=""4000"" Nullable=""true"" />
  </EntityType>
  <EntityType Name=""_WorkOrders"">
    <Key>
      <PropertyRef Name=""_WorkOrderId"" />
    </Key>
    <Property Name=""_WorkOrderId"" Type=""int"" StoreGeneratedPattern=""Identity"" Nullable=""false"" />
    <Property Name=""_EmployeeId"" Type=""int"" Nullable=""false"" />
    <Property Name=""_Details"" Type=""nvarchar"" MaxLength=""4000"" Nullable=""true"" />
  </EntityType>
  <Association Name=""_CurrentEmployee_Manager"">
    <End Role=""_Employees"" Type=""Self._Employees"" Multiplicity=""0..1"" />
    <End Role=""_EmployeesSelf"" Type=""Self._Employees"" Multiplicity=""*"" />
    <ReferentialConstraint>
      <Principal Role=""_Employees"">
        <PropertyRef Name=""_EmployeeId"" />
      </Principal>
      <Dependent Role=""_EmployeesSelf"">
        <PropertyRef Name=""_CurrentEmployeeEmployeeId"" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name=""_CurrentEmployee_Office"">
    <End Role=""_Offices"" Type=""Self._Offices"" Multiplicity=""0..1"" />
    <End Role=""_Employees"" Type=""Self._Employees"" Multiplicity=""*"" />
    <ReferentialConstraint>
      <Principal Role=""_Offices"">
        <PropertyRef Name=""_Number"" />
        <PropertyRef Name=""_BuildingId"" />
      </Principal>
      <Dependent Role=""_Employees"">
        <PropertyRef Name=""_OfficeNumber"" />
        <PropertyRef Name=""_OfficeBuildingId"" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name=""_Building_Offices"">
    <End Role=""_Buildings"" Type=""Self._Buildings"" Multiplicity=""1"">
      <OnDelete Action=""Cascade"" />
    </End>
    <End Role=""_Offices"" Type=""Self._Offices"" Multiplicity=""*"" />
    <ReferentialConstraint>
      <Principal Role=""_Buildings"">
        <PropertyRef Name=""_BuildingId"" />
      </Principal>
      <Dependent Role=""_Offices"">
        <PropertyRef Name=""_BuildingId"" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name=""_MailRoom_Building"">
    <End Role=""_Buildings"" Type=""Self._Buildings"" Multiplicity=""1"">
      <OnDelete Action=""Cascade"" />
    </End>
    <End Role=""_MailRooms"" Type=""Self._MailRooms"" Multiplicity=""*"" />
    <ReferentialConstraint>
      <Principal Role=""_Buildings"">
        <PropertyRef Name=""_BuildingId"" />
      </Principal>
      <Dependent Role=""_MailRooms"">
        <PropertyRef Name=""_BuildingId"" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name=""_Whiteboard_Office"">
    <End Role=""_Offices"" Type=""Self._Offices"" Multiplicity=""0..1"" />
    <End Role=""_Whiteboards"" Type=""Self._Whiteboards"" Multiplicity=""*"" />
    <ReferentialConstraint>
      <Principal Role=""_Offices"">
        <PropertyRef Name=""_Number"" />
        <PropertyRef Name=""_BuildingId"" />
      </Principal>
      <Dependent Role=""_Whiteboards"">
        <PropertyRef Name=""_OfficeNumber"" />
        <PropertyRef Name=""_OfficeBuildingId"" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name=""_BuildingDetail_Building"">
    <End Role=""_Buildings"" Type=""Self._Buildings"" Multiplicity=""1"" />
    <End Role=""_BuildingDetails"" Type=""Self._BuildingDetails"" Multiplicity=""0..1"" />
    <ReferentialConstraint>
      <Principal Role=""_Buildings"">
        <PropertyRef Name=""_BuildingId"" />
      </Principal>
      <Dependent Role=""_BuildingDetails"">
        <PropertyRef Name=""_BuildingId"" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <Association Name=""_WorkOrder_Employee"">
    <End Role=""_Employees"" Type=""Self._Employees"" Multiplicity=""1"">
      <OnDelete Action=""Cascade"" />
    </End>
    <End Role=""_WorkOrders"" Type=""Self._WorkOrders"" Multiplicity=""*"" />
    <ReferentialConstraint>
      <Principal Role=""_Employees"">
        <PropertyRef Name=""_EmployeeId"" />
      </Principal>
      <Dependent Role=""_WorkOrders"">
        <PropertyRef Name=""_EmployeeId"" />
      </Dependent>
    </ReferentialConstraint>
  </Association>
  <EntityContainer Name=""_dbo"">
    <EntitySet Name=""_Employees"" EntityType=""Self._Employees"" />
    <EntitySet Name=""_Offices"" EntityType=""Self._Offices"" />
    <EntitySet Name=""_Buildings"" EntityType=""Self._Buildings"" />
    <EntitySet Name=""_MailRooms"" EntityType=""Self._MailRooms"" />
    <EntitySet Name=""_Whiteboards"" EntityType=""Self._Whiteboards"" />
    <EntitySet Name=""_BuildingDetails"" EntityType=""Self._BuildingDetails"" />
    <EntitySet Name=""_WorkOrders"" EntityType=""Self._WorkOrders"" />
    <AssociationSet Name=""_CurrentEmployee_Manager"" Association=""Self._CurrentEmployee_Manager"">
      <End Role=""_Employees"" EntitySet=""_Employees"" />
      <End Role=""_EmployeesSelf"" EntitySet=""_Employees"" />
    </AssociationSet>
    <AssociationSet Name=""_CurrentEmployee_Office"" Association=""Self._CurrentEmployee_Office"">
      <End Role=""_Offices"" EntitySet=""_Offices"" />
      <End Role=""_Employees"" EntitySet=""_Employees"" />
    </AssociationSet>
    <AssociationSet Name=""_Building_Offices"" Association=""Self._Building_Offices"">
      <End Role=""_Buildings"" EntitySet=""_Buildings"" />
      <End Role=""_Offices"" EntitySet=""_Offices"" />
    </AssociationSet>
    <AssociationSet Name=""_MailRoom_Building"" Association=""Self._MailRoom_Building"">
      <End Role=""_Buildings"" EntitySet=""_Buildings"" />
      <End Role=""_MailRooms"" EntitySet=""_MailRooms"" />
    </AssociationSet>
    <AssociationSet Name=""_Whiteboard_Office"" Association=""Self._Whiteboard_Office"">
      <End Role=""_Offices"" EntitySet=""_Offices"" />
      <End Role=""_Whiteboards"" EntitySet=""_Whiteboards"" />
    </AssociationSet>
    <AssociationSet Name=""_BuildingDetail_Building"" Association=""Self._BuildingDetail_Building"">
      <End Role=""_Buildings"" EntitySet=""_Buildings"" />
      <End Role=""_BuildingDetails"" EntitySet=""_BuildingDetails"" />
    </AssociationSet>
    <AssociationSet Name=""_WorkOrder_Employee"" Association=""Self._WorkOrder_Employee"">
      <End Role=""_Employees"" EntitySet=""_Employees"" />
      <End Role=""_WorkOrders"" EntitySet=""_WorkOrders"" />
    </AssociationSet>
  </EntityContainer>
  <Function Name=""_AllOffices"" Aggregate=""false"" BuiltIn=""false"" NiladicFunction=""false"" IsComposable=""false"" ParameterTypeSemantics=""AllowImplicitConversion"" Schema=""_dbo"" />
  <Function Name=""_AllSiteInfo"" Aggregate=""false"" BuiltIn=""false"" NiladicFunction=""false"" IsComposable=""false"" ParameterTypeSemantics=""AllowImplicitConversion"" Schema=""_dbo"" />
  <Function Name=""_EmployeeIdsInOffice"" Aggregate=""false"" BuiltIn=""false"" NiladicFunction=""false"" IsComposable=""false"" ParameterTypeSemantics=""AllowImplicitConversion"" Schema=""_dbo"">
    <Parameter Name=""_OfficeNumber"" Type=""nvarchar"" Mode=""In"" />
    <Parameter Name=""_BuildingId"" Type=""uniqueidentifier"" Mode=""In"" />
  </Function>
  <Function Name=""_OfficesInBuilding"" Aggregate=""false"" BuiltIn=""false"" NiladicFunction=""false"" IsComposable=""false"" ParameterTypeSemantics=""AllowImplicitConversion"" Schema=""_dbo"">
    <Parameter Name=""_BuildingId"" Type=""uniqueidentifier"" Mode=""In"" />
  </Function>
  <Function Name=""_SkimOffLeaveBalance"" Aggregate=""false"" BuiltIn=""false"" NiladicFunction=""false"" IsComposable=""false"" ParameterTypeSemantics=""AllowImplicitConversion"" Schema=""_dbo"">
    <Parameter Name=""_First"" Type=""nvarchar"" Mode=""In"" />
    <Parameter Name=""_Last"" Type=""nvarchar"" Mode=""In"" />
  </Function>
</Schema>";

        private const string UnderpantsMsl = @"
<Mapping Space=""C-S"" xmlns=""http://schemas.microsoft.com/ado/2009/11/mapping/cs"">
  <EntityContainerMapping StorageEntityContainer=""_dbo"" CdmEntityContainer=""_AdvancedPatternsModelFirstContext"">
    <EntitySetMapping Name=""_Employees"">
      <EntityTypeMapping TypeName=""IsTypeOf(_AdvancedPatternsModelFirst._EmployeeMf)"">
        <MappingFragment StoreEntitySet=""_Employees"">
          <ScalarProperty Name=""_EmployeeId"" ColumnName=""_EmployeeId"" />
          <ScalarProperty Name=""_FirstName"" ColumnName=""_FirstName"" />
          <ScalarProperty Name=""_LastName"" ColumnName=""_LastName"" />
        </MappingFragment>
      </EntityTypeMapping>
      <EntityTypeMapping TypeName=""_AdvancedPatternsModelFirst._CurrentEmployeeMf"">
        <MappingFragment StoreEntitySet=""_Employees"">
          <ScalarProperty Name=""_EmployeeId"" ColumnName=""_EmployeeId"" />
          <ScalarProperty Name=""_FirstName"" ColumnName=""_FirstName"" />
          <ScalarProperty Name=""_LastName"" ColumnName=""_LastName"" />
          <ScalarProperty Name=""_LeaveBalance"" ColumnName=""_LeaveBalance"" />
          <Condition Value=""CurrentEmployee"" ColumnName=""_Discriminator"" />
        </MappingFragment>
      </EntityTypeMapping>
      <EntityTypeMapping TypeName=""_AdvancedPatternsModelFirst._PastEmployeeMf"">
        <MappingFragment StoreEntitySet=""_Employees"">
          <ScalarProperty Name=""_EmployeeId"" ColumnName=""_EmployeeId"" />
          <ScalarProperty Name=""_FirstName"" ColumnName=""_FirstName"" />
          <ScalarProperty Name=""_LastName"" ColumnName=""_LastName"" />
          <ScalarProperty Name=""_TerminationDate"" ColumnName=""_TerminationDate"" />
          <Condition Value=""PastEmployee"" ColumnName=""_Discriminator"" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name=""_Offices"">
      <EntityTypeMapping TypeName=""_AdvancedPatternsModelFirst._OfficeMf"">
        <MappingFragment StoreEntitySet=""_Offices"">
          <ScalarProperty Name=""_Number"" ColumnName=""_Number"" />
          <ScalarProperty Name=""_BuildingId"" ColumnName=""_BuildingId"" />
          <ScalarProperty Name=""_Description"" ColumnName=""_Description"" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name=""_Buildings"">
      <EntityTypeMapping TypeName=""_AdvancedPatternsModelFirst._BuildingMf"">
        <MappingFragment StoreEntitySet=""_Buildings"">
          <ScalarProperty Name=""_BuildingId"" ColumnName=""_BuildingId"" />
          <ScalarProperty Name=""_Name"" ColumnName=""_Name"" />
          <ScalarProperty Name=""_Value"" ColumnName=""_Value"" />
          <ComplexProperty Name=""_Address"" TypeName=""_AdvancedPatternsModelFirst._AddressMf"">
            <ScalarProperty Name=""_Street"" ColumnName=""_Street"" />
            <ScalarProperty Name=""_City"" ColumnName=""_City"" />
            <ScalarProperty Name=""_State"" ColumnName=""_State"" />
            <ScalarProperty Name=""_ZipCode"" ColumnName=""_ZipCode"" />
            <ComplexProperty Name=""_SiteInfo"" TypeName=""_AdvancedPatternsModelFirst._SiteInfoMf"">
              <ScalarProperty Name=""_Zone"" ColumnName=""_Zone"" />
              <ScalarProperty Name=""_Environment"" ColumnName=""_Environment"" />
            </ComplexProperty>
          </ComplexProperty>
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name=""_MailRooms"">
      <EntityTypeMapping TypeName=""_AdvancedPatternsModelFirst._MailRoomMf"">
        <MappingFragment StoreEntitySet=""_MailRooms"">
          <ScalarProperty Name=""_id"" ColumnName=""_id"" />
          <ScalarProperty Name=""_BuildingId"" ColumnName=""_BuildingId"" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name=""_Whiteboards"">
      <EntityTypeMapping TypeName=""_AdvancedPatternsModelFirst._WhiteboardMf"">
        <MappingFragment StoreEntitySet=""_Whiteboards"">
          <ScalarProperty Name=""_iD"" ColumnName=""_iD"" />
          <ScalarProperty Name=""_AssetTag"" ColumnName=""_AssetTag"" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name=""_BuildingDetails"">
      <EntityTypeMapping TypeName=""_AdvancedPatternsModelFirst._BuildingDetailMf"">
        <MappingFragment StoreEntitySet=""_BuildingDetails"">
          <ScalarProperty Name=""_BuildingId"" ColumnName=""_BuildingId"" />
          <ScalarProperty Name=""_Details"" ColumnName=""_Details"" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <EntitySetMapping Name=""_WorkOrders"">
      <EntityTypeMapping TypeName=""_AdvancedPatternsModelFirst._WorkOrderMf"">
        <MappingFragment StoreEntitySet=""_WorkOrders"">
          <ScalarProperty Name=""_WorkOrderId"" ColumnName=""_WorkOrderId"" />
          <ScalarProperty Name=""_EmployeeId"" ColumnName=""_EmployeeId"" />
          <ScalarProperty Name=""_Details"" ColumnName=""_Details"" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
    <AssociationSetMapping Name=""_CurrentEmployee_Manager"" TypeName=""_AdvancedPatternsModelFirst._CurrentEmployee_Manager"" StoreEntitySet=""_Employees"">
      <EndProperty Name=""_CurrentEmployee_Manager_Target"">
        <ScalarProperty Name=""_EmployeeId"" ColumnName=""_CurrentEmployeeEmployeeId"" />
      </EndProperty>
      <EndProperty Name=""_CurrentEmployee_Manager_Source"">
        <ScalarProperty Name=""_EmployeeId"" ColumnName=""_EmployeeId"" />
      </EndProperty>
      <Condition IsNull=""false"" ColumnName=""_CurrentEmployeeEmployeeId"" />
    </AssociationSetMapping>
    <AssociationSetMapping Name=""_Whiteboard_Office"" TypeName=""_AdvancedPatternsModelFirst._Whiteboard_Office"" StoreEntitySet=""_Whiteboards"">
      <EndProperty Name=""_Whiteboard_Office_Target"">
        <ScalarProperty Name=""_Number"" ColumnName=""_OfficeNumber"" />
        <ScalarProperty Name=""_BuildingId"" ColumnName=""_OfficeBuildingId"" />
      </EndProperty>
      <EndProperty Name=""_Whiteboard_Office_Source"">
        <ScalarProperty Name=""_iD"" ColumnName=""_iD"" />
      </EndProperty>
      <Condition IsNull=""false"" ColumnName=""_OfficeNumber"" />
      <Condition IsNull=""false"" ColumnName=""_OfficeBuildingId"" />
    </AssociationSetMapping>
    <AssociationSetMapping Name=""_CurrentEmployee_Office"" TypeName=""_AdvancedPatternsModelFirst._CurrentEmployee_Office"" StoreEntitySet=""_Employees"">
      <EndProperty Name=""_CurrentEmployee_Office_Target"">
        <ScalarProperty Name=""_Number"" ColumnName=""_OfficeNumber"" />
        <ScalarProperty Name=""_BuildingId"" ColumnName=""_OfficeBuildingId"" />
      </EndProperty>
      <EndProperty Name=""_CurrentEmployee_Office_Source"">
        <ScalarProperty Name=""_EmployeeId"" ColumnName=""_EmployeeId"" />
      </EndProperty>
      <Condition IsNull=""false"" ColumnName=""_OfficeNumber"" />
      <Condition IsNull=""false"" ColumnName=""_OfficeBuildingId"" />
    </AssociationSetMapping>
    <FunctionImportMapping FunctionImportName=""_AllOfficesStoredProc"" FunctionName=""_dboNamespace._AllOffices"" />
    <FunctionImportMapping FunctionImportName=""_EmployeeIdsInOfficeStoredProc"" FunctionName=""_dboNamespace._EmployeeIdsInOffice"" />
    <FunctionImportMapping FunctionImportName=""_OfficesInBuildingStoredProc"" FunctionName=""_dboNamespace._OfficesInBuilding"" />
    <FunctionImportMapping FunctionImportName=""_SkimOffLeaveBalanceStoredProc"" FunctionName=""_dboNamespace._SkimOffLeaveBalance"" />
    <FunctionImportMapping FunctionImportName=""_AllSiteInfoStoredProc"" FunctionName=""_dboNamespace._AllSiteInfo"">
      <ResultMapping>
        <ComplexTypeMapping TypeName=""_AdvancedPatternsModelFirst._SiteInfoMf"">
          <ScalarProperty Name=""_Zone"" ColumnName=""_Zone"" />
          <ScalarProperty Name=""_Environment"" ColumnName=""_Environment"" />
        </ComplexTypeMapping>
      </ResultMapping>
    </FunctionImportMapping>
  </EntityContainerMapping>
</Mapping>";
    }
}
