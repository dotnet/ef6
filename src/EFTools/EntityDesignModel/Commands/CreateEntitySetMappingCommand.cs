// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    internal class CreateEntitySetMappingCommand : Command
    {
        private EntityContainerMapping _entityContainerMapping;
        private readonly ConceptualEntitySet _entitySet;
        private EntitySetMapping _created;

        internal CreateEntitySetMappingCommand(EntityContainerMapping entityContainerMapping, ConceptualEntitySet entitySet)
        {
            Debug.Assert(entitySet != null, "entitySet should not be null");
            Debug.Assert(entitySet.Artifact != null, "entitySet's artifact should not be null");

            CommandValidation.ValidateConceptualEntitySet(entitySet);

            _entityContainerMapping = entityContainerMapping;
            _entitySet = entitySet;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // if don't have an ECM yet, go create one
            if (_entityContainerMapping == null)
            {
                var cmd = new CreateEntityContainerMappingCommand(_entitySet.Artifact);
                CommandProcessor.InvokeSingleCommand(cpc, cmd);

                _entityContainerMapping = cmd.EntityContainerMapping;
            }

            Debug.Assert(_entityContainerMapping != null, "_entityContainerMapping should not be null");
            if (_entityContainerMapping == null)
            {
                throw new CannotLocateParentItemException();
            }

            // create the ESM
            var esm = new EntitySetMapping(_entityContainerMapping, null);
            esm.Name.SetRefName(_entitySet);
            _entityContainerMapping.AddEntitySetMapping(esm);

            XmlModelHelper.NormalizeAndResolve(esm);

            _created = esm;
        }

        /// <summary>
        ///     The EntitySetMapping created by this command.
        /// </summary>
        internal EntitySetMapping EntitySetMapping
        {
            get { return _created; }
        }
    }
}
