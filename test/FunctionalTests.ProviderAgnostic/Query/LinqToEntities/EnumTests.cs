// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query.LinqToEntities
{
    using System.Data.Entity.TestModels.ProviderAgnosticModel;
    using System.IO;
    using System.Linq;
    using Xunit;

    public class EnumTests
    {
        [Fact]
        public void Cast_property_to_enum()
        {
            using (var context = new ProviderAgnosticContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;
                var query = context.AllTypes.Where(a => a.EnumProperty == AllTypesEnum.EnumValue1).Select(a => a.EnumProperty);

                // verify that correct enums are filtered out (verify count and value)
                var results = query.ToList();
                var expectedCount = context.AllTypes.ToList().Count(a => a.EnumProperty == AllTypesEnum.EnumValue1);
                Assert.True(results.All(r => r == AllTypesEnum.EnumValue1));
                Assert.Equal(expectedCount, results.Count);
            }
        }

        [Fact]
        public void Cast_constant_to_enum()
        {
            using (var context = new ProviderAgnosticContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;
                var query = context.AllTypes.Where(a => a.EnumProperty == (AllTypesEnum)1).Select(a => a.EnumProperty);

                // verify that correct enums are filtered out (verify count and value)
                var results = query.ToList();
                var expectedCount = context.AllTypes.ToList().Count(a => a.EnumProperty == (AllTypesEnum)1);
                Assert.True(results.All(r => r == AllTypesEnum.EnumValue1));
                Assert.Equal(expectedCount, results.Count);
            }
        }

        [Fact]
        public void Unnamed_enum_constant_in_Where_clause()
        {
            using (var context = new ProviderAgnosticContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                var query = context.AllTypes.Where(a => a.EnumProperty == (AllTypesEnum)42).Select(p => p.EnumProperty);

                // verify all results are filtered out
                var results = query.ToList();
                Assert.False(results.Any());
            }
        }

        [Fact]
        public void Enum_in_OrderBy_clause()
        {
            using (var context = new ProviderAgnosticContext())
            {
                var query = context.AllTypes.OrderBy(a => a.EnumProperty).Select(a => a.EnumProperty);

                // verify order is correct
                var results = query.ToList();
                var highest = (int)results[0];
                foreach (var result in results)
                {
                    Assert.True((int)result >= highest);
                    highest = (int)result;
                }
            }
        }

        [Fact]
        public void Enum_in_GroupBy_clause()
        {
            using (var context = new ProviderAgnosticContext())
            {
                var query = context.AllTypes.GroupBy(a => a.EnumProperty).Select(
                    g => new
                    {
                        g.Key,
                        Count = g.Count()
                    });

                var results = query.ToList().OrderBy(r => r.Key).ToList();
                var expected = context.AllTypes.ToList().GroupBy(a => a.EnumProperty).Select(
                    g => new
                    {
                        g.Key,
                        Count = g.Count()
                    }).OrderBy(r => r.Key).ToList();
                QueryTestHelpers.VerifyQueryResult(expected, results, (o, i) => o.Key == i.Key && o.Count == i.Count);
            }
        }

        [Fact]
        public void Enum_in_Join_clause()
        {
            using (var context = new ProviderAgnosticContext())
            {
                var query = context.AllTypes.Take(1).Join(
                    context.AllTypes, o => o.EnumProperty, i => i.EnumProperty, (o, i) => new
                        {
                            OuterKey = o.Id,
                            OuterEnum = o.EnumProperty,
                            InnerKey = i.Id,
                            InnerEnum = i.EnumProperty
                        });

                // verify that all entities with matching enum values are joined
                // and that correct enum values get joined
                var firstEnum = context.AllTypes.ToList().Take(1).First().EnumProperty;
                var allTypesWithFirstEnumIds = context.AllTypes.ToList().Where(a => a.EnumProperty == firstEnum).Select(a => a.Id).ToList();
                var results = query.ToList();
                Assert.Equal(allTypesWithFirstEnumIds.Count(), results.Count());
                foreach (var result in results)
                {
                    Assert.Equal(firstEnum, result.InnerEnum);
                    Assert.Equal(firstEnum, result.OuterEnum);
                    Assert.True(allTypesWithFirstEnumIds.Contains(result.OuterKey));
                }
            }
        }

        [Fact]
        public void Enum_with_coalesce_operator()
        {
            using (var context = new ProviderAgnosticContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                var query = context.AllTypes.Select(p => p.Id % 2 == 0 ? p.EnumProperty : (AllTypesEnum?)null)
                                   .Select(a => a ?? AllTypesEnum.EnumValue1);

                var results = query.ToList();
                var expected = context.AllTypes.ToList().Select(p => p.Id % 2 == 0 ? p.EnumProperty : (AllTypesEnum?)null)
                                      .Select(a => a ?? AllTypesEnum.EnumValue1).ToList();

                Assert.Equal(expected.Count, results.Count);
                for (var i = 0; i < results.Count; i++)
                {
                    Assert.Equal(expected[i], results[i]);
                }
            }
        }

        [Fact]
        public void Casting_to_enum_undeclared_in_model_works()
        {
            using (var context = new ProviderAgnosticContext())
            {
                var query = context.AllTypes.Select(a => (FileAccess)a.Id);

                // not much to verify here, just make sure we don't throw
                query.ToList();
            }
        }
    }
}
