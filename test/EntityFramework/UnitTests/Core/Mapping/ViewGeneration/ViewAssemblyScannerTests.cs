// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Reflection;
    using Moq;
    using Xunit;

    public class ViewAssemblyScannerTests : TestBase
    {
        [Fact]
        public void Collections_passed_to_constructor_are_copied()
        {
            var found = new List<Assembly>
                {
                    typeof(object).Assembly,
                    typeof(DbContext).Assembly
                };

            var visited = new Dictionary<Assembly, bool>
                {
                    { typeof(FactAttribute).Assembly, true },
                    { typeof(object).Assembly, false },
                };

            var scanner = new ViewAssemblyScanner(found, visited, new ViewAssemblyChecker());

            Assert.NotSame(found, scanner.ViewAssemblies);
            Assert.NotSame(visited, scanner.VisitedAssemblies);

            Assert.Equal(found, scanner.ViewAssemblies);
            Assert.Equal(visited, scanner.VisitedAssemblies);
        }

        [Fact]
        public void ScanAssembly_can_scan_only_a_single_assembly()
        {
            var mockChecker = new Mock<ViewAssemblyChecker>();
            var scanner = new ViewAssemblyScanner(new List<Assembly>(), new Dictionary<Assembly, bool>(), mockChecker.Object);

            scanner.ScanAssembly(typeof(DbContext).Assembly, followReferences: false);
            mockChecker.Verify(m => m.IsViewAssembly(typeof(DbContext).Assembly), Times.Once());
            mockChecker.Verify(m => m.IsViewAssembly(It.IsAny<Assembly>()), Times.Once());
        }

        [Fact]
        public void ScanAssembly_can_scan_all_referenced_assemblies()
        {
            var mockChecker = new Mock<ViewAssemblyChecker>();
            var scanner = new ViewAssemblyScanner(new List<Assembly>(), new Dictionary<Assembly, bool>(), mockChecker.Object);

            scanner.ScanAssembly(typeof(DbContext).Assembly, followReferences: true);
            mockChecker.Verify(m => m.IsViewAssembly(typeof(DbContext).Assembly), Times.Once());
            mockChecker.Verify(m => m.IsViewAssembly(typeof(RequiredAttribute).Assembly), Times.Once());
        }

        [Fact]
        public void ScanAssembly_scans_assembly_only_if_it_has_not_already_been_scanned()
        {
            var mockChecker = new Mock<ViewAssemblyChecker>();
            var scanner = new ViewAssemblyScanner(new List<Assembly>(), new Dictionary<Assembly, bool>(), mockChecker.Object);

            scanner.ScanAssembly(typeof(object).Assembly, followReferences: false);
            mockChecker.Verify(m => m.IsViewAssembly(typeof(object).Assembly), Times.Once());

            scanner.ScanAssembly(typeof(object).Assembly, followReferences: false);
            mockChecker.Verify(m => m.IsViewAssembly(typeof(object).Assembly), Times.Once());
        }

        [Fact]
        public void ScanAssembly_processes_assembly_again_if_previously_references_were_not_scanned_but_now_they_should_be()
        {
            var mockChecker = new Mock<ViewAssemblyChecker>();
            var scanner = new ViewAssemblyScanner(new List<Assembly>(), new Dictionary<Assembly, bool>(), mockChecker.Object);

            scanner.ScanAssembly(typeof(DbContext).Assembly, followReferences: false);
            mockChecker.Verify(m => m.IsViewAssembly(typeof(DbContext).Assembly), Times.Once());
            mockChecker.Verify(m => m.IsViewAssembly(typeof(RequiredAttribute).Assembly), Times.Never());

            scanner.ScanAssembly(typeof(DbContext).Assembly, followReferences: true);
            mockChecker.Verify(m => m.IsViewAssembly(typeof(DbContext).Assembly));
            mockChecker.Verify(m => m.IsViewAssembly(typeof(RequiredAttribute).Assembly), Times.Once());
        }

        [Fact]
        public void ScanAssembly_does_not_processes_assembly_again_if_references_were_already_scanned()
        {
            var mockChecker = new Mock<ViewAssemblyChecker>();
            var scanner = new ViewAssemblyScanner(new List<Assembly>(), new Dictionary<Assembly, bool>(), mockChecker.Object);

            scanner.ScanAssembly(typeof(DbContext).Assembly, followReferences: true);
            mockChecker.Verify(m => m.IsViewAssembly(typeof(DbContext).Assembly), Times.Once());
            mockChecker.Verify(m => m.IsViewAssembly(typeof(RequiredAttribute).Assembly), Times.Once());

            scanner.ScanAssembly(typeof(DbContext).Assembly, followReferences: true);
            mockChecker.Verify(m => m.IsViewAssembly(typeof(DbContext).Assembly), Times.Once());
            mockChecker.Verify(m => m.IsViewAssembly(typeof(RequiredAttribute).Assembly), Times.Once());
        }

        [Fact]
        public void ScanAssembly_records_which_assemblies_contain_view_generation_attribute()
        {
            var mockChecker = new Mock<ViewAssemblyChecker>();
            mockChecker.Setup(m => m.IsViewAssembly(typeof(DbContext).Assembly)).Returns(false);
            mockChecker.Setup(m => m.IsViewAssembly(typeof(RequiredAttribute).Assembly)).Returns(true);
            var scanner = new ViewAssemblyScanner(new List<Assembly>(), new Dictionary<Assembly, bool>(), mockChecker.Object);

            scanner.ScanAssembly(typeof(DbContext).Assembly, followReferences: true);

            Assert.True(scanner.ViewAssemblies.Contains(typeof(RequiredAttribute).Assembly));
            Assert.False(scanner.ViewAssemblies.Contains(typeof(DbContext).Assembly));
        }
    }
}
