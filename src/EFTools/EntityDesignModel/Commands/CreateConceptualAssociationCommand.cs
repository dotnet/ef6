// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Pluralization;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Integrity;
    using Microsoft.Data.Entity.Design.VersioningFacade;

    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    internal class CreateConceptualAssociationCommand : Command
    {
        internal static readonly string PrereqId = "CreateConceptualAssociationCommand";

        internal string Name { get; set; }
        internal ConceptualEntityType End1Entity { get; set; }
        internal ConceptualEntityType End2Entity { get; set; }
        internal string End1Multiplicity { get; set; }
        internal string End2Multiplicity { get; set; }
        internal bool ShouldCreateForeignKeyProperties { get; set; }
        private AssociationEnd _end1;
        private AssociationEnd _end2;
        internal string NavigationPropertyInEnd1Entity { get; set; }
        internal string NavigationPropertyInEnd2Entity { get; set; }
        internal bool UniquifyNames { get; set; }

        protected bool ShouldCreateNavigationPropertyEnd1
        {
            get { return !string.IsNullOrEmpty(NavigationPropertyInEnd1Entity); }
        }

        protected bool ShouldCreateNavigationPropertyEnd2
        {
            get { return !string.IsNullOrEmpty(NavigationPropertyInEnd2Entity); }
        }

        protected internal Association CreatedAssociation { get; protected set; }

        internal AssociationEnd End1
        {
            get { return _end1; }
        }

        internal AssociationEnd End2
        {
            get { return _end2; }
        }

        internal CreateConceptualAssociationCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
        }

        internal CreateConceptualAssociationCommand(
            string name, ConceptualEntityType end1Entity, string end1Multiplicity, string end1NavigationProperty,
            ConceptualEntityType end2Entity, string end2Multiplicity, string end2NavigationProperty, bool uniquifyNames,
            bool createForeignKeyProperties)
            : base(PrereqId)
        {
            ValidateString(name);
            CommandValidation.ValidateConceptualEntityType(end1Entity);
            ValidateString(end1Multiplicity);
            CommandValidation.ValidateConceptualEntityType(end2Entity);
            ValidateString(end2Multiplicity);

            Name = name;

            End1Entity = end1Entity;
            End1Multiplicity = end1Multiplicity;
            NavigationPropertyInEnd1Entity = end1NavigationProperty;

            End2Entity = end2Entity;
            End2Multiplicity = end2Multiplicity;
            NavigationPropertyInEnd2Entity = end2NavigationProperty;

            UniquifyNames = uniquifyNames;
            ShouldCreateForeignKeyProperties = createForeignKeyProperties;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            var service = cpc.EditingContext.GetEFArtifactService();
            var artifact = service.Artifact;

            // the model that we want to add the association to
            var model = artifact.ConceptualModel();

            // check for uniqueness of the assocation and association set names
            // if uniquifyNames is true then make them unique, otherwise throw
            // an exception if they're not (always uniquify associationSetName
            // regardless as we get bugs if not)
            var assocName = Name;
            var assocSetName = assocName;
            if (UniquifyNames)
            {
                assocName = ModelHelper.GetUniqueName(typeof(Association), model, assocName);
                assocSetName = ModelHelper.GetUniqueName(typeof(AssociationSet), model.FirstEntityContainer, assocName);

                // ensure unique NavigationProperty names
                if (ShouldCreateNavigationPropertyEnd1
                    && !ModelHelper.IsUniquePropertyName(End1Entity, NavigationPropertyInEnd1Entity, true)
                    || NavigationPropertyInEnd1Entity == End1Entity.LocalName.Value)
                {
                    var namesToAvoid = new HashSet<string>();
                    namesToAvoid.Add(End1Entity.LocalName.Value);
                    namesToAvoid.Add(NavigationPropertyInEnd2Entity);
                    NavigationPropertyInEnd1Entity = ModelHelper.GetUniqueConceptualPropertyName(
                        NavigationPropertyInEnd1Entity, End1Entity, namesToAvoid);
                }

                if (ShouldCreateNavigationPropertyEnd2
                    && !ModelHelper.IsUniquePropertyName(End2Entity, NavigationPropertyInEnd2Entity, true)
                    || NavigationPropertyInEnd2Entity == End2Entity.LocalName.Value)
                {
                    var namesToAvoid = new HashSet<string> { End2Entity.LocalName.Value, NavigationPropertyInEnd1Entity };
                    NavigationPropertyInEnd2Entity = ModelHelper.GetUniqueConceptualPropertyName(
                        NavigationPropertyInEnd2Entity, End2Entity, namesToAvoid);
                }
            }
            else
            {
                assocSetName = ModelHelper.GetUniqueName(typeof(AssociationSet), model.FirstEntityContainer, assocName);

                string msg = null;
                if (!ModelHelper.IsUniqueName(typeof(Association), model, assocName, false, out msg))
                {
                    throw new InvalidOperationException(msg);
                }
                else if (!ModelHelper.IsUniqueName(typeof(AssociationSet), model.FirstEntityContainer, assocSetName, false, out msg))
                {
                    throw new InvalidOperationException(msg);
                }
                else if (ShouldCreateNavigationPropertyEnd1
                         && (!ModelHelper.IsUniquePropertyName(End1Entity, NavigationPropertyInEnd1Entity, true)))
                {
                    msg = string.Format(CultureInfo.CurrentCulture, Resources.NAME_NOT_UNIQUE, NavigationPropertyInEnd1Entity);
                    throw new InvalidOperationException(msg);
                }
                else if (ShouldCreateNavigationPropertyEnd2
                         && (!ModelHelper.IsUniquePropertyName(End2Entity, NavigationPropertyInEnd2Entity, true)))
                {
                    msg = string.Format(CultureInfo.CurrentCulture, Resources.NAME_NOT_UNIQUE, NavigationPropertyInEnd2Entity);
                    throw new InvalidOperationException(msg);
                }
                else if (NavigationPropertyInEnd1Entity == End1Entity.LocalName.Value)
                {
                    msg = string.Format(
                        CultureInfo.CurrentCulture, Resources.NavPropNameSameAsContainer, NavigationPropertyInEnd1Entity);
                    throw new InvalidOperationException(msg);
                }
                else if (NavigationPropertyInEnd2Entity == End2Entity.LocalName.Value)
                {
                    msg = string.Format(
                        CultureInfo.CurrentCulture, Resources.NavPropNameSameAsContainer, NavigationPropertyInEnd2Entity);
                    throw new InvalidOperationException(msg);
                }
            }

            // create the new item in our model
            var association = new Association(model, null);
            association.LocalName.Value = assocName;
            model.AddAssociation(association);
            XmlModelHelper.NormalizeAndResolve(association);

            // create the first end
            _end1 = new AssociationEnd(association, null);
            _end1.Type.SetRefName(End1Entity);
            _end1.Role.Value = End1Entity.LocalName.Value;
            _end1.Multiplicity.Value = End1Multiplicity;
            association.AddAssociationEnd(_end1);
            XmlModelHelper.NormalizeAndResolve(_end1);

            // create the second end
            _end2 = new AssociationEnd(association, null);
            _end2.Type.SetRefName(End2Entity);
            var endRoleValue = End2Entity.LocalName.Value;
            if (_end1.Role.Value.Equals(endRoleValue))
            {
                // avoid duplicate Role values between the two ends. This will occur in self-associations.
                // Appending "1" is consistent with how model-gen chooses a unique name.
                endRoleValue = endRoleValue + "1";
            }
            _end2.Role.Value = endRoleValue;
            _end2.Multiplicity.Value = End2Multiplicity;
            association.AddAssociationEnd(_end2);
            XmlModelHelper.NormalizeAndResolve(_end2);

            // create the association set for this association
            var cmd = new CreateAssociationSetCommand(assocSetName, association);
            CommandProcessor.InvokeSingleCommand(cpc, cmd);
            var set = cmd.AssociationSet;
            Debug.Assert(set != null, "unable to create association set");

            CreateNavigationPropertyCommand navcmd;

            if (ShouldCreateNavigationPropertyEnd1)
            {
                navcmd = new CreateNavigationPropertyCommand(NavigationPropertyInEnd1Entity, End1Entity, association, _end1, _end2);
                CommandProcessor.InvokeSingleCommand(cpc, navcmd);
            }

            if (ShouldCreateNavigationPropertyEnd2)
            {
                navcmd = new CreateNavigationPropertyCommand(NavigationPropertyInEnd2Entity, End2Entity, association, _end2, _end1);
                CommandProcessor.InvokeSingleCommand(cpc, navcmd);
            }

            if (ShouldCreateForeignKeyProperties)
            {
                CreateForeignKeyProperties.AddRule(cpc, association);
            }

            CreatedAssociation = association;
        }

        internal void SetCreatedAssociation(Association createdAssociation)
        {
            CreatedAssociation = createdAssociation;
        }

        internal static Association CreateAssociationAndAssociationSetWithDefaultNames(
            CommandProcessorContext cpc, ConceptualEntityType end1Entity, ConceptualEntityType end2Entity)
        {
            var service = cpc.EditingContext.GetEFArtifactService();
            var artifact = service.Artifact;

            // the model that we want to add the association to
            var model = artifact.ConceptualModel();
            if (model == null)
            {
                throw new CannotLocateParentItemException();
            }

            // Should have discovered the association name through the behavior service. Going back to default
            var associationName = ModelHelper.GetUniqueName(
                typeof(Association), model, end1Entity.LocalName.Value + end2Entity.LocalName.Value);

            // pluralization service is based on English only for Dev10
            IPluralizationService pluralizationService = null;
            var pluralize = ModelHelper.GetDesignerPropertyValueFromArtifactAsBool(
                OptionsDesignerInfo.ElementName,
                OptionsDesignerInfo.AttributeEnablePluralization, OptionsDesignerInfo.EnablePluralizationDefault, artifact);
            if (pluralize)
            {
                pluralizationService = DependencyResolver.GetService<IPluralizationService>();
            }

            var end1Multiplicity = ModelConstants.Multiplicity_One;
            var end2Multiplicity = ModelConstants.Multiplicity_Many;

            var proposedEnd1NavPropName = ModelHelper.ConstructProposedNavigationPropertyName(
                pluralizationService, end2Entity.LocalName.Value, end2Multiplicity);
            var end1NavigationPropertyName = ModelHelper.GetUniqueConceptualPropertyName(proposedEnd1NavPropName, end1Entity);
            var proposedEnd2NavPropName = ModelHelper.ConstructProposedNavigationPropertyName(
                pluralizationService, end1Entity.LocalName.Value, end1Multiplicity);
            var end2NavigationPropertyName = ModelHelper.GetUniqueConceptualPropertyName(
                proposedEnd2NavPropName, end2Entity, new HashSet<string> { end1NavigationPropertyName });

            var cac = new CreateConceptualAssociationCommand(
                associationName,
                end1Entity, end1Multiplicity, end1NavigationPropertyName,
                end2Entity, end2Multiplicity, end2NavigationPropertyName,
                false, // uniquify names
                false); // create foreign key properties
            var cp = new CommandProcessor(cpc, cac);
            cp.Invoke();

            return cac.CreatedAssociation;
        }
    }
}
