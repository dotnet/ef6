// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.Explorer
{
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class ExplorerStorageAssociation : ExplorerAssociation
    {
        public ExplorerStorageAssociation(EditingContext context, Association assoc, ExplorerEFElement parent)
            : base(context, assoc, parent)
        {
            // do nothing
        }

        internal override string ExplorerImageResourceKeyName
        {
            get { return "ForeignKeyPngIcon"; }
        }
    }
}
