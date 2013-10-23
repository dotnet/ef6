// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class CreateOrUpdateEndPropertyCommand : CreateEndPropertyCommand
    {
        internal IEnumerable<Property> ConceptualKeyProperties { get; set; }
        internal IEnumerable<Property> StorageKeyProperties { get; set; }

        internal CreateOrUpdateEndPropertyCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            var endProperty = AssociationSetMapping.EndProperties().FirstOrDefault(
                ep =>
                string.Equals(ep.Name.XAttribute.Value, AssociationSetEnd.GetRefNameForBinding(ep.Name), StringComparison.CurrentCulture));

            // EndProperty does not exist, create it
            if (endProperty == null)
            {
                base.InvokeInternal(cpc);
                endProperty = EndProperty;
                Debug.Assert(endProperty != null, "Could not create end property");
            }

            // Update Scalar properties
            if (endProperty != null)
            {
                var oldScalarProperties = endProperty.ScalarProperties().ToList();
                foreach (var oldProperty in oldScalarProperties)
                {
                    oldProperty.Delete();
                }

                Debug.Assert(
                    ConceptualKeyProperties.Count() == StorageKeyProperties.Count(),
                    "Found different number of keys in storage and conceptual models");

                var conceptualEnumerator = ConceptualKeyProperties.GetEnumerator();
                var storageEnumerator = StorageKeyProperties.GetEnumerator();

                while (conceptualEnumerator.MoveNext()
                       && storageEnumerator.MoveNext())
                {
                    var createEndScalarCommand = new CreateEndScalarPropertyCommand(
                        endProperty, conceptualEnumerator.Current, storageEnumerator.Current);

                    CommandProcessor.InvokeSingleCommand(cpc, createEndScalarCommand);
                }
            }
        }
    }
}
