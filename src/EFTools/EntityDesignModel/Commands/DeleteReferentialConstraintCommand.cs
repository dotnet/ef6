// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class DeleteReferentialConstraintCommand : DeleteEFElementCommand
    {
        private List<Property> _dependentProperties;

        internal DeleteReferentialConstraintCommand(ReferentialConstraint referentialConstraint)
            : base(referentialConstraint)
        {
        }

        internal DeleteReferentialConstraintCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
        }

        internal IList<Property> DependentProperties
        {
            get { return _dependentProperties; }
        }

        protected override void PreInvoke(CommandProcessorContext cpc)
        {
            _dependentProperties = new List<Property>();
            var referentialConstraint = EFElement as ReferentialConstraint;
            base.PreInvoke(cpc);

            if (referentialConstraint != null
                && referentialConstraint.Dependent != null)
            {
                foreach (var property in referentialConstraint.Dependent.Properties)
                {
                    _dependentProperties.Add(property);
                }
            }
        }
    }
}
