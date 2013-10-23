// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace EFDesigner.InProcTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using EFDesignerTestInfrastructure.VS;
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.Data.Entity.Design.Package;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.VisualStudio.TestTools.VsIdeTesting;

    [TestClass]
    public class AutomaticDbContextTests
    {
        private IEdmPackage _package;

        public AutomaticDbContextTests()
        {
            PackageManager.LoadEDMPackage(VsIdeTestHostContext.ServiceProvider);
            _package = PackageManager.Package;
        }

        public TestContext TestContext { get; set; }

        private static DTE Dte
        {
            get { return VsIdeTestHostContext.Dte; }
        }

        private string ModelEdmxFilePath
        {
            get { return Path.Combine(TestContext.DeploymentDirectory, @"TestData\Model\v3\Simple.edmx"); }
        }

        [ClassCleanup]
        public static void ClassCleanUp()
        {
            Dte.Solution.Close();
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void AddDbContextTemplates_does_not_add_the_template_items_when_the_templates_are_not_installed()
        {
            UITestRunner.Execute(
                () =>
                    {
                        var project = CreateProject("NoDbContextTemplates", "3.5", "VisualBasic");
                        project.ProjectItems.AddFromFileCopy(ModelEdmxFilePath);
                        var edmxItem = project.ProjectItems.OfType<ProjectItem>().Single(i => i.Name == "Simple.edmx");

                        new DbContextCodeGenerator("FakeDbCtx{0}{1}EF5.zip").AddDbContextTemplates(edmxItem);

                        Assert.IsFalse(project.ProjectItems.OfType<ProjectItem>().Any(i => i.Name == "Simple.tt"));
                        Assert.IsFalse(project.ProjectItems.OfType<ProjectItem>().Any(i => i.Name == "Simple.Context.tt"));
                    });
        }

        // This test requires the EF 5.x DbContext item templates to be installed.
        [TestMethod]
        [HostType("VS IDE")]
        public void FindDbContextTemplate_returns_null_for_project_targeting_dotNET3_5()
        {
            UITestRunner.Execute(
                () => { Assert.IsNull(new DbContextCodeGenerator().FindDbContextTemplate(CreateProject("Net35", "3.5", "CSharp"))); });
        }

        // This test requires the EF 5.x DbContext item templates to be installed.
        [TestMethod]
        [HostType("VS IDE")]
        public void FindDbContextTemplate_finds_the_EF5_CSharp_template_when_targeting_dotNET4_with_CSharp()
        {
            UITestRunner.Execute(
                () =>
                    {
                        var proj = CreateProject("DbContextCSharpNet40", "4", "CSharp");
                        var dbCtxGenerator = new DbContextCodeGenerator();
                        var ctxTemplate = dbCtxGenerator.FindDbContextTemplate(proj);
                        Assert.IsTrue(ctxTemplate.EndsWith(@"DbCtxCSEF5\DbContext_CS_V5.0.vstemplate"));
                    });
        }

        // This test requires the EF 5.x DbContext item templates to be installed.
        [TestMethod]
        [HostType("VS IDE")]
        public void FindDbContextTemplate_finds_the_EF5_CSharp_template_when_targeting_dotNET4_5_with_CSharp()
        {
            UITestRunner.Execute(
                () =>
                    {
                        var proj = CreateProject("DbContextCSharpNet45", "4.5", "CSharp");
                        var dbCtxGenerator = new DbContextCodeGenerator();
                        var ctxTemplate = dbCtxGenerator.FindDbContextTemplate(proj);
                        Assert.IsTrue(ctxTemplate.EndsWith(@"DbCtxCSEF5\DbContext_CS_V5.0.vstemplate"));
                    });
        }

        // This test requires the EF 5.x DbContext item templates to be installed.
        [TestMethod]
        [HostType("VS IDE")]
        public void FindDbContextTemplate_finds_the_EF5_VB_template_when_targeting_dotNET4_with_VB()
        {
            UITestRunner.Execute(
                () =>
                    {
                        var proj = CreateProject("DbContextVBNet40", "4", "VisualBasic");
                        var dbCtxGenerator = new DbContextCodeGenerator();
                        var ctxTemplate = dbCtxGenerator.FindDbContextTemplate(proj);
                        Assert.IsTrue(ctxTemplate.EndsWith(@"DbCtxVBEF5\DbContext_VB_V5.0.vstemplate"));
                    });
        }

        // This test requires the EF 5.x DbContext item templates to be installed.
        [TestMethod]
        [HostType("VS IDE")]
        public void FindDbContextTemplate_finds_the_EF5_VB_template_when_targeting_dotNET4_5_with_VB()
        {
            UITestRunner.Execute(
                () =>
                    {
                        var proj = CreateProject("DbContextVBNet45", "4.5", "VisualBasic");
                        var dbCtxGenerator = new DbContextCodeGenerator();
                        var ctxTemplate = dbCtxGenerator.FindDbContextTemplate(proj);
                        Assert.IsTrue(ctxTemplate.EndsWith(@"DbCtxVBEF5\DbContext_VB_V5.0.vstemplate"));
                    });
        }

        // This test requires the EF 5.x DbContext item templates to be installed.
        [TestMethod]
        [HostType("VS IDE")]
        public void FindDbContextTemplate_finds_the_EF5_CSharp_web_site_template_when_targeting_dotNET4_web_site_with_CSharp()
        {
            UITestRunner.Execute(
                () =>
                    {
                        var proj = CreateWebSiteProject("DbContextCSharpNet40Web", "4", "CSharp");
                        var dbCtxGenerator = new DbContextCodeGenerator();
                        var ctxTemplate = dbCtxGenerator.FindDbContextTemplate(proj);
                        Assert.IsTrue(ctxTemplate.EndsWith(@"DbCtxCSWSEF5\DbContext_CS_WS_V5.0.vstemplate"));
                    });
        }

        // This test requires the EF 5.x DbContext item templates to be installed.
        [TestMethod]
        [HostType("VS IDE")]
        public void FindDbContextTemplate_finds_the_EF5_CSharp_web_site_template_when_targeting_dotNET4_5_web_site_with_CSharp()
        {
            UITestRunner.Execute(
                () =>
                    {
                        var proj = CreateWebSiteProject("DbContextCSharpNet45Web", "4.5", "CSharp");
                        var dbCtxGenerator = new DbContextCodeGenerator();
                        var ctxTemplate = dbCtxGenerator.FindDbContextTemplate(proj);
                        Assert.IsTrue(ctxTemplate.EndsWith(@"DbCtxCSWSEF5\DbContext_CS_WS_V5.0.vstemplate"));
                    });
        }

        // This test requires the EF 5.x DbContext item templates to be installed.
        [TestMethod]
        [HostType("VS IDE")]
        public void FindDbContextTemplate_finds_the_EF5_VB_web_site_template_when_targeting_dotNET4_web_site_with_VB()
        {
            UITestRunner.Execute(
                () =>
                    {
                        var proj = CreateWebSiteProject("DbContextVBNet40Web", "4", "VisualBasic");
                        var dbCtxGenerator = new DbContextCodeGenerator();
                        var ctxTemplate = dbCtxGenerator.FindDbContextTemplate(proj);
                        Assert.IsTrue(ctxTemplate.EndsWith(@"DbCtxVBWSEF5\DbContext_VB_WS_V5.0.vstemplate"));
                    });
        }

        // This test requires the EF 5.x DbContext item templates to be installed.
        [TestMethod]
        [HostType("VS IDE")]
        public void FindDbContextTemplate_finds_the_EF5_VB_web_site_template_when_targeting_dotNET4_5_web_site_with_VB()
        {
            UITestRunner.Execute(
                () =>
                    {
                        var proj = CreateWebSiteProject("DbContextVBNet45Web", "4.5", "VisualBasic");
                        var dbCtxGenerator = new DbContextCodeGenerator();
                        var ctxTemplate = dbCtxGenerator.FindDbContextTemplate(proj);
                        Assert.IsTrue(ctxTemplate.EndsWith(@"DbCtxVBWSEF5\DbContext_VB_WS_V5.0.vstemplate"));
                    });
        }

        // This test requires the EF 6.x DbContext item templates to be installed.
        [TestMethod]
        [HostType("VS IDE")]
        public void FindDbContextTemplate_finds_the_EF6_CSharp_template()
        {
            UITestRunner.Execute(
                () =>
                    {
                        var project = CreateProject("DbContextCSharpNet45EF6", "4.5", "CSharp");
                        var generator = new DbContextCodeGenerator();

                        var template = generator.FindDbContextTemplate(project, useLegacyTemplate: false);

                        StringAssert.EndsWith(template, @"DbCtxCSEF6\DbContext_CS_V6.0.vstemplate");
                    });
        }

        // This test requires the EF 6.x DbContext item templates to be installed.
        [TestMethod]
        [HostType("VS IDE")]
        public void FindDbContextTemplate_finds_the_EF6_VB_template()
        {
            UITestRunner.Execute(
                () =>
                    {
                        var project = CreateProject("DbContextVBNet45EF6", "4.5", "VisualBasic");
                        var generator = new DbContextCodeGenerator();

                        var template = generator.FindDbContextTemplate(project, useLegacyTemplate: false);

                        StringAssert.EndsWith(template, @"DbCtxVBEF6\DbContext_VB_V6.0.vstemplate");
                    });
        }

        // This test requires the EF 6.x DbContext item templates to be installed.
        [TestMethod]
        [HostType("VS IDE")]
        public void FindDbContextTemplate_finds_the_EF6_CSharp_web_site_template()
        {
            UITestRunner.Execute(
                () =>
                    {
                        var project = CreateWebSiteProject("DbContextCSharpNet45WebEF6", "4.5", "CSharp");
                        var generator = new DbContextCodeGenerator();

                        var template = generator.FindDbContextTemplate(project, useLegacyTemplate: false);

                        StringAssert.EndsWith(template, @"DbCtxCSWSEF6\DbContext_CS_WS_V6.0.vstemplate");
                    });
        }

        // This test requires the EF 6.x DbContext item templates to be installed.
        [TestMethod]
        [HostType("VS IDE")]
        public void FindDbContextTemplate_finds_the_EF6_VB_web_site_template()
        {
            UITestRunner.Execute(
                () =>
                    {
                        var project = CreateWebSiteProject("DbContextVBNet45WebEF6", "4.5", "VisualBasic");
                        var generator = new DbContextCodeGenerator();

                        var template = generator.FindDbContextTemplate(project, useLegacyTemplate: false);

                        StringAssert.EndsWith(template, @"DbCtxVBWSEF6\DbContext_VB_WS_V6.0.vstemplate");
                    });
        }

        // This test requires the EF 5.x DbContext item templates to be installed.
        [TestMethod]
        [HostType("VS IDE")]
        public void AddDbContextTemplates_does_not_add_the_template_items_to_the_item_collection_when_targeting_dotNET3_5()
        {
            UITestRunner.Execute(
                () =>
                    {
                        var project = CreateProject("TemplatesNet35", "3.5", "VisualBasic");
                        var edmxItem = project.ProjectItems.AddFromFileCopy(ModelEdmxFilePath);

                        new DbContextCodeGenerator().AddDbContextTemplates(edmxItem);

                        Assert.IsFalse(project.ProjectItems.OfType<ProjectItem>().Any(i => i.Name == "Simple.tt"));
                        Assert.IsFalse(project.ProjectItems.OfType<ProjectItem>().Any(i => i.Name == "Simple.Context.tt"));
                    });
        }

        // This test requires the EF 5.x DbContext item templates to be installed.
        [TestMethod]
        [HostType("VS IDE")]
        public void AddDbContextTemplates_adds_the_template_items_nested_under_the_EDMX_item()
        {
            UITestRunner.Execute(
                () =>
                    {
                        var project = CreateProject("TemplatesNet40", "4", "CSharp");
                        var edmxItem = project.ProjectItems.AddFromFileCopy(ModelEdmxFilePath);
                        edmxItem.Open();

                        new DbContextCodeGenerator().AddDbContextTemplates(edmxItem);

                        var typesT4 = edmxItem.ProjectItems.OfType<ProjectItem>().Single(i => i.Name == "Simple.tt");
                        Assert.IsTrue(typesT4.get_FileNames(1).EndsWith(@"TemplatesNet40\Simple.tt"));

                        var contextT4 = edmxItem.ProjectItems.OfType<ProjectItem>().Single(i => i.Name == "Simple.Context.tt");
                        Assert.IsTrue(contextT4.get_FileNames(1).EndsWith(@"TemplatesNet40\Simple.Context.tt"));
                    });
        }

        // This test requires the EF 5.x DbContext item templates to be installed.
        [TestMethod]
        [HostType("VS IDE")]
        public void AddDbContextTemplates_does_not_nest_existing_tt_files_or_non_tt_files_added_at_the_same_time_as_the_template_items()
        {
            UITestRunner.Execute(
                () =>
                    {
                        var project = CreateProject("TemplatesNet40_Nesting", "4", "CSharp");
                        project.ProjectItems.AddFromTemplate(
                            ((Solution2)Dte.Solution).GetProjectItemTemplate("XMLFile.zip", "CSharp"), "another.tt");
                        var edmxItem = project.ProjectItems.AddFromFileCopy(ModelEdmxFilePath);
                        edmxItem.Open();

                        new DbContextCodeGenerator().AddDbContextTemplates(edmxItem);

                        Assert.IsFalse(edmxItem.ProjectItems.OfType<ProjectItem>().Any(i => i.Name == "another.tt"));
                        Assert.IsFalse(edmxItem.ProjectItems.OfType<ProjectItem>().Any(i => i.Name == "packages.config"));

                        var additional = project.ProjectItems.OfType<ProjectItem>().Single(i => i.Name == "packages.config");
                        Assert.IsTrue(additional.get_FileNames(1).EndsWith(@"TemplatesNet40_Nesting\packages.config"));
                    });
        }

        // This test requires the EF 5.x DbContext item templates to be installed.
        [TestMethod]
        [HostType("VS IDE")]
        public void AddDbContextTemplates_adds_the_template_items_to_the_item_collection_for_a_website_project()
        {
            UITestRunner.Execute(
                () =>
                    {
                        var project = CreateWebSiteProject("TemplatesNet45Web", "4.5", "CSharp");
                        var appCode = project.ProjectItems.AddFolder("App_Code");
                        var edmxItem = appCode.ProjectItems.AddFromFileCopy(ModelEdmxFilePath);
                        edmxItem.Open();

                        new DbContextCodeGenerator().AddDbContextTemplates(edmxItem);

                        var typesT4 = appCode.ProjectItems.OfType<ProjectItem>().Single(i => i.Name == "Simple.tt");
                        Assert.IsTrue(typesT4.get_FileNames(1).EndsWith(@"TemplatesNet45Web\App_Code\Simple.tt"));

                        var contextT4 = appCode.ProjectItems.OfType<ProjectItem>().Single(i => i.Name == "Simple.Context.tt");
                        Assert.IsTrue(contextT4.get_FileNames(1).EndsWith(@"TemplatesNet45Web\App_Code\Simple.Context.tt"));
                    });
        }

        // This test requires the EF 6.x DbContext item templates to be installed.
        [TestMethod]
        [HostType("VS IDE")]
        public void AddDbContextTemplates_is_noop_when_called_more_than_once()
        {
            UITestRunner.Execute(
                () =>
                    {
                        var project = CreateProject("TemplatesNet45Twice", "4.5", "CSharp");
                        var edmxItem = project.ProjectItems.AddFromFileCopy(ModelEdmxFilePath);
                        edmxItem.Open();

                        new DbContextCodeGenerator().AddDbContextTemplates(edmxItem, useLegacyTemplate: false);
                        new DbContextCodeGenerator().AddDbContextTemplates(edmxItem, useLegacyTemplate: false);

                        CollectionAssert.IsSubsetOf(
                            new[] { "Simple.tt", "Simple.Context.tt" },
                            (edmxItem.ProjectItems ?? edmxItem.Collection).Cast<ProjectItem>().Select(i => i.Name).ToArray());
                    });
        }

        // This test requires the EF 6.x DbContext item templates to be installed.
        [TestMethod]
        [HostType("VS IDE")]
        public void AddDbContextTemplates_is_noop_when_called_more_than_once_and_website()
        {
            UITestRunner.Execute(
                () =>
                    {
                        var project = CreateWebSiteProject("TemplatesNet45TwiceWeb", "4.5", "CSharp");
                        var appCode = project.ProjectItems.AddFolder("App_Code");
                        var edmxItem = appCode.ProjectItems.AddFromFileCopy(ModelEdmxFilePath);
                        edmxItem.Open();

                        new DbContextCodeGenerator().AddDbContextTemplates(edmxItem, useLegacyTemplate: false);
                        new DbContextCodeGenerator().AddDbContextTemplates(edmxItem, useLegacyTemplate: false);

                        CollectionAssert.IsSubsetOf(
                            new[] { "Simple.tt", "Simple.Context.tt" },
                            appCode.ProjectItems.Cast<ProjectItem>().Select(i => i.Name).ToArray());
                    });
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void AddAndNestCodeGenTemplates_does_not_fail_if_EDMX_project_item_is_null()
        {
            var run = false;
            DbContextCodeGenerator.AddAndNestCodeGenTemplates(null, () => run = true);
            Assert.IsTrue(run);
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void AddNewItemDialogFilter_only_accepts_items_with_file_names_for_well_known_template_types()
        {
            var _ = new Guid();
            int filterResult;
            var filter = new MicrosoftDataEntityDesignCommandSet.AddNewItemDialogFilter();

            new List<string>
                {
                    @"C:\Some Templates\ADONETArtifactGenerator_OldSchool.vstemplate",
                    @"C:\Some Templates\DbContext_InTheBox.vstemplate",
                }.ForEach(
                    f =>
                        {
                            Assert.AreEqual(VSConstants.S_OK, filter.FilterListItemByTemplateFile(ref _, f, out filterResult));
                            Assert.AreEqual(0, filterResult);
                        });

            Assert.AreEqual(
                VSConstants.S_OK,
                filter.FilterListItemByTemplateFile(ref _, @"C:\Some Templates\Not An EF Template.vstemplate", out filterResult));
            Assert.AreEqual(1, filterResult);
        }

        private Project CreateProject(string projectName, string targetFramework, string projectLanguage)
        {
            return CreateProject(
                projectName,
                () =>
                ((Solution2)Dte.Solution).GetProjectTemplate("ConsoleApplication.zip|FrameworkVersion=" + targetFramework, projectLanguage));
        }

        private Project CreateWebSiteProject(string projectName, string targetFramework, string projectLanguage)
        {
            return CreateProject(
                projectName,
                () =>
                ((Solution2)Dte.Solution).GetProjectTemplate("EmptyWeb.zip|FrameworkVersion=" + targetFramework, @"Web\" + projectLanguage));
        }

        private Project CreateProject(string projectName, Func<string> getTemplate)
        {
            var solutionPath = Directory.GetParent(TestContext.DeploymentDirectory).FullName;

            if (!Dte.Solution.IsOpen)
            {
                Dte.Solution.Create(solutionPath, "AutomaticDbContextTests");
            }

            var projectDir = Path.Combine(solutionPath, projectName);
            if (Directory.Exists(projectDir))
            {
                Directory.Delete(projectDir, true);
            }

            Dte.Solution.AddFromTemplate(getTemplate(), projectDir, projectName, false);

            return Dte.Solution.Projects.OfType<Project>().First(p => p.Name.Contains(projectName));
        }
    }
}
