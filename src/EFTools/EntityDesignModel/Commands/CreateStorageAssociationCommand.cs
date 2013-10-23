// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class CreateStorageAssociationCommand : Command
    {
        internal static readonly string PrereqId = "CreateStorageAssociationCommand";

        internal string Name { get; set; }
        internal EntityType FkTable { get; set; }
        internal EntityType PkTable { get; set; }
        internal bool DoesFkFormPk { get; set; }
        internal bool IsNullableFk { get; set; }
        internal bool UniquifyNames { get; set; }
        internal string PkMultiplicityOverride { get; set; }
        internal string FkMultiplicityOverride { get; set; }
        internal string PkRoleNameOverride { get; set; }
        internal string FkRoleNameOverride { get; set; }

        private AssociationEnd _createdAssociationFkEnd;
        private AssociationEnd _createdAssociationPkEnd;

        internal CreateStorageAssociationCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
        }

        internal CreateStorageAssociationCommand(
            string name, EntityType fkTable, EntityType pkTable, bool doesFkFormPk, bool isNullableFk, bool uniquifyNames)
            : base(PrereqId)
        {
            Name = name;
            FkTable = fkTable;
            PkTable = pkTable;
            DoesFkFormPk = doesFkFormPk;
            IsNullableFk = isNullableFk;
            UniquifyNames = uniquifyNames;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            var service = cpc.EditingContext.GetEFArtifactService();
            var artifact = service.Artifact;

            // the model that we want to add the association to
            var model = artifact.StorageModel();

            // check for uniqueness
            var assocName = Name;
            var assocSetName = assocName;
            if (UniquifyNames)
            {
                assocName = ModelHelper.GetUniqueName(typeof(Association), model, assocName);
                assocSetName = ModelHelper.GetUniqueName(typeof(AssociationSet), model.FirstEntityContainer, assocName);
            }
            else
            {
                // check for uniqueness of the association name
                string msg = null;
                if (ModelHelper.IsUniqueName(typeof(Association), model, assocName, false, out msg) == false)
                {
                    throw new InvalidOperationException(msg);
                }

                // check for uniqueness of the association set name
                if (ModelHelper.IsUniqueName(typeof(AssociationSet), model.FirstEntityContainer, assocSetName, false, out msg) == false)
                {
                    throw new InvalidOperationException(msg);
                }
            }

            // create the new item in our model
            var association = new Association(model, null);
            association.LocalName.Value = assocName;
            model.AddAssociation(association);
            XmlModelHelper.NormalizeAndResolve(association);

            // create the ends of the association
            var fkEnd = new AssociationEnd(association, null);
            fkEnd.Type.SetRefName(FkTable);
            fkEnd.Role.Value = FkRoleNameOverride ?? ModelHelper.CreateFKAssociationEndName(FkTable.LocalName.Value);
            if (FkMultiplicityOverride != null)
            {
                fkEnd.Multiplicity.Value = FkMultiplicityOverride;
            }
            else
            {
                fkEnd.Multiplicity.Value = DoesFkFormPk ? ModelConstants.Multiplicity_ZeroOrOne : ModelConstants.Multiplicity_Many;
            }
            association.AddAssociationEnd(fkEnd);
            XmlModelHelper.NormalizeAndResolve(fkEnd);

            var pkEnd = new AssociationEnd(association, null);
            pkEnd.Type.SetRefName(PkTable);
            pkEnd.Role.Value = PkRoleNameOverride ?? ModelHelper.CreatePKAssociationEndName(PkTable.LocalName.Value);
            if (PkMultiplicityOverride != null)
            {
                pkEnd.Multiplicity.Value = PkMultiplicityOverride;
            }
            else
            {
                pkEnd.Multiplicity.Value = IsNullableFk ? ModelConstants.Multiplicity_ZeroOrOne : ModelConstants.Multiplicity_One;
            }
            association.AddAssociationEnd(pkEnd);
            XmlModelHelper.NormalizeAndResolve(pkEnd);

            var cmd = new CreateAssociationSetCommand(assocSetName, association, ModelSpace.Storage);
            CommandProcessor.InvokeSingleCommand(cpc, cmd);
            var set = cmd.AssociationSet;
            Debug.Assert(set != null, "failed to create an AssociationSet");

            Association = association;
            _createdAssociationFkEnd = fkEnd;
            _createdAssociationPkEnd = pkEnd;
        }

        /// <summary>
        ///     The Association that this command created
        /// </summary>
        protected internal Association Association { get; protected set; }

        /// <summary>
        ///     The AssociationEnd that this command created
        /// </summary>
        internal AssociationEnd AssociationFkEnd
        {
            get { return _createdAssociationFkEnd; }
        }

        /// <summary>
        ///     The AssociationEnd that this command created
        /// </summary>
        internal AssociationEnd AssociationPkEnd
        {
            get { return _createdAssociationPkEnd; }
        }
    }
}
