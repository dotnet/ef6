// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System;
    using System.Collections;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.IO;
    using System.Linq;
    using EnvDTE;
    using Microsoft.VisualStudio.TextTemplating;
    using Microsoft.VisualStudio.TextTemplating.VSHost;
    using Moq;
    using UnitTests.TestHelpers;
    using Xunit;
    using Resources = Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties.Resources;

    public class CodeFirstModelGeneratorTests
    {
        private static DbModel _model;

        private static DbModel Model
        {
            get
            {
                if (_model == null)
                {
                    var modelBuilder = new DbModelBuilder();
                    modelBuilder.Entity<Entity>();
                    _model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
                }

                return _model;
            }
        }

        [Fact]
        public void Generate_returns_code_when_cs()
        {
            var generator = new CodeFirstModelGenerator(MockDTE.CreateProject(), Mock.Of<IServiceProvider>());

            var files = generator.Generate(Model, "WebApplication1.Models", "MyContext", "MyContextConnString").ToArray();

            Assert.Equal(2, files.Length);
            Assert.Equal(new[] { "MyContext.cs", "Entity.cs" }, files.Select(p => p.Key));
        }

        [Fact]
        public void Generate_returns_code_when_cs_and_empty_model()
        {
            var generator = new CodeFirstModelGenerator(MockDTE.CreateProject(), Mock.Of<IServiceProvider>());

            var files = generator.Generate(null, "WebApplication1.Models", "MyContext", "MyContextConnString").ToArray();

            Assert.Equal(1, files.Length);
            Assert.Equal("MyContext.cs", files[0].Key);
            Assert.Contains(Resources.CodeFirstCodeFile_DbSetComment_CS, files[0].Value);
        }

        [Fact]
        public void Generate_returns_code_when_vb()
        {
            var generator = new CodeFirstModelGenerator(
                MockDTE.CreateProject(kind: MockDTE.VBProjectKind),
                Mock.Of<IServiceProvider>());

            var files = generator.Generate(Model, "WebApplication1.Models", "MyContext", "MyContextConnString").ToArray();

            Assert.Equal(2, files.Length);
            Assert.Equal(new[] { "MyContext.vb", "Entity.vb" }, files.Select(p => p.Key));
        }

        [Fact]
        public void Generate_returns_code_when_vb_and_empty_model()
        {
            var generator = new CodeFirstModelGenerator(
                MockDTE.CreateProject(kind: MockDTE.VBProjectKind),
                Mock.Of<IServiceProvider>());

            var files = generator.Generate(null, "WebApplication1.Models", "MyContext", "MyContextConnString").ToArray();

            Assert.Equal(1, files.Length);
            Assert.Equal("MyContext.vb", files[0].Key);
            Assert.Contains(Resources.CodeFirstCodeFile_DbSetComment_VB, files[0].Value);
        }

        [Fact]
        public void Generate_returns_code_when_cs_and_customized()
        {
            var project = MockDTE.CreateProject();
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(p => p.GetService(typeof(STextTemplating))).Returns(GetTextTemplatingService());

            using (AddCustomizedTemplates(project))
            {
                var generator = new CodeFirstModelGenerator(project, serviceProvider.Object);

                var files = generator.Generate(Model, "WebApplication1.Models", "MyContext", "MyContextConnString").ToArray();

                Assert.Equal(2, files.Length);
                Assert.Equal(new[] { "MyContext.cs", "Entity.cs" }, files.Select(p => p.Key));
                Assert.True(files.All(p => p.Value == "Customized"));
            }
        }

        [Fact]
        public void Generate_returns_code_when_vb_and_customized()
        {
            var project = MockDTE.CreateProject(kind: MockDTE.VBProjectKind);
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(p => p.GetService(typeof(STextTemplating))).Returns(GetTextTemplatingService());

            using (AddCustomizedTemplates(project))
            {
                var generator = new CodeFirstModelGenerator(project, serviceProvider.Object);

                var files = generator.Generate(Model, "WebApplication1.Models", "MyContext", "MyContextConnString").ToArray();

                Assert.Equal(2, files.Length);
                Assert.Equal(new[] { "MyContext.vb", "Entity.vb" }, files.Select(p => p.Key));
                Assert.True(files.All(p => p.Value == "Customized"));
            }
        }

        [Fact]
        public void Generate_throws_when_error_in_context_template()
        {
            var project = MockDTE.CreateProject();
            var exception = new Exception();
            var textTemplatingService = GetTextTemplatingService();
            Mock.Get(textTemplatingService)
                .Setup(s => s.ProcessTemplate(It.IsAny<string>(), It.IsAny<string>(), null, null))
                .Throws(exception);
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(p => p.GetService(typeof(STextTemplating))).Returns(textTemplatingService);

            using (AddCustomizedTemplates(project))
            {
                var generator = new CodeFirstModelGenerator(project, serviceProvider.Object);

                var ex = Assert.Throws<CodeFirstModelGenerationException>(
                    () => generator.Generate(Model, "WebApplication1.Models", "MyContext", "MyContextConnString").ToArray());

                Assert.Equal(
                    string.Format(Resources.ErrorGeneratingCodeFirstModel, "MyContext.cs"),
                    ex.Message);
                Assert.Same(exception, ex.InnerException);
            }
        }

        [Fact]
        public void Generate_throws_when_error_in_entity_type_template()
        {
            var project = MockDTE.CreateProject();
            var i = 0;
            var exception = new Exception();
            var textTemplatingService = GetTextTemplatingService();
            Mock.Get(textTemplatingService)
                .Setup(s => s.ProcessTemplate(It.IsAny<string>(), It.IsAny<string>(), null, null))
                .Callback(() => { if (i++ > 0) throw exception; });
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(p => p.GetService(typeof(STextTemplating))).Returns(textTemplatingService);

            using (AddCustomizedTemplates(project))
            {
                var generator = new CodeFirstModelGenerator(project, serviceProvider.Object);

                var ex = Assert.Throws<CodeFirstModelGenerationException>(
                    () => generator.Generate(Model, "WebApplication1.Models", "MyContext", "MyContextConnString").ToArray());

                Assert.Equal(
                    string.Format(Resources.ErrorGeneratingCodeFirstModel, "Entity.cs"),
                    ex.Message);
                Assert.Same(exception, ex.InnerException);
            }
        }

        private static ITextTemplating GetTextTemplatingService()
        {
            var textTemplating = new Mock<ITextTemplating>();
            textTemplating.Setup(t => t.ProcessTemplate(It.IsAny<string>(), It.IsAny<string>(), null, null))
                .Returns((object _, string content, object __, object ___) => content);

            var session = Mock.Of<ITextTemplatingSession>();

            var sessionHost = textTemplating.As<ITextTemplatingSessionHost>();
            sessionHost.Setup(h => h.CreateSession()).Returns(session);
            sessionHost.SetupGet(h => h.Session).Returns(session);

            return textTemplating.Object;
        }

        private static IDisposable AddCustomizedTemplates(Project project)
        {
            var templatePath = Path.GetTempFileName();
            File.WriteAllText(templatePath, "Customized");

            var property = new Mock<Property>();
            property.SetupGet(p => p.Name).Returns("FullPath");
            property.SetupGet(p => p.Value).Returns(templatePath);

            var propertyArray = new[] { property.Object };

            var properties = new Mock<Properties>();
            properties.As<IEnumerable>().Setup(p => p.GetEnumerator()).Returns(() => propertyArray.GetEnumerator());

            var contextCSharpItem = new Mock<ProjectItem>();
            var contextVBItem = new Mock<ProjectItem>();
            var entityCSharpItem = new Mock<ProjectItem>();
            var entityVBItem = new Mock<ProjectItem>();

            foreach (var projectItem in new[] { contextCSharpItem, contextVBItem, entityCSharpItem, entityVBItem })
            {
                projectItem.SetupGet(i => i.Properties).Returns(properties.Object);
            }

            var actionProjectItems = new Mock<ProjectItems>();
            actionProjectItems.Setup(i => i.Item("Context.cs.t4")).Returns(contextCSharpItem.Object);
            actionProjectItems.Setup(i => i.Item("Context.vb.t4")).Returns(contextVBItem.Object);
            actionProjectItems.Setup(i => i.Item("EntityType.cs.t4")).Returns(entityCSharpItem.Object);
            actionProjectItems.Setup(i => i.Item("EntityType.vb.t4")).Returns(entityVBItem.Object);

            var actionItem = new Mock<ProjectItem>();
            actionItem.SetupGet(i => i.ProjectItems).Returns(actionProjectItems.Object);

            var templatesProjectItems = new Mock<ProjectItems>();
            templatesProjectItems.Setup(i => i.Item("EFModelFromDatabase")).Returns(actionItem.Object);

            var templatesItem = new Mock<ProjectItem>();
            templatesItem.SetupGet(i => i.ProjectItems).Returns(templatesProjectItems.Object);

            var projectItems = new Mock<ProjectItems>();
            projectItems.Setup(i => i.Item("CodeTemplates")).Returns(templatesItem.Object);

            Mock.Get(project).SetupGet(p => p.ProjectItems).Returns(projectItems.Object);

            return new CleanupAction(() => File.Delete(templatePath));
        }

        private class Entity
        {
            public int Id { get; set; }
        }
    }
}