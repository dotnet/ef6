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
    ///     Rule fired when an Association is created
    /// </summary>
    [RuleOn(typeof(Association), FireTime = TimeToFire.TopLevelCommit)]
    internal sealed class Association_AddRule : AddRule
    {
        /// <summary>
        ///     Do the following when a new Association is created:
        ///     - Initialize the "End1" and "End2" properties (displayed on the connector decorators)
        ///     - Set the "Name" property to a sensible default
        ///     - Update the navigation property of the Source and Target entities
        /// </summary>
        public override void ElementAdded(ElementAddedEventArgs e)
        {
            base.ElementAdded(e);

            var addedAssociation = e.ModelElement as Association;

            Debug.Assert(addedAssociation != null);
            Debug.Assert(addedAssociation.SourceEntityType != null);
            Debug.Assert(addedAssociation.TargetEntityType != null);
            Debug.Assert(addedAssociation.SourceEntityType.EntityDesignerViewModel != null);

            if (addedAssociation != null
                && addedAssociation.SourceEntityType != null
                && addedAssociation.TargetEntityType != null
                && addedAssociation.SourceEntityType.EntityDesignerViewModel != null)
            {
                var tx = ModelUtils.GetCurrentTx(e.ModelElement.Store);
                Debug.Assert(tx != null);
                if (tx != null
                    && !tx.IsSerializing)
                {
                    // create the new association
                    ViewModelChangeContext.GetNewOrExistingContext(tx).ViewModelChanges.Add(new AssociationAdd(addedAssociation));
                }
            }
        }
    }
}
