// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Visitor
{
    using Microsoft.Data.Entity.Design.Model.Entity;

#if DEBUG
    /// <summary>
    ///     Verifies the integrity of an Escher model.
    /// </summary>
    internal class VerifyEscherModelIntegrityVisitor : VerifyModelIntegrityVisitor
    {
        public VerifyEscherModelIntegrityVisitor(
            bool checkDisposed, bool checkUnresolved, bool checkXObject, bool checkAnnotations, bool checkBindingIntegrity)
            : base(checkDisposed, checkUnresolved, checkXObject, checkAnnotations, checkBindingIntegrity)
        {
        }

        public VerifyEscherModelIntegrityVisitor()
        {
        }

        /// <summary>
        ///     Gets a value indicating whether the EFObject should be visited.
        /// </summary>
        protected override bool ShouldVisit(EFObject efObject)
        {
            return !(efObject.RuntimeModelRoot() is StorageEntityModel);
        }

        /// <summary>
        ///     Gets a value indicating whether the EFObject is a valid ghost node.
        /// </summary>
        /// <remarks>
        ///     A valid ghost node will not cause an error if it has an incorrect or stale xlinq annotation.
        /// </remarks>
        protected override bool IsValidGhostNode(EFObject efObject)
        {
            return ModelHelper.IsPartOfGhostMappingNode(efObject);
        }
    }
#endif
}
