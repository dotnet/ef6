// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.Migrations.Extensions;
    using System.Data.Entity.Migrations.Utilities;
    using System.Data.Entity.Utilities;
    using System.IO;
    using EnvDTE;

    internal abstract class MigrationsDomainCommand
    {
        private readonly AppDomain _domain;
        private readonly DomainDispatcher _dispatcher;

        protected MigrationsDomainCommand()
        {
            _domain = AppDomain.CurrentDomain;
            _dispatcher = (DomainDispatcher)_domain.GetData("efDispatcher");
        }

        public virtual Project Project
        {
            get { return (Project)_domain.GetData("project"); }
        }

        public Project StartUpProject
        {
            get { return (Project)_domain.GetData("startUpProject"); }
        }

        public Project ContextProject
        {
            get { return (Project)_domain.GetData("contextProject"); }
        }

        protected AppDomain Domain
        {
            get { return _domain; }
        }

        public void Execute(Action command)
        {
            DebugCheck.NotNull(command);

            Init();

            try
            {
                command();
            }
            catch (Exception ex)
            {
                Throw(ex);
            }
        }

        public virtual void WriteLine(string message)
        {
            DebugCheck.NotEmpty(message);

            _dispatcher.WriteLine(message);
        }

        public virtual void WriteWarning(string message)
        {
            DebugCheck.NotEmpty(message);

            _dispatcher.WriteWarning(message);
        }

        public void WriteVerbose(string message)
        {
            DebugCheck.NotEmpty(message);

            _dispatcher.WriteVerbose(message);
        }

        public virtual ToolingFacade GetFacade(string configurationTypeName = null, bool useContextWorkingDirectory = false)
        {
            if (configurationTypeName == null)
            {
                configurationTypeName = (string)Domain.GetData("configurationTypeName");
            }

            var connectionStringName = (string)Domain.GetData("connectionStringName");
            var connectionString = (string)Domain.GetData("connectionString");
            var connectionProviderName = (string)Domain.GetData("connectionProviderName");
            DbConnectionInfo connectionStringInfo = null;

            if (!string.IsNullOrWhiteSpace(connectionStringName))
            {
                connectionStringInfo = new DbConnectionInfo(connectionStringName);
            }
            else if (!string.IsNullOrWhiteSpace(connectionString))
            {
                connectionStringInfo = new DbConnectionInfo(connectionString, connectionProviderName);
            }

            var startUpProject = StartUpProject;
            var assemblyName = Project.GetTargetName();
            var contextAssemblyName = ContextProject.GetTargetName();
            var workingDirectory = useContextWorkingDirectory ? ContextProject.GetTargetDir() : Project.GetTargetDir();

            string configurationFile;
            string dataDirectory = null;

            if (startUpProject.IsWebProject())
            {
                var startUpProjectDir = startUpProject.GetProjectDir();

                configurationFile = startUpProject.GetFileName("Web.config");
                dataDirectory = Path.Combine(startUpProjectDir, "App_Data");
            }
            else
            {
                configurationFile = startUpProject.GetFileName("App.config");
            }

            return new ToolingFacade(
                assemblyName,
                contextAssemblyName,
                configurationTypeName,
                workingDirectory,
                configurationFile,
                dataDirectory,
                connectionStringInfo)
                       {
                           LogInfoDelegate = WriteLine,
                           LogWarningDelegate = WriteWarning,
                           LogVerboseDelegate = WriteVerbose
                       };
        }

        public T GetAnonymousArgument<T>(string name)
        {
            return (T)_domain.GetData(name);
        }

        private void Init()
        {
            _domain.SetData("wasError", false);
            _domain.SetData("error.Message", null);
            _domain.SetData("error.TypeName", null);
            _domain.SetData("error.StackTrace", null);
        }

        private void Throw(Exception ex)
        {
            DebugCheck.NotNull(ex);

            _domain.SetData("wasError", true);
            _domain.SetData("error.Message", ex.Message);
            _domain.SetData("error.TypeName", ex.GetType().FullName);
            _domain.SetData("error.StackTrace", ex.ToString());
        }
    }
}
