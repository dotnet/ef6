// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if NET452

namespace System.Data.Entity.ConnectionFactoryConfig
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using EnvDTE;
    using Moq;
    using VSLangProj;
    using Xunit;

    public class ReferenceRemoverTests : TestBase
    {
        [Fact]
        public void TryRemoveReference_finds_and_removes_only_given_strongly_named_assembly_reference()
        {
            var toRemove = CreateMockReference("Unicorns.Rule", "Pk1");
            var references = new List<Mock<Reference>>
                {
                    CreateMockReference("Unicorns.Rule", "PK1", isStrongNamed: false),
                    CreateMockReference("Unicorns.Rule", "PK2"),
                    CreateMockReference("Horses.ForTheWin", "PK1"),
                    CreateMockReference(null, null),
                    toRemove,
                };

            new ReferenceRemover(CreateMockProject(references.Select(r => r.Object)).Object).TryRemoveReference("Unicorns.Rule", "PK1");

            foreach (var reference in references)
            {
                reference.Verify(m => m.Remove(), reference == toRemove ? Times.Once() : Times.Never());
            }
        }

        [Fact]
        public void TryRemoveReference_is_resilient_to_non_VS_projects()
        {
            var mockNonVsProject = new Mock<Random>();

            var mockProject = new Mock<Project>();
            mockProject.Setup(m => m.Object).Returns(mockNonVsProject.Object);

            new ReferenceRemover(mockProject.Object).TryRemoveReference("Unicorns.Rule", "PK1");
        }

        [Fact]
        public void TryRemoveReference_is_resilient_to_non_references_in_the_reference_collection()
        {
            var references = new List<Random>
                {
                    new Random()
                };

            new ReferenceRemover(CreateMockProject(references).Object).TryRemoveReference("Unicorns.Rule", "PK1");
        }

        [Fact]
        public void TryRemoveSystemDataEntity_finds_and_removes_System_Data_Entity()
        {
            var toRemove = CreateMockReference("System.Data.Entity", "B77A5C561934E089");
            var references = new List<Mock<Reference>>
                {
                    CreateMockReference("EntityFramework", "B77A5C561934E089"),
                    toRemove,
                    CreateMockReference("System.Data", "B77A5C561934E089"),
                };

            new ReferenceRemover(CreateMockProject(references.Select(r => r.Object)).Object).TryRemoveSystemDataEntity();

            foreach (var reference in references)
            {
                reference.Verify(m => m.Remove(), reference == toRemove ? Times.Once() : Times.Never());
            }
        }

        private static Mock<Project> CreateMockProject(IEnumerable references)
        {
            var mockReferences = new Mock<References>();
            mockReferences.As<IEnumerable>().Setup(p => p.GetEnumerator()).Returns(references.GetEnumerator());

            var mockVsProject = new Mock<VSProject>();
            mockVsProject.Setup(m => m.References).Returns(mockReferences.Object);

            var mockProject = new Mock<Project>();
            mockProject.Setup(m => m.Object).Returns(mockVsProject.Object);
            return mockProject;
        }

        private static Mock<Reference> CreateMockReference(string identity, string publicKeyToken, bool isStrongNamed = true)
        {
            var mockReference = new Mock<Reference>();
            mockReference.Setup(m => m.Identity).Returns(identity);
            mockReference.Setup(m => m.PublicKeyToken).Returns(publicKeyToken);
            mockReference.Setup(m => m.StrongName).Returns(isStrongNamed);
            return mockReference;
        }
    }
}

#endif
