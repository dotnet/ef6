// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Collections.Generic;
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.Migrations.Resources;
    using System.Data.Entity.Migrations.Utilities;
    using System.Linq;
    using EnvDTE;
    using Moq;
    using Xunit;

    public class AddMigrationCommandTests
    {
        [Fact]
        public void Writes_scaffolding_message_when_new_migration()
        {
            var command = new TestableAddMigrationCommand();

            command.Execute("M", false, false);

            Assert.Equal(Strings.ScaffoldingMigration("M"), command.Messages.Single());
        }

        [Fact]
        public void Opens_migration_file_when_no_error()
        {
            var command = new TestableAddMigrationCommand();

            command.Execute("M", false, false);

            command.MockDispatcher.Verify(p => p.OpenFile(@".\M"));
        }

        [Fact]
        public void Writes_rescaffolding_message_when_rescaffolding()
        {
            var command = new TestableAddMigrationCommand();

            command.MockToolingFacade
                   .Setup(f => f.Scaffold("M", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                   .Returns(
                       new ScaffoldedMigration
                       {
                           IsRescaffold = true
                       });

            command.Execute("M", false, false);

            Assert.Equal(Strings.RescaffoldingMigration("M"), command.Messages.Single());
        }

        [Fact]
        public void Writes_code_behind_warning_when_new_migration()
        {
            var command = new TestableAddMigrationCommand();

            command.MockToolingFacade
                   .Setup(f => f.Scaffold("M", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                   .Returns(
                       new ScaffoldedMigration
                           {
                               MigrationId = "Foo"
                           });

            command.Execute("M", false, false);

            Assert.Equal(Strings.SnapshotBehindWarning("M"), command.Warnings.Single());
        }

        [Fact]
        public void Writes_rescaffold_warning_when_new_migration_name_matches_only_applied_migration()
        {
            var command = new TestableAddMigrationCommand();

            command.MockToolingFacade
                   .Setup(f => f.GetDatabaseMigrations()).Returns(new[] { "201301040020540_M" });

            command.Execute("M", false, false);

            Assert.Equal(
                Strings.DidYouMeanToRescaffold("M", "$InitialDatabase", "M"),
                command.Warnings.Last().Trim());
        }

        [Fact]
        public void Writes_rescaffold_warning_when_new_migration_name_matches_last_applied_migration()
        {
            var command = new TestableAddMigrationCommand();

            command.MockToolingFacade
                   .Setup(f => f.GetDatabaseMigrations())
                   .Returns(new[] { "201301040020540_M2", "201301040020540_M1" });

            command.Execute("M2", false, false);

            Assert.Equal(
                Strings.DidYouMeanToRescaffold("M2", "201301040020540_M1", "M2"),
                command.Warnings.Last().Trim());
        }

        internal class TestableAddMigrationCommand : AddMigrationCommand
        {
            public readonly Mock<ToolingFacade> MockToolingFacade
                = new Mock<ToolingFacade>
                      {
                          DefaultValue = DefaultValue.Mock
                      };

            public readonly Mock<Project> MockProject
                = new Mock<Project>
                      {
                          DefaultValue = DefaultValue.Mock
                      };

            public readonly Mock<DomainDispatcher> MockDispatcher = new Mock<DomainDispatcher>();

            public readonly List<string> Messages = new List<string>();
            public readonly List<string> Warnings = new List<string>();

            public TestableAddMigrationCommand()
            {
                AppDomain.CurrentDomain.SetData("efDispatcher", MockDispatcher.Object);

                var mockFullPathProperty = new Mock<Property>();
                mockFullPathProperty.SetupGet(p => p.Value).Returns(".");

                MockProject.Setup(p => p.Properties.Item("FullPath")).Returns(mockFullPathProperty.Object);

                var mockRootNamespaceProperty = new Mock<Property>();
                mockRootNamespaceProperty.SetupGet(p => p.Value).Returns("N");

                MockProject.Setup(p => p.Properties.Item("RootNamespace")).Returns(mockRootNamespaceProperty.Object);
            }

            public override ToolingFacade GetFacade(string configurationTypeName = null)
            {
                return MockToolingFacade.Object;
            }

            public override Project Project
            {
                get { return MockProject.Object; }
            }

            public override void WriteLine(string message)
            {
                Messages.Add(message);
            }

            public override void WriteWarning(string message)
            {
                Warnings.Add(message);
            }

            protected override string WriteMigration(string name, bool force, ScaffoldedMigration scaffoldedMigration, bool rescaffolding)
            {
                return name;
            }
        }
    }
}
