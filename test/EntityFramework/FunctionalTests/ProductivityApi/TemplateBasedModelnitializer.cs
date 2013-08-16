// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System.Data.Entity;
    using System.Data.Entity.Design;
    using System.IO;
    using FunctionalTests.ProductivityApi.TemplateModels.CsAdvancedPatterns;
    using FunctionalTests.ProductivityApi.TemplateModels.CsMonsterModel;

    public static class TemplateTestsDatabaseInitializer
    {
        public static void InitializeModelFirstDatabases(bool runInitializers = true)
        {
            const string prefix = "System.Data.Entity.TestModels.TemplateModels.Schemas.";
            ResourceUtilities.CopyEmbeddedResourcesToCurrentDir(
                typeof(TemplateTests).Assembly,
                prefix,
                /*overwrite*/ true,
                "AdvancedPatterns.edmx",
                "MonsterModel.csdl",
                "MonsterModel.msl",
                "MonsterModel.ssdl");

            // Extract the csdl, msl, and ssdl from the edmx so that they can be referenced in the connection string.
            ModelHelpers.WriteMetadataFiles(File.ReadAllText(@".\AdvancedPatterns.edmx"), @".\AdvancedPatterns");

            if (runInitializers)
            {
                using (var context = new AdvancedPatternsModelFirstContext())
                {
                    context.Database.Initialize(force: false);
                }

                using (var context = new MonsterModel())
                {
                    Database.SetInitializer(new DropCreateDatabaseAlways<MonsterModel>());
                    context.Database.Initialize(force: false);
                }
            }

        }
    }
}
