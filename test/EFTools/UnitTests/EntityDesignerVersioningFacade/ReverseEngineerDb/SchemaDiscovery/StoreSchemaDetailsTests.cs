// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb.SchemaDiscovery
{
    using System.Linq;
    using Xunit;

    public class StoreSchemaDetailsTests
    {
        [Fact]
        public void StoreSchemaDetails_initialized_correctly()
        {
            var tableDetails = Enumerable.Empty<TableDetailsRow>();
            var viewDetails = Enumerable.Empty<TableDetailsRow>();
            var relationshipDetails = Enumerable.Empty<RelationshipDetailsRow>();
            var functionDetails = Enumerable.Empty<FunctionDetailsRowView>();
            var tvfReturnTypeDetails = Enumerable.Empty<TableDetailsRow>();

            var storeSchemaDetails = new StoreSchemaDetails(
                tableDetails, viewDetails, relationshipDetails, functionDetails, tvfReturnTypeDetails);

            Assert.Same(tableDetails, storeSchemaDetails.TableDetails);
            Assert.Same(viewDetails, storeSchemaDetails.ViewDetails);
            Assert.Same(relationshipDetails, storeSchemaDetails.RelationshipDetails);
            Assert.Same(functionDetails, storeSchemaDetails.FunctionDetails);
            Assert.Same(tvfReturnTypeDetails, storeSchemaDetails.TVFReturnTypeDetails);
        }
    }
}
