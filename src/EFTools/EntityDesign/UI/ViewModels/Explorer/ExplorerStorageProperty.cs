// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.Explorer
{
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class ExplorerStorageProperty : ExplorerProperty
    {
        public ExplorerStorageProperty(EditingContext context, Property property, ExplorerEFElement parent)
            : base(context, property, parent)
        {
            // do nothing
        }

        internal override string ExplorerImageResourceKeyName
        {
            get
            {
                if (IsKeyProperty)
                {
                    return "TableKeyColumnPngIcon";
                }
                else
                {
                    return "TableColumnPngIcon";
                }
            }
        }
    }
}
