// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class DeleteAssociationCommand : DeleteEFElementCommand
    {
        internal string DeletedAssociationName { get; private set; }

        protected internal Association Association
        {
            get
            {
                var elem = EFElement as Association;
                Debug.Assert(elem != null, "underlying element does not exist or is not an Association");
                if (elem == null)
                {
                    throw new InvalidModelItemException();
                }
                return elem;
            }
        }

        internal DeleteAssociationCommand(Association association)
            : base(association)
        {
            CommandValidation.ValidateAssociation(association);

            SaveDeletedInformation();
        }

        internal DeleteAssociationCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
        }

        private void SaveDeletedInformation()
        {
            DeletedAssociationName = Association.Name.Value;
        }

        protected override void PreInvoke(CommandProcessorContext cpc)
        {
            // We need to save off the deleted association name so that it can be identified by the Storage->Conceptual
            // translator after the element is gone.
            SaveDeletedInformation();

            base.PreInvoke(cpc);
        }
    }
}
