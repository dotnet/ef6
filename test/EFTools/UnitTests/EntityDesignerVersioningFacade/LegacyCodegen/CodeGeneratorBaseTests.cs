// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using LegacyMetadata = System.Data.Metadata.Edm;

namespace Microsoft.Data.Entity.Design.VersioningFacade.LegacyCodegen
{
    using Xunit;

    public class CodeGeneratorBaseTests
    {
        [Fact]
        public void Create_returns_EntityClassGenerator_for_EF1()
        {
            Assert.IsType<EntityClassGenerator>(
                CodeGeneratorBase.Create(LanguageOption.GenerateCSharpCode, EntityFrameworkVersion.Version1));
        }

        [Fact]
        public void Create_returns_EntityCodeGenerator_for_EF4_and_EF5()
        {
            Assert.IsType<EntityCodeGenerator>(
                CodeGeneratorBase.Create(LanguageOption.GenerateCSharpCode, EntityFrameworkVersion.Version2));

            Assert.IsType<EntityCodeGenerator>(
                CodeGeneratorBase.Create(LanguageOption.GenerateCSharpCode, EntityFrameworkVersion.Version3));
        }
    }
}
