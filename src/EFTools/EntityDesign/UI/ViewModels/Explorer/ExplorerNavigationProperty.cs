// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.Explorer
{
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class ExplorerNavigationProperty : EntityDesignExplorerEFElement
    {
        public ExplorerNavigationProperty(EditingContext context, NavigationProperty navigationProperty, ExplorerEFElement parent)
            : base(context, navigationProperty, parent)
        {
            // do nothing
        }

        protected override void LoadChildrenFromModel()
        {
            // do nothing
        }

        protected override void LoadWpfChildrenCollection()
        {
            // do nothing
        }

        internal override string ExplorerImageResourceKeyName
        {
            get { return "NavigationPropertyPngIcon"; }
        }
    }
}
