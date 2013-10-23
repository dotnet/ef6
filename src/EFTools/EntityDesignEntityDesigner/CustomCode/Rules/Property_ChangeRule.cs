// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using EDMModelUtils = Microsoft.Data.Entity.Design.Model.ModelHelper;
using Model = Microsoft.Data.Entity.Design.Model.Entity;

namespace Microsoft.Data.Entity.Design.EntityDesigner.Rules
{
    using System;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges;
    using Microsoft.Data.Entity.Design.EntityDesigner.Utils;
    using Microsoft.Data.Entity.Design.EntityDesigner.View;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VisualStudio.Modeling;
    using Microsoft.VisualStudio.Modeling;
    using Microsoft.VisualStudio.Modeling.Diagrams;

    /// <summary>
    ///     Rule fired when a Property changes
    /// </summary>
    [RuleOn(typeof(Property), FireTime = TimeToFire.TopLevelCommit)]
    internal sealed class Property_ChangeRule : ChangeRule
    {
        /// <summary>
        ///     Do the following when an Entity changes:
        ///     - Update roles in related Associations
        /// </summary>
        /// <param name="e"></param>
        public override void ElementPropertyChanged(ElementPropertyChangedEventArgs e)
        {
            base.ElementPropertyChanged(e);

            var changedProperty = e.ModelElement as Property;

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

                // if EntityType property was changed and EntityDesignerDiagram's DisplayNameAndType flag is set to true, we need to refresh the entity shape diagram.
                if (e.DomainProperty.Id == Property.TypeDomainPropertyId
                    && null != diagram
                    && diagram.DisplayNameAndType)
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
                    var viewModel = changedProperty.EntityType.EntityDesignerViewModel;
                    // ensure name is unique and valid if the name has changed
                    if (e.DomainProperty.Id == NameableItem.NameDomainPropertyId)
                    {
                        // if we are creating this, the old name will be empty so there is no 'change' to do
                        if (String.IsNullOrEmpty((string)e.OldValue))
                        {
                            return;
                        }

                        var modelProperty = viewModel.ModelXRef.GetExisting(changedProperty) as Model.Entity.Property;
                        Debug.Assert(modelProperty != null, "modelProperty is null");

                        string errorMessage;
                        if (!EDMModelUtils.ValidatePropertyName(modelProperty, changedProperty.Name, true, out errorMessage))
                        {
                            throw new InvalidOperationException(errorMessage);
                        }

                        ViewModelChangeContext.GetNewOrExistingContext(tx).ViewModelChanges.Add(new PropertyChange(changedProperty));
                    }
                }
            }
        }
    }
}
