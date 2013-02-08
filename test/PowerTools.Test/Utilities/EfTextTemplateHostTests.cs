// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace Microsoft.DbContextPackage.Utilities
{
    using System;
    using System.CodeDom.Compiler;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization.Formatters.Binary;
    using Microsoft.VisualStudio.TextTemplating;
    using Xunit;

    public class EfTextTemplateHostTests
    {
        public class ResolveAssemblyReference
        {
            [Fact]
            public void Resolves_assembly_locations()
            {
                var host = new EfTextTemplateHost();
                var assemblyLocation = typeof(Type).Assembly.Location;

                var resolvedReference = host.ResolveAssemblyReference(
                    assemblyLocation);

                Assert.True(File.Exists(resolvedReference));
                Assert.Equal("mscorlib.dll", Path.GetFileName(resolvedReference));
            }

            [Fact]
            public void Resolves_full_assembly_names()
            {
                var host = new EfTextTemplateHost();
                var assemblyName = typeof(Type).Assembly.GetName();

                var resolvedReference = host.ResolveAssemblyReference(
                    assemblyName.FullName);

                Assert.True(File.Exists(resolvedReference));
                Assert.Equal("mscorlib.dll", Path.GetFileName(resolvedReference));
            }

            [Fact]
            public void Resolves_simple_assembly_names()
            {
                var host = new EfTextTemplateHost();
                var assemblyName = typeof(Type).Assembly.GetName();

                var resolvedReference = host.ResolveAssemblyReference(
                    assemblyName.Name);

                Assert.True(File.Exists(resolvedReference));
                Assert.Equal("mscorlib.dll", Path.GetFileName(resolvedReference));
            }
        }

        [Fact]
        public void StandardAssemblyReferences_includes_basic_references()
        {
            ITextTemplatingEngineHost host = new EfTextTemplateHost();
            var powerToolsAssemblyName = typeof(EfTextTemplateHost)
                .Assembly
                .GetName()
                .Name;

            var references = host.StandardAssemblyReferences
                .Select(r => Path.GetFileNameWithoutExtension(r))
                .ToArray();

            Assert.Contains(powerToolsAssemblyName, references);
            Assert.Contains("System", references);
            Assert.Contains("System.Core", references);
            Assert.Contains("System.Data.Entity", references);
        }

        [Fact]
        public void StandardImports_includes_basic_imports()
        {
            ITextTemplatingEngineHost host = new EfTextTemplateHost();
            var hostNamespace = typeof(EfTextTemplateHost).Namespace;

            var imports = host.StandardImports;

            Assert.Contains("System", imports);
            Assert.Contains(hostNamespace, imports);
        }

        [Fact]
        public void LogErrors_sets_Errors()
        {
            var efHost = new EfTextTemplateHost();
            var host = (ITextTemplatingEngineHost)efHost;
            var errors = new CompilerErrorCollection();

            host.LogErrors(errors);

            Assert.Same(errors, efHost.Errors);
        }

        public class ResolvePath
        {
            [Fact]
            public void Resolves_absolute_paths()
            {
                const string path = @"C:\File.ext";
                ITextTemplatingEngineHost host = new EfTextTemplateHost();

                var resolvedPath = host.ResolvePath(path);

                Assert.Equal(path, resolvedPath);
            }

            [Fact]
            public void Resolves_relative_paths_when_TemplateFile_absolute()
            {
                ITextTemplatingEngineHost host = new EfTextTemplateHost
                    {
                        TemplateFile = @"C:\Template.tt"
                    };

                var resolvedPath = host.ResolvePath("File.ext");

                Assert.Equal(@"C:\File.ext", resolvedPath);
            }

            [Fact]
            public void Returns_original_path_when_unresolvable()
            {
                const string path = "File.ext";
                ITextTemplatingEngineHost host = new EfTextTemplateHost();

                var resolvedPath = host.ResolvePath(path);

                Assert.Equal(path, resolvedPath);
            }
        }

        [Fact]
        public void SetFileExtension_sets_FileExtension()
        {
            const string extension = ".ext";
            var efHost = new EfTextTemplateHost();
            var host = (ITextTemplatingEngineHost)efHost;

            host.SetFileExtension(extension);

            Assert.Equal(extension, efHost.FileExtension);
        }
    }
}
