// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace Microsoft.DbContextPackage.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Design;
    using System.Data.Mapping;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using EnvDTE;
    using Microsoft.DbContextPackage.Extensions;
    using Microsoft.DbContextPackage.Resources;
    using Microsoft.DbContextPackage.Utilities;
    using Microsoft.VisualStudio.Shell.Design;
    using Microsoft.VisualStudio.Shell.Interop;
    using VSLangProj;
    using Task = System.Threading.Tasks.Task;

    internal class OptimizeContextHandler
    {
        private readonly DbContextPackage _package;

        public OptimizeContextHandler(DbContextPackage package)
        {
            DebugCheck.NotNull(package);

            _package = package;
        }

        public void OptimizeContext(dynamic context)
        {
            Type contextType = context.GetType();

            try
            {
                var selectedItem = _package.DTE2.SelectedItems.Item(1);
                var selectedItemExtension = (string)selectedItem.ProjectItem.Properties.Item("Extension").Value;
                var languageOption = selectedItemExtension == FileExtensions.CSharp
                    ? LanguageOption.GenerateCSharpCode
                    : LanguageOption.GenerateVBCode;
                var objectContext = DbContextPackage.GetObjectContext(context);
                var baseFileName = contextType.Name;

                if (GetEntityFrameworkVersion(contextType) < new Version(6, 0))
                {
                    var mappingCollection = (StorageMappingItemCollection)objectContext.MetadataWorkspace.GetItemCollection(DataSpace.CSSpace);

                    OptimizeContextEF5(languageOption, baseFileName, mappingCollection, selectedItem);
                }
                else
                {
                    var metadataWorkspace = objectContext.MetadataWorkspace;
                    var getItemCollection = ((Type)metadataWorkspace.GetType()).GetMethod("GetItemCollection");
                    var dataSpace = getItemCollection.GetParameters().First().ParameterType;
                    var mappingCollection = getItemCollection.Invoke(
                        metadataWorkspace,
                        new[] { Enum.Parse(dataSpace, "CSSpace") });

                    OptimizeContextEF6(
                        languageOption,
                        baseFileName,
                        mappingCollection,
                        selectedItem,
                        contextType.FullName);
                }
            }
            catch (Exception ex)
            {
                _package.LogError(Strings.Optimize_ContextError(contextType.Name), ex);
            }
        }

        public void OptimizeEdmx(string inputPath)
        {
            DebugCheck.NotEmpty(inputPath);

            var baseFileName = Path.GetFileNameWithoutExtension(inputPath);

            try
            {
                var selectedItem = _package.DTE2.SelectedItems.Item(1);
                var project = selectedItem.ProjectItem.ContainingProject;
                var languageOption = project.CodeModel.Language == CodeModelLanguageConstants.vsCMLanguageCSharp
                    ? LanguageOption.GenerateCSharpCode
                    : LanguageOption.GenerateVBCode;
                var edmxUtility = new EdmxUtility(inputPath);

                var ef6Reference = ((VSProject)project.Object).References.Cast<Reference>().FirstOrDefault(
                    r => r.Name.EqualsIgnoreCase("EntityFramework")
                        && Version.Parse(r.Version) >= new Version(6, 0)
                        && r.PublicKeyToken.EqualsIgnoreCase("b77a5c561934e089"));

                if (ef6Reference == null)
                {
                    var mappingCollection = edmxUtility.GetMappingCollection();

                    OptimizeContextEF5(languageOption, baseFileName, mappingCollection, selectedItem);
                }
                else
                {
                    var typeService = _package.GetService<DynamicTypeService>();
                    var solution = _package.GetService<SVsSolution, IVsSolution>();
                    IVsHierarchy hierarchy;
                    solution.GetProjectOfUniqueName(project.UniqueName, out hierarchy);
                    var typeResolutionService = typeService.GetTypeResolutionService(hierarchy);
                    var ef6Assembly = typeResolutionService.GetAssembly(
                        new AssemblyName { Name = ef6Reference.Name, Version = new Version(ef6Reference.Version) });

                    if (ef6Assembly != null)
                    {
                        string containerName;
                        var mappingCollection = edmxUtility.GetMappingCollectionEF6(ef6Assembly, out containerName);
                        var contextTypeName = selectedItem.ProjectItem.GetDefaultNamespace() + "." + containerName;

                        OptimizeContextEF6(languageOption, baseFileName, mappingCollection, selectedItem, contextTypeName);
                    }
                    else
                    {
                        var mappingCollection = edmxUtility.GetMappingCollection();

                        OptimizeContextEF5(languageOption, baseFileName, mappingCollection, selectedItem);
                    }

                }
            }
            catch (Exception ex)
            {
                _package.LogError(Strings.Optimize_EdmxError(baseFileName), ex);
            }
        }

        private void OptimizeContextCore(
            LanguageOption languageOption,
            string baseFileName,
            SelectedItem selectedItem,
            Action<string> generateAction)
        {
            DebugCheck.NotEmpty(baseFileName);

            var progressTimer = new Timer { Interval = 1000 };

            try
            {
                var selectedItemPath = (string)selectedItem.ProjectItem.Properties.Item("FullPath").Value;
                var viewsFileName = baseFileName
                    + ".Views"
                    + ((languageOption == LanguageOption.GenerateCSharpCode)
                        ? FileExtensions.CSharp
                        : FileExtensions.VisualBasic);
                var viewsPath = Path.Combine(
                    Path.GetDirectoryName(selectedItemPath),
                    viewsFileName);

                _package.DTE2.SourceControl.CheckOutItemIfNeeded(viewsPath);

                var progress = 1;
                progressTimer.Tick += (sender, e) =>
                    {
                        _package.DTE2.StatusBar.Progress(true, string.Empty, progress, 100);
                        progress = progress == 100 ? 1 : progress + 1;
                        _package.DTE2.StatusBar.Text = Strings.Optimize_Begin(baseFileName);
                    };

                progressTimer.Start();

                Task.Factory.StartNew(
                        () =>
                        {
                            generateAction(viewsPath);
                        })
                    .ContinueWith(
                        t =>
                        {
                            progressTimer.Stop();
                            _package.DTE2.StatusBar.Progress(false);

                            if (t.IsFaulted)
                            {
                                _package.LogError(Strings.Optimize_Error(baseFileName), t.Exception);

                                return;
                            }

                            selectedItem.ProjectItem.ContainingProject.ProjectItems.AddFromFile(viewsPath);
                            _package.DTE2.ItemOperations.OpenFile(viewsPath);

                            _package.DTE2.StatusBar.Text = Strings.Optimize_End(baseFileName, Path.GetFileName(viewsPath));
                        },
                        TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch
            {
                progressTimer.Stop();
                _package.DTE2.StatusBar.Progress(false);

                throw;
            }
        }

        private void OptimizeContextEF5(
            LanguageOption languageOption,
            string baseFileName,
            StorageMappingItemCollection mappingCollection,
            SelectedItem selectedItem)
        {
            DebugCheck.NotEmpty(baseFileName);
            DebugCheck.NotNull(mappingCollection);

            OptimizeContextCore(
                languageOption,
                baseFileName,
                selectedItem,
                viewsPath =>
                {
                    var viewGenerator = new EntityViewGenerator(languageOption);
                    var errors = viewGenerator.GenerateViews(mappingCollection, viewsPath);
                    errors.HandleErrors(Strings.Optimize_SchemaError(baseFileName));
                });
        }

        private void OptimizeContextEF6(
            LanguageOption languageOption,
            string baseFileName,
            dynamic mappingCollection,
            SelectedItem selectedItem,
            string contextTypeName)
        {
            DebugCheck.NotEmpty(baseFileName);

            OptimizeContextCore(
                languageOption,
                baseFileName,
                selectedItem,
                viewsPath =>
                {
                    var edmSchemaError = ((Type)mappingCollection.GetType()).Assembly
                        .GetType("System.Data.Entity.Core.Metadata.Edm.EdmSchemaError", true);
                    var listOfEdmSchemaError = typeof(List<>).MakeGenericType(edmSchemaError);
                    var errors = Activator.CreateInstance(listOfEdmSchemaError);
                    var views = ((Type)mappingCollection.GetType())
                        .GetMethod("GenerateViews", new[] { listOfEdmSchemaError })
                        .Invoke(mappingCollection, new[] { errors });

                    foreach (var error in (IEnumerable<dynamic>)errors)
                    {
                        if ((int)error.Severity == 1)
                        {
                            throw new EdmSchemaErrorException(Strings.Optimize_SchemaError(baseFileName));
                        }
                    }

                    var viewGenerator = languageOption == LanguageOption.GenerateVBCode
                        ? (IViewGenerator)new VBViewGenerator()
                        : new CSharpViewGenerator();
                    viewGenerator.ContextTypeName = contextTypeName;
                    viewGenerator.MappingHashValue = mappingCollection.ComputeMappingHashValue();
                    viewGenerator.Views = views;

                    File.WriteAllText(viewsPath, viewGenerator.TransformText());
                });
        }

        private Version GetEntityFrameworkVersion(Type contextType)
        {
            DebugCheck.NotNull(contextType);

            while (contextType != null
                && contextType.FullName != "System.Data.Entity.DbContext"
                && contextType.Assembly.GetName().Name != "EntityFramework")
            {
                contextType = contextType.BaseType;
            }

            Debug.Assert(contextType != null);

            return contextType.Assembly.GetName().Version;
        }
    }
}