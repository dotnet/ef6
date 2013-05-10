// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Collections.Generic;
    using System.Data.Entity.Migrations.Extensions;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.Resources;
    using System.Data.Entity.Migrations.Utilities;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using EnvDTE;

    internal class EnableMigrationsCommand : MigrationsDomainCommand
    {
        public EnableMigrationsCommand(bool enableAutomaticMigrations, bool force)
        {
            Execute(
                () =>
                    {
                        var project = Project;

                        var contextTypeName = GetAnonymousArgument<string>("ContextTypeName");
                        var migrationsDirectory = GetAnonymousArgument<string>("MigrationsDirectory");
                        var qualifiedContextTypeName = FindContextToEnable(contextTypeName);
                        var isVb = project.CodeModel.Language == CodeModelLanguageConstants.vsCMLanguageVB;
                        var fileName = isVb ? "Configuration.vb" : "Configuration.cs";
                        var template = LoadTemplate(fileName);

                        var tokens = new Dictionary<string, string>();

                        if (!string.IsNullOrWhiteSpace(migrationsDirectory))
                        {
                            tokens["migrationsDirectory"]
                                = "\r\n            MigrationsDirectory = "
                                  + (!isVb ? "@" : null)
                                  + "\"" + migrationsDirectory + "\""
                                  + (!isVb ? ";" : null);
                        }
                        else
                        {
                            migrationsDirectory = "Migrations";
                        }

                        tokens["enableAutomaticMigrations"]
                            = enableAutomaticMigrations
                                  ? (isVb ? "True" : "true")
                                  : (isVb ? "False" : "false");

                        var rootNamespace = project.GetRootNamespace();
                        var migrationsNamespace = migrationsDirectory.Replace("\\", ".");

                        tokens["namespace"]
                            = !isVb && !string.IsNullOrWhiteSpace(rootNamespace)
                                  ? rootNamespace + "." + migrationsNamespace
                                  : migrationsNamespace;

                        if (string.IsNullOrWhiteSpace(qualifiedContextTypeName))
                        {
                            tokens["contextType"]
                                = isVb
                                      ? "[[type name]]"
                                      : "/* TODO: put your Code First context type name here */";

                            if (isVb)
                            {
                                tokens["contextTypeComment"]
                                    = "\r\n        'TODO: replace [[type name]] with your Code First context type name";
                            }
                        }
                        else if (isVb && qualifiedContextTypeName.StartsWith(rootNamespace + "."))
                        {
                            tokens["contextType"]
                                = qualifiedContextTypeName.Substring(rootNamespace.Length + 1).Replace('+', '.');
                        }
                        else
                        {
                            tokens["contextType"] = qualifiedContextTypeName.Replace('+', '.');
                        }

                        if (Path.IsPathRooted(migrationsDirectory))
                        {
                            throw new MigrationsException(Strings.MigrationsDirectoryParamIsRooted(migrationsDirectory));
                        }

                        var path = Path.Combine(migrationsDirectory, fileName);
                        var absolutePath = Path.Combine(project.GetProjectDir(), path);

                        if (!force
                            && File.Exists(absolutePath))
                        {
                            throw Error.MigrationsAlreadyEnabled(project.Name);
                        }

                        project.AddFile(path, new TemplateProcessor().Process(template, tokens));

                        if (StartUpProject.TryBuild()
                            && project.TryBuild())
                        {
                            var configurationTypeName = rootNamespace + "." + migrationsNamespace + ".Configuration";

                            using (var facade = GetFacade(configurationTypeName))
                            {
                                WriteLine(Strings.EnableMigrations_BeginInitialScaffold);

                                var scaffoldedMigration
                                    = facade.ScaffoldInitialCreate(project.GetLanguage(), rootNamespace);

                                if (scaffoldedMigration != null)
                                {
                                    if (!enableAutomaticMigrations)
                                    {
                                        new MigrationWriter(this).Write(scaffoldedMigration);

                                        WriteWarning(Strings.EnableMigrations_InitialScaffold(scaffoldedMigration.MigrationId));
                                    }

                                    // We found an initial create so we need to add an explicit ContextKey
                                    // assignment to the configuration

                                    tokens["contextKey"]
                                        = "\r\n            ContextKey = "
                                          + "\"" + qualifiedContextTypeName + "\""
                                          + (!isVb ? ";" : null);

                                    File.WriteAllText(absolutePath, new TemplateProcessor().Process(template, tokens));
                                }
                            }
                        }

                        project.OpenFile(path);

                        WriteLine(Strings.EnableMigrations_Success(project.Name));
                    });
        }

        private string FindContextToEnable(string contextTypeName)
        {
            // We need to load the users assembly in another AppDomain because you can't reload an assembly
            // If the load fails, it will block any further loads of the users assembly in the AppDomain
            // If the load succeeds, the loaded assembly is cached and can't be refreshed if the user changes their code and recompiles
            using (var facade = GetFacade(null, useContextWorkingDirectory: true))
            {
                return facade.GetContextType(contextTypeName);
            }
        }

        private string LoadTemplate(string name)
        {
            DebugCheck.NotEmpty(name);

            var stream = GetType().Assembly.GetManifestResourceStream("System.Data.Entity.Templates." + name);
            Debug.Assert(stream != null);

            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
