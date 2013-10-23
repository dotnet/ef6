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

    [RuleOn(typeof(InheritanceConnector), FireTime = TimeToFire.TopLevelCommit)]
    internal sealed class InheritanceConnector_AddRule : AddRule
    {
        public override void ElementAdded(ElementAddedEventArgs e)
        {
            var inheritanceConnector = e.ModelElement as InheritanceConnector;
            Debug.Assert(inheritanceConnector != null);

            var tx = ModelUtils.GetCurrentTx(inheritanceConnector.Store);
            Debug.Assert(tx != null);
            if (tx != null
                && !tx.IsSerializing)
            {
                ViewModelChangeContext.GetNewOrExistingContext(tx).ViewModelChanges.Add(new InheritanceConnectorAdd(inheritanceConnector));
            }
        }
    }

    [RuleOn(typeof(InheritanceConnector), FireTime = TimeToFire.TopLevelCommit)]
    internal sealed class InheritanceConnector_ChangeRule : ChangeRule
    {
        public override void ElementPropertyChanged(ElementPropertyChangedEventArgs e)
        {
            var inheritanceConnector = e.ModelElement as InheritanceConnector;
            Debug.Assert(inheritanceConnector != null);

            if (inheritanceConnector != null)
            {
                // for some reason when deleting connector, DSL invokes ChangeRule, so just return if it's deleted
                if (inheritanceConnector.IsDeleted)
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
                            .ViewModelChanges.Add(new InheritanceConnectorChange(inheritanceConnector, e.DomainProperty.Id));
                    }
                }
            }
        }
    }

    [RuleOn(typeof(InheritanceConnector), FireTime = TimeToFire.TopLevelCommit)]
    internal sealed class InheritanceConnector_DeleteRule : DeleteRule
    {
        public override void ElementDeleted(ElementDeletedEventArgs e)
        {
            base.ElementDeleted(e);

            var inheritanceConnector = e.ModelElement as InheritanceConnector;
            if (inheritanceConnector != null)
            {
                var tx = ModelUtils.GetCurrentTx(inheritanceConnector.Store);
                Debug.Assert(tx != null);
                if (tx != null
                    && !tx.IsSerializing)
                {
                    ViewModelChangeContext.GetNewOrExistingContext(tx)
                        .ViewModelChanges.Add(new InheritanceConnectorDelete(inheritanceConnector));
                }
            }
        }
    }
}
