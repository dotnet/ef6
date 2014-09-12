// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.ELinq
{
    using System.Data.Entity.Query;
    using System.Globalization;
    using System.Linq;
    using Xunit;

    public class StringConcatTests : FunctionalTestBase
    {
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
            public short ShortProp { get; set; }
            public int IntProp { get; set; }
            public long LongProp { get; set; }
            public byte ByteProp { get; set; }
            public bool BoolProp { get; set; }
            public decimal DecimalProp { get; set; }
            public float FloatProp { get; set; }
            public double DoubleProp { get; set; }
            public SomeEnum EnumProp { get; set; }
            public DateTime DateTimeProp { get; set; }
            public DateTimeOffset DateTimeOffsetProp { get; set; }
            public TimeSpan TimeSpanProp { get; set; }

            public Guid? NullableGuidProp { get; set; }
            public short? NullableShortProp { get; set; }
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
            public TimeSpan? NullableTimeSpanProp { get; set; }
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

                for (var i = -1; i < 20; i++)
                {
                    var blog = new SomeEntity
                    {
                        StringProp = i.ToString(),
                        GuidProp = Guid.NewGuid(),
                        ShortProp = (short)(i + 1),
                        IntProp = i + 2,
                        LongProp = i + 3,
                        ByteProp = (byte)(i & 255),
                        BoolProp = i % 2 == 0,
                        DecimalProp = (decimal)(i + 0.3),
                        FloatProp = (float)(i + 0.5),
                        DoubleProp = (i + 0.7),
                        EnumProp = (SomeEnum)i,
                        DateTimeProp = DateTime.Now,
                        DateTimeOffsetProp = DateTimeOffset.Now,
                        TimeSpanProp = DateTimeOffset.UtcNow.Offset,

                        NullableGuidProp = null,
                        NullableShortProp = null,
                        NullableIntProp = null,
                        NullableLongProp = null,
                        NullableByteProp = null,
                        NullableBoolProp = null,
                        NullableDecimalProp = null,
                        NullableFloatProp = null,
                        NullableDoubleProp = null,
                        NullableEnumProp = null,
                        NullableDateTimeProp = null,
                        NullableDateTimeOffsetProp = null,
                        NullableTimeSpanProp = null
                    };
                    db.Entities.Add(blog);

                    blog = new SomeEntity
                    {
                        StringProp = i.ToString(),
                        GuidProp = Guid.NewGuid(),
                        ShortProp = (short)(i + 1),
                        IntProp = i + 2,
                        LongProp = i + 3,
                        ByteProp = (byte)(i & 255),
                        BoolProp = i % 2 == 0,
                        DecimalProp = (decimal)(i + 0.2),
                        FloatProp = (float)(i + 0.4),
                        DoubleProp = (i + 0.6),
                        EnumProp = (SomeEnum)(i - 1),
                        DateTimeProp = DateTime.Now,
                        DateTimeOffsetProp = DateTimeOffset.Now,
                        TimeSpanProp = DateTimeOffset.UtcNow.Offset,

                        NullableGuidProp = Guid.NewGuid(),
                        NullableShortProp = (short)(i + 1),
                        NullableIntProp = i + 2,
                        NullableLongProp = i +3,
                        NullableByteProp = (byte)(i & 255),
                        NullableBoolProp = i % 2 == 0,
                        NullableDecimalProp = (decimal)(i + 0.1),
                        NullableFloatProp = (float)(i + 0.4),
                        NullableDoubleProp = (i + 0.8),
                        NullableEnumProp = (SomeEnum)(i + 1),
                        NullableDateTimeProp = DateTime.Now,
                        NullableDateTimeOffsetProp = DateTimeOffset.Now,
                        NullableTimeSpanProp = DateTimeOffset.UtcNow.Offset
                    };
                    db.Entities.Add(blog); 
                }
            }
        }

        static StringConcatTests()
        {
            Database.SetInitializer(new StringConcatInitializer());
        }

        [Fact]
        public void Issue1904_StringConcatPlus_can_concat_constant_string_with_property_value()
        {
            using (var db = new StringConcatContext())
            {

                var q = db.Entities.Select(b => new
                {
                    Entity = b,
                    StringProp = "a" + b.StringProp + "b",
                    BoolProp = "a" + b.BoolProp + "b",
                    GuidProp = "a" + b.GuidProp + "b",
                    ByteProp = "a" + b.ByteProp + "b",
                    ShortProp = "a" + b.ShortProp + "b",
                    IntProp = "a" + b.IntProp + "b",
                    LongProp = "a" + b.LongProp + "b",
                    DoubleProp = "a" + b.DoubleProp + "b",
                    FloatProp = "a" + b.FloatProp + "b",
                    DecimalProp = "a" + b.DecimalProp + "b",
                    EnumProp = "a" + b.EnumProp + "b",
                    DateTimeProp = "a" + b.DateTimeProp + "b",
                    DateTimeOffsetProp = "a" + b.DateTimeOffsetProp + "b",
                    TimeSpanProp = "a" + b.TimeSpanProp + "b"
                }).ToString();


                var projections = db.Entities.Select(b => new
                {
                    Entity = b,
                    StringProp = "a" + b.StringProp + "b",
                    BoolProp = "a" + b.BoolProp + "b",
                    GuidProp = "a" + b.GuidProp + "b",
                    ByteProp = "a" + b.ByteProp + "b",
                    ShortProp = "a" + b.ShortProp + "b",
                    IntProp = "a" + b.IntProp + "b",
                    LongProp = "a" + b.LongProp + "b",
                    DoubleProp = "a" + b.DoubleProp + "b",
                    FloatProp = "a" + b.FloatProp + "b",
                    DecimalProp = "a" + b.DecimalProp + "b",
                    EnumProp = "a" + b.EnumProp + "b",
                    DateTimeProp = "a" + b.DateTimeProp + "b",
                    DateTimeOffsetProp = "a" + b.DateTimeOffsetProp + "b",
                    TimeSpanProp = "a" + b.TimeSpanProp + "b"
                }).ToArray();


                //Compare projected properties to values calculated in .NET
                foreach (var projection in projections)
                {
                    Assert.Equal("a" + projection.Entity.StringProp + "b", projection.StringProp);
                    Assert.Equal("a" + projection.Entity.ShortProp + "b", projection.ShortProp);
                    Assert.Equal("a" + projection.Entity.IntProp + "b", projection.IntProp);
                    Assert.Equal("a" + projection.Entity.LongProp + "b", projection.LongProp);
                    Assert.Equal("a" + projection.Entity.ByteProp + "b", projection.ByteProp);
                    Assert.Equal("a" + projection.Entity.BoolProp + "b", projection.BoolProp);
                    Assert.Equal("a" + projection.Entity.GuidProp + "b", projection.GuidProp);
                    Assert.Equal("a" + projection.Entity.FloatProp.ToString(NumberFormatInfo.InvariantInfo) + "b", projection.FloatProp);
                    Assert.Equal("a" + projection.Entity.DoubleProp.ToString(NumberFormatInfo.InvariantInfo) + "b", projection.DoubleProp);
                    Assert.Equal("a" + projection.Entity.DecimalProp.ToString(NumberFormatInfo.InvariantInfo) + "b", projection.DecimalProp);
                    Assert.Equal("a" + projection.Entity.EnumProp + "b", projection.EnumProp);
                    //DateTime.ToString() uses DB localization settings, unknown expected
                    Assert.True(
                        projection.DateTimeProp.StartsWith("a") && 
                        projection.DateTimeProp.EndsWith("b") && 
                        projection.DateTimeProp.Contains(projection.Entity.DateTimeProp.Year.ToString()));
                    Assert.True(
                        projection.DateTimeOffsetProp.StartsWith("a") &&
                        projection.DateTimeOffsetProp.EndsWith("b") &&
                        projection.DateTimeOffsetProp.Contains(projection.Entity.DateTimeOffsetProp.Year.ToString()));
                    Assert.True(
                        projection.TimeSpanProp.StartsWith("a") &&
                        projection.TimeSpanProp.EndsWith("b") &&
                        projection.TimeSpanProp.Contains(projection.Entity.TimeSpanProp.Minutes.ToString()));
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
                    ShortProp = string.Concat("a" , b.ShortProp , "b"),
                    IntProp = string.Concat("a" , b.IntProp , "b"),
                    LongProp = string.Concat("a" , b.LongProp , "b"),
                    DoubleProp = string.Concat("a" , b.DoubleProp , "b"),
                    FloatProp = string.Concat("a" , b.FloatProp , "b"),
                    DecimalProp = string.Concat("a" , b.DecimalProp , "b"),
                    EnumProp = string.Concat("a" , b.EnumProp , "b"),
                    DateTimeProp = string.Concat("a",b.DateTimeProp,"b"),
                    DateTimeOffsetProp = string.Concat("a", b.DateTimeOffsetProp, "b"),
                    TimeSpanProp = string.Concat("a", b.TimeSpanProp, "b")
                }).ToArray();

                //Compare projected properties to values calculated in .NET
                foreach (var projection in projections)
                {
                    Assert.Equal("a" + projection.Entity.StringProp + "b", projection.StringProp);
                    Assert.Equal("a" + projection.Entity.IntProp + "b", projection.IntProp);
                    Assert.Equal("a" + projection.Entity.LongProp + "b", projection.LongProp);
                    Assert.Equal("a" + projection.Entity.ShortProp + "b", projection.ShortProp);
                    Assert.Equal("a" + projection.Entity.ByteProp + "b", projection.ByteProp);
                    Assert.Equal("a" + projection.Entity.BoolProp + "b", projection.BoolProp);
                    Assert.Equal("a" + projection.Entity.GuidProp + "b", projection.GuidProp);
                    Assert.Equal("a" + projection.Entity.FloatProp.ToString(NumberFormatInfo.InvariantInfo) + "b", projection.FloatProp);
                    Assert.Equal("a" + projection.Entity.DoubleProp.ToString(NumberFormatInfo.InvariantInfo) + "b", projection.DoubleProp);
                    Assert.Equal("a" + projection.Entity.DecimalProp.ToString(NumberFormatInfo.InvariantInfo) + "b", projection.DecimalProp);
                    Assert.Equal("a" + projection.Entity.EnumProp + "b", projection.EnumProp);
                    //DateTime.ToString() uses DB localization settings, unknown expected
                    Assert.True(
                        projection.DateTimeProp.StartsWith("a") &&
                        projection.DateTimeProp.EndsWith("b") &&
                        projection.DateTimeProp.Contains(projection.Entity.DateTimeProp.Year.ToString()));
                    Assert.True(
                        projection.DateTimeOffsetProp.StartsWith("a") &&
                        projection.DateTimeOffsetProp.EndsWith("b") &&
                        projection.DateTimeOffsetProp.Contains(projection.Entity.DateTimeOffsetProp.Year.ToString()));
                    Assert.True(
                        projection.TimeSpanProp.StartsWith("a") &&
                        projection.TimeSpanProp.EndsWith("b") &&
                        projection.TimeSpanProp.Contains(projection.Entity.TimeSpanProp.Minutes.ToString()));
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
                    ShortProp = "a" + (short?)null + "b",
                    IntProp = "a" + (int?)null + "b",
                    LongProp = "a" + (long?)null + "b",
                    DoubleProp = "a" + (double?)null + "b",
                    FloatProp = "a" + (float?)null + "b",
                    DecimalProp = "a" + (decimal?)null + "b",
                    EnumProp = "a" + (SomeEnum?)null + "b",
                    DateTimeProp = "a" + (DateTime?)null + "b",
                    DateTimeOffsetProp = "a" + (DateTimeOffset?)null + "b",
                    TimeSpanProp = "a" + (TimeSpan?)null + "b",
                }).ToArray();

                //Ensure consistent null behavior with .NET
                foreach (var projection in projections)
                {
                    Assert.Equal("ab", projection.StringProp);
                    Assert.Equal("ab", projection.ShortProp);
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
                    Assert.Equal("ab", projection.DateTimeOffsetProp);
                    Assert.Equal("ab", projection.TimeSpanProp);
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
                    ShortProp = string.Concat("a", (short?)null, "b"),
                    IntProp = string.Concat("a" , (int?)null , "b"),
                    LongProp = string.Concat("a" , (long?)null , "b"),
                    DoubleProp = string.Concat("a" , (double?)null , "b"),
                    FloatProp = string.Concat("a" , (float?)null , "b"),
                    DecimalProp = string.Concat("a", (decimal?)null, "b"),
                    EnumProp = string.Concat("a",(SomeEnum?)null,"b"),
                    DateTimeProp = string.Concat("a", (DateTime?)null, "b"),
                    DateTimeOffsetProp = string.Concat("a", (DateTimeOffset?)null, "b"),
                    TimeSpanProp = string.Concat("a", (TimeSpan?)null, "b"),
                }).ToArray();

                //Ensure consistent null behavior with .NET
                foreach (var projection in projections)
                {
                    Assert.Equal("ab", projection.StringProp);
                    Assert.Equal("ab", projection.ShortProp);
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
                    Assert.Equal("ab", projection.DateTimeOffsetProp);
                    Assert.Equal("ab", projection.TimeSpanProp);
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
                        ShortProp = "a" + b.NullableShortProp + "b",
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
                    Assert.Equal("a" + projection.Entity.NullableIntProp + "b", projection.IntProp);
                    Assert.Equal("a" + projection.Entity.NullableLongProp + "b", projection.LongProp);
                    Assert.Equal("a" + projection.Entity.NullableByteProp + "b", projection.ByteProp);
                    Assert.Equal("a" + projection.Entity.NullableShortProp + "b", projection.ShortProp);
                    Assert.Equal("a" + projection.Entity.NullableBoolProp + "b", projection.BoolProp);
                    Assert.Equal("a" + projection.Entity.NullableGuidProp + "b", projection.GuidProp);
                    Assert.Equal(
                        "a"
                        + (projection.Entity.NullableFloatProp.HasValue
                            ? projection.Entity.NullableFloatProp.Value.ToString(NumberFormatInfo.InvariantInfo)
                            : null) + "b", projection.FloatProp);
                    Assert.Equal(
                        "a"
                        + (projection.Entity.NullableDoubleProp.HasValue
                            ? projection.Entity.NullableDoubleProp.Value.ToString(NumberFormatInfo.InvariantInfo)
                            : null) + "b", projection.DoubleProp);
                    Assert.Equal(
                        "a"
                        + (projection.Entity.NullableDecimalProp.HasValue
                            ? projection.Entity.NullableDecimalProp.Value.ToString(NumberFormatInfo.InvariantInfo)
                            : null) + "b", projection.DecimalProp);
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
                        ShortProp = string.Concat("a", b.NullableShortProp, "b"),
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
                    Assert.Equal("a" + projection.Entity.NullableIntProp + "b", projection.IntProp);
                    Assert.Equal("a" + projection.Entity.NullableLongProp + "b", projection.LongProp);
                    Assert.Equal("a" + projection.Entity.NullableByteProp + "b", projection.ByteProp);
                    Assert.Equal("a" + projection.Entity.NullableShortProp + "b", projection.ShortProp);
                    Assert.Equal("a" + projection.Entity.NullableBoolProp + "b", projection.BoolProp);
                    Assert.Equal("a" + projection.Entity.NullableGuidProp + "b", projection.GuidProp);
                    Assert.Equal(
                        "a"
                        + (projection.Entity.NullableFloatProp.HasValue
                            ? projection.Entity.NullableFloatProp.Value.ToString(NumberFormatInfo.InvariantInfo)
                            : null) + "b", projection.FloatProp);
                    Assert.Equal(
                        "a"
                        + (projection.Entity.NullableDoubleProp.HasValue
                            ? projection.Entity.NullableDoubleProp.Value.ToString(NumberFormatInfo.InvariantInfo)
                            : null) + "b", projection.DoubleProp);
                    Assert.Equal(
                        "a"
                        + (projection.Entity.NullableDecimalProp.HasValue
                            ? projection.Entity.NullableDecimalProp.Value.ToString(NumberFormatInfo.InvariantInfo)
                            : null) + "b", projection.DecimalProp);
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
                        ShortProp = (string)null + b.NullableShortProp + "b",
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
                    Assert.Equal((string)null + projection.Entity.NullableShortProp + "b", projection.ShortProp);
                    Assert.Equal((string)null + projection.Entity.NullableBoolProp + "b", projection.BoolProp);
                    Assert.Equal((string)null + projection.Entity.NullableGuidProp + "b", projection.GuidProp);
                    Assert.Equal(
                        (string)null
                        + (projection.Entity.NullableFloatProp.HasValue
                            ? projection.Entity.NullableFloatProp.Value.ToString(NumberFormatInfo.InvariantInfo)
                            : null) + "b", projection.FloatProp);
                    Assert.Equal(
                        (string)null
                        + (projection.Entity.NullableDoubleProp.HasValue
                            ? projection.Entity.NullableDoubleProp.Value.ToString(NumberFormatInfo.InvariantInfo)
                            : null) + "b", projection.DoubleProp);
                    Assert.Equal(
                        (string)null
                        + (projection.Entity.NullableDecimalProp.HasValue
                            ? projection.Entity.NullableDecimalProp.Value.ToString(NumberFormatInfo.InvariantInfo)
                            : null) + "b", projection.DecimalProp);
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
                        ShortProp = string.Concat((string)null, b.NullableShortProp, "b"),
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
                    Assert.Equal((string)null + projection.Entity.NullableShortProp + "b", projection.ShortProp);
                    Assert.Equal((string)null + projection.Entity.NullableLongProp + "b", projection.LongProp);
                    Assert.Equal((string)null + projection.Entity.NullableByteProp + "b", projection.ByteProp);
                    Assert.Equal((string)null + projection.Entity.NullableBoolProp + "b", projection.BoolProp);
                    Assert.Equal((string)null + projection.Entity.NullableGuidProp + "b", projection.GuidProp);
                    Assert.Equal(
                        (string)null
                        + (projection.Entity.NullableFloatProp.HasValue
                            ? projection.Entity.NullableFloatProp.Value.ToString(NumberFormatInfo.InvariantInfo)
                            : null) + "b", projection.FloatProp);
                    Assert.Equal(
                        (string)null
                        + (projection.Entity.NullableDoubleProp.HasValue
                            ? projection.Entity.NullableDoubleProp.Value.ToString(NumberFormatInfo.InvariantInfo)
                            : null) + "b", projection.DoubleProp);
                    Assert.Equal(
                        (string)null
                        + (projection.Entity.NullableDecimalProp.HasValue
                            ? projection.Entity.NullableDecimalProp.Value.ToString(NumberFormatInfo.InvariantInfo)
                            : null) + "b", projection.DecimalProp);
                    Assert.Equal((string)null + projection.Entity.NullableEnumProp + "b", projection.EnumProp);
                }
            }
        }

        [Fact]
        public void Issue1904_StringConcatMethod_can_handle_constant_values_of_object_type()
        {
            using (var db = new StringConcatContext())
            {
                var actualSql = db.Entities.Select(
                    e => string.Concat(3, SomeEnum.SomeA, "xyz", e.DateTimeProp, (short)42)).ToString();

                Assert.Equal(@"SELECT 
     CAST( 3 AS nvarchar(max)) + N'SomeA' + N'xyz' +  CAST( [Extent1].[DateTimeProp] AS nvarchar(max)) +  CAST( cast(42 as smallint) AS nvarchar(max)) AS [C1]
    FROM [dbo].[SomeEntities] AS [Extent1]", actualSql);
            }
        }

        [Fact]
        public void Issue2075_StringConcatMethod_can_handle_constant_values_of_string_type()
        {
            using (var db = new StringConcatContext())
            {
                var actualSql = db.Entities.Select(
                    e => string.Concat("3", "SomeEnum.SomeA", "xyz", e.StringProp, "abc", e.StringProp)).ToString();

                Assert.Equal(@"SELECT 
    N'3' + N'SomeEnum.SomeA' + N'xyz' + CASE WHEN ([Extent1].[StringProp] IS NULL) THEN N'' ELSE [Extent1].[StringProp] END + N'abc' + CASE WHEN ([Extent1].[StringProp] IS NULL) THEN N'' ELSE [Extent1].[StringProp] END AS [C1]
    FROM [dbo].[SomeEntities] AS [Extent1]", actualSql);
            }
        }

        [Fact]
        public void Issue1904_StringConcatMethod_can_handle_arguments_created_explicitly_as_object_array()
        {          
            using (var db = new StringConcatContext())
            {
                var args = new object[] { 3, SomeEnum.SomeA, "xyz", (short)42 };
                var actualSql = db.Entities.Select(e => string.Concat(args)).ToString();

                Assert.Equal(@"SELECT 
     CAST( 3 AS nvarchar(max)) + N'SomeA' + N'xyz' +  CAST( cast(42 as smallint) AS nvarchar(max)) AS [C1]
    FROM [dbo].[SomeEntities] AS [Extent1]", actualSql);
            }
        }

        [Fact]
        public void Issue2073_StringConcatMethod_can_handle_arguments_created_explicitly_as_string_array()
        {
            using (var db = new StringConcatContext())
            {
                var args = new [] { "3", "SomeEnum.SomeA", "xyz", "42" };
                var actualSql = db.Entities.Select(e => string.Concat(args)).ToString();

                Assert.Equal(@"SELECT 
    N'3' + N'SomeEnum.SomeA' + N'xyz' + N'42' AS [C1]
    FROM [dbo].[SomeEntities] AS [Extent1]", actualSql);
            }
        }

        [Fact]
        public void Issue1904_StringConcatMethod_collapses_null_values_to_empty_string()
        {
            using (var db = new StringConcatContext())
            {
                foreach (var useDbNullSemantics in new[] { true, false })
                {
                    db.Configuration.UseDatabaseNullSemantics = useDbNullSemantics;

                    var actualSql = db.Entities.Select(e => string.Concat(null, null, null)).ToString();

                    Assert.Equal(@"SELECT 
    N'' AS [C1]
    FROM [dbo].[SomeEntities] AS [Extent1]", actualSql);
                }
            }
        }

        [Fact]
        public void Issue1904_StringConcat_generates_correct_sql()
        {
            using (var db = new StringConcatContext())
            {
                var actualSql = db.Entities
                    .Select(
                        b => new
                        {
                            ConcatMethod = string.Concat(b.StringProp, b.BoolProp, b.EnumProp, b.GuidProp),
                            ConcatPlus = b.StringProp + b.BoolProp + b.EnumProp + b.GuidProp,
                        });

                const string expectedSql = @"SELECT 
    [Extent1].[EnumProp] AS [EnumProp], 
    CASE WHEN ([Extent1].[StringProp] IS NULL) THEN N'' ELSE [Extent1].[StringProp] END + CASE WHEN ([Extent1].[BoolProp] = 1) THEN N'True' WHEN ([Extent1].[BoolProp] = 0) THEN N'False' ELSE N'' END + CASE WHEN ([Extent1].[EnumProp] = 0) THEN N'SomeA' WHEN ([Extent1].[EnumProp] = 1) THEN N'SomeB' WHEN ([Extent1].[EnumProp] = 2) THEN N'SomeC' WHEN ( CAST( [Extent1].[EnumProp] AS int) IS NULL) THEN N'' ELSE  CAST(  CAST( [Extent1].[EnumProp] AS int) AS nvarchar(max)) END + LOWER( CAST( [Extent1].[GuidProp] AS nvarchar(max))) AS [C1], 
    CASE WHEN ([Extent1].[StringProp] IS NULL) THEN N'' ELSE [Extent1].[StringProp] END + CASE WHEN ([Extent1].[BoolProp] = 1) THEN N'True' WHEN ([Extent1].[BoolProp] = 0) THEN N'False' ELSE N'' END + CASE WHEN ([Extent1].[EnumProp] = 0) THEN N'SomeA' WHEN ([Extent1].[EnumProp] = 1) THEN N'SomeB' WHEN ([Extent1].[EnumProp] = 2) THEN N'SomeC' WHEN ( CAST( [Extent1].[EnumProp] AS int) IS NULL) THEN N'' ELSE  CAST(  CAST( [Extent1].[EnumProp] AS int) AS nvarchar(max)) END + LOWER( CAST( [Extent1].[GuidProp] AS nvarchar(max))) AS [C2]
    FROM [dbo].[SomeEntities] AS [Extent1]";
                QueryTestHelpers.VerifyDbQuery(actualSql, expectedSql);
            }
        }

        [Fact]
        public void Issue2075_StringConcat_generates_correct_sql_for_string_only_args()
        {
            using (var db = new StringConcatContext())
            {
                var actualSql = db.Entities
                    .Select(
                        b => new
                        {
                            ConcatMethod = string.Concat(b.StringProp, b.StringProp, b.StringProp, "x", b.StringProp),
                            ConcatPlus = b.StringProp + b.StringProp + b.StringProp + "x" + b.StringProp,
                        });

                const string expectedSql = @"SELECT 
    1 AS [C1], 
    CASE WHEN ([Extent1].[StringProp] IS NULL) THEN N'' ELSE [Extent1].[StringProp] END + CASE WHEN ([Extent1].[StringProp] IS NULL) THEN N'' ELSE [Extent1].[StringProp] END + CASE WHEN ([Extent1].[StringProp] IS NULL) THEN N'' ELSE [Extent1].[StringProp] END + N'x' + CASE WHEN ([Extent1].[StringProp] IS NULL) THEN N'' ELSE [Extent1].[StringProp] END AS [C2], 
    CASE WHEN ([Extent1].[StringProp] IS NULL) THEN N'' ELSE [Extent1].[StringProp] END + CASE WHEN ([Extent1].[StringProp] IS NULL) THEN N'' ELSE [Extent1].[StringProp] END + CASE WHEN ([Extent1].[StringProp] IS NULL) THEN N'' ELSE [Extent1].[StringProp] END + N'x' + CASE WHEN ([Extent1].[StringProp] IS NULL) THEN N'' ELSE [Extent1].[StringProp] END AS [C3]
    FROM [dbo].[SomeEntities] AS [Extent1]";

                QueryTestHelpers.VerifyDbQuery(actualSql, expectedSql);
            }
        }

        [Fact]
        public void Issue2075_StringConcat_throws_for_null_array_arg()
        {
            using (var db = new StringConcatContext())
            {
                Assert.Equal(
                    "args",
                    Assert.Throws<ArgumentNullException>(
                        () => db.Entities.Select(b => string.Concat((object[])null)).ToString()).ParamName);

                Assert.Equal(
                    "values",
                    Assert.Throws<ArgumentNullException>(
                        () => db.Entities.Select(b => string.Concat((string[])null)).ToString()).ParamName);
            }
        }

        public class CodePlex2457 : FunctionalTestBase
        {
            public class Customer
            {
                public int Id { get; set; }
                public string FirstName { get; set; }
                public string LastName { get; set; }
            }

            public class Context : DbContext
            {
                static Context()
                {
                    Database.SetInitializer<Context>(null);
                }

                public DbSet<Customer> Customers { get; set; }
            }

            [Fact]
            public void Concatenated_strings_are_checked_if_null_with_csharp_null_semantics()
            {
                using (var context = new Context())
                {
                    context.Configuration.UseDatabaseNullSemantics = false;

                    var query = context.Customers.Select(c => c.FirstName + c.LastName);

                    QueryTestHelpers.VerifyDbQuery(
                        query,
@"SELECT 
    CASE WHEN ([Extent1].[FirstName] IS NULL) THEN N'' ELSE [Extent1].[FirstName] END + CASE WHEN ([Extent1].[LastName] IS NULL) THEN N'' ELSE [Extent1].[LastName] END AS [C1]
    FROM [dbo].[Customers] AS [Extent1]");
                }
            }

            [Fact]
            public void Concatenated_strings_are_not_checked_if_null_with_database_null_semantics()
            {
                using (var context = new Context())
                {
                    context.Configuration.UseDatabaseNullSemantics = true;

                    var query = context.Customers.Select(c => c.FirstName + c.LastName);

                    QueryTestHelpers.VerifyDbQuery(
                        query,
@"SELECT 
    [Extent1].[FirstName] + [Extent1].[LastName] AS [C1]
    FROM [dbo].[Customers] AS [Extent1]");
                }
            }
        }
    }
}