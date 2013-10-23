// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using EnvDTE;
    using Moq;
    using Moq.Protected;
    using UnitTests.TestHelpers;
    using VSLangProj;
    using Xunit;

    public class RetargettingHandlerTests
    {
        [Fact]
        public void RetargetFilesInProject_retargets_Edmx_files_in_project()
        {
            var mockDte = new MockDTE(".NETFramework, Version=v4.5", references: new Reference[0]);

            var projectItems =
                new[]
                    {
                        MockDTE.CreateProjectItem("C:\\model1.edmx"),
                        MockDTE.CreateProjectItem("D:\\model2.EDMX")
                    };

            var fileInfos =
                projectItems.Select(
                    i =>
                    new VSFileFinder.VSFileInfo
                        {
                            Hierarchy = mockDte.Hierarchy,
                            ItemId = mockDte.AddProjectItem(i),
                            Path = i.Object.get_FileNames(1)
                        }).ToArray();

            var mockRetargetingHandler =
                new Mock<RetargetingHandler>(mockDte.Hierarchy, mockDte.ServiceProvider)
                    {
                        CallBase = true
                    };

            mockRetargetingHandler
                .Protected()
                .Setup<IEnumerable<VSFileFinder.VSFileInfo>>("GetEdmxFileInfos")
                .Returns(fileInfos);

            mockRetargetingHandler
                .Protected().Setup<bool>("IsDataServicesEdmx", ItExpr.IsAny<string>()).Returns(false);

            mockRetargetingHandler
                .Protected()
                .Setup<XmlDocument>("RetargetFile", ItExpr.IsAny<string>(), ItExpr.IsAny<Version>()).Returns(new XmlDocument());

            mockRetargetingHandler
                .Protected()
                .Setup("WriteModifiedFiles", ItExpr.IsAny<Project>(), ItExpr.IsAny<Dictionary<string, object>>())
                .Callback(
                    (Project project, Dictionary<string, object> documentMap) =>
                    Assert.Equal(fileInfos.Select(f => f.Path), documentMap.Keys));

            mockRetargetingHandler.Object.RetargetFilesInProject();

            mockRetargetingHandler.Protected().Verify("IsDataServicesEdmx", Times.Exactly(2), ItExpr.IsAny<string>());
            mockRetargetingHandler.Protected().Verify("RetargetFile", Times.Exactly(2), ItExpr.IsAny<string>(), ItExpr.IsAny<Version>());
        }

        [Fact]
        public void RetargetFilesInProject_wont_retarget_data_services_Edmx_files_in_project()
        {
            var mockDte = new MockDTE(".NETFramework, Version=v4.5", references: new Reference[0]);

            var projectItems =
                new[]
                    {
                        MockDTE.CreateProjectItem("C:\\model1.edmx"),
                        MockDTE.CreateProjectItem("D:\\model2.EDMX")
                    };

            var fileInfos =
                projectItems.Select(
                    i =>
                    new VSFileFinder.VSFileInfo
                        {
                            Hierarchy = mockDte.Hierarchy,
                            ItemId = mockDte.AddProjectItem(i),
                            Path = i.Object.get_FileNames(1)
                        }).ToArray();

            var mockRetargetingHandler =
                new Mock<RetargetingHandler>(mockDte.Hierarchy, mockDte.ServiceProvider)
                    {
                        CallBase = true
                    };

            mockRetargetingHandler
                .Protected()
                .Setup<IEnumerable<VSFileFinder.VSFileInfo>>("GetEdmxFileInfos")
                .Returns(fileInfos);

            mockRetargetingHandler
                .Protected().Setup<bool>("IsDataServicesEdmx", ItExpr.IsAny<string>()).Returns(true);

            mockRetargetingHandler
                .Protected()
                .Setup("WriteModifiedFiles", ItExpr.IsAny<Project>(), ItExpr.IsAny<Dictionary<string, object>>())
                .Callback((Project project, Dictionary<string, object> documentMap) => Assert.Empty(documentMap));

            mockRetargetingHandler.Object.RetargetFilesInProject();

            mockRetargetingHandler.Protected().Verify("IsDataServicesEdmx", Times.Exactly(2), ItExpr.IsAny<string>());
            mockRetargetingHandler.Protected().Verify("RetargetFile", Times.Never(), ItExpr.IsAny<string>(), ItExpr.IsAny<Version>());
        }
    }
}
