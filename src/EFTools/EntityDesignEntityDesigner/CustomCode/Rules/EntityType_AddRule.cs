// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Model = Microsoft.Data.Entity.Design.Model.Entity;

namespace Microsoft.Data.Entity.Design.EntityDesigner.Rules
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges;
    using Microsoft.Data.Entity.Design.EntityDesigner.Utils;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VisualStudio.Modeling;
    using Microsoft.VisualStudio.Modeling;

    /// <summary>
    ///     Rule fired when an EntityType is created
    /// </summary>
    [RuleOn(typeof(EntityType), FireTime = TimeToFire.TopLevelCommit)]
    internal sealed class EntityType_AddRule : AddRule
    {
        /// <summary>
        ///     Do the following when a new EntityType shape is created:
        ///     - Add the new EntityType to the model
        /// </summary>
        /// <param name="e"></param>
        public override void ElementAdded(ElementAddedEventArgs e)
        {
            base.ElementAdded(e);

            var addedEntity = e.ModelElement as EntityType;
            Debug.Assert(addedEntity != null);
            Debug.Assert(addedEntity.EntityDesignerViewModel != null);

            if ((addedEntity != null)
                && (addedEntity.EntityDesignerViewModel != null))
            {
                var viewModel = addedEntity.EntityDesignerViewModel;
                Debug.Assert(viewModel != null);

                var tx = ModelUtils.GetCurrentTx(e.ModelElement.Store);
                Debug.Assert(tx != null, "Make sure we have a Current Active Tx");
                if (tx != null
                    && !tx.IsSerializing)
                {
                    // Remove the added DSL EntityType.
                    // When Escher model is updated, there will be a code that will create the EntityType back
                    viewModel.EntityTypes.Remove(addedEntity);
                    addedEntity.Delete();

                    // create the model change and add it to the current transaction changelist
                    ViewModelChangeContext.GetNewOrExistingContext(tx).ViewModelChanges.Add(new EntityTypeAdd());
                }
            }
        }
    }
}
