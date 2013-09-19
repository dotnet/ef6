// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Functionals.Utilities;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Reflection;
    using System.Transactions;
    using Another.Place;
    using FunctionalTests.ProductivityApi.TemplateModels.CsAdvancedPatterns;
    using FunctionalTests.ProductivityApi.TemplateModels.CsMonsterModel;
    using Xunit;
    using Xunit.Extensions;

    ///<summary>
    ///    Tests for context/entity classes generated from productivity T4 templates.
    ///</summary>
    public class TemplateTests : FunctionalTestBase
    {
        #region Infrastructure/setup

        static TemplateTests()
        {
            TemplateTestsDatabaseInitializer.InitializeModelFirstDatabases();
        }

        #endregion

        #region Simple tests that the context and entities works

        private const string Building18Id = "18181818-1818-1818-1818-181818181818";

        [Fact]
        public void Read_and_write_using_AdvancedPatternsModelFirst_created_from_T4_template()
        {
            var building18 = CreateBuilding();

            using (new TransactionScope())
            {
                using (var context = new AdvancedPatternsModelFirstContext())
                {
                    context.Buildings.Add(building18);

                    var foundBuilding = context.Buildings.Find(new Guid(Building18Id));
                    Assert.Equal("Building 18", foundBuilding.Name);

                    context.SaveChanges();
                }

                using (var context = new AdvancedPatternsModelFirstContext())
                {
                    var foundBuilding = context.Buildings.Find(new Guid(Building18Id));
                    Assert.Equal("Building 18", foundBuilding.Name);
                    Assert.Equal(3, context.Entry(foundBuilding).Collection(b => b.Offices).Query().Count());

                    var arthursOffice = context.Offices.Single(o => o.Number == "1/1125");
                    Assert.Same(foundBuilding, arthursOffice.GetBuilding());
                }
            }
        }

        private static BuildingMf CreateBuilding()
        {
            var building18 = new BuildingMf(
                new Guid(Building18Id), "Building 18", -1000000m,
                new AddressMf("Across From 25", "Redmond", "WA", "98052", 7, "70's"));

            foreach (var office in new[]
                                       {
                                           new OfficeMf
                                               {
                                                   Number = "1/1125"
                                               },
                                           new OfficeMf
                                               {
                                                   Number = "1/1120"
                                               },
                                           new OfficeMf
                                               {
                                                   Number = "1/1123"
                                               },
                                       })
            {
                building18.Offices.Add(office);
            }

            return building18;
        }

        [Fact]
        [AutoRollback]
        public void Read_and_write_using_MonsterModel_created_from_T4_template()
        {
            int orderId;
            int? customerId;

            using (var context = new MonsterModel())
            {
                var entry = context.Entry(CreateOrder());
                entry.State = EntityState.Added;

                context.SaveChanges();

                orderId = entry.Entity.OrderId;
                customerId = entry.Entity.CustomerId;
            }

            using (var context = new MonsterModel())
            {
                var order = context.Order.Include(o => o.Customer).Single(o => o.CustomerId == customerId);

                Assert.Equal(orderId, order.OrderId);
                Assert.True(order.Customer.Orders.Contains(order));
            }
        }

        private static OrderMm CreateOrder()
        {
            return new OrderMm
                       {
                           OrderId = -1,
                           Customer =
                               new CustomerMm
                                   {
                                       CustomerId = -1,
                                       Name = "Jim Lathrop",
                                       ContactInfo =
                                           new ContactDetailsMm
                                               {
                                                   Email = "jil@newmonics.com",
                                                   HomePhone =
                                                       new PhoneMm
                                                           {
                                                               PhoneNumber = "555-5555-551",
                                                               Extension = "x555",
                                                               PhoneType = PhoneTypeMm.Cell
                                                           },
                                                   WorkPhone =
                                                       new PhoneMm
                                                           {
                                                               PhoneNumber = "555-5555-552",
                                                               Extension = "x555",
                                                               PhoneType = PhoneTypeMm.Land
                                                           },
                                                   MobilePhone =
                                                       new PhoneMm
                                                           {
                                                               PhoneNumber = "555-5555-553",
                                                               Extension = "x555",
                                                               PhoneType = PhoneTypeMm.Satellite
                                                           },
                                               },
                                       Auditing =
                                           new AuditInfoMm
                                               {
                                                   ModifiedBy = "barney",
                                                   ModifiedDate = DateTime.Now,
                                                   Concurrency =
                                                       new ConcurrencyInfoMm
                                                           {
                                                               QueriedDateTime = DateTime.Now,
                                                               Token = "1"
                                                           },
                                               },
                                   },
                           Concurrency = new ConcurrencyInfoMm
                                             {
                                                 QueriedDateTime = DateTime.Now,
                                                 Token = "1"
                                             },
                       };
        }

        #endregion

        #region Tests for default container name when using model/database first (Dev11 142609)

        [Fact]
        public void Default_container_name_is_set_when_there_is_a_single_container_in_the_model()
        {
            using (var context = new AdvancedPatternsModelFirstContext())
            {
                Assert.Equal(
                    "AdvancedPatternsModelFirstContext",
                    ((IObjectContextAdapter)context).ObjectContext.DefaultContainerName);
            }
        }

        #endregion

        #region Tests for not needing LoadFromAssembly with model/database first (Dev11 142609)

        [Fact]
        public void Object_space_types_are_loaded_when_code_drops_down_to_ObjectContext()
        {
            using (var context = new AdvancedPatternsModelFirstContext())
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;

                // The following would previously throw because it requires manual o-space loading
                var results =
                    objectContext.CreateQuery<object>(
                        "select e.Number from AdvancedPatternsModelFirstContext.Offices as e").ToList();

                Assert.Equal(4, results.Count);
            }
        }

        #endregion

        #region Function import tests

        [Fact]
        public void Can_read_entities_from_a_stored_proc_mapped_to_a_function_import()
        {
            using (var context = new AdvancedPatternsModelFirstContext())
            {
                var offices = context.AllOfficesStoredProc().ToList();

                Assert.Equal(4, offices.Count);
                Assert.Equal(4, context.Offices.Local.Count);
                var officeNumbers = new List<string>
                    {
                        "1/1221", "1/1223", "2/1458", "2/1789"
                    };

                foreach (var officeNumber in officeNumbers)
                {
                    offices.Single(o => o.Number == officeNumber);
                }
            }
        }

        [Fact]
        public void Can_read_entities_from_a_stored_proc_mapped_to_a_function_import_with_merge_option()
        {
            using (var context = new AdvancedPatternsModelFirstContext())
            {
                var offices = context.AllOfficesStoredProc(MergeOption.NoTracking).ToList();

                Assert.Equal(4, offices.Count);
                Assert.Equal(0, context.Offices.Local.Count);
                var officeNumbers = new List<string>
                    {
                        "1/1221", "1/1223", "2/1458", "2/1789"
                    };

                foreach (var officeNumber in officeNumbers)
                {
                    offices.Single(o => o.Number == officeNumber);
                }
            }
        }

        [Fact]
        public void Can_read_entities_from_a_stored_proc_with_parameters_mapped_to_a_function_import()
        {
            using (var context = new AdvancedPatternsModelFirstContext())
            {
                var offices =
                    context.OfficesInBuildingStoredProc(AdvancedPatternsModelFirstInitializer.KnownBuildingGuid).ToList();

                Assert.Equal(2, offices.Count);
                Assert.Equal(2, context.Offices.Local.Count);
                var officeNumbers = new List<string>
                    {
                        "1/1221", "1/1223"
                    };

                foreach (var officeNumber in officeNumbers)
                {
                    offices.Single(o => o.Number == officeNumber);
                }
            }
        }

        [Fact]
        public void Can_read_entities_from_a_stored_proc_with_parameters_mapped_to_a_function_import_with_merge_option()
        {
            using (var context = new AdvancedPatternsModelFirstContext())
            {
                var offices =
                    context.OfficesInBuildingStoredProc(
                        AdvancedPatternsModelFirstInitializer.KnownBuildingGuid,
                        MergeOption.NoTracking).ToList();

                Assert.Equal(2, offices.Count);
                Assert.Equal(0, context.Offices.Local.Count);
                var officeNumbers = new List<string>
                    {
                        "1/1221", "1/1223"
                    };

                foreach (var officeNumber in officeNumbers)
                {
                    offices.Single(o => o.Number == officeNumber);
                }
            }
        }

        [Fact]
        public void Can_read_complex_objects_from_a_stored_proc_mapped_to_a_function_import()
        {
            using (var context = new AdvancedPatternsModelFirstContext())
            {
                var siteInfo = context.AllSiteInfoStoredProc().ToList();

                Assert.Equal(2, siteInfo.Count);
                var officeNumbers = new List<string>
                    {
                        "Clean", "Contaminated"
                    };

                foreach (var officeNumber in officeNumbers)
                {
                    siteInfo.Single(o => o.Environment == officeNumber);
                }
            }
        }

        [Fact]
        public void Can_read_scalar_return_from_a_stored_proc_mapped_to_a_function_import()
        {
            using (var context = new AdvancedPatternsModelFirstContext())
            {
                var employeeIds =
                    context.EmployeeIdsInOfficeStoredProc(
                        "1/1221",
                        AdvancedPatternsModelFirstInitializer.KnownBuildingGuid).
                        ToList();

                Assert.Equal(1, employeeIds.Count);
                Assert.Equal("Rowan", context.Employees.Find(employeeIds.Single()).FirstName);
            }
        }

        [Fact]
        public void Can_execute_a_stored_proc_that_returns_no_results_mapped_to_a_function_import()
        {
            using (var context = new AdvancedPatternsModelFirstContext())
            {
                var result = context.SkimOffLeaveBalanceStoredProc("Arthur", "Vickers");

                Assert.Equal(-1, result);
                Assert.Equal(
                    0M,
                    context.Set<CurrentEmployeeMf>().Where(e => e.FirstName == "Arthur").Single().LeaveBalance);
            }
        }

        [Fact]
        public void Executing_a_stored_proc_mapped_to_a_function_import_honors_append_only_merge_option()
        {
            using (var context = new AdvancedPatternsModelFirstContext())
            {
                // Arrange
                var office = context.Offices.Find("1/1221", AdvancedPatternsModelFirstInitializer.KnownBuildingGuid);
                context.Entry(office).Property("Description").CurrentValue = "Test";

                // Act
                context.AllOfficesStoredProc(MergeOption.AppendOnly).ToList();

                // Verify
                Assert.True(context.Entry(office).State == EntityState.Modified);
                Assert.True(context.ChangeTracker.Entries<OfficeMf>().Count() == 4);
            }
        }

        [Fact]
        public void Executing_a_stored_proc_mapped_to_a_function_import_honors_no_tracking_merge_option()
        {
            using (var context = new AdvancedPatternsModelFirstContext())
            {
                // Act
                context.AllOfficesStoredProc(MergeOption.NoTracking).ToList();

                // Verify
                Assert.True(context.ChangeTracker.Entries<OfficeMf>().Count() == 0);
            }
        }

        [Fact]
        public void Executing_a_stored_proc_mapped_to_a_function_import_with_merge_option_overwrite_changes()
        {
            using (var context = new AdvancedPatternsModelFirstContext())
            {
                // Arrange
                var office = context.Offices.Find("1/1221", AdvancedPatternsModelFirstInitializer.KnownBuildingGuid);
                context.Entry(office).Property("Description").CurrentValue = "Test";

                // Act
                context.AllOfficesStoredProc(MergeOption.OverwriteChanges).ToList();

                // Verify
                Assert.True(context.Entry(office).State == EntityState.Unchanged);
                Assert.True(context.ChangeTracker.Entries<OfficeMf>().Count() == 4);
            }
        }

        [Fact]
        public void Executing_a_stored_proc_mapped_to_a_function_import_with_merge_option_preserve_changes()
        {
            using (var context = new AdvancedPatternsModelFirstContext())
            {
                // Arrange
                var office1 = context.Offices.Find("1/1221", AdvancedPatternsModelFirstInitializer.KnownBuildingGuid);
                var office2 = context.Offices.Find("1/1223", AdvancedPatternsModelFirstInitializer.KnownBuildingGuid);
                context.Entry(office2).Property("Description").CurrentValue = "Test";

                // Act
                context.AllOfficesStoredProc(MergeOption.PreserveChanges).ToList();

                // Verify
                Assert.True(context.Entry(office1).State == EntityState.Unchanged);
                Assert.True(context.Entry(office2).State == EntityState.Modified);
                Assert.True(context.ChangeTracker.Entries<OfficeMf>().Count() == 4);
            }
        }

        #endregion

        #region Tests for specific properties of the generated code

        [Fact]
        public void CSharp_DbContext_template_creates_collections_that_are_initialized_with_HashSets()
        {
            Assert.IsType<HashSet<OrderLineMm>>(new OrderMm().OrderLines);
        }

        [Fact]
        public void CSharp_DbContext_template_initializes_complex_properties_on_entities()
        {
            Assert.NotNull(new CustomerMm().ContactInfo);
        }

        [Fact]
        public void CSharp_DbContext_template_initializes_complex_properties_on_complex_objects()
        {
            Assert.NotNull(new CustomerMm().ContactInfo.HomePhone);
        }

        [Fact]
        public void CSharp_DbContext_template_initializes_default_values()
        {
            Assert.Equal(1, new OrderLineMm().Quantity);
            Assert.Equal("C", new LicenseMm().LicenseClass);
        }

        [Fact]
        public void CSharp_DbContext_template_initializes_default_values_on_complex_objects()
        {
            Assert.Equal("None", new PhoneMm().Extension);
        }

        [Fact]
        public void CSharp_DbContext_template_creates_non_virtual_scalar_and_complex_properties()
        {
            Assert.False(typeof(CustomerMm).GetDeclaredProperty("Name").Getter().IsVirtual);
            Assert.False(typeof(CustomerMm).GetDeclaredProperty("Name").Setter().IsVirtual);
            Assert.False(typeof(CustomerMm).GetDeclaredProperty("ContactInfo").Getter().IsVirtual);
            Assert.False(typeof(CustomerMm).GetDeclaredProperty("ContactInfo").Setter().IsVirtual);
        }

        [Fact]
        public void CSharp_DbContext_template_creates_virtual_collection_and_reference_navigation_properties()
        {
            Assert.True(typeof(CustomerMm).GetDeclaredProperty("Orders").Getter().IsVirtual);
            Assert.True(typeof(CustomerMm).GetDeclaredProperty("Orders").Setter().IsVirtual);
            Assert.True(typeof(CustomerMm).GetDeclaredProperty("Info").Getter().IsVirtual);
            Assert.True(typeof(CustomerMm).GetDeclaredProperty("Info").Setter().IsVirtual);
        }

        [Fact]
        public void CSharp_DbContext_template_creates_an_abstract_class_for_abstract_types_in_the_model()
        {
            Assert.True(typeof(EmployeeMf).IsAbstract);
        }

        [Fact]
        public void CSharp_DbContext_template_can_create_private_non_virtual_nav_prop()
        {
            var navProp = typeof(OfficeMf).GetDeclaredProperty("Building");
            Assert.True(navProp.Getter().IsPrivate);
            Assert.False(navProp.Getter().IsVirtual);
            Assert.True(navProp.Setter().IsPrivate);
            Assert.False(navProp.Setter().IsVirtual);
        }

        [Fact]
        public void CSharp_DbContext_template_can_create_fully_internal_nav_prop()
        {
            var navProp = typeof(WorkOrderMf).GetDeclaredProperty("Employee");
            Assert.True(navProp.Getter().IsAssembly);
            Assert.True(navProp.Getter().IsVirtual);
            Assert.True(navProp.Setter().IsAssembly);
            Assert.True(navProp.Setter().IsVirtual);
        }

        [Fact]
        public void CSharp_DbContext_template_can_create_nav_prop_with_specific_getter_access()
        {
            var navProp = typeof(OfficeMf).GetDeclaredProperty("WhiteBoards");
            Assert.True(navProp.Getter().IsAssembly);
            Assert.True(navProp.Getter().IsVirtual);
            Assert.True(navProp.Setter().IsPublic);
            Assert.True(navProp.Setter().IsVirtual);
        }

        [Fact]
        public void CSharp_DbContext_template_can_create_nav_prop_with_specific_setter_access()
        {
            var navProp = typeof(BuildingMf).GetDeclaredProperty("MailRooms");
            Assert.True(navProp.Getter().IsAssembly);
            Assert.True(navProp.Getter().IsVirtual);
            Assert.True(navProp.Setter().IsPrivate);
            Assert.False(navProp.Setter().IsVirtual);
        }

        [Fact]
        public void CSharp_DbContext_template_can_create_fully_internal_primitive_prop()
        {
            var prop = typeof(BuildingMf).GetDeclaredProperty("Value");
            Assert.True(prop.Getter().IsAssembly);
            Assert.True(prop.Setter().IsAssembly);
        }

        [Fact]
        public void CSharp_DbContext_template_can_create_primitive_prop_with_specific_getter_access()
        {
            var prop = typeof(EmployeeMf).GetDeclaredProperty("LastName");
            Assert.True(prop.Getter().IsPrivate);
            Assert.True(prop.Setter().IsAssembly);
        }

        [Fact]
        public void CSharp_DbContext_template_can_create_primitive_prop_with_specific_setter_access()
        {
            var prop = typeof(EmployeeMf).GetDeclaredProperty("FirstName");
            Assert.True(prop.Getter().IsPublic);
            Assert.True(prop.Setter().IsPrivate);
        }

        [Fact]
        public void CSharp_DbContext_template_can_create_fully_internal_primitive_prop_on_a_complex_type()
        {
            var prop = typeof(AddressMf).GetDeclaredProperty("State");
            Assert.True(prop.Getter().IsAssembly);
            Assert.True(prop.Setter().IsAssembly);
        }

        [Fact]
        public void CSharp_DbContext_template_can_create_primitive_prop_on_a_complex_type_with_specific_getter_access()
        {
            var prop = typeof(AddressMf).GetDeclaredProperty("City");
            Assert.True(prop.Getter().IsPrivate);
            Assert.True(prop.Setter().IsPublic);
        }

        [Fact]
        public void CSharp_DbContext_template_can_create_primitive_prop_on_a_complex_type_with_specific_setter_access()
        {
            var prop = typeof(AddressMf).GetDeclaredProperty("ZipCode");
            Assert.True(prop.Getter().IsAssembly);
            Assert.True(prop.Setter().IsPrivate);
        }

        [Fact]
        public void CSharp_DbContext_template_can_create_complex_prop_with_different_getter_and_setter_access()
        {
            var prop = typeof(BuildingMf).GetDeclaredProperty("Address");
            Assert.True(prop.Getter().IsAssembly);
            Assert.True(prop.Setter().IsPrivate);
        }

        [Fact]
        public void
            CSharp_DbContext_template_can_create_complex_prop_on_a_complex_type_with_different_getter_and_setter_access()
        {
            var prop = typeof(AddressMf).GetDeclaredProperty("SiteInfo");
            Assert.True(prop.Getter().IsAssembly);
            Assert.True(prop.Setter().IsPrivate);
        }

        [Fact]
        public void CSharp_DbContext_template_can_create_non_public_DbSet_property()
        {
            var prop = typeof(AdvancedPatternsModelFirstContext).GetDeclaredProperty("MailRooms");
            Assert.True(prop.Getter().IsAssembly);
            Assert.True(prop.Setter().IsAssembly);
        }

        [Fact]
        public void CSharp_DbContext_template_can_create_non_public_complex_type()
        {
            Assert.True(typeof(SiteInfoMf).IsNotPublic);
        }

        [Fact]
        public void CSharp_DbContext_template_can_create_non_public_entity_type()
        {
            Assert.True(typeof(MailRoomMf).IsNotPublic);
        }

        [Fact]
        public void CSharp_DbContext_template_can_create_non_public_context_type()
        {
            Assert.True(typeof(AdvancedPatternsModelFirstContext).IsNotPublic);
        }

        [Fact]
        public void CSharp_DbContext_template_has_lazy_loading_enabled_by_default()
        {
            using (var context = new MonsterModel())
            {
                Assert.True(context.Configuration.LazyLoadingEnabled);
            }
        }

        [Fact]
        public void CSharp_DbContext_template_allows_lazy_loading_to_be_turned_off()
        {
            using (var context = new AdvancedPatternsModelFirstContext())
            {
                Assert.False(context.Configuration.LazyLoadingEnabled);
            }
        }

        [Fact]
        public void CSharp_DbContext_template_creates_sets_for_all_base_types_but_not_derived_types()
        {
            var expectedMonsterSets = new[]
                                          {
                                              "Customer", "Barcode", "IncorrectScan", "BarcodeDetail", "Complaint",
                                              "Resolution",
                                              "Login", "SuspiciousActivity", "SmartCard", "RSAToken", "PasswordReset",
                                              "PageView",
                                              "LastLogin", "Message", "Order", "OrderNote", "OrderQualityCheck", "OrderLine"
                                              ,
                                              "Product", "ProductDetail", "ProductReview", "ProductPhoto",
                                              "ProductWebFeature", "Supplier",
                                              "SupplierLogo", "SupplierInfo", "CustomerInfo", "Computer", "ComputerDetail",
                                              "Driver",
                                              "License",
                                          };

            var notExpectedMonsterSets = new[]
                                             {
                                                 "ProductPageView", "BackOrderLine", "DiscontinuedProduct",
                                             };

            var properties = new HashSet<string>(typeof(MonsterModel).GetRuntimeProperties().Where(p => p.IsPublic()).Select(p => p.Name));

            foreach (var expected in expectedMonsterSets)
            {
                Assert.True(properties.Contains(expected), String.Format("No DbSet property found for {0}", expected));
            }

            foreach (var notExpected in notExpectedMonsterSets)
            {
                Assert.False(
                    properties.Contains(notExpected),
                    String.Format("Found unexpected DbSet property for {0}", notExpected));
            }
        }

        [Fact]
        public void CSharp_DbContext_template_creates_a_derived_class_for_derived_types_in_the_model()
        {
            Assert.True(typeof(CurrentEmployeeMf).BaseType == typeof(EmployeeMf));
        }

        [Fact]
        public void CSharp_DbContext_template_can_create_private_non_virtual_function_import()
        {
            Assert.True(typeof(MonsterModel).GetDeclaredMethod("FunctionImport1").IsPrivate);
            Assert.False(typeof(MonsterModel).GetDeclaredMethod("FunctionImport1").IsVirtual);
        }

        [Fact]
        public void CSharp_DbContext_template_can_create_function_import_with_specific_method_access()
        {
            Assert.True(typeof(MonsterModel).GetDeclaredMethod("FunctionImport2", new Type[0]).IsAssembly);
            Assert.True(typeof(MonsterModel).GetDeclaredMethod("FunctionImport2", new[] { typeof(MergeOption) }).IsAssembly);
        }

        [Fact]
        public void CSharp_DbContext_template_creates_virtual_function_imports()
        {
            Assert.True(typeof(AdvancedPatternsModelFirstContext).GetDeclaredMethod("AllOfficesStoredProc", new Type[0]).IsVirtual);
            Assert.True(typeof(AdvancedPatternsModelFirstContext).GetDeclaredMethod("AllOfficesStoredProc", new[] { typeof(MergeOption) }).IsVirtual);
        }

        [Fact]
        public void CSharp_DbContext_template_can_create_nullable_prop()
        {
            Assert.True(
                typeof(CurrentEmployeeMf).GetDeclaredProperty("LeaveBalance").PropertyType ==
                typeof(decimal?));
        }

        [Fact]
        public void CSharp_DbContext_template_can_create_nullable_prop_on_a_complex_type()
        {
            Assert.True(typeof(SiteInfoMf).GetDeclaredProperty("Zone").PropertyType == typeof(int?));
        }

        #endregion

        #region Code First mode

        [Fact]
        public void Template_generated_context_throws_when_used_in_Code_First_mode()
        {
            using (
                var context =
                    new AdvancedPatternsModelFirstContext(SimpleConnectionString("AdvancedPatternsModelFirstContext")))
            {
                Assert.Equal(
                    new UnintentionalCodeFirstException().Message,
                    Assert.Throws<UnintentionalCodeFirstException>(
                        () => context.Database.Initialize(force: false)).Message);
            }
        }

        #endregion
    }
}
