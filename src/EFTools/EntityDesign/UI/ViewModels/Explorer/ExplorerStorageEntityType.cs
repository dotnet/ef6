// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.Explorer
{
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model.Entity;

    // ExplorerConceptualEntityType must be distinguished from 
    // ExplorerStorageEntityType in order to allow the XAML
    // to load different images for them
    internal class ExplorerStorageEntityType : ExplorerEntityType
    {
        public ExplorerStorageEntityType(EditingContext context, EntityType entityType, ExplorerEFElement parent)
            : base(context, entityType, parent)
        {
            // do nothing
        }

        internal override string ExplorerImageResourceKeyName
        {
            get { return "TablePngIcon"; }
        }
    }
}
