// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Xunit;
namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using Microsoft.Data.Entity.Design.Common;

    public class CodeIdentifierUtilsTests
    {
        [Fact]
        public void IsValidIdentifier_returns_true_for_valid_identifier()
        {
            Assert.True(
                new CodeIdentifierUtils(VisualStudioProjectSystem.WebApplication, LangEnum.CSharp).IsValidIdentifier("@abc"));
            Assert.True(
                new CodeIdentifierUtils(VisualStudioProjectSystem.WebApplication, LangEnum.VisualBasic).IsValidIdentifier("_abc"));
            Assert.True(
                new CodeIdentifierUtils(VisualStudioProjectSystem.Website, LangEnum.VisualBasic).IsValidIdentifier("abc1"));
        }

        [Fact]
        public void IsValidIdentifier_returns_false_for_invalid_identifier()
        {
            Assert.False(
                new CodeIdentifierUtils(VisualStudioProjectSystem.WebApplication, LangEnum.CSharp).IsValidIdentifier("class"));
            Assert.False(
                new CodeIdentifierUtils(VisualStudioProjectSystem.WebApplication, LangEnum.VisualBasic).IsValidIdentifier("abc.def"));
            Assert.False(
                new CodeIdentifierUtils(VisualStudioProjectSystem.Website, LangEnum.VisualBasic).IsValidIdentifier("a bc"));
            Assert.False(
                new CodeIdentifierUtils(VisualStudioProjectSystem.Website, LangEnum.VisualBasic).IsValidIdentifier("3abc"));
        }

        [Fact]
        public void IsValidIdentifier_returns_false_if_identifier_not_valid_for_CSharp_and_VB_for_Website_project()
        {
            Assert.False(
                new CodeIdentifierUtils(VisualStudioProjectSystem.Website, LangEnum.VisualBasic).IsValidIdentifier("@abc"));
        }

        [Fact]
        public void CreateValidIdentifier_returns_identifier_if_already_valid()
        {
            const string identifier = "testId";

            Assert.Same(identifier,
                new CodeIdentifierUtils(VisualStudioProjectSystem.WebApplication, LangEnum.CSharp).CreateValidIdentifier(identifier));

            Assert.Same(identifier,
                new CodeIdentifierUtils(VisualStudioProjectSystem.WebApplication, LangEnum.VisualBasic).CreateValidIdentifier(identifier));

            Assert.Same(identifier,
                new CodeIdentifierUtils(VisualStudioProjectSystem.Website, LangEnum.CSharp).CreateValidIdentifier(identifier));

            Assert.Same(identifier,
                new CodeIdentifierUtils(VisualStudioProjectSystem.Website, LangEnum.VisualBasic).CreateValidIdentifier(identifier));
        }

        [Fact]
        public void CreateValidIdentifier_creates_valid_identifiers()
        {
            Assert.Equal("ab", 
                new CodeIdentifierUtils(VisualStudioProjectSystem.WebApplication, LangEnum.CSharp).CreateValidIdentifier("a b"));

            Assert.Equal("ab",
                new CodeIdentifierUtils(VisualStudioProjectSystem.WebApplication, LangEnum.VisualBasic).CreateValidIdentifier("a b"));

            Assert.Equal("ab",
                new CodeIdentifierUtils(VisualStudioProjectSystem.WebApplication, LangEnum.CSharp).CreateValidIdentifier("a.b"));

            Assert.Equal("ab",
                new CodeIdentifierUtils(VisualStudioProjectSystem.WebApplication, LangEnum.VisualBasic).CreateValidIdentifier("a.b"));

            Assert.Equal("_for",
                new CodeIdentifierUtils(VisualStudioProjectSystem.WebApplication, LangEnum.CSharp).CreateValidIdentifier("for"));

            Assert.Equal("_For",
                new CodeIdentifierUtils(VisualStudioProjectSystem.WebApplication, LangEnum.VisualBasic).CreateValidIdentifier("For"));

            Assert.Equal("@class",
                new CodeIdentifierUtils(VisualStudioProjectSystem.WebApplication, LangEnum.CSharp).CreateValidIdentifier("@class"));

            Assert.Equal("_class",
                new CodeIdentifierUtils(VisualStudioProjectSystem.WebApplication, LangEnum.VisualBasic).CreateValidIdentifier("@class"));

            Assert.Equal("_123",
                new CodeIdentifierUtils(VisualStudioProjectSystem.WebApplication, LangEnum.CSharp).CreateValidIdentifier("123"));

            Assert.Equal("_123",
                new CodeIdentifierUtils(VisualStudioProjectSystem.WebApplication, LangEnum.VisualBasic).CreateValidIdentifier("123"));

            Assert.Equal("_",
                new CodeIdentifierUtils(VisualStudioProjectSystem.WebApplication, LangEnum.CSharp).CreateValidIdentifier("_"));

            Assert.Equal("_",
                new CodeIdentifierUtils(VisualStudioProjectSystem.WebApplication, LangEnum.VisualBasic).CreateValidIdentifier("_"));

            Assert.Equal("_",
                new CodeIdentifierUtils(VisualStudioProjectSystem.WebApplication, LangEnum.CSharp).CreateValidIdentifier("..."));

            Assert.Equal("_",
                new CodeIdentifierUtils(VisualStudioProjectSystem.WebApplication, LangEnum.VisualBasic).CreateValidIdentifier("..."));



            Assert.Equal("_class",
                new CodeIdentifierUtils(VisualStudioProjectSystem.Website, LangEnum.CSharp).CreateValidIdentifier("@class"));

            Assert.Equal("_Dim",
                new CodeIdentifierUtils(VisualStudioProjectSystem.Website, LangEnum.VisualBasic).CreateValidIdentifier("@Dim"));

            Assert.Equal("_Dim",
                new CodeIdentifierUtils(VisualStudioProjectSystem.Website, LangEnum.CSharp).CreateValidIdentifier("Dim"));

            Assert.Equal("_Dim",
                new CodeIdentifierUtils(VisualStudioProjectSystem.Website, LangEnum.VisualBasic).CreateValidIdentifier("Dim"));
        }
    }
}
