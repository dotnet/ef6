// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.ELinq
{
    using System.Data.Entity.Query;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using Xunit;

    public class ToStringTests : FunctionalTestBase
    {
        [Flags]
        public enum FlagEnum
        {
            Flag1 = 0x01,
            Flag2 = 0x02,
            Flag4 = 0x04,
        }

        public enum SomeEnum
        {
            SomeA,
            SomeB,
            SomeC,
        }

        public class SomeEntity
        {
            public int Id { get; set; }
            public string StringProp { get; set; }
            public Guid GuidProp { get; set; }
            public int IntProp { get; set; }
            public long LongProp { get; set; }
            public byte ByteProp { get; set; }
            public bool BoolProp { get; set; }
            public decimal DecimalProp { get; set; }
            public float FloatProp { get; set; }
            public double DoubleProp { get; set; }
            public SomeEnum EnumProp { get; set; }
            public DateTime DateTimeProp { get; set; }
            public DateTimeOffset DateTimeOffset { get; set; }

            public string NullStringProp { get; set; }
            public Guid? NullableGuidProp { get; set; }
            public int? NullableIntProp { get; set; }
            public long? NullableLongProp { get; set; }
            public byte? NullableByteProp { get; set; }
            public bool? NullableBoolProp { get; set; }
            public decimal? NullableDecimalProp { get; set; }
            public float? NullableFloatProp { get; set; }
            public double? NullableDoubleProp { get; set; }
            public SomeEnum? NullableEnumProp { get; set; }
            public DateTime? NullableDateTimeProp { get; set; }
            public DateTimeOffset? NullableDateTimeOffsetProp { get; set; }
        }

        public class ToStringContext : DbContext
        {
            public DbSet<SomeEntity> Entities { get; set; }
        }

        public class ToStringInitializer : DropCreateDatabaseAlways<ToStringContext>
        {
            protected override void Seed(ToStringContext db)
            {
                base.Seed(db);

                for (var i = -1; i < 20; i++)
                {
                    var blog = new SomeEntity
                    {
                        StringProp = i.ToString(),
                        BoolProp = i % 2 == 0,
                        ByteProp = (byte)(i & 255),
                        DecimalProp = (decimal)(i + 0.5),
                        DoubleProp = (i + 0.5),
                        FloatProp = (float)(i + 0.5),
                        GuidProp = Guid.NewGuid(),
                        IntProp = i,
                        LongProp = i,
                        EnumProp = SomeEnum.SomeA,
                        DateTimeProp = DateTime.Now,
                        DateTimeOffset = DateTimeOffset.Now,
                        NullStringProp = null,
                        NullableBoolProp = null,
                        NullableByteProp = null,
                        NullableDecimalProp = null,
                        NullableDoubleProp = null,
                        NullableFloatProp = null,
                        NullableGuidProp = null,
                        NullableIntProp = null,
                        NullableLongProp = null,
                        NullableEnumProp = null,
                        NullableDateTimeProp = null,
                        NullableDateTimeOffsetProp = null,
                    };
                    db.Entities.Add(blog);

                    blog = new SomeEntity
                    {
                        StringProp = i.ToString(),
                        BoolProp = i % 2 == 0,
                        ByteProp = (byte)(i & 255),
                        DecimalProp = (decimal)(i + 0.5),
                        DoubleProp = (i + 0.5),
                        FloatProp = (float)(i + 0.5),
                        GuidProp = Guid.NewGuid(),
                        IntProp = i,
                        LongProp = i,
                        EnumProp = SomeEnum.SomeA,
                        DateTimeProp = DateTime.Now,
                        DateTimeOffset = DateTimeOffset.Now,
                        NullStringProp = null,
                        NullableBoolProp = i % 2 == 0,
                        NullableByteProp = (byte)(i & 255),
                        NullableDecimalProp = (decimal)(i + 0.5),
                        NullableDoubleProp = (i + 0.5),
                        NullableFloatProp = (float)(i + 0.5),
                        NullableGuidProp = Guid.NewGuid(),
                        NullableIntProp = i,
                        NullableLongProp = i,
                        NullableEnumProp = SomeEnum.SomeB,
                        NullableDateTimeProp = DateTime.Now,
                        NullableDateTimeOffsetProp = DateTimeOffset.Now,
                    };
                    db.Entities.Add(blog);
                }
            }
        }

        static ToStringTests()
        {
            Database.SetInitializer(new ToStringInitializer());
        }

        [Fact]
        public void Issue1904_ToString_works_for_non_flag_enum()
        {
            using (var db = new ToStringContext())
            {
                var projection = db.Entities
                    .Where(e => e.EnumProp == SomeEnum.SomeA && e.NullableEnumProp == null)
                    .Select(
                        b => new
                        {
                            EnumConstant = SomeEnum.SomeA.ToString(),
                            EnumProp = b.EnumProp.ToString(),
                            EnumConstantUnknown = ((SomeEnum)99).ToString(),
                            EnumPropUnknown = ((SomeEnum)((int)b.EnumProp) + 99).ToString(),
                            EnumNullProp = b.NullableEnumProp.ToString()
                        }).First();

                Assert.Equal(SomeEnum.SomeA.ToString(), projection.EnumConstant);
                Assert.Equal(SomeEnum.SomeA.ToString(), projection.EnumProp);
                Assert.Equal(((SomeEnum)99).ToString(), projection.EnumConstantUnknown);
                Assert.Equal(((SomeEnum)99).ToString(), projection.EnumPropUnknown);
                Assert.Equal(((SomeEnum?)null).ToString(), projection.EnumNullProp);
            }
        }

        [Fact]
        public void Issue1904_ToString_works_inside_predicates()
        {
            using (var db = new ToStringContext())
            {
                var projections = db.Entities
                    .Where(e => e.IntProp.ToString() == db.Entities.FirstOrDefault().IntProp.ToString())
                    .Select(
                        e => new
                        {
                            IntProp = e.IntProp.ToString(),
                        })
                    .ToArray();

                var first = db.Entities.FirstOrDefault().IntProp.ToString();
                foreach (var projection in projections)
                {
                    Assert.Equal(first, projection.IntProp);
                }
            }
        }

        [Fact]
        public void Issue1904_ToString_works_for_non_nullable_primitives()
        {
            using (var db = new ToStringContext())
            {
                var guid = Guid.Empty;
                var projection = db.Entities
                    .Where(e => e.EnumProp == SomeEnum.SomeA)
                    .Select(
                        b => new
                        {
                            ByteProp = ((byte)1).ToString(),
                            ShortProp = ((short)2).ToString(),
                            IntProp = 3.ToString(),
                            LongProp = ((long)4).ToString(),
                            FloatProp = ((float)5).ToString(),
                            DoubleProp = ((double)6).ToString(),
                            DecimalProp = ((decimal)7).ToString(),
                            DateTimeProp = DateTime.Now.ToString(),
                            GuidProp = guid.ToString(),
                        }).First();

                Assert.Equal("1", projection.ByteProp);
                Assert.Equal("2", projection.ShortProp);
                Assert.Equal("3", projection.IntProp);
                Assert.Equal("4", projection.LongProp);
                Assert.Equal("5", projection.FloatProp);
                Assert.Equal("6", projection.DoubleProp);
                Assert.Equal("7", projection.DecimalProp);
                //DateTime is DB server localized, expect unknown..
                Assert.Equal(guid.ToString(), projection.GuidProp);
            }
        }

        [Fact]
        public void Issue1904_ToString_throws_NotSupportedException_for_not_supported_types()
        {
            using (var db = new ToStringContext())
            {
                Assert.Throws<NotSupportedException>(
                    () => db.Entities.Select(b => new byte[] { 1, 2, 3 }.ToString()).ToString());
            }
        }

        [Fact]
        public void Issue1904_ToString_throws_NotSupportedException_Flag_enums()
        {
            using (var db = new ToStringContext())
            {
                Assert.Contains(
                    "FlagsAttribute",
                    Assert.Throws<NotSupportedException>(
                        () => db.Entities.Select(b => FlagEnum.Flag1.ToString()).ToString()).Message);
            }
        }

        [Fact]
        public void Issue1904_ToString_works_for_nullable_enum()
        {
            using (var db = new ToStringContext())
            {
                var projections = db.Entities
                    .Where(e => e.NullableEnumProp == null)
                    .Select(
                        e => new
                        {
                            EnumProp = e.NullableEnumProp.ToString(),
                        })
                    .ToArray();

                foreach (var projection in projections)
                {
                    Assert.Equal("", projection.EnumProp);
                }
            }
        }

        [Fact]
        public void Issue1904_ToString_gives_correct_sql_for_nullable_properties()
        {
            using (var db = new ToStringContext())
            {
                var actualSql = db.Entities
                    .Where(e => e.NullableBoolProp == null)
                    .Select(
                        b => new
                        {
                            BoolProp = b.NullableBoolProp.ToString(),
                            ByteProp = b.NullableByteProp.ToString(),
                            IntProp = b.NullableIntProp.ToString(),
                            LongProp = b.NullableLongProp.ToString(),
                            FloatProp = b.NullableFloatProp.ToString(),
                            DoubleProp = b.NullableDoubleProp.ToString(),
                            DecimalProp = b.NullableDecimalProp.ToString(),
                            DateTimeProp = b.NullableDateTimeProp.ToString(),
                            DateTimeOffsetProp = b.NullableDateTimeOffsetProp.ToString(),
                            GuidProp = b.NullableGuidProp.ToString(),
                            EnumProp = b.NullableEnumProp.ToString(),
                            StringProp = b.StringProp.ToString()
                        }).ToString();

                const string expectedSql = @"SELECT 
    1 AS [C1], 
    CASE WHEN ([Extent1].[NullableBoolProp] = 1) THEN N'True' WHEN ([Extent1].[NullableBoolProp] = 0) THEN N'False' ELSE N'' END AS [C2], 
    CASE WHEN ([Extent1].[NullableByteProp] IS NULL) THEN N'' ELSE  CAST( [Extent1].[NullableByteProp] AS nvarchar(max)) END AS [C3], 
    CASE WHEN ([Extent1].[NullableIntProp] IS NULL) THEN N'' ELSE  CAST( [Extent1].[NullableIntProp] AS nvarchar(max)) END AS [C4], 
    CASE WHEN ([Extent1].[NullableLongProp] IS NULL) THEN N'' ELSE  CAST( [Extent1].[NullableLongProp] AS nvarchar(max)) END AS [C5], 
    CASE WHEN ([Extent1].[NullableFloatProp] IS NULL) THEN N'' ELSE  CAST( [Extent1].[NullableFloatProp] AS nvarchar(max)) END AS [C6], 
    CASE WHEN ([Extent1].[NullableDoubleProp] IS NULL) THEN N'' ELSE  CAST( [Extent1].[NullableDoubleProp] AS nvarchar(max)) END AS [C7], 
    CASE WHEN ([Extent1].[NullableDecimalProp] IS NULL) THEN N'' ELSE  CAST( [Extent1].[NullableDecimalProp] AS nvarchar(max)) END AS [C8], 
    CASE WHEN ([Extent1].[NullableDateTimeProp] IS NULL) THEN N'' ELSE  CAST( [Extent1].[NullableDateTimeProp] AS nvarchar(max)) END AS [C9], 
    CASE WHEN ([Extent1].[NullableDateTimeOffsetProp] IS NULL) THEN N'' ELSE  CAST( [Extent1].[NullableDateTimeOffsetProp] AS nvarchar(max)) END AS [C10], 
    CASE WHEN ([Extent1].[NullableGuidProp] IS NULL) THEN N'' ELSE LOWER( CAST( [Extent1].[NullableGuidProp] AS nvarchar(max))) END AS [C11], 
    CASE WHEN ([Extent1].[NullableEnumProp] = 0) THEN N'SomeA' WHEN ([Extent1].[NullableEnumProp] = 1) THEN N'SomeB' WHEN ([Extent1].[NullableEnumProp] = 2) THEN N'SomeC' WHEN ( CAST( [Extent1].[NullableEnumProp] AS int) IS NULL) THEN N'' ELSE  CAST(  CAST( [Extent1].[NullableEnumProp] AS int) AS nvarchar(max)) END AS [C12], 
    CASE WHEN ([Extent1].[StringProp] IS NULL) THEN N'' ELSE [Extent1].[StringProp] END AS [C13]
    FROM [dbo].[SomeEntities] AS [Extent1]
    WHERE [Extent1].[NullableBoolProp] IS NULL";

                Assert.Equal(expectedSql, actualSql);
            }
        }

        [Fact]
        public void Issue1904_ToString_gives_correct_sql_for_nullable_constants()
        {
            using (var db = new ToStringContext())
            {
                var dateTime = new DateTime(2014, 01, 01);
                var dateTimeOffset = new DateTimeOffset(dateTime);
                var actualSql = db.Entities
                    .Where(e => e.NullableBoolProp == null)
                    .Select(
                        b => new
                        {
                            BoolProp = ((bool?)true).ToString(),
                            ByteProp = ((byte?)1).ToString(),
                            IntProp = ((int?)2).ToString(),
                            LongProp = ((long?)3).ToString(),
                            FloatProp = ((float?)4).ToString(),
                            DoubleProp = ((double?)5).ToString(),
                            DecimalProp = ((decimal?)6).ToString(),
                            DateTimeProp = ((DateTime?)dateTime).ToString(),
                            DateTimeOffsetProp = ((DateTimeOffset?)dateTimeOffset).ToString(),
                            GuidProp = ((Guid?)Guid.Empty).ToString(),
                            EnumProp = ((SomeEnum?)SomeEnum.SomeA).ToString(),
                            EnumPropUnnamedValue = ((SomeEnum?)(-42)).ToString(),
                            StringProp = "abc".ToString()
                        });

                const string expectedSql = @"SELECT 
    1 AS [C1], 
    N'True' AS [C2], 
     CAST( cast(1 as tinyint) AS nvarchar(max)) AS [C3], 
     CAST( 2 AS nvarchar(max)) AS [C4], 
     CAST( cast(3 as bigint) AS nvarchar(max)) AS [C5], 
     CAST( cast(4 as real) AS nvarchar(max)) AS [C6], 
     CAST( cast(5 as float(53)) AS nvarchar(max)) AS [C7], 
     CAST( cast(6 as decimal(18)) AS nvarchar(max)) AS [C8], 
     CAST( @p__linq__0 AS nvarchar(max)) AS [C9], 
     CAST( @p__linq__1 AS nvarchar(max)) AS [C10], 
    LOWER( CAST( @p__linq__2 AS nvarchar(max))) AS [C11], 
    N'SomeA' AS [C12], 
    N'-42' AS [C13], 
    N'abc' AS [C14]
    FROM [dbo].[SomeEntities] AS [Extent1]
    WHERE [Extent1].[NullableBoolProp] IS NULL";
                QueryTestHelpers.VerifyDbQuery(actualSql, expectedSql);
            }
        }

        [Fact]
        public void Issue1904_ToString_gives_correct_sql_for_null_constants()
        {
            using (var db = new ToStringContext())
            {
                var actualSql = db.Entities
                    .Where(e => e.NullableBoolProp == null)
                    .Select(
                        b => new
                        {
                            BoolProp = ((bool?)null).ToString(),
                            ByteProp = ((byte?)null).ToString(),
                            IntProp = ((int?)null).ToString(),
                            LongProp = ((long?)null).ToString(),
                            FloatProp = ((float?)null).ToString(),
                            DoubleProp = ((double?)null).ToString(),
                            DecimalProp = ((decimal?)null).ToString(),
                            DateTimeProp = ((DateTime?)null).ToString(),
                            DateTimeOffsetProp = ((DateTimeOffset?)null).ToString(),
                            GuidProp = ((Guid?)null).ToString(),
                            EnumProp = ((SomeEnum?)null).ToString(),
                        });

                const string expectedSql = @"SELECT 
    1 AS [C1], 
    N'' AS [C2], 
    N'' AS [C3], 
    N'' AS [C4], 
    N'' AS [C5], 
    N'' AS [C6], 
    N'' AS [C7], 
    N'' AS [C8], 
    N'' AS [C9], 
    N'' AS [C10], 
    N'' AS [C11], 
    N'' AS [C12]
    FROM [dbo].[SomeEntities] AS [Extent1]
    WHERE [Extent1].[NullableBoolProp] IS NULL";
                QueryTestHelpers.VerifyDbQuery(actualSql, expectedSql);

            }
        }

        [Fact]
        public void ToString_creates_casts_for_constant_values()
        {
            var originalCulture = Thread.CurrentThread.CurrentCulture;
            try
            {
                //pl-PL uses ',' as the decimal separator
                Thread.CurrentThread.CurrentCulture = new CultureInfo("pl-PL");

                using (var db = new ToStringContext())
                {
                    var actualSql = db.Entities.Select(
                        e => new { Double = 1.2f.ToString(), Date = new DateTime(2014, 12, 13).ToString() });

                    // note that the date is converted to string by the provider
                    const string expectedSql = @"SELECT 
    1 AS [C1], 
     CAST( cast(1.2 as real) AS nvarchar(max)) AS [C2], 
     CAST( convert(datetime2, '2014-12-13 00:00:00.0000000', 121) AS nvarchar(max)) AS [C3]
    FROM [dbo].[SomeEntities] AS [Extent1]";

                    QueryTestHelpers.VerifyDbQuery(actualSql, expectedSql);
                }
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = originalCulture;
            }
        }
    }
}