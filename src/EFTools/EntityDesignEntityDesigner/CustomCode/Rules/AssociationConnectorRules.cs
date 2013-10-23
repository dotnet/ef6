// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.Rules
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges;
    using Microsoft.Data.Entity.Design.EntityDesigner.Utils;
    using Microsoft.Data.Entity.Design.EntityDesigner.View;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VisualStudio.Modeling;
    using Microsoft.VisualStudio.Modeling;
    using Microsoft.VisualStudio.Modeling.Diagrams;

    [RuleOn(typeof(AssociationConnector), FireTime = TimeToFire.TopLevelCommit)]
    internal sealed class AssociationConnector_AddRule : AddRule
    {
        public override void ElementAdded(ElementAddedEventArgs e)
        {
            var associationConnector = e.ModelElement as AssociationConnector;
            Debug.Assert(associationConnector != null);

            var tx = ModelUtils.GetCurrentTx(associationConnector.Store);
            Debug.Assert(tx != null);
            if (tx != null
                && !tx.IsSerializing)
            {
                ViewModelChangeContext.GetNewOrExistingContext(tx).ViewModelChanges.Add(new AssociationConnectorAdd(associationConnector));
            }
        }
    }

    [RuleOn(typeof(AssociationConnector), FireTime = TimeToFire.TopLevelCommit)]
    internal sealed class AssociationConnector_ChangeRule : ChangeRule
    {
        public override void ElementPropertyChanged(ElementPropertyChangedEventArgs e)
        {
            var associationConnector = e.ModelElement as AssociationConnector;
            Debug.Assert(associationConnector != null);

            if (associationConnector != null)
            {
                // for some reason when deleting connector, DSL invokes ChangeRule, so just return if it's deleted
                if (associationConnector.IsDeleted)
                {
                    return;
                }

                var tx = ModelUtils.GetCurrentTx(e.ModelElement.Store);
                Debug.Assert(tx != null);
                if (tx != null
                    && !tx.IsSerializing)
                {
                    if (e.DomainProperty.Id == LinkShape.EdgePointsDomainPropertyId
                        || e.DomainProperty.Id == LinkShape.ManuallyRoutedDomainPropertyId)
                    {
                        ViewModelChangeContext.GetNewOrExistingContext(tx)
                            .ViewModelChanges.Add(new AssociationConnectorChange(associationConnector, e.DomainProperty.Id));
                    }
                }
            }
        }
    }

    [RuleOn(typeof(AssociationConnector), FireTime = TimeToFire.TopLevelCommit)]
    internal sealed class AssociationConnector_DeleteRule : DeleteRule
    {
        public override void ElementDeleted(ElementDeletedEventArgs e)
        {
            base.ElementDeleted(e);

            var associationConnector = e.ModelElement as AssociationConnector;
            if (associationConnector != null)
            {
                var tx = ModelUtils.GetCurrentTx(associationConnector.Store);
                Debug.Assert(tx != null);
                if (tx != null
                    && !tx.IsSerializing)
                {
                    ViewModelChangeContext.GetNewOrExistingContext(tx)
                        .ViewModelChanges.Add(new AssociationConnectorDelete(associationConnector));
                }
            }
        }
    }
}
