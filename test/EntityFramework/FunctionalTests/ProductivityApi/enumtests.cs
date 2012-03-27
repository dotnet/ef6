namespace ProductivityApiTests
{
    using System.Data;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Xunit;

    public class EnumTests : FunctionalTestBase
    {
        #region Tests for Translate (Dev11 201757)

        [Fact]
        public void Translate_from_int_to_enum_in_the_model_should_not_always_return_zero()
        {
            using (var context = new EnumyCatContext())
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;

                using (var dtr = CreateIntReader())
                {
                    Assert.Equal(BreedType.NorwegianForestCat, objectContext.Translate<BreedType>(dtr).Single());
                }
            }
        }

        [Fact]
        public void Translate_from_int_to_int_should_not_always_return_zero()
        {
            using (var context = new EnumyCatContext())
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;

                using (var dtr = CreateIntReader())
                {
                    Assert.Equal(3, objectContext.Translate<int>(dtr).Single());
                }
            }
        }

        [Fact]
        public void Translate_from_int_to_enum_not_in_the_model_should_not_always_return_zero()
        {
            using (var context = new EnumyCatContext())
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;

                using (var dtr = CreateIntReader())
                {
                    Assert.Equal(EnumNotInModel.NorwegianForestCat,
                                 objectContext.Translate<EnumNotInModel>(dtr).Single());
                }
            }
        }

        [Fact]
        public void Translate_from_long_to_enum_in_the_model_should_not_always_return_zero()
        {
            using (var context = new EnumyCatContext())
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;

                using (var dtr = CreateLongReader())
                {
                    Assert.Equal(LongBreedType.NorwegianForestCat, objectContext.Translate<LongBreedType>(dtr).Single());
                }
            }
        }

        [Fact]
        public void Translate_from_long_to_long_should_not_always_return_zero()
        {
            using (var context = new EnumyCatContext())
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;

                using (var dtr = CreateLongReader())
                {
                    Assert.Equal(3L, objectContext.Translate<long>(dtr).Single());
                }
            }
        }

        [Fact]
        public void Translate_from_long_to_enum_not_in_the_model_should_not_always_return_zero()
        {
            using (var context = new EnumyCatContext())
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;

                using (var dtr = CreateLongReader())
                {
                    Assert.Equal(LongEnumNotInModel.NorwegianForestCat,
                                 objectContext.Translate<LongEnumNotInModel>(dtr).Single());
                }
            }
        }

        [Fact]
        public void Translate_from_unsigned_long_to_unsigned_long_should_always_return_zero()
        {
            using (var context = new EnumyCatContext())
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;

                using (var dtr = CreateULongReader())
                {
                    // Will always return zero because EF does not understand unsigned longs
                    Assert.Equal(0UL, objectContext.Translate<ulong>(dtr).Single());
                }
            }
        }

        [Fact]
        public void Translate_from_unsigned_long_to_enum_not_in_the_model_should_always_return_zero()
        {
            using (var context = new EnumyCatContext())
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;

                using (var dtr = CreateULongReader())
                {
                    // Will always return zero because EF does not know understand unsigned long type 
                    // which is the underlying type of the enum
                    Assert.Equal((ULongEnumNotInModel)0, objectContext.Translate<ULongEnumNotInModel>(dtr).Single());
                }
            }
        }

        [Fact]
        public void Translate_from_int_to_arbitary_class_containing_enum_in_the_model_should_not_always_return_zero()
        {
            using (var context = new EnumyCatContext())
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;

                using (var dtr = CreateIntIntULongReader())
                {
                    var result = objectContext.Translate<ClassNotInModel>(dtr).Single();

                    Assert.Equal(BreedType.NorwegianForestCat, result.Breed);
                }
            }
        }

        [Fact]
        public void
            Translate_from_int_to_arbitary_class_containing_enum_not_in_the_model_should_not_always_populate_with_zero()
        {
            using (var context = new EnumyCatContext())
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;

                using (var dtr = CreateIntIntULongReader())
                {
                    var result = objectContext.Translate<ClassNotInModel>(dtr).Single();

                    Assert.Equal(EnumNotInModel.NorwegianForestCat, result.NotInModel);
                }
            }
        }

        [Fact]
        public void Translate_from_int_to_arbitary_class_containing_ulong_model_always_populates_with_zero()
        {
            using (var context = new EnumyCatContext())
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;

                using (var dtr = CreateIntIntULongReader())
                {
                    var result = objectContext.Translate<ClassNotInModel>(dtr).Single();

                    // Property will always be zero because EF does not know about unsigned longs
                    Assert.Equal(0UL, result.ULong);
                }
            }
        }

        private static DataTableReader CreateIntReader()
        {
            var dt = new DataTable();
            dt.Columns.Add(new DataColumn("AnIntType", typeof(int)));
            dt.LoadDataRow(new object[] { 3 }, true);
            return new DataTableReader(dt);
        }

        private static DataTableReader CreateLongReader()
        {
            var dt = new DataTable();
            dt.Columns.Add(new DataColumn("ALongType", typeof(long)));
            dt.LoadDataRow(new object[] { 3 }, true);
            return new DataTableReader(dt);
        }

        private static DataTableReader CreateULongReader()
        {
            var dt = new DataTable();
            dt.Columns.Add(new DataColumn("AnUnsignedLongType", typeof(ulong)));
            dt.LoadDataRow(new object[] { 3 }, true);
            return new DataTableReader(dt);
        }

        private static DataTableReader CreateIntIntULongReader()
        {
            var dt = new DataTable();
            dt.Columns.Add(new DataColumn("Breed", typeof(int)));
            dt.Columns.Add(new DataColumn("NotInModel", typeof(int)));
            dt.Columns.Add(new DataColumn("ULong", typeof(int)));
            dt.LoadDataRow(new object[] { 3, 3, 3UL }, true);
            return new DataTableReader(dt);
        }

        #endregion
    }

    #region Model with enums and other un-mapped types

    public class EnumyCatContext : DbContext
    {
        public EnumyCatContext()
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<EnumyCatContext>());
        }

        public DbSet<EnumyCat> Cats { get; set; }
    }

    public enum LongBreedType : long
    {
        Burmese = 1L,
        Tonkinese = 2L,
        NorwegianForestCat = 3L,
    }

    public enum BreedType
    {
        Burmese = 1,
        Tonkinese = 2,
        NorwegianForestCat = 3,
    }

    public class EnumyCat
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public BreedType Breed { get; set; }
        public LongBreedType LongBreed { get; set; }
    }

    public enum EnumNotInModel
    {
        Burmese = 1,
        Tonkinese = 2,
        NorwegianForestCat = 3,
    }

    public enum LongEnumNotInModel : long
    {
        Burmese = 1L,
        Tonkinese = 2L,
        NorwegianForestCat = 3L,
    }

    public enum ULongEnumNotInModel : ulong
    {
        Burmese = 1UL,
        Tonkinese = 2UL,
        NorwegianForestCat = 3UL,
    }

    public class ClassNotInModel
    {
        public BreedType Breed { get; set; }
        public EnumNotInModel NotInModel { get; set; }
        public ulong ULong { get; set; }
    }

    #endregion
}