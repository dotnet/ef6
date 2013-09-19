// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ConnectionFactoryConfig
{
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Utilities;
    using System.Xml.Linq;

    internal class AddDefaultConnectionFactoryCommand : MigrationsDomainCommand
    {
        public AddDefaultConnectionFactoryCommand(string typeName, string[] constructorArguments)
        {
            // Using check because this is effecitively public surface since
            // it is called by a PowerShell command.
            Check.NotEmpty(typeName, "typeName");

            Execute(() => Execute(typeName, constructorArguments));
        }

        public void Execute(string typeName, string[] constructorArguments)
        {
            DebugCheck.NotEmpty(typeName);

            var manipulator = new ConfigFileManipulator();
            var processor = new ConfigFileProcessor();

            new ConfigFileFinder().FindConfigFiles(
                Project.ProjectItems,
                i => processor.ProcessConfigFile(
                    i, new Func<XDocument, bool>[]
                        {
                            c => manipulator.AddOrUpdateConfigSection(c, GetType().Assembly.GetName().Version),
                            c => manipulator.AddOrUpdateConnectionFactoryInConfig(
                                c, new ConnectionFactorySpecification(typeName, constructorArguments))
                        }));
        }
    }
}
