// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.Data.Entity.Design.Package
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Xml;
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.Data.Entity.Design.VisualStudio.SingleFileGenerator;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Modeling.Shell;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using VSLangProj80;
    using VsWebSite;

    internal partial class MicrosoftDataEntityDesignEditorFactory : IVsEditorFactory, IVsEditorFactoryNotify
    {
        private const string CustomTool = "CustomTool";

        #region IVsEditorFactoryNotify Members

        int IVsEditorFactoryNotify.NotifyDependentItemSaved(
            IVsHierarchy pHier, uint itemidParent, string pszMkDocumentParent, uint itemidDependent, string pszMkDocumentDependent)
        {
            return VSConstants.S_OK;
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsHierarchy.GetSite(Microsoft.VisualStudio.OLE.Interop.IServiceProvider@)")]
        int IVsEditorFactoryNotify.NotifyItemAdded(uint grfEFN, IVsHierarchy pHier, uint itemid, string pszMkDocument)
        {
            object o;
            var hr = pHier.GetProperty(itemid, (int)__VSHPROPID.VSHPROPID_ExtObject, out o);

            if (NativeMethods.Succeeded(hr))
            {
                var projectItem = o as ProjectItem;
                if (projectItem != null
                    && VsUtils.EntityFrameworkSupportedInProject(projectItem.ContainingProject, ServiceProvider, allowMiscProject: false))
                {
                    if (EdmUtils.IsDataServicesEdmx(projectItem.get_FileNames(1)))
                    {
                        // if the EDMX has a data services node, don't add the SingleFileGenerator, etc.
                        return VSConstants.S_OK;
                    }

                    IOleServiceProvider oleSP;
                    pHier.GetSite(out oleSP);
                    using (var sp = new ServiceProvider(oleSP))
                    {
                        var appType = VsUtils.GetApplicationType(sp, projectItem.ContainingProject);

                        // set the project item properties
                        SetProjectItemProperties(projectItem, appType);
                    }

                    if (grfEFN != (uint)__EFNFLAGS.EFN_ClonedFromTemplate)
                    {
                        // we're not adding from template i.e. Add Existing Item
                        var referenceFileNames = GetReferencesFromTemplateForProject(projectItem.ContainingProject);
                        AddMissingReferences(projectItem, referenceFileNames);
                        AddBuildProvider(projectItem);
                    }
                }
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        ///     Set properties such as "Custom Tool" and "Item Type"
        /// </summary>
        internal static void SetProjectItemProperties(ProjectItem projectItem, VisualStudioProjectSystem applicationType)
        {
            // set the CustomTool property for the SingleFileGenerator
            EdmUtils.ToggleEdmxItemCustomToolProperty(projectItem, true);

            // set the ItemType property to "EntityDeploy".  This is the "build action" of the item.
            if (applicationType != VisualStudioProjectSystem.Website)
            {
                var prop = projectItem.Properties.Item(ModelObjectItemWizard.ItemTypePropertyName);
                if (prop != null)
                {
                    prop.Value = ModelObjectItemWizard.EntityDeployBuildActionName;
                }
            }
        }

        /// <summary>
        ///     When we "Add Existing Item", VS leaves it up to us to add the missing references in the project.
        /// </summary>
        /// <param name="projectItem">item whose references will be added to the owning project</param>
        /// <param name="referenceFileNames">collection of names to add to project references</param>
        internal static void AddMissingReferences(ProjectItem projectItem, ICollection<string> referenceFileNames)
        {
            if (referenceFileNames != null
                && referenceFileNames.Count != 0)
            {
                if (projectItem.ContainingProject != null
                    && projectItem.ContainingProject.Object != null)
                {
                    var vsProject = projectItem.ContainingProject.Object as VSProject2;
                    var vsWebSite = projectItem.ContainingProject.Object as VSWebSite;
                    if (vsProject != null)
                    {
                        AddMissingReferencesForProject(vsProject, referenceFileNames);
                    }
                    else if (vsWebSite != null)
                    {
                        AddMissingReferencesForWebsite(vsWebSite, referenceFileNames);
                    }
                    else
                    {
                        Debug.Fail("Could not resolve the project type to add references.");
                    }
                }
            }
        }

        /// <summary>
        ///     Registers our build provider for items added via "Add Existing Item" in WebSite projects
        /// </summary>
        internal static void AddBuildProvider(ProjectItem projectItem)
        {
            if (projectItem.ContainingProject.Object is VSWebSite)
            {
                VsUtils.RegisterBuildProviders(projectItem.ContainingProject);
            }
        }

        private static void AddMissingReferencesForProject(VSProject2 standardProject, ICollection<string> referenceFileNames)
        {
            foreach (var referenceFileName in referenceFileNames)
            {
                if (standardProject.References.Find(referenceFileName) == null)
                {
                    standardProject.References.Add(referenceFileName);
                }
            }
        }

        private static void AddMissingReferencesForWebsite(VSWebSite webSiteProject, ICollection<string> referenceFileNames)
        {
            // first convert the enumerable into a hash for quick lookup
            var websiteReferenceHash = new HashSet<string>();
            var websiteReferenceEnumerator = webSiteProject.References.GetEnumerator();
            var netRefPath = EdmUtils.GetRuntimeAssemblyPath(webSiteProject.Project, Services.ServiceProvider);

            while (websiteReferenceEnumerator.MoveNext())
            {
                var assemblyReference = websiteReferenceEnumerator.Current as AssemblyReference;
                if (assemblyReference != null)
                {
                    websiteReferenceHash.Add(assemblyReference.Name);
                }
            }

            foreach (var referenceFileName in referenceFileNames)
            {
                if (!websiteReferenceHash.Contains(referenceFileName))
                {
                    // first try the GAC
                    try
                    {
                        webSiteProject.References.AddFromGAC(referenceFileName);
                    }
                    catch (FileNotFoundException)
                    {
                        // attempt to add the file from the net framework directory.
                        // TODO: We should check OOB DataFx Installation folder first before looking at .net framework folder.
                        // Tracked by bug: 740496
                        try
                        {
                            webSiteProject.References.AddFromFile(Path.Combine(netRefPath, referenceFileName));
                        }
                        catch (FileNotFoundException)
                        {
                            // can't do anything else; leave it up to the build process to pick up the
                            // errors and encourage the user to add the references manually.
                        }
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static ICollection<string> GetReferencesFromTemplateForProject(Project project)
        {
            ICollection<string> referenceFileNames = new List<string>();
            var solution2 = project.DTE.Solution as Solution2;
            if (null != solution2)
            {
                // get the itemTemplate vstemplate zip file name based on the type of project and language
                string itemTemplateZipFile = null;
                var projectKind = VsUtils.GetProjectKind(project);
                if (projectKind == VsUtils.ProjectKind.CSharp)
                {
                    itemTemplateZipFile = EntityDesigner.Utils.Constants.AdoNetEntityDataModelCSharp;
                }
                else if (projectKind == VsUtils.ProjectKind.VB)
                {
                    itemTemplateZipFile = EntityDesigner.Utils.Constants.AdoNetEntityDataModelVB;
                }
                else if (projectKind == VsUtils.ProjectKind.Web)
                {
                    if (VsUtils.IsWebSiteVBProject(project))
                    {
                        itemTemplateZipFile = EntityDesigner.Utils.Constants.AdoNetEntityDataModelAspNetVB;
                    }
                    else if (VsUtils.IsWebSiteCSharpProject(project))
                    {
                        itemTemplateZipFile = EntityDesigner.Utils.Constants.AdoNetEntityDataModelAspNetCSharp;
                    }
                }

                // use the solution to get the path of the item template, look at the XML, and find all references.
                if (itemTemplateZipFile != null)
                {
                    var itemTemplatePath = solution2.GetProjectItemTemplate(itemTemplateZipFile, project.Kind);
                    if (!String.IsNullOrEmpty(itemTemplatePath))
                    {
                        try
                        {
                            var xmlDocument = EdmUtils.SafeLoadXmlFromPath(itemTemplatePath);
                            var nsmgr = new XmlNamespaceManager(xmlDocument.NameTable);
                            nsmgr.AddNamespace("vst", "http://schemas.microsoft.com/developer/vstemplate/2005");
                            var referenceNodeList =
                                xmlDocument.SelectNodes(
                                    "/vst:VSTemplate/vst:TemplateContent/vst:References/vst:Reference/vst:Assembly", nsmgr);
                            foreach (XmlElement referenceNode in referenceNodeList)
                            {
                                referenceFileNames.Add(referenceNode.InnerText);
                            }
                        }
                        catch
                        {
                            // leave it up to the build process to pick up the errors
                        }
                    }
                }
            }

            return referenceFileNames;
        }

        int IVsEditorFactoryNotify.NotifyItemRenamed(IVsHierarchy pHier, uint itemid, string pszMkDocumentOld, string pszMkDocumentNew)
        {
            // we need to check that this file has the "custom tool" property
            var projectItem = VsUtils.GetProjectItem(pHier, itemid);
            if (projectItem != null
                && projectItem.Properties != null
                && projectItem.Properties.Item(CustomTool) != null
                && (string)(projectItem.Properties.Item(CustomTool).Value) == EntityModelCodeGenerator.CodeGenToolName)
            {
                EntityModelCodeGenerator.AddNameOfItemToBeRenamed(itemid, pszMkDocumentOld);
            }
            return VSConstants.S_OK;
        }

        #endregion

        //
        // implement IVsEditorFactor.CreateEditorInstance here.  This is necessary to have this method invoked, 
        // since CreateEditorInstance in the base-class isn't virtual
        //
        int IVsEditorFactory.CreateEditorInstance(
            uint createFlags,
            string fileName,
            string physicalView,
            IVsHierarchy hierarchy,
            uint itemId,
            IntPtr existingDocData,
            out IntPtr docView,
            out IntPtr docData,
            out string editorCaption,
            out Guid cmdUI,
            out int createDocWinFlags)
        {
            docData = IntPtr.Zero;
            docView = IntPtr.Zero;
            editorCaption = null;
            cmdUI = Guid.Empty;
            createDocWinFlags = 0;

            var hr = base.CreateEditorInstance(
                createFlags, fileName, physicalView, hierarchy, itemId, existingDocData, out docView, out docData, out editorCaption,
                out cmdUI, out createDocWinFlags);
            return hr;
        }

        /// <summary>
        ///     This method returns the physical view identifier.
        ///     If Escher logical view is passed in the parameter, the viewContext should contain diagram id information.
        ///     We set the physical view identifier to be equal to diagram id because we want diagram to have its own docview.
        /// </summary>
        /// <param name="logicalView"></param>
        /// <param name="viewContext"></param>
        /// <returns></returns>
        protected override string MapLogicalView(Guid logicalView, object viewContext)
        {
            if (logicalView == PackageConstants.guidLogicalView
                && viewContext != null)
            {
                Debug.Assert(
                    viewContext is String && !String.IsNullOrEmpty(viewContext.ToString()),
                    "EditorFactory expects diagram id information in viewContext parameter.");
                return viewContext.ToString();
            }
            return base.MapLogicalView(logicalView, viewContext);
        }

        protected override ModelingDocView CreateDocView(ModelingDocData docData, string physicalView, out string editorCaption)
        {
            // Set EditorCaption to be null here because we do not want EditorFactory to update the value.
            // We will manually set the EditorCaption when the View is loaded (see code in MicrosoftDataEntityDesignDocView.cs).
            editorCaption = null;
            return new MicrosoftDataEntityDesignDocView(docData, ServiceProvider, physicalView);
        }
    }
}
