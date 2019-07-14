// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    /// <summary>
    ///     This class updates either the ReferentialConstraint or the AssociationSetMapping depending on whether or not we're including
    ///     foreign keys. The reason why we need a command that can pivot between these two behaviors is because the Hydration Translator
    ///     does not have the context of the artifact at the time commands are created.
    /// </summary>
    internal class CreateOrUpdateConceptualConstraintOrMapping : Command
    {
        // AssociationSetMapping properties
        internal Association Association { get; set; }
        internal AssociationSet AssociationSet { get; set; }
        internal EntityContainerMapping EntityContainerMapping { get; set; }
        internal StorageEntitySet StorageEntitySet { get; set; }
        internal IEnumerable<Property> StorageDependentTypeForeignKeyProperties { get; set; }
        internal IEnumerable<Property> StorageDependentTypeKeyProperties { get; set; }
        internal ConceptualEntityType ConceptualPrincipalType { get; set; }
        internal ConceptualEntityType ConceptualDependentType { get; set; }

        // ReferentialConstraint properties
        internal AssociationEnd PrincipalEnd { get; set; }
        internal AssociationEnd DependentEnd { get; set; }
        internal IEnumerable<Property> PrincipalProperties { get; set; }

        // Shared properties between AssociationSetMapping and ReferentialConstraint
        internal IEnumerable<Property> DependentProperties { get; set; }

        internal bool IncludeFkProperties { get; set; }
        internal bool UseReferentialConstraint { get; set; }

        internal CreateOrUpdateConceptualConstraintOrMapping(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
        }

        internal bool IsFkMappingSettingValid
        {
            get
            {
                // If we're using referential constraints, IncludeFkProperties must be set so that the referential constraint can
                // reference the dependent fk property.
                return !UseReferentialConstraint || IncludeFkProperties;
            }
        }

        protected override void PreInvoke(CommandProcessorContext cpc)
        {
            base.PreInvoke(cpc);

            if (!IsFkMappingSettingValid)
            {
                throw new InvalidOperationException("Cannot use referential constraints unless foreign key properties are included");
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            DesignerInfo designerInfo;

            Debug.Assert(cpc.Artifact != null, "Artifact was null");
            if (Association != null
                && cpc.Artifact != null
                && cpc.Artifact.DesignerInfo() != null
                && cpc.Artifact.DesignerInfo().TryGetDesignerInfo(OptionsDesignerInfo.ElementName, out designerInfo))
            {
                // APPDB_SCENARIO: We cannot use referential constraints for 0..1:0..1 or 1:1 associations, since these exist as configured
                //                 0..1:* or 1:* associations and so introducing a referential constraint would cause validation errors.
                // Must use Referential Constraint for 1:0..1 relationship as using an AssociationSetMapping results in illegal reference to the same ID column twice (since the PK is also the FK)
                if (Association.IsOneToZeroOrOne
                    || (UseReferentialConstraint && !(Association.IsZeroOrOneToZeroOrOne || Association.IsOneToOne)))
                {
                    // We're including fk columns, so the update will consist of a ref constraint
                    var createRefConCommand = new CreateOrUpdateReferentialConstraintCommand(
                        (c, subCpc) =>
                            {
                                var cmd = c as CreateOrUpdateReferentialConstraintCommand;
                                cmd.PrincipalEnd = PrincipalEnd;
                                cmd.DependentEnd = DependentEnd;
                                cmd.PrincipalProperties = PrincipalProperties;
                                cmd.DependentProperties = DependentProperties;

                                return cmd.PrincipalEnd != null && cmd.DependentEnd != null;
                            });

                    CommandProcessor.InvokeSingleCommand(cpc, createRefConCommand);
                }
                else
                {
                    // We're not including fk columns, so the update will consist of an association set mapping and a deletes of the fk columns (if they exist)
                    // otherwise update AssociationSetMapping appropriately
                    var createMapCommand = new CreateOrUpdateAssociationSetMappingCommand(
                        (c, subCpc) =>
                            {
                                var cmd = c as CreateOrUpdateAssociationSetMappingCommand;
                                cmd.Association = Association;
                                cmd.AssociationSet = AssociationSet;
                                cmd.EntityContainerMapping = EntityContainerMapping;
                                cmd.StorageEntitySet = StorageEntitySet;

                                return cmd.Association != null && cmd.AssociationSet != null && cmd.EntityContainerMapping != null
                                       && cmd.StorageEntitySet != null;
                            });

                    CommandProcessor.InvokeSingleCommand(cpc, createMapCommand);

                    // Delete the fk properties in the conceptual layer if they exist. Do not delete primary key properties though!
                    if (!IncludeFkProperties)
                    {
                        var propertiesToDelete =
                            DependentProperties.Where(p => p.EntityType != null && !p.EntityType.ResolvableKeys.Contains(p)).ToList();
                        foreach (var p in propertiesToDelete)
                        {
                            var deletePropertyCmd = new DeletePropertyCommand(
                                (c, subCpc) =>
                                    {
                                        var cmd = c as DeletePropertyCommand;
                                        cmd.EFElement = p;
                                        return cmd.EFElement != null;
                                    });

                            CommandProcessor.InvokeSingleCommand(cpc, deletePropertyCmd);
                        }
                    }

                    // Add or update the EndProperty elements for the AssociationSetMapping. Try to work out which end is the principal
                    // end by looking at the multiplicity, since we don't have a referential constraint in this case.
                    AssociationSetEnd principalSetEnd;
                    AssociationSetEnd dependentSetEnd;

                    Debug.Assert(
                        AssociationSet.AssociationSetEnds().First().Role.Target != null,
                        "Role Target for Association End was null, AssociationSetMapping update failed");
                    if (AssociationSet.AssociationSetEnds().First().Role.Target != null)
                    {
                        if (Association.End1.Multiplicity.Value == ModelConstants.Multiplicity_Many)
                        {
                            principalSetEnd = AssociationSet.AssociationSetEnds().Last();
                            dependentSetEnd = AssociationSet.AssociationSetEnds().First();
                        }
                        else
                        {
                            principalSetEnd = AssociationSet.AssociationSetEnds().First();
                            dependentSetEnd = AssociationSet.AssociationSetEnds().Last();
                        }

                        var dependentEndPropertyCmd = new CreateOrUpdateEndPropertyCommand(
                            (c, subCpc) =>
                                {
                                    var cmd = c as CreateOrUpdateEndPropertyCommand;
                                    cmd.AssociationSetEnd = dependentSetEnd;
                                    cmd.AssociationSetMapping = createMapCommand.AssociationSetMapping;
                                    cmd.StorageKeyProperties = StorageDependentTypeKeyProperties;
                                    cmd.ConceptualKeyProperties =
                                        ConceptualDependentType.SafeInheritedAndDeclaredProperties.Where(p => p.IsKeyProperty);

                                    return cmd.AssociationSetEnd != null && cmd.AssociationSetMapping != null;
                                });

                        var principalEndPropertyCmd = new CreateOrUpdateEndPropertyCommand(
                            (c, subCpc) =>
                                {
                                    var cmd = c as CreateOrUpdateEndPropertyCommand;
                                    cmd.AssociationSetEnd = principalSetEnd;
                                    cmd.AssociationSetMapping = createMapCommand.AssociationSetMapping;
                                    cmd.StorageKeyProperties = StorageDependentTypeForeignKeyProperties;
                                    cmd.ConceptualKeyProperties =
                                        ConceptualPrincipalType.SafeInheritedAndDeclaredProperties.Where(p => p.IsKeyProperty);

                                    return cmd.AssociationSetEnd != null && cmd.AssociationSetMapping != null;
                                });

                        CommandProcessor.InvokeSingleCommand(cpc, dependentEndPropertyCmd);
                        CommandProcessor.InvokeSingleCommand(cpc, principalEndPropertyCmd);
                    }
                }
            }
        }
    }
}
