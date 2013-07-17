' Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
Imports System.Data.Entity
Imports System.Data.Entity.Infrastructure
Imports System.Data.Entity.Core.Objects
Imports System.Data.Entity.TestHelpers
Imports System.Reflection
Imports System.Transactions
Imports AdvancedPatternsVB
Imports Another.Place
Imports Xunit.Extensions

''' <summary>
''' Visual Basic tests that use T4 models generated in Visual Basic.
''' </summary>
Public Class TemplateTests
    Inherits FunctionalTestBase

#Region "Infrastructure/setup"

    Private Const MemberBindingFlags As BindingFlags = BindingFlags.Instance Or BindingFlags.NonPublic Or BindingFlags.Public

    Shared Sub New()
        DbConfiguration.SetConfiguration(New FunctionalTestsConfiguration())
        InitializeModelFirstDatabases(False)

        Using context = New AdvancedPatternsModelFirstContext
            Database.SetInitializer(New AdvancedPatternsModelFirstInitializer())
            context.Database.Initialize(False)
        End Using

        Using context = New MonsterModel
            Database.SetInitializer(New DropCreateDatabaseAlways(Of MonsterModel))
            context.Database.Initialize(False)
        End Using
    End Sub

#End Region

#Region "Simple tests that the context and entities works"

    <Fact()> _
    Public Sub Read_and_write_using_AdvancedPatternsModelFirst_created_from_Visual_Basic_T4_template()

        Dim building18 = CreateBuilding()

        Using New TransactionScope()
            Using context = New AdvancedPatternsModelFirstContext()
                context.Buildings.Add(building18)

                Dim foundBuilding = context.Buildings.Find(New Guid(Building18Id))
                Assert.Equal("Building 18", foundBuilding.Name)

                context.SaveChanges()
            End Using

            Using context = New AdvancedPatternsModelFirstContext()
                Dim foundBuilding = context.Buildings.Find(New Guid(Building18Id))
                Assert.Equal("Building 18", foundBuilding.Name)
                Assert.Equal(3, context.Entry(foundBuilding).Collection(Function(b) b.Offices).Query().Count())

                Dim arthursOffice = context.Offices.[Single](Function(o) o.Number = "1/1125")
                Assert.Same(foundBuilding, arthursOffice.GetBuilding())
            End Using
        End Using

    End Sub

    Private Const Building18Id As String = "18181818-1818-1818-1818-181818181818"

    Private Shared Function CreateBuilding() As BuildingMf

        Dim building18 = New BuildingMf(New Guid(Building18Id), "Building 18", -1000000D,
                                        New AddressMf("Across From 25", "Redmond", "WA", "98052", 7, "70's"))

        Dim office = New OfficeMf
        office.Number = "1/1125"
        building18.Offices.Add(office)
        office = New OfficeMf
        office.Number = "1/1120"
        building18.Offices.Add(office)
        office = New OfficeMf
        office.Number = "1/1123"
        building18.Offices.Add(office)

        Return building18

    End Function

    <Fact(), AutoRollback()> _
    Public Sub Read_and_write_using_MonsterModel_created_from_Visual_Basic_T4_template()

        Dim orderId As Integer
        Dim customerId As System.Nullable(Of Integer)

        Using context = New MonsterModel()
            Dim entry = context.Entry(CreateOrder())
            entry.State = EntityState.Added

            context.SaveChanges()

            orderId = entry.Entity.OrderId
            customerId = entry.Entity.CustomerId
        End Using

        Using context = New MonsterModel()
            Dim order = context.Order.Include(Function(o) o.Customer).[Single](Function(o) CBool(o.CustomerId = customerId))

            Assert.Equal(orderId, order.OrderId)
            Assert.True(order.Customer.Orders.Contains(order))
        End Using

    End Sub

    Private Shared Function CreateOrder() As OrderMm

        Dim order = New OrderMm()

        order.OrderId = -1
        order.Customer = New CustomerMm()
        order.Customer.CustomerId = -1
        order.Customer.Name = "Jim Lathrop"
        order.Customer.ContactInfo.Email = "jil@newmonics.com"
        order.Customer.ContactInfo.HomePhone.PhoneNumber = "555-5555-551"
        order.Customer.ContactInfo.HomePhone.Extension = "x555"
        order.Customer.ContactInfo.HomePhone.PhoneType = PhoneTypeMm.Cell
        order.Customer.ContactInfo.WorkPhone.PhoneNumber = "555-5555-552"
        order.Customer.ContactInfo.WorkPhone.Extension = "x555"
        order.Customer.ContactInfo.WorkPhone.PhoneType = PhoneTypeMm.Land
        order.Customer.ContactInfo.MobilePhone.PhoneNumber = "555-5555-553"
        order.Customer.ContactInfo.MobilePhone.Extension = "x555"
        order.Customer.ContactInfo.MobilePhone.PhoneType = PhoneTypeMm.Satellite
        order.Customer.Auditing.ModifiedBy = "barney"
        order.Customer.Auditing.ModifiedDate = DateTime.Now
        order.Customer.Auditing.Concurrency.QueriedDateTime = DateTime.Now
        order.Customer.Auditing.Concurrency.Token = "1"
        order.Concurrency.QueriedDateTime = DateTime.Now
        order.Concurrency.Token = "1"

        Return order

    End Function

#End Region

#Region "Function import tests"

    Public Shared ReadOnly KnownBuildingGuid As New Guid("21EC2020-3AEA-1069-A2DD-08002B30309D")

    <Fact()> _
    Public Sub Can_read_entities_from_a_stored_proc_mapped_to_a_function_import()
        Using context = New AdvancedPatternsModelFirstContext()
            Dim offices = context.AllOfficesStoredProc().ToList()

            Assert.Equal(4, offices.Count)
            Assert.Equal(4, context.Offices.Local.Count)
            Dim expected = New List(Of String)() From {"1/1221", "1/1223", "2/1458", "2/1789"}
            expected.ForEach(Function(n) offices.Where(Function(o) o.Number = n).[Single]())
        End Using
    End Sub

    <Fact()> _
    Public Sub Can_read_entities_from_a_stored_proc_mapped_to_a_function_import_with_merge_option()
        Using context = New AdvancedPatternsModelFirstContext()
            Dim offices = context.AllOfficesStoredProc(MergeOption.NoTracking).ToList()

            Assert.Equal(4, offices.Count)
            Assert.Equal(0, context.Offices.Local.Count)
            Dim expected = New List(Of String)() From {"1/1221", "1/1223", "2/1458", "2/1789"}
            expected.ForEach(Function(n) offices.Where(Function(o) o.Number = n).[Single]())
        End Using
    End Sub

    <Fact()> _
    Public Sub Can_read_entities_from_a_stored_proc_with_parameters_mapped_to_a_function_import()
        Using context = New AdvancedPatternsModelFirstContext()
            Dim offices = context.OfficesInBuildingStoredProc(KnownBuildingGuid).ToList()

            Assert.Equal(2, offices.Count)
            Assert.Equal(2, context.Offices.Local.Count)
            Dim expected = New List(Of String)() From {"1/1221", "1/1223"}
            expected.ForEach(Function(n) offices.Where(Function(o) o.Number = n).[Single]())
        End Using
    End Sub

    <Fact()> _
    Public Sub Can_read_entities_from_a_stored_proc_with_parameters_mapped_to_a_function_import_with_merge_option()
        Using context = New AdvancedPatternsModelFirstContext()
            Dim offices = context.OfficesInBuildingStoredProc(KnownBuildingGuid, MergeOption.NoTracking).ToList()

            Assert.Equal(2, offices.Count)
            Assert.Equal(0, context.Offices.Local.Count)
            Dim expected = New List(Of String)() From {"1/1221", "1/1223"}
            expected.ForEach(Function(n) offices.Where(Function(o) o.Number = n).[Single]())
        End Using
    End Sub

    <Fact()> _
    Public Sub Can_read_complex_objects_from_a_stored_proc_mapped_to_a_function_import()
        Using context = New AdvancedPatternsModelFirstContext()
            Dim siteInfo = context.AllSiteInfoStoredProc().ToList()

            Assert.Equal(2, siteInfo.Count)
            Dim expected = New List(Of String)() From {"Clean", "Contaminated"}
            expected.ForEach(Function(n) siteInfo.Where(Function(o) o.Environment = n).[Single]())
        End Using
    End Sub

    <Fact()> _
    Public Sub Can_read_scalar_return_from_a_stored_proc_mapped_to_a_function_import()
        Using context = New AdvancedPatternsModelFirstContext()
            Dim employeeIds = context.EmployeeIdsInOfficeStoredProc("1/1221", KnownBuildingGuid).ToList()

            Assert.Equal(1, employeeIds.Count)
            Assert.Equal("Rowan", context.Employees.Find(employeeIds.[Single]()).FirstName)
        End Using
    End Sub

    <Fact()> _
    Public Sub Can_execute_a_stored_proc_that_returns_no_results_mapped_to_a_function_import()
        Using context = New AdvancedPatternsModelFirstContext()
            Dim result = context.SkimOffLeaveBalanceStoredProc("Arthur", "Vickers")

            Assert.Equal(-1, result)
            Assert.Equal(0D, context.[Set](Of CurrentEmployeeMf)().Where(Function(e) e.FirstName = "Arthur").[Single]().LeaveBalance)
        End Using
    End Sub

    <Fact()> _
    Public Sub Executing_a_stored_proc_mapped_to_a_function_import_honors_append_only_merge_option()
        Using context = New AdvancedPatternsModelFirstContext()
            Dim office = context.Offices.Find("1/1221", KnownBuildingGuid)
            context.Entry(office).Property("Description").CurrentValue = "Test"

            Dim offices = context.AllOfficesStoredProc(MergeOption.AppendOnly).ToList()

            Assert.True(context.Entry(office).State = EntityState.Modified)
            Assert.True(context.ChangeTracker.Entries(Of OfficeMf)().Count = 4)
        End Using
    End Sub

    <Fact()> _
    Public Sub Executing_a_stored_proc_mapped_to_a_function_import_honors_no_tracking_merge_option()
        Using context = New AdvancedPatternsModelFirstContext()
            Dim offices = context.AllOfficesStoredProc(MergeOption.NoTracking).ToList()

            Assert.True(context.ChangeTracker.Entries(Of OfficeMf)().Count() = 0)
        End Using
    End Sub

    <Fact()> _
    Public Sub Executing_a_stored_proc_mapped_to_a_function_import_with_merge_option_overwrite_changes()
        Using context = New AdvancedPatternsModelFirstContext()
            Dim office = context.Offices.Find("1/1221", KnownBuildingGuid)
            context.Entry(office).Property("Description").CurrentValue = "Test"

            Dim offices = context.AllOfficesStoredProc(MergeOption.OverwriteChanges).ToList()

            Assert.True(context.Entry(office).State = EntityState.Unchanged)
            Assert.True(context.ChangeTracker.Entries(Of OfficeMf)().Count() = 4)
        End Using
    End Sub

    <Fact()> _
    Public Sub Executing_a_stored_proc_mapped_to_a_function_import_with_merge_option_preserve_changes()
        Using context = New AdvancedPatternsModelFirstContext()
            Dim office1 = context.Offices.Find("1/1221", KnownBuildingGuid)
            Dim office2 = context.Offices.Find("1/1223", KnownBuildingGuid)
            context.Entry(office2).Property("Description").CurrentValue = "Test"

            Dim offices = context.AllOfficesStoredProc(MergeOption.PreserveChanges).ToList()

            Assert.True(context.Entry(office1).State = EntityState.Unchanged)
            Assert.True(context.Entry(office2).State = EntityState.Modified)
            Assert.True(context.ChangeTracker.Entries(Of OfficeMf)().Count() = 4)
        End Using
    End Sub

#End Region

#Region "Tests for specific properties of the generated code"

    <Fact()> _
    Public Sub VB_DbContext_template_creates_collections_that_are_initialized_with_HashSets()
        Assert.IsType(GetType(HashSet(Of OrderLineMm)), New OrderMm().OrderLines)
    End Sub

    <Fact()> _
    Public Sub VB_DbContext_template_initializes_complex_properties_on_entities()
        Assert.NotNull(New CustomerMm().ContactInfo)
    End Sub

    <Fact()> _
    Public Sub VB_DbContext_template_initializes_complex_properties_on_complex_objects()
        Assert.NotNull(New CustomerMm().ContactInfo.HomePhone)
    End Sub

    <Fact()> _
    Public Sub VB_DbContext_template_initializes_default_values()
        Assert.Equal(1, New OrderLineMm().Quantity)
        Assert.Equal("C", New LicenseMm().LicenseClass)
    End Sub

    <Fact()> _
    Public Sub VB_DbContext_template_initializes_default_values_on_complex_objects()
        Assert.Equal("None", New PhoneMm().Extension)
    End Sub

    <Fact()> _
    Public Sub VB_DbContext_template_creates_non_virtual_scalar_and_complex_properties()
        Assert.False(GetType(CustomerMm).GetProperty("Name").GetGetMethod().IsVirtual)
        Assert.False(GetType(CustomerMm).GetProperty("Name").GetSetMethod().IsVirtual)
        Assert.False(GetType(CustomerMm).GetProperty("ContactInfo").GetGetMethod().IsVirtual)
        Assert.False(GetType(CustomerMm).GetProperty("ContactInfo").GetSetMethod().IsVirtual)
    End Sub

    <Fact()> _
    Public Sub VB_DbContext_template_creates_virtual_collection_and_reference_navigation_properties()
        Assert.True(GetType(CustomerMm).GetProperty("Orders").GetGetMethod().IsVirtual)
        Assert.True(GetType(CustomerMm).GetProperty("Orders").GetSetMethod().IsVirtual)
        Assert.True(GetType(CustomerMm).GetProperty("Info").GetGetMethod().IsVirtual)
        Assert.True(GetType(CustomerMm).GetProperty("Info").GetSetMethod().IsVirtual)
    End Sub

    <Fact()> _
    Public Sub VB_DbContext_template_creates_an_abstract_class_for_abstract_types_in_the_model()
        Assert.True(GetType(EmployeeMf).IsAbstract)
    End Sub

    <Fact()> _
    Public Sub VB_DbContext_template_can_create_private_non_virtual_nav_prop()
        Dim navProp = GetType(OfficeMf).GetProperty("Building", MemberBindingFlags)
        Assert.True(navProp.GetGetMethod(nonPublic:=True).IsPrivate)
        Assert.False(navProp.GetGetMethod(nonPublic:=True).IsVirtual)
        Assert.True(navProp.GetSetMethod(nonPublic:=True).IsPrivate)
        Assert.False(navProp.GetSetMethod(nonPublic:=True).IsVirtual)
    End Sub

    <Fact()> _
    Public Sub VB_DbContext_template_can_create_fully_internal_nav_prop()
        Dim navProp = GetType(WorkOrderMf).GetProperty("Employee", MemberBindingFlags)
        Assert.True(navProp.GetGetMethod(nonPublic:=True).IsAssembly)
        Assert.True(navProp.GetGetMethod(nonPublic:=True).IsVirtual)
        Assert.True(navProp.GetSetMethod(nonPublic:=True).IsAssembly)
        Assert.True(navProp.GetSetMethod(nonPublic:=True).IsVirtual)
    End Sub

    <Fact()> _
    Public Sub VB_DbContext_template_can_create_nav_prop_with_specific_getter_access()
        Dim navProp = GetType(OfficeMf).GetProperty("WhiteBoards", MemberBindingFlags)
        Assert.True(navProp.GetGetMethod(nonPublic:=True).IsAssembly)
        Assert.True(navProp.GetGetMethod(nonPublic:=True).IsVirtual)
        Assert.True(navProp.GetSetMethod(nonPublic:=True).IsPublic)
        Assert.True(navProp.GetSetMethod(nonPublic:=True).IsVirtual)
    End Sub

    <Fact()> _
    Public Sub VB_DbContext_template_can_create_nav_prop_with_specific_setter_access()
        Dim navProp = GetType(BuildingMf).GetProperty("MailRooms", MemberBindingFlags)
        Assert.True(navProp.GetGetMethod(nonPublic:=True).IsAssembly)
        Assert.False(navProp.GetGetMethod(nonPublic:=True).IsVirtual) ' Can't have virtual and private setter in VB
        Assert.True(navProp.GetSetMethod(nonPublic:=True).IsPrivate)
        Assert.False(navProp.GetSetMethod(nonPublic:=True).IsVirtual)
    End Sub

    <Fact()> _
    Public Sub VB_DbContext_template_can_create_fully_internal_primitive_prop()
        Dim prop = GetType(BuildingMf).GetProperty("Value", MemberBindingFlags)
        Assert.True(prop.GetGetMethod(nonPublic:=True).IsAssembly)
        Assert.True(prop.GetSetMethod(nonPublic:=True).IsAssembly)
    End Sub

    <Fact()> _
    Public Sub VB_DbContext_template_can_create_primitive_prop_with_specific_getter_access()
        Dim prop = GetType(EmployeeMf).GetProperty("LastName", MemberBindingFlags)
        Assert.True(prop.GetGetMethod(nonPublic:=True).IsPrivate)
        Assert.True(prop.GetSetMethod(nonPublic:=True).IsAssembly)
    End Sub

    <Fact()> _
    Public Sub VB_DbContext_template_can_create_primitive_prop_with_specific_setter_access()
        Dim prop = GetType(EmployeeMf).GetProperty("FirstName", MemberBindingFlags)
        Assert.True(prop.GetGetMethod(nonPublic:=True).IsPublic)
        Assert.True(prop.GetSetMethod(nonPublic:=True).IsPrivate)
    End Sub

    <Fact()> _
    Public Sub VB_DbContext_template_can_create_fully_internal_primitive_prop_on_a_complex_type()
        Dim prop = GetType(AddressMf).GetProperty("State", MemberBindingFlags)
        Assert.True(prop.GetGetMethod(nonPublic:=True).IsAssembly)
        Assert.True(prop.GetSetMethod(nonPublic:=True).IsAssembly)
    End Sub

    <Fact()> _
    Public Sub VB_DbContext_template_can_create_primitive_prop_on_a_complex_type_with_specific_getter_access()
        Dim prop = GetType(AddressMf).GetProperty("City", MemberBindingFlags)
        Assert.True(prop.GetGetMethod(nonPublic:=True).IsPrivate)
        Assert.True(prop.GetSetMethod(nonPublic:=True).IsPublic)
    End Sub

    <Fact()> _
    Public Sub VB_DbContext_template_can_create_primitive_prop_on_a_complex_type_with_specific_setter_access()
        Dim prop = GetType(AddressMf).GetProperty("ZipCode", MemberBindingFlags)
        Assert.True(prop.GetGetMethod(nonPublic:=True).IsAssembly)
        Assert.True(prop.GetSetMethod(nonPublic:=True).IsPrivate)
    End Sub

    <Fact()> _
    Public Sub VB_DbContext_template_can_create_complex_prop_with_different_getter_and_setter_access()
        Dim prop = GetType(BuildingMf).GetProperty("Address", MemberBindingFlags)
        Assert.True(prop.GetGetMethod(nonPublic:=True).IsAssembly)
        Assert.True(prop.GetSetMethod(nonPublic:=True).IsPrivate)
    End Sub

    <Fact()> _
    Public Sub VB_DbContext_template_can_create_complex_prop_on_a_complex_type_with_different_getter_and_setter_access()
        Dim prop = GetType(AddressMf).GetProperty("SiteInfo", MemberBindingFlags)
        Assert.True(prop.GetGetMethod(nonPublic:=True).IsAssembly)
        Assert.True(prop.GetSetMethod(nonPublic:=True).IsPrivate)
    End Sub

    <Fact()> _
    Public Sub VB_DbContext_template_can_create_non_public_DbSet_property()
        Dim prop = GetType(AdvancedPatternsModelFirstContext).GetProperty("MailRooms", MemberBindingFlags)
        Assert.True(prop.GetGetMethod(nonPublic:=True).IsAssembly)
        Assert.True(prop.GetSetMethod(nonPublic:=True).IsAssembly)
    End Sub

    <Fact()> _
    Public Sub VB_DbContext_template_can_create_non_public_complex_type()
        Assert.True(GetType(SiteInfoMf).IsNotPublic)
    End Sub

    <Fact()> _
    Public Sub VB_DbContext_template_can_create_non_public_entity_type()
        Assert.True(GetType(MailRoomMf).IsNotPublic)
    End Sub

    <Fact()> _
    Public Sub VB_DbContext_template_can_create_non_public_context_type()
        Assert.True(GetType(AdvancedPatternsModelFirstContext).IsNotPublic)
    End Sub

    <Fact()> _
    Public Sub CSharp_DbContext_template_has_lazy_loading_enabled_by_default()
        Using context = New MonsterModel()
            Assert.True(context.Configuration.LazyLoadingEnabled)
        End Using
    End Sub

    <Fact()> _
    Public Sub CSharp_DbContext_template_allows_lazy_loading_to_be_turned_off()
        Using context = New AdvancedPatternsModelFirstContext()
            Assert.False(context.Configuration.LazyLoadingEnabled)
        End Using
    End Sub

    <Fact()> _
    Public Sub VB_DbContext_template_creates_sets_for_all_base_types_but_not_derived_types()
        Dim expectedMonsterSets = New String() { _
            "Customer", "Barcode", "IncorrectScan", "BarcodeDetail", "Complaint", "Resolution", _
            "Login", "SuspiciousActivity", "SmartCard", "RSAToken", "PasswordReset", "PageView", _
            "LastLogin", "Message", "Order", "OrderNote", "OrderQualityCheck", "OrderLine", _
            "Product", "ProductDetail", "ProductReview", "ProductPhoto", "ProductWebFeature", "Supplier", _
            "SupplierLogo", "SupplierInfo", "CustomerInfo", "Computer", "ComputerDetail", "Driver", _
            "License"}

        Dim notExpectedMonsterSets = New String() {"ProductPageView", "BackOrderLine", "DiscontinuedProduct"}

        Dim properties = New HashSet(Of String)(GetType(MonsterModel).GetProperties().[Select](Function(p) p.Name))

        For Each expected As String In expectedMonsterSets
            Assert.True(properties.Contains(expected), String.Format("No DbSet property found for {0}", expected))
        Next

        For Each notExpected As String In notExpectedMonsterSets
            Assert.False(properties.Contains(notExpected), String.Format("Found unexpected DbSet property for {0}", notExpected))
        Next
    End Sub

    <Fact()> _
    Public Sub VB_DbContext_template_creates_a_derived_class_for_derived_types_in_the_model()
        Assert.True(GetType(CurrentEmployeeMf).BaseType = GetType(EmployeeMf))
    End Sub

    <Fact()> _
    Public Sub CSharp_DbContext_template_can_create_private_non_virtual_function_import()
        Assert.True(GetType(MonsterModel).GetMethod("FunctionImport1", MemberBindingFlags).IsPrivate)
        Assert.False(GetType(MonsterModel).GetMethod("FunctionImport1", MemberBindingFlags).IsVirtual)
    End Sub

    <Fact()> _
    Public Sub CSharp_DbContext_template_can_create_function_import_with_specific_method_access()
        Assert.True(GetType(MonsterModel).GetMethod("FunctionImport2", MemberBindingFlags, Nothing, New Type() {}, Nothing).IsAssembly)
        Assert.True(GetType(MonsterModel).GetMethod("FunctionImport2", MemberBindingFlags, Nothing, New Type() {GetType(MergeOption)}, Nothing).IsAssembly)
    End Sub

    <Fact()> _
    Public Sub CSharp_DbContext_template_creates_virtual_function_imports()
        Assert.True(GetType(AdvancedPatternsModelFirstContext).GetMethod("AllOfficesStoredProc", MemberBindingFlags, Nothing, New Type() {}, Nothing).IsVirtual)
        Assert.True(GetType(AdvancedPatternsModelFirstContext).GetMethod("AllOfficesStoredProc", MemberBindingFlags, Nothing, New Type() {GetType(MergeOption)}, Nothing).IsVirtual)
    End Sub

    <Fact()> _
    Public Sub CSharp_DbContext_template_can_create_nullable_prop()
        Assert.True(GetType(CurrentEmployeeMf).GetProperty("LeaveBalance", MemberBindingFlags).PropertyType = GetType(Nullable(Of Decimal)))
    End Sub

    <Fact()> _
    Public Sub CSharp_DbContext_template_can_create_nullable_prop_on_a_complex_type()
        Assert.True(GetType(SiteInfoMf).GetProperty("Zone", MemberBindingFlags).PropertyType = GetType(Nullable(Of Integer)))
    End Sub

#End Region

#Region "Code First mode"

    <Fact()> _
    Public Sub Template_generated_context_throws_when_used_in_Code_First_mode()
        Using context = New AdvancedPatternsModelFirstContext(SimpleConnectionString("AdvancedPatternsModelFirstContext"))

            Assert.Equal(New UnintentionalCodeFirstException().Message, Assert.Throws(Of UnintentionalCodeFirstException)(Sub() context.Database.Initialize(force:=False)).Message)
        End Using
    End Sub

#End Region

End Class

