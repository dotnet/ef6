// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb.SchemaDiscovery
{
    using System.Collections.Generic;
    using System.Diagnostics;

    internal class StoreSchemaDetails
    {
        public readonly IEnumerable<TableDetailsRow> TableDetails;
        public readonly IEnumerable<TableDetailsRow> ViewDetails;
        public readonly IEnumerable<RelationshipDetailsRow> RelationshipDetails;
        public readonly IEnumerable<FunctionDetailsRowView> FunctionDetails;
        public readonly IEnumerable<TableDetailsRow> TVFReturnTypeDetails;

        public StoreSchemaDetails(
            IEnumerable<TableDetailsRow> tableDetails,
            IEnumerable<TableDetailsRow> viewDetails,
            IEnumerable<RelationshipDetailsRow> relationshipDetails,
            IEnumerable<FunctionDetailsRowView> functionDetails,
            IEnumerable<TableDetailsRow> tvfReturnTypeDetails)
        {
            Debug.Assert(tableDetails != null, "tableDetails != null");
            Debug.Assert(viewDetails != null, "viewDetails != null");
            Debug.Assert(relationshipDetails != null, "relationshipDetails != null");
            Debug.Assert(functionDetails != null, "functionDetails != null");
            Debug.Assert(tvfReturnTypeDetails != null, "tvfReturnTypeDetails != null");

            TableDetails = tableDetails;
            ViewDetails = viewDetails;
            RelationshipDetails = relationshipDetails;
            FunctionDetails = functionDetails;
            TVFReturnTypeDetails = tvfReturnTypeDetails;
        }
    }
}
