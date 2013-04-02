// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ConnectionFactoryConfig
{
    using System.Data.Entity.Migrations;
    using System.ServiceProcess;
    using System.Xml.Linq;
    using Microsoft.Win32;

    internal class InitializeEntityFrameworkCommand : MigrationsDomainCommand
    {
        public InitializeEntityFrameworkCommand()
        {
            Execute(Execute);
        }

        public void Execute()
        {
            using (
                var detector = new SqlServerDetector(
                    Registry.LocalMachine, new ServiceControllerProxy(new ServiceController("MSSQL$SQLEXPRESS"))))
            {
                var factorySpecification = detector.BuildConnectionFactorySpecification();
                var manipulator = new ConfigFileManipulator();
                var processor = new ConfigFileProcessor();

                new ConfigFileFinder().FindConfigFiles(
                    Project.ProjectItems,
                    i => processor.ProcessConfigFile(
                        i, new Func<XDocument, bool>[]
                            {
                                c => manipulator.AddOrUpdateConfigSection(c, GetType().Assembly.GetName().Version),
                                c => manipulator.AddConnectionFactoryToConfig(c, factorySpecification)
                            }));
            }

            new ReferenceRemover(Project).TryRemoveSystemDataEntity();
        }
    }
}
