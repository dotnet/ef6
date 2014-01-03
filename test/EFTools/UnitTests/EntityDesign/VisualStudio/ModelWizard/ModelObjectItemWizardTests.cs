// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard
{
    using EnvDTE;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine;
    using System.Collections.Generic;
    using UnitTests.TestHelpers;
    using Xunit;

    public class ModelObjectItemWizardTests
    {
        [Fact]
        public void ShouldAddProjectItem_returns_correct_values_for_CSharp_non_WebSite_projects_EmptyModel()
        {
            var wizard = CreateWizard(LangEnum.CSharp, /*isWebSite*/ false, ModelGenerationOption.EmptyModel);

            Assert.False(wizard.ShouldAddProjectItem("ProjectItem.cs"));
            Assert.False(wizard.ShouldAddProjectItem("ProjectItem.vb"));
            Assert.True(wizard.ShouldAddProjectItem("ProjectItem.edmx"));
            Assert.True(wizard.ShouldAddProjectItem("ProjectItem.edmx.diagram"));
        }

        [Fact]
        public void ShouldAddProjectItem_returns_correct_values_for_CSharp_non_WebSite_projects_EmptyModelCodeFirst()
        {
            var wizard = CreateWizard(LangEnum.CSharp, /*isWebSite*/ false, ModelGenerationOption.EmptyModelCodeFirst);

            Assert.True(wizard.ShouldAddProjectItem("ProjectItem.cs"));
            Assert.False(wizard.ShouldAddProjectItem("ProjectItem.vb"));
            Assert.False(wizard.ShouldAddProjectItem("ProjectItem.edmx"));
            Assert.False(wizard.ShouldAddProjectItem("ProjectItem.edmx.diagram"));
        }

        [Fact]
        public void ShouldAddProjectItem_returns_correct_values_for_CSharp_non_WebSite_projects_FromDatabase()
        {
            var wizard = CreateWizard(LangEnum.CSharp, /*isWebSite*/ false, ModelGenerationOption.GenerateFromDatabase);

            Assert.False(wizard.ShouldAddProjectItem("ProjectItem.cs"));
            Assert.False(wizard.ShouldAddProjectItem("ProjectItem.vb"));
            Assert.True(wizard.ShouldAddProjectItem("ProjectItem.edmx"));
            Assert.True(wizard.ShouldAddProjectItem("ProjectItem.edmx.diagram"));
        }


        public void ShouldAddProjectItem_returns_correct_values_for_CSharp_WebSite_projects_EmptyModel()
        {
            var wizard = CreateWizard(LangEnum.CSharp, /*isWebSite*/ true, ModelGenerationOption.EmptyModel);

            Assert.False(wizard.ShouldAddProjectItem("ProjectItem.cs"));
            Assert.False(wizard.ShouldAddProjectItem("ProjectItem.vb"));
            Assert.True(wizard.ShouldAddProjectItem("ProjectItem.edmx"));
            Assert.True(wizard.ShouldAddProjectItem("ProjectItem.edmx.diagram"));
        }

        [Fact]
        public void ShouldAddProjectItem_returns_correct_values_for_CSharp_WebSite_projects_EmptyModelCodeFirst()
        {
            var wizard = CreateWizard(LangEnum.CSharp, /*isWebSite*/ true, ModelGenerationOption.EmptyModelCodeFirst);

            Assert.True(wizard.ShouldAddProjectItem("ProjectItem.cs"));
            Assert.False(wizard.ShouldAddProjectItem("ProjectItem.vb"));
            Assert.False(wizard.ShouldAddProjectItem("ProjectItem.edmx"));
            Assert.False(wizard.ShouldAddProjectItem("ProjectItem.edmx.diagram"));
        }

        [Fact]
        public void ShouldAddProjectItem_returns_correct_values_for_CSharp_WebSite_projects_FromDatabase()
        {
            var wizard = CreateWizard(LangEnum.CSharp, /*isWebSite*/ true, ModelGenerationOption.GenerateFromDatabase);

            Assert.False(wizard.ShouldAddProjectItem("ProjectItem.cs"));
            Assert.False(wizard.ShouldAddProjectItem("ProjectItem.vb"));
            Assert.True(wizard.ShouldAddProjectItem("ProjectItem.edmx"));
            Assert.True(wizard.ShouldAddProjectItem("ProjectItem.edmx.diagram"));
        }

        [Fact]
        public void ShouldAddProjectItem_returns_correct_values_for_VB_non_WebSite_projects_EmptyModel()
        {
            var wizard = CreateWizard(LangEnum.VisualBasic, /*isWebSite*/ false, ModelGenerationOption.EmptyModel);

            Assert.False(wizard.ShouldAddProjectItem("ProjectItem.cs"));
            Assert.False(wizard.ShouldAddProjectItem("ProjectItem.vb"));
            Assert.True(wizard.ShouldAddProjectItem("ProjectItem.edmx"));
            Assert.True(wizard.ShouldAddProjectItem("ProjectItem.edmx.diagram"));
        }

        [Fact]
        public void ShouldAddProjectItem_returns_correct_values_for_VB_non_WebSite_projects_EmptyModelCodeFirst()
        {
            var wizard = CreateWizard(LangEnum.VisualBasic, /*isWebSite*/ false, ModelGenerationOption.EmptyModelCodeFirst);

            Assert.False(wizard.ShouldAddProjectItem("ProjectItem.cs"));
            Assert.True(wizard.ShouldAddProjectItem("ProjectItem.vb"));
            Assert.False(wizard.ShouldAddProjectItem("ProjectItem.edmx"));
            Assert.False(wizard.ShouldAddProjectItem("ProjectItem.edmx.diagram"));
        }

        [Fact]
        public void ShouldAddProjectItem_returns_correct_values_for_VB_non_WebSite_projects_FromDatabase()
        {
            var wizard = CreateWizard(LangEnum.VisualBasic, /*isWebSite*/ false, ModelGenerationOption.GenerateFromDatabase);

            Assert.False(wizard.ShouldAddProjectItem("ProjectItem.cS"));
            Assert.False(wizard.ShouldAddProjectItem("ProjectItem.vB"));
            Assert.True(wizard.ShouldAddProjectItem("ProjectItem.edmx"));
            Assert.True(wizard.ShouldAddProjectItem("ProjectItem.edmx.diagram"));
        }


        public void ShouldAddProjectItem_returns_correct_values_for_VB_WebSite_projects_EmptyModel()
        {
            var wizard = CreateWizard(LangEnum.VisualBasic, /*isWebSite*/ true, ModelGenerationOption.EmptyModel);

            Assert.False(wizard.ShouldAddProjectItem("ProjectItem.CS"));
            Assert.False(wizard.ShouldAddProjectItem("ProjectItem.vb"));
            Assert.True(wizard.ShouldAddProjectItem("ProjectItem.edmx"));
            Assert.True(wizard.ShouldAddProjectItem("ProjectItem.edmx.diagram"));
        }

        [Fact]
        public void ShouldAddProjectItem_returns_correct_values_for_VB_WebSite_projects_EmptyModelCodeFirst()
        {
            var wizard = CreateWizard(LangEnum.VisualBasic, /*isWebSite*/ true, ModelGenerationOption.EmptyModelCodeFirst);

            Assert.False(wizard.ShouldAddProjectItem("ProjectItem.Cs"));
            Assert.True(wizard.ShouldAddProjectItem("ProjectItem.VB"));
            Assert.False(wizard.ShouldAddProjectItem("ProjectItem.edmx"));
            Assert.False(wizard.ShouldAddProjectItem("ProjectItem.edmx.diAGram"));
        }

        [Fact]
        public void ShouldAddProjectItem_returns_correct_values_for_VB_WebSite_projects_FromDatabase()
        {
            var wizard = CreateWizard(LangEnum.VisualBasic, /*isWebSite*/ true, ModelGenerationOption.GenerateFromDatabase);

            Assert.False(wizard.ShouldAddProjectItem("ProjectItem.cs"));
            Assert.False(wizard.ShouldAddProjectItem("ProjectItem.vb"));
            Assert.True(wizard.ShouldAddProjectItem("ProjectItem.EdMx"));
            Assert.True(wizard.ShouldAddProjectItem("ProjectItem.edmx.diagram"));
        }

        private static ModelObjectItemWizard CreateWizard(LangEnum language, bool isWebSite, ModelGenerationOption modelGenerationOption)
        {
            Project project;

            if (isWebSite)
            {
                project =
                    MockDTE.CreateWebSite(
                        properties: new Dictionary<string, object>
                            {
                                { "CurrentWebsiteLanguage", language == LangEnum.CSharp ? "C#" : "VB" }
                            });
            }
            else
            {
                project = MockDTE.CreateProject(kind: language == LangEnum.CSharp ? MockDTE.CSharpProjectKind : MockDTE.VBProjectKind);
            }

            var modelBuilderSettings = new ModelBuilderSettings
            {
                Project = project,
                GenerationOption = modelGenerationOption
            };

            return new ModelObjectItemWizard(modelBuilderSettings);
        }
    }
}
