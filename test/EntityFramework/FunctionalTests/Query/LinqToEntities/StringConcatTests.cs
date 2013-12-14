// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.ELinq
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Query;
    using System.Linq;
    using System.Linq.Expressions;
    using Xunit;

    public class StringConcatTests : FunctionalTestBase
    {
        public enum SomeEnum
        {
            SomeA=0,
            SomeB=1,
            SomeC=2,
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
        }

        public class StringConcatContext : DbContext
        {
            public DbSet<SomeEntity> Entities { get; set; }
        }

        public class StringConcatInitializer : DropCreateDatabaseAlways<StringConcatContext>
        {
            protected override void Seed(StringConcatContext db)
            {
                base.Seed(db);

                for (int i = -1; i < 20; i++)
                {
                    var blog = new SomeEntity
                    {
                        StringProp = i.ToString(),
                        BoolProp = i % 2 == 0,
                        ByteProp = (byte)(i & 255),
                        DecimalProp = (decimal)(i +0.5),
                        DoubleProp = (i +0.5),
                        FloatProp = (float)(i+0.5),
                        GuidProp = Guid.NewGuid(),
                        IntProp = i,
                        LongProp = i,
                        EnumProp = SomeEnum.SomeA,
                        DateTimeProp = DateTime.Now,
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
                    };
                    db.Entities.Add(blog); 
                }
            }
        }

        static StringConcatTests()
        {
            Database.SetInitializer<StringConcatContext>(new StringConcatInitializer());
        }

        [Fact]
        public void Issue1904_StringConcatPlus_can_concat_constant_string_with_property_value()
        {
            using (var db = new StringConcatContext())
            {
                var projections = db.Entities.Select(b => new
                {
                    Entity = b,
                    StringProp = "a" + b.StringProp + "b",
                    BoolProp = "a" + b.BoolProp + "b",
                    GuidProp = "a" + b.GuidProp + "b",
                    ByteProp = "a" + b.ByteProp + "b",
                    IntProp = "a" + b.IntProp + "b",
                    LongProp = "a" + b.LongProp + "b",
                    DoubleProp = "a" + b.DoubleProp + "b",
                    FloatProp = "a" + b.FloatProp + "b",
                    DecimalProp = "a" + b.DecimalProp + "b",
                    EnumProp = "a" + b.EnumProp + "b",
                    DateTimeProp = "a" + b.DateTimeProp + "b",
                }).ToArray();


                //Compare projected properties to values calculated in .NET
                foreach (var projection in projections)
                {
                    //assert string + primitive
                    Assert.Equal("a" + projection.Entity.StringProp + "b", projection.StringProp);
                    Assert.Equal("a" + projection.Entity.IntProp + "b", projection.IntProp);
                    Assert.Equal("a" + projection.Entity.LongProp + "b", projection.LongProp);
                    Assert.Equal("a" + projection.Entity.ByteProp + "b", projection.ByteProp);
                    Assert.Equal("a" + projection.Entity.BoolProp + "b", projection.BoolProp);
                    Assert.Equal("a" + projection.Entity.GuidProp + "b", projection.GuidProp);
                    Assert.Equal("a" + projection.Entity.FloatProp.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "b", projection.FloatProp);
                    Assert.Equal("a" + projection.Entity.DoubleProp.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "b", projection.DoubleProp);
                    Assert.Equal("a" + projection.Entity.DecimalProp.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "b", projection.DecimalProp);
                    Assert.Equal("a" + projection.Entity.EnumProp + "b", projection.EnumProp);
                    //DateTime.ToString() uses DB localization semantics, unknown expected
                    Assert.True(projection.DateTimeProp.StartsWith("a") && projection.DateTimeProp.EndsWith("b"));
                }
            }
        }

        [Fact]
        public void Issue1904_StringConcatMethod_can_concat_constant_string_with_property_value()
        {
            using (var db = new StringConcatContext())
            {
                var projections = db.Entities.Select(b => new
                {
                    Entity = b,
                    StringProp = string.Concat("a" , b.StringProp , "b"),
                    BoolProp = string.Concat("a" , b.BoolProp , "b"),
                    GuidProp = string.Concat("a" , b.GuidProp , "b"),
                    ByteProp = string.Concat("a" , b.ByteProp , "b"),
                    IntProp = string.Concat("a" , b.IntProp , "b"),
                    LongProp = string.Concat("a" , b.LongProp , "b"),
                    DoubleProp = string.Concat("a" , b.DoubleProp , "b"),
                    FloatProp = string.Concat("a" , b.FloatProp , "b"),
                    DecimalProp = string.Concat("a" , b.DecimalProp , "b"),
                    EnumProp = string.Concat("a" , b.EnumProp , "b"),
                    DateTimeProp = string.Concat("a",b.DateTimeProp,"b")
                }).ToArray();

                //Compare projected properties to values calculated in .NET
                foreach (var projection in projections)
                {
                    //assert string + primitive
                    Assert.Equal("a" + projection.Entity.StringProp + "b", projection.StringProp);
                    Assert.Equal("a" + projection.Entity.IntProp + "b", projection.IntProp);
                    Assert.Equal("a" + projection.Entity.LongProp + "b", projection.LongProp);
                    Assert.Equal("a" + projection.Entity.ByteProp + "b", projection.ByteProp);
                    Assert.Equal("a" + projection.Entity.BoolProp + "b", projection.BoolProp);
                    Assert.Equal("a" + projection.Entity.GuidProp + "b", projection.GuidProp);
                    Assert.Equal("a" + projection.Entity.FloatProp.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "b", projection.FloatProp);
                    Assert.Equal("a" + projection.Entity.DoubleProp.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "b", projection.DoubleProp);
                    Assert.Equal("a" + projection.Entity.DecimalProp.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "b", projection.DecimalProp);
                    Assert.Equal("a" + projection.Entity.EnumProp + "b", projection.EnumProp);
                    //DateTime.ToString() uses DB localization semantics, unknown expected
                    Assert.True(projection.DateTimeProp.StartsWith("a") && projection.DateTimeProp.EndsWith("b"));
                }
            }
        }

        [Fact]
        public void Issue1904_StringConcatPlus_can_concat_constant_string_with_constant_null_primitive()
        {
            using (var db = new StringConcatContext())
            {
                var projections = db.Entities.Select(b => new
                {
                    Entity = b,
                    StringProp = "a" + (string)null + "b",
                    BoolProp = "a" + (bool?)null + "b",
                    GuidProp = "a" + (Guid?)null + "b",
                    ByteProp = "a" + (byte?)null + "b",
                    IntProp = "a" + (int?)null + "b",
                    LongProp = "a" + (long?)null + "b",
                    DoubleProp = "a" + (double?)null + "b",
                    FloatProp = "a" + (float?)null + "b",
                    DecimalProp = "a" + (decimal?)null + "b",
                    EnumProp = "a" + (SomeEnum?)null + "b",
                    DateTimeProp = "a" + (DateTime?)null + "b",
                }).ToArray();


                //Ensure consistent null behavior with .NET
                foreach (var projection in projections)
                {
                    //assert string + int
                    Assert.Equal("ab" , projection.StringProp);
                    Assert.Equal("ab" , projection.IntProp);
                    Assert.Equal("ab" , projection.LongProp);
                    Assert.Equal("ab" , projection.ByteProp);
                    Assert.Equal("ab" , projection.BoolProp);
                    Assert.Equal("ab" , projection.GuidProp);
                    Assert.Equal("ab" , projection.FloatProp);
                    Assert.Equal("ab" , projection.DoubleProp);
                    Assert.Equal("ab" , projection.DecimalProp);
                    Assert.Equal("ab", projection.EnumProp);
                    Assert.Equal("ab", projection.DateTimeProp);
                }
            }
        }

        [Fact]
        public void Issue1904_StringConcatMethod_can_concat_constant_string_with_constant_null_primitive()
        {
            using (var db = new StringConcatContext())
            {
                var projections = db.Entities.Select(b => new
                {
                    Entity = b,
                    StringProp = string.Concat("a" , (string)null , "b"),
                    BoolProp = string.Concat("a" , (bool?)null , "b"),
                    GuidProp = string.Concat("a" , (Guid?)null , "b"),
                    ByteProp = string.Concat("a" , (byte?)null , "b"),
                    IntProp = string.Concat("a" , (int?)null , "b"),
                    LongProp = string.Concat("a" , (long?)null , "b"),
                    DoubleProp = string.Concat("a" , (double?)null , "b"),
                    FloatProp = string.Concat("a" , (float?)null , "b"),
                    DecimalProp = string.Concat("a", (decimal?)null, "b"),
                    EnumProp = string.Concat("a",(SomeEnum?)null,"b"),
                    DateTimeProp = string.Concat("a", (DateTime?)null, "b"),
                }).ToArray();

                //Ensure consistent null behavior with .NET
                foreach (var projection in projections)
                {
                    //assert string + int
                    Assert.Equal("ab", projection.StringProp);
                    Assert.Equal("ab", projection.IntProp);
                    Assert.Equal("ab", projection.LongProp);
                    Assert.Equal("ab", projection.ByteProp);
                    Assert.Equal("ab", projection.BoolProp);
                    Assert.Equal("ab", projection.GuidProp);
                    Assert.Equal("ab", projection.FloatProp);
                    Assert.Equal("ab", projection.DoubleProp);
                    Assert.Equal("ab", projection.DecimalProp);
                    Assert.Equal("ab", projection.EnumProp);
                    Assert.Equal("ab", projection.DateTimeProp);
                }
            }
        }

        [Fact]
        public void Issue1904_StringConcatPlus_can_concat_constant_string_with_nullable_property()
        {
            using (var db = new StringConcatContext())
            {
                var projections = db.Entities
                    .Select(b => new
                    {
                        Entity = b,
                        BoolProp = "a" + b.NullableBoolProp + "b",
                        GuidProp = "a" + b.NullableGuidProp + "b",
                        ByteProp = "a" + b.NullableByteProp + "b",
                        IntProp = "a" + b.NullableIntProp + "b",
                        LongProp = "a" + b.NullableLongProp + "b",
                        DoubleProp = "a" + b.NullableDoubleProp + "b",
                        FloatProp = "a" + b.NullableFloatProp + "b",
                        DecimalProp = "a" + b.NullableDecimalProp + "b",
                        EnumProp = "a" + b.NullableEnumProp + "b",
                    }).ToArray();

                //Ensure consistent null behavior with .NET
                foreach (var projection in projections)
                {
                    //assert string + primitive
                    Assert.Equal("a" + projection.Entity.NullableIntProp + "b", projection.IntProp);
                    Assert.Equal("a" + projection.Entity.NullableLongProp + "b", projection.LongProp);
                    Assert.Equal("a" + projection.Entity.NullableByteProp + "b", projection.ByteProp);
                    Assert.Equal("a" + projection.Entity.NullableBoolProp + "b", projection.BoolProp);
                    Assert.Equal("a" + projection.Entity.NullableGuidProp + "b", projection.GuidProp);
                    Assert.Equal("a" + (projection.Entity.NullableFloatProp.HasValue ? projection.Entity.NullableFloatProp.Value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) : null) + "b", projection.FloatProp);
                    Assert.Equal("a" + (projection.Entity.NullableDoubleProp.HasValue ? projection.Entity.NullableDoubleProp.Value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) : null) + "b", projection.DoubleProp);
                    Assert.Equal("a" + (projection.Entity.NullableDecimalProp.HasValue ? projection.Entity.NullableDecimalProp.Value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) : null) + "b", projection.DecimalProp);
                    Assert.Equal("a" + projection.Entity.NullableEnumProp + "b", projection.EnumProp);
                }
            }
        }

        [Fact]
        public void Issue1904_StringConcatMethod_can_concat_constant_string_with_nullable_property()
        {
            using (var db = new StringConcatContext())
            {
                var projections = db.Entities
                    .Select(b => new
                    {
                        Entity = b,
                        BoolProp = string.Concat("a", b.NullableBoolProp, "b"),
                        GuidProp = string.Concat("a", b.NullableGuidProp, "b"),
                        ByteProp = string.Concat("a", b.NullableByteProp, "b"),
                        IntProp = string.Concat("a", b.NullableIntProp, "b"),
                        LongProp = string.Concat("a", b.NullableLongProp, "b"),
                        DoubleProp = string.Concat("a", b.NullableDoubleProp, "b"),
                        FloatProp = string.Concat("a", b.NullableFloatProp, "b"),
                        DecimalProp = string.Concat("a", b.NullableDecimalProp, "b"),
                        EnumProp = string.Concat("a", b.NullableEnumProp, "b"),
                    }).ToArray();


                //Ensure consistent null behavior with .NET
                foreach (var projection in projections)
                {
                    //assert string + primitive
                    Assert.Equal("a" + projection.Entity.NullableIntProp + "b", projection.IntProp);
                    Assert.Equal("a" + projection.Entity.NullableLongProp + "b", projection.LongProp);
                    Assert.Equal("a" + projection.Entity.NullableByteProp + "b", projection.ByteProp);
                    Assert.Equal("a" + projection.Entity.NullableBoolProp + "b", projection.BoolProp);
                    Assert.Equal("a" + projection.Entity.NullableGuidProp + "b", projection.GuidProp);
                    Assert.Equal("a" + (projection.Entity.NullableFloatProp.HasValue ? projection.Entity.NullableFloatProp.Value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) : null) + "b", projection.FloatProp);
                    Assert.Equal("a" + (projection.Entity.NullableDoubleProp.HasValue ? projection.Entity.NullableDoubleProp.Value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) : null) + "b", projection.DoubleProp);
                    Assert.Equal("a" + (projection.Entity.NullableDecimalProp.HasValue ? projection.Entity.NullableDecimalProp.Value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) : null) + "b", projection.DecimalProp);
                    Assert.Equal("a" + projection.Entity.NullableEnumProp + "b", projection.EnumProp);
                }
            }
        }

        [Fact]
        public void Issue1904_StringConcatPlus_can_concat_constant_null_with_nullable_property()
        {
            using (var db = new StringConcatContext())
            {
                var projections = db.Entities
                    .Select(b => new
                    {
                        Entity = b,
                        BoolProp = (string)null + b.NullableBoolProp + "b",
                        GuidProp = (string)null + b.NullableGuidProp + "b",
                        ByteProp = (string)null + b.NullableByteProp + "b",
                        IntProp = (string)null + b.NullableIntProp + "b",
                        LongProp = (string)null + b.NullableLongProp + "b",
                        DoubleProp = (string)null + b.NullableDoubleProp + "b",
                        FloatProp = (string)null + b.NullableFloatProp + "b",
                        DecimalProp = (string)null + b.NullableDecimalProp + "b",
                        EnumProp = (string)null + b.NullableEnumProp + "b",
                    }).ToArray();


                //Ensure consistent null behavior with .NET
                foreach (var projection in projections)
                {
                    //assert string + primitive
                    Assert.Equal((string)null + projection.Entity.NullableIntProp + "b", projection.IntProp);
                    Assert.Equal((string)null + projection.Entity.NullableLongProp + "b", projection.LongProp);
                    Assert.Equal((string)null + projection.Entity.NullableByteProp + "b", projection.ByteProp);
                    Assert.Equal((string)null + projection.Entity.NullableBoolProp + "b", projection.BoolProp);
                    Assert.Equal((string)null + projection.Entity.NullableGuidProp + "b", projection.GuidProp);
                    Assert.Equal((string)null + (projection.Entity.NullableFloatProp.HasValue ? projection.Entity.NullableFloatProp.Value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) : null) + "b", projection.FloatProp);
                    Assert.Equal((string)null + (projection.Entity.NullableDoubleProp.HasValue ? projection.Entity.NullableDoubleProp.Value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) : null) + "b", projection.DoubleProp);
                    Assert.Equal((string)null + (projection.Entity.NullableDecimalProp.HasValue ? projection.Entity.NullableDecimalProp.Value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) : null) + "b", projection.DecimalProp);
                    Assert.Equal((string)null + projection.Entity.NullableEnumProp + "b", projection.EnumProp);
                }
            }
        }

        [Fact]
        public void Issue1904_StringConcatMethod_can_concat_constant_null_with_nullable_property()
        {
            using (var db = new StringConcatContext())
            {
                var projections = db.Entities
                    .Select(b => new
                    {
                        Entity = b,
                        BoolProp = string.Concat((string)null , b.NullableBoolProp , "b"),
                        GuidProp = string.Concat((string)null, b.NullableGuidProp, "b"),
                        ByteProp = string.Concat((string)null, b.NullableByteProp, "b"),
                        IntProp = string.Concat((string)null, b.NullableIntProp, "b"),
                        LongProp = string.Concat((string)null, b.NullableLongProp, "b"),
                        DoubleProp = string.Concat((string)null, b.NullableDoubleProp, "b"),
                        FloatProp = string.Concat((string)null, b.NullableFloatProp, "b"),
                        DecimalProp = string.Concat((string)null, b.NullableDecimalProp, "b"),
                        EnumProp = string.Concat((string)null, b.NullableEnumProp, "b"),
                    }).ToArray();

                //Ensure consistent null behavior with .NET
                foreach (var projection in projections)
                {
                    //assert string + primitive
                    Assert.Equal((string)null + projection.Entity.NullableIntProp + "b", projection.IntProp);
                    Assert.Equal((string)null + projection.Entity.NullableLongProp + "b", projection.LongProp);
                    Assert.Equal((string)null + projection.Entity.NullableByteProp + "b", projection.ByteProp);
                    Assert.Equal((string)null + projection.Entity.NullableBoolProp + "b", projection.BoolProp);
                    Assert.Equal((string)null + projection.Entity.NullableGuidProp + "b", projection.GuidProp);
                    Assert.Equal((string)null + (projection.Entity.NullableFloatProp.HasValue?projection.Entity.NullableFloatProp.Value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo):null) + "b", projection.FloatProp);
                    Assert.Equal((string)null + (projection.Entity.NullableDoubleProp.HasValue?projection.Entity.NullableDoubleProp.Value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo):null) + "b", projection.DoubleProp);
                    Assert.Equal((string)null + (projection.Entity.NullableDecimalProp.HasValue?projection.Entity.NullableDecimalProp.Value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo):null) + "b", projection.DecimalProp);
                    Assert.Equal((string)null + projection.Entity.NullableEnumProp + "b", projection.EnumProp);
                }
            }
        }

        [Fact]
        public void Issue1904_StringConcat_generates_correct_sql()
        {
            using (var db = new StringConcatContext())
            {
                var actualSql = db.Entities
                    .Select(b => new
                    {
                        ConcatMethod = string.Concat(b.StringProp,b.BoolProp,b.EnumProp,b.GuidProp),
                        ConcatPlus = b.StringProp + b.BoolProp + b.EnumProp + b.GuidProp,
                    });

                var expectedSql = @"SELECT 
    [Extent1].[EnumProp] AS [EnumProp], 
    CASE WHEN ([Extent1].[StringProp] IS NULL) THEN N'' ELSE [Extent1].[StringProp] END + CASE WHEN ([Extent1].[BoolProp] = 1) THEN N'True' WHEN ([Extent1].[BoolProp] = 0) THEN N'False' ELSE N'' END + CASE WHEN ( CAST( [Extent1].[EnumProp] AS int) = 0) THEN N'SomeA' WHEN ( CAST( [Extent1].[EnumProp] AS int) = 1) THEN N'SomeB' WHEN ( CAST( [Extent1].[EnumProp] AS int) = 2) THEN N'SomeC' WHEN ( CAST( [Extent1].[EnumProp] AS int) IS NULL) THEN N'' ELSE  CAST(  CAST( [Extent1].[EnumProp] AS int) AS nvarchar(max)) END + LOWER( CAST( [Extent1].[GuidProp] AS nvarchar(max))) AS [C1], 
    CASE WHEN ([Extent1].[StringProp] IS NULL) THEN N'' ELSE [Extent1].[StringProp] END + CASE WHEN ([Extent1].[BoolProp] = 1) THEN N'True' WHEN ([Extent1].[BoolProp] = 0) THEN N'False' ELSE N'' END + CASE WHEN ( CAST( [Extent1].[EnumProp] AS int) = 0) THEN N'SomeA' WHEN ( CAST( [Extent1].[EnumProp] AS int) = 1) THEN N'SomeB' WHEN ( CAST( [Extent1].[EnumProp] AS int) = 2) THEN N'SomeC' WHEN ( CAST( [Extent1].[EnumProp] AS int) IS NULL) THEN N'' ELSE  CAST(  CAST( [Extent1].[EnumProp] AS int) AS nvarchar(max)) END + LOWER( CAST( [Extent1].[GuidProp] AS nvarchar(max))) AS [C2]
    FROM [dbo].[SomeEntities] AS [Extent1]";
                QueryTestHelpers.VerifyDbQuery (actualSql,expectedSql);
            }

            using (var db = new StringConcatContext())
            {
                db.Configuration.UseDatabaseNullSemantics = true;
                var actualSql = db.Entities
                    .Select(b => new
                    {
                        ConcatMethod = string.Concat(b.StringProp, b.BoolProp, b.EnumProp, b.GuidProp),
                        ConcatPlus = b.StringProp + b.BoolProp + b.EnumProp + b.GuidProp,
                    });

                var expectedSql = @"SELECT 
    [Extent1].[EnumProp] AS [EnumProp], 
    [Extent1].[StringProp] + CASE WHEN ([Extent1].[BoolProp] = 1) THEN N'True' WHEN ([Extent1].[BoolProp] = 0) THEN N'False' END + CASE WHEN ( CAST( [Extent1].[EnumProp] AS int) = 0) THEN N'SomeA' WHEN ( CAST( [Extent1].[EnumProp] AS int) = 1) THEN N'SomeB' WHEN ( CAST( [Extent1].[EnumProp] AS int) = 2) THEN N'SomeC' WHEN ( CAST( [Extent1].[EnumProp] AS int) IS NULL) THEN CAST(NULL AS varchar(1)) ELSE  CAST(  CAST( [Extent1].[EnumProp] AS int) AS nvarchar(max)) END + LOWER( CAST( [Extent1].[GuidProp] AS nvarchar(max))) AS [C1], 
    [Extent1].[StringProp] + CASE WHEN ([Extent1].[BoolProp] = 1) THEN N'True' WHEN ([Extent1].[BoolProp] = 0) THEN N'False' END + CASE WHEN ( CAST( [Extent1].[EnumProp] AS int) = 0) THEN N'SomeA' WHEN ( CAST( [Extent1].[EnumProp] AS int) = 1) THEN N'SomeB' WHEN ( CAST( [Extent1].[EnumProp] AS int) = 2) THEN N'SomeC' WHEN ( CAST( [Extent1].[EnumProp] AS int) IS NULL) THEN CAST(NULL AS varchar(1)) ELSE  CAST(  CAST( [Extent1].[EnumProp] AS int) AS nvarchar(max)) END + LOWER( CAST( [Extent1].[GuidProp] AS nvarchar(max))) AS [C2]
    FROM [dbo].[SomeEntities] AS [Extent1]";
                QueryTestHelpers.VerifyDbQuery(actualSql, expectedSql);
            }
        }
    }
}