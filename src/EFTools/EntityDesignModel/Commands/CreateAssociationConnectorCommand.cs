// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class CreateAssociationConnectorCommand : Command
    {
        private readonly Diagram _diagram;
        private readonly Association _association;
        private AssociationConnector _created;

        internal CreateAssociationConnectorCommand(Diagram diagram, Association association)
        {
            CommandValidation.ValidateAssociation(association);
            Debug.Assert(diagram != null, "diagram is null");

            _diagram = diagram;
            _association = association;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            var associationConnector = new AssociationConnector(_diagram, null);
            _diagram.AddAssociationConnector(associationConnector);

            associationConnector.Association.SetRefName(_association);

            XmlModelHelper.NormalizeAndResolve(associationConnector);

            _created = associationConnector;
        }

        internal AssociationConnector AssociationConnector
        {
            get { return _created; }
        }
    }
}
