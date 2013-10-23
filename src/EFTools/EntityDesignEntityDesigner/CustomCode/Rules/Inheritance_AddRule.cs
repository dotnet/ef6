// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Model = Microsoft.Data.Entity.Design.Model.Entity;

namespace Microsoft.Data.Entity.Design.EntityDesigner.Rules
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges;
    using Microsoft.Data.Entity.Design.EntityDesigner.Utils;
    using Microsoft.Data.Entity.Design.EntityDesigner.View;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VisualStudio.Modeling;
    using Microsoft.VisualStudio.Modeling;
    using Microsoft.VisualStudio.Modeling.Diagrams;
    using EntityType = Microsoft.Data.Entity.Design.Model.Entity.EntityType;

    /// <summary>
    ///     Rule fired when an Inheritance is created
    /// </summary>
    [RuleOn(typeof(Inheritance), FireTime = TimeToFire.TopLevelCommit)]
    internal sealed class Inheritance_AddRule : AddRule
    {
        /// <summary>
        ///     Do the following when a new Inheritance is created:
        ///     - Display a warning and proceed per user choice
        ///     - Remove any keys defined in the derived entity (and their derived entities down the hierarchy)
        /// </summary>
        /// <param name="e"></param>
        public override void ElementAdded(ElementAddedEventArgs e)
        {
            base.ElementAdded(e);

            var addedInheritance = e.ModelElement as Inheritance;
            Debug.Assert(addedInheritance != null);
            Debug.Assert(addedInheritance.SourceEntityType != null);
            Debug.Assert(addedInheritance.SourceEntityType.EntityDesignerViewModel != null);
            Debug.Assert(addedInheritance.TargetEntityType != null);

            if (addedInheritance != null
                && addedInheritance.SourceEntityType != null
                && addedInheritance.SourceEntityType.EntityDesignerViewModel != null
                && addedInheritance.TargetEntityType != null)
            {
                // We need to invalidate the target entitytypeshape element; so base type name will be updated correctly.
                foreach (var pe in  PresentationViewsSubject.GetPresentation(addedInheritance.TargetEntityType))
                {
                    var entityShape = pe as EntityTypeShape;
                    if (entityShape != null)
                    {
                        entityShape.Invalidate();
                    }
                }

                var tx = ModelUtils.GetCurrentTx(e.ModelElement.Store);
                Debug.Assert(tx != null);
                if (tx != null
                    && !tx.IsSerializing)
                {
                    var source = addedInheritance.SourceEntityType;
                    var viewModel = source.EntityDesignerViewModel;

                    var b = viewModel.ModelXRef.GetExisting(addedInheritance.SourceEntityType) as EntityType;
                    var d = viewModel.ModelXRef.GetExisting(addedInheritance.TargetEntityType) as EntityType;

                    var baseEntity = b as ConceptualEntityType;
                    var derivedEntity = d as ConceptualEntityType;

                    Debug.Assert(b != null ? baseEntity != null : true, "EntityType is not ConceptualEntityType");
                    Debug.Assert(d != null ? derivedEntity != null : true, "EntityType is not ConceptualEntityType");

                    Debug.Assert(baseEntity != null && derivedEntity != null);

                    ViewModelChangeContext.GetNewOrExistingContext(tx)
                        .ViewModelChanges.Add(new InheritanceAdd(addedInheritance, baseEntity, derivedEntity));
                }
            }
        }
    }
}
