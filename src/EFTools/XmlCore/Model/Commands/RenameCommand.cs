// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Model.Integrity;
    using Microsoft.Data.Tools.XmlDesignerBase;

    internal abstract class RenameCommand : Command
    {
        internal RenameCommand(EFNormalizableItem element, string newName, bool uniquenessIsCaseSensitive)
        {
            Element = element;
            NewName = newName;
            UniquenessIsCaseSensitive = uniquenessIsCaseSensitive;
            OldName = Element.GetNameAttribute().Value;
        }

        public RenameCommand(Func<Command, CommandProcessorContext, bool> bindingAction, bool uniquenessIsCaseSensitive)
            : base(bindingAction)
        {
            UniquenessIsCaseSensitive = uniquenessIsCaseSensitive;
        }

        protected internal EFNormalizableItem Element { get; set; }

        protected internal string OldName { get; set; }

        protected internal string NewName { get; set; }

        protected internal bool UniquenessIsCaseSensitive { get; private set; }

        protected abstract bool IsUniqueNameForExistingItem(out string errorMessage);
        protected abstract void RenameRelatedElements(CommandProcessorContext cpc);
        protected abstract RenameCommand CloneCommand(EFNormalizableItem itemToRename);

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            Debug.Assert(Element != null);

            // check to see if this name is valid
            EFAttribute attr = Element.GetNameAttribute();

            var contentValidator = Element.Artifact.ModelManager.GetAttributeContentValidator(Element.Artifact);
            Debug.Assert(contentValidator != null, "Attribute content validator is null");
            if (!contentValidator.IsValidAttributeValue(NewName, attr))
            {
                // not valid content
                var msg = string.Format(CultureInfo.CurrentCulture, Resources.INVALID_NC_NAME_CHAR, NewName);
                throw new CommandValidationFailedException(msg);
            }

            string errorMessage = null;
            if (!IsUniqueNameForExistingItem(out errorMessage))
            {
                if (String.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = string.Format(CultureInfo.CurrentCulture, Resources.NAME_NOT_UNIQUE, NewName);
                }
                throw new CommandValidationFailedException(errorMessage);
            }

            RenameRelatedElements(cpc);

            // before doing the rename, identify any binding that was referencing this node or a child of this node,
            // and add it to the list of things to rebind
            CheckArtifactBindings.ScheduleChildAntiDependenciesForRebinding(cpc, Element);

            // Get the list of anti-dependencies before doing the rename, normalize and resolve steps.
            // This way all dependent items, including child elements which get unbound during NormalizeAndResolve, are included in the antiDeps list.
            var antiDeps = new List<EFObject>();
            antiDeps.AddRange(Element.GetAntiDependencies());

            // do the rename
            Element.Rename(NewName);
            XmlModelHelper.NormalizeAndResolve(Element);

            // now update any items that point to this item so that they use the new name
            foreach (var efObject in antiDeps)
            {
                var binding = efObject as ItemBinding;
                if (binding != null)
                {
                    binding.SetRefName(Element);
                }
            }

            // identify any binding that was referencing this node or a child of this node,
            // and add it to the list of things to rebind
            CheckArtifactBindings.ScheduleChildAntiDependenciesForRebinding(cpc, Element);

            // schedule unknown symbols for rebinding, since they may be fixed by the rename 
            CheckArtifactBindings.ScheduleUnknownBindingsForRebind(cpc, Element.Artifact.ArtifactSet);
        }
    }
}
