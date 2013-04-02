// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ConnectionFactoryConfig
{
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Utilities;
    using System.Xml.Linq;

    internal class AddProviderCommand : MigrationsDomainCommand
    {
        public AddProviderCommand(string invariantName, string typeName)
        {
            // Using check because this is effecitively public surface since
            // it is called by a PowerShell command.
            Check.NotEmpty(invariantName, "invariantName");
            Check.NotEmpty(typeName, "typeName");

            Execute(() => Execute(invariantName, typeName));
        }

        public void Execute(string invariantName, string typeName)
        {
            DebugCheck.NotEmpty(invariantName);
            DebugCheck.NotEmpty(typeName);

            var manipulator = new ConfigFileManipulator();
            var processor = new ConfigFileProcessor();

            new ConfigFileFinder().FindConfigFiles(
                Project.ProjectItems,
                i => processor.ProcessConfigFile(
                    i, new Func<XDocument, bool>[]
                        {
                            c => manipulator.AddOrUpdateConfigSection(c, GetType().Assembly.GetName().Version),
                            c => manipulator.AddProviderToConfig(c, invariantName, typeName)
                        }));
        }
    }
}
