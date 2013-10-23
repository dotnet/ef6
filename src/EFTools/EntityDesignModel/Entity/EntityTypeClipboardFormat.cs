// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Tools.XmlDesignerBase.Common.Diagnostics;

    // Represents EntityType info stored in Clipboard
    [Serializable]
    internal class EntityTypeClipboardFormat : AnnotatableElementClipboardFormat
    {
        private readonly string _entityName;
        private readonly string _entitySetName;
        private readonly Color _entityTypeShapeFillColor = EntityDesignerDiagramConstant.EntityTypeShapeDefaultFillColor;
        private readonly List<PropertyClipboardFormat> _properties;
        private readonly List<NavigationPropertyClipboardFormat> _navigationProperties;

        internal EntityTypeClipboardFormat(EntityTypeShape entityTypeShape)
            : this(entityTypeShape.EntityType.Target)
        {
            _entityTypeShapeFillColor = entityTypeShape.FillColor.Value;
        }

        internal EntityTypeClipboardFormat(EntityType entity)
            : base(entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            _entityName = entity.LocalName.Value;
            var entitySet = entity.EntitySet;

            _navigationProperties = new List<NavigationPropertyClipboardFormat>();

            var cet = entity as ConceptualEntityType;
            if (cet != null)
            {
                // c-side entity type
                // don't use base type EntitySet name
                if (cet.HasResolvableBaseType
                    || entitySet == null)
                {
                    _entitySetName = ModelHelper.ConstructProposedEntitySetName(entity.Artifact, _entityName);
                }
                else
                {
                    _entitySetName = entitySet.LocalName.Value;
                }
                // Sort the navigation-properties based on navigation-properties' XElement orders.
                // This is so that we can create the copy of navigation-properties in that order.
                foreach (var navigationProperty in ModelHelper.GetListOfPropertiesInTheirXElementsOrder(cet.NavigationProperties().ToList())
                    )
                {
                    _navigationProperties.Add(new NavigationPropertyClipboardFormat(navigationProperty));
                }
            }
            else
            {
                // s-side entity type

                // don't use base type EntitySet name
                if (entitySet == null)
                {
                    _entitySetName = ModelHelper.ConstructProposedEntitySetName(entity.Artifact, _entityName);
                }
                else
                {
                    _entitySetName = entitySet.LocalName.Value;
                }
            }

            _properties = new List<PropertyClipboardFormat>();
            // Sort the properties based on properties's XElement order. This is that so we can create the copy of properties in that order.
            foreach (var property in ModelHelper.GetListOfPropertiesInTheirXElementsOrder(entity.Properties().ToList()))
            {
                _properties.Add(new PropertyClipboardFormat(property));
            }
        }

        internal string TraceString()
        {
            var sb = new StringBuilder(
                "[" + typeof(EntityTypeClipboardFormat).Name +
                " entityName=" + _entityName +
                ", entitySetName=" + _entitySetName +
                ", entityTypeShapeFillColor=" + _entityTypeShapeFillColor);

            sb.Append(
                " " + EFToolsTraceUtils.FormatNamedEnumerable(
                    "properties", _properties, pcf => pcf.TraceString()));

            sb.Append(
                ", " + EFToolsTraceUtils.FormatNamedEnumerable(
                    "navigationProperties", _navigationProperties,
                    npcf => npcf.TraceString()));

            sb.Append("]");

            return sb.ToString();
        }

        internal string EntityName
        {
            get { return _entityName; }
        }

        internal string EntitySetName
        {
            get { return _entitySetName; }
        }

        internal Color EntityTypeShapeFillColor
        {
            get { return _entityTypeShapeFillColor; }
        }

        internal IList<PropertyClipboardFormat> Properties
        {
            get { return _properties; }
        }

        internal IList<NavigationPropertyClipboardFormat> NavigationProperties
        {
            get { return _navigationProperties; }
        }

        internal NavigationPropertyClipboardFormat GetNavigationPropertyClipboard(string associationName, string associationEndRole)
        {
            foreach (var navigationProperty in NavigationProperties)
            {
                if (String.Compare(navigationProperty.Relationship, associationName, StringComparison.OrdinalIgnoreCase) == 0
                    && String.Compare(navigationProperty.FromRole, associationEndRole, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return navigationProperty;
                }
            }
            return null;
        }
    }
}
