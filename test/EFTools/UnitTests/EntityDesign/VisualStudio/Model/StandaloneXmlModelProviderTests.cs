// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Model
{
    using EnvDTE;
    using Microsoft.Data.Entity.Design.Extensibility;
    using Microsoft.Data.Tools.XmlDesignerBase.Model;
    using Moq;
    using Moq.Protected;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using UnitTests.TestHelpers;
    using VSLangProj;
    using Xunit;

    public class StandaloneXmlModelProviderTests
    {
        [Fact]
        public void TryGetBufferViaExtensions_returns_false_when_converter_is_present_but_transformer_is_absent_for_non_edmx_file()
        {
            var mockDte = new MockDTE(".NETFramework, Version=v4.5", references: new Reference[0]);
            var mockProjectItem = new Mock<ProjectItem>();
            mockProjectItem.SetupGet(i => i.ContainingProject).Returns(mockDte.Project);
            mockProjectItem.Setup(i => i.get_FileNames(It.IsAny<short>())).Returns("non-edmx-file.xmde");

            var mockConversionData = new Mock<IEntityDesignerConversionData>();
            mockConversionData.SetupGet(d => d.FileExtension).Returns("xmde");

            var mockConversionExtension = new Mock<IModelConversionExtension>();
            mockConversionExtension
                .Setup(e => e.OnAfterFileLoaded(It.IsAny<ModelConversionExtensionContext>()))
                .Callback<ModelConversionExtensionContext>(
                    ctx =>
                        {
                            Assert.Equal(ctx.EntityFrameworkVersion, new Version(3, 0, 0, 0));
                            Assert.Equal(ctx.FileInfo.Name, "non-edmx-file.xmde");
                            Assert.Same(ctx.ProjectItem, mockProjectItem.Object);
                            Assert.Same(ctx.Project, mockDte.Project);
                        });

            var converter =
                new Lazy<IModelConversionExtension, IEntityDesignerConversionData>(
                    () => mockConversionExtension.Object, mockConversionData.Object);

            string outputDocument;
            List<ExtensionError> errors;
            Assert.False(
                StandaloneXmlModelProvider.TryGetBufferViaExtensions(
                    mockDte.ServiceProvider, mockProjectItem.Object, string.Empty, new[] { converter },
                    new Lazy<IModelTransformExtension>[0], out outputDocument, out errors));

            Assert.Empty(outputDocument);
            Assert.Empty(errors);

            mockConversionExtension.Verify(e => e.OnAfterFileLoaded(It.IsAny<ModelConversionExtensionContext>()), Times.Once());
            mockConversionExtension.Verify(e => e.OnBeforeFileSaved(It.IsAny<ModelConversionExtensionContext>()), Times.Never());
            mockConversionData.Verify(e => e.FileExtension, Times.Once());
        }

        [Fact]
        // note that this may not be the desired behavior see: https://entityframework.codeplex.com/workitem/1371
        public void TryGetBufferViaExtensions_throws_when_converter_is_absent_for_non_edmx_file()
        {
            var mockDte = new MockDTE(".NETFramework, Version=v4.5", references: new Reference[0]);
            var mockProjectItem = new Mock<ProjectItem>();
            mockProjectItem.SetupGet(i => i.ContainingProject).Returns(mockDte.Project);
            mockProjectItem.Setup(i => i.get_FileNames(It.IsAny<short>())).Returns("non-edmx-file.xmde");

            // need to pass a transformer since there must be at least onve converter or transformer
            // and the test tests a case where converter does not exist
            var mockTransformExtension = new Mock<IModelTransformExtension>();
            var transformer = new Lazy<IModelTransformExtension>(() => mockTransformExtension.Object);

            string outputDocument;
            List<ExtensionError> errors;

            Assert.Equal(
                Resources.Extensibility_NoConverterForExtension,
                Assert.Throws<InvalidOperationException>(
                    () => StandaloneXmlModelProvider.TryGetBufferViaExtensions(
                        mockDte.ServiceProvider, mockProjectItem.Object, "<root/>",
                        new Lazy<IModelConversionExtension, IEntityDesignerConversionData>[0],
                        new[] { transformer }, out outputDocument, out errors)).Message);
        }

        [Fact]
        public void TryGetBufferViaExtensions_returns_false_when_transformer_does_not_modify_original_document()
        {
            var mockDte = new MockDTE(".NETFramework, Version=v4.5", references: new Reference[0]);
            var mockProjectItem = new Mock<ProjectItem>();
            mockProjectItem.SetupGet(i => i.ContainingProject).Returns(mockDte.Project);
            mockProjectItem.Setup(i => i.get_FileNames(It.IsAny<short>())).Returns("model.edmx");

            var mockTransformExtension = new Mock<IModelTransformExtension>();
            mockTransformExtension
                .Setup(e => e.OnAfterModelLoaded(It.IsAny<ModelTransformExtensionContext>()))
                .Callback<ModelTransformExtensionContext>(
                    ctx =>
                        {
                            Assert.Equal(ctx.EntityFrameworkVersion, new Version(3, 0, 0, 0));
                            Assert.True(XNode.DeepEquals(ctx.OriginalDocument, XDocument.Parse("<root />")));
                            Assert.Same(ctx.ProjectItem, mockProjectItem.Object);
                            Assert.Same(ctx.Project, mockDte.Project);
                        });

            var transformer = new Lazy<IModelTransformExtension>(() => mockTransformExtension.Object);

            string outputDocument;
            List<ExtensionError> errors;
            Assert.False(
                StandaloneXmlModelProvider.TryGetBufferViaExtensions(
                    mockDte.ServiceProvider, mockProjectItem.Object, "<root/>",
                    new Lazy<IModelConversionExtension, IEntityDesignerConversionData>[0],
                    new[] { transformer }, out outputDocument, out errors));

            Assert.Empty(outputDocument);
            Assert.Empty(errors);

            mockTransformExtension.Verify(
                e => e.OnAfterModelLoaded(It.IsAny<ModelTransformExtensionContext>()), Times.Once());
            mockTransformExtension.Verify(
                e => e.OnBeforeModelSaved(It.IsAny<ModelTransformExtensionContext>()), Times.Never());
        }

        [Fact]
        public void TryGetBufferViaExtensions_returns_true_when_transformer_is_present_but_converter_is_absent_for_edmx_file()
        {
            var mockDte = new MockDTE(".NETFramework, Version=v4.5", references: new Reference[0]);
            var mockProjectItem = new Mock<ProjectItem>();
            mockProjectItem.SetupGet(i => i.ContainingProject).Returns(mockDte.Project);
            mockProjectItem.Setup(i => i.get_FileNames(It.IsAny<short>())).Returns("model.edmx");

            var mockTransformExtension = new Mock<IModelTransformExtension>();
            mockTransformExtension
                .Setup(e => e.OnAfterModelLoaded(It.IsAny<ModelTransformExtensionContext>()))
                .Callback<ModelTransformExtensionContext>(
                    ctx =>
                        {
                            Assert.Equal(ctx.EntityFrameworkVersion, new Version(3, 0, 0, 0));
                            Assert.True(XNode.DeepEquals(ctx.OriginalDocument, XDocument.Parse("<root />")));
                            Assert.Same(ctx.ProjectItem, mockProjectItem.Object);
                            Assert.Same(ctx.Project, mockDte.Project);

                            var modifiedDocument = new XDocument(ctx.OriginalDocument);
                            modifiedDocument.Root.Add(new XAttribute("test", "value"));
                            ctx.CurrentDocument = modifiedDocument;
                        });

            var transformer = new Lazy<IModelTransformExtension>(() => mockTransformExtension.Object);

            string outputDocument;
            List<ExtensionError> errors;
            Assert.True(
                StandaloneXmlModelProvider.TryGetBufferViaExtensions(
                    mockDte.ServiceProvider, mockProjectItem.Object, "<root/>",
                    new Lazy<IModelConversionExtension, IEntityDesignerConversionData>[0],
                    new[] { transformer }, out outputDocument, out errors));

            Assert.True(XNode.DeepEquals(XDocument.Parse("<root test=\"value\" />"), XDocument.Parse(outputDocument)));
            Assert.Empty(errors);

            mockTransformExtension.Verify(
                e => e.OnAfterModelLoaded(It.IsAny<ModelTransformExtensionContext>()), Times.Once());
            mockTransformExtension.Verify(
                e => e.OnBeforeModelSaved(It.IsAny<ModelTransformExtensionContext>()), Times.Never());
        }

        // [Fact] https://entityframework.codeplex.com/workitem/1371
        public void TryGetBufferViaExtensions_passes_content_from_converter_to_transformer_and_returns_true()
        {
            var mockDte = new MockDTE(".NETFramework, Version=v4.5", references: new Reference[0]);
            var mockProjectItem = new Mock<ProjectItem>();
            mockProjectItem.SetupGet(i => i.ContainingProject).Returns(mockDte.Project);
            mockProjectItem.Setup(i => i.get_FileNames(It.IsAny<short>())).Returns("non-edmx-file.xmde");

            // converter setup
            var mockConversionData = new Mock<IEntityDesignerConversionData>();
            mockConversionData.SetupGet(d => d.FileExtension).Returns("xmde");

            var mockConversionExtension = new Mock<IModelConversionExtension>();
            mockConversionExtension
                .Setup(e => e.OnAfterFileLoaded(It.IsAny<ModelConversionExtensionContext>()))
                .Callback<ModelConversionExtensionContext>(
                    ctx =>
                        {
                            Assert.Equal(ctx.EntityFrameworkVersion, new Version(3, 0, 0, 0));
                            Assert.Equal(ctx.FileInfo.Name, "non-edmx-file.xmde");
                            Assert.Same(ctx.ProjectItem, mockProjectItem.Object);
                            Assert.Same(ctx.Project, mockDte.Project);

                            // https://entityframework.codeplex.com/workitem/1371
                            // ctx.CurrentDocument = "<root />";
                        });

            // transformer setup
            var mockTransformExtension = new Mock<IModelTransformExtension>();
            mockTransformExtension
                .Setup(e => e.OnAfterModelLoaded(It.IsAny<ModelTransformExtensionContext>()))
                .Callback<ModelTransformExtensionContext>(
                    ctx =>
                        {
                            Assert.Equal(ctx.EntityFrameworkVersion, new Version(3, 0, 0, 0));
                            Assert.True(XNode.DeepEquals(ctx.OriginalDocument, XDocument.Parse("<root />")));
                            Assert.Same(ctx.ProjectItem, mockProjectItem.Object);
                            Assert.Same(ctx.Project, mockDte.Project);

                            var modifiedDocument = new XDocument(ctx.OriginalDocument);
                            modifiedDocument.Root.Add(new XAttribute("test", "value"));
                            ctx.CurrentDocument = modifiedDocument;
                        });

            var converter =
                new Lazy<IModelConversionExtension, IEntityDesignerConversionData>(
                    () => mockConversionExtension.Object, mockConversionData.Object);
            var transformer = new Lazy<IModelTransformExtension>(() => mockTransformExtension.Object);

            string outputDocument;
            List<ExtensionError> errors;
            Assert.True(
                StandaloneXmlModelProvider.TryGetBufferViaExtensions(
                    mockDte.ServiceProvider, mockProjectItem.Object, string.Empty,
                    new[] { converter }, new[] { transformer }, out outputDocument, out errors));

            Assert.True(XNode.DeepEquals(XDocument.Parse("<root test=\"value\" />"), XDocument.Parse(outputDocument)));
            Assert.Empty(errors);

            mockConversionExtension.Verify(
                e => e.OnAfterFileLoaded(It.IsAny<ModelConversionExtensionContext>()), Times.Once());
            mockConversionExtension.Verify(
                e => e.OnBeforeFileSaved(It.IsAny<ModelConversionExtensionContext>()), Times.Never());

            mockTransformExtension.Verify(
                e => e.OnAfterModelLoaded(It.IsAny<ModelTransformExtensionContext>()), Times.Once());
            mockTransformExtension.Verify(
                e => e.OnBeforeModelSaved(It.IsAny<ModelTransformExtensionContext>()), Times.Never());

            mockConversionData.Verify(e => e.FileExtension, Times.Exactly(2));
        }

        [Fact]
        public void Build_creates_annotated_XDocument()
        {
            var serviceProvider = new Mock<IServiceProvider>().Object;
            var mockModelProvider = new Mock<StandaloneXmlModelProvider>(serviceProvider) { CallBase = true };
            mockModelProvider.Protected()
                .Setup<string>("ReadEdmxContents", ItExpr.IsAny<Uri>())
                .Returns("<root>\n<child />\n</root>");

            var xDoc = mockModelProvider.Object.GetXmlModel(new Uri("z:\\model.edmx")).Document;

            Assert.True(
                xDoc.Descendants().All(
                    e =>
                        {
                            var textRange = e.GetTextRange();
                            return textRange != null && textRange.OpenStartLine > 0 && textRange.OpenStartColumn > 0;
                        }));
        }
    }
}
