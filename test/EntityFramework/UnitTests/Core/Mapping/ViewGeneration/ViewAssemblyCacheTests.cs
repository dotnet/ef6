// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.ViewGeneration;
    using System.Linq;
    using System.Reflection;
    using Moq;
    using Xunit;

    public class ViewAssemblyCacheTests : TestBase
    {
        [Fact]
        public void CheckAssembly_can_scan_only_a_single_assembly()
        {
            var mockChecker = new Mock<ViewAssemblyChecker>();
            mockChecker.Setup(m => m.IsViewAssembly(typeof(DbContext).Assembly)).Returns(true);
            mockChecker.Setup(m => m.IsViewAssembly(typeof(RequiredAttribute).Assembly)).Returns(true);

            var cache = new ViewAssemblyCache(mockChecker.Object);

            cache.CheckAssembly(typeof(DbContext).Assembly, followReferences: false);
            Assert.Equal(new[] { typeof(DbContext).Assembly }, cache.Assemblies);
        }

        [Fact]
        public void CheckAssembly_can_scan_all_referenced_assemblies()
        {
            var mockChecker = new Mock<ViewAssemblyChecker>();
            mockChecker.Setup(m => m.IsViewAssembly(typeof(DbContext).Assembly)).Returns(true);
            mockChecker.Setup(m => m.IsViewAssembly(typeof(RequiredAttribute).Assembly)).Returns(true);

            var cache = new ViewAssemblyCache(mockChecker.Object);

            cache.CheckAssembly(typeof(DbContext).Assembly, followReferences: true);
            Assert.Equal(new[] { typeof(DbContext).Assembly, typeof(RequiredAttribute).Assembly }, cache.Assemblies);
        }

        [Fact]
        public void CheckAssembly_scans_assembly_only_if_it_has_not_already_been_scanned()
        {
            var mockChecker = new Mock<ViewAssemblyChecker>();
            var cache = new ViewAssemblyCache(mockChecker.Object);

            cache.CheckAssembly(typeof(object).Assembly, followReferences: false);
            mockChecker.Verify(m => m.IsViewAssembly(typeof(object).Assembly), Times.Once());

            cache.CheckAssembly(typeof(object).Assembly, followReferences: false);
            mockChecker.Verify(m => m.IsViewAssembly(typeof(object).Assembly), Times.Once());
        }

        [Fact]
        public void CheckAssembly_processes_assembly_again_if_previously_references_were_not_scanned_but_now_they_should_be()
        {
            var mockChecker = new Mock<ViewAssemblyChecker>();
            var cache = new ViewAssemblyCache(mockChecker.Object);

            cache.CheckAssembly(typeof(DbContext).Assembly, followReferences: false);
            mockChecker.Verify(m => m.IsViewAssembly(typeof(DbContext).Assembly), Times.Once());
            mockChecker.Verify(m => m.IsViewAssembly(typeof(RequiredAttribute).Assembly), Times.Never());

            cache.CheckAssembly(typeof(DbContext).Assembly, followReferences: true);
            mockChecker.Verify(m => m.IsViewAssembly(typeof(DbContext).Assembly));
            mockChecker.Verify(m => m.IsViewAssembly(typeof(RequiredAttribute).Assembly), Times.Once());
        }

        [Fact]
        public void CheckAssembly_does_not_processes_assembly_again_if_references_were_already_scanned()
        {
            var mockChecker = new Mock<ViewAssemblyChecker>();
            var cache = new ViewAssemblyCache(mockChecker.Object);

            cache.CheckAssembly(typeof(DbContext).Assembly, followReferences: true);
            mockChecker.Verify(m => m.IsViewAssembly(typeof(DbContext).Assembly), Times.Once());
            mockChecker.Verify(m => m.IsViewAssembly(typeof(RequiredAttribute).Assembly), Times.Once());

            cache.CheckAssembly(typeof(DbContext).Assembly, followReferences: true);
            mockChecker.Verify(m => m.IsViewAssembly(typeof(DbContext).Assembly), Times.Once());
            mockChecker.Verify(m => m.IsViewAssembly(typeof(RequiredAttribute).Assembly), Times.Once());
        }

        [Fact]
        public void Clear_clears_all_cached_assembly_information()
        {
            var mockChecker = new Mock<ViewAssemblyChecker>();
            mockChecker.Setup(m => m.IsViewAssembly(typeof(object).Assembly)).Returns(true);
            var cache = new ViewAssemblyCache(mockChecker.Object);

            cache.CheckAssembly(typeof(object).Assembly, followReferences: false);
            mockChecker.Verify(m => m.IsViewAssembly(typeof(object).Assembly), Times.Once());
            Assert.Equal(new[] { typeof(object).Assembly }, cache.Assemblies);

            cache.Clear();
            Assert.Equal(Enumerable.Empty<Assembly>(), cache.Assemblies);

            cache.CheckAssembly(typeof(object).Assembly, followReferences: false);
            mockChecker.Verify(m => m.IsViewAssembly(typeof(object).Assembly), Times.Exactly(2));
            Assert.Equal(new[] { typeof(object).Assembly }, cache.Assemblies);
        }

        /// <summary>
        ///     This test makes calls from multiple threads such that we have at least some chance of finding threading
        ///     issues. As with any test of this type just because the test passes does not mean that the code is
        ///     correct. On the other hand if this test ever fails (EVEN ONCE) then we know there is a problem to
        ///     be investigated. DON'T just re-run and think things are okay if the test then passes.
        /// </summary>
        [Fact]
        public void CheckAssembly_can_be_called_from_multiple_threads_concurrently()
        {
            var expectedAssemblies = new[] { typeof(PregenContextEdmxViews).Assembly };
            var cache = new ViewAssemblyCache();
            ExecuteInParallel(
                () =>
                    {
                        cache.CheckAssembly(typeof(PregenContextEdmxViews).Assembly, followReferences: true);
                        Assert.Equal(expectedAssemblies, cache.Assemblies);
                    });
        }

        /// <summary>
        ///     This test makes calls from multiple threads such that we have at least some chance of finding threading
        ///     issues. As with any test of this type just because the test passes does not mean that the code is
        ///     correct. On the other hand if this test ever fails (EVEN ONCE) then we know there is a problem to
        ///     be investigated. DON'T just re-run and think things are okay if the test then passes.
        /// </summary>
        [Fact]
        public void CheckAssembly_for_different_assemblies_can_be_called_from_multiple_threads_concurrently()
        {
            var expectedAssemblies = new[] { typeof(PregenContextEdmxViews).Assembly };
            var cache = new ViewAssemblyCache();
            ExecuteInParallel(
                () =>
                    {
                        cache.CheckAssembly(typeof(RequiredAttribute).Assembly, followReferences: true);
                        cache.CheckAssembly(typeof(FactAttribute).Assembly, followReferences: true);
                        cache.CheckAssembly(typeof(PregenContextEdmxViews).Assembly, followReferences: true);
                        Assert.Equal(expectedAssemblies, cache.Assemblies);
                    });
        }

        /// <summary>
        ///     This test makes calls from multiple threads such that we have at least some chance of finding threading
        ///     issues. As with any test of this type just because the test passes does not mean that the code is
        ///     correct. On the other hand if this test ever fails (EVEN ONCE) then we know there is a problem to
        ///     be investigated. DON'T just re-run and think things are okay if the test then passes.
        /// </summary>
        [Fact]
        public void Clear_can_be_called_from_multiple_threads_concurrently()
        {
            var cache = new ViewAssemblyCache();
            cache.CheckAssembly(typeof(PregenContextEdmxViews).Assembly, followReferences: true);
            ExecuteInParallel(
                () =>
                    {
                        cache.Clear();
                        Assert.Equal(Enumerable.Empty<Assembly>(), cache.Assemblies);
                    });
        }

        /// <summary>
        ///     This test makes calls from multiple threads such that we have at least some chance of finding threading
        ///     issues. As with any test of this type just because the test passes does not mean that the code is
        ///     correct. On the other hand if this test ever fails (EVEN ONCE) then we know there is a problem to
        ///     be investigated. DON'T just re-run and think things are okay if the test then passes.
        /// </summary>
        [Fact]
        public void Assemblies_can_be_called_from_multiple_threads_concurrently()
        {
            var expectedAssemblies = new[] { typeof(PregenContextEdmxViews).Assembly };
            var cache = new ViewAssemblyCache();
            cache.CheckAssembly(typeof(PregenContextEdmxViews).Assembly, followReferences: true);
            ExecuteInParallel(() => Assert.Equal(expectedAssemblies, cache.Assemblies));
        }
    }
}
