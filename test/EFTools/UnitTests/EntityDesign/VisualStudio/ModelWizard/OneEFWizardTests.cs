// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard
{
    using System;
    using System.Collections.Generic;
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties;
    using Microsoft.VisualStudio.TemplateWizard;
    using Moq;
    using UnitTests.TestHelpers;
    using Xunit;

    public class OneEFWizardTests
    {
        [Fact]
        public void RunStarted_adds_replacement_values_for_comments_to_replacementsDictionary_CSharp()
        {
            const string className = "MyContext";
            const string myNamespace = "Very.Important.Project";

            var wizard = new OneEFWizard();

            foreach (var isWebSite in new[] { false, true })
            {
                var automationObject = CreateDTE2WithProject(LangEnum.CSharp, isWebSite);
                var replacementsDictionary = new Dictionary<string, string>
                { { "$safeitemname$", className }, { "$rootnamespace$", myNamespace } };

                wizard.RunStarted(automationObject, replacementsDictionary, WizardRunKind.AsNewItem, new object[0]);

                Assert.True(replacementsDictionary.ContainsKey("$ctorcomment$"));
                Assert.True(replacementsDictionary.ContainsKey("$dbsetcomment$"));

                Assert.Equal(
                    replacementsDictionary["$ctorcomment$"], 
                    string.Format(Resources.CodeFirstCodeFile_CtorComment_CS, className, myNamespace));
                Assert.Equal(
                    replacementsDictionary["$dbsetcomment$"],
                    Resources.CodeFirstCodeFile_DbSetComment_CS);
            }
        }

        [Fact]
        public void RunStarted_adds_replacement_values_for_comments_to_replacementsDictionary_VisualBasic()
        {
            const string className = "MyContext";
            const string myNamespace = "Very.Important.Project";

            var wizard = new OneEFWizard();

            foreach (var isWebSite in new[] { false, true })
            {
                var automationObject = CreateDTE2WithProject(LangEnum.VisualBasic, isWebSite);
                var replacementsDictionary = new Dictionary<string, string> { { "$safeitemname$", className }, { "$rootnamespace$", myNamespace } };

                wizard.RunStarted(automationObject, replacementsDictionary, WizardRunKind.AsNewItem, new object[0]);

                Assert.True(replacementsDictionary.ContainsKey("$ctorcomment$"));
                Assert.True(replacementsDictionary.ContainsKey("$dbsetcomment$"));

                Assert.Equal(
                    replacementsDictionary["$ctorcomment$"],
                    string.Format(Resources.CodeFirstCodeFile_CtorComment_VB, className, myNamespace));
                Assert.Equal(
                    replacementsDictionary["$dbsetcomment$"],
                    Resources.CodeFirstCodeFile_DbSetComment_VB);
            }
        }

        private static DTE2 CreateDTE2WithProject(LangEnum projectLanguage, bool isWebSite)
        {
              Project project;
 
             if (isWebSite)
             {
                 project =
                     MockDTE.CreateWebSite(
                         properties: new Dictionary<string, object>
                             {
                                 { "CurrentWebsiteLanguage", projectLanguage == LangEnum.CSharp ? "C#" : "VB" }
                             });
             }
             else
             {
                 project = MockDTE.CreateProject(kind: projectLanguage == LangEnum.CSharp ? MockDTE.CSharpProjectKind : MockDTE.VBProjectKind);
             }

            var mockDTE2 = new Mock<DTE2>();
            mockDTE2.Setup(d => d.ActiveSolutionProjects).Returns(new[] {project});

            return mockDTE2.Object;
        }
    }
}