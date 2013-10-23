// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Data.Entity.Design.Model.Designer;

    internal class DeleteEntityTypeShapeCommand : DeleteEFElementCommand
    {
        /// <summary>
        ///     Deletes the passed in EntityTypeShape and the associated connectors.
        /// </summary>
        /// <param name="entityType"></param>
        internal DeleteEntityTypeShapeCommand(EntityTypeShape entityTypeShape)
            : base(entityTypeShape)
        {
        }

        private EntityTypeShape EntityTypeShape
        {
            get
            {
                var elem = EFElement as EntityTypeShape;
                Debug.Assert(elem != null, "underlying element does not exist or is not an EntityTypeShape");
                if (elem == null)
                {
                    throw new InvalidModelItemException();
                }
                return elem;
            }
        }

        /// <summary>
        ///     We override this method to do some specialized processing of removing connectors associated with the shape.
        /// </summary>
        /// <param name="cpc"></param>
        protected override void RemoveAntiDeps(CommandProcessorContext cpc)
        {
            var associationConnectors = ModelHelper.GetListOfAssociationConnectorsForEntityTypeShape(EntityTypeShape).ToArray();
            foreach (var associationConnector in associationConnectors)
            {
                DeleteEFElementCommand.DeleteInTransaction(cpc, associationConnector);
            }

            var inheritanceConnectors = ModelHelper.GetListOfInheritanceConnectorsForEntityTypeShape(EntityTypeShape).ToArray();
            foreach (var inheritanceConnector in inheritanceConnectors)
            {
                DeleteEFElementCommand.DeleteInTransaction(cpc, inheritanceConnector);
            }

            // process the remaining antiDeps normally
            base.RemoveAntiDeps(cpc);
        }
    }
}
