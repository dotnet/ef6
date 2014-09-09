// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace UnitTests.EntityDesignPackage.CustomCode
{
    using System;
    using System.Xml.Linq;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.Extensibility;
    using Microsoft.Data.Entity.Design.Package;
    using Microsoft.VisualStudio.Shell.Interop;
    using Moq;
    using UnitTests.TestHelpers;
    using VSLangProj;
    using Xunit;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    public class MicrosoftDataEntityDesignDocDataTests
    {
        [Fact]
        public void DispatchSaveToExtensions_returns_passed_string_if_passed_string_is_not_valid_xml()
        {
            var mockServicePrvider = new Mock<IServiceProvider>();

            const string fileContents = "invalid edmx";

            var docData = new MicrosoftDataEntityDesignDocData(mockServicePrvider.Object, Guid.NewGuid());
            docData.OnRegisterDocData(42, new Mock<IVsHierarchy>().Object, 3);
            Assert.Same(
                fileContents,
                docData.DispatchSaveToExtensions(
                    mockServicePrvider.Object, new Mock<ProjectItem>().Object, fileContents,
                    new Lazy<IModelConversionExtension, IEntityDesignerConversionData>[1],
                    new Lazy<IModelTransformExtension>[0]));
        }

        [Fact]
        public void DispatchSaveToExtensions_invokes_serializers_if_present()
        {
            var inputDocument = XDocument.Parse("<model />");
            var updatedDocument = XDocument.Parse("<model x=\"1\" />");

            var mockDte = new MockDTE(".NETFramework, Version=v4.5", references: new Reference[0]);
            var mockProjectItem = new Mock<ProjectItem>();
            mockProjectItem.SetupGet(i => i.ContainingProject).Returns(mockDte.Project);

            var mockSerializerExtension = new Mock<IModelTransformExtension>();
            mockSerializerExtension
                .Setup(e => e.OnBeforeModelSaved(It.IsAny<ModelTransformExtensionContext>()))
                .Callback<ModelTransformExtensionContext>(
                    context =>
                        {
                            Assert.Equal(new Version(3, 0, 0, 0), context.EntityFrameworkVersion);
                            Assert.True(XNode.DeepEquals(inputDocument, context.OriginalDocument));
                            context.CurrentDocument = updatedDocument;
                        });

            var docData = new MicrosoftDataEntityDesignDocData(mockDte.ServiceProvider, Guid.NewGuid());
            docData.RenameDocData(0, mockDte.Hierarchy, 3, "model.edmx");

            var result = docData.DispatchSaveToExtensions(
                mockDte.ServiceProvider, mockProjectItem.Object, inputDocument.ToString(),
                new Lazy<IModelConversionExtension, IEntityDesignerConversionData>[1],
                new[] { new Lazy<IModelTransformExtension>(() => mockSerializerExtension.Object) });

            Assert.True(XNode.DeepEquals(updatedDocument, XDocument.Parse(result)));

            mockSerializerExtension.Verify(
                e => e.OnAfterModelLoaded(It.IsAny<ModelTransformExtensionContext>()), Times.Never());
            mockSerializerExtension.Verify(
                e => e.OnBeforeModelSaved(It.IsAny<ModelTransformExtensionContext>()), Times.Once());
        }

#if (VS11 || VS12) // TODO: uncomment this when figure out why VS14 runtime does not allow callback at line 84
        [Fact]
        public void DispatchSaveToExtensions_invokes_converter_for_non_edmx_files_if_present()
        {
            var mockDte = new MockDTE(".NETFramework, Version=v4.5", references: new Reference[0]);
            var mockProjectItem = new Mock<ProjectItem>();
            mockProjectItem.SetupGet(i => i.ContainingProject).Returns(mockDte.Project);

            var mockConversionExtension = new Mock<IModelConversionExtension>();
            mockConversionExtension
                .Setup(c => c.OnBeforeFileSaved(It.IsAny<ModelConversionExtensionContext>()))
                .Callback<ModelConversionExtensionContext>(
                    context =>
                        {
                            Assert.Equal(new Version(3, 0, 0, 0), context.EntityFrameworkVersion);
                            context.OriginalDocument = "my model";
                        });

            var mockConversionData = new Mock<IEntityDesignerConversionData>();
            mockConversionData.Setup(d => d.FileExtension).Returns("xmde");

            var docData = new MicrosoftDataEntityDesignDocData(mockDte.ServiceProvider, Guid.NewGuid());
            docData.RenameDocData(0, mockDte.Hierarchy, 3, "model.xmde");

            Assert.Same(
                "my model",
                docData.DispatchSaveToExtensions(
                    mockDte.ServiceProvider, mockProjectItem.Object, "<model />",
                    new[]
                        {
                            new Lazy<IModelConversionExtension, IEntityDesignerConversionData>(
                                () => mockConversionExtension.Object, mockConversionData.Object),
                        },
                    new Lazy<IModelTransformExtension>[0]));

            mockConversionExtension.Verify(
                e => e.OnAfterFileLoaded(It.IsAny<ModelConversionExtensionContext>()), Times.Never());
            mockConversionExtension.Verify(
                e => e.OnBeforeFileSaved(It.IsAny<ModelConversionExtensionContext>()), Times.Once());
        }
#endif

        [Fact]
        public void DispatchSaveToExtensions_throws_for_non_edmx_if_converter_is_missing()
        {
            var mockDte = new MockDTE(".NETFramework, Version=v4.5", references: new Reference[0]);
            var mockProjectItem = new Mock<ProjectItem>();
            mockProjectItem.SetupGet(i => i.ContainingProject).Returns(mockDte.Project);

            var docData = new MicrosoftDataEntityDesignDocData(mockDte.ServiceProvider, Guid.NewGuid());
            docData.RenameDocData(0, mockDte.Hierarchy, 3, "model.xmde");

            var mockSerializerExtension = new Mock<IModelTransformExtension>();

            Assert.Equal(
                Resources.Extensibility_NoConverterForExtension,
                Assert.Throws<InvalidOperationException>(
                    () => docData.DispatchSaveToExtensions(
                        mockDte.ServiceProvider, mockProjectItem.Object, "<model />",
                        new Lazy<IModelConversionExtension, IEntityDesignerConversionData>[0],
                        new[] { new Lazy<IModelTransformExtension>(() => mockSerializerExtension.Object) })).Message);
        }
    }
}
