// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Package
{
    using System.Collections.Generic;
    using System.Data.Common;
    using EnvDTE;
    using Moq;
    using UnitTests.TestHelpers;
    using VSLangProj;
    using Xunit;

    public class ConnectionManagerTests
    {
        [Fact]
        public void
            InjectEFAttributesIntoConnectionString_does_not_App_attribute_to_connection_string_if_App_attribute_already_exists()
        {
            var connectionStringBuilder = new DbConnectionStringBuilder();
            connectionStringBuilder["MultipleActiveResultSets"] = "True"; // just to skip the code we are not targeting in this test
            connectionStringBuilder["App"] = "XYZ";
            ConnectionManager.InjectEFAttributesIntoConnectionString(
                new Mock<Project>().Object, null, connectionStringBuilder, "System.Data.SqlClient", null, null, null);

            Assert.Equal("XYZ", connectionStringBuilder["App"]);
        }

        [Fact]
        public void
            InjectEFAttributesIntoConnectionString_does_not_App_attribute_to_connection_string_if_Application_Name_attribute_already_exists(
            )
        {
            var connectionStringBuilder = new DbConnectionStringBuilder();
            connectionStringBuilder["MultipleActiveResultSets"] = "True"; // just to skip the code we are not targeting in this test
            connectionStringBuilder["Application Name"] = "XYZ";
            ConnectionManager.InjectEFAttributesIntoConnectionString(
                new Mock<Project>().Object, null, connectionStringBuilder, "System.Data.SqlClient", null, null, null);

            Assert.Equal("XYZ", connectionStringBuilder["Application Name"]);
            Assert.False(connectionStringBuilder.ContainsKey("App"));
        }

        [Fact]
        public void InjectEFAttributesIntoConnectionString_does_not_App_attribute_to_connection_string_for_non_EF_projects()
        {
            var monikerHelper = new MockDTE("XBox,Version=v4.5");

            var connectionStringBuilder = new DbConnectionStringBuilder();
            connectionStringBuilder["MultipleActiveResultSets"] = "True"; // just to skip the code we are not targeting in this test
            ConnectionManager.InjectEFAttributesIntoConnectionString(
                monikerHelper.Project, monikerHelper.ServiceProvider, connectionStringBuilder, "System.Data.SqlClient",
                null, null, null);

            Assert.False(connectionStringBuilder.ContainsKey("App"));
        }

        [Fact]
        public void InjectEFAttributesIntoConnectionString_adds_App_attribute_to_connection_string_for_EF_projects_if_needed()
        {
            var monikerHelper = new MockDTE(".NETFramework,Version=v4.5");

            var connectionStringBuilder = new DbConnectionStringBuilder();
            connectionStringBuilder["MultipleActiveResultSets"] = "True"; // just to skip the code we are not targeting in this test
            ConnectionManager.InjectEFAttributesIntoConnectionString(
                monikerHelper.Project, monikerHelper.ServiceProvider, connectionStringBuilder, "System.Data.SqlClient",
                null, null, null);

            Assert.Equal("EntityFramework", connectionStringBuilder["App"]);
        }

        [Fact]
        public void GetMetadataFileNamesFromArtifactFileName_creates_metadata_file_names_for_non_null_edmx_ProjectItem()
        {
            var mockDte = new MockDTE(".NETFramework, Version=v4.5", references: new Reference[0]);
            mockDte.SetProjectProperties(new Dictionary<string, object> { { "FullPath", @"D:\Projects\Project\Folder" } });
            var mockParentProjectItem = new Mock<ProjectItem>();
            mockParentProjectItem.Setup(p => p.Collection).Returns(new Mock<ProjectItems>().Object);
            mockParentProjectItem.Setup(p => p.Name).Returns("Folder");

            var mockModelProjectItem = new Mock<ProjectItem>();
            var mockCollection = new Mock<ProjectItems>();
            mockCollection.Setup(p => p.Parent).Returns(mockParentProjectItem.Object);
            mockModelProjectItem.Setup(p => p.Collection).Returns(mockCollection.Object);

            var metadataFileNames =
                ConnectionManager.GetMetadataFileNamesFromArtifactFileName(
                mockDte.Project, @"c:\temp\myModel.edmx", mockDte.ServiceProvider, (_, __) => mockModelProjectItem.Object);

            Assert.Equal(@".\Folder\myModel.csdl", metadataFileNames[0]);
            Assert.Equal(@".\Folder\myModel.ssdl", metadataFileNames[1]);
            Assert.Equal(@".\Folder\myModel.msl", metadataFileNames[2]);
        }

        [Fact]
        public void GetMetadataFileNamesFromArtifactFileName_creates_metadata_file_names_for_null_edmx_ProjectItem()
        {
            var mockDte = new MockDTE(".NETFramework, Version=v4.5", references: new Reference[0]);
            mockDte.SetProjectProperties(new Dictionary<string, object> { { "FullPath", @"C:\Projects\Project\Folder" } });
            
            var metadataFileNames =
                ConnectionManager.GetMetadataFileNamesFromArtifactFileName(
                mockDte.Project, @"c:\temp\myModel.edmx", mockDte.ServiceProvider, (_, __) => null);

            Assert.Equal(@".\..\..\..\temp\myModel.csdl", metadataFileNames[0]);
            Assert.Equal(@".\..\..\..\temp\myModel.ssdl", metadataFileNames[1]);
            Assert.Equal(@".\..\..\..\temp\myModel.msl", metadataFileNames[2]);
        }
    }
}
