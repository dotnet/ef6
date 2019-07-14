
namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Collections;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.CodeGeneration.Generators;
    using Microsoft.Data.Entity.Design.Common;
    using Moq;
    using System;
    using System.Linq;
    using UnitTests.TestHelpers;
    using VSLangProj;
    using Xunit;

    public class CodeGeneratorFactoryTests
    {
        [Fact]
        public void GetContextGenerator_returns_correct_context_generator_for_empty_model()
        {
            var generatorFactory = new CodeGeneratorFactory(Mock.Of<Project>());

            Assert.IsType<CSharpCodeFirstEmptyModelGenerator>(
                generatorFactory.GetContextGenerator(LangEnum.CSharp, isEmptyModel: true));
            Assert.IsType<VBCodeFirstEmptyModelGenerator>(
                generatorFactory.GetContextGenerator(LangEnum.VisualBasic, isEmptyModel: true));
        }

        [Fact]
        public void GetContextGenerator_returns_correct_non_customized_context_generator_if_model_not_empty()
        {
            var generatorFactory = new CodeGeneratorFactory(Mock.Of<Project>());

            Assert.IsType<DefaultCSharpContextGenerator>(
                generatorFactory.GetContextGenerator(LangEnum.CSharp, isEmptyModel: false));
            Assert.IsType<DefaultVBContextGenerator>(
                generatorFactory.GetContextGenerator(LangEnum.VisualBasic, isEmptyModel: false));
        }

        [Fact]
        public void GetContextGenerator_returns_correct_customized__context_generator_if_model_not_empty_CS()
        {
            var mockDte = SetupMockProjectWithCustomizedTemplate(@"CodeTemplates\EFModelFromDatabase\Context.cs.t4");

            var generatorFactory = new CodeGeneratorFactory(mockDte.Project);

            Assert.IsType<CustomGenerator>(generatorFactory.GetContextGenerator(LangEnum.CSharp, isEmptyModel: false));
        }

        [Fact]
        public void GetContextGenerator_returns_correct_customized_context_generator_if_model_not_empty_VB()
        {
            var mockDte = SetupMockProjectWithCustomizedTemplate(@"CodeTemplates\EFModelFromDatabase\Context.vb.t4");

            var generatorFactory = new CodeGeneratorFactory(mockDte.Project);

            Assert.IsType<CustomGenerator>(generatorFactory.GetContextGenerator(LangEnum.VisualBasic, isEmptyModel: false));
        }

        [Fact]
        public void GetEntityTypeGenerator_returns_correct_non_customized_entity_type_generator()
        {
            var generatorFactory = new CodeGeneratorFactory(Mock.Of<Project>());

            Assert.IsType<DefaultCSharpEntityTypeGenerator>(
                generatorFactory.GetEntityTypeGenerator(LangEnum.CSharp));
            Assert.IsType<DefaultVBEntityTypeGenerator>(
                generatorFactory.GetEntityTypeGenerator(LangEnum.VisualBasic));
        }

        [Fact]
        public void GetEntityTypeGenerator_returns_correct_customized_entity_type_generator_CS()
        {
            var mockDte = SetupMockProjectWithCustomizedTemplate(@"CodeTemplates\EFModelFromDatabase\Entity.cs.t4");

            var generatorFactory = new CodeGeneratorFactory(mockDte.Project);

            Assert.IsType<CustomGenerator>(generatorFactory.GetEntityTypeGenerator(LangEnum.CSharp));
        }

        [Fact]
        public void GetEntityTypeGenerator_returns_correct_customized_entity_type_generator_VB()
        {
            var mockDte = SetupMockProjectWithCustomizedTemplate(@"CodeTemplates\EFModelFromDatabase\Entity.vb.t4");

            var generatorFactory = new CodeGeneratorFactory(mockDte.Project);

            Assert.IsType<CustomGenerator>(generatorFactory.GetEntityTypeGenerator(LangEnum.CSharp));
        }

        private static MockDTE SetupMockProjectWithCustomizedTemplate(string templatePath)
        {
            Mock<ProjectItem> mockChildProjectItem = null;

            foreach (var step in templatePath.Split('\\').Reverse())
            {
                var mockProjectItem = MockDTE.CreateProjectItem(step);

                var mockProperty = new Mock<Property>();
                mockProperty.Setup(p => p.Name).Returns("FullPath");
                mockProperty.Setup(p => p.Value).Returns(step);

                var mockProperties = new Mock<Properties>();
                mockProperties.As<IEnumerable>()
                    .Setup(p => p.GetEnumerator())
                    .Returns(() => new[] { mockProperty.Object }.GetEnumerator());

                mockProjectItem.
                    Setup(i => i.Properties)
                    .Returns(mockProperties.Object);
                    
                mockProjectItem
                    .Setup(p => p.ProjectItems)
                    .Returns(CreateProjectItems(mockChildProjectItem == null ? null : mockChildProjectItem.Object));

                mockChildProjectItem = mockProjectItem;
            }

            var mockDte = new MockDTE(".NETFramework, Version=v4.5", references: new Reference[0]);

            mockChildProjectItem
                .SetupGet(i => i.ContainingProject)
                .Returns(mockDte.Project);

            Mock.Get(mockDte.Project)
                .Setup(p => p.ProjectItems)
                .Returns(CreateProjectItems(mockChildProjectItem.Object));

            return mockDte;
        }

        private static ProjectItems CreateProjectItems(ProjectItem childProjectItem)
        {
            var mockProjectItems = new Mock<ProjectItems>();

            if (childProjectItem != null)
            {
                mockProjectItems
                    .Setup(p => p.Item(It.IsAny<object>()))
                    .Returns(childProjectItem);
            }

            return mockProjectItems.Object;
        }

    }
}
