// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.ELinq
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Query;
    using System.Linq;
    using System.Linq.Expressions;
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
            SomeA = 0,
            SomeB = 1,
            SomeC = 2,
        }

        public class SomeEntity
        {
            public int Id { get; set; }
            public string StringProp { get; set; }
            public Guid GuidProp { get; set; }
            public int IntProp { get; set; }
            public long LongProp { get; set; }
       //     public sbyte SByteProp { get; set; }
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
  //          public sbyte? NullableSByteProp { get; set; }
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

                for (int i = -1; i < 20; i++)
                {
                    var blog = new SomeEntity
                    {
                        StringProp = i.ToString(),
                        BoolProp = i % 2 == 0,
                //        SByteProp = sbyte.MinValue,
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
               //         NullableSByteProp = null,
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
                //        SByteProp = sbyte.MinValue,
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
                //        NullableSByteProp = sbyte.MinValue,
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
            Database.SetInitializer<ToStringContext>(new ToStringInitializer());
        }
      
        [Fact]
        public void Issue1904_ToString_works_for_non_flag_enum()
        {
            using (var db = new ToStringContext())
            {
                var projection = db.Entities
                    .Where(e => e.EnumProp == SomeEnum.SomeA && e.NullableEnumProp == null)
                    .Select(b => new
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

            using (var db = new ToStringContext())
            {
                db.Configuration.UseDatabaseNullSemantics = true;
                var projection = db.Entities
                    .Where(e => e.EnumProp == SomeEnum.SomeA && e.NullableEnumProp == null)
                    .Select(b => new
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
                Assert.Equal(null, projection.EnumNullProp); //expect null for dbnull behavior
            }
        }

        [Fact]
        public void Issue1904_ToString_works_inside_predicates()
        {
            using (var db = new ToStringContext())
            {
                var projections = db.Entities
                    .Where(e => e.IntProp.ToString() == db.Entities.FirstOrDefault().IntProp.ToString())
                    .Select(e => new
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
                    .Select(b => new
                    {
            //            SByteProp = ((sbyte)1).ToString(),
                        ByteProp = ((byte)1).ToString(),
                        ShortProp = ((short)1).ToString(),
                        IntProp = 1.ToString(),
                        LongProp = ((long)1).ToString(),
                        FloatProp = ((float)1).ToString(),
                        DoubleProp = ((double)1).ToString(),
                        DecimalProp = ((decimal)1).ToString(),
                        DateTimeProp = DateTime.Now.ToString(),
                        GuidProp = guid.ToString(),
                    }).First();

            //    Assert.Equal("1", projection.SByteProp);
                Assert.Equal("1", projection.ByteProp);
                Assert.Equal("1", projection.ShortProp);
                Assert.Equal("1", projection.IntProp);
                Assert.Equal("1", projection.LongProp);
                Assert.Equal("1", projection.FloatProp);
                Assert.Equal("1", projection.DoubleProp);
                Assert.Equal("1", projection.DecimalProp);
                //DateTime is DB server localized, expect unknown..
                Assert.Equal(guid.ToString(), projection.GuidProp);
            }
        }

        [Fact]
        public void Issue1904_ToString_throws_NotSupportedException_for_non_supported_types()
        {
            using (var db = new ToStringContext())
            {
                //byte array
                Assert.Throws<NotSupportedException>(() =>
                {
                    var projection = db.Entities
                        .Select(b => new
                        {
                            ShouldThrow = (new byte[] { 1, 2, 3 }).ToArray()
                        }).First();
                });

                //char
                Assert.Throws<NotSupportedException>(() =>
                {
                    var projection = db.Entities
                        .Select(b => new
                        {
                            ShouldThrow = 'A'.ToString()
                        }).First();
                });
            }
        }

        [Fact]
        public void Issue1904_ToString_throws_NotSupportedException_Flag_enums()
        {
            using (var db = new ToStringContext())
            {
                Assert.Throws<NotSupportedException>(() =>
                {
                    var projection = db.Entities
                        .Select(b => new
                        {
                            ShouldThrow = FlagEnum.Flag1.ToString(),
                        }).First();
                });
            }
        }

        [Fact]
        public void Issue1904_ToString_works_for_nullable_enum()
        {
            using (var db = new ToStringContext())
            {
                var projections = db.Entities
                    .Where(e => e.NullableEnumProp == null)
                    .Select(e => new
                    {
                        EnumProp = e.NullableEnumProp.ToString(),
                    })
                    .ToArray();

                var first = db.Entities.FirstOrDefault().IntProp.ToString();
                foreach (var projection in projections)
                {
                    Assert.Equal("", projection.EnumProp);
                }
            }

            using (var db = new ToStringContext())
            {
                db.Configuration.UseDatabaseNullSemantics = true;

                var projections = db.Entities
                    .Where(e => e.NullableEnumProp == null)
                    .Select(e => new
                    {
                        EnumProp = e.NullableEnumProp.ToString(),
                    })
                    .ToArray();

                var first = db.Entities.FirstOrDefault().IntProp.ToString();
                foreach (var projection in projections)
                {
                    Assert.Equal(null, projection.EnumProp); //expect null for db null semantics
                }
            }
        }

        [Fact]
        public void Issue1904_ToString_works_for_nullable_primitives()
        {
            using (var db = new ToStringContext())
            {
                var guid = Guid.Empty;
                var projection = db.Entities
                    .Where(e => e.NullableBoolProp == null)
                    .Select(b => new
                    {
                        //    SByteProp = ((sbyte)1).ToString(),
                        BoolProp = b.NullableBoolProp.ToString(),
                        ByteProp = b.NullableByteProp.ToString(),
                        IntProp = b.NullableIntProp.ToString(),
                        LongProp = b.NullableLongProp.ToString(),
                        FloatProp = b.NullableFloatProp.ToString(),
                        DoubleProp = b.NullableDoubleProp.ToString(),
                        DecimalProp = b.NullableDecimalProp.ToString(),
                        DateTimeProp = b.NullableDateTimeProp.ToString(),
                        GuidProp = b.NullableGuidProp.ToString(),
                        StringProp = b.NullStringProp.ToString(),
                        EnumProp = b.NullableEnumProp.ToString(),
                    }).First();

                //Assert.Equal((sbyte)1, projection.SByteProp);
                Assert.Equal("", projection.ByteProp);
                Assert.Equal("", projection.BoolProp);
                Assert.Equal("", projection.IntProp);
                Assert.Equal("", projection.LongProp);
                Assert.Equal("", projection.FloatProp);
                Assert.Equal("", projection.DoubleProp);
                Assert.Equal("", projection.DecimalProp);
                //DateTime is DB server localized, expect unknown..
                Assert.Equal("", projection.GuidProp);

                // null string ToString() does not give "" in .NET, it throws NullReferenceException
                // however, string concat on null strings treats null as ""
                Assert.Equal("", projection.StringProp);
                Assert.Equal("", projection.EnumProp);
            }

            using (var db = new ToStringContext())
            {
                db.Configuration.UseDatabaseNullSemantics = true;
                var guid = Guid.Empty;
                var projection = db.Entities
                    .Where(e => e.NullableBoolProp == null)
                    .Select(b => new
                    {
                        //    SByteProp = ((sbyte)1).ToString(),
                        BoolProp = b.NullableBoolProp.ToString(),
                        ByteProp = b.NullableByteProp.ToString(),
                        IntProp = b.NullableIntProp.ToString(),
                        LongProp = b.NullableLongProp.ToString(),
                        FloatProp = b.NullableFloatProp.ToString(),
                        DoubleProp = b.NullableDoubleProp.ToString(),
                        DecimalProp = b.NullableDecimalProp.ToString(),
                        DateTimeProp = b.NullableDateTimeProp.ToString(),
                        GuidProp = b.NullableGuidProp.ToString(),
                        StringProp = b.NullStringProp.ToString(),
                        EnumProp = b.NullableEnumProp.ToString(),
                    }).First();

                //Assert.Equal((sbyte)1, projection.SByteProp);
                Assert.Equal(null, projection.ByteProp);
                Assert.Equal(null, projection.BoolProp);
                Assert.Equal(null, projection.IntProp);
                Assert.Equal(null, projection.LongProp);
                Assert.Equal(null, projection.FloatProp);
                Assert.Equal(null, projection.DoubleProp);
                Assert.Equal(null, projection.DecimalProp);
                //DateTime is DB server localized, expect unknown..
                Assert.Equal(null, projection.GuidProp);
                Assert.Equal(null, projection.StringProp);
                Assert.Equal(null, projection.EnumProp);
            }
        }

        [Fact]
        public void Issue1904_ToString_gives_correct_sql_for_nullable_properties()
        {
            using (var db = new ToStringContext())
            {
                var actualSql = db.Entities
                    .Where(e => e.NullableBoolProp == null)
                    .Select(b => new
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
                    }).ToString();

                string expectedSql = @"SELECT 
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
    CASE WHEN ( CAST( [Extent1].[NullableEnumProp] AS int) = 0) THEN N'SomeA' WHEN ( CAST( [Extent1].[NullableEnumProp] AS int) = 1) THEN N'SomeB' WHEN ( CAST( [Extent1].[NullableEnumProp] AS int) = 2) THEN N'SomeC' WHEN ( CAST( [Extent1].[NullableEnumProp] AS int) IS NULL) THEN N'' ELSE  CAST(  CAST( [Extent1].[NullableEnumProp] AS int) AS nvarchar(max)) END AS [C12]
    FROM [dbo].[SomeEntities] AS [Extent1]
    WHERE [Extent1].[NullableBoolProp] IS NULL";
                Assert.Equal(actualSql, expectedSql);

            }

            using (var db = new ToStringContext())
            {
                db.Configuration.UseDatabaseNullSemantics = true;
                var actualSql = db.Entities
                    .Where(e => e.NullableBoolProp == null)
                    .Select(b => new
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
                    }).ToString();

                string expectedSql = @"SELECT 
    1 AS [C1], 
    CASE WHEN ([Extent1].[NullableBoolProp] = 1) THEN N'True' WHEN ([Extent1].[NullableBoolProp] = 0) THEN N'False' END AS [C2], 
     CAST( [Extent1].[NullableByteProp] AS nvarchar(max)) AS [C3], 
     CAST( [Extent1].[NullableIntProp] AS nvarchar(max)) AS [C4], 
     CAST( [Extent1].[NullableLongProp] AS nvarchar(max)) AS [C5], 
     CAST( [Extent1].[NullableFloatProp] AS nvarchar(max)) AS [C6], 
     CAST( [Extent1].[NullableDoubleProp] AS nvarchar(max)) AS [C7], 
     CAST( [Extent1].[NullableDecimalProp] AS nvarchar(max)) AS [C8], 
     CAST( [Extent1].[NullableDateTimeProp] AS nvarchar(max)) AS [C9], 
     CAST( [Extent1].[NullableDateTimeOffsetProp] AS nvarchar(max)) AS [C10], 
    LOWER( CAST( [Extent1].[NullableGuidProp] AS nvarchar(max))) AS [C11], 
    CASE WHEN ( CAST( [Extent1].[NullableEnumProp] AS int) = 0) THEN N'SomeA' WHEN ( CAST( [Extent1].[NullableEnumProp] AS int) = 1) THEN N'SomeB' WHEN ( CAST( [Extent1].[NullableEnumProp] AS int) = 2) THEN N'SomeC' WHEN ( CAST( [Extent1].[NullableEnumProp] AS int) IS NULL) THEN CAST(NULL AS varchar(1)) ELSE  CAST(  CAST( [Extent1].[NullableEnumProp] AS int) AS nvarchar(max)) END AS [C12]
    FROM [dbo].[SomeEntities] AS [Extent1]
    WHERE [Extent1].[NullableBoolProp] IS NULL";

                Assert.Equal(actualSql, expectedSql);
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
                    .Select(b => new
                    {
                        BoolProp = ((bool?)true).ToString(),
                        ByteProp = ((byte?)1).ToString(),
                        IntProp = ((int?)1).ToString(),
                        LongProp = ((long?)1).ToString(),
                        FloatProp = ((float?)1).ToString(),
                        DoubleProp = ((double?)1).ToString(),
                        DecimalProp = ((decimal?)1).ToString(),
                        //TODO: are params considered to be constants or does sql generation need to apply full generation?
                        DateTimeProp = ((DateTime?)dateTime).ToString(),
                        DateTimeOffsetProp = ((DateTimeOffset?)dateTimeOffset).ToString(),
                        GuidProp = ((Guid?)Guid.Empty).ToString(),
                        EnumProp = ((SomeEnum?)SomeEnum.SomeA).ToString(),
                    });

                string expectedSql = @"SELECT 
    1 AS [C1], 
    N'True' AS [C2], 
     CAST( cast(1 as tinyint) AS nvarchar(max)) AS [C3], 
     CAST( 1 AS nvarchar(max)) AS [C4], 
     CAST( cast(1 as bigint) AS nvarchar(max)) AS [C5], 
     CAST( cast(1 as real) AS nvarchar(max)) AS [C6], 
     CAST( cast(1 as float(53)) AS nvarchar(max)) AS [C7], 
     CAST( cast(1 as decimal(18)) AS nvarchar(max)) AS [C8], 
    CASE WHEN (@p__linq__0 IS NULL) THEN N'' ELSE  CAST( @p__linq__0 AS nvarchar(max)) END AS [C9], 
    CASE WHEN (@p__linq__1 IS NULL) THEN N'' ELSE  CAST( @p__linq__1 AS nvarchar(max)) END AS [C10], 
    CASE WHEN (@p__linq__2 IS NULL) THEN N'' ELSE LOWER( CAST( @p__linq__2 AS nvarchar(max))) END AS [C11], 
    N'SomeA' AS [C12]
    FROM [dbo].[SomeEntities] AS [Extent1]
    WHERE [Extent1].[NullableBoolProp] IS NULL";
                QueryTestHelpers.VerifyDbQuery(actualSql, expectedSql);

            }

            using (var db = new ToStringContext())
            {
                db.Configuration.UseDatabaseNullSemantics = true;
                var dateTime = new DateTime(2014, 01, 01);
                var dateTimeOffset = new DateTimeOffset(dateTime);
                var actualSql = db.Entities
                    .Where(e => e.NullableBoolProp == null)
                    .Select(b => new
                    {
                        BoolProp = ((bool?)true).ToString(),
                        ByteProp = ((byte?)1).ToString(),
                        IntProp = ((int?)1).ToString(),
                        LongProp = ((long?)1).ToString(),
                        FloatProp = ((float?)1).ToString(),
                        DoubleProp = ((double?)1).ToString(),
                        DecimalProp = ((decimal?)1).ToString(),
                        DateTimeProp = ((DateTime?)dateTime).ToString(),
                        DateTimeOffsetProp = ((DateTimeOffset?)dateTimeOffset).ToString(),
                        GuidProp = ((Guid?)Guid.Empty).ToString(),
                        EnumProp = ((SomeEnum?)SomeEnum.SomeA).ToString(),
                    });

                string expectedSql = @"SELECT 
    1 AS [C1], 
    N'True' AS [C2], 
     CAST( cast(1 as tinyint) AS nvarchar(max)) AS [C3], 
     CAST( 1 AS nvarchar(max)) AS [C4], 
     CAST( cast(1 as bigint) AS nvarchar(max)) AS [C5], 
     CAST( cast(1 as real) AS nvarchar(max)) AS [C6], 
     CAST( cast(1 as float(53)) AS nvarchar(max)) AS [C7], 
     CAST( cast(1 as decimal(18)) AS nvarchar(max)) AS [C8], 
     CAST( @p__linq__0 AS nvarchar(max)) AS [C9], 
     CAST( @p__linq__1 AS nvarchar(max)) AS [C10], 
    LOWER( CAST( @p__linq__2 AS nvarchar(max))) AS [C11], 
    N'SomeA' AS [C12]
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
                    .Select(b => new
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

                string expectedSql = @"SELECT 
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

            using (var db = new ToStringContext())
            {
                db.Configuration.UseDatabaseNullSemantics = true;
                var dateTime = new DateTime(2014, 01, 01);
                var dateTimeOffset = new DateTimeOffset(dateTime);
                var actualSql = db.Entities
                    .Where(e => e.NullableBoolProp == null)
                    .Select(b => new
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

                string expectedSql = @"SELECT 
    1 AS [C1], 
    CAST(NULL AS varchar(1)) AS [C2], 
    CAST(NULL AS varchar(1)) AS [C3], 
    CAST(NULL AS varchar(1)) AS [C4], 
    CAST(NULL AS varchar(1)) AS [C5], 
    CAST(NULL AS varchar(1)) AS [C6], 
    CAST(NULL AS varchar(1)) AS [C7], 
    CAST(NULL AS varchar(1)) AS [C8], 
    CAST(NULL AS varchar(1)) AS [C9], 
    CAST(NULL AS varchar(1)) AS [C10], 
    LOWER(CAST(NULL AS varchar(1))) AS [C11], 
    CAST(NULL AS varchar(1)) AS [C12]
    FROM [dbo].[SomeEntities] AS [Extent1]
    WHERE [Extent1].[NullableBoolProp] IS NULL";

                QueryTestHelpers.VerifyDbQuery(actualSql, expectedSql);
            }
        }

    }
}