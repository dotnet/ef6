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
    ///     Rule fired when an NavigationProperty changes
    /// </summary>
    [RuleOn(typeof(Property), FireTime = TimeToFire.TopLevelCommit)]
    internal sealed class Property_AddRule : AddRule
    {
        /// <summary>
        ///     Do the following when an Entity changes:
        ///     - Update roles in related Associations
        /// </summary>
        /// <param name="e"></param>
        public override void ElementAdded(ElementAddedEventArgs e)
        {
            base.ElementAdded(e);

            var addedProperty = e.ModelElement as Property;
            Debug.Assert(addedProperty != null);
            Debug.Assert(addedProperty.EntityType != null && addedProperty.EntityType.EntityDesignerViewModel != null);

            if (addedProperty != null
                && addedProperty.EntityType != null
                && addedProperty.EntityType.EntityDesignerViewModel != null)
            {
                var tx = ModelUtils.GetCurrentTx(e.ModelElement.Store);
                Debug.Assert(tx != null);
                if (tx != null
                    && !tx.IsSerializing)
                {
                    ViewModelChangeContext.GetNewOrExistingContext(tx).ViewModelChanges.Add(new PropertyAdd(addedProperty));
                }
            }
        }
    }
}
