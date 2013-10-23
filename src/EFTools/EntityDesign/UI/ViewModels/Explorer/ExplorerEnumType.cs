// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.Explorer
{
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class ExplorerEnumType : EntityDesignExplorerEFElement
    {
        public ExplorerEnumType(EditingContext context, EnumType enumType, ExplorerEFElement parent)
            : base(context, enumType, parent)
        {
            // do nothing
        }

        // whether the name of the Enum Type is editable inline in the Explorer
        public override bool IsEditableInline
        {
            get { return true; }
        }

        protected override void InsertChild(EFElement efElementToInsert)
        {
            // do nothing
        }

        protected override void LoadChildrenFromModel()
        {
            // do nothing.
        }

        protected override void LoadWpfChildrenCollection()
        {
            // do nothing
        }

        internal override string ExplorerImageResourceKeyName
        {
            get { return "EnumPngIcon"; }
        }
    }
}
