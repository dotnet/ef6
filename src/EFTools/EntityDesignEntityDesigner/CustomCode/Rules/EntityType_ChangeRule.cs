// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Model = Microsoft.Data.Entity.Design.Model.Entity;

namespace Microsoft.Data.Entity.Design.EntityDesigner.Rules
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges;
    using Microsoft.Data.Entity.Design.EntityDesigner.Properties;
    using Microsoft.Data.Entity.Design.EntityDesigner.Utils;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
    using Microsoft.Data.Entity.Design.Model.Validation;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VisualStudio.Modeling;
    using Microsoft.VisualStudio.Modeling;

    /// <summary>
    ///     Rule fired when an EntityType changes
    /// </summary>
    [RuleOn(typeof(EntityType), FireTime = TimeToFire.TopLevelCommit)]
    internal sealed class EntityType_ChangeRule : ChangeRule
    {
        /// <summary>
        ///     Do the following when an EntityType changes:
        ///     - Update roles in related Associations
        /// </summary>
        /// <param name="e"></param>
        public override void ElementPropertyChanged(ElementPropertyChangedEventArgs e)
        {
            base.ElementPropertyChanged(e);

            // if the element is deleted or about to be deleted, this rule will get fired.
            // Just return immediately here because we don't care if the entity-type's property has changed.
            if (e.ModelElement.IsDeleted
                || e.ModelElement.IsDeleting)
            {
                return;
            }

            var changedEntity = e.ModelElement as EntityType;
            Debug.Assert(changedEntity != null);
            Debug.Assert(changedEntity.EntityDesignerViewModel != null);

            if (changedEntity != null
                && changedEntity.EntityDesignerViewModel != null)
            {
                var viewModel = changedEntity.EntityDesignerViewModel;
                var tx = ModelUtils.GetCurrentTx(e.ModelElement.Store);
                Debug.Assert(tx != null);
                // don't do the auto update stuff if we are in the middle of deserialization
                if (tx != null
                    && !tx.IsSerializing)
                {
                    // are they changing the name?
                    if (e.DomainProperty.Id == NameableItem.NameDomainPropertyId)
                    {
                        // if we are creating this Entity, there is no 'change' to do
                        if (viewModel.ModelXRef.GetExisting(changedEntity) == null)
                        {
                            return;
                        }

                        if (!EscherAttributeContentValidator.IsValidCsdlEntityTypeName(changedEntity.Name))
                        {
                            throw new InvalidOperationException(
                                String.Format(CultureInfo.CurrentCulture, Resources.Error_EntityNameInvalid, changedEntity.Name));
                        }

                        if (ModelUtils.IsUniqueName(changedEntity, changedEntity.Name, viewModel.EditingContext) == false)
                        {
                            throw new InvalidOperationException(
                                String.Format(CultureInfo.CurrentCulture, Resources.Error_EntityNameDuplicate, changedEntity.Name));
                        }

                        ViewModelChangeContext.GetNewOrExistingContext(tx).ViewModelChanges.Add(new EntityTypeChange(changedEntity));
                    }
                }
            }
        }
    }
}
