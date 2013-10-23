// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    internal class CreateEndPropertyCommand : Command
    {
        internal static readonly string PrereqId = "CreateEndPropertyCommand";

        internal AssociationSetMapping AssociationSetMapping { get; set; }
        internal AssociationSetEnd AssociationSetEnd { get; set; }
        private EndProperty _created;

        internal CreateEndPropertyCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
        }

        internal CreateEndPropertyCommand(AssociationSetMapping associationSetMapping, AssociationSetEnd associationSetEnd)
            : base(PrereqId)
        {
            AssociationSetMapping = associationSetMapping;
            AssociationSetEnd = associationSetEnd;
        }

        internal CreateEndPropertyCommand(CreateAssociationSetMappingCommand prereq, AssociationSetEnd associationSetEnd)
            : base(PrereqId)
        {
            AssociationSetEnd = associationSetEnd;
            AddPreReqCommand(prereq);
        }

        protected override void ProcessPreReqCommands()
        {
            if (AssociationSetMapping == null)
            {
                var prereq = GetPreReqCommand(CreateAssociationSetMappingCommand.PrereqId) as CreateAssociationSetMappingCommand;
                if (prereq != null)
                {
                    AssociationSetMapping = prereq.AssociationSetMapping;
                }

                Debug.Assert(AssociationSetMapping != null, "We didn't get a good AssociationSetMapping out of the Command");
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            var end = new EndProperty(AssociationSetMapping, null);
            end.Name.SetRefName(AssociationSetEnd);
            AssociationSetMapping.AddEndProperty(end);

            XmlModelHelper.NormalizeAndResolve(end);

            Debug.Assert(end.Name.Target != null, "Could not resolve AssociationSetEnd in an EndProperty");
            _created = end;
        }

        internal EndProperty EndProperty
        {
            get { return _created; }
        }
    }
}
