// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Integrity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    /// <summary>
    ///     Breaks the inheritance chain for the passed in entity.  This essentially removes the BaseType attribute from the
    ///     EntityType definition.
    ///     Other work is needed when inheritance is deleted.
    ///     1. We need to create a new entity set for this class
    ///     2. Any mappings cannot be inside the old EntitySetMapping, so these need to be moved
    ///     3. We register to run the EnforceEntitySetMappingRules check so rewrite any MSL as needed
    /// </summary>
    internal class DeleteInheritanceCommand : Command
    {
        private readonly ConceptualEntityType _derivedType;
        internal string DerivedTypeName { get; private set; }
        internal string BaseTypeName { get; private set; }

        /// <summary>
        ///     Deletes the base type from passed in entity type
        /// </summary>
        /// <param name="derivedType">Must be non-null and a c-space entity</param>
        internal DeleteInheritanceCommand(ConceptualEntityType derivedType)
        {
            CommandValidation.ValidateConceptualEntityType(derivedType);

            _derivedType = derivedType;
        }

        internal DeleteInheritanceCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
        }

        private void SaveDeletedInformation()
        {
            DerivedTypeName = _derivedType.Name.Value;
            BaseTypeName = _derivedType.BaseType.Target.Name.Value;
        }

        protected override void PreInvoke(CommandProcessorContext cpc)
        {
            Debug.Assert(_derivedType.BaseType.Target != null, "The derivedType passed does not derive from any type");
            SaveDeletedInformation();

            base.PreInvoke(cpc);

            // register to enforce MSL rules on the existing entity set, which won't include the derived type any more
            // after this command completes
            EnforceEntitySetMappingRules.AddRule(cpc, _derivedType.EntitySet);
        }

        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "newetm")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DerivedType")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "InvokeInternal")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // safety check, this should never be hit
            Debug.Assert(_derivedType != null, "InvokeInternal is called when DerivedType is null.");
            if (_derivedType == null)
            {
                throw new InvalidOperationException("InvokeInternal is called when DerivedType is null.");
            }

            // store off some local variables
            var baseType = _derivedType.BaseType.Target;
            var baseEntitySet = baseType.EntitySet as ConceptualEntitySet;
            var baseEntitySetMapping = (baseEntitySet == null ? null : baseEntitySet.EntitySetMapping);

            // save off a HashSet of all base types up the inheritance tree for searching later
            var allBaseTypes = new HashSet<EntityType>();
            for (var baseEntityType = baseType; baseEntityType != null; baseEntityType = baseEntityType.BaseType.Target)
            {
                allBaseTypes.Add(baseEntityType);
            }

            // set up list of all derived types down the inheritance tree
            var derivedAndAllDerivedTypes = new List<ConceptualEntityType>();
            derivedAndAllDerivedTypes.Add(_derivedType);
            derivedAndAllDerivedTypes.AddRange(_derivedType.ResolvableAllDerivedTypes);

            // remove any mappings which refer to properties which were inherited as these will
            // no longer be valid when the inheritance is deleted (and would cause the Mapping Details
            // window to open in non-edit mode). This must be done _before_ proceeding to clone the
            // EntityTypeMapping below and before we delete the inheritance (i.e. before the 
            // DefaultableValue Target becomes unresolved).
            var scalarPropsToDelete = new List<ScalarProperty>();
            if (allBaseTypes.Count > 0)
            {
                foreach (EntityType et in derivedAndAllDerivedTypes)
                {
                    foreach (var etm in et.GetAntiDependenciesOfType<EntityTypeMapping>())
                    {
                        foreach (var mf in etm.MappingFragments())
                        {
                            foreach (var sp in mf.AllScalarProperties())
                            {
                                var prop = sp.Name.Target;
                                if (prop != null)
                                {
                                    // find EntityType of which this Property is a member
                                    var propEntityType = prop.GetParentOfType(typeof(EntityType)) as EntityType;
                                    if (propEntityType != null
                                        && allBaseTypes.Contains(propEntityType))
                                    {
                                        // sp references a Property of an EntityType which will no longer
                                        // be in the inheritance hierarchy - so delete the mapping
                                        scalarPropsToDelete.Add(sp);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // cannot delete the ScalarProperty while enumerating over them - so instead delete in separate loop below
            foreach (var sp in scalarPropsToDelete)
            {
                DeleteEFElementCommand.DeleteInTransaction(cpc, sp);
            }

            // remove the inheritance
            _derivedType.BaseType.SetRefName(null);

            // re-resolve the derived type.  This will set the state of the _derivedType to be resolved (it could be unresolved because base-type binding could have been a duplicate or unknown binding). 
            _derivedType.State = EFElementState.Normalized;
            _derivedType.Resolve(_derivedType.Artifact.ArtifactSet);

            // the entity container we want to add it to
            var entityContainer = _derivedType.EntityModel.EntityContainer;
            Debug.Assert(entityContainer != null, "DerivedType does not have an Entity Container");

            // since this type no longer derives, it is stand alone and needs its own entity set
            // derive a name for the new entity set and create it
            var trialSetName = ModelHelper.ConstructProposedEntitySetName(_derivedType.Artifact, _derivedType.LocalName.Value);
            var ces = new CreateEntitySetCommand(trialSetName, _derivedType, true);
            CommandProcessor.InvokeSingleCommand(cpc, ces);
            var newEntitySet = ces.EntitySet as ConceptualEntitySet;

            // if the old entityset had mappings, then some may need to be moved
            if (baseEntitySetMapping != null)
            {
                // create a new EntitySetMapping for the new EntitySet that we made for the formally derivedType
                var createESM = new CreateEntitySetMappingCommand(entityContainer.EntityContainerMapping, newEntitySet);
                CommandProcessor.InvokeSingleCommand(cpc, createESM);
                var newEntitySetMapping = createESM.EntitySetMapping;

                // this type no longer derives from the type it used to, so its mappings can no longer
                // exist under the old EntitySetMapping, so we need to move them
                // move any EntityTypeMappings from the old EntitySetMapping used by the former base type
                // to the new one created for the new EntitySet and EntitySetMapping
                foreach (EntityType changingType in derivedAndAllDerivedTypes)
                {
                    var etms = new List<EntityTypeMapping>();
                    etms.AddRange(changingType.GetAntiDependenciesOfType<EntityTypeMapping>());

                    foreach (var etm in etms)
                    {
                        // here, to work around an xml editor bug, we clone the entity type mapping, instead of re-parenting it
                        var newetm = etm.Clone(newEntitySetMapping);

                        // now delete the old entity type mapping & dispose it. 
                        DeleteEFElementCommand.DeleteInTransaction(cpc, etm);
                    }
                }
            }

            //
            //  if there are any referential constraints properties whose principal ends refer to keys in the 
            //  old derived type, delete them
            //   
            foreach (var end in _derivedType.GetAntiDependenciesOfType<AssociationEnd>())
            {
                foreach (var role in end.GetAntiDependenciesOfType<ReferentialConstraintRole>())
                {
                    var rc = role.Parent as ReferentialConstraint;
                    if (rc != null
                        && rc.Principal == role)
                    {
                        foreach (var pr in rc.Principal.PropertyRefs)
                        {
                            Command cmd = new DeleteReferentialConstraintPropertyRefCommand(pr);
                            // don't invoke this command now, as it will modify the collection we're iterating over
                            CommandProcessor.EnqueueCommand(cmd);
                        }
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected override void PostInvoke(CommandProcessorContext cpc)
        {
            // now that we have a new entity set, make sure that it and any derived types use correct MSL
            EnforceEntitySetMappingRules.AddRule(cpc, _derivedType.EntitySet);

            // see if this type is used by association ends; since we deleted the inheritance, this entity got
            // a new EntitySet so we need to change any EntitySet references for corresponding AssociationSetEnds to the new one
            Association association = null;
            foreach (var end in _derivedType.GetAntiDependenciesOfType<AssociationEnd>())
            {
                foreach (var setEnd in end.GetAntiDependenciesOfType<AssociationSetEnd>())
                {
                    setEnd.EntitySet.SetRefName(_derivedType.EntitySet);
                    XmlModelHelper.NormalizeAndResolve(setEnd);
                }

                association = end.Parent as Association;
            }

            // try to recreate the AssociationSetMapping if one exists
            if (association != null
                && association.AssociationSet != null
                && association.AssociationSet.AssociationSetMapping != null
                && association.AssociationSet.AssociationSetMapping.XObject != null)
            {
                // store off the entity set for later
                var storeEntitySet = association.AssociationSet.AssociationSetMapping.StoreEntitySet.Target;

                // delete it
                DeleteEFElementCommand.DeleteInTransaction(cpc, association.AssociationSet.AssociationSetMapping);

                // create a new one (if we can)
                if (storeEntitySet != null
                    && storeEntitySet.EntityType.Target != null)
                {
                    var set = storeEntitySet.EntityType.Target as StorageEntityType;
                    Debug.Assert(storeEntitySet.EntityType.Target == null || set != null, "EntityType is not StorageEntityType");

                    CreateAssociationSetMappingCommand.CreateAssociationSetMappingAndIntellimatch(cpc, association, set);
                }
            }

            base.PostInvoke(cpc);
        }
    }
}
