// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Integrity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    internal class CreateMappingFragmentCommand : Command
    {
        internal EntityType ConceptualEntityType { get; set; }
        private readonly EntityTypeMappingKind _entityTypeMappingKind;
        private EntityTypeMapping _entityTypeMapping;
        internal StorageEntitySet StorageEntitySet { get; set; }
        private MappingFragment _createdMappingFragment;

        internal CreateMappingFragmentCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
        }

        /// <summary>
        ///     Create MappingFragment for the passed in ConceptualEntityType and StoreEntityType, this will
        ///     create the fragment in the IsTypeOf ETM.
        /// </summary>
        /// <param name="conceptualEntityType">This must be a valid EntityType from the C-Model.</param>
        /// <param name="storeEntityType">This must be a valid EntityType from the S-Model.</param>
        internal CreateMappingFragmentCommand(EntityType conceptualEntityType, EntityType storeEntityType)
            : this(conceptualEntityType, storeEntityType, EntityTypeMappingKind.IsTypeOf)
        {
        }

        /// <summary>
        ///     Create MappingFragment for the passed in ConceptualEntityType and StoreEntityType, and inside the
        ///     ETM based on the passed in kind.
        /// </summary>
        /// <param name="conceptualEntityType">This must be a valid EntityType from the C-Model.</param>
        /// <param name="storeEntityType">This must be a valid EntityType from the S-Model.</param>
        /// <param name="kind">Specify whether this should be put in an IsTypeOf or Default ETM</param>
        internal CreateMappingFragmentCommand(EntityType conceptualEntityType, EntityType storeEntityType, EntityTypeMappingKind kind)
        {
            CommandValidation.ValidateStorageEntityType(storeEntityType);
            CommandValidation.ValidateConceptualEntityType(conceptualEntityType);

            ConceptualEntityType = conceptualEntityType;
            _entityTypeMappingKind = kind;
            _entityTypeMapping = null;
            StorageEntitySet = storeEntityType.EntitySet as StorageEntitySet;

            CommandValidation.ValidateStorageEntitySet(StorageEntitySet);
        }

        /// <summary>
        ///     Creates a MappingFragment for the passed in StorageEntitySet in the passed in ETM.
        /// </summary>
        /// <param name="entityTypeMapping">This must a valid EntityTypeMapping.</param>
        /// <param name="entitySet">This must be a valid StorageEntitySet.</param>
        internal CreateMappingFragmentCommand(EntityTypeMapping entityTypeMapping, StorageEntitySet storageEntitySet)
        {
            CommandValidation.ValidateEntityTypeMapping(entityTypeMapping);
            CommandValidation.ValidateStorageEntitySet(storageEntitySet);

            ConceptualEntityType = entityTypeMapping.FirstBoundConceptualEntityType;
            _entityTypeMappingKind = entityTypeMapping.Kind;
            _entityTypeMapping = entityTypeMapping;
            StorageEntitySet = storageEntitySet;
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // see if we have the ETM we need, if not create it
            if (_entityTypeMapping == null)
            {
                _entityTypeMapping = ModelHelper.FindEntityTypeMapping(cpc, ConceptualEntityType, _entityTypeMappingKind, true);
            }

            // make sure it was created
            Debug.Assert(_entityTypeMapping != null, "We should have created an EntityTypeMapping if needed, it is still null.");
            if (_entityTypeMapping == null)
            {
                throw new CannotLocateParentItemException();
            }

            _createdMappingFragment = new MappingFragment(_entityTypeMapping, null);
            _createdMappingFragment.StoreEntitySet.SetRefName(StorageEntitySet);
            _entityTypeMapping.AddMappingFragment(_createdMappingFragment);

            XmlModelHelper.NormalizeAndResolve(_createdMappingFragment);

            EnforceEntitySetMappingRules.AddRule(cpc, _entityTypeMapping.EntitySetMapping);
        }

        /// <summary>
        ///     The MappingFragment created by this command
        /// </summary>
        internal MappingFragment MappingFragment
        {
            get { return _createdMappingFragment; }
        }

        /// <summary>
        ///     Creates a new MappingFragment in the existing EntityTypeMapping
        ///     based on another MappingFragment (fragToClone) in a different artifact.
        ///     All the other parameters are presumed to already exist in the same artifact
        ///     as the EntityTypeMapping.
        /// </summary>
        internal static MappingFragment CloneMappingFragment(
            CommandProcessorContext cpc, MappingFragment fragToClone,
            EntityTypeMapping existingEntityTypeMapping, StorageEntitySet existingEntitySet)
        {
            var createFragmentCommand = new CreateMappingFragmentCommand(existingEntityTypeMapping, existingEntitySet);
            var cp = new CommandProcessor(cpc, createFragmentCommand);
            cp.Invoke();

            var frag = createFragmentCommand.MappingFragment;
            Debug.Assert(frag != null, "Could not locate or create the required mapping fragment");

            if (frag != null)
            {
                foreach (var sp in fragToClone.ScalarProperties())
                {
                    Property entityProperty = null;
                    if (sp.Name != null
                        && sp.Name.Target != null
                        && sp.Name.Target.LocalName != null
                        && sp.Name.Target.LocalName.Value != null)
                    {
                        entityProperty = ModelHelper.FindPropertyForEntityTypeMapping(
                            existingEntityTypeMapping, sp.Name.Target.LocalName.Value);
                        Debug.Assert(
                            entityProperty != null,
                            "Cannot find Property with name " + sp.Name.Target.LocalName.Value + " for EntityTypeMapping "
                            + existingEntityTypeMapping.ToPrettyString());
                    }

                    Property tableColumn = null;
                    if (frag.StoreEntitySet != null
                        && frag.StoreEntitySet.Target != null
                        && frag.StoreEntitySet.Target.EntityType != null
                        && frag.StoreEntitySet.Target.EntityType.Target != null
                        && sp.ColumnName != null
                        && sp.ColumnName.Target != null
                        && sp.ColumnName.Target.LocalName != null
                        && sp.ColumnName.Target.LocalName.Value != null)
                    {
                        tableColumn = ModelHelper.FindProperty(
                            frag.StoreEntitySet.Target.EntityType.Target, sp.ColumnName.Target.LocalName.Value);
                        Debug.Assert(
                            tableColumn != null,
                            "Cannot find Property with name " + sp.ColumnName.Target.LocalName.Value + " for EntityType "
                            + frag.StoreEntitySet.Target.EntityType.Target.ToPrettyString());
                    }

                    if (entityProperty != null
                        && tableColumn != null)
                    {
                        var createScalarCommand = new CreateFragmentScalarPropertyCommand(frag, entityProperty, tableColumn);
                        var cp2 = new CommandProcessor(cpc, createScalarCommand);
                        cp2.Invoke();
                    }
                }
            }

            return frag;
        }
    }
}
