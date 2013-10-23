// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.Explorer
{
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.VisualStudio;

    internal abstract class EntityDesignExplorerEFElement : ExplorerEFElement
    {
        protected EntityDesignExplorerEFElement(EditingContext context, EFElement modelItem, ExplorerEFElement parent)
            : base(context, modelItem, parent)
        {
        }

        protected override void InsertChild(EFElement efElementToInsert)
        {
            if (efElementToInsert is Documentation)
            {
                // the ViewModel does not keep track of Documentation
                // elements for any ExplorerEFElement but it is not 
                // an error - so just return
                return;
            }

            base.InsertChild(efElementToInsert);
        }

        // Override EditableName to catch CommandValidationFailedException
        public override string EditableName
        {
            get { return base.EditableName; }
            set
            {
                try
                {
                    base.EditableName = value;
                }
                catch (CommandValidationFailedException cvfe)
                {
                    // if attempt to rename fails show error dialog and
                    // re-throw which will cause calling code to revert
                    // display name to original name
                    VsUtils.ShowErrorDialog(cvfe.Message);
                    throw;
                }
            }
        }
    }
}
