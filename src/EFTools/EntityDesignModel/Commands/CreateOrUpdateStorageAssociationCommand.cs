// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class CreateOrUpdateStorageAssociationCommand : CreateStorageAssociationCommand
    {
        internal CreateOrUpdateStorageAssociationCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            var association = ModelHelper.FindAssociation(cpc.Artifact.StorageModel(), Name);

            if (association == null)
            {
                // If this Association does not exist, create it
                base.InvokeInternal(cpc);
            }
            else
            {
                // If the Association already exists, update it
                Debug.Assert(
                    association.AssociationEnds().Count == 2, "Association element is invalid, it should always have exactly 2 ends");
                if (association.AssociationEnds().Count == 2)
                {
                    AssociationEnd principal;
                    AssociationEnd dependent;
                    ModelHelper.DeterminePrincipalDependentAssociationEnds(
                        association, out principal, out dependent,
                        ModelHelper.DeterminePrincipalDependentAssociationEndsScenario.CreateForeignKeyProperties);
                    var updatedPrincipalRoleName = ModelHelper.CreatePKAssociationEndName(PkTable.LocalName.Value);
                    var principalMultiplicity = IsNullableFk ? ModelConstants.Multiplicity_ZeroOrOne : ModelConstants.Multiplicity_One;
                    var updatedDependentRoleName = ModelHelper.CreateFKAssociationEndName(FkTable.LocalName.Value);
                    var dependentMultiplicity = DoesFkFormPk ? ModelConstants.Multiplicity_ZeroOrOne : ModelConstants.Multiplicity_Many;

                    if (string.Compare(principal.Role.Value, updatedPrincipalRoleName, StringComparison.Ordinal) != 0)
                    {
                        principal.Role.Value = updatedPrincipalRoleName;
                    }

                    if (string.Compare(dependent.Role.Value, updatedDependentRoleName, StringComparison.Ordinal) != 0)
                    {
                        dependent.Role.Value = updatedDependentRoleName;
                    }

                    principal.Type.SetRefName(PkTable);
                    dependent.Type.SetRefName(FkTable);

                    if (string.Compare(principal.Multiplicity.Value, principalMultiplicity, StringComparison.Ordinal) != 0)
                    {
                        principal.Multiplicity.Value = principalMultiplicity;
                    }

                    if (string.Compare(dependent.Multiplicity.Value, dependentMultiplicity, StringComparison.Ordinal) != 0)
                    {
                        dependent.Multiplicity.Value = dependentMultiplicity;
                    }
                }

                Association = association;
            }
        }
    }
}
