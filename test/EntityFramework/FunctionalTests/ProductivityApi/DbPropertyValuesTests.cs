// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using AdvancedPatternsModel;
    using ConcurrencyModel;
    using NamespaceForContext;
    using Xunit;

    /// <summary>
    /// Functional tests for property values.
    /// Unit tests also exist in the unit tests project.
    /// </summary>
    public class DbPropertyValuesTests : FunctionalTestBase
    {
        #region Tests for reading from generic property values

        [Fact]
        public void Scalar_current_values_can_be_accessed_as_a_property_dictionary()
        {
            TestPropertyValuesScalars(e => e.CurrentValues);
        }

        [Fact]
        public void Scalar_original_values_can_be_accessed_as_a_property_dictionary()
        {
            TestPropertyValuesScalars(e => e.OriginalValues);
        }

        [Fact]
        public void Scalar_store_values_can_be_accessed_as_a_property_dictionary()
        {
            TestPropertyValuesScalars(e => e.GetDatabaseValues());
        }

#if !NET40

        [Fact]
        public void Scalar_store_values_can_be_accessed_asynchronously_as_a_property_dictionary()
        {
            TestPropertyValuesScalars(e => e.GetDatabaseValuesAsync().Result);
        }

#endif

        private void TestPropertyValuesScalars(Func<DbEntityEntry<Building>, DbPropertyValues> getPropertyValues)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");

                var values = getPropertyValues(context.Entry(building));

                Assert.Equal("Building One", values["Name"]);
                Assert.Equal(1500000m, values["Value"]);
            }
        }

        [Fact]
        public void Scalar_current_values_of_a_derived_object_can_be_accessed_as_a_property_dictionary()
        {
            TestPropertyValuesDerivedScalars(e => e.CurrentValues);
        }

        [Fact]
        public void Scalar_original_values_of_a_derived_object_can_be_accessed_as_a_property_dictionary()
        {
            TestPropertyValuesDerivedScalars(e => e.OriginalValues);
        }

        [Fact]
        public void Scalar_store_values_of_a_derived_object_can_be_accessed_as_a_property_dictionary()
        {
            TestPropertyValuesDerivedScalars(e => e.GetDatabaseValues());
        }

#if !NET40

        [Fact]
        public void Scalar_store_values_of_a_derived_object_can_be_accessed_asynchronously_as_a_property_dictionary()
        {
            TestPropertyValuesDerivedScalars(e => e.GetDatabaseValuesAsync().Result);
        }

#endif

        private void TestPropertyValuesDerivedScalars(
            Func<DbEntityEntry<CurrentEmployee>, DbPropertyValues> getPropertyValues)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var employee = context.Employees.OfType<CurrentEmployee>().Single(b => b.FirstName == "Rowan");

                var values = getPropertyValues(context.Entry(employee));

                Assert.Equal("Miller", values["LastName"]);
                Assert.Equal(45m, values["LeaveBalance"]);
            }
        }

        [Fact]
        public void Complex_current_values_can_be_accessed_as_a_property_dictionary()
        {
            TestPropertyValuesComplex(e => e.CurrentValues);
        }

        [Fact]
        public void Complex_original_values_can_be_accessed_as_a_property_dictionary()
        {
            TestPropertyValuesComplex(e => e.OriginalValues);
        }

        [Fact]
        public void Complex_store_values_can_be_accessed_as_a_property_dictionary()
        {
            TestPropertyValuesComplex(e => e.GetDatabaseValues());
        }

#if !NET40

        [Fact]
        public void Complex_store_values_can_be_accessed_asynchronously_as_a_property_dictionary()
        {
            TestPropertyValuesComplex(e => e.GetDatabaseValuesAsync().Result);
        }

#endif

        private void TestPropertyValuesComplex(Func<DbEntityEntry<Building>, DbPropertyValues> getPropertyValues)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");

                var values = getPropertyValues(context.Entry(building));
                var addressValues = (DbPropertyValues)values["Address"];

                Assert.Equal("100 Work St", addressValues["Street"]);
                Assert.Equal("Redmond", addressValues["City"]);
                Assert.Equal("WA", addressValues["State"]);
                Assert.Equal("98052", addressValues["ZipCode"]);

                var siteValues = (DbPropertyValues)addressValues["SiteInfo"];
                Assert.Equal("Clean", siteValues["Environment"]);
                Assert.Equal(1, siteValues["Zone"]);
            }
        }

        [Fact]
        public void Complex_current_values_of_a_TPT_derived_object_can_be_accessed_as_a_property_dictionary()
        {
            TestPropertyValuesDerivedComplex(e => e.CurrentValues);
        }

        [Fact]
        public void Complex_original_values_of_a_TPT_derived_object_can_be_accessed_as_a_property_dictionary()
        {
            TestPropertyValuesDerivedComplex(e => e.OriginalValues);
        }

        [Fact]
        public void Complex_store_values_of_a_TPT_derived_object_can_be_accessed_as_a_property_dictionary()
        {
            TestPropertyValuesDerivedComplex(e => e.GetDatabaseValues());
        }

#if !NET40

        [Fact]
        public void Complex_store_values_of_a_TPT_derived_object_can_be_accessed_asynchronously_as_a_property_dictionary()
        {
            TestPropertyValuesDerivedComplex(e => e.GetDatabaseValuesAsync().Result);
        }

#endif

        private void TestPropertyValuesDerivedComplex(
            Func<DbEntityEntry<TitleSponsor>, DbPropertyValues> getPropertyValues)
        {
            using (var context = new F1Context())
            {
                var sponsor = context.Sponsors.OfType<TitleSponsor>().Single(s => s.Name == "Vodafone");

                var values = getPropertyValues(context.Entry(sponsor));
                var complexValues = (DbPropertyValues)values["Details"];

                Assert.Equal(10, complexValues["Days"]);
                Assert.Equal(50m, complexValues["Space"]);
            }
        }

        [Fact]
        public void Null_complex_current_values_result_in_a_null_property_dictionary_when_accessed()
        {
            TestPropertyValuesNullComplex(e => e.CurrentValues);
        }

        [Fact]
        public void Null_complex_original_values_result_in_a_null_property_dictionary_when_accessed()
        {
            TestPropertyValuesNullComplex(e => e.OriginalValues);
        }

        private void TestPropertyValuesNullComplex(Func<DbEntityEntry<Building>, DbPropertyValues> getPropertyValues)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var buildingId =
                    context.Buildings.Where(b => b.Name == "Building One").Select(b => b.BuildingId).Single();

                var building = new Building
                                   {
                                       BuildingId = buildingId,
                                       Name = "New Building"
                                   };
                context.Buildings.Attach(building);

                var values = getPropertyValues(context.Entry(building));
                var addressValues = values["Address"];

                Assert.Null(addressValues);
            }
        }

        [Fact]
        public void Null_complex_original_values_result_in_exception_when_querying_for_store_values()
        {
            Null_complex_original_values_result_in_exception_when_querying_for_store_values_implementation(e => e.GetDatabaseValues());
        }

#if !NET40

        [Fact]
        public void Null_complex_original_values_result_in_exception_when_querying_for_store_values_asynchronously()
        {
            Null_complex_original_values_result_in_exception_when_querying_for_store_values_implementation(
                e =>
                ExceptionHelpers.UnwrapAggregateExceptions(() => e.GetDatabaseValuesAsync().Result));
        }

#endif

        private void Null_complex_original_values_result_in_exception_when_querying_for_store_values_implementation(
            Func<DbEntityEntry<Building>, DbPropertyValues> getPropertyValues)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var buildingId =
                    context.Buildings.Where(b => b.Name == "Building One").Select(b => b.BuildingId).Single();

                var building = new Building
                                   {
                                       BuildingId = buildingId,
                                       Name = "New Building"
                                   };
                context.Buildings.Attach(building);

                Assert.Throws<InvalidOperationException>(() => getPropertyValues(context.Entry(building))).
                    ValidateMessage(
                        "DbPropertyValues_CannotGetStoreValuesWhenComplexPropertyIsNull", "Address",
                        "Building");
            }
        }

        #endregion

        #region Tests for reading from non-generic property values

        [Fact]
        public void Scalar_current_values_can_be_accessed_as_a_non_generic_property_dictionary()
        {
            TestNonGenericPropertyValuesScalars(e => e.CurrentValues);
        }

        [Fact]
        public void Scalar_original_values_can_be_accessed_as_a_non_generic_property_dictionary()
        {
            TestNonGenericPropertyValuesScalars(e => e.OriginalValues);
        }

        [Fact]
        public void Scalar_store_values_can_be_accessed_as_a_non_generic_property_dictionary()
        {
            TestNonGenericPropertyValuesScalars(e => e.GetDatabaseValues());
        }

#if !NET40

        [Fact]
        public void Scalar_store_values_can_be_accessed_asynchronously_as_a_non_generic_property_dictionary()
        {
            TestNonGenericPropertyValuesScalars(e => e.GetDatabaseValuesAsync().Result);
        }

#endif

        private void TestNonGenericPropertyValuesScalars(Func<DbEntityEntry, DbPropertyValues> getPropertyValues)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                object building = context.Buildings.Single(b => b.Name == "Building One");

                var values = getPropertyValues(context.Entry(building));

                Assert.Equal("Building One", values["Name"]);
                Assert.Equal(1500000m, values["Value"]);

                Assert.Equal("Building One", values.GetValue<string>("Name"));
                Assert.Equal(1500000m, values.GetValue<decimal>("Value"));
            }
        }

        [Fact]
        public void Scalar_current_values_of_a_derived_object_can_be_accessed_as_a_non_generic_property_dictionary()
        {
            TestNonGenericPropertyValuesDerivedScalars(e => e.CurrentValues);
        }

        [Fact]
        public void Scalar_original_values_of_a_derived_object_can_be_accessed_as_a_non_generic_property_dictionary()
        {
            TestNonGenericPropertyValuesDerivedScalars(e => e.OriginalValues);
        }

        [Fact]
        public void Scalar_store_values_of_a_derived_object_can_be_accessed_as_a_non_generic_property_dictionary()
        {
            TestNonGenericPropertyValuesDerivedScalars(e => e.GetDatabaseValues());
        }

#if !NET40

        [Fact]
        public void Scalar_store_values_of_a_derived_object_can_be_accessed_asynchronously_as_a_non_generic_property_dictionary()
        {
            TestNonGenericPropertyValuesDerivedScalars(e => e.GetDatabaseValuesAsync().Result);
        }

#endif

        private void TestNonGenericPropertyValuesDerivedScalars(Func<DbEntityEntry, DbPropertyValues> getPropertyValues)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                object employee = context.Employees.OfType<CurrentEmployee>().Single(b => b.FirstName == "Rowan");

                var values = getPropertyValues(context.Entry(employee));

                Assert.Equal("Miller", values["LastName"]);
                Assert.Equal(45m, values["LeaveBalance"]);
            }
        }

        [Fact]
        public void Complex_current_values_can_be_accessed_as_a_non_generic_property_dictionary()
        {
            TestNonGenericPropertyValuesComplex(e => e.CurrentValues);
        }

        [Fact]
        public void Complex_original_values_can_be_accessed_as_a_non_generic_property_dictionary()
        {
            TestNonGenericPropertyValuesComplex(e => e.OriginalValues);
        }

        [Fact]
        public void Complex_store_values_can_be_accessed_as_a_non_generic_property_dictionary()
        {
            TestNonGenericPropertyValuesComplex(e => e.GetDatabaseValues());
        }

#if !NET40

        [Fact]
        public void Complex_store_values_can_be_accessed_asynchronously_as_a_non_generic_property_dictionary()
        {
            TestNonGenericPropertyValuesComplex(e => e.GetDatabaseValuesAsync().Result);
        }

#endif

        private void TestNonGenericPropertyValuesComplex(Func<DbEntityEntry, DbPropertyValues> getPropertyValues)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                object building = context.Buildings.Single(b => b.Name == "Building One");

                var values = getPropertyValues(context.Entry(building));
                var addressValues = (DbPropertyValues)values["Address"];

                Assert.Equal("100 Work St", addressValues["Street"]);
                Assert.Equal("Redmond", addressValues["City"]);
                Assert.Equal("WA", addressValues["State"]);
                Assert.Equal("98052", addressValues["ZipCode"]);

                var siteValues = (DbPropertyValues)addressValues["SiteInfo"];
                Assert.Equal("Clean", siteValues["Environment"]);
                Assert.Equal(1, siteValues["Zone"]);

                addressValues = values.GetValue<DbPropertyValues>("Address");

                Assert.Equal("100 Work St", addressValues["Street"]);
                Assert.Equal("Redmond", addressValues["City"]);
                Assert.Equal("WA", addressValues["State"]);
                Assert.Equal("98052", addressValues["ZipCode"]);

                siteValues = addressValues.GetValue<DbPropertyValues>("SiteInfo");
                Assert.Equal("Clean", siteValues["Environment"]);
                Assert.Equal(1, siteValues["Zone"]);
            }
        }

        [Fact]
        public void Complex_current_values_of_a_TPT_derived_object_can_be_accessed_as_a_non_generic_property_dictionary()
        {
            TestNonGenericPropertyValuesDerivedComplex(e => e.CurrentValues);
        }

        [Fact]
        public void Complex_original_values_of_a_TPT_derived_object_can_be_accessed_as_a_non_generic_property_dictionary()
        {
            TestNonGenericPropertyValuesDerivedComplex(e => e.OriginalValues);
        }

        [Fact]
        public void Complex_store_values_of_a_TPT_derived_object_can_be_accessed_as_a_non_generic_property_dictionary()
        {
            TestNonGenericPropertyValuesDerivedComplex(e => e.GetDatabaseValues());
        }

#if !NET40

        [Fact]
        public void Complex_store_values_of_a_TPT_derived_object_can_be_accessed_asynchronously_as_a_non_generic_property_dictionary()
        {
            TestNonGenericPropertyValuesDerivedComplex(e => e.GetDatabaseValuesAsync().Result);
        }

#endif

        private void TestNonGenericPropertyValuesDerivedComplex(Func<DbEntityEntry, DbPropertyValues> getPropertyValues)
        {
            using (var context = new F1Context())
            {
                object sponsor = context.Sponsors.OfType<TitleSponsor>().Single(s => s.Name == "Vodafone");

                var values = getPropertyValues(context.Entry(sponsor));
                var complexValues = (DbPropertyValues)values["Details"];

                Assert.Equal(10, complexValues["Days"]);
                Assert.Equal(50m, complexValues["Space"]);
            }
        }

        #endregion

        #region Tests for writing to generic property values

        [Fact]
        public void Scalar_current_values_can_be_set_using_a_property_dictionary()
        {
            TestSetPropertyValuesScalars(e => e.CurrentValues, e => e.CurrentValues);
        }

        [Fact]
        public void Scalar_original_values_can_be_set_using_a_property_dictionary()
        {
            TestSetPropertyValuesScalars(e => e.OriginalValues, e => e.GetUpdatableOriginalValues());
        }

        private void TestSetPropertyValuesScalars(
            Func<DbEntityEntry<Building>, DbPropertyValues> getPropertyValues,
            Func<ObjectStateEntry, IDataRecord> getDataRecord)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var values = getPropertyValues(context.Entry(building));

                values["Name"] = "Building 18";
                values["Value"] = -1000m;

                Assert.Equal("Building 18", values["Name"]);
                Assert.Equal(-1000m, values["Value"]);

                var dataRecord =
                    getDataRecord(GetObjectContext(context).ObjectStateManager.GetObjectStateEntry(building));
                Assert.Equal("Building 18", dataRecord["Name"]);
                Assert.Equal(-1000m, dataRecord["Value"]);
            }
        }

        [Fact]
        public void Individual_properties_of_complex_current_values_can_be_set_using_a_property_dictionary()
        {
            TestSetPropertyValuesComplex(e => e.CurrentValues, e => e.CurrentValues);
        }

        [Fact]
        public void Individual_properties_of_complex_original_values_can_be_set_using_a_property_dictionary()
        {
            TestSetPropertyValuesComplex(e => e.OriginalValues, e => e.GetUpdatableOriginalValues());
        }

        private void TestSetPropertyValuesComplex(
            Func<DbEntityEntry<Building>, DbPropertyValues> getPropertyValues,
            Func<ObjectStateEntry, IDataRecord> getDataRecord)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var values = getPropertyValues(context.Entry(building));

                var addressValues = (DbPropertyValues)values["Address"];
                addressValues["Street"] = "1 Microsoft Way";
                addressValues["ZipCode"] = "98027";

                Assert.Equal("1 Microsoft Way", addressValues["Street"]);
                Assert.Equal("Redmond", addressValues["City"]);
                Assert.Equal("WA", addressValues["State"]);
                Assert.Equal("98027", addressValues["ZipCode"]);

                var siteValues = (DbPropertyValues)addressValues["SiteInfo"];
                siteValues["Zone"] = 2;

                Assert.Equal("Clean", siteValues["Environment"]);
                Assert.Equal(2, siteValues["Zone"]);

                var dataRecord =
                    getDataRecord(GetObjectContext(context).ObjectStateManager.GetObjectStateEntry(building));
                var addressDataRecord = (IDataRecord)dataRecord["Address"];

                Assert.Equal("1 Microsoft Way", addressDataRecord["Street"]);
                Assert.Equal("Redmond", addressDataRecord["City"]);
                Assert.Equal("WA", addressDataRecord["State"]);
                Assert.Equal("98027", addressDataRecord["ZipCode"]);

                var siteDataRecord = (IDataRecord)addressDataRecord["SiteInfo"];

                Assert.Equal("Clean", siteDataRecord["Environment"]);
                Assert.Equal(2, siteDataRecord["Zone"]);
            }
        }

        [Fact]
        public void
            Complex_current_values_can_be_set_using_at_the_complex_object_level_using_a_nested_property_dictionary()
        {
            TestSetFullPropertyValuesComplex(e => e.CurrentValues, e => e.CurrentValues);
        }

        [Fact]
        public void
            Complex_original_values_can_be_set_using_at_the_complex_object_level_using_a_nested_property_dictionary()
        {
            TestSetFullPropertyValuesComplex(e => e.OriginalValues, e => e.GetUpdatableOriginalValues());
        }

        private void TestSetFullPropertyValuesComplex(
            Func<DbEntityEntry<Building>, DbPropertyValues> getPropertyValues,
            Func<ObjectStateEntry, IDataRecord> getDataRecord)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building1 = context.Buildings.Single(b => b.Name == "Building One");
                var building2 = context.Buildings.Single(b => b.Name == "Building Two");

                var values1 = getPropertyValues(context.Entry(building1));
                var values2 = getPropertyValues(context.Entry(building2));

                // Set all the properties from building1 Address onto building2 Address
                // This includes the nested SiteInfo properties.
                var addressValues1 = (DbPropertyValues)values1["Address"];
                values2["Address"] = addressValues1;

                var addressValues2 = (DbPropertyValues)values2["Address"];
                Assert.Equal("100 Work St", addressValues2["Street"]);
                Assert.Equal("Redmond", addressValues2["City"]);
                Assert.Equal("WA", addressValues2["State"]);
                Assert.Equal("98052", addressValues2["ZipCode"]);

                var siteValues2 = (DbPropertyValues)addressValues2["SiteInfo"];

                Assert.Equal("Clean", siteValues2["Environment"]);
                Assert.Equal(1, siteValues2["Zone"]);

                // Validate the underlying DbDataRecords.
                var dataRecord =
                    getDataRecord(GetObjectContext(context).ObjectStateManager.GetObjectStateEntry(building2));
                var addressDataRecord = (IDataRecord)dataRecord["Address"];

                Assert.Equal("100 Work St", addressDataRecord["Street"]);
                Assert.Equal("Redmond", addressDataRecord["City"]);
                Assert.Equal("WA", addressDataRecord["State"]);
                Assert.Equal("98052", addressDataRecord["ZipCode"]);

                var siteDataRecord = (IDataRecord)addressDataRecord["SiteInfo"];

                Assert.Equal("Clean", siteDataRecord["Environment"]);
                Assert.Equal(1, siteDataRecord["Zone"]);
            }
        }

        #endregion

        #region Tests for writing to non-generic property values

        [Fact]
        public void Scalar_current_values_can_be_set_using_a_non_generic_property_dictionary()
        {
            TestSetNonGenericPropertyValuesScalars(e => e.CurrentValues, e => e.CurrentValues);
        }

        [Fact]
        public void Scalar_original_values_can_be_set_using_a_non_generic_property_dictionary()
        {
            TestSetNonGenericPropertyValuesScalars(e => e.OriginalValues, e => e.GetUpdatableOriginalValues());
        }

        private void TestSetNonGenericPropertyValuesScalars(
            Func<DbEntityEntry, DbPropertyValues> getPropertyValues,
            Func<ObjectStateEntry, IDataRecord> getDataRecord)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var values = getPropertyValues(context.Entry(building));

                values["Name"] = "Building 18";
                values["Value"] = -1000m;

                Assert.Equal("Building 18", values["Name"]);
                Assert.Equal(-1000m, values["Value"]);

                var dataRecord =
                    getDataRecord(GetObjectContext(context).ObjectStateManager.GetObjectStateEntry(building));
                Assert.Equal("Building 18", dataRecord["Name"]);
                Assert.Equal(-1000m, dataRecord["Value"]);
            }
        }

        [Fact]
        public void Individual_properties_of_complex_current_values_can_be_set_using_a_non_generic_property_dictionary()
        {
            TestSetNonGenericPropertyValuesComplex(e => e.CurrentValues, e => e.CurrentValues);
        }

        [Fact]
        public void Individual_properties_of_complex_original_values_can_be_set_using_a_non_generic_property_dictionary()
        {
            TestSetNonGenericPropertyValuesComplex(e => e.OriginalValues, e => e.GetUpdatableOriginalValues());
        }

        private void TestSetNonGenericPropertyValuesComplex(
            Func<DbEntityEntry, DbPropertyValues> getPropertyValues,
            Func<ObjectStateEntry, IDataRecord> getDataRecord)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var values = getPropertyValues(context.Entry(building));

                var addressValues = (DbPropertyValues)values["Address"];
                addressValues["Street"] = "1 Microsoft Way";
                addressValues["ZipCode"] = "98027";

                Assert.Equal("1 Microsoft Way", addressValues["Street"]);
                Assert.Equal("Redmond", addressValues["City"]);
                Assert.Equal("WA", addressValues["State"]);
                Assert.Equal("98027", addressValues["ZipCode"]);

                var siteValues = (DbPropertyValues)addressValues["SiteInfo"];
                siteValues["Zone"] = 2;

                Assert.Equal("Clean", siteValues["Environment"]);
                Assert.Equal(2, siteValues["Zone"]);

                var dataRecord =
                    getDataRecord(GetObjectContext(context).ObjectStateManager.GetObjectStateEntry(building));
                var addressDataRecord = (IDataRecord)dataRecord["Address"];

                Assert.Equal("1 Microsoft Way", addressDataRecord["Street"]);
                Assert.Equal("Redmond", addressDataRecord["City"]);
                Assert.Equal("WA", addressDataRecord["State"]);
                Assert.Equal("98027", addressDataRecord["ZipCode"]);

                var siteDataRecord = (IDataRecord)addressDataRecord["SiteInfo"];

                Assert.Equal("Clean", siteDataRecord["Environment"]);
                Assert.Equal(2, siteDataRecord["Zone"]);
            }
        }

        [Fact]
        public void Complex_current_values_can_be_set_using_at_the_complex_object_level_using_a_nested_non_generic_property_dictionary()
        {
            TestSetFullNonGenericPropertyValuesComplex(e => e.CurrentValues, e => e.CurrentValues);
        }

        [Fact]
        public void Complex_original_values_can_be_set_using_at_the_complex_object_level_using_a_nested_non_generic_property_dictionary()
        {
            TestSetFullNonGenericPropertyValuesComplex(e => e.OriginalValues, e => e.GetUpdatableOriginalValues());
        }

        private void TestSetFullNonGenericPropertyValuesComplex(
            Func<DbEntityEntry, DbPropertyValues> getPropertyValues,
            Func<ObjectStateEntry, IDataRecord> getDataRecord)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building1 = context.Buildings.Single(b => b.Name == "Building One");
                var building2 = context.Buildings.Single(b => b.Name == "Building Two");

                var values1 = getPropertyValues(context.Entry(building1));
                var values2 = getPropertyValues(context.Entry(building2));

                // Set all the properties from building1 Address onto building2 Address
                // This includes the nested SiteInfo properties.
                var addressValues1 = (DbPropertyValues)values1["Address"];
                values2["Address"] = addressValues1;

                var addressValues2 = (DbPropertyValues)values2["Address"];
                Assert.Equal("100 Work St", addressValues2["Street"]);
                Assert.Equal("Redmond", addressValues2["City"]);
                Assert.Equal("WA", addressValues2["State"]);
                Assert.Equal("98052", addressValues2["ZipCode"]);

                var siteValues2 = (DbPropertyValues)addressValues2["SiteInfo"];

                Assert.Equal("Clean", siteValues2["Environment"]);
                Assert.Equal(1, siteValues2["Zone"]);

                // Validate the underlying DbDataRecords.
                var dataRecord =
                    getDataRecord(GetObjectContext(context).ObjectStateManager.GetObjectStateEntry(building2));
                var addressDataRecord = (IDataRecord)dataRecord["Address"];

                Assert.Equal("100 Work St", addressDataRecord["Street"]);
                Assert.Equal("Redmond", addressDataRecord["City"]);
                Assert.Equal("WA", addressDataRecord["State"]);
                Assert.Equal("98052", addressDataRecord["ZipCode"]);

                var siteDataRecord = (IDataRecord)addressDataRecord["SiteInfo"];

                Assert.Equal("Clean", siteDataRecord["Environment"]);
                Assert.Equal(1, siteDataRecord["Zone"]);
            }
        }

        #endregion

        #region Tests for cloning generic property values to objects

        [Fact]
        public void Current_values_can_be_copied_into_an_object()
        {
            TestPropertyValuesClone(e => e.CurrentValues);
        }

        [Fact]
        public void Original_values_can_be_copied_into_an_object()
        {
            TestPropertyValuesClone(e => e.OriginalValues);
        }

        [Fact]
        public void Store_values_can_be_copied_into_an_object()
        {
            TestPropertyValuesClone(e => e.GetDatabaseValues());
        }

#if !NET40

        [Fact]
        public void Store_values_can_be_copied_into_an_object_asynchronously()
        {
            TestPropertyValuesClone(e => e.GetDatabaseValuesAsync().Result);
        }

#endif

        private void TestPropertyValuesClone(Func<DbEntityEntry<Building>, DbPropertyValues> getPropertyValues)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");

                var buildingClone = (Building)getPropertyValues(context.Entry(building)).ToObject();

                Assert.Equal("Building One", buildingClone.Name);
                Assert.Equal(1500000m, buildingClone.Value);
                Assert.Equal("100 Work St", buildingClone.Address.Street);
                Assert.Equal("Redmond", buildingClone.Address.City);
                Assert.Equal("WA", buildingClone.Address.State);
                Assert.Equal("98052", buildingClone.Address.ZipCode);
                Assert.Equal("Clean", buildingClone.Address.SiteInfo.Environment);
                Assert.Equal(1, buildingClone.Address.SiteInfo.Zone);
            }
        }

        [Fact]
        public void Current_values_for_derived_object_can_be_copied_into_an_object()
        {
            TestPropertyValuesDerivedClone(e => e.CurrentValues);
        }

        [Fact]
        public void Original_values_for_derived_object_can_be_copied_into_an_object()
        {
            TestPropertyValuesDerivedClone(e => e.OriginalValues);
        }

        [Fact]
        public void Store_values_for_derived_object_can_be_copied_into_an_object()
        {
            TestPropertyValuesDerivedClone(e => e.GetDatabaseValues());
        }

#if !NET40

        [Fact]
        public void Store_values_for_derived_object_can_be_copied_into_an_object_asynchronously()
        {
            TestPropertyValuesDerivedClone(e => e.GetDatabaseValuesAsync().Result);
        }

#endif

        private void TestPropertyValuesDerivedClone(
            Func<DbEntityEntry<CurrentEmployee>, DbPropertyValues> getPropertyValues)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var employee = context.Employees.OfType<CurrentEmployee>().Single(b => b.FirstName == "Rowan");

                var clone = (CurrentEmployee)getPropertyValues(context.Entry(employee)).ToObject();

                Assert.Equal("Rowan", clone.FirstName);
                Assert.Equal("Miller", clone.LastName);
                Assert.Equal(45m, clone.LeaveBalance);
            }
        }

        [Fact]
        public void Complex_current_value_can_be_cloned_from_property_Values()
        {
            TestPropertyValuesComplexClone(e => e.CurrentValues);
        }

        [Fact]
        public void Complex_original_value_can_be_cloned_from_property_Values()
        {
            TestPropertyValuesComplexClone(e => e.OriginalValues);
        }

        [Fact]
        public void Complex_store_value_can_be_cloned_from_property_Values()
        {
            TestPropertyValuesComplexClone(e => e.GetDatabaseValues());
        }

#if !NET40

        [Fact]
        public void Complex_store_value_can_be_cloned_from_property_Values_asynchronously()
        {
            TestPropertyValuesComplexClone(e => e.GetDatabaseValuesAsync().Result);
        }

#endif

        private void TestPropertyValuesComplexClone(Func<DbEntityEntry<Building>, DbPropertyValues> getPropertyValues)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");

                var values = getPropertyValues(context.Entry(building));
                var addressValues = (DbPropertyValues)values["Address"];

                var addressClone = (Address)addressValues.ToObject();

                Assert.Equal("100 Work St", addressClone.Street);
                Assert.Equal("Redmond", addressClone.City);
                Assert.Equal("WA", addressClone.State);
                Assert.Equal("98052", addressClone.ZipCode);

                var siteValues = (DbPropertyValues)addressValues["SiteInfo"];
                var siteClone = (SiteInfo)siteValues.ToObject();

                Assert.Equal("Clean", siteClone.Environment);
                Assert.Equal(1, siteClone.Zone);
            }
        }

        [Fact]
        public void Complex_current_values_from_derived_object_can_be_cloned_from_property_Values()
        {
            TestPropertyValuesDerivedComplexClone(e => e.CurrentValues);
        }

        [Fact]
        public void Complex_original_valus_from_derived_object_can_be_cloned_from_property_Values()
        {
            TestPropertyValuesDerivedComplexClone(e => e.OriginalValues);
        }

        [Fact]
        public void Complex_store_valus_from_derived_object_can_be_cloned_from_property_Values()
        {
            TestPropertyValuesDerivedComplexClone(e => e.GetDatabaseValues());
        }

#if !NET40

        [Fact]
        public void Complex_store_valus_from_derived_object_can_be_cloned_from_property_Values_asynchronously()
        {
            TestPropertyValuesDerivedComplexClone(e => e.GetDatabaseValuesAsync().Result);
        }

#endif

        private void TestPropertyValuesDerivedComplexClone(
            Func<DbEntityEntry<TitleSponsor>, DbPropertyValues> getPropertyValues)
        {
            using (var context = new F1Context())
            {
                var sponsor = context.Sponsors.OfType<TitleSponsor>().Single(s => s.Name == "Vodafone");

                var values = getPropertyValues(context.Entry(sponsor));
                var complexValues = (DbPropertyValues)values["Details"];

                var sponsorClone = (TitleSponsor)values.ToObject();
                var detailsClone = (SponsorDetails)complexValues.ToObject();

                Assert.Equal("Vodafone", sponsorClone.Name);
                Assert.Equal(50m, detailsClone.Space);
                Assert.Equal(10, detailsClone.Days);
            }
        }

        #endregion

        #region Tests for cloning non-generic property values to objects

        [Fact]
        public void Current_values_can_be_copied_from_a_non_generic_property_dictionary_into_an_object()
        {
            TestNonGenericPropertyValuesClone(e => e.CurrentValues);
        }

        [Fact]
        public void Original_values_can_be_copied_non_generic_property_dictionary_into_an_object()
        {
            TestNonGenericPropertyValuesClone(e => e.OriginalValues);
        }

        [Fact]
        public void Store_values_can_be_copied_non_generic_property_dictionary_into_an_object()
        {
            TestNonGenericPropertyValuesClone(e => e.GetDatabaseValues());
        }

#if !NET40

        [Fact]
        public void Store_values_can_be_copied_asynchronously_non_generic_property_dictionary_into_an_object()
        {
            TestNonGenericPropertyValuesClone(e => e.GetDatabaseValuesAsync().Result);
        }

#endif

        private void TestNonGenericPropertyValuesClone(Func<DbEntityEntry, DbPropertyValues> getPropertyValues)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                object building = context.Buildings.Single(b => b.Name == "Building One");

                var buildingClone = (Building)getPropertyValues(context.Entry(building)).ToObject();

                Assert.Equal("Building One", buildingClone.Name);
                Assert.Equal(1500000m, buildingClone.Value);
                Assert.Equal("100 Work St", buildingClone.Address.Street);
                Assert.Equal("Redmond", buildingClone.Address.City);
                Assert.Equal("WA", buildingClone.Address.State);
                Assert.Equal("98052", buildingClone.Address.ZipCode);
                Assert.Equal("Clean", buildingClone.Address.SiteInfo.Environment);
                Assert.Equal(1, buildingClone.Address.SiteInfo.Zone);
            }
        }

        [Fact]
        public void Complex_current_value_can_be_cloned_from_a_non_generic_property_dictionary()
        {
            TestNonGenericPropertyValuesComplexClone(e => e.CurrentValues);
        }

        [Fact]
        public void Complex_original_value_can_be_cloned_from_a_non_generic_property_dictionary()
        {
            TestNonGenericPropertyValuesComplexClone(e => e.OriginalValues);
        }

        [Fact]
        public void Complex_store_value_can_be_cloned_from_a_non_generic_property_dictionary()
        {
            TestNonGenericPropertyValuesComplexClone(e => e.GetDatabaseValues());
        }

#if !NET40

        [Fact]
        public void Complex_store_value_can_be_cloned_asynchronously_from_a_non_generic_property_dictionary()
        {
            TestNonGenericPropertyValuesComplexClone(e => e.GetDatabaseValuesAsync().Result);
        }

#endif

        private void TestNonGenericPropertyValuesComplexClone(Func<DbEntityEntry, DbPropertyValues> getPropertyValues)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                object building = context.Buildings.Single(b => b.Name == "Building One");

                var values = getPropertyValues(context.Entry(building));
                var addressValues = (DbPropertyValues)values["Address"];

                var addressClone = (Address)addressValues.ToObject();

                Assert.Equal("100 Work St", addressClone.Street);
                Assert.Equal("Redmond", addressClone.City);
                Assert.Equal("WA", addressClone.State);
                Assert.Equal("98052", addressClone.ZipCode);

                var siteValues = (DbPropertyValues)addressValues["SiteInfo"];
                var siteClone = (SiteInfo)siteValues.ToObject();

                Assert.Equal("Clean", siteClone.Environment);
                Assert.Equal(1, siteClone.Zone);
            }
        }

        #endregion

        #region Tests for cloning generic property values to to other property values

        [Fact]
        public void Current_values_can_be_copied_into_a_cloned_dictionary()
        {
            TestPropertyValuesCloneToValues(e => e.CurrentValues);
        }

        [Fact]
        public void Original_values_can_be_copied_into_a_cloned_dictionary()
        {
            TestPropertyValuesCloneToValues(e => e.OriginalValues);
        }

        [Fact]
        public void Store_values_can_be_copied_into_a_cloned_dictionary()
        {
            TestPropertyValuesCloneToValues(e => e.GetDatabaseValues());
        }

#if !NET40

        [Fact]
        public void Store_values_can_be_copied_into_a_cloned_dictionary_asynchronously()
        {
            TestPropertyValuesCloneToValues(e => e.GetDatabaseValuesAsync().Result);
        }

#endif

        private void TestPropertyValuesCloneToValues(Func<DbEntityEntry<Building>, DbPropertyValues> getPropertyValues)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");

                var buildingValues = getPropertyValues(context.Entry(building));
                var addressValues = (DbPropertyValues)buildingValues["Address"];
                var infoValues = (DbPropertyValues)addressValues["SiteInfo"];

                var clonedBuildingValues = buildingValues.Clone();

                Assert.Equal("Building One", clonedBuildingValues["Name"]);
                Assert.Equal(1500000m, clonedBuildingValues["Value"]);

                var clonedAddressValues = (DbPropertyValues)clonedBuildingValues["Address"];

                Assert.Equal("100 Work St", clonedAddressValues["Street"]);
                Assert.Equal("Redmond", clonedAddressValues["City"]);
                Assert.Equal("WA", clonedAddressValues["State"]);
                Assert.Equal("98052", clonedAddressValues["ZipCode"]);

                var clonedInfoValues = (DbPropertyValues)clonedAddressValues["SiteInfo"];

                Assert.Equal("Clean", clonedInfoValues["Environment"]);
                Assert.Equal(1, clonedInfoValues["Zone"]);

                // Test modification of cloned property values does not impact original property values

                var newKey = new Guid();
                clonedBuildingValues["BuildingId"] = newKey; // Can change primary key on clone
                clonedBuildingValues["Name"] = "Building 18";
                clonedAddressValues["Street"] = "1 Microsoft Way";
                clonedInfoValues["Zone"] = 2;

                Assert.Equal(newKey, clonedBuildingValues["BuildingId"]);
                Assert.Equal("Building 18", clonedBuildingValues["Name"]);
                Assert.Equal("1 Microsoft Way", clonedAddressValues["Street"]);
                Assert.Equal(2, clonedInfoValues["Zone"]);

                Assert.Equal("Building One", buildingValues["Name"]);
                Assert.Equal("100 Work St", addressValues["Street"]);
                Assert.Equal(1, infoValues["Zone"]);
            }
        }

        #endregion

        #region Tests for cloning non-generic property values to to other property values

        [Fact]
        public void Current_values_can_be_copied_into_a_non_generic_cloned_dictionary()
        {
            TestNonGenericPropertyValuesCloneToValues(e => e.CurrentValues);
        }

        [Fact]
        public void Original_values_can_be_copied_into_a_non_generic_cloned_dictionary()
        {
            TestNonGenericPropertyValuesCloneToValues(e => e.OriginalValues);
        }

        [Fact]
        public void Store_values_can_be_copied_into_a_non_generic_cloned_dictionary()
        {
            TestNonGenericPropertyValuesCloneToValues(e => e.GetDatabaseValues());
        }

#if !NET40

        [Fact]
        public void Store_values_can_be_copied_asynchronously_into_a_non_generic_cloned_dictionary()
        {
            TestNonGenericPropertyValuesCloneToValues(e => e.GetDatabaseValuesAsync().Result);
        }

#endif

        private void TestNonGenericPropertyValuesCloneToValues(Func<DbEntityEntry, DbPropertyValues> getPropertyValues)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");

                var buildingValues = getPropertyValues(context.Entry(building));
                var addressValues = (DbPropertyValues)buildingValues["Address"];
                var infoValues = (DbPropertyValues)addressValues["SiteInfo"];

                var clonedBuildingValues = buildingValues.Clone();

                Assert.Equal("Building One", clonedBuildingValues["Name"]);
                Assert.Equal(1500000m, clonedBuildingValues["Value"]);

                var clonedAddressValues = (DbPropertyValues)clonedBuildingValues["Address"];

                Assert.Equal("100 Work St", clonedAddressValues["Street"]);
                Assert.Equal("Redmond", clonedAddressValues["City"]);
                Assert.Equal("WA", clonedAddressValues["State"]);
                Assert.Equal("98052", clonedAddressValues["ZipCode"]);

                var clonedInfoValues = (DbPropertyValues)clonedAddressValues["SiteInfo"];

                Assert.Equal("Clean", clonedInfoValues["Environment"]);
                Assert.Equal(1, clonedInfoValues["Zone"]);

                // Test modification of cloned dictionaries does not impact original property values

                var newKey = new Guid();
                clonedBuildingValues["BuildingId"] = newKey; // Can change primary key on clone
                clonedBuildingValues["Name"] = "Building 18";
                clonedAddressValues["Street"] = "1 Microsoft Way";
                clonedInfoValues["Zone"] = 2;

                Assert.Equal(newKey, clonedBuildingValues["BuildingId"]);
                Assert.Equal("Building 18", clonedBuildingValues["Name"]);
                Assert.Equal("1 Microsoft Way", clonedAddressValues["Street"]);
                Assert.Equal(2, clonedInfoValues["Zone"]);

                Assert.Equal("Building One", buildingValues["Name"]);
                Assert.Equal("100 Work St", addressValues["Street"]);
                Assert.Equal(1, infoValues["Zone"]);
            }
        }

        #endregion

        #region Tests for access to current and original values for entities in different states

        [Fact]
        public void Current_values_cannot_be_read_or_set_for_an_object_in_the_Deleted_state()
        {
            TestPropertyValuesNegativeForState(e => e.CurrentValues, "CurrentValues", EntityState.Deleted);
        }

        [Fact]
        public void Original_values_can_be_read_and_set_for_an_object_in_the_Deleted_state()
        {
            TestPropertyValuesPositiveForState(e => e.OriginalValues, EntityState.Deleted);
        }

        [Fact]
        public void Store_values_can_be_read_and_set_for_an_object_in_the_Deleted_state()
        {
            TestPropertyValuesPositiveForState(e => e.GetDatabaseValues(), EntityState.Deleted);
        }

#if !NET40

        [Fact]
        public void Store_values_can_be_read_and_set_for_an_object_in_the_Deleted_state_asynchronously()
        {
            TestPropertyValuesPositiveForState(e => e.GetDatabaseValuesAsync().Result, EntityState.Deleted);
        }

#endif

        [Fact]
        public void Current_values_can_be_read_and_set_for_an_object_in_the_Unchanged_state()
        {
            TestPropertyValuesPositiveForState(e => e.CurrentValues, EntityState.Unchanged);
        }

        [Fact]
        public void Original_values_can_be_read_and_set_for_an_object_in_the_Unchanged_state()
        {
            TestPropertyValuesPositiveForState(e => e.OriginalValues, EntityState.Unchanged);
        }

        [Fact]
        public void Store_values_can_be_read_and_set_for_an_object_in_the_Unchanged_state()
        {
            TestPropertyValuesPositiveForState(e => e.GetDatabaseValues(), EntityState.Unchanged);
        }

#if !NET40

        [Fact]
        public void Store_values_can_be_read_and_set_for_an_object_in_the_Unchanged_state_asynchronously()
        {
            TestPropertyValuesPositiveForState(e => e.GetDatabaseValuesAsync().Result, EntityState.Unchanged);
        }

#endif

        [Fact]
        public void Current_values_can_be_read_and_set_for_an_object_in_the_Modified_state()
        {
            TestPropertyValuesPositiveForState(e => e.CurrentValues, EntityState.Modified);
        }

        [Fact]
        public void Original_values_can_be_read_and_set_for_an_object_in_the_Modified_state()
        {
            TestPropertyValuesPositiveForState(e => e.OriginalValues, EntityState.Modified);
        }

        [Fact]
        public void Store_values_can_be_read_and_set_for_an_object_in_the_Modified_state()
        {
            TestPropertyValuesPositiveForState(e => e.GetDatabaseValues(), EntityState.Modified);
        }

#if !NET40

        [Fact]
        public void Store_values_can_be_read_and_set_for_an_object_in_the_Modified_state_asynchronously()
        {
            TestPropertyValuesPositiveForState(e => e.GetDatabaseValuesAsync().Result, EntityState.Modified);
        }

#endif

        [Fact]
        public void Current_values_can_be_read_and_set_for_an_object_in_the_Added_state()
        {
            TestPropertyValuesPositiveForState(e => e.CurrentValues, EntityState.Added);
        }

        [Fact]
        public void Original_values_cannot_be_read_or_set_for_an_object_in_the_Added_state()
        {
            TestPropertyValuesNegativeForState(e => e.OriginalValues, "OriginalValues", EntityState.Added);
        }

        [Fact]
        public void Store_values_cannot_be_read_or_set_for_an_object_in_the_Added_state()
        {
            TestPropertyValuesNegativeForDetached(e => e.GetDatabaseValues(), "GetDatabaseValues");
        }

#if !NET40

        [Fact]
        public void Store_values_cannot_be_read_or_set_for_an_object_in_the_Added_state_asynchronously()
        {
            TestPropertyValuesNegativeForDetached(
                e =>
                ExceptionHelpers.UnwrapAggregateExceptions(() => e.GetDatabaseValuesAsync().Result), "GetDatabaseValuesAsync");
        }

#endif

        [Fact]
        public void Current_values_cannot_be_read_or_set_for_a_Detached_object()
        {
            TestPropertyValuesNegativeForDetached(e => e.CurrentValues, "CurrentValues");
        }

        [Fact]
        public void Original_values_cannot_be_read_or_set_for_a_Detached_object()
        {
            TestPropertyValuesNegativeForDetached(e => e.OriginalValues, "OriginalValues");
        }

        [Fact]
        public void Store_values_cannot_be_read_or_set_for_a_Detached_object()
        {
            TestPropertyValuesNegativeForDetached(e => e.GetDatabaseValues(), "GetDatabaseValues");
        }

#if !NET40

        [Fact]
        public void Store_values_cannot_be_read_or_set_for_a_Detached_object_asynchronously()
        {
            TestPropertyValuesNegativeForDetached(
                e =>
                ExceptionHelpers.UnwrapAggregateExceptions(() => e.GetDatabaseValuesAsync().Result), "GetDatabaseValuesAsync");
        }

#endif

        private void TestPropertyValuesPositiveForState(
            Func<DbEntityEntry<Building>, DbPropertyValues> getPropertyValues, EntityState state)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var entry = context.Entry(building);
                entry.State = state;

                var values = getPropertyValues(entry);

                Assert.Equal("Building One", values["Name"]);
                values["Name"] = "Building One Prime";
            }
        }

        private void TestPropertyValuesNegativeForState(
            Func<DbEntityEntry<Building>, DbPropertyValues> getPropertyValues, string methodName, EntityState state)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var entry = context.Entry(context.Buildings.Single(b => b.Name == "Building One"));

                entry.State = state;

                Assert.Throws<InvalidOperationException>(() => getPropertyValues(entry)).ValidateMessage(
                    "DbPropertyValues_CannotGetValuesForState", methodName, state.ToString());
            }
        }

        private void TestPropertyValuesNegativeForDetached(
            Func<DbEntityEntry<Building>, DbPropertyValues> getPropertyValues, string methodName)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var entry = context.Entry(context.Buildings.Single(b => b.Name == "Building One"));
                entry.State = EntityState.Detached;

                Assert.Throws<InvalidOperationException>(() => getPropertyValues(entry)).ValidateMessage(
                    "DbEntityEntry_NotSupportedForDetached", methodName, "Building");
            }
        }

        #endregion

        #region Tests for setting values from an object to a generic Values

        [Fact]
        public void Current_values_can_be_set_from_an_object_using_generic_dictionary()
        {
            TestGenericObjectSetValues(e => e.CurrentValues, e => e.CurrentValues);
        }

        [Fact]
        public void Original_values_can_be_set_from_an_object_using_generic_dictionary()
        {
            TestGenericObjectSetValues(e => e.OriginalValues, e => e.GetUpdatableOriginalValues());
        }

        private void TestGenericObjectSetValues(
            Func<DbEntityEntry<Building>, DbPropertyValues> getPropertyValues,
            Func<ObjectStateEntry, IDataRecord> getDataRecord)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var buildingValues = getPropertyValues(context.Entry(building));
                var addressValues = (DbPropertyValues)buildingValues["Address"];
                var infoValues = (DbPropertyValues)addressValues["SiteInfo"];

                var newBuilding = new Building
                                      {
                                          BuildingId = new Guid(building.BuildingId.ToString()),
                                          Name = "Values End",
                                          Value = building.Value,
                                          Address = new Address
                                                        {
                                                            Street = "The Hill",
                                                            City = "Hobbiton",
                                                            State = "WF",
                                                            ZipCode = "00001",
                                                            SiteInfo = new SiteInfo
                                                                           {
                                                                               Zone = 3,
                                                                               Environment = "Comfortable"
                                                                           }
                                                        },
                                      };

                buildingValues.SetValues(newBuilding);

                // Check Values

                Assert.Equal("Values End", buildingValues["Name"]);
                Assert.Equal(1500000m, buildingValues["Value"]);

                Assert.Equal("The Hill", addressValues["Street"]);
                Assert.Equal("Hobbiton", addressValues["City"]);
                Assert.Equal("WF", addressValues["State"]);
                Assert.Equal("00001", addressValues["ZipCode"]);

                Assert.Equal("Comfortable", infoValues["Environment"]);
                Assert.Equal(3, infoValues["Zone"]);

                ValidateBuildingPropereties(context, building, getDataRecord);
            }
        }

        private void ValidateBuildingPropereties(
            AdvancedPatternsMasterContext context, Building building,
            Func<ObjectStateEntry, IDataRecord> getDataRecord)
        {
            // Check underlying data record

            var stateEntry = GetObjectContext(context).ObjectStateManager.GetObjectStateEntry(building);
            var buildingRecord = getDataRecord(stateEntry);
            var addressRecord = (IDataRecord)buildingRecord["Address"];
            var siteRecord = (IDataRecord)addressRecord["SiteInfo"];

            Assert.Equal("Values End", buildingRecord["Name"]);
            Assert.Equal(1500000m, buildingRecord["Value"]);

            Assert.Equal("The Hill", addressRecord["Street"]);
            Assert.Equal("Hobbiton", addressRecord["City"]);
            Assert.Equal("WF", addressRecord["State"]);
            Assert.Equal("00001", addressRecord["ZipCode"]);

            Assert.Equal("Comfortable", siteRecord["Environment"]);
            Assert.Equal(3, siteRecord["Zone"]);

            // Check modified props

            var modifiedProps = new HashSet<string>(stateEntry.GetModifiedProperties());
            Assert.True(modifiedProps.Contains("Name"));
            Assert.True(modifiedProps.Contains("Address"));
            Assert.False(modifiedProps.Contains("BuildingId"));
            Assert.False(modifiedProps.Contains("Value"));
        }

        #endregion

        #region Tests for setting values from an object to a non-generic Values

        [Fact]
        public void Current_values_can_be_set_from_an_object_using_non_generic_dictionary()
        {
            TestNonGenericObjectSetValues(e => e.CurrentValues, e => e.CurrentValues);
        }

        [Fact]
        public void Original_values_can_be_set_from_an_object_using_non_generic_dictionary()
        {
            TestNonGenericObjectSetValues(e => e.OriginalValues, e => e.GetUpdatableOriginalValues());
        }

        private void TestNonGenericObjectSetValues(
            Func<DbEntityEntry, DbPropertyValues> getPropertyValues,
            Func<ObjectStateEntry, IDataRecord> getDataRecord)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var buildingValues = getPropertyValues(context.Entry(building));
                var addressValues = (DbPropertyValues)buildingValues["Address"];
                var infoValues = (DbPropertyValues)addressValues["SiteInfo"];

                var newBuilding = new Building
                                      {
                                          BuildingId = new Guid(building.BuildingId.ToString()),
                                          Name = "Values End",
                                          Value = building.Value,
                                          Address = new Address
                                                        {
                                                            Street = "The Hill",
                                                            City = "Hobbiton",
                                                            State = "WF",
                                                            ZipCode = "00001",
                                                            SiteInfo = new SiteInfo
                                                                           {
                                                                               Zone = 3,
                                                                               Environment = "Comfortable"
                                                                           }
                                                        },
                                      };

                buildingValues.SetValues(newBuilding);

                // Check Values

                Assert.Equal("Values End", buildingValues["Name"]);
                Assert.Equal(1500000m, buildingValues["Value"]);

                Assert.Equal("The Hill", addressValues["Street"]);
                Assert.Equal("Hobbiton", addressValues["City"]);
                Assert.Equal("WF", addressValues["State"]);
                Assert.Equal("00001", addressValues["ZipCode"]);

                Assert.Equal("Comfortable", infoValues["Environment"]);
                Assert.Equal(3, infoValues["Zone"]);

                ValidateBuildingPropereties(context, building, getDataRecord);
            }
        }

        #endregion

        #region Tests for setting values from one generic Values to another generic Values

        [Fact]
        public void Current_values_can_be_set_from_one_generic_dictionary_to_another_generic_dictionary()
        {
            TestGenericValuesSetValues(e => e.CurrentValues, e => e.CurrentValues);
        }

        [Fact]
        public void Original_values_can_be_set_from_one_generic_dictionary_to_another_generic_dictionary()
        {
            TestGenericValuesSetValues(e => e.OriginalValues, e => e.GetUpdatableOriginalValues());
        }

        private void TestGenericValuesSetValues(
            Func<DbEntityEntry<Building>, DbPropertyValues> getPropertyValues,
            Func<ObjectStateEntry, IDataRecord> getDataRecord)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var buildingValues = getPropertyValues(context.Entry(building));
                var addressValues = (DbPropertyValues)buildingValues["Address"];
                var infoValues = (DbPropertyValues)addressValues["SiteInfo"];

                var clonedBuildingValues = buildingValues.Clone();
                var clonedAddressValues = (DbPropertyValues)clonedBuildingValues["Address"];
                var clonedInfoValues = (DbPropertyValues)clonedAddressValues["SiteInfo"];

                clonedBuildingValues["BuildingId"] = new Guid(building.BuildingId.ToString());
                clonedBuildingValues["Name"] = "Values End";
                clonedBuildingValues["Value"] = building.Value;
                clonedAddressValues["Street"] = "The Hill";
                clonedAddressValues["City"] = "Hobbiton";
                clonedAddressValues["State"] = "WF";
                clonedAddressValues["ZipCode"] = "00001";
                clonedInfoValues["Zone"] = 3;
                clonedInfoValues["Environment"] = "Comfortable";

                buildingValues.SetValues(clonedBuildingValues);

                // Check Values

                Assert.Equal("Values End", buildingValues["Name"]);
                Assert.Equal(1500000m, buildingValues["Value"]);

                Assert.Equal("The Hill", addressValues["Street"]);
                Assert.Equal("Hobbiton", addressValues["City"]);
                Assert.Equal("WF", addressValues["State"]);
                Assert.Equal("00001", addressValues["ZipCode"]);

                Assert.Equal("Comfortable", infoValues["Environment"]);
                Assert.Equal(3, infoValues["Zone"]);

                ValidateBuildingPropereties(context, building, getDataRecord);
            }
        }

        #endregion

        #region Tests for setting values from one non-generic Values to another non-generic Values

        [Fact]
        public void Current_values_can_be_set_from_one_non_generic_dictionary_to_another_generic_dictionary()
        {
            TestNonGenericValuesSetValues(e => e.CurrentValues, e => e.CurrentValues);
        }

        [Fact]
        public void Original_values_can_be_set_from_one_non_generic_dictionary_to_another_generic_dictionary()
        {
            TestNonGenericValuesSetValues(e => e.OriginalValues, e => e.GetUpdatableOriginalValues());
        }

        private void TestNonGenericValuesSetValues(
            Func<DbEntityEntry, DbPropertyValues> getPropertyValues,
            Func<ObjectStateEntry, IDataRecord> getDataRecord)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var buildingValues = getPropertyValues(context.Entry(building));
                var addressValues = (DbPropertyValues)buildingValues["Address"];
                var infoValues = (DbPropertyValues)addressValues["SiteInfo"];

                var clonedBuildingValues = buildingValues.Clone();
                var clonedAddressValues = (DbPropertyValues)clonedBuildingValues["Address"];
                var clonedInfoValues = (DbPropertyValues)clonedAddressValues["SiteInfo"];

                clonedBuildingValues["BuildingId"] = new Guid(building.BuildingId.ToString());
                clonedBuildingValues["Name"] = "Values End";
                clonedBuildingValues["Value"] = building.Value;
                clonedAddressValues["Street"] = "The Hill";
                clonedAddressValues["City"] = "Hobbiton";
                clonedAddressValues["State"] = "WF";
                clonedAddressValues["ZipCode"] = "00001";
                clonedInfoValues["Zone"] = 3;
                clonedInfoValues["Environment"] = "Comfortable";

                buildingValues.SetValues(clonedBuildingValues);

                // Check Values

                Assert.Equal("Values End", buildingValues["Name"]);
                Assert.Equal(1500000m, buildingValues["Value"]);

                Assert.Equal("The Hill", addressValues["Street"]);
                Assert.Equal("Hobbiton", addressValues["City"]);
                Assert.Equal("WF", addressValues["State"]);
                Assert.Equal("00001", addressValues["ZipCode"]);

                Assert.Equal("Comfortable", infoValues["Environment"]);
                Assert.Equal(3, infoValues["Zone"]);

                ValidateBuildingPropereties(context, building, getDataRecord);
            }
        }

        #endregion

        #region Tests for attempting to change the properties to invalid values

        [Fact]
        public void Primary_key_in_current_values_cannot_be_changed_in_property_dictionary()
        {
            TestKeyChange(e => e.CurrentValues, "ObjectStateEntry_CannotModifyKeyProperty");
        }

        [Fact]
        public void Primary_key_in_original_values_cannot_be_changed_in_property_dictionary()
        {
            TestKeyChange(e => e.OriginalValues, "ObjectStateEntry_SetOriginalPrimaryKey");
        }

        private void TestKeyChange(
            Func<DbEntityEntry<Building>, DbPropertyValues> getPropertyValues,
            string exceptionStringResource)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var key = building.BuildingId;
                var values = getPropertyValues(context.Entry(building));

                Assert.Throws<InvalidOperationException>(() => values["BuildingId"] = new Guid()).ValidateMessage(
                    exceptionStringResource, "BuildingId");

                Assert.Equal(key, values["BuildingId"]);
                Assert.Equal(key, building.BuildingId);
            }
        }

        [Fact]
        public void Non_nullable_property_in_current_values_cannot_be_set_to_null_in_property_dictionary()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var values = context.Entry(building).CurrentValues;

                Assert.Throws<ConstraintException>(() => values["Value"] = null).ValidateMessage(
                    "Materializer_SetInvalidValue", "Decimal", "Building", "Value", "null");

                Assert.Equal(1500000m, values["Value"]);
                Assert.Equal(1500000m, building.Value);
            }
        }

        [Fact]
        public void Non_nullable_property_in_original_values_can_be_set_to_null_in_property_dictionary()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var values = context.Entry(building).OriginalValues;

                Assert.Throws<InvalidOperationException>(() => values["Value"] = null).ValidateMessage(
                    "ObjectStateEntry_NullOriginalValueForNonNullableProperty",
                    "Value", "Value", "AdvancedPatternsModel.Building");

                Assert.Equal(1500000m, values["Value"]);
            }
        }

        [Fact]
        public void Non_nullable_property_in_cloned_dictionary_can_be_set_to_null()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var values = context.Entry(building).CurrentValues.Clone();

                values["Value"] = null;

                Assert.Null(values["Value"]);
            }
        }

        [Fact]
        public void Property_in_current_values_cannot_be_set_to_instance_of_wrong_type()
        {
            TestSetWrongType(e => e.CurrentValues);
        }

        [Fact]
        public void Property_in_original_values_cannot_be_set_to_instance_of_wrong_type()
        {
            TestSetWrongType(e => e.OriginalValues);
        }

        [Fact]
        public void Property_in_cloned_dictionary_cannot_be_set_to_instance_of_wrong_type()
        {
            TestSetWrongType(e => e.CurrentValues.Clone());
        }

        private void TestSetWrongType(Func<DbEntityEntry<Building>, DbPropertyValues> getPropertyValues)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var values = getPropertyValues(context.Entry(building));

                Assert.Throws<InvalidOperationException>(() => values["Name"] = 1).ValidateMessage(
                    "DbPropertyValues_WrongTypeForAssignment", "Int32", "Name", "String", "Building");

                Assert.Equal("Building One", values["Name"]);
                Assert.Equal("Building One", building.Name);
            }
        }

        [Fact]
        public void Primary_key_in_current_values_cannot_be_changed_by_setting_values_from_object()
        {
            TestKeyChangeByObject(e => e.CurrentValues, "ObjectStateEntry_CannotModifyKeyProperty");
        }

        [Fact]
        public void Primary_key_in_original_values_cannot_be_changed_by_setting_values_from_object()
        {
            TestKeyChangeByObject(e => e.OriginalValues, "ObjectStateEntry_SetOriginalPrimaryKey");
        }

        private void TestKeyChangeByObject(
            Func<DbEntityEntry<Building>, DbPropertyValues> getPropertyValues,
            string exceptionStringResource)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var key = building.BuildingId;
                var values = getPropertyValues(context.Entry(building));

                var newBuilding = (Building)values.ToObject();
                newBuilding.BuildingId = new Guid();

                Assert.Throws<InvalidOperationException>(() => values.SetValues(newBuilding)).ValidateMessage(
                    exceptionStringResource, "BuildingId");

                Assert.Equal(key, values["BuildingId"]);
                Assert.Equal(key, building.BuildingId);
            }
        }

        [Fact]
        public void Primary_key_in_current_values_cannot_be_changed_by_setting_values_from_another_dictionary()
        {
            TestKeyChangeByValues(e => e.CurrentValues, "ObjectStateEntry_CannotModifyKeyProperty");
        }

        [Fact]
        public void Primary_key_in_original_values_cannot_be_changed_by_setting_values_from_another_dictionary()
        {
            TestKeyChangeByValues(e => e.OriginalValues, "ObjectStateEntry_SetOriginalPrimaryKey");
        }

        private void TestKeyChangeByValues(
            Func<DbEntityEntry<Building>, DbPropertyValues> getPropertyValues,
            string exceptionStringResource)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var key = building.BuildingId;
                var values = getPropertyValues(context.Entry(building));

                var clone = values.Clone();
                clone["BuildingId"] = new Guid();

                Assert.Throws<InvalidOperationException>(() => values.SetValues(clone)).ValidateMessage(
                    exceptionStringResource, "BuildingId");

                Assert.Equal(key, values["BuildingId"]);
                Assert.Equal(key, building.BuildingId);
            }
        }

        [Fact]
        public void Non_nullable_property_in_current_values_cannot_be_set_to_null_from_another_dictionary()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var values = context.Entry(building).CurrentValues;

                var clone = values.Clone();
                clone["Value"] = null;

                Assert.Throws<ConstraintException>(() => values.SetValues(clone)).ValidateMessage(
                    "Materializer_SetInvalidValue", "Decimal",
                    "Building", "Value", "null");
            }
        }

        [Fact]
        public void Non_nullable_property_in_original_values_cannot_be_set_to_null_from_another_dictionary()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var values = context.Entry(building).OriginalValues;

                var clone = values.Clone();
                clone["Value"] = null;

                Assert.Throws<InvalidOperationException>(() => values.SetValues(clone)).ValidateMessage(
                    "ObjectStateEntry_NullOriginalValueForNonNullableProperty",
                    "Value", "Value", "AdvancedPatternsModel.Building");
            }
        }

        #endregion

        #region Tests for PropertyNames

        [Fact]
        public void PropertyNames_for_current_values_returns_readonly_c_space_property_names()
        {
            TestPropertyNames(e => e.CurrentValues);
        }

        [Fact]
        public void PropertyNames_for_original_values_returns_readonly_c_space_property_names()
        {
            TestPropertyNames(e => e.OriginalValues);
        }

        [Fact]
        public void PropertyNames_for_store_values_returns_readonly_c_space_property_names()
        {
            TestPropertyNames(e => e.GetDatabaseValues());
        }

#if !NET40

        [Fact]
        public void PropertyNames_for_store_values_returns_readonly_c_space_property_names_asynchronously()
        {
            TestPropertyNames(e => e.GetDatabaseValuesAsync().Result);
        }

#endif

        [Fact]
        public void PropertyNames_for_cloned_dictionary_returns_readonly_c_space_property_names()
        {
            TestPropertyNames(e => e.CurrentValues.Clone());
        }

        private void TestPropertyNames(Func<DbEntityEntry<Building>, DbPropertyValues> getPropertyValues)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var buildingValues = getPropertyValues(context.Entry(building));
                var addressValues = (DbPropertyValues)buildingValues["Address"];
                var infoValues = (DbPropertyValues)addressValues["SiteInfo"];

                var buildingNames = buildingValues.PropertyNames;
                Assert.True(((ICollection<string>)buildingNames).IsReadOnly);

                Assert.Equal(5, buildingNames.Count());
                Assert.True(buildingNames.Contains("BuildingId"));
                Assert.True(buildingNames.Contains("Name"));
                Assert.True(buildingNames.Contains("Value"));
                Assert.True(buildingNames.Contains("Address"));
                Assert.True(buildingNames.Contains("PrincipalMailRoomId"));

                var addressNames = addressValues.PropertyNames;
                Assert.True(((ICollection<string>)addressNames).IsReadOnly);

                Assert.Equal(5, addressNames.Count());
                Assert.True(addressNames.Contains("Street"));
                Assert.True(addressNames.Contains("City"));
                Assert.True(addressNames.Contains("State"));
                Assert.True(addressNames.Contains("ZipCode"));
                Assert.True(addressNames.Contains("SiteInfo"));

                var infoNames = infoValues.PropertyNames;
                Assert.True(((ICollection<string>)infoNames).IsReadOnly);

                Assert.Equal(2, infoNames.Count());
                Assert.True(infoNames.Contains("Environment"));
                Assert.True(infoNames.Contains("Zone"));
            }
        }

        #endregion

        #region Tests for creating proxy instance from generic dictionary

        [Fact]
        public void ToObject_for_current_values_creates_proxy()
        {
            TestCreateProxy(e => e.CurrentValues);
        }

        [Fact]
        public void ToObject_for_original_values_creates_proxy()
        {
            TestCreateProxy(e => e.OriginalValues);
        }

        [Fact]
        public void ToObject_for_store_values_creates_proxy()
        {
            TestCreateProxy(e => e.GetDatabaseValues());
        }

#if !NET40

        [Fact]
        public void ToObject_for_store_values_creates_proxy_asynchronously()
        {
            TestCreateProxy(e => e.GetDatabaseValuesAsync().Result);
        }

#endif

        [Fact]
        public void ToObject_for_cloned_dictionary_creates_proxy()
        {
            TestCreateProxy(e => e.CurrentValues.Clone());
        }

        private void TestCreateProxy(Func<DbEntityEntry<Building>, DbPropertyValues> getPropertyValues)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var buildingValues = getPropertyValues(context.Entry(building));

                var clone = buildingValues.ToObject();

                Assert.IsAssignableFrom<Building>(clone);
                Assert.NotSame(typeof(Building), clone.GetType());
            }
        }

        #endregion

        #region Tests for creating proxy instance from non-generic dictionary

        [Fact]
        public void ToObject_for_non_generic_current_values_creates_proxy()
        {
            TestNonGenericCreateProxy(e => e.CurrentValues);
        }

        [Fact]
        public void ToObject_for_non_generic_original_values_creates_proxy()
        {
            TestNonGenericCreateProxy(e => e.OriginalValues);
        }

        [Fact]
        public void ToObject_for_non_generic_store_values_creates_proxy()
        {
            TestNonGenericCreateProxy(e => e.GetDatabaseValues());
        }

#if !NET40

        [Fact]
        public void ToObject_for_non_generic_store_values_creates_proxy_asynchronously()
        {
            TestNonGenericCreateProxy(e => e.GetDatabaseValuesAsync().Result);
        }

#endif

        [Fact]
        public void ToObject_for_non_generic_cloned_dictionary_creates_proxy()
        {
            TestNonGenericCreateProxy(e => e.CurrentValues.Clone());
        }

        private void TestNonGenericCreateProxy(Func<DbEntityEntry, DbPropertyValues> getPropertyValues)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var buildingValues = getPropertyValues(context.Entry(building));

                var clone = buildingValues.ToObject();

                Assert.IsAssignableFrom<Building>(clone);
                Assert.NotSame(typeof(Building), clone.GetType());
            }
        }

        #endregion

        #region Tests specific to queries for store values

        [Fact]
        public void GetDatabaseValues_for_entity_not_in_the_store_returns_null()
        {
            GetDatabaseValues_for_entity_not_in_the_store_returns_null_implementation(e => e.GetDatabaseValues());
        }

#if !NET40

        [Fact]
        public void GetDatabaseValuesAsync_for_entity_not_in_the_store_returns_null()
        {
            GetDatabaseValues_for_entity_not_in_the_store_returns_null_implementation(e => e.GetDatabaseValuesAsync().Result);
        }

#endif

        private void GetDatabaseValues_for_entity_not_in_the_store_returns_null_implementation(
            Func<DbEntityEntry, DbPropertyValues> getPropertyValues)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building =
                    (Building)
                    context.Entry(context.Buildings.Single(b => b.Name == "Building One")).CurrentValues.ToObject();
                building.BuildingId = new Guid();

                context.Buildings.Attach(building);

                Assert.Null(getPropertyValues(context.Entry(building)));
            }
        }

        [Fact]
        public void NonGeneric_GetDatabaseValues_for_entity_not_in_the_store_returns_null()
        {
            NonGeneric_GetDatabaseValues_for_entity_not_in_the_store_returns_null_implementation(e => e.GetDatabaseValues());
        }

#if !NET40

        [Fact]
        public void NonGeneric_GetDatabaseValuesAsync_for_entity_not_in_the_store_returns_null()
        {
            NonGeneric_GetDatabaseValues_for_entity_not_in_the_store_returns_null_implementation(e => e.GetDatabaseValuesAsync().Result);
        }

#endif

        private void NonGeneric_GetDatabaseValues_for_entity_not_in_the_store_returns_null_implementation(
            Func<DbEntityEntry, DbPropertyValues> getPropertyValues)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building =
                    (Building)
                    context.Entry(context.Buildings.Single(b => b.Name == "Building One")).CurrentValues.ToObject();
                building.BuildingId = new Guid();

                context.Buildings.Attach(building);

                Assert.Null(getPropertyValues(context.Entry((object)building)));
            }
        }

        [Fact]
        public void GetDatabaseValues_for_derived_entity_not_in_the_store_returns_null()
        {
            GetDatabaseValues_for_derived_entity_not_in_the_store_returns_null_implementation(e => e.GetDatabaseValues());
        }

#if !NET40

        [Fact]
        public void GetDatabaseValuesAsync_for_derived_entity_not_in_the_store_returns_null()
        {
            GetDatabaseValues_for_derived_entity_not_in_the_store_returns_null_implementation(e => e.GetDatabaseValuesAsync().Result);
        }

#endif

        private void GetDatabaseValues_for_derived_entity_not_in_the_store_returns_null_implementation(
            Func<DbEntityEntry, DbPropertyValues> getPropertyValues)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var employee = (CurrentEmployee)context.Entry(
                    context.Employees
                                                    .OfType<CurrentEmployee>()
                                                    .Single(b => b.FirstName == "Rowan"))
                                                    .CurrentValues
                                                    .ToObject();
                employee.EmployeeId = -77;

                context.Employees.Attach(employee);

                Assert.Null(getPropertyValues(context.Entry(employee)));
            }
        }

        [Fact]
        public void NonGeneric_GetDatabaseValues_for_derived_entity_not_in_the_store_returns_null()
        {
            NonGeneric_GetDatabaseValues_for_derived_entity_not_in_the_store_returns_null_implementation(e => e.GetDatabaseValues());
        }

#if !NET40

        [Fact]
        public void NonGeneric_GetDatabaseValuesAsync_for_derived_entity_not_in_the_store_returns_null()
        {
            NonGeneric_GetDatabaseValues_for_derived_entity_not_in_the_store_returns_null_implementation(
                e => e.GetDatabaseValuesAsync().Result);
        }

#endif

        public void NonGeneric_GetDatabaseValues_for_derived_entity_not_in_the_store_returns_null_implementation(
            Func<DbEntityEntry, DbPropertyValues> getPropertyValues)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var employee = (CurrentEmployee)context.Entry(
                    context.Employees
                                                    .OfType<CurrentEmployee>()
                                                    .Single(b => b.FirstName == "Rowan"))
                                                    .CurrentValues
                                                    .ToObject();
                employee.EmployeeId = -77;

                context.Employees.Attach(employee);

                Assert.Null(context.Entry((object)employee).GetDatabaseValues());
            }
        }

        [Fact]
        public void GetDatabaseValues_for_the_wrong_type_in_the_store_returns_null()
        {
            GetDatabaseValues_for_the_wrong_type_in_the_store_returns_null_implementation(e => e.GetDatabaseValues());
        }

#if !NET40

        [Fact]
        public void GetDatabaseValuesAsync_for_the_wrong_type_in_the_store_returns_null()
        {
            GetDatabaseValues_for_the_wrong_type_in_the_store_returns_null_implementation(e => e.GetDatabaseValuesAsync().Result);
        }

#endif

        public void GetDatabaseValues_for_the_wrong_type_in_the_store_returns_null_implementation(
            Func<DbEntityEntry, DbPropertyValues> getPropertyValues)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var pastEmployeeId = context.Employees
                    .OfType<PastEmployee>()
                    .AsNoTracking()
                    .FirstOrDefault()
                    .EmployeeId;

                var employee = (CurrentEmployee)context.Entry(
                    context.Employees
                                                    .OfType<CurrentEmployee>()
                                                    .Single(b => b.FirstName == "Rowan"))
                                                    .CurrentValues
                                                    .ToObject();
                employee.EmployeeId = pastEmployeeId;

                context.Employees.Attach(employee);

                Assert.Null(getPropertyValues(context.Entry(employee)));
            }
        }

        [Fact]
        public void NonGeneric_GetDatabaseValues_for_the_wrong_type_in_the_store_throws()
        {
            NonGeneric_GetDatabaseValues_for_the_wrong_type_in_the_store_throws_implementation(e => e.GetDatabaseValues());
        }

#if !NET40

        [Fact]
        public void NonGeneric_GetDatabaseValuesAsync_for_the_wrong_type_in_the_store_throws()
        {
            NonGeneric_GetDatabaseValues_for_the_wrong_type_in_the_store_throws_implementation(e => e.GetDatabaseValuesAsync().Result);
        }

#endif

        public void NonGeneric_GetDatabaseValues_for_the_wrong_type_in_the_store_throws_implementation(
            Func<DbEntityEntry, DbPropertyValues> getPropertyValues)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var pastEmployeeId = context.Employees
                    .OfType<PastEmployee>()
                    .AsNoTracking()
                    .FirstOrDefault()
                    .EmployeeId;

                var employee = (CurrentEmployee)context.Entry(
                    context.Employees
                                                    .OfType<CurrentEmployee>()
                                                    .Single(b => b.FirstName == "Rowan"))
                                                    .CurrentValues
                                                    .ToObject();
                employee.EmployeeId = pastEmployeeId;

                context.Employees.Attach(employee);

                Assert.Null(context.Entry((object)employee).GetDatabaseValues());
            }
        }

        [Fact]
        public void Store_values_really_are_store_values_not_current_or_original_values()
        {
            Store_values_really_are_store_values_not_current_or_original_values_implementation(e => e.GetDatabaseValues());
        }

#if !NET40

        [Fact]
        public void Store_values_really_are_store_values_not_current_or_original_values_async()
        {
            Store_values_really_are_store_values_not_current_or_original_values_implementation(e => e.GetDatabaseValuesAsync().Result);
        }

#endif

        public void Store_values_really_are_store_values_not_current_or_original_values_implementation(
            Func<DbEntityEntry, DbPropertyValues> getPropertyValues)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                building.Name = "Values End";
                building.Address.City = "Hobbiton";
                building.Address.SiteInfo.Environment = "Comfortable";

                context.Entry(building).State = EntityState.Unchanged;

                var storeValues = (Building)getPropertyValues(context.Entry(building)).ToObject();

                Assert.Equal("Building One", storeValues.Name);
                Assert.Equal("Redmond", storeValues.Address.City);
                Assert.Equal("Clean", storeValues.Address.SiteInfo.Environment);
            }
        }

        [Fact]
        public void Setting_store_values_does_not_change_current_or_original_values()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");

                var storeValues = context.Entry(building).GetDatabaseValues();
                storeValues["Name"] = "Bag End";
                storeValues.GetValue<DbPropertyValues>("Address")["City"] = "Hobbiton";
                storeValues.GetValue<DbPropertyValues>("Address").GetValue<DbPropertyValues>("SiteInfo")["Environment"]
                    = "Comfortable";

                var currentValues = (Building)context.Entry(building).CurrentValues.ToObject();
                Assert.Equal("Building One", currentValues.Name);
                Assert.Equal("Redmond", currentValues.Address.City);
                Assert.Equal("Clean", currentValues.Address.SiteInfo.Environment);

                var originalValues = (Building)context.Entry(building).OriginalValues.ToObject();
                Assert.Equal("Building One", originalValues.Name);
                Assert.Equal("Redmond", originalValues.Address.City);
                Assert.Equal("Clean", originalValues.Address.SiteInfo.Environment);
            }
        }

        #endregion

        #region GetDatabaseValues with different namespaces (Dev11 293334)

        [Fact]
        public void GetDatabaseValues_uses_the_CLR_namespace_when_context_and_entities_are_in_different_namespaces()
        {
            GetDatabaseValues_uses_the_CLR_namespace_when_context_and_entities_are_in_different_namespaces_implementation(
                e => e.GetDatabaseValues());
        }

#if !NET40

        [Fact]
        public void GetDatabaseValuesAsync_uses_the_CLR_namespace_when_context_and_entities_are_in_different_namespaces()
        {
            GetDatabaseValues_uses_the_CLR_namespace_when_context_and_entities_are_in_different_namespaces_implementation(
                e => e.GetDatabaseValuesAsync().Result);
        }

#endif

        public void GetDatabaseValues_uses_the_CLR_namespace_when_context_and_entities_are_in_different_namespaces_implementation(
            Func<DbEntityEntry, DbPropertyValues> getPropertyValues)
        {
            using (var context = new ContextInANamespace())
            {
                var superFoo = context.SuperFoos.Single();
                var foo = context.Foos.Single(f => f.Id != superFoo.Id);

                var fooValues = getPropertyValues(context.Entry(foo));
                var superFooValues = getPropertyValues(context.Entry(superFoo));

                Assert.Equal(superFoo.Id, superFooValues["Id"]);
                Assert.Equal(superFoo.SomeSuperFoo, superFooValues["SomeSuperFoo"]);
                Assert.Equal(foo.Id, fooValues["Id"]);
            }
        }

        [Fact]
        public void GetDatabaseValues_uses_the_CLR_namespace_when_context_Model_namespace_does_not_match_code_namespaces()
        {
            GetDatabaseValues_uses_the_CLR_namespace_when_context_Model_namespace_does_not_match_code_namespaces_implementation(
                e => e.GetDatabaseValues());
        }

#if !NET40

        [Fact]
        public void GetDatabaseValuesAsync_uses_the_CLR_namespace_when_context_Model_namespace_does_not_match_code_namespaces()
        {
            GetDatabaseValues_uses_the_CLR_namespace_when_context_Model_namespace_does_not_match_code_namespaces_implementation(
                e => e.GetDatabaseValuesAsync().Result);
        }

#endif

        public void GetDatabaseValues_uses_the_CLR_namespace_when_context_Model_namespace_does_not_match_code_namespaces_implementation(
            Func<DbEntityEntry, DbPropertyValues> getPropertyValues)
        {
            using (var context = new ContextWithAModelNamespace())
            {
                var superFoo = context.SuperFoos.Single();
                var foo = context.Foos.Single(f => f.Id != superFoo.Id);

                var fooValues = getPropertyValues(context.Entry(foo));
                var superFooValues = context.Entry(superFoo).GetDatabaseValues();

                Assert.Equal(superFoo.Id, superFooValues["Id"]);
                Assert.Equal(superFoo.SomeSuperFoo, superFooValues["SomeSuperFoo"]);
                Assert.Equal(foo.Id, fooValues["Id"]);
            }
        }

        #endregion
    }
}

#region Model with different namespaces

namespace NamespaceForContext
{
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using NamespaceForEntities1;
    using NamespaceForEntities2;

    public class ContextInANamespace : DbContext
    {
        public ContextInANamespace()
        {
            Database.SetInitializer(new ContextInANamespaceInitializer());
        }

        public DbSet<FooInANamespace> Foos { get; set; }
        public DbSet<SuperFooInANamespace> SuperFoos { get; set; }
    }

    public class ContextWithAModelNamespace : ContextInANamespace
    {
        public ContextWithAModelNamespace()
        {
            Database.SetInitializer<ContextWithAModelNamespace>(new ContextInANamespaceInitializer());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<ModelNamespaceConvention>();
        }
    }

    public class ContextInANamespaceInitializer : DropCreateDatabaseIfModelChanges<ContextInANamespace>
    {
        protected override void Seed(ContextInANamespace context)
        {
            context.Foos.Add(new FooInANamespace());
            context.SuperFoos.Add(
                new SuperFooInANamespace
                    {
                        SomeSuperFoo = "Baa!"
                    });
        }
    }
}

namespace NamespaceForEntities1
{
    public class FooInANamespace
    {
        public int Id { get; set; }
    }
}

namespace NamespaceForEntities2
{
    using NamespaceForEntities1;

    public class SuperFooInANamespace : FooInANamespace
    {
        public string SomeSuperFoo { get; set; }
    }
}

#endregion
