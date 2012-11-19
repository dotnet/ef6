// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Utilities
{
    using System.Collections.Generic;
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.Migrations.Extensions;
    using System.Data.Entity.Migrations.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.IO;
    using System.Resources;

    internal class MigrationWriter
    {
        private readonly MigrationsDomainCommand _command;

        public MigrationWriter(MigrationsDomainCommand command)
        {
            DebugCheck.NotNull(command);

            _command = command;
        }

        public string Write(ScaffoldedMigration scaffoldedMigration, bool rescaffolding = false, bool force = false, string name = null)
        {
            DebugCheck.NotNull(scaffoldedMigration);

            var userCodeFileName = scaffoldedMigration.MigrationId + "." + scaffoldedMigration.Language;
            var userCodePath = Path.Combine(scaffoldedMigration.Directory, userCodeFileName);
            var designerCodeFileName = scaffoldedMigration.MigrationId + ".Designer."
                                       + scaffoldedMigration.Language;
            var designerCodePath = Path.Combine(scaffoldedMigration.Directory, designerCodeFileName);
            var resourcesFileName = scaffoldedMigration.MigrationId + ".resx";
            var resourcesPath = Path.Combine(scaffoldedMigration.Directory, resourcesFileName);

            if (rescaffolding && !force)
            {
                var absoluteUserCodePath = Path.Combine(_command.Project.GetProjectDir(), userCodePath);

                if (!string.Equals(scaffoldedMigration.UserCode, File.ReadAllText(absoluteUserCodePath)))
                {
                    Debug.Assert(!string.IsNullOrWhiteSpace(name));

                    _command.WriteWarning(Strings.RescaffoldNoForce(name));
                }
            }
            else
            {
                _command.Project.AddFile(userCodePath, scaffoldedMigration.UserCode);
            }

            WriteResources(userCodePath, resourcesPath, scaffoldedMigration.Resources);
            _command.Project.AddFile(designerCodePath, scaffoldedMigration.DesignerCode);

            return userCodePath;
        }

        private void WriteResources(string userCodePath, string resourcesPath, IDictionary<string, object> resources)
        {
            DebugCheck.NotEmpty(userCodePath);
            DebugCheck.NotEmpty(resourcesPath);
            DebugCheck.NotNull(resources);

            var absoluteResourcesPath = Path.Combine(_command.Project.GetProjectDir(), resourcesPath);

            _command.Project.EditFile(resourcesPath);

            using (var writer = new ResXResourceWriter(absoluteResourcesPath))
            {
                resources.Each(i => writer.AddResource(i.Key, i.Value));
            }

            _command.Project.AddFile(resourcesPath);
        }
    }
}
