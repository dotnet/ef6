// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query.LinqToEntities
{
    using System.Collections.Generic;
    using System.Data.Entity.Functionals.Utilities;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.TestModels.ArubaModel;
    using System.Linq;
    using System.Reflection;
    using Xunit;

    public class MaterializationTests : FunctionalTestBase
    {
        [Fact]
        public void Materialize_array_of_entity_properties_throws()
        {
            using (var context = new ArubaContext())
            {
                var query = context.Runs.Where(r => r.Id == 1).Select(r => new int[] { r.Id, r.RunOwner.Id });
                var innerException = Assert.Throws<TargetInvocationException>(() => query.ToList())
                    .InnerException;

                Assert.IsType<InvalidOperationException>(innerException);
                innerException.ValidateMessage(
                          typeof(DbContext).Assembly(),
                          "ObjectQuery_UnableToMaterializeArray",
                          null,
                          "System.Int32[]",
                          "System.Collections.Generic.List`1[System.Int32]");
            }
        }

        [Fact]
        public void Materializing_empty_list_throws()
        {
            using (var context = new ArubaContext())
            {
                var query = context.Runs.Select(r => new List<int> { });
                Assert.Throws<NotSupportedException>(() => query.ToList()).
                       ValidateMessage(
                           typeof(DbContext).Assembly(),
                           "ELinq_UnsupportedEnumerableType",
                           null,
                           "System.Collections.Generic.List`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]");
            }
        }

        [Fact]
        public void Can_materialize_null_complex_type()
        {
            using (var context = new ArubaContext())
            {
                var query = context.Runs.Select(r => r.Tasks.Where(t => t.Id < 0).Select(t => t.TaskInfo).FirstOrDefault());
                var results = query.ToList();
                Assert.IsType<List<ArubaTaskInfo>>(results);
                foreach (var result in results)
                {
                    Assert.Null(result);
                }
            }
        }
    }
}