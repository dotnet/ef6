// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    internal class CreateEntityContainerMappingCommand : Command
    {
        private readonly EFArtifact _artifact;
        private EntityContainerMapping _created;

        internal CreateEntityContainerMappingCommand(EFArtifact artifact)
        {
            Debug.Assert(artifact != null, "artifact should not be null");
            Debug.Assert(
                artifact.ConceptualModel().EntityContainerCount == 1,
                "conceptual model EntityContainer count (" + artifact.ConceptualModel().EntityContainerCount + ") should be 1");
            Debug.Assert(
                artifact.StorageModel().EntityContainerCount == 1,
                "storage model EntityContainer count (" + artifact.StorageModel().EntityContainerCount + ") should be 1");
            Debug.Assert(
                artifact.MappingModel().FirstEntityContainerMapping == null, "mapping model FirstEntityContainer should not be null");

            _artifact = artifact;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "InvokeInternal")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            if (_artifact == null)
            {
                throw new InvalidOperationException("InvokeInternal is called when _artifact is null");
            }

            var ecm = new EntityContainerMapping(_artifact.MappingModel(), null);
            ecm.CdmEntityContainer.SetRefName(_artifact.ConceptualModel().FirstEntityContainer);
            ecm.StorageEntityContainer.SetRefName(_artifact.StorageModel().FirstEntityContainer);
            _artifact.MappingModel().AddEntityContainerMapping(ecm);

            XmlModelHelper.NormalizeAndResolve(ecm);

            _created = ecm;
        }

        /// <summary>
        ///     The EntityContainerMapping created by this command.
        /// </summary>
        internal EntityContainerMapping EntityContainerMapping
        {
            get { return _created; }
        }
    }
}
