// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Integrity
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    /// <summary>
    ///     This class will ensure that our MSL generation rules are applied for a given EntitySet.  If the
    ///     EntityType in the set is the root of a hierarchy, the entire hierarchy is checked.  This is done with
    ///     three passes over the hierarchy.
    ///     1. A model is built of EntityInfo and TableInfo classes that match the current state
    ///     2. Mapping rules are applied and the local model is changed to reflect what we want the MSL to look like
    ///     3. The local model is used to change the actual model
    /// </summary>
    internal class EnforceEntitySetMappingRules : IIntegrityCheck
    {
        private readonly CommandProcessorContext _cpc;
        private readonly EntitySetMapping _entitySetMapping;
        private readonly HashSet<EFObject> _itemsToDelete = new HashSet<EFObject>();

        internal EnforceEntitySetMappingRules(CommandProcessorContext cpc, EntitySetMapping entitySetMapping)
        {
            _cpc = cpc;
            _entitySetMapping = entitySetMapping;
        }

        public bool IsEqual(IIntegrityCheck otherCheck)
        {
            var typedOtherCheck = otherCheck as EnforceEntitySetMappingRules;
            if (typedOtherCheck != null
                && typedOtherCheck._entitySetMapping == _entitySetMapping)
            {
                return true;
            }

            return false;
        }

        public void Invoke()
        {
            Debug.Assert(_entitySetMapping != null, "The EntitySetMapping reference is null");

            // if the entity set mapping was deleted in this transaction, just return since we won't need to process it
            if (_entitySetMapping == null
                || _entitySetMapping.XObject == null)
            {
                return;
            }

            // get the entity set for this mapping
            Debug.Assert(_entitySetMapping.Name.Target != null, "The EntitySetMapping being used does not point to a C-Side entity set");
            var conceptualEntitySet = _entitySetMapping.Name.Target;

            // the set will contain the root entity of any hierarchy
            Debug.Assert(
                conceptualEntitySet.EntityType.Target != null,
                "Cannot find the root entity referenced by the EntitySetMapping and the EntitySet");
            var rootEntityType = conceptualEntitySet.EntityType.Target as ConceptualEntityType;
            Debug.Assert(conceptualEntitySet.EntityType.Target is ConceptualEntityType, "EntityType is not a ConceptualEntityType");

            // if this type has no children then no need to change the EntityTypeMappingKind
            if (rootEntityType.ResolvableDirectDerivedTypes.Count == 0)
            {
                return;
            }

            // if root EntityType has Mixed InheritanceMappingStrategy do not attempt to enforce rules
            var inheritanceMappingStrategy =
                ModelHelper.DetermineCurrentInheritanceStrategy(rootEntityType);
            if (InheritanceMappingStrategy.Mixed == inheritanceMappingStrategy)
            {
                Debug.Fail(
                    "Mixed mode - this should only happen if the user has hand-edited the file in which case the Mapping Details " +
                    "window should be in safe mode and so this code should not be executed");
                return;
            }

            // the root node of our mapping plan
            var rootInfo = new EntityInfo(rootEntityType);
            rootInfo.InheritanceStrategy = inheritanceMappingStrategy;

            // walk the hierarchy, gathering the information we need
            PopulateEntityInfoRecurse(rootInfo);

#if EXTRA_MAPPING_DEBUG_INFO
            DumpInfoTree(rootInfo, GetDefaultDumpFileName("POP"), false);
#endif

            // derive our plan for this mapping
            DeriveMappingPlanRecurse(rootInfo, false);

#if EXTRA_MAPPING_DEBUG_INFO
            DumpInfoTree(rootInfo, GetDefaultDumpFileName("PLAN"), false);
#endif

            // now go fix up the mappings
            ApplyMappingPlanRecurse(rootInfo);

            // remove extra stuff
            CleanUp();

#if EXTRA_MAPPING_DEBUG_INFO
            EntityInfo rootAfterApply = new EntityInfo(rootEntityType);
            PopulateEntityInfoRecurse(rootAfterApply);
            DumpInfoTree(rootAfterApply, GetDefaultDumpFileName("POST"), true);
#endif
        }

        /// <summary>
        ///     Loop through our _itemToDelete list and make the calls to ModelController to
        ///     actually remove the item.
        /// </summary>
        private void CleanUp()
        {
            foreach (var deleteMe in _itemsToDelete)
            {
                var deleteElement = deleteMe as EFElement;
                if (deleteElement != null)
                {
                    DeleteEFElementCommand.DeleteInTransaction(_cpc, deleteElement);
                }
                else
                {
                    Debug.Fail("We are trying to delete a type that we don't handle");
                    deleteMe.Delete();
                }
            }
        }

        /// <summary>
        ///     This method makes sure that:
        ///     1. Any given item is not duplicated in the list
        ///     2. If the newDeleted item's parent is already in the list, don't add it
        ///     3. If any children of newDeleted are already in the list, remove the child item
        ///     This greatly optimizes the actually CleanUp() process.
        /// </summary>
        /// <param name="newDeleted"></param>
        private void AddToDeleteList(EFObject newDeleted)
        {
            // if newDeleted is already there, just return
            if (_itemsToDelete.Contains(newDeleted))
            {
                return;
            }

            // don't add newDeleted if any of its parents are already being deleted
            var container = newDeleted.Parent;
            while (container != null)
            {
                if (_itemsToDelete.Contains(container))
                {
                    return;
                }
                container = container.Parent;
            }

            // if the newDeleted item is the parent of an existing item
            // remove the child item and just add the parent below
            RemoveChildrenOfNewDeleted(newDeleted as EFContainer);

            // add the item
            _itemsToDelete.Add(newDeleted);
        }

        private void RemoveChildrenOfNewDeleted(EFContainer item)
        {
            // first check to ensure that we have been sent an EFContainer
            if (item != null)
            {
                // check all children of this item, the list of items to delete could contain 
                // a number of siblings, so we need to check every child at any given level
                foreach (var child in item.Children)
                {
                    if (_itemsToDelete.Contains(child))
                    {
                        // remove this child item, don't need to recurse any further
                        _itemsToDelete.Remove(child);
                    }
                    else
                    {
                        // only recurse further if we didn't remove this child
                        RemoveChildrenOfNewDeleted(child as EFContainer);
                    }
                }
            }
        }

        #region Apply Mapping Plan

        private void ApplyMappingPlanRecurse(EntityInfo info)
        {
            ProcessEntityTypeMapping(info, EntityTypeMappingKind.IsTypeOf);
            ProcessEntityTypeMapping(info, EntityTypeMappingKind.Default);

            ProcessFunctionMapping(info);

            // go do this for all children
            foreach (var childInfo in info.Children)
            {
                ApplyMappingPlanRecurse(childInfo);
            }
        }

        private void ProcessEntityTypeMapping(EntityInfo info, EntityTypeMappingKind kind)
        {
            var etm = ModelHelper.FindEntityTypeMapping(_cpc, info.EntityType, kind, false);

            if (info.UsesEntityTypeMappingKind(kind))
            {
                // find or create the entity type mapping
                if (etm == null)
                {
                    var createETM = new CreateEntityTypeMappingCommand(
                        _entitySetMapping,
                        info.EntityType,
                        kind);
                    CommandProcessor.InvokeSingleCommand(_cpc, createETM);
                    etm = createETM.EntityTypeMapping;
                }
                Debug.Assert(etm != null, "Could not locate or create the required EntityTypeMapping");

                ProcessMappingFragments(info, etm);
            }
            else
            {
                // don't need it, remove it if we have one
                if (etm != null)
                {
                    AddToDeleteList(etm);
                }
            }
        }

        private void ProcessMappingFragments(EntityInfo info, EntityTypeMapping etm)
        {
            // process each relationship between the type and a table
            foreach (var table in info.Tables.Keys)
            {
                var tableInfo = info.Tables[table];

                // find or create the mapping fragment
                var frag = FindMappingFragment(etm, table);
                if (tableInfo.UsesEntityTypeMappingKind(etm.Kind))
                {
                    if (frag == null)
                    {
                        var cmd = new CreateMappingFragmentCommand(etm, table.EntitySet as StorageEntitySet);
                        CommandProcessor.InvokeSingleCommand(_cpc, cmd);
                        frag = cmd.MappingFragment;
                    }
                    Debug.Assert(frag != null, "Could not locate or create the required MappingFragment");

                    ProcessMappingFragment(info, table, frag);
                }
                else
                {
                    // don't need it, remove it if we have one
                    if (frag != null)
                    {
                        AddToDeleteList(frag);
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void ProcessMappingFragment(EntityInfo info, EntityType table, MappingFragment frag)
        {
            // move any scalar mappings to this fragment if they aren't there
            foreach (var sp in info.NonKeyScalars)
            {
                Debug.Assert(sp.ColumnName.Target != null, "Found a ScalarProperty with an unknown column binding");

                if (sp.ColumnName.Target.Parent == table
                    && sp.MappingFragment != frag)
                {
                    // delete the old, create the new
                    AddToDeleteList(sp);

                    var cmd = new CreateFragmentScalarPropertyTreeCommand(frag, sp.GetMappedPropertiesList(), sp.ColumnName.Target);
                    var cp = new CommandProcessor(_cpc, cmd);
                    cp.Invoke();
                }
            }

            // move any conditions to this fragment if they aren't there
            foreach (var cond in info.Conditions)
            {
                Debug.Assert(cond.ColumnName.Target != null, "Found a Condition with an unknown column binding");

                if (cond.ColumnName.Target.Parent == table
                    && cond.MappingFragment != frag)
                {
                    // save off the condition information
                    bool? isNull = null;
                    if (cond.IsNull.Value == Condition.IsNullConstant)
                    {
                        isNull = true;
                    }
                    else if (cond.IsNull.Value == Condition.IsNotNullConstant)
                    {
                        isNull = false;
                    }

                    var conditionValue = cond.Value.Value;
                    var column = cond.ColumnName.Target;

                    // delete the old, create the new
                    AddToDeleteList(cond);

                    var cmd = new CreateFragmentConditionCommand(frag, column, isNull, conditionValue);
                    var cp = new CommandProcessor(_cpc, cmd);
                    cp.Invoke();
                }
            }

            // build a list of all of the keys
            var keysToMap = new List<Property>();
            keysToMap.AddRange(info.KeyProperties);

            // move any key scalar mappings to this fragment if they exist in a different one - provided they are for the same table
            foreach (var sp in info.KeyScalars)
            {
                Debug.Assert(sp.ColumnName.Target != null, "Found a ScalarProperty with an unknown column binding");

                if (sp.ColumnName.Target.Parent == table
                    && sp.MappingFragment != frag)
                {
                    var property = sp.Name.Target;
                    var column = sp.ColumnName.Target;

                    // delete the old, create the new
                    AddToDeleteList(sp);

                    var cmd = new CreateFragmentScalarPropertyCommand(frag, property, column);
                    var cp = new CommandProcessor(_cpc, cmd);
                    cp.Invoke();
                }

                // since we've mapped this one now, remove it from our list of things to do
                keysToMap.Remove(sp.Name.Target);
            }

            // if its TPH, all keys need to be here
            // (Note: if it's not TPH the user needs to specify any missing keys manually)
            if (info.InheritanceStrategy == InheritanceMappingStrategy.TablePerHierarchy)
            {
                // loop through the base most type's keys and add those that we haven't mapped
                foreach (var keyRemaining in keysToMap)
                {
                    var sp = FindKeyMappingInAllParents(info, keyRemaining);
                    if (sp != null
                        && sp.ColumnName.Target != null
                        && sp.ColumnName.Target.Parent == table)
                    {
                        var cmd = new CreateFragmentScalarPropertyCommand(frag, sp.Name.Target, sp.ColumnName.Target);
                        var cp = new CommandProcessor(_cpc, cmd);
                        cp.Invoke();
                    }
                }
            }

            // replicate all non-key base type scalars here if the parent uses a Default ETM
            // (since there is no parent IsTypeOf ETM from which to "inherit" them)
            if (info.Parent != null
                && info.Parent.UsesEntityTypeMappingKind(EntityTypeMappingKind.Default))
            {
                // first gather the list of scalars from all parents
                var parentScalars = new List<ScalarProperty>();
                GatherNonKeyScalarsFromAllParents(info.Parent, parentScalars);

                // then build a list of those scalars used in our fragment
                var existingMappedProperties = new HashSet<Property>();
                foreach (var existingScalar in frag.ScalarProperties())
                {
                    existingMappedProperties.Add(existingScalar.Name.Target);
                }

                // finally, add those in that aren't already in the fragment
                foreach (var sp in parentScalars)
                {
                    Debug.Assert(sp.ColumnName.Target != null, "Found a ScalarProperty with an unknown column binding");

                    // don't duplicate and only add those that use the same table as us
                    if (existingMappedProperties.Contains(sp.Name.Target) == false
                        && sp.ColumnName.Target.EntityType == table)
                    {
                        var cmd = new CreateFragmentScalarPropertyTreeCommand(frag, sp.GetMappedPropertiesList(), sp.ColumnName.Target);
                        var cp = new CommandProcessor(_cpc, cmd);
                        cp.Invoke();

                        existingMappedProperties.Add(sp.Name.Target);
                    }
                }
            }

            // make sure that we don't have any extra scalars
            // so gather the list of all SPs we expect to be here
            var expectedMappedProperties = new List<Property>();
            expectedMappedProperties.AddRange(info.KeyProperties);
            expectedMappedProperties.AddRange(info.NonKeyProperties);
            if (info.Parent != null
                && info.Parent.UsesEntityTypeMappingKind(EntityTypeMappingKind.Default))
            {
                GatherNonKeyPropertiesFromAllParents(info.Parent, expectedMappedProperties);
            }

            // remove any that aren't in our expected list
            foreach (var sp in frag.ScalarProperties())
            {
                if (expectedMappedProperties.Contains(sp.Name.Target) == false)
                {
                    AddToDeleteList(sp);
                }
            }
        }

        private void ProcessFunctionMapping(EntityInfo info)
        {
            var etm = ModelHelper.FindEntityTypeMapping(_cpc, info.EntityType, EntityTypeMappingKind.Function, false);
            if (etm != null)
            {
                Debug.Assert(etm.ModificationFunctionMapping != null, "ModificationFunctionMapping was null");
                if (etm.ModificationFunctionMapping != null)
                {
                    ProcessModificationFunction(info, etm.ModificationFunctionMapping.InsertFunction);
                    ProcessModificationFunction(info, etm.ModificationFunctionMapping.UpdateFunction);
                    ProcessModificationFunction(info, etm.ModificationFunctionMapping.DeleteFunction);
                }
            }
        }

        private void ProcessModificationFunction(EntityInfo info, ModificationFunction function)
        {
            if (function == null)
            {
                return;
            }

            // make sure that we don't have any extra scalars (i.e. from entities that are no longer parents of this entity)
            // so gather the list of all SPs we expect to be here
            var expectedMappedProperties = new List<Property>();
            expectedMappedProperties.AddRange(info.KeyProperties);
            expectedMappedProperties.AddRange(info.NonKeyProperties);
            if (info.Parent != null)
            {
                GatherNonKeyPropertiesFromAllParents(info.Parent, expectedMappedProperties);
            }

            // remove any that aren't in our expected list
            foreach (var sp in function.ScalarProperties())
            {
                if (expectedMappedProperties.Contains(sp.Name.Target) == false)
                {
                    AddToDeleteList(sp);
                }
            }

            // make sure that we don't have any extra AssociationEnds
            var cet = info.EntityType;
            Debug.Assert(cet != null, "EntityType is not a ConceptualEntityType");
            var selfAndBaseTypes = new List<ConceptualEntityType>(cet.SafeSelfAndBaseTypes);
            foreach (var end in function.AssociationEnds())
            {
                var from = end.From.Target;
                var to = end.To.Target;
                if (from != null
                    && to != null
                    && from.Role.Target != null
                    && to.Role.Target != null)
                {
                    var fromType = from.Role.Target.Type.Target as ConceptualEntityType;
                    var toType = to.Role.Target.Type.Target as ConceptualEntityType;

                    Debug.Assert(
                        from.Role.Target.Type.Target != null ? fromType != null : true, "fromType EntityType is not a ConceptualEntityType");
                    Debug.Assert(
                        to.Role.Target.Type.Target != null ? toType != null : true, "toType EntityType is not a ConceptualEntityType");

                    if (fromType != null
                        && toType != null)
                    {
                        if (selfAndBaseTypes.Contains(fromType)
                            || selfAndBaseTypes.Contains(toType))
                        {
                            continue;
                        }
                    }
                }

                AddToDeleteList(end);
            }
        }

        private static MappingFragment FindMappingFragment(EntityTypeMapping etm, EntityType table)
        {
            foreach (var fragment in etm.MappingFragments())
            {
                if (fragment.StoreEntitySet.Target == table.EntitySet)
                {
                    return fragment;
                }
            }

            return null;
        }

        private static ScalarProperty FindKeyMappingInAllParents(EntityInfo info, Property key)
        {
            var sp = info.FindKeyMapping(key);

            if (sp == null
                && info.Parent != null)
            {
                return FindKeyMappingInAllParents(info.Parent, key);
            }
            else
            {
                return sp;
            }
        }

        private static void GatherNonKeyScalarsFromAllParents(EntityInfo info, List<ScalarProperty> scalars)
        {
            scalars.AddRange(info.NonKeyScalars);

            if (info.Parent != null)
            {
                GatherNonKeyScalarsFromAllParents(info.Parent, scalars);
            }
        }

        private static void GatherNonKeyPropertiesFromAllParents(EntityInfo info, List<Property> properties)
        {
            properties.AddRange(info.NonKeyProperties);

            if (info.Parent != null)
            {
                GatherNonKeyPropertiesFromAllParents(info.Parent, properties);
            }
        }

        #endregion

        #region Derive Mapping Plan

        private static void DeriveMappingPlanRecurse(EntityInfo info, bool hierarchyHasUnconditionalIsTypeOfMapping)
        {
            var hierarchyHasUnconditionalIsTypeOfMappingForChildren = hierarchyHasUnconditionalIsTypeOfMapping;
            if (info.Tables.Count > 0)
            {
                foreach (var table in info.Tables.Keys)
                {
                    var tableInfo = info.Tables[table];
                    DetermineFragmentPlacement(info, tableInfo);

                    // Check to see if we need to deal with unconditional isTypeOfs.
                    // Note: we only want to check parent hierarchy here i.e. if we find one
                    // it should only affects its children not other Tables mapped to
                    // the same EntityInfo - which is why we use hierarchyHasUnconditionalIsTypeOfMappingForChildren.
                    if (hierarchyHasUnconditionalIsTypeOfMapping == false)
                    {
                        // if no parent has an unconditional IsTypeOf mapping, see if we do
                        if (tableInfo.EntityTypeMappingKind == EntityTypeMappingKind.IsTypeOf
                            && info.Conditions.Count == 0)
                        {
                            hierarchyHasUnconditionalIsTypeOfMappingForChildren = true;
                        }
                    }
                    else
                    {
                        // we have a parent with an unconditional IsTypeOf mapping, we can't also be in TPH
                        // (unless we are abstract)
                        if (info.InheritanceStrategy == InheritanceMappingStrategy.TablePerHierarchy
                            && tableInfo.EntityTypeMappingKind == EntityTypeMappingKind.IsTypeOf
                            && info.Conditions.Count == 0
                            && info.EntityType.IsConcrete)
                        {
                            // we have to move everything to the Default ETM
                            tableInfo.EntityTypeMappingKind = EntityTypeMappingKind.Default;
                        }
                    }
                }
            }

            // process all children
            foreach (var childInfo in info.Children)
            {
                DeriveMappingPlanRecurse(childInfo, hierarchyHasUnconditionalIsTypeOfMappingForChildren);
            }
        }

        private static void DetermineFragmentPlacement(EntityInfo info, TableInfo tableInfo)
        {
            var kind = EntityTypeMappingKind.IsTypeOf;

            // special processing for abstract types, they can only have IsTypeOf mappings
            // also, we only have to process entities that have S-side conditions
            if (info.EntityType.Abstract.Value == false
                && info.Conditions.Count > 0)
            {
                var columns = new HashSet<Property>();
                GatherColumnsUsedInSSideConditionsByAllChildren(info, columns, false);

                foreach (var cond in info.Conditions)
                {
                    // We are only looking for S-side conditions here, cond.ColumnName.Target may be null for e.g. C-side Conditions
                    if (cond.ColumnName != null
                        && cond.ColumnName.Target != null
                        && cond.ColumnName.Target.EntityType != null
                        && cond.ColumnName.Target.EntityType == tableInfo.Table
                        && columns.Contains(cond.ColumnName.Target))
                    {
                        // derived classes are using this, so we have to put it
                        // into the default ETM
                        kind = EntityTypeMappingKind.Default;
                        break;
                    }
                }
            }

            tableInfo.EntityTypeMappingKind = kind;
        }

        private static void GatherColumnsUsedInSSideConditionsByAllChildren(EntityInfo info, HashSet<Property> columns, bool checkCurrent)
        {
            if (checkCurrent)
            {
                foreach (var cond in info.Conditions)
                {
                    // We are only looking for S-side conditions here, cond.ColumnName.Target may be null for e.g. C-side Conditions
                    if (cond.ColumnName.Target != null
                        && columns.Contains(cond.ColumnName.Target) == false)
                    {
                        columns.Add(cond.ColumnName.Target);
                    }
                }
            }

            foreach (var childInfo in info.Children)
            {
                GatherColumnsUsedInSSideConditionsByAllChildren(childInfo, columns, true);
            }
        }

        #endregion

        #region Populate EntityInfo tree

        private void PopulateEntityInfoRecurse(EntityInfo parentInfo)
        {
            // load data for this entity
            GatherKeysAndLocalProperties(parentInfo);

            // load information about the IsTypeOf ETM (if one exists)
            var etm1 = ModelHelper.FindEntityTypeMapping(_cpc, parentInfo.EntityType, EntityTypeMappingKind.IsTypeOf, false);
            if (etm1 != null)
            {
#if EXTRA_MAPPING_DEBUG_INFO
                parentInfo.HasIsTypeOfMapping = true;
#endif
                GatherScalarsForEntityTypeMapping(parentInfo, etm1);
                GatherConditionsForEntityTypeMapping(parentInfo, etm1);
            }

            // load information about the Default ETM (if one exists)
            var etm2 = ModelHelper.FindEntityTypeMapping(_cpc, parentInfo.EntityType, EntityTypeMappingKind.Default, false);
            if (etm2 != null)
            {
#if EXTRA_MAPPING_DEBUG_INFO
                parentInfo.HasDefaultMapping = true;
#endif
                GatherScalarsForEntityTypeMapping(parentInfo, etm2);
                GatherConditionsForEntityTypeMapping(parentInfo, etm2);
            }

            // recurse into its children (do not include children with Mixed InheritanceMappingStrategy)
            foreach (var derived in parentInfo.EntityType.ResolvableDirectDerivedTypes)
            {
                var inheritanceMappingStrategy =
                    ModelHelper.DetermineCurrentInheritanceStrategy(derived);
                Debug.Assert(
                    InheritanceMappingStrategy.Mixed != inheritanceMappingStrategy,
                    "Mixed mode - this should only happen if the user has hand-edited the file in which case the Mapping Details window should be in safe mode and so this code should not be executed");
                if (InheritanceMappingStrategy.Mixed != inheritanceMappingStrategy)
                {
                    var childInfo = new EntityInfo(derived, parentInfo);
                    childInfo.InheritanceStrategy = inheritanceMappingStrategy;
                    PopulateEntityInfoRecurse(childInfo);
                }
            }
        }

        private static void GatherKeysAndLocalProperties(EntityInfo info)
        {
            info.KeyProperties.AddRange(info.EntityType.ResolvableTopMostBaseType.ResolvableKeys);

            foreach (var property in info.EntityType.Properties())
            {
                if (property.IsKeyProperty == false)
                {
                    info.NonKeyProperties.Add(property);
                }
            }
        }

        private static void GatherScalarsForEntityTypeMapping(EntityInfo info, EntityTypeMapping etm)
        {
            foreach (var frag in etm.MappingFragments())
            {
                Debug.Assert(frag.StoreEntitySet.Target != null, "frag.StoreEntitySet.Target should not be null");
                Debug.Assert(
                    frag.StoreEntitySet.Target.EntityType.Target != null, "frag.StoreEntitySet.Target.EntityType.Target should not be null");

                if (frag.StoreEntitySet.Target != null
                    && frag.StoreEntitySet.Target.EntityType.Target != null
                    && info.HasTable(frag.StoreEntitySet.Target.EntityType.Target) == false)
                {
                    var table = frag.StoreEntitySet.Target.EntityType.Target;
                    var ti = new TableInfo(table);
                    ti.EntityTypeMappingKind = etm.Kind;
                    info.Tables.Add(table, ti);
                }

                foreach (var sp in frag.ScalarProperties())
                {
                    if (info.KeyProperties.Contains(sp.Name.Target))
                    {
                        info.KeyScalars.Add(sp);
                    }
                    else
                    {
                        info.NonKeyScalars.Add(sp);
                    }
                }

                foreach (var cp in frag.ComplexProperties())
                {
                    GatherScalarsFromComplexProperty(info, cp);
                }
            }
        }

        private static void GatherScalarsFromComplexProperty(EntityInfo info, ComplexProperty complexProperty)
        {
            foreach (var sp in complexProperty.ScalarProperties())
            {
                info.NonKeyScalars.Add(sp);
            }

            foreach (var cp in complexProperty.ComplexProperties())
            {
                GatherScalarsFromComplexProperty(info, cp);
            }
        }

        private static void GatherConditionsForEntityTypeMapping(EntityInfo info, EntityTypeMapping etm)
        {
            foreach (var frag in etm.MappingFragments())
            {
                Debug.Assert(frag.StoreEntitySet.Target != null, "Found a MappingFragment with an unknown StoreEntitySet binding");
                Debug.Assert(
                    frag.StoreEntitySet.Target.EntityType.Target != null, "Found an S-Side entity set with an unknown entity binding");

                if (frag.StoreEntitySet.Target != null
                    && frag.StoreEntitySet.Target.EntityType.Target != null
                    && info.HasTable(frag.StoreEntitySet.Target.EntityType.Target) == false)
                {
                    var table = frag.StoreEntitySet.Target.EntityType.Target;
                    var ti = new TableInfo(table);
                    ti.EntityTypeMappingKind = etm.Kind;
                    info.Tables.Add(table, ti);
                }

                foreach (var cond in frag.Conditions())
                {
                    info.Conditions.Add(cond);
                }
            }
        }

        #endregion

        #region Dump Info Tree

#if EXTRA_MAPPING_DEBUG_INFO

        private static string GetDefaultDumpFileName(string suffix)
        {
            // pause a bit so that we don't have duplicate file names (need to let the clock advance some)
            Thread.Sleep(500);

            string dumpFileName = string.Format("C:\\MappingRulesDebugDump-{0}-{1}.txt",
                DateTime.Now.ToString("s").Replace(":", "-"),
                suffix);
            return dumpFileName;
        }

        private static void DumpInfoTree(EntityInfo rootInfo, string fileName, bool verify)
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                DumpInfoTreeRecurse(rootInfo, sw, 0, verify);
            }
        }

        private static void DumpInfoTreeRecurse(EntityInfo info, StreamWriter sw, int level, bool verify)
        {
            string padding = new String(' ', level * 3);

            if (verify)
            {
                Debug.Assert(!(info.HasDefaultMapping && info.HasIsTypeOfMapping), padding);
            sw.WriteLine("{0}Entity: {1}", padding, info.EntityType.LocalName.Value);
            sw.WriteLine("{0} Has IsTypeOf Mapping?: {1}", padding, info.HasIsTypeOfMapping);
            sw.WriteLine("{0} Has Default Mapping?: {1}", padding, info.HasDefaultMapping);
            sw.WriteLine("{0} Is Abstract?: {1}", padding, info.EntityType.Abstract.Value);
            sw.WriteLine("{0} Key Properties: {1}", padding, DumpPropertyListToString(info.KeyProperties));
            sw.WriteLine("{0} Non-Key Properties: {1}", padding, DumpPropertyListToString(info.NonKeyProperties));
            sw.WriteLine("{0} Inheritance Strategy: {1}", padding, info.InheritanceStrategy.ToString());
            
            if (info.Tables.Count > 0)
            {
                // dump the tables it's mapped to
                sw.WriteLine("{0} Tables:", padding);

                foreach (TableInfo tableInfo in info.Tables.Values)
                {
                    sw.WriteLine("{0} * {1}, ETM: {2}",
                        padding,
                        tableInfo.Table.LocalName.Value,
                        DumpEntityTypeMappingKindToString(tableInfo.EntityTypeMappingKind));
                }
            }

            if (info.KeyScalars.Count > 0 || info.NonKeyScalars.Count > 0)
            {
                // dump the scalars
                sw.WriteLine("{0} ScalarProperties:", padding);

                foreach (ScalarProperty sp in info.KeyScalars)
                {
                    sw.WriteLine("{0} * {1} <-> {2} (Key)",
                        padding,
                        sp.Name.RefName,
                        sp.ColumnName.RefName);
                }

                foreach (ScalarProperty sp in info.NonKeyScalars)
                {
                    sw.WriteLine("{0} * {1} <-> {2}",
                        padding,
                        sp.Name.RefName,
                        sp.ColumnName.RefName);
                }
            }

            if (info.Conditions.Count > 0)
            {
                // dump conditions
                sw.WriteLine("{0} Conditions:", padding);

                foreach (Condition cond in info.Conditions)
                {
                    sw.WriteLine("{0} * {1}",
                        padding,
                        DumpConditionToString(cond));
                }
            }

            // recurse its children
            foreach (EntityInfo childInfo in info.Children)
            {
                DumpInfoTreeRecurse(childInfo, sw, level+1, verify);
            }
        }

        private static string DumpConditionToString(Condition cond)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(cond.ColumnName.RefName);
            sb.Append(" ");
            if (cond.IsNull.Value != null)
            {
                if (cond.IsNull.Value == Condition.IsNullConstant)
                {
                    sb.Append("Is Null");
                }
                else
                {
                    sb.Append("Is Not Null");
                }
            }
            else if (cond.Value.Value != null)
            {
                sb.Append("= ");
                sb.Append(cond.Value.Value);
            }
            else
            {
                sb.Append("(undefined)");
            }

            return sb.ToString();
        }

        private static string DumpPropertyListToString(List<Property> list)
        {
            StringBuilder sb = new StringBuilder();

            foreach (Property prop in list)
            {
                sb.Append(prop.LocalName.Value);
                sb.Append(", ");
            }

            return sb.ToString();
        }

        private static string DumpEntityTypeMappingKindToString(EntityTypeMappingKind kind)
        {
            if (kind == EntityTypeMappingKind.Default) return "Default";
            if (kind == EntityTypeMappingKind.IsTypeOf) return "IsTypeOf";

            return "Derive";
        }

#endif

        #endregion

        #region Private Classes and Enums

        private class EntityInfo
        {
            internal readonly ConceptualEntityType EntityType;
            internal readonly EntityInfo Parent;
            internal readonly List<EntityInfo> Children = new List<EntityInfo>();
#if EXTRA_MAPPING_DEBUG_INFO
            internal bool HasIsTypeOfMapping;
            internal bool HasDefaultMapping;
#endif
            internal InheritanceMappingStrategy InheritanceStrategy = InheritanceMappingStrategy.NoInheritance;
            internal readonly List<Property> KeyProperties = new List<Property>();
            internal readonly List<Property> NonKeyProperties = new List<Property>();
            internal readonly List<ScalarProperty> KeyScalars = new List<ScalarProperty>();
            internal readonly List<ScalarProperty> NonKeyScalars = new List<ScalarProperty>();
            internal readonly List<Condition> Conditions = new List<Condition>();
            internal readonly Dictionary<EntityType, TableInfo> Tables = new Dictionary<EntityType, TableInfo>();

            internal EntityInfo(ConceptualEntityType entityType)
            {
                EntityType = entityType;
            }

            internal EntityInfo(ConceptualEntityType entityType, EntityInfo parentInfo)
            {
                EntityType = entityType;
                Parent = parentInfo;
                Parent.Children.Add(this);
            }

            internal ScalarProperty FindKeyMapping(Property key)
            {
                foreach (var sp in KeyScalars)
                {
                    if (sp.Name.Target == key)
                    {
                        return sp;
                    }
                }
                return null;
            }

            internal bool HasTable(EntityType table)
            {
                if (Tables.ContainsKey(table))
                {
                    return true;
                }

                return false;
            }

            internal bool UsesEntityTypeMappingKind(EntityTypeMappingKind kind)
            {
                // for non-abstract types, evaluate what is needed
                foreach (var pair in Tables)
                {
                    if (pair.Value.UsesEntityTypeMappingKind(kind))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private class TableInfo
        {
            internal readonly EntityType Table;
            internal EntityTypeMappingKind EntityTypeMappingKind = EntityTypeMappingKind.IsTypeOf;

            internal TableInfo(EntityType table)
            {
                Table = table;
            }

            internal bool UsesEntityTypeMappingKind(EntityTypeMappingKind kind)
            {
                return (EntityTypeMappingKind == kind);
            }
        }

        #endregion

        internal static void AddRule(CommandProcessorContext cpc, EntityType element)
        {
            if (element != null
                && element.EntitySet != null)
            {
                AddRule(cpc, element.EntitySet);
            }
        }

        internal static void AddRule(CommandProcessorContext cpc, EntitySet element)
        {
            if (element != null)
            {
                foreach (var esm in element.GetAntiDependenciesOfType<EntitySetMapping>())
                {
                    AddRule(cpc, esm);
                }
            }
        }

        internal static void AddRule(CommandProcessorContext cpc, ScalarProperty element)
        {
            if (element != null)
            {
                AddRule(cpc, element.MappingFragment.EntityTypeMapping.EntitySetMapping);
            }
        }

        internal static void AddRule(CommandProcessorContext cpc, Condition element)
        {
            if (element != null)
            {
                AddRule(cpc, element.MappingFragment.EntityTypeMapping.EntitySetMapping);
            }
        }

        internal static void AddRule(CommandProcessorContext cpc, EntitySetMapping element)
        {
            if (element != null)
            {
                IIntegrityCheck check = new EnforceEntitySetMappingRules(cpc, element);
                cpc.AddIntegrityCheck(check);
            }
        }
    }
}
