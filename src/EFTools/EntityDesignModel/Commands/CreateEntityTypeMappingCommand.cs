// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    internal class CreateEntityTypeMappingCommand : Command
    {
        private readonly ConceptualEntityType _entityType;
        private readonly EntityTypeMappingKind _kind;
        private EntitySetMapping _entitySetMapping;
        private EntityTypeMapping _created;

        /// <summary>
        ///     This command eases creating EntityTypeMappings because the views can interact with entityTypes and be
        ///     abstracted away from there being different types of ETMs.
        ///     For every EntitySetMapping (one per Entity), there will always be one EntityTypeMapping.
        ///     &lt;EntityTypeMapping TypeName=&quot;Person&quot; /&gt;
        ///     or
        ///     &lt;EntityTypeMapping TypeName=&quot;IsTypeOf(Person)&quot; /&gt;
        ///     - Use the IsType when a) the table is mapped with no conditions or b) the table is
        ///     mapped with conditions, none of which are repeated by none of its subtypes.
        ///     - Use the Default otherwise.
        ///     This logic is enforced by the EnforceEntitySetMappingRules integrity check. We always
        ///     start out by creating an IsTypeOf.
        /// </summary>
        /// <param name="entityType">This must be a valid EntityType from the C-Model.</param>
        internal CreateEntityTypeMappingCommand(ConceptualEntityType entityType)
            : this(entityType, EntityTypeMappingKind.IsTypeOf)
        {
        }

        /// <summary>
        ///     Creates an EntityTypeMapping for the passed in type and kind.   If an ETM of the kind doesn't exist,
        ///     then it creates it.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="kind"></param>
        internal CreateEntityTypeMappingCommand(ConceptualEntityType entityType, EntityTypeMappingKind kind)
        {
            CommandValidation.ValidateConceptualEntityType(entityType);

            _entityType = entityType;
            _kind = kind;
        }

        /// <summary>
        ///     Creates an EntityTypeMapping in the passed in EntitySetMapping, for the passed in type and kind.
        /// </summary>
        /// <param name="entitySetMapping">If this is null, then an EntitySetMapping will be created.</param>
        /// <param name="entityType">This must be a valid EntityType from the C-Model.</param>
        /// <param name="kind">Which kind of ETM to create.</param>
        internal CreateEntityTypeMappingCommand(
            EntitySetMapping entitySetMapping, ConceptualEntityType entityType, EntityTypeMappingKind kind)
        {
            CommandValidation.ValidateEntitySetMapping(entitySetMapping);
            CommandValidation.ValidateConceptualEntityType(entityType);

            _entitySetMapping = entitySetMapping;
            _entityType = entityType;
            _kind = kind;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // make sure that there isn't an ETM of this kind already
            var entityTypeMapping = ModelHelper.FindEntityTypeMapping(cpc, _entityType, _kind, false);
            Debug.Assert(entityTypeMapping == null, "We are calling CreateEntityTypeMappingCommand and there is already one of this Kind");
            if (entityTypeMapping != null)
            {
                _created = entityTypeMapping;
                return;
            }

            // see if we can get the EntitySetMapping for our entity (if we weren't passed it)
            if (_entitySetMapping == null)
            {
                var ces = _entityType.EntitySet as ConceptualEntitySet;
                Debug.Assert(ces != null, "_entityType.EntitySet should be a ConceptualEntitySet");

                // find the EntitySetMapping for this type (V1 assumption is that there is only ESM per ES)
                EntitySetMapping esm = null;
                foreach (var depMapping in ces.GetAntiDependenciesOfType<EntitySetMapping>())
                {
                    esm = depMapping;
                    break;
                }

                _entitySetMapping = esm;
            }

            // if we still don't have an ESM, create one
            if (_entitySetMapping == null)
            {
                var cmd = new CreateEntitySetMappingCommand(
                    _entityType.Artifact.MappingModel().FirstEntityContainerMapping,
                    _entityType.EntitySet as ConceptualEntitySet);
                CommandProcessor.InvokeSingleCommand(cpc, cmd);
                _entitySetMapping = cmd.EntitySetMapping;
            }
            Debug.Assert(
                _entitySetMapping != null,
                "_entitySetMapping should not be null - we have been unable to find or create an EntitySetMapping");

            // create the ETM
            var etm = new EntityTypeMapping(_entitySetMapping, null, _kind);
            etm.TypeName.SetRefName(_entityType);
            _entitySetMapping.AddEntityTypeMapping(etm);

            XmlModelHelper.NormalizeAndResolve(etm);

            _created = etm;
        }

        /// <summary>
        ///     The EntityTypeMapping created by this command.
        /// </summary>
        internal EntityTypeMapping EntityTypeMapping
        {
            get { return _created; }
        }

        /// <summary>
        ///     Creates a new EntityTypeMapping in the existing EntitySetMapping
        ///     based on another EntityTypeMapping (etmToClone) in a different artifact.
        ///     All the other parameters are presumed to already exist in the same artifact
        ///     as the EntitySetMapping.
        /// </summary>
        internal static EntityTypeMapping CloneEntityTypeMapping(
            CommandProcessorContext cpc,
            EntityTypeMapping etmToClone, EntitySetMapping existingEntitySetMapping,
            ConceptualEntityType existingEntityType, EntityTypeMappingKind kind)
        {
            var createETM = new CreateEntityTypeMappingCommand(existingEntitySetMapping, existingEntityType, kind);
            var cp = new CommandProcessor(cpc, createETM);
            cp.Invoke();

            var etm = createETM.EntityTypeMapping;

            foreach (var mappingFragment in etmToClone.MappingFragments())
            {
                var sesToClone = mappingFragment.StoreEntitySet.Target as StorageEntitySet;
                var ses = existingEntitySetMapping.EntityContainerMapping.Artifact.
                              StorageModel().FirstEntityContainer.GetFirstNamedChildByLocalName(sesToClone.LocalName.Value)
                          as StorageEntitySet;
                CreateMappingFragmentCommand.CloneMappingFragment(cpc, mappingFragment, etm, ses);
            }

            return etm;
        }
    }
}
