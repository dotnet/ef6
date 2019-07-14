// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class CreateNavigationPropertyCommand : Command
    {
        internal static readonly string PrereqId = "CreateNavigationPropertyCommand";

        internal string Name { get; set; }
        internal ConceptualEntityType Entity { get; set; }
        internal Association Association { get; set; }
        internal AssociationEnd FromEnd { get; set; }
        internal AssociationEnd ToEnd { get; set; }
        private NavigationProperty _createdNavigationProperty;

        internal CreateNavigationPropertyCommand(
            string name, ConceptualEntityType entity, Association association, AssociationEnd end1, AssociationEnd end2)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            Name = name;
            Entity = entity;
            Association = association;
            FromEnd = end1;
            ToEnd = end2;
        }

        internal CreateNavigationPropertyCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
        }

        internal NavigationProperty NavigationProperty
        {
            get { return _createdNavigationProperty; }
        }

        internal static NavigationProperty CreateDefaultProperty(CommandProcessorContext cpc, string name, ConceptualEntityType entity)
        {
            if (cpc == null)
            {
                throw new ArgumentNullException("cpc");
            }
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            var cpcd = new CreateNavigationPropertyCommand(name, entity, null, null, null);

            var cp = new CommandProcessor(cpc, cpcd);
            cp.Invoke();

            return cpcd.NavigationProperty;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // we allow the command to set null _endpoints if needed.
            var navProp1 = new NavigationProperty(Entity, null);
            navProp1.LocalName.Value = Name;
            navProp1.Relationship.SetRefName(Association);
            navProp1.FromRole.SetRefName(FromEnd);
            navProp1.ToRole.SetRefName(ToEnd);
            Entity.AddNavigationProperty(navProp1);
            _createdNavigationProperty = navProp1;
            XmlModelHelper.NormalizeAndResolve(navProp1);
        }
    }
}
