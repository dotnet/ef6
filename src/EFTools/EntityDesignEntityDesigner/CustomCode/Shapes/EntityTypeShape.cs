// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.View
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Globalization;
    using System.Windows.Forms;
    using Microsoft.Data.Entity.Design.EntityDesigner.CustomCode.Utils;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.VisualStudio.Modeling;
    using Microsoft.VisualStudio.Modeling.Diagrams;
    using Microsoft.VisualStudio.Modeling.Immutability;
    using Microsoft.VisualStudio.PlatformUI;
    using Diagram = Microsoft.Data.Entity.Design.Model.Designer.Diagram;
    using Resources = Microsoft.Data.Entity.Design.EntityDesigner.Properties.Resources;

    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal partial class EntityTypeShape
    {
        internal static readonly string PropertiesCompartmentName = "Properties";
        internal static readonly string NavigationCompartmentName = "Navigation";
        internal static readonly Color TransparentColor = Color.Magenta;

        private static Image _keyPropertyBitmap;
        private static Image _propertyBitmap;
        private static Image _complexPropertyBitmap;
        private static Image _navigationPropertyBitmap;
        private bool _isAdjustedForFillColor;

        internal class EntityChevronButtonField : ChevronButtonField
        {
            internal EntityChevronButtonField(string fieldName)
                : base(fieldName)
            {
            }

            protected override Image GetButtonImage(ShapeElement parentShape)
            {
                var entityShape = parentShape as EntityTypeShape;
                var state = IsExpanded(parentShape);
                if (state.HasValue
                    && entityShape != null)
                {
                    return state.Value
                               ? CachedFillColorAppearance(entityShape.FillColor).ChevronExpanded
                               : CachedFillColorAppearance(entityShape.FillColor).ChevronCollapsed;
                }
                return null;
            }

            internal bool? IsExpanded(ShapeElement parentShape)
            {
                return GetValue(parentShape) as bool?;
            }
        }

        internal class EntityImageField : ImageField
        {
            private readonly Func<FillColorAppearance, Bitmap> _bitmapAccessor;

            internal EntityImageField(string fieldName, Func<FillColorAppearance, Bitmap> bitmapAccessor)
                : base(fieldName)
            {
                _bitmapAccessor = bitmapAccessor;
            }

            public override Image GetDisplayImage(ShapeElement parentShape)
            {
                var entityShape = parentShape as EntityTypeShape;
                return entityShape != null
                           ? _bitmapAccessor(CachedFillColorAppearance(entityShape.FillColor))
                           : DefaultImage;
            }
        }

        internal static Color EmphasisShapeOutlineColor
        {
            get { return VSColorTheme.GetThemedColor(EnvironmentColors.ClassDesignerEmphasisBorderColorKey); }
        }

        /// <summary>
        ///     Setup initial shape size and position
        /// </summary>
        public override void OnInitialize()
        {
            base.OnInitialize();
            //
            // The following code was taken from the Dsl's DslDesigner code to fix up a pathological perf problem
            // when all shapes are in the same region.  
            //
            // Seed shapes along a gradient so that there is no overlap, favor vertical over horizontal scrolling
            // Done in the constructor so it happens before GraphObject initialization
            var randomized = new PointD(
                (EntityDesignerViewModel.EntityShapeLocationSeed % 7) * 3.0, EntityDesignerViewModel.EntityShapeLocationSeed * 1.5);
            var bounds = AbsoluteBounds;
            bounds.Location = randomized;
            AbsoluteBounds = bounds;
            EntityDesignerViewModel.EntityShapeLocationSeed++;

            // Provide keyboard shortcuts for collapse and expand of this EntityType shape
            this.KeyUp += EntityTypeShape_KeyUp;
        }

        /// <summary>
        ///     Were theme colors already applied?
        /// </summary>
        internal static bool IsColorThemeSet { get; set; }

        /// <summary>
        ///     Paint emphasis for selected shape, but first check if we need to update colors.
        /// </summary>
        public override void OnPaintEmphasis(DiagramPaintEventArgs e)
        {
            if (!IsColorThemeSet)
            {
                SetColorTheme();
            }
            base.OnPaintEmphasis(e);
        }

        private void SetColorTheme()
        {
            ClassStyleSet.OverridePenColor(DiagramPens.EmphasisOutline, EmphasisShapeOutlineColor);
            // We shouldn't need to do this again unless the user changes the theme.
            IsColorThemeSet = true;
        }

        public EntityType TypedModelElement
        {
            get { return ModelElement as EntityType; }
        }

        public new EntityDesignerDiagram Diagram
        {
            get { return base.Diagram as EntityDesignerDiagram; }
        }

        public override bool HasToolTip
        {
            get { return true; }
        }

        public string Name
        {
            get
            {
                var headerTitleTextField = FindShapeField(ShapeFields, "Name") as TextField;
                return headerTitleTextField != null ? headerTitleTextField.GetDisplayText(this) : null;
            }
        }

        public override void OnPaintShape(DiagramPaintEventArgs e)
        {
            // Have we adjusted the other colors in the shape for this fill color?
            if (!_isAdjustedForFillColor)
            {
                AdjustForFillColor();
            }

            base.OnPaintShape(e);
            // DoPaintEmphasis requires some information from canvas (for example: zoom factor), 
            // so we need to skip drawing shapes' emphasis if they are drawn outside of diagram canvas (for example: in thumbnail view).
            if (e.View != null)
            {
                // If the shape is in the EmphasizedShapes list, draw the emphasis shape around the shape.
                if (Diagram.EmphasizedShapes.Contains(new DiagramItem(this)))
                {
                    ShapeGeometry.DoPaintEmphasis(e, this);
                }
            }
        }

        /// <summary>
        ///     If the fill color changes we need to adjust other colors again.
        /// </summary>
        protected override void OnFillColorChanged(Color newValue)
        {
            // Users should never be able to set transparent color.
            if (newValue == Color.Transparent)
            {
                FillColor = EntityDesignerDiagramConstant.EntityTypeShapeDefaultFillColor;
                return;
            }
            // We adjust colors in the shape on the next painting.
            _isAdjustedForFillColor = false;
        }

        private void AdjustForFillColor()
        {
            // If the shape is very dark, we make the title text white, and viceversa.
            StyleSet.OverrideBrushColor(DiagramBrushes.ShapeText, CachedFillColorAppearance(FillColor).TextColor);
            // We draw a thin outline of a slightly different color to improve distinguishability when shape color looks like background (and because it looks good).
            StyleSet.OverridePenColor(OutlinePenId, CachedFillColorAppearance(FillColor).OutlineColor);
            // We shouldn't need to do this again unless the user changes the color for this shape.
            _isAdjustedForFillColor = true;
        }

        /// <summary>
        ///     We override default shape highlighting to make it subtler.
        /// </summary>
        protected override int ModifyLuminosity(int currentLuminosity, DiagramClientView view)
        {
            if (view.HighlightedShapes.Contains(new DiagramItem(this)))
            {
                return GetHighlightLuminosity(currentLuminosity);
            }
            return currentLuminosity;
        }

        /// <summary>
        ///     Common part of formula to calculate outline and highlight colors.
        /// </summary>
        private static int GetHighlightLuminosity(int currentLuminosity)
        {
            return currentLuminosity < 120 ? currentLuminosity + 40 : currentLuminosity - 40;
        }

        internal struct FillColorAppearance
        {
            public Color TextColor;
            public Color OutlineColor;
            public Bitmap EntityGlyph;
            public Bitmap BaseTypeIcon;
            public Bitmap ChevronExpanded;
            public Bitmap ChevronCollapsed;
        }

        private static Bitmap MakeBitmapTransparent(Bitmap source)
        {
            source.MakeTransparent(TransparentColor);
            return source;
        }

        // bitmaps are cached to avoid deserializing the resources more than once and creating multiple instances in memory 
        private static readonly Bitmap EntityGlyph = MakeBitmapTransparent(Resources.EntityGlyph);
        private static readonly Bitmap BaseTypeIcon = MakeBitmapTransparent(Resources.BaseTypeIcon);
        private static readonly Bitmap ChevronExpanded = MakeBitmapTransparent(Resources.ChevronExpanded);
        private static readonly Bitmap ChevronCollapsed = MakeBitmapTransparent(Resources.ChevronCollapsed);

        /// <summary>
        ///     Calculates appropriate colors and icons for a shape's fill color.
        /// </summary>
        private static FillColorAppearance CalculateFillColorAppearance(Color fillColor)
        {
            var hslColor = HslColor.FromRgbColor(fillColor);
            return new FillColorAppearance
                {
                    TextColor = GetTextColor(fillColor),
                    OutlineColor
                        = new HslColor
                            {
                                Hue = hslColor.Hue,
                                Saturation = hslColor.Saturation * 3 / 5,
                                Luminosity = GetHighlightLuminosity(hslColor.Luminosity)
                            }.ToRgbColor(),
#if VS12ORNEWER
                    EntityGlyph = ThemeUtils.GetThemedButtonImage(EntityGlyph, fillColor),
                    BaseTypeIcon = ThemeUtils.GetThemedButtonImage(BaseTypeIcon, fillColor),
                    ChevronExpanded = ThemeUtils.GetThemedButtonImage(ChevronExpanded, fillColor),
                    ChevronCollapsed = ThemeUtils.GetThemedButtonImage(ChevronCollapsed, fillColor)
#else
                    // avoid icon inversion in VS11 because it causes unclear images on light backgrounds
                    EntityGlyph = EntityGlyph,
                    BaseTypeIcon = BaseTypeIcon,
                    ChevronExpanded = ChevronExpanded,
                    ChevronCollapsed = ChevronCollapsed
#endif
                };
        }

        private static readonly double _blackBrightness = GetRelativeBrightness(Color.Black);
        private static readonly double _whiteBrightness = GetRelativeBrightness(Color.White);

        private static Color GetTextColor(Color fillColor)
        {
            var fillBrightness = GetRelativeBrightness(fillColor);
            return GetContrast(_blackBrightness, fillBrightness)
                   > GetContrast(_whiteBrightness, fillBrightness)
                ? Color.Black
                : Color.White;
        }

        private static double GetContrast(double b1, double b2)
        {
            return b1 > b2
                ? (b1 + 0.05) / (b2 + 0.05)
                : (b2 + 0.05) / (b1 + 0.05);
        }

        private static double GetRelativeBrightness(Color color)
        {
            return 0.2126 * GetRelativeColorPart(color.R)
                   + 0.7152 * GetRelativeColorPart(color.G)
                   + 0.0722 * GetRelativeColorPart(color.B);
        }

        private static double GetRelativeColorPart(byte colorPart)
        {
            var part = (colorPart / 255.0);
            return part <= 0.03928 ? part / 12.92 : Math.Pow((part + 0.055) / 1.055, 2.4);
        }

        /// <summary>
        ///     This method is used to cache the results of a function so the funcion is ever at most invoked
        ///     once for each possible value of the argument and only one copy of the result for every value
        ///     of the argument is ever allocated in memory
        /// </summary>
        /// <typeparam name="TArg">the type of the argument of the function</typeparam>
        /// <typeparam name="TResult">the type of the result of the function</typeparam>
        /// <param name="func">original function</param>
        /// <returns>"memoized" version of the original function</returns>
        /// <remarks>
        ///     The correct usage of the Memoize method is to only call it once passing as an argument the
        ///     function to memoize. The returned "memoized" function can be invoked many times to take
        ///     advantage of the cached results. This is a good general reference on memoization:
        ///     http://en.wikipedia.org/wiki/Memoization
        /// </remarks>
        private static Func<TArg, TResult> Memoize<TArg, TResult>(Func<TArg, TResult> func)
        {
            var map = new Dictionary<TArg, TResult>();
            return arg =>
                {
                    TResult result;
                    if (!map.TryGetValue(arg, out result))
                    {
                        result = func(arg);
                        map.Add(arg, result);
                    }
                    return result;
                };
        }

        private static readonly Func<Color, FillColorAppearance> CachedFillColorAppearance
            = Memoize<Color, FillColorAppearance>(CalculateFillColorAppearance);

        public override bool CanMove
        {
            get { return (Partition.GetLocks() & Locks.Properties) != Locks.Properties; }
        }

        public override NodeSides ResizableSides
        {
            get
            {
                if ((Partition.GetLocks() & Locks.Properties) != Locks.Properties)
                {
                    return base.ResizableSides;
                }
                return NodeSides.None;
            }
        }

        public EntityTypeElementListCompartment PropertiesCompartment
        {
            get { return FindCompartment(PropertiesCompartmentName) as EntityTypeElementListCompartment; }
        }

        public EntityTypeElementListCompartment NavigationCompartment
        {
            get { return FindCompartment(NavigationCompartmentName) as EntityTypeElementListCompartment; }
        }

        /// <summary>
        ///     Provides an image to display to the left Property elements in the EntityShape.
        ///     It caches the bitmaps for performance reasons.
        /// </summary>
        private Image PropertyDisplayImageGetter(ModelElement element)
        {
            if (element is Property)
            {
                var scalarProperty = element as ScalarProperty;
                if (scalarProperty != null)
                {
                    if (scalarProperty.EntityKey)
                    {
                        return _keyPropertyBitmap ??
                               (_keyPropertyBitmap =
                                ThemeUtils.GetThemedButtonImage(
                                    Resources.PropertyPK,
                                    PropertiesCompartment.CompartmentFillColor));
                    }
                    return _propertyBitmap ??
                           (_propertyBitmap =
                            ThemeUtils.GetThemedButtonImage(
                                Resources.Property,
                                PropertiesCompartment.CompartmentFillColor));
                }
                Debug.Assert(element is ComplexProperty, "Unknown property type");
                return _complexPropertyBitmap ??
                       (_complexPropertyBitmap =
                        ThemeUtils.GetThemedButtonImage(
                            Resources.ComplexProperty,
                            PropertiesCompartment.CompartmentFillColor));
            }
            return null;
        }

        /// <summary>
        ///     Provides an image to display to the left NavigationProperty elements in the EntityShape.
        ///     It caches the bitmaps for performance reasons.
        /// </summary>
        private Image NavigationDisplayImageGetter(ModelElement element)
        {
            var property = element as NavigationProperty;
            if (property != null)
            {
                return _navigationPropertyBitmap ??
                       (_navigationPropertyBitmap =
                        ThemeUtils.GetThemedButtonImage(
                            Resources.NavigationProperty,
                            NavigationCompartment.CompartmentFillColor));
            }
            return null;
        }

        // DDBugs 56442: Keep long class names from overlapping the expand/collapse button
        protected override void InitializeDecorators(IList<ShapeField> shapeFields, IList<Decorator> decorators)
        {
            // Call the base class FIRST because it wipes all the anchoring information
            base.InitializeDecorators(shapeFields, decorators);

            var headerTitleTextField = FindShapeField(shapeFields, "Name") as TextField;
            var headerExpandCollapseButtonField = FindShapeField(shapeFields, "ExpandCollapse");

            Debug.Assert(headerTitleTextField != null, "headerTitleTextField != null");
            // DDBugs 56442: Keep long class names from overlapping the expand/collapse button
            headerTitleTextField.AnchoringBehavior.SetRightAnchor(headerExpandCollapseButtonField, AnchoringBehavior.Edge.Left, 0.05);
        }

        protected override void InitializeShapeFields(IList<ShapeField> shapeFields)
        {
            base.InitializeShapeFields(shapeFields);
            var headerTitleTextField = FindShapeField(shapeFields, "Name") as TextField;

            Debug.Assert(headerTitleTextField != null, "headerTitleTextField != null");

            // DD 40501: Specify the MSAA name/description for the class title
            headerTitleTextField.DefaultAccessibleName = Resources.AccName_EntityTypeHeader;
            headerTitleTextField.DefaultAccessibleDescription = Resources.AccDesc_EntityTypeHeader;

            // replace default glyphs in header with ones that respond to filler color and scale with the UX
            ReplaceField(
                shapeFields, "ExpandCollapse",
                fieldName => new EntityChevronButtonField(fieldName)
                                {
                                    DefaultSelectable = false,
                                    DefaultFocusable = false
                                });
            ReplaceField(
                shapeFields, "IconDecorator",
                fieldName => new EntityImageField(fieldName, appearance => appearance.EntityGlyph));
            ReplaceField(
                shapeFields, "BaseTypeIconDecorator",
                fieldName => new EntityImageField(fieldName, appearance => appearance.BaseTypeIcon));
        }

        private static TField ReplaceField<TField>(IList<ShapeField> shapeFields, string fieldName, Func<string, TField> constructor)
            where TField : ShapeField
        {
            Debug.Assert(shapeFields != null, "shapeFields != null");
            Debug.Assert(fieldName != null, "fieldName != null");
            Debug.Assert(constructor != null, "constructor != null");
            var originalField = FindShapeField(shapeFields, fieldName);
            Debug.Assert(originalField != null, "originalField != null");
            shapeFields.Remove(originalField);
            var newField = constructor(fieldName);
            Debug.Assert(newField != null, "newField != null");
            shapeFields.Add(newField);
            return newField;
        }

        public override void EnsureCompartments()
        {
            base.EnsureCompartments();

            SetAccessibilityOnCompartment(PropertiesCompartment, true);
            SetAccessibilityOnCompartment(NavigationCompartment, false);

            // need to add our own handler for KeyUp event in order to disable Insert key for Navigation Compartment
            // and provide keyboard shortcuts for expand and collapse.
            NavigationCompartment.KeyUp += NavigationCompartmentKeyUpHandler;

            // since Properties compartment contains now two DomainClasses (Scalar and Complex) we need to handle Insert key on our own
            // and provide keyboard shortcuts for expand and collapse.
            PropertiesCompartment.KeyUp += PropertiesCompartmentKeyUpHandler;
        }

        private void PropertiesCompartmentKeyUpHandler(object sender, DiagramKeyEventArgs e)
        {
            if ((Partition.GetLocks() & Locks.Add) == Locks.Add)
            {
                return;
            }

            // on Insert add new ScalarProperty to the compartment
            if (e != null
                && e.KeyCode == Keys.Insert)
            {
                var compartment = sender as ElementListCompartment;
                if (compartment != null)
                {
                    var domainClassInfo = compartment.Partition.DomainDataDirectory.GetDomainClass(ScalarProperty.DomainClassId);
                    compartment.HandleNewListItemInsertion(e.DiagramClientView, domainClassInfo);
                    e.Handled = true;
                }
            }

            // Provide keyboard shortcuts for expand and collapse.
            if (e.Control && e.KeyCode == Keys.Up)
            {
                PropertiesCompartment.EnsureExpandedState(false);
                e.Handled = true;
            }

            if (e.Control && e.KeyCode == Keys.Down)
            {
                PropertiesCompartment.EnsureExpandedState(true);
                e.Handled = true;
            }
        }

        // disable default ElementListCompartment action for KeyUp event if it's Insert key
        // and provide keyboard shortcuts for expand and collapse.
        private void NavigationCompartmentKeyUpHandler(object sender, DiagramKeyEventArgs e)
        {
            if ((Partition.GetLocks() & Locks.Add) == Locks.Add)
            {
                return;
            }

            if (e.KeyCode == Keys.Insert)
            {
                e.Handled = true;
            }

            if (e.Control && e.KeyCode == Keys.Up)
            {
                NavigationCompartment.EnsureExpandedState(false);
                e.Handled = true;
            }

            if (e.Control && e.KeyCode == Keys.Down)
            {
                NavigationCompartment.EnsureExpandedState(true);
                e.Handled = true;
            }
        }

        private static void SetAccessibilityOnCompartment(ElementListCompartment compartment, bool scalarProperty)
        {
            if (compartment != null)
            {
                compartment.ItemAccessibleNameGetter = AccessibilityNameGetterMethod;
                compartment.ItemAccessibleDescriptionGetter = AccessibilityDescriptionGetterMethod;

                // DD 40502: ListCompartments get accessibility info from their field
                var hdrField = compartment.FindShapeField("HdrText") as TextField; //Compartment.HeaderTextFieldName
                var listField = compartment.FindShapeField("MainListField") as ListField; //ListCompartment.MainListFieldName

                if (hdrField != null
                    && listField != null)
                {
                    string aaName;
                    string aaDescription;

                    if (scalarProperty)
                    {
                        aaName = Resources.AccName_EntityTypeScalarPropertyCompartment;
                        aaDescription = Resources.AccDesc_EntityTypeScalarPropertyCompartment;
                    }
                    else
                    {
                        aaName = Resources.AccName_EntityTypeNavigationPropertyCompartment;
                        aaDescription = Resources.AccDesc_EntityTypeNavigationPropertyCompartment;
                    }

                    hdrField.DefaultAccessibleName = aaName;
                    hdrField.DefaultAccessibleDescription = aaDescription;
                    listField.DefaultAccessibleName = aaName;
                    listField.DefaultAccessibleDescription = aaDescription;
                }
            }
        }

        internal static string AccessibilityNameGetterMethod(ModelElement element)
        {
            var named = element as NameableItem;
            if (named != null)
            {
                return named.Name;
            }
            return "??";
        }

        internal static string AccessibilityDescriptionGetterMethod(ModelElement element)
        {
            if (element is Property)
            {
                return string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.AccDesc_ScalarProperty,
                    Resources.CompClassName_ScalarProperty);
            }
            if (element is NavigationProperty)
            {
                return string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.AccDesc_NavigationProperty,
                    Resources.CompClassName_NavigationProperty);
            }
            return "??";
        }

        protected override CompartmentMapping[] GetCompartmentMappings(Type melType)
        {
            var baseMappings = base.GetCompartmentMappings(melType);

            foreach (var mapping in baseMappings)
            {
                var elementMap = mapping as ElementListCompartmentMapping;
                if (elementMap != null)
                {
                    if (elementMap.CompartmentId.Equals(PropertiesCompartmentName, StringComparison.OrdinalIgnoreCase))
                    {
                        elementMap.ImageGetter = PropertyDisplayImageGetter;
                    }
                    else if (elementMap.CompartmentId.Equals(NavigationCompartmentName, StringComparison.OrdinalIgnoreCase))
                    {
                        elementMap.ImageGetter = NavigationDisplayImageGetter;
                    }
                }
            }

            return baseMappings;
        }

        public override SizeD MinimumResizableSize
        {
            get
            {
                var size = base.MinimumResizableSize;
                size.Width = 0.6;

                return size;
            }
        }

        public override string AccessibleName
        {
            get
            {
                return string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.AccDesc_EntityType,
                    TypedModelElement != null
                    && !string.IsNullOrWhiteSpace(TypedModelElement.Name)
                        ? TypedModelElement.Name
                        : Resources.Acc_Unnamed);
            }
        }

        public override string AccessibleDescription
        {
            get
            {
                return string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.AccDesc_EntityType,
                    Resources.CompClassName_EntityType);
            }
        }

        public override string AccessibleHelp
        {
            get
            {
                return string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.AccHelp_EntityType,
                    IsExpanded ? Resources.ExpandedStateExpanded : Resources.ExpandedStateCollapsed);
            }
        }

        private void EntityTypeShape_KeyUp(object sender, DiagramKeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Up)
            {
                this.EnsureExpandedState(false);
                e.Handled = true;
            }

            if (e.Control && e.KeyCode == Keys.Down)
            {
                this.EnsureExpandedState(true);
                e.Handled = true;
            }
        }

        /// <summary>
        ///     Remove shadow from the entity type shape.
        /// </summary>
        public override bool HasShadow
        {
            get { return false; }
        }

        /// <summary>
        ///     Gets an array of CompartmentDescription for all compartments shown on this shape
        ///     The method will replace ElementListCompartmentDescription instances with EntityTypeElementListCompartmentDescription instances.
        /// </summary>
        public override CompartmentDescription[] GetCompartmentDescriptions()
        {
            var compartmentDescription = base.GetCompartmentDescriptions();

            Debug.Assert(compartmentDescription.Length == 2, "There should be 2 compartment descriptions.");
            if (compartmentDescription.Length == 2)
            {
                var originalElementListCompartmentDescription = compartmentDescription[0] as ListCompartmentDescription;
                Debug.Assert(
                    originalElementListCompartmentDescription != null,
                    "The compartment description is not type of ListCompartmentDescription. Actual type: "
                    + compartmentDescription[0].GetType().Name);
                if (originalElementListCompartmentDescription != null)
                {
                    compartmentDescription[0] = new EntityTypeElementListCompartmentDescription(
                        originalElementListCompartmentDescription,
                        !EntityDesignerDiagramConstant.IsDefaultShapeExpanded,
                        true);
                }

                originalElementListCompartmentDescription = compartmentDescription[1] as ListCompartmentDescription;
                Debug.Assert(
                    originalElementListCompartmentDescription != null,
                    "The compartment description is not type of ListCompartmentDescription. Actual type: "
                    + compartmentDescription[1].GetType().Name);
                if (originalElementListCompartmentDescription != null)
                {
                    compartmentDescription[1] = new EntityTypeElementListCompartmentDescription(
                        originalElementListCompartmentDescription,
                        !EntityDesignerDiagramConstant.IsDefaultShapeExpanded,
                        false);
                }
            }
            return compartmentDescription;
        }

        /// <summary>
        ///     Return DiagramItem object for a given EntityType's Property.
        /// </summary>
        public DiagramItem GetDiagramItemForProperty(PropertyBase propertyBase)
        {
            Debug.Assert(propertyBase != null, "propertyBase is null.");
            if (propertyBase != null)
            {
                var property = propertyBase as Property;
                var navigationProperty = propertyBase as NavigationProperty;
                Debug.Assert(
                    property != null || navigationProperty != null,
                    "Unexpected property type. Property Type: " + propertyBase.GetType().Name);
                if (property != null
                    || navigationProperty != null)
                {
                    var compartment = navigationProperty != null ? NavigationCompartment : PropertiesCompartment;
                    var propertyIndex = compartment.Items.IndexOf(propertyBase);
                    Debug.Assert(
                        propertyIndex != -1, "Could not find property: " + propertyBase.Name + " in entity-type: " + AccessibleName);

                    if (propertyIndex != -1)
                    {
                        return new DiagramItem(compartment, compartment.ListField, new ListItemSubField(propertyIndex));
                    }
                }
            }
            return null;
        }
    }
}
