// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.VersioningFacade;

    internal class CreateReferentialConstraintCommand : Command
    {
        internal AssociationEnd PrincipalEnd { get; set; }
        internal AssociationEnd DependentEnd { get; set; }
        internal IEnumerable<Property> PrincipalProperties { get; set; }
        internal IEnumerable<Property> DependentProperties { get; set; }
        private ReferentialConstraint _createdRefConstraint;

        internal CreateReferentialConstraintCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
        }

        /// <summary>
        ///     Creates a ReferentialConstraint in the association that is the parent of the principal
        ///     End.
        /// </summary>
        /// <param name="principalEnd"></param>
        /// <param name="dependentEnd"></param>
        /// <param name="principalProperties"></param>
        /// <param name="dependentProperties"></param>
        internal CreateReferentialConstraintCommand(
            AssociationEnd principalEnd, AssociationEnd dependentEnd,
            IEnumerable<Property> principalProperties, IEnumerable<Property> dependentProperties)
        {
            CommandValidation.ValidateAssociationEnd(principalEnd);
            CommandValidation.ValidateAssociationEnd(dependentEnd);

            PrincipalEnd = principalEnd;
            DependentEnd = dependentEnd;
            PrincipalProperties = principalProperties;
            DependentProperties = dependentProperties;
        }

        /// <summary>
        ///     Creates a ReferentialConstraint in the association that was created by the prereq command.
        /// </summary>
        /// <param name="prereq"></param>
        /// <param name="principalProperties"></param>
        /// <param name="dependentProperties"></param>
        internal CreateReferentialConstraintCommand(
            CreateStorageAssociationCommand prereq,
            IEnumerable<Property> principalProperties, IEnumerable<Property> dependentProperties)
        {
            ValidatePrereqCommand(prereq);

            PrincipalProperties = principalProperties;
            DependentProperties = dependentProperties;

            AddPreReqCommand(prereq);
        }

        protected override void ProcessPreReqCommands()
        {
            if (PrincipalEnd == null
                || DependentEnd == null)
            {
                var prereq = GetPreReqCommand(CreateStorageAssociationCommand.PrereqId) as CreateStorageAssociationCommand;
                if (prereq != null)
                {
                    PrincipalEnd = prereq.AssociationPkEnd;
                    DependentEnd = prereq.AssociationFkEnd;
                }

                Debug.Assert(PrincipalEnd != null && DependentEnd != null, "Didn't get valid ends from the prereq command");
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "InvokeInternal")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "PrincipalEnd")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DependentEnd")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            Debug.Assert(
                PrincipalEnd != null && DependentEnd != null, "InvokeInternal is called when PrincipalEnd or DependentEnd is null.");
            if (PrincipalEnd == null
                || DependentEnd == null)
            {
                throw new InvalidOperationException("InvokeInternal is called when PrincipalEnd or DependentEnd is null.");
            }

            var association = PrincipalEnd.Parent as Association;
            Debug.Assert(
                association != null && association == DependentEnd.Parent, "Association parent for both ends must agree and be not null");

            var principalProps = PrincipalProperties.ToList();
            var dependentProps = DependentProperties.ToList();

            Debug.Assert(principalProps.Count == dependentProps.Count, "Number of principal and dependent properties must agree");
            Debug.Assert(principalProps.Count > 0, "Number of properties must be positive");

            var referentialConstraint = new ReferentialConstraint(association, null);
            association.ReferentialConstraint = referentialConstraint;
            XmlModelHelper.NormalizeAndResolve(referentialConstraint);

            var principalRole = new ReferentialConstraintRole(referentialConstraint, null);
            var dependentRole = new ReferentialConstraintRole(referentialConstraint, null);

            var service = cpc.EditingContext.GetEFArtifactService();
            // we can't pass the type of referential constraint role ("Principal" or "Dependent")
            // in the constructor because XElement gets created in base constructor
            // before we have a chance to set any properties
            if (association.EntityModel.IsCSDL)
            {
                var csdlNamespaceName = SchemaManager.GetCSDLNamespaceName(service.Artifact.SchemaVersion);
                principalRole.XElement.Name = XName.Get(ReferentialConstraint.ElementNamePrincipal, csdlNamespaceName);
                dependentRole.XElement.Name = XName.Get(ReferentialConstraint.ElementNameDependent, csdlNamespaceName);
            }
            else
            {
                var ssdlNamespaceName = SchemaManager.GetSSDLNamespaceName(service.Artifact.SchemaVersion);
                principalRole.XElement.Name = XName.Get(ReferentialConstraint.ElementNamePrincipal, ssdlNamespaceName);
                dependentRole.XElement.Name = XName.Get(ReferentialConstraint.ElementNameDependent, ssdlNamespaceName);
            }

            principalRole.Role.SetRefName(PrincipalEnd);
            dependentRole.Role.SetRefName(DependentEnd);

            referentialConstraint.Principal = principalRole;
            referentialConstraint.Dependent = dependentRole;

            XmlModelHelper.NormalizeAndResolve(principalRole);
            XmlModelHelper.NormalizeAndResolve(dependentRole);

            for (var i = 0; i < principalProps.Count; i++)
            {
                principalRole.AddPropertyRef(principalProps[i]);
                dependentRole.AddPropertyRef(dependentProps[i]);
            }

            Debug.Assert(
                principalProps.Count == 0
                || (principalRole.PropertyRefs.First().Name.Target != null && dependentRole.PropertyRefs.First().Name.Target != null),
                "Unresolved property references");

            _createdRefConstraint = referentialConstraint;
        }

        internal ReferentialConstraint ReferentialConstraint
        {
            get { return _createdRefConstraint; }
        }

        internal void SetCreatedReferentialConstraint(ReferentialConstraint refConstraint)
        {
            _createdRefConstraint = refConstraint;
        }
    }
}
