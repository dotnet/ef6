// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.Explorer
{
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class ExplorerConceptualProperty : ExplorerProperty
    {
        public ExplorerConceptualProperty(EditingContext context, Property property, ExplorerEFElement parent)
            : base(context, property, parent)
        {
            // do nothing
        }

        public override bool IsEditableInline
        {
            get
            {
                // Conceptual property names are editable inline if they are a 
                // within a Complex Type whether they are scalar properties or
                // complex properties
                Property prop = ModelItem as ConceptualProperty;
                if (null == prop)
                {
                    prop = ModelItem as ComplexConceptualProperty;
                }
                if (null != prop)
                {
                    var ct = prop.Parent as ComplexType;
                    if (null != ct)
                    {
                        return true;
                    }
                }

                return base.IsEditableInline;
            }
        }

        internal override string ExplorerImageResourceKeyName
        {
            get
            {
                if (ModelItem is ComplexConceptualProperty)
                {
                    return "ComplexPropertyPngIcon";
                }
                else
                {
                    if (IsKeyProperty)
                    {
                        return "PropertyKeyPngIcon";
                    }
                    else
                    {
                        return "PropertyPngIcon";
                    }
                }
            }
        }
    }
}
