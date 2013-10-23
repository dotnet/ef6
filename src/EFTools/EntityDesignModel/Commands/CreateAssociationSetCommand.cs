// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class CreateAssociationSetCommand : Command
    {
        internal string Name { get; set; }
        internal Association Association { get; set; }
        internal ModelSpace CommandModelSpace { get; set; }
        private AssociationSet _createdAssociationSet;

        internal CreateAssociationSetCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
        }

        internal CreateAssociationSetCommand(string name, Association association)
            : this(name, association, ModelSpace.Conceptual)
        {
        }

        internal CreateAssociationSetCommand(string name, Association association, ModelSpace modelSpace)
        {
            ValidateString(name);
            CommandValidation.ValidateAssociation(association);

            Name = name;
            Association = association;
            CommandModelSpace = modelSpace;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "InvokeInternal")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            Debug.Assert(Association != null, "InvokeInternal is called when Association is null.");
            if (Association == null)
            {
                throw new InvalidOperationException("InvokeInternal is called when Association is null");
            }

            var service = cpc.EditingContext.GetEFArtifactService();
            var artifact = service.Artifact;

            // the entity container we want to add it to
            BaseEntityContainer entityContainer = null;
            switch (CommandModelSpace)
            {
                case ModelSpace.Conceptual:
                    entityContainer = artifact.ConceptualModel().FirstEntityContainer;
                    break;
                case ModelSpace.Storage:
                    entityContainer = artifact.StorageModel().FirstEntityContainer;
                    break;
            }
            Debug.Assert(entityContainer != null, "No entity container");

            // check for uniqueness
            string msg = null;
            if (ModelHelper.IsUniqueName(typeof(AssociationSet), entityContainer, Name, false, out msg) == false)
            {
                throw new InvalidOperationException(msg);
            }

            // create the new item in our model
            var associationSet = new AssociationSet(entityContainer, null);
            associationSet.LocalName.Value = Name;
            entityContainer.AddAssociationSet(associationSet);

            // set the association binding: needs to happen before the ends are resolved
            if (Association != null)
            {
                associationSet.Association.SetRefName(Association);
            }

            // TODO: what should we create if these bindings are unknown?
            var end1 = Association.AssociationEnds()[0];
            var end2 = Association.AssociationEnds()[1];
            if (end1 != null
                && end1.Type.Status == BindingStatus.Known
                && end2 != null
                && end2.Type.Status == BindingStatus.Known)
            {
                var setEnd1 = new AssociationSetEnd(associationSet, null);
                setEnd1.Role.SetRefName(end1);
                setEnd1.EntitySet.SetRefName(end1.Type.Target.EntitySet);
                associationSet.AddAssociationSetEnd(setEnd1);

                var setEnd2 = new AssociationSetEnd(associationSet, null);
                setEnd2.Role.SetRefName(end2);
                setEnd2.EntitySet.SetRefName(end2.Type.Target.EntitySet);
                associationSet.AddAssociationSetEnd(setEnd2);
            }

            XmlModelHelper.NormalizeAndResolve(associationSet);

            _createdAssociationSet = associationSet;
        }

        /// <summary>
        ///     The AssociationSet that this command created
        /// </summary>
        internal AssociationSet AssociationSet
        {
            get { return _createdAssociationSet; }
        }
    }
}
