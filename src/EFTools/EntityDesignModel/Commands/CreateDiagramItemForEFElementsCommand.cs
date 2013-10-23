// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.Model.Entity;

    /// <summary>
    ///     This command creates diagram shape for EFElements.
    /// </summary>
    internal class CreateDiagramItemForEFElementsCommand : Command
    {
        private readonly IList<EFElement> _efElements;
        private readonly Diagram _diagram;
        private readonly bool _createRelatedEntities;

        internal CreateDiagramItemForEFElementsCommand(IList<EFElement> efElements, Diagram diagram, bool createRelatedEntities)
        {
            _efElements = efElements;
            _diagram = diagram;
            _createRelatedEntities = createRelatedEntities;
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            if (_efElements != null)
            {
                foreach (var efElement in _efElements)
                {
                    // --------------------------------------------------------------------------------
                    // | EF Element Type                   | Diagram shape type                       |
                    // --------------------------------------------------------------------------------
                    // | Conceptual Entity Type              Entity Type Shape                        |
                    // | Association                         Association Connector                    |
                    // | Association Set                     Association Connector                    |
                    // | Entity Set                          Entity Type Shapes for all ET in the set |
                    // | Property                            Property's entity type shape             |
                    // --------------------------------------------------------------------------------

                    var entityType = efElement as ConceptualEntityType;
                    var association = efElement as Association;
                    var entitySet = efElement as EntitySet;
                    var associationSet = efElement as AssociationSet;

                    if (efElement is Property)
                    {
                        entityType = efElement.GetParentOfType(typeof(ConceptualEntityType)) as ConceptualEntityType;
                    }

                    if (associationSet != null
                        && associationSet.Association.Status == BindingStatus.Known)
                    {
                        association = associationSet.Association.Target;
                    }

                    if (entityType != null)
                    {
                        CreateEntityTypeShapeCommand.CreateEntityTypeShapeAndConnectorsInDiagram(
                            cpc, _diagram, entityType, _createRelatedEntities);
                    }
                    else if (association != null)
                    {
                        Debug.Assert(
                            association.AssociationEnds().Count == 2,
                            "Received incorrect number of AssociationEnds (" + association.AssociationEnds().Count + ") for Association "
                            + association.ToPrettyString() + " should be 2.");

                        var assocEnds = association.AssociationEnds();
                        var assocEnd1 = assocEnds[0];
                        var assocEnd2 = assocEnds[1];

                        if (assocEnd1.Type.Status == BindingStatus.Known
                            && assocEnd2.Type.Status == BindingStatus.Known)
                        {
                            CreateEntityTypeShapeCommand.CreateEntityTypeShapeAndConnectorsInDiagram(
                                cpc, _diagram, assocEnd1.Type.Target as ConceptualEntityType, _createRelatedEntities);
                            // Check whether the association is self association or not.
                            // If it is a self association, then we can skip creating the shape for the second associationEnd's entity type
                            // since both association-ends point to the same entity type. 
                            if (assocEnd1.Type.Target != assocEnd2.Type.Target)
                            {
                                CreateEntityTypeShapeCommand.CreateEntityTypeShapeAndConnectorsInDiagram(
                                    cpc, _diagram, assocEnd2.Type.Target as ConceptualEntityType, _createRelatedEntities);
                            }
                        }
                    }
                    else if (entitySet != null)
                    {
                        foreach (var et in entitySet.GetEntityTypesInTheSet())
                        {
                            CreateEntityTypeShapeCommand.CreateEntityTypeShapeAndConnectorsInDiagram(
                                cpc, _diagram, et as ConceptualEntityType, _createRelatedEntities);
                        }
                    }
                    else
                    {
                        Debug.Fail("Unable to create diagram shape for EFElement with type:" + efElement.GetType().Name);
                    }
                }
            }
        }
    }
}
