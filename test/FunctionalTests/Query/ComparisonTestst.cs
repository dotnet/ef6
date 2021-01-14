// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query.LinqToEntities
{
    using System.Linq;
    using Xunit;

    public class ComparisonTests : FunctionalTestBase
    {
        [Fact]
        public void Compare_rows_containing_elements_with_different_MaxLength_values()
        {
            using (var ctx = new ComparisonTestContext())
            {
                var expectedSql =
@"SELECT 
    [Extent1].[Id] AS [Id]
    FROM [dbo].[StringEntities] AS [Extent1]
    WHERE ([Extent1].[Id] = [Extent1].[Id]) AND ([Extent1].[NonUnicodeFixedLength25] = [Extent1].[NonUnicodeFixedLength35])";

                var query = ctx.Strings.Where(s => new { first = s.Id, second = s.NonUnicodeFixedLength25 } == new { first = s.Id, second = s.NonUnicodeFixedLength35 }).Select(c => c.Id);
                QueryTestHelpers.VerifyQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Compare_rows_containing_fixed_length_with_variable_length()
        {
            using (var ctx = new ComparisonTestContext())
            {
                var expectedSql =
@"SELECT 
    [Extent1].[Id] AS [Id]
    FROM [dbo].[StringEntities] AS [Extent1]
    WHERE ([Extent1].[Id] = [Extent1].[Id]) AND ([Extent1].[NonUnicodeFixedLength25] = [Extent1].[NonUnicodeVariableLength25])";

                var query = ctx.Strings.Where(s => new { first = s.Id, second = s.NonUnicodeFixedLength25 } == new { first = s.Id, second = s.NonUnicodeVariableLength25 }).Select(c => c.Id);
                QueryTestHelpers.VerifyQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Compare_rows_containing_unicode_string_with_nonunicode_string()
        {
            using (var ctx = new ComparisonTestContext())
            {
                var expectedSql1 =
@"SELECT 
    [Extent1].[Id] AS [Id]
    FROM [dbo].[StringEntities] AS [Extent1]
    WHERE ([Extent1].[Id] = [Extent1].[Id]) AND ([Extent1].[UnicodeFixedLength25] = [Extent1].[NonUnicodeFixedLength25])";

                var query1 = ctx.Strings.Where(s => new { first = s.Id, second = s.UnicodeFixedLength25 } == new { first = s.Id, second = s.NonUnicodeFixedLength25 }).Select(c => c.Id);
                QueryTestHelpers.VerifyQuery(query1, expectedSql1);

                var expectedSql2 =
@"SELECT 
    [Extent1].[Id] AS [Id]
    FROM [dbo].[StringEntities] AS [Extent1]
    WHERE ([Extent1].[Id] = [Extent1].[Id]) AND ([Extent1].[UnicodeVariableLength25] = [Extent1].[NonUnicodeVariableLength35])";


                var query2 = ctx.Strings.Where(s => new { first = s.Id, second = s.UnicodeVariableLength25 } == new { first = s.Id, second = s.NonUnicodeVariableLength35 }).Select(c => c.Id);
                QueryTestHelpers.VerifyQuery(query2, expectedSql2);
            }
        }

        [Fact]
        public void Compare_rows_containing_nullable_string_with_nonnullable_string()
        {
            using (var ctx = new ComparisonTestContext())
            {
                var expectedSql =
@"SELECT 
    [Extent1].[Id] AS [Id]
    FROM [dbo].[StringEntities] AS [Extent1]
    WHERE ([Extent1].[Id] = [Extent1].[Id]) AND ([Extent1].[NonUnicodeFixedLength25] = [Extent1].[NullableNonUnicodeVariableLength25])";

                var query = ctx.Strings.Where(s => new { first = s.Id, second = s.NonUnicodeFixedLength25 } == new { first = s.Id, second = s.NullableNonUnicodeVariableLength25 }).Select(c => c.Id);
                QueryTestHelpers.VerifyQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Compare_rows_containing_decimal_with_numeric()
        {
            using (var ctx = new ComparisonTestContext())
            {
                var expectedSql =
@"SELECT 
    [Extent1].[Id] AS [Id]
    FROM [dbo].[NumericEntities] AS [Extent1]
    WHERE ([Extent1].[Id] = [Extent1].[Id]) AND ([Extent1].[DefaultDecimal] = [Extent1].[DefaultNumeric])";

                var query = ctx.Numerics.Where(s => new { first = s.Id, second = s.DefaultDecimal } == new { first = s.Id, second = s.DefaultNumeric }).Select(c => c.Id);
                QueryTestHelpers.VerifyQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Compare_rows_containing_decimal_with_with_different_precision()
        {
            using (var ctx = new ComparisonTestContext())
            {
                var expectedSql =
@"SELECT 
    [Extent1].[Id] AS [Id]
    FROM [dbo].[NumericEntities] AS [Extent1]
    WHERE ([Extent1].[Id] = [Extent1].[Id]) AND ([Extent1].[Decimal204] = [Extent1].[Decimal282])";

                var query = ctx.Numerics.Where(s => new { first = s.Id, second = s.Decimal204 } == new { first = s.Id, second = s.Decimal282 }).Select(c => c.Id);
                QueryTestHelpers.VerifyQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Compare_rows_containing_datetime_with_datetime2()
        {
            using (var ctx = new ComparisonTestContext())
            {
                var expectedSql =
@"SELECT 
    [Extent1].[Id] AS [Id]
    FROM [dbo].[DateTimeEntities] AS [Extent1]
    WHERE ([Extent1].[Id] = [Extent1].[Id]) AND ([Extent1].[DefaultDateTime] = [Extent1].[DefaultDateTime2])";

                var query = ctx.DateTimes.Where(s => new { first = s.Id, second = s.DefaultDateTime } == new { first = s.Id, second = s.DefaultDateTime2 }).Select(c => c.Id);
                QueryTestHelpers.VerifyQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Compare_rows_containing_datetime2_with_different_precision()
        {
            using (var ctx = new ComparisonTestContext())
            {
                var expectedSql =
@"SELECT 
    [Extent1].[Id] AS [Id]
    FROM [dbo].[DateTimeEntities] AS [Extent1]
    WHERE ([Extent1].[Id] = [Extent1].[Id]) AND ([Extent1].[DefaultDateTime2] = [Extent1].[DateTime25])";

                var query = ctx.DateTimes.Where(s => new { first = s.Id, second = s.DefaultDateTime2 } == new { first = s.Id, second = s.DateTime25 }).Select(c => c.Id);
                QueryTestHelpers.VerifyQuery(query, expectedSql);
            }
        }

        private class ComparisonTestContext : DbContext
        {
            public DbSet<StringEntity> Strings { get; set; }
            public DbSet<NumericEntity> Numerics { get; set; }
            public DbSet<DateTimeEntity> DateTimes { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<StringEntity>().Property(e => e.NonUnicodeFixedLength25).IsUnicode(false).IsFixedLength().HasMaxLength(25).IsRequired();
                modelBuilder.Entity<StringEntity>().Property(e => e.NonUnicodeFixedLength35).IsUnicode(false).IsFixedLength().HasMaxLength(35).IsRequired();
                modelBuilder.Entity<StringEntity>().Property(e => e.NonUnicodeVariableLength25).IsUnicode(false).IsVariableLength().HasMaxLength(25).IsRequired();
                modelBuilder.Entity<StringEntity>().Property(e => e.NonUnicodeVariableLength35).IsUnicode(false).IsVariableLength().HasMaxLength(35).IsRequired();

                modelBuilder.Entity<StringEntity>().Property(e => e.UnicodeFixedLength25).IsUnicode(true).IsFixedLength().HasMaxLength(25).IsRequired();
                modelBuilder.Entity<StringEntity>().Property(e => e.UnicodeFixedLength35).IsUnicode(true).IsFixedLength().HasMaxLength(35).IsRequired();
                modelBuilder.Entity<StringEntity>().Property(e => e.UnicodeVariableLength25).IsUnicode(true).IsVariableLength().HasMaxLength(25).IsRequired();
                modelBuilder.Entity<StringEntity>().Property(e => e.UnicodeVariableLength35).IsUnicode(true).IsVariableLength().HasMaxLength(35).IsRequired();

                modelBuilder.Entity<StringEntity>().Property(e => e.NullableNonUnicodeVariableLength25).IsUnicode(false).IsVariableLength().HasMaxLength(25).IsOptional();
                modelBuilder.Entity<StringEntity>().Property(e => e.NullableNonUnicodeVariableLength35).IsUnicode(false).IsVariableLength().HasMaxLength(35).IsOptional();

                modelBuilder.Entity<NumericEntity>().Property(e => e.DefaultDecimal).HasColumnType("decimal");
                modelBuilder.Entity<NumericEntity>().Property(e => e.Decimal204).HasColumnType("decimal").HasPrecision(20, 4);
                modelBuilder.Entity<NumericEntity>().Property(e => e.Decimal282).HasColumnType("decimal").HasPrecision(28, 2);

                modelBuilder.Entity<NumericEntity>().Property(e => e.DefaultNumeric).HasColumnType("numeric");
                modelBuilder.Entity<NumericEntity>().Property(e => e.Numeric204).HasColumnType("numeric").HasPrecision(20, 4);
                modelBuilder.Entity<NumericEntity>().Property(e => e.Numeric282).HasColumnType("numeric").HasPrecision(28, 2);

                modelBuilder.Entity<DateTimeEntity>().Property(e => e.DefaultDateTime).HasColumnType("datetime");
                modelBuilder.Entity<DateTimeEntity>().Property(e => e.DefaultDateTime2).HasColumnType("datetime2");
                modelBuilder.Entity<DateTimeEntity>().Property(e => e.DateTime25).HasColumnType("datetime2").HasPrecision(5);
            }
        }

        public class StringEntity
        {
            public int Id { get; set; }
            public string UnicodeFixedLength25 { get; set; }
            public string UnicodeFixedLength35 { get; set; }
            public string UnicodeVariableLength25 { get; set; }
            public string UnicodeVariableLength35 { get; set; }

            public string NonUnicodeFixedLength25 { get; set; }
            public string NonUnicodeFixedLength35 { get; set; }
            public string NonUnicodeVariableLength25 { get; set; }
            public string NonUnicodeVariableLength35 { get; set; }

            public string NullableNonUnicodeVariableLength25 { get; set; }
            public string NullableNonUnicodeVariableLength35 { get; set; }
        }

        public class NumericEntity
        {
            public int Id { get; set; }
            public decimal DefaultDecimal { get; set; }
            public decimal Decimal204 { get; set; }
            public decimal Decimal282 { get; set; }

            public decimal DefaultNumeric { get; set; }
            public decimal Numeric204 { get; set; }
            public decimal Numeric282 { get; set; }
        }

        public class DateTimeEntity
        {
            public int Id { get; set; }
            public DateTime DefaultDateTime { get; set; }
            public DateTime DefaultDateTime2 { get; set; }
            public DateTime DateTime25 { get; set; }
        }
    }
}
