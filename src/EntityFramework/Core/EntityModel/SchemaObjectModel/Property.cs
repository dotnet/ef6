// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.EntityModel.SchemaObjectModel
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Xml;

    internal abstract class Property : SchemaElement
    {
        /// <summary>
        /// Creates a Property object
        /// </summary>
        /// <param name="parentElement">The parent element</param>
        internal Property(StructuredType parentElement)
            : base(parentElement)
        {
        }

        /// <summary>
        /// Gets the Type of the property
        /// </summary>
        public abstract SchemaType Type { get; }

        protected override bool HandleElement(XmlReader reader)
        {
            if (base.HandleElement(reader))
            {
                return true;
            }
            else if (Schema.DataModel
                     == SchemaDataModelOption.EntityDataModel)
            {
                if (CanHandleElement(reader, XmlConstants.ValueAnnotation))
                {
                    // EF does not support this EDM 3.0 element, so ignore it.
                    SkipElement(reader);
                    return true;
                }
                else if (CanHandleElement(reader, XmlConstants.TypeAnnotation))
                {
                    // EF does not support this EDM 3.0 element, so ignore it.
                    SkipElement(reader);
                    return true;
                }
            }
            return false;
        }
    }
}
