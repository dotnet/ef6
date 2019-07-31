// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Spatial
{
    using System.Data.Entity.Hierarchy;
    using Moq;
    using Xunit;

    public class HierarchyIdUnitTests
    {
        [Fact]
        public void Check_hierarchyid_functions()
        {
// ReSharper disable ConditionIsAlwaysTrueOrFalse
            Assert.Equal(null == new HierarchyId(null), true);
            Assert.Equal(null != new HierarchyId(null), false);
// ReSharper restore ConditionIsAlwaysTrueOrFalse
            Assert.Equal(new HierarchyId("/1/2/") == new HierarchyId("/1/2" + "/"), true);
            Assert.Equal(new HierarchyId("/-1/2.1/") != new HierarchyId("/-1/2.1" + "/"), false);
            Assert.Equal(new HierarchyId("/1/2.1/").GetAncestor(1), new HierarchyId("/1/"));
            Assert.Equal(new HierarchyId("/1/2.1/3/").GetAncestor(2), new HierarchyId("/1/"));
            Assert.Equal(new HierarchyId("/1/").GetAncestor(2) == null, true);
            Assert.Equal(new HierarchyId("/1/").GetAncestor(2) == new HierarchyId(null), true);
            Assert.Equal(new HierarchyId("/1/").GetAncestor(1), new HierarchyId("/"));
            Assert.Equal(new HierarchyId("/1/").IsDescendantOf(null), true);
            Assert.Equal(new HierarchyId("/1/").IsDescendantOf(new HierarchyId(null)), true);
            Assert.Equal(new HierarchyId("/1/2/").IsDescendantOf(new HierarchyId("/")), true);
            Assert.Equal(new HierarchyId("/1/2/").IsDescendantOf(new HierarchyId("/1/")), true);
            Assert.Equal(new HierarchyId("/1/2/").IsDescendantOf(new HierarchyId("/1/2/")), true);
            Assert.Equal(new HierarchyId("/1/2/").IsDescendantOf(new HierarchyId("/1/2/3/")), false);
            Assert.Equal(new HierarchyId("/1/2/").IsDescendantOf(new HierarchyId("/1/3/")), false);
            Assert.Equal(new HierarchyId("/1/2/").GetReparentedValue(null, null) == null, true);
            Assert.Equal(new HierarchyId("/1/2/").GetReparentedValue(new HierarchyId("/1/3/"), null) == null, true);
            Assert.Equal(new HierarchyId("/1/2/").GetReparentedValue(null, new HierarchyId("/1/3/")) == null, true);
            Assert.Throws<ArgumentException>(
                () => new HierarchyId("/1/2/").GetReparentedValue(new HierarchyId("/1/3/"), new HierarchyId("/1/4/")));
            Assert.Equal(
                new HierarchyId("/1/2/3/").GetReparentedValue(new HierarchyId("/1/2/"), new HierarchyId("/1/3/"))
                == new HierarchyId("/1/3/3/"), true);
            Assert.Equal(new HierarchyId(null).GetDescendant(null, null) == null, true);
            Assert.Equal(new HierarchyId(null).GetDescendant(new HierarchyId("/1/1/"), null) == null, true);
            Assert.Equal(new HierarchyId(null).GetDescendant(new HierarchyId("/1/2/"), new HierarchyId("/1/3/")) == null, true);
            Assert.Throws<ArgumentException>(
                () => new HierarchyId("/1/2/").GetDescendant(new HierarchyId("/1/"), new HierarchyId("/2/")));
            Assert.Throws<ArgumentException>(
                () => new HierarchyId("/1/2/").GetDescendant(new HierarchyId("/1/2/"), new HierarchyId("/2/")));
            Assert.Throws<ArgumentException>(
                () => new HierarchyId("/1/2/").GetDescendant(new HierarchyId("/1/2/3/"), new HierarchyId("/2/")));
            Assert.Throws<ArgumentException>(
                () => new HierarchyId("/1/2/").GetDescendant(new HierarchyId("/1/2/3/"), new HierarchyId("/1/2/4/1/")));
            Assert.Throws<ArgumentException>(
                () => new HierarchyId("/1/2/").GetDescendant(new HierarchyId("/1/2/1/"), new HierarchyId("/1/2/1/")));
            Assert.Throws<ArgumentException>(
                () => new HierarchyId("/1/2/").GetDescendant(new HierarchyId("/1/2/2/"), new HierarchyId("/1/2/1/")));
            Assert.Equal(new HierarchyId("/1/2/").GetDescendant(new HierarchyId("/1/2/1/"), null), new HierarchyId("/1/2/2/"));
            Assert.Equal(new HierarchyId("/1/2/").GetDescendant(null, new HierarchyId("/1/2/1/")), new HierarchyId("/1/2/0/"));
            Assert.Equal(new HierarchyId("/1/2/").GetDescendant(null, new HierarchyId("/1/2/0/")), new HierarchyId("/1/2/-1/"));
            Assert.Equal(
                new HierarchyId("/1/2/").GetDescendant(new HierarchyId("/1/2/1/"), new HierarchyId("/1/2/3/")), new HierarchyId("/1/2/2/"));
            Assert.Equal(
                new HierarchyId("/1/2/").GetDescendant(new HierarchyId("/1/2/1/"), new HierarchyId("/1/2/2/")), new HierarchyId("/1/2/1.1/"));
            Assert.Equal(
                new HierarchyId("/1/2/").GetDescendant(new HierarchyId("/1/2/1/"), new HierarchyId("/1/2/2.1/")),
                new HierarchyId("/1/2/1.1/"));
            Assert.Equal(
                new HierarchyId("/1/2/").GetDescendant(new HierarchyId("/1/2/1.1.1/"), new HierarchyId("/1/2/1.3.1/")),
                new HierarchyId("/1/2/1.2/"));
        }
    }
}
