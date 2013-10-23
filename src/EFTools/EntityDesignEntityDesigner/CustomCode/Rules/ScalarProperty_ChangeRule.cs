// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Model = Microsoft.Data.Entity.Design.Model.Entity;

namespace Microsoft.Data.Entity.Design.EntityDesigner.Rules
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges;
    using Microsoft.Data.Entity.Design.EntityDesigner.Utils;
    using Microsoft.Data.Entity.Design.EntityDesigner.View;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VisualStudio.Modeling;
    using Microsoft.VisualStudio.Modeling;
    using Microsoft.VisualStudio.Modeling.Diagrams;

    /// <summary>
    ///     Rule fired when a ScalarProperty changes
    /// </summary>
    [RuleOn(typeof(ScalarProperty), FireTime = TimeToFire.TopLevelCommit)]
    internal sealed class ScalarProperty_ChangeRule : ChangeRule
    {
        public override void ElementPropertyChanged(ElementPropertyChangedEventArgs e)
        {
            base.ElementPropertyChanged(e);

            var changedProperty = e.ModelElement as ScalarProperty;

            Debug.Assert(changedProperty != null);

            // this rule will fire if a PropertyRef gets deleted (this happens if a keyed property that has a sibling keyed property is deleted),
            // in which case we ignore this change.
            if (changedProperty.IsDeleted)
            {
                return;
            }

            Debug.Assert(changedProperty.EntityType != null && changedProperty.EntityType.EntityDesignerViewModel != null);

            if (changedProperty != null
                && changedProperty.EntityType != null
                && changedProperty.EntityType.EntityDesignerViewModel != null)
            {
                var diagram = changedProperty.EntityType.EntityDesignerViewModel.GetDiagram();
                Debug.Assert(diagram != null, "EntityDesignerDiagram is null");

                // if EntityKey property changed, we need to invalidate properties compartment for this property to refresh the icon
                if (e.DomainProperty.Id == ScalarProperty.EntityKeyDomainPropertyId)
                {
                    foreach (var pe in PresentationViewsSubject.GetPresentation(changedProperty.EntityType))
                    {
                        var entityShape = pe as EntityTypeShape;
                        if (entityShape != null)
                        {
                            entityShape.PropertiesCompartment.Invalidate(true);
                        }
                    }
                }

                var tx = ModelUtils.GetCurrentTx(e.ModelElement.Store);
                Debug.Assert(tx != null);
                // don't do the auto update stuff if we are in the middle of deserialization
                if (tx != null
                    && !tx.IsSerializing)
                {
                    if (e.DomainProperty.Id == ScalarProperty.EntityKeyDomainPropertyId)
                    {
                        ViewModelChangeContext.GetNewOrExistingContext(tx)
                            .ViewModelChanges.Add(new ScalarPropertyKeyChange(changedProperty));
                    }
                }
            }
        }
    }
}
