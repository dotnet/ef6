// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb
{
    using System.Data.Entity.Core.Metadata.Edm;

    internal class AssociationSetEndDetails
    {
        public readonly AssociationSetEnd AssociationSetEnd;
        public readonly RelationshipMultiplicity Multiplicity;
        public readonly OperationAction DeleteBehavior;

        public AssociationSetEndDetails(
            AssociationSetEnd associationSetEnd, RelationshipMultiplicity multiplicity,
            OperationAction deleteBehavior)
        {
            AssociationSetEnd = associationSetEnd;
            Multiplicity = multiplicity;
            DeleteBehavior = deleteBehavior;
        }
    }
}
