// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class CreateOrUpdateConceptualAssociationCommand : CreateConceptualAssociationCommand
    {
        internal CreateOrUpdateConceptualAssociationCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            CreatedAssociation = ModelHelper.FindAssociation(cpc.Artifact.ConceptualModel(), Name);

            if (CreatedAssociation == null)
            {
                // This Association does not exist, create it
                base.InvokeInternal(cpc);
            }
            else
            {
                // The Association already exists, update it
                Debug.Assert(
                    CreatedAssociation.AssociationEnds().Count == 2, "Association element is invalid, it should always have exactly 2 ends");
                if (CreatedAssociation.AssociationEnds().Count == 2)
                {
                    AssociationEnd principal;
                    AssociationEnd dependent;
                    ModelHelper.DeterminePrincipalDependentEndsForAnyAssociationType(CreatedAssociation, out principal, out dependent);

                    if (principal.Type.Target == null
                        || !string.Equals(principal.Type.Target.Name.Value, End1Entity.LocalName.Value, StringComparison.Ordinal))
                    {
                        principal.Type.SetRefName(End1Entity);
                        principal.Role.Value = End1Entity.LocalName.Value;
                    }

                    if (dependent.Type.Target == null
                        || !string.Equals(dependent.Type.Target.Name.Value, End2Entity.LocalName.Value, StringComparison.Ordinal))
                    {
                        dependent.Type.SetRefName(End2Entity);
                        var endRoleValue = End2Entity.LocalName.Value;
                        if (principal.Role.Value.Equals(endRoleValue))
                        {
                            // avoid duplicate Role values between the two ends. This will occur in self-associations.
                            // Appending "1" is consistent with how model-gen chooses a unique name.
                            endRoleValue = endRoleValue + "1";
                        }
                        dependent.Role.Value = endRoleValue;
                    }

                    if (!string.Equals(principal.Multiplicity.Value, End1Multiplicity, StringComparison.Ordinal))
                    {
                        principal.Multiplicity.Value = End1Multiplicity;
                    }

                    if (!string.Equals(dependent.Multiplicity.Value, End2Multiplicity, StringComparison.Ordinal))
                    {
                        dependent.Multiplicity.Value = End2Multiplicity;
                    }

                    // We have to resolve the association after both the principal and dependent have been updated here. The reason is because 
                    // if we resolve the principal and dependent separately we will end up with duplicate symbols in the symbol table because
                    // the previous end didn't get removed.
                    XmlModelHelper.NormalizeAndResolve(CreatedAssociation);

                    // Also update the AssociationSet
                    var associationSet = CreatedAssociation.AssociationSet;

                    // It's possible for the association to exist but not the associationSet when a rename in the EntityDesigner is propagated
                    // to the database and the resulting hydration events flow back up.
                    if (associationSet == null)
                    {
                        var assocSetName = ModelHelper.GetUniqueName(
                            typeof(AssociationSet), cpc.Artifact.ConceptualModel().FirstEntityContainer, Name);
                        var cmd = new CreateAssociationSetCommand(assocSetName, CreatedAssociation);
                        CommandProcessor.InvokeSingleCommand(cpc, cmd);
                        associationSet = cmd.AssociationSet;
                    }

                    if (associationSet != null
                        && principal.Type.Status == BindingStatus.Known
                        && dependent.Type.Status == BindingStatus.Known
                        && associationSet.PrincipalEnd != null
                        && associationSet.DependentEnd != null)
                    {
                        associationSet.PrincipalEnd.Role.SetRefName(principal);
                        associationSet.PrincipalEnd.EntitySet.SetRefName(principal.Type.Target.EntitySet);

                        associationSet.DependentEnd.Role.SetRefName(dependent);
                        associationSet.DependentEnd.EntitySet.SetRefName(dependent.Type.Target.EntitySet);
                        XmlModelHelper.NormalizeAndResolve(associationSet);
                    }

                    var navProp1 = principal.GetAntiDependenciesOfType<NavigationProperty>()
                        .FirstOrDefault(np => np.FromRole.Target == principal);
                    if (navProp1 != null && ShouldCreateNavigationPropertyEnd1)
                    {
                        navProp1.Name.Value = NavigationPropertyInEnd1Entity;
                    }

                    var navProp2 = dependent.GetAntiDependenciesOfType<NavigationProperty>()
                        .FirstOrDefault(np => np.FromRole.Target == dependent);
                    if (navProp2 != null && ShouldCreateNavigationPropertyEnd2)
                    {
                        navProp2.Name.Value = NavigationPropertyInEnd2Entity;
                    }
                }
            }
        }
    }
}
