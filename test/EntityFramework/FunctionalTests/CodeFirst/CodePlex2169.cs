// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.CodeFirst
{
    using Xunit;

    public class CodePlex2169 : TestBase
    {
        public class VagueItem
        {
            public int KeyProperty1 { get; set; }
            public int KeyProperty2 { get; set; }
            public int KeyProperty3 { get; set; }
            public int KeyProperty4 { get; set; }
            public int KeyProperty5 { get; set; }
            public int KeyProperty6 { get; set; }
            public int KeyProperty7 { get; set; }
            public int KeyProperty8 { get; set; }
            public int KeyProperty9 { get; set; }
            public int KeyProperty10 { get; set; }
            public int NonKeyProperty1 { get; set; }
        }

        public class VagueContext : DbContext
        {
            static VagueContext()
            {
                Database.SetInitializer<VagueContext>(null);
            }

            public DbSet<VagueItem> VagueItems { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                var keyOrder = 0;

                modelBuilder.Properties()
                    .Where(info => info.Name.StartsWith("KeyProperty"))
                    .Configure(config => config.IsKey().HasColumnOrder(keyOrder++));

                modelBuilder.Entity<VagueItem>()
                    .Property(x => x.NonKeyProperty1)
                    .HasColumnName("Custom");

            }
        }

        [Fact]
        public void Can_rename_non_key_column_if_key_column_count_is_greater_than_MetadataCollection_dictionary_threshold()
        {
            Assert.DoesNotThrow(
                () =>
                {
                    using (var context = new VagueContext())
                    {
                        Assert.NotNull(context.VagueItems.ToString());
                    }
                });
        }
    }
}
