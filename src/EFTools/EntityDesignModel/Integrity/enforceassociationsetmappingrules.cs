// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Integrity
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    /// <summary>
    ///     This class enforces rules about how we should generate MSL for AssociationSetMappings.  Currently, this
    ///     is focused on ensuring that mappings who need conditions have them added correctly.
    ///     Add a condition:
    ///     1. When an association end's cardinality is 0..1
    ///     2. When an association end's type is not a base type, the subtype is mapped using TPH, the association is not *:* and the
    ///     and the association's foreign key is on the TPH side (ergo, the association is mapped to the same table as the TPH hierarchy)
    /// </summary>
    internal class EnforceAssociationSetMappingRules : IIntegrityCheck
    {
        private readonly CommandProcessorContext _cpc;
        private readonly AssociationSetMapping _associationSetMapping;

        internal EnforceAssociationSetMappingRules(CommandProcessorContext cpc, AssociationSetMapping associationSetMapping)
        {
            _cpc = cpc;
            _associationSetMapping = associationSetMapping;
        }

        public bool IsEqual(IIntegrityCheck otherCheck)
        {
            var typedOtherCheck = otherCheck as EnforceAssociationSetMappingRules;
            if (typedOtherCheck != null
                && typedOtherCheck._associationSetMapping == _associationSetMapping)
            {
                return true;
            }

            return false;
        }

        public void Invoke()
        {
            Debug.Assert(_associationSetMapping != null, "The AssociationSetMapping reference is null");

            // if the association set mapping was deleted in this transaction, just return since we won't need to process it
            if (_associationSetMapping == null
                || _associationSetMapping.XObject == null)
            {
                return;
            }

            var association = _associationSetMapping.TypeName.Target;
            Debug.Assert(association != null, "AssociationSetMapping had an invalid Association reference");
            if (association == null)
            {
                return;
            }

            // if the association was deleted in this transaction, just return since we won't need to process it
            if (association.XObject == null)
            {
                return;
            }

            var ends = association.AssociationEnds();
            Debug.Assert(ends.Count == 2, "AssociationSetMapping's Association does not have 2 ends.");
            if (ends.Count < 2)
            {
                return;
            }

            var end1 = ends[0];
            var end2 = ends[1];
            Debug.Assert(end1 != null && end2 != null, "AssociationSetMapping's Association has an invalid reference to one or more Ends");
            if (end1 == null
                || end2 == null)
            {
                return;
            }

            var ses = _associationSetMapping.StoreEntitySet.Target;
            Debug.Assert(ses != null, "AssociationSetMapping had an invalid StoreEntitySet reference");
            if (ses == null)
            {
                return;
            }

            // see if we need conditions
            var needsConditionEnd1 = DoesEndNeedCondition(end1, end2, ses.EntityType.Target);
            var needsConditionEnd2 = DoesEndNeedCondition(end2, end1, ses.EntityType.Target);

            // clear out existing conditions
            var existingConditions = new List<Condition>();
            existingConditions.AddRange(_associationSetMapping.Conditions());
            for (var i = existingConditions.Count - 1; i >= 0; i--)
            {
                var condition = existingConditions[i];
                DeleteEFElementCommand.DeleteInTransaction(_cpc, condition);
            }

            if (needsConditionEnd1 || needsConditionEnd2)
            {
                // now see which conditions we need to add, looking at each EndProperty for the AssociationSetMapping
                var conditionCreateForColumn = new HashSet<Property>();
                foreach (var endProperty in _associationSetMapping.EndProperties())
                {
                    if (endProperty.Name.Target != null)
                    {
                        // then look at each mapped ScalarProperty
                        foreach (var sp in endProperty.ScalarProperties())
                        {
                            // if the column mapped is not a key, add a condition (checking for dupes)
                            if (sp.ColumnName.Target != null
                                && sp.ColumnName.Target.IsKeyProperty == false
                                && conditionCreateForColumn.Contains(sp.ColumnName.Target) == false)
                            {
                                var createCond = new CreateEndConditionCommand(_associationSetMapping, sp.ColumnName.Target, false, null);
                                CommandProcessor.InvokeSingleCommand(_cpc, createCond);

                                conditionCreateForColumn.Add(sp.ColumnName.Target);
                            }
                        }
                    }
                }
            }
        }

        internal static void AddRule(CommandProcessorContext cpc, AssociationSetMapping associationSetMapping)
        {
            Debug.Assert(associationSetMapping != null, "associationSetMapping should not be null");

            IIntegrityCheck check = new EnforceAssociationSetMappingRules(cpc, associationSetMapping);
            cpc.AddIntegrityCheck(check);
        }

        private static bool DoesEndNeedCondition(AssociationEnd end, AssociationEnd otherEnd, EntityType associationSetMappingTable)
        {
            if (end.Multiplicity.Value == ModelConstants.Multiplicity_ZeroOrOne)
            {
                return true;
            }

            var cet = end.Type.Target as ConceptualEntityType;
            Debug.Assert(end.Type.Target != null ? cet != null : true, "EntityType is not a ConceptualEntityType");

            if (cet != null
                && cet.HasResolvableBaseType)
            {
                // the subtype is mapped using TPH
                EntityType tphTable = null;
                if (IsMappedUsingTph(cet, out tphTable))
                {
                    // the association is not *:*
                    if (!(end.Multiplicity.Value == ModelConstants.Multiplicity_Many
                          && otherEnd.Multiplicity.Value == ModelConstants.Multiplicity_Many))
                    {
                        // and the association's foreign key is on the TPH side (ergo, the association is mapped to the same table as the TPH hierarchy)
                        if (tphTable == associationSetMappingTable)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static bool IsMappedUsingTph(ConceptualEntityType derivedType, out EntityType tphTable)
        {
            var baseType = derivedType.SafeBaseType;
            if (derivedType != null
                && baseType != null)
            {
                var tablesMappedToBaseType = ModelHelper.GetTablesMappedFrom(baseType);
                var tablesMappedToDerivedType = ModelHelper.GetTablesMappedFrom(derivedType);

                foreach (var tableMappedToBaseType in tablesMappedToBaseType)
                {
                    if (tablesMappedToDerivedType.Contains(tableMappedToBaseType))
                    {
                        tphTable = tableMappedToBaseType;
                        return true;
                    }
                }
            }

            tphTable = null;
            return false;
        }
    }
}
