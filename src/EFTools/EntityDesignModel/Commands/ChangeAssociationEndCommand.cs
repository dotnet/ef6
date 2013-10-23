// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Integrity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    /// <summary>
    ///     This class lets you change aspects of an AssociationEnd.
    /// </summary>
    internal class ChangeAssociationEndCommand : Command
    {
        internal AssociationEnd End { get; private set; }

        internal string Multiplicity { get; private set; }

        internal string OldMultiplicity { get; private set; }

        internal string Role { get; private set; }

        internal string OldRole { get; private set; }

        /// <summary>
        ///     Creates a command of that can change an AssociationEnd
        /// </summary>
        /// <param name="end">The end to change</param>
        /// <param name="multiplicity">Changes the end's multiplicity, this may end up adding or removing conditions on any AssociationSetMappings</param>
        /// <param name="role">Changes the Role property of the end</param>
        internal ChangeAssociationEndCommand(AssociationEnd end, string multiplicity, string role)
        {
            End = end;
            Multiplicity = multiplicity;
            Role = role;
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            if (!string.IsNullOrEmpty(Role)
                && !string.Equals(End.Role.Value, Role, StringComparison.Ordinal))
            {
                // TODO:  should this command be enqueued in the command processor?
                RenameCommand c = new EntityDesignRenameCommand(End, Role, true);
                CommandProcessor.InvokeSingleCommand(cpc, c);

                // bug 563525: we need to update EndProperties within AssociationSetMappings if the AssociationEnd changes.
                // we update the "Role" of an AssociationSetEnd in the RenameCommand but the SingleItemBinding that we have to update
                // that is bound to the AssociationSetEnd is unique to this situation; it is not technically a "refactor rename".
                var associationSetEnd = End.GetAntiDependenciesOfType<AssociationSetEnd>().FirstOrDefault();

                if (associationSetEnd != null)
                {
                    // we need to renormalize the associationSetEnd, since the role name will have changed.
                    XmlModelHelper.NormalizeAndResolve(associationSetEnd);

                    var endPropertiesInAssocSetMappings = associationSetEnd.GetAntiDependenciesOfType<EndProperty>();
                    foreach (var endProperty in endPropertiesInAssocSetMappings)
                    {
                        endProperty.Name.SetRefName(associationSetEnd);
                        CheckArtifactBindings.ScheduleBindingsForRebind(cpc, new HashSet<ItemBinding> { endProperty.Name });
                    }
                }
            }
        }

        protected override void PreInvoke(CommandProcessorContext cpc)
        {
            base.PreInvoke(cpc);

            OldRole = End.Role.Value;
            OldMultiplicity = End.Multiplicity.Value;

            // Bug 599719: If this end's role equals the other end's role (within the association), we end up with the same normalized
            // names, and thus corrupt the symbol table. Attempts to rebind SingleItemBindings will corrupt the model. We have to
            // short-circuit the renaming here.
            var parentAssociation = End.Parent as Association;
            Debug.Assert(parentAssociation != null, "Where is the association for this association end?");
            if (parentAssociation != null)
            {
                var associationEnd = ModelHelper.FindAssociationEnd(parentAssociation, Role);
                if (associationEnd != null)
                {
                    var msg = String.Format(CultureInfo.CurrentCulture, Resources.Error_AssociationEndInAssocNotUnique, Role);
                    throw new CommandValidationFailedException(msg);
                }
            }

            if (string.IsNullOrEmpty(Multiplicity) == false
                && !string.Equals(End.Multiplicity.Value, Multiplicity, StringComparison.OrdinalIgnoreCase))
            {
                End.Multiplicity.Value = Multiplicity;
                var association = End.Parent as Association;
                if (association != null
                    && association.AssociationSet != null
                    && association.AssociationSet.AssociationSetMapping != null)
                {
                    EnforceAssociationSetMappingRules.AddRule(cpc, association.AssociationSet.AssociationSetMapping);
                }
            }

            InferReferentialConstraints.AddRule(cpc, End.Parent as Association);
        }
    }
}
