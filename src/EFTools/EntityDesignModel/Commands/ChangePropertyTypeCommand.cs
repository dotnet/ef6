// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class ChangePropertyTypeCommand : Command
    {
        public Property Property { get; set; }
        internal string NewTypeName { get; set; }

        public ChangePropertyTypeCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
        }

        internal ChangePropertyTypeCommand(Property property, string newType)
        {
            CommandValidation.ValidateProperty(property);
            ValidateString(newType);

            Property = property;
            NewTypeName = newType;
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ConceptualProperty")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "StorageProperty")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "InvokeInternal")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            Debug.Assert(Property != null, "InvokeInternal is called when Property is null.");

            if (Property == null)
            {
                throw new InvalidOperationException("InvokeInternal is called when Property is null.");
            }

            var concProp = Property as ConceptualProperty;
            var storeProp = Property as StorageProperty;

            if (concProp == null
                && storeProp == null)
            {
                throw new InvalidOperationException("InvokeInternal is called when the Property is neither a ConceptualProperty nor a StorageProperty");
            }

            var typeName = (concProp != null ? concProp.TypeName : storeProp.TypeName);

            if (String.Compare(typeName, NewTypeName, StringComparison.Ordinal) == 0)
            {
                // no change needed
                return;
            }

            // Remove all facets for previous type (except Nullable - we persist the setting of that across types)
            Property.RemoveAllFacetsExceptNullable();

            if (concProp != null)
            {
                var cModel = (ConceptualEntityModel)concProp.GetParentOfType(typeof(ConceptualEntityModel));
                Debug.Assert(cModel != null, "Unable to find ConceptualModel for property:" + concProp.DisplayName);

                if (cModel != null)
                {
                    var enumType = ModelHelper.FindEnumType(cModel, NewTypeName);
                    if (enumType != null)
                    {
                        concProp.ChangePropertyType(enumType);
                    }
                    else
                    {
                        concProp.ChangePropertyType(NewTypeName);
                    }
                }
            }
            else
            {
                storeProp.Type.Value = NewTypeName;
            }
        }
    }
}
