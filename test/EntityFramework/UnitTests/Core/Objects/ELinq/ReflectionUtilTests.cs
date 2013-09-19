// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.ELinq
{
    using System.Data.Entity.Utilities;
    using System.Linq;
    using Xunit;

    public class ReflectionUtilTests
    {
        [Fact]
        public void MethodMap_contains_all_supported_LINQ_methods()
        {
            Assert.Equal(2, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "AsQueryable"));
            Assert.Equal(4, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "Where"));
            Assert.Equal(2, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "OfType"));
            Assert.Equal(2, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "Cast"));
            Assert.Equal(4, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "Select"));
            Assert.Equal(8, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "SelectMany"));
            Assert.Equal(4, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "Join"));
            Assert.Equal(4, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "GroupJoin"));
            Assert.Equal(4, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "OrderBy"));
            Assert.Equal(4, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "OrderByDescending"));
            Assert.Equal(4, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "ThenBy"));
            Assert.Equal(4, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "ThenByDescending"));
            Assert.Equal(2, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "Take"));
            Assert.Equal(4, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "TakeWhile"));
            Assert.Equal(2, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "Skip"));
            Assert.Equal(4, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "SkipWhile"));
            Assert.Equal(16, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "GroupBy"));
            Assert.Equal(4, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "Distinct"));
            Assert.Equal(2, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "Concat"));
            Assert.Equal(2, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "Zip"));
            Assert.Equal(4, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "Union"));
            Assert.Equal(4, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "Intersect"));
            Assert.Equal(4, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "Except"));
            Assert.Equal(4, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "First"));
            Assert.Equal(4, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "FirstOrDefault"));
            Assert.Equal(4, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "Last"));
            Assert.Equal(4, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "LastOrDefault"));
            Assert.Equal(4, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "Single"));
            Assert.Equal(4, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "SingleOrDefault"));
            Assert.Equal(2, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "ElementAt"));
            Assert.Equal(2, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "ElementAtOrDefault"));
            Assert.Equal(4, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "DefaultIfEmpty"));
            Assert.Equal(4, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "Contains"));
            Assert.Equal(2, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "Reverse"));
            Assert.Equal(4, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "SequenceEqual"));
            Assert.Equal(4, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "Any"));
            Assert.Equal(2, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "All"));
            Assert.Equal(4, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "Count"));
            Assert.Equal(4, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "LongCount"));
            Assert.Equal(24, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "Min"));
            Assert.Equal(24, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "Max"));
            Assert.Equal(40, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "Sum"));
            Assert.Equal(40, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "Average"));
            Assert.Equal(6, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "Aggregate"));
            Assert.Equal(1, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "AsEnumerable"));
            Assert.Equal(1, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "ToArray"));
            Assert.Equal(1, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "ToList"));
            Assert.Equal(4, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "ToDictionary"));
            Assert.Equal(4, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "ToLookup"));
            Assert.Equal(1, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "Range"));
            Assert.Equal(1, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "Repeat"));
            Assert.Equal(1, ReflectionUtil.MethodMap.Keys.Count(k => k.Name == "Empty"));

            Assert.Equal(
                Enum.GetValues(typeof(SequenceMethod)).OfType<SequenceMethod>().OrderBy(e => e),
                ReflectionUtil.MethodMap.Values.Distinct().OrderBy(e => e));

            Assert.Equal(298, ReflectionUtil.MethodMap.Count);
        }

        [Fact]
        public void InverseMap_contains_all_supported_LINQ_methods()
        {
            foreach (var sequenceMethod in Enum.GetValues(typeof(SequenceMethod))
                .OfType<SequenceMethod>()
                .Where(e => e != SequenceMethod.NotSupported))
            {
                Assert.True(sequenceMethod.ToString().StartsWith(ReflectionUtil.InverseMap[sequenceMethod].Name));
            }

            Assert.Equal(
                Enum.GetValues(typeof(SequenceMethod)).OfType<SequenceMethod>().OrderBy(e => e),
                ReflectionUtil.InverseMap.Keys.Distinct().OrderBy(e => e));

            Assert.Equal(167, ReflectionUtil.InverseMap.Count);
        }
    }
}
