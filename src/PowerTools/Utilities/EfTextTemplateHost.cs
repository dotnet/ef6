// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace Microsoft.DbContextPackage.Utilities
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Data.Metadata.Edm;
    using System.Data.Objects;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Microsoft.DbContextPackage.Resources;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TextTemplating;

    public class EfTextTemplateHost : ITextTemplatingEngineHost
    {
        public EntityType EntityType { get; set; }
        public EntityContainer EntityContainer { get; set; }
        public string Namespace { get; set; }
        public string ModelsNamespace { get; set; }
        public string MappingNamespace { get; set; }
        public Version EntityFrameworkVersion { get; set; }
        public EntitySet TableSet { get; set; }
        public Dictionary<EdmProperty, EdmProperty> PropertyToColumnMappings { get; set; }
        public Dictionary<AssociationType, Tuple<EntitySet, Dictionary<RelationshipEndMember, Dictionary<EdmMember, string>>>> ManyToManyMappings { get; set; }

        #region T4 plumbing

        public CompilerErrorCollection Errors { get; set; }
        public string FileExtension { get; set; }
        public Encoding OutputEncoding { get; set; }
        public string TemplateFile { get; set; }

        public virtual string ResolveAssemblyReference(string assemblyReference)
        {
            if (File.Exists(assemblyReference))
            {
                return assemblyReference;
            }

            try
            {
                // TODO: This is failing to resolve partial assembly names (e.g. "System.Xml")
                var assembly = Assembly.Load(assemblyReference);

                if (assembly != null)
                {
                    return assembly.Location;
                }
            }
            catch (FileNotFoundException)
            {
            }
            catch (FileLoadException)
            {
            }
            catch (BadImageFormatException)
            {
            }

            return string.Empty;
        }

        IList<string> ITextTemplatingEngineHost.StandardAssemblyReferences
        {
            get
            {
                return new[]
                    {
                        Assembly.GetExecutingAssembly().Location,
                        typeof(Uri).Assembly.Location,
                        typeof(Enumerable).Assembly.Location,
                        typeof(ObjectContext).Assembly.Location,

                        //       Because of the issue in ResolveAssemblyReference, these are not being
                        //       loaded but are required by the default templates
                        typeof(System.Data.AcceptRejectRule).Assembly.Location,
                        typeof(System.Data.Entity.Design.EdmToObjectNamespaceMap).Assembly.Location,
                        typeof(System.Xml.ConformanceLevel).Assembly.Location,
                        typeof(System.Xml.Linq.Extensions).Assembly.Location,
                        typeof(EnvDTE._BuildEvents).Assembly.Location
                    };
            }
        }

        IList<string> ITextTemplatingEngineHost.StandardImports
        {
            get
            {
                return new[]
                    {
                        "System",
                        "Microsoft.DbContextPackage.Utilities"
                    };
            }
        }

        object ITextTemplatingEngineHost.GetHostOption(string optionName)
        {
            if (optionName == "CacheAssemblies")
            {
                return 1;
            }

            return null;
        }

        bool ITextTemplatingEngineHost.LoadIncludeText(string requestFileName, out string content, out string location)
        {
            location = ((ITextTemplatingEngineHost)this).ResolvePath(requestFileName);

            if (File.Exists(location))
            {
                content = File.ReadAllText(location);

                return true;
            }

            using (var rootKey = VSRegistry.RegistryRoot(__VsLocalRegistryType.RegType_Configuration))
            using (var includeFoldersKey = rootKey.OpenSubKey(@"TextTemplating\IncludeFolders\.tt"))
            {
                foreach (var valueName in includeFoldersKey.GetValueNames())
                {
                    var includeFolder = includeFoldersKey.GetValue(valueName) as string;

                    if (includeFolder == null)
                    {
                        continue;
                    }

                    location = Path.Combine(includeFolder, requestFileName);

                    if (File.Exists(location))
                    {
                        content = File.ReadAllText(location);

                        // Our implementation doesn't require respecting the CleanupBehavior custom directive, and since
                        // implementing a fallback custom directive processor would essencially force us to have two
                        // different versions of the EF Power Tools (one for VS 2010, another one for VS 2012) the simplest
                        // solution is to remove the custom directive from the in-memory copy of the ttinclude
                        content = content.Replace(@"<#@ CleanupBehavior Processor=""T4VSHost"" CleanupAfterProcessingTemplate=""true"" #>", "");

                        return true;
                    }
                }
            }

            location = string.Empty;
            content = string.Empty;

            return false;
        }

        void ITextTemplatingEngineHost.LogErrors(CompilerErrorCollection errors)
        {
            Errors = errors;
        }

        AppDomain ITextTemplatingEngineHost.ProvideTemplatingAppDomain(string content)
        {
            return AppDomain.CurrentDomain;
        }

        Type ITextTemplatingEngineHost.ResolveDirectiveProcessor(string processorName)
        {
            throw Error.UnknownDirectiveProcessor(processorName);
        }

        string ITextTemplatingEngineHost.ResolveParameterValue(string directiveId, string processorName, string parameterName)
        {
            return string.Empty;
        }

        string ITextTemplatingEngineHost.ResolvePath(string path)
        {
            if (!Path.IsPathRooted(path) && Path.IsPathRooted(TemplateFile))
            {
                return Path.Combine(Path.GetDirectoryName(TemplateFile), path);
            }

            return path;
        }

        void ITextTemplatingEngineHost.SetFileExtension(string extension)
        {
            FileExtension = extension;
        }

        void ITextTemplatingEngineHost.SetOutputEncoding(Encoding encoding, bool fromOutputDirective)
        {
            OutputEncoding = encoding;
        }

        #endregion
    }
}
