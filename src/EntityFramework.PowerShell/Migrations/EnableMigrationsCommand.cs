namespace System.Data.Entity.Migrations
{
    using System.Collections.Generic;
    using System.Data.Entity.Migrations.Extensions;
    using System.Data.Entity.Migrations.Resources;
    using System.Data.Entity.Migrations.Utilities;
    using System.Diagnostics.Contracts;
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
                    var qualifiedContextTypeName = FindContextToEnable(contextTypeName);
                    var isVb = project.CodeModel.Language == CodeModelLanguageConstants.vsCMLanguageVB;
                    var fileName = isVb ? "Configuration.vb" : "Configuration.cs";
                    var template = LoadTemplate(fileName);

                    var tokens = new Dictionary<string, string>();

                    tokens["enableAutomaticMigrations"]
                        = enableAutomaticMigrations
                              ? (isVb ? "True" : "true")
                              : (isVb ? "False" : "false");

                    var rootNamespace = project.GetRootNamespace();
                    tokens["rootnamespace"] = rootNamespace;

                    if (string.IsNullOrWhiteSpace(qualifiedContextTypeName))
                    {
                        tokens["contexttype"]
                            = isVb
                                  ? "[[type name]]"
                                  : "/* TODO: put your Code First context type name here */";

                        if (isVb)
                        {
                            tokens["contexttypecomment"]
                                = "\r\n        'TODO: replace [[type name]] with your Code First context type name";
                        }
                    }
                    else if (isVb && qualifiedContextTypeName.StartsWith(rootNamespace + "."))
                    {
                        tokens["contexttype"] =
                            qualifiedContextTypeName.Substring(rootNamespace.Length + 1).Replace('+', '.');
                    }
                    else
                    {
                        tokens["contexttype"] = qualifiedContextTypeName.Replace('+', '.');
                    }

                    var path = Path.Combine("Migrations", fileName);
                    var absolutePath = Path.Combine(project.GetProjectDir(), path);

                    if (!force
                        && File.Exists(absolutePath))
                    {
                        throw Error.MigrationsAlreadyEnabled(project.Name);
                    }

                    project.AddFile(path, new TemplateProcessor().Process(template, tokens));
                    project.OpenFile(path);

                    if (!enableAutomaticMigrations && StartUpProject.TryBuild()
                        && project.TryBuild())
                    {
                        using (var facade = GetFacade())
                        {
                            WriteLine(Strings.EnableMigrations_BeginInitialScaffold);

                            var scaffoldedMigration
                                = facade.ScaffoldInitialCreate(project.GetLanguage(), project.GetRootNamespace());

                            if (scaffoldedMigration != null)
                            {
                                new MigrationWriter(this).Write(scaffoldedMigration);

                                WriteWarning(
                                    Strings.EnableMigrations_InitialScaffold(scaffoldedMigration.MigrationId));
                            }
                        }
                    }

                    WriteLine(Strings.EnableMigrations_Success(project.Name));
                });
        }

        private string FindContextToEnable(string contextTypeName)
        {
            // We need to load the users assembly in another AppDomain because you can't reload an assembly
            // If the load fails, it will block any further loads of the users assembly in the AppDomain
            // If the load succeeds, the loaded assembly is cached and can't be refreshed if the user changes their code and recompiles
            using (var facade = GetFacade())
            {
                return facade.GetContextType(contextTypeName);
            }
        }

        private string LoadTemplate(string name)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));

            var stream = GetType().Assembly.GetManifestResourceStream("System.Data.Entity.Templates." + name);
            Contract.Assert(stream != null);

            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
