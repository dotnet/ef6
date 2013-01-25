// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests
{
    using System.Data.Entity;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Services;
    using System.Data.Services.Common;
    using Xunit;

    public sealed class DataServicesTests : TestBase
    {
        [Fact]
        public void Validate_Basic_DataServices_Attributes()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<DataServiceFoo>();

            var databaseMapping = BuildMapping(modelBuilder);

            var mws = databaseMapping.ToMetadataWorkspace();

            var edmCollection = mws.GetItemCollection(DataSpace.CSpace);

            edmCollection.GetItem<EntityType>("CodeFirstNamespace.DataServiceFoo");
        }
    }

    [MimeType("MimeProp", "text/plain")]
    [HasStream]
    //[EntityPropertyMappingAttribute("OtherProp", SyndicationItemProperty.AuthorName, true, "critVal")]
    [EntityPropertyMapping("OtherProp", SyndicationItemProperty.AuthorName,
        SyndicationTextContentKind.Plaintext, true)]
    [EntityPropertyMapping("OtherProp", "targetPath3", "prefix3", "http://my.org/", true)]
    [EntityPropertyMapping("Inner/Data", SyndicationItemProperty.AuthorName,
        SyndicationTextContentKind.Plaintext, true)]
    public class DataServiceFoo
    {
        public int Id { get; set; }

        public string MimeProp { get; set; }

        public string OtherProp { get; set; }

        public Inner Inner { get; set; }
    }

    public class Inner
    {
        public string Data { get; set; }
    }
}
