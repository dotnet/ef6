// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Package
{
    using System.Data.Common;
    using EnvDTE;
    using Moq;
    using UnitTests.TestHelpers;
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
    }
}
