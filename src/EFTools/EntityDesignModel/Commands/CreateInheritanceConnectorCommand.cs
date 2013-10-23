// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class CreateInheritanceConnectorCommand : Command
    {
        private readonly Diagram _diagram;
        private readonly EntityType _entity;
        private InheritanceConnector _created;

        internal CreateInheritanceConnectorCommand(Diagram diagram, EntityType entity)
        {
            CommandValidation.ValidateConceptualEntityType(entity);
            Debug.Assert(diagram != null, "diagram is null");

            _diagram = diagram;
            _entity = entity;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            var inheritanceConnector = new InheritanceConnector(_diagram, null);
            _diagram.AddInheritanceConnector(inheritanceConnector);

            inheritanceConnector.EntityType.SetRefName(_entity);

            XmlModelHelper.NormalizeAndResolve(inheritanceConnector);

            _created = inheritanceConnector;
        }

        internal InheritanceConnector InheritanceConnector
        {
            get { return _created; }
        }
    }
}
