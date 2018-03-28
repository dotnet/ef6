// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.View
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.VisualStudio.Modeling;
    using Microsoft.VisualStudio.Modeling.Diagrams;
    using Diagram = Microsoft.Data.Entity.Design.Model.Designer.Diagram;
    using Resources = Microsoft.Data.Entity.Design.EntityDesigner.Properties.Resources;

    [DomainObjectId("53d36908-9892-4495-8754-da5f4d7969d4")]
    internal class EntityTypeElementListCompartment : ElementListCompartment
    {
        private bool _isScalarPropertiesCompartment;

        /// <summary>
        ///     EntityTypeElementListCompartment Constructor
        /// </summary>
        /// <param name="store">Store where new element is to be created.</param>
        /// <param name="propertyAssignments">List of domain property id/value pairs to set once the element is created.</param>
        public EntityTypeElementListCompartment(Store store,
             bool isScalarPropertiesCompartment,
             params PropertyAssignment[] propertyAssignments)
            : this(store != null ? store.DefaultPartitionForClass(DomainClassId) : null,
                  isScalarPropertiesCompartment,
                  propertyAssignments)
        {
            Initialize();
        }

        /// <summary>
        ///     EntityTypeElementListCompartment Constructor
        /// </summary>
        /// <param name="partition">Partition where new element is to be created.</param>
        /// <param name="propertyAssignments">List of domain property id/value pairs to set once the element is created.</param>
        public EntityTypeElementListCompartment(Partition partition,
             bool isScalarPropertiesCompartment,
             params PropertyAssignment[] propertyAssignments)
            : base(partition, propertyAssignments)
        {
            _isScalarPropertiesCompartment = isScalarPropertiesCompartment;

            Initialize();
        }

        private void Initialize()
        {
            ListField.AlternateFontId = new StyleSetResourceId(string.Empty, "ShapeTextBoldUnderline");
        }

        protected override void InitializeResources(StyleSet classStyleSet)
        {
            base.InitializeResources(classStyleSet);

            // Custom Font Settings.
            // Create a diagram shape text that has been bolded and underlined.
            var diagram = Diagram;
            Debug.Assert(diagram != null, "Unable to find instance of Diagram from CompartmentList instance.");
            if (diagram != null)
            {
                var fontSettings = diagram.StyleSet.GetOverriddenFontSettings(DiagramFonts.ShapeText);
                Debug.Assert(fontSettings != null, "Why Diagram doesn't contains FontSettings for ShapeText?");
                if (fontSettings != null)
                {
                    fontSettings.Bold = true;
                    fontSettings.Underline = true;
                    classStyleSet.AddFont(
                        new StyleSetResourceId(string.Empty, "ShapeTextBoldUnderline"), DiagramFonts.ShapeText, fontSettings);
                }
            }
        }

        /// <summary>
        ///     Gets drawing information for a single list item in the list field.
        /// </summary>
        /// <param name="listField">The child list field requesting the drawing information.</param>
        /// <param name="row">The zero-based row number of the list item to draw.</param>
        /// <param name="itemDrawInfo">An ItemDrawInfo that receives the drawing information.</param>
        public override void GetItemDrawInfo(ListField listField, int row, ItemDrawInfo itemDrawInfo)
        {
            base.GetItemDrawInfo(listField, row, itemDrawInfo);
            Debug.Assert(ParentShape != null, "ElementListCompartment should be contained in another shape.");
            if (ParentShape != null)
            {
                var ets = ParentShape as EntityTypeShape;
                Debug.Assert(
                    ets != null, "Expected ElementListCompartment's parent type:EntityTypeShape , Actual:" + ParentShape.GetType().Name);

                if (ets != null
                    && ets.Diagram != null)
                {
                    //  if the compartment list item is in the EmphasizedShapes list, then set the flag so that the item will be drawn in alternate font.
                    // (The list item's font will be bolded and underlined).
                    if (ets.Diagram.EmphasizedShapes.Contains(new DiagramItem(this, ListField, new ListItemSubField(row))))
                    {
                        itemDrawInfo.AlternateFont = true;
                    }
                }
            }
        }

        public override string AccessibleHelp
        {
            get
            {
                if (_isScalarPropertiesCompartment)
                {
                    return Resources.AccHelp_EntityTypeScalarPropertyCompartment;
                }

                return Resources.AccHelp_EntityTypeNavigationPropertyCompartment;
            }
        }

        internal void CollapseInTransaction()
        {
            if (IsExpanded)
            {
                using (var txn = Store.TransactionManager.BeginTransaction())
                {
                    IsExpanded = false;
                    txn.Commit();
                }
            }
        }


        internal void ExpandInTransaction()
        {
            if (!IsExpanded)
            {
                using (var txn = Store.TransactionManager.BeginTransaction())
                {
                    IsExpanded = true;
                    txn.Commit();
                }
            }
        }
    }
}
