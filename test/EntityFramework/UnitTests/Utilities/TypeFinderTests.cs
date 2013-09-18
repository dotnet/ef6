// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Linq;
    using System.Reflection;
    using Xunit;

    public class TypeFinderTests : TestBase
    {
        [Fact]
        public void FindType_returns_type_with_given_name_if_specified()
        {
            Assert.Same(
                typeof(FindMe),
                new TypeFinder(GetType().Assembly()).FindType(typeof(ABase), typeof(FindMe).FullName, t => t));
        }

        [Fact]
        public void FindType_returns_single_ignore_case_matching_type_if_specified_type_name_is_not_found_directly()
        {
            Assert.Same(
                typeof(FindMe),
                new TypeFinder(GetType().Assembly()).FindType(typeof(ABase), typeof(FindMe).Name.ToLowerInvariant(), t => t));
        }

        public class ABase
        {
        }

        public class FindMe : ABase
        {
        }

        public class OuterForFilter
        {
            public class FindMe // Won't be found because doesn't inherit from ABase
            {
            }
        }

        [Fact]
        public void FindType_returns_single_case_matching_type_if_specified_type_name_is_not_found_directly()
        {
            Assert.Same(
                typeof(FindMeWhatWHAT),
                new TypeFinder(GetType().Assembly()).FindType(typeof(ABase), typeof(FindMeWhatWHAT).Name, t => t));
        }

        public class FindMeWhatWhat : ABase
        {
        }

        public class FindMeWhatWHAT : ABase
        {
        }

        [Fact]
        public void FindType_can_throw_if_no_type_matching_name_is_found()
        {
            Assert.Equal(
                "EntityFramework.UnitTests Bad_Type_Name",
                Assert.Throws<InvalidOperationException>(
                    () => new TypeFinder(GetType().Assembly()).FindType(
                        typeof(ABase),
                        "Bad_Type_Name",
                        t => t,
                        noTypeWithName: (t, a) => new InvalidOperationException(a + " " + t))).Message);
        }

        [Fact]
        public void FindType_can_return_null_if_no_type_matching_name_is_found()
        {
            Assert.Null(new TypeFinder(GetType().Assembly()).FindType(typeof(ABase), "Bad_Type_Name", t => t));
        }

        [Fact]
        public void FindType_can_throw_if_multiple_types_matching_name_are_found()
        {
            Assert.Equal(
                "EntityFramework.UnitTests MultipleMe",
                Assert.Throws<InvalidOperationException>(
                    () => new TypeFinder(GetType().Assembly()).FindType(
                        typeof(ABase),
                        typeof(MultipleMe).Name,
                        t => t,
                        multipleTypesWithName: (t, a) => new InvalidOperationException(a + " " + t))).Message);
        }

        [Fact]
        public void FindType_can_return_null_if_multiple_types_matching_name_are_found()
        {
            Assert.Null(new TypeFinder(GetType().Assembly()).FindType(typeof(ABase), typeof(MultipleMe).Name, t => t));
        }

        public class MultipleMe : ABase
        {
        }

        public class OuterForName
        {
            public class MultipleMe : ABase
            {
            }
        }

        [Fact]
        public void FindType_returns_single_matching_type_if_no_type_name_is_specified()
        {
            Assert.Same(
                typeof(CanYouFindMe),
                new TypeFinder(GetType().Assembly()).FindType(
                    typeof(ABase),
                    null,
                    t => t.Where(n => n.GetRuntimeProperties().Any(p => p.Name == "DiscoverMe"))));
        }

        public class CanYouFindMe : ABase
        {
            public object DiscoverMe { get; set; }
        }

        [Fact]
        public void FindType_can_throw_if_filter_returns_no_types()
        {
            Assert.Equal(
                "EntityFramework.UnitTests",
                Assert.Throws<InvalidOperationException>(
                    () => new TypeFinder(GetType().Assembly()).FindType(
                        typeof(ABase),
                        null,
                        t => t.Where(n => n.GetRuntimeProperties().Any(p => p.Name == "DontDiscoverMe")),
                        noType: a => new InvalidOperationException(a))).Message);
        }

        [Fact]
        public void FindType_can_throw_if_filter_returns_many_types()
        {
            Assert.Null(
                new TypeFinder(GetType().Assembly()).FindType(
                    typeof(ABase),
                    null,
                    t => t.Where(n => n.GetRuntimeProperties().Any(p => p.Name == "DontDiscoverMe"))));
        }

        [Fact]
        public void FindType_can_return_null_if_filter_returns_no_types()
        {
            Assert.Equal(
                "CanYouFindMeTwo CanYouFindMeThree EntityFramework.UnitTests",
                Assert.Throws<InvalidOperationException>(
                    () => new TypeFinder(GetType().Assembly()).FindType(
                        typeof(ABase),
                        null,
                        t => t.Where(n => n.GetRuntimeProperties().Any(p => p.Name == "DiscoverMeMeMe")),
                        multipleTypes: (a, t) => new InvalidOperationException(t.First().Name + " " + t.Skip(1).First().Name + " " + a)))
                      .Message);
        }

        [Fact]
        public void FindType_can_return_null_if_filter_returns_many_types()
        {
            Assert.Null(
                new TypeFinder(GetType().Assembly()).FindType(
                    typeof(ABase),
                    null,
                    t => t.Where(n => n.GetRuntimeProperties().Any(p => p.Name == "DiscoverMeMeMe"))));
        }

        public class CanYouFindMeTwo : ABase
        {
            public object DiscoverMeMeMe { get; set; }
        }

        public class CanYouFindMeThree : ABase
        {
            public object DiscoverMeMeMe { get; set; }
        }
    }
}
