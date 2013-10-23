// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Designer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Tools.Model.Diagram;

    internal class Diagram : EFNameableItem, IDiagram
    {
        internal static readonly string ElementName = "Diagram";
        internal static readonly string AttributeZoomLevel = "ZoomLevel";
        internal static readonly string AttributeShowGrid = "ShowGrid";
        internal static readonly string AttributeSnapToGrid = "SnapToGrid";
        internal static readonly string AttributeDisplayType = "DisplayType";
        internal static readonly string AttributeId = "DiagramId";

        private readonly List<EntityTypeShape> _entityTypeShapes = new List<EntityTypeShape>();
        private readonly List<AssociationConnector> _associationConnectors = new List<AssociationConnector>();
        private readonly List<InheritanceConnector> _inheritanceConnectors = new List<InheritanceConnector>();
        private DefaultableValue<int> _zoomLevelAttr;
        private DefaultableValue<bool> _showGridAttr;
        private DefaultableValue<bool> _snapToGridAttr;
        private DefaultableValue<bool> _displayTypeAttr;
        private DiagramIdDefaultableValue _id;

        internal Diagram(EFElement parent, XElement element)
            : base(parent, element)
        {
        }

        internal DefaultableValue<string> Id
        {
            get
            {
                if (_id == null)
                {
                    _id = new DiagramIdDefaultableValue(this);
                }
                return _id;
            }
        }

        #region IDiagram interface

        string IDiagram.Id
        {
            get { return Id.Value; }
        }

        string IDiagram.Name
        {
            get { return DisplayName; }
        }

        #endregion

        internal DefaultableValue<int> ZoomLevel
        {
            get
            {
                if (_zoomLevelAttr == null)
                {
                    _zoomLevelAttr = new ZoomLevelDefaultableValue(this);
                }
                return _zoomLevelAttr;
            }
        }

        internal DefaultableValue<bool> ShowGrid
        {
            get
            {
                if (_showGridAttr == null)
                {
                    _showGridAttr = new ShowGridDefaultableValue(this);
                }
                return _showGridAttr;
            }
        }

        internal DefaultableValue<bool> SnapToGrid
        {
            get
            {
                if (_snapToGridAttr == null)
                {
                    _snapToGridAttr = new SnapToGridDefaultableValue(this);
                }
                return _snapToGridAttr;
            }
        }

        internal DefaultableValue<bool> DisplayType
        {
            get
            {
                if (_displayTypeAttr == null)
                {
                    _displayTypeAttr = new DisplayTypeDefaultableValue(this);
                }
                return _displayTypeAttr;
            }
        }

        /// <summary>
        ///     Return true if there is diagram object that represents efelement/efobject in the diagram.
        /// </summary>
        /// <param name="efObject"></param>
        /// <returns></returns>
        internal bool IsEFObjectRepresentedInDiagram(EFObject efObject)
        {
            var entityTypesInDiagram = EntityTypeShapes.Select(ets => ets.EntityType.Target).ToList();

            var entityType = efObject.GetParentOfType(typeof(ConceptualEntityType)) as ConceptualEntityType;
            var association = efObject.GetParentOfType(typeof(Association)) as Association;
            var associationSet = efObject as AssociationSet;
            var entitySet = efObject as EntitySet;

            // if efobject is an associationset, check if the corresponding association is in the diagram.
            if (associationSet != null
                && associationSet.Association.Status == BindingStatus.Known)
            {
                association = associationSet.Association.Target;
            }

            // if efobject is an entity-set, Return true only if all the entity-types contained in the set are represented in the diagram.
            if (entitySet != null)
            {
                foreach (var et in entitySet.GetEntityTypesInTheSet())
                {
                    if (entityTypesInDiagram.Contains(et) == false)
                    {
                        return false;
                    }
                }
                return true;
            }
            else if (entityType != null)
            {
                return entityTypesInDiagram.Contains(entityType);
            }
            else if (association != null)
            {
                var associationsInDiagram = AssociationConnectors.Select(a => a.Association.Target).ToList();
                return associationsInDiagram.Contains(association);
            }
            return false;
        }

        internal ICollection<EntityTypeShape> EntityTypeShapes
        {
            get { return _entityTypeShapes.AsReadOnly(); }
        }

        internal ICollection<AssociationConnector> AssociationConnectors
        {
            get { return _associationConnectors.AsReadOnly(); }
        }

        internal ICollection<InheritanceConnector> InheritanceConnectors
        {
            get { return _inheritanceConnectors.AsReadOnly(); }
        }

        internal void AddEntityTypeShape(EntityTypeShape shape)
        {
            _entityTypeShapes.Add(shape);
        }

        internal void AddAssociationConnector(AssociationConnector connector)
        {
            _associationConnectors.Add(connector);
        }

        internal void AddInheritanceConnector(InheritanceConnector connector)
        {
            _inheritanceConnectors.Add(connector);
        }

        #region overrides

        // we unfortunately get a warning from the compiler when we use the "base" keyword in "iterator" types generated by using the
        // "yield return" keyword.  By adding this method, I was able to get around this.  Unfortunately, I wasn't able to figure out
        // a way to implement this once and have derived classes share the implementation (since the "base" keyword is resolved at 
        // compile-time and not at runtime.
        private IEnumerable<EFObject> BaseChildren
        {
            get { return base.Children; }
        }

        internal override IEnumerable<EFObject> Children
        {
            get
            {
                foreach (var efobj in BaseChildren)
                {
                    yield return efobj;
                }

                foreach (EFObject efobj in _entityTypeShapes)
                {
                    yield return efobj;
                }

                foreach (EFObject efobj in _associationConnectors)
                {
                    yield return efobj;
                }

                foreach (EFObject efobj in _inheritanceConnectors)
                {
                    yield return efobj;
                }

                yield return ZoomLevel;
                yield return ShowGrid;
                yield return SnapToGrid;
                yield return DisplayType;
                yield return Id;
            }
        }

        protected override void OnChildDeleted(EFContainer efContainer)
        {
            var shape = efContainer as EntityTypeShape;
            if (shape != null)
            {
                _entityTypeShapes.Remove(shape);
            }

            var associationConnector = efContainer as AssociationConnector;
            if (associationConnector != null)
            {
                _associationConnectors.Remove(associationConnector);
            }

            var inheritanceConnector = efContainer as InheritanceConnector;
            if (inheritanceConnector != null)
            {
                _inheritanceConnectors.Remove(inheritanceConnector);
            }

            base.OnChildDeleted(efContainer);
        }

#if DEBUG
        internal override ICollection<string> MyAttributeNames()
        {
            var s = base.MyAttributeNames();
            s.Add(AttributeZoomLevel);
            s.Add(AttributeShowGrid);
            s.Add(AttributeSnapToGrid);
            s.Add(AttributeDisplayType);
            s.Add(AttributeId);
            return s;
        }

        internal override ICollection<string> MyChildElementNames()
        {
            var s = base.MyChildElementNames();
            s.Add(EntityTypeShape.ElementName);
            s.Add(AssociationConnector.ElementName);
            s.Add(InheritanceConnector.ElementName);
            return s;
        }
#endif

        protected override void PreParse()
        {
            Debug.Assert(State != EFElementState.Parsed, "this object should not already be in the parsed state");

            ClearEFObject(_id);
            _id = null;
            ClearEFObject(_zoomLevelAttr);
            _zoomLevelAttr = null;
            ClearEFObject(_showGridAttr);
            _showGridAttr = null;
            ClearEFObject(_snapToGridAttr);
            _snapToGridAttr = null;
            ClearEFObject(_displayTypeAttr);
            _displayTypeAttr = null;

            ClearEFObjectCollection(_entityTypeShapes);
            ClearEFObjectCollection(_associationConnectors);
            ClearEFObjectCollection(_inheritanceConnectors);

            base.PreParse();
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal override bool ParseSingleElement(ICollection<XName> unprocessedElements, XElement elem)
        {
            if (elem.Name.LocalName == EntityTypeShape.ElementName)
            {
                var shape = new EntityTypeShape(this, elem);
                shape.Parse(unprocessedElements);
                _entityTypeShapes.Add(shape);
            }
            else if (elem.Name.LocalName == AssociationConnector.ElementName)
            {
                var associationConnector = new AssociationConnector(this, elem);
                associationConnector.Parse(unprocessedElements);
                _associationConnectors.Add(associationConnector);
            }
            else if (elem.Name.LocalName == InheritanceConnector.ElementName)
            {
                var inheritanceConnector = new InheritanceConnector(this, elem);
                inheritanceConnector.Parse(unprocessedElements);
                _inheritanceConnectors.Add(inheritanceConnector);
            }
            else
            {
                return base.ParseSingleElement(unprocessedElements, elem);
            }
            return true;
        }

        #endregion

        private class DiagramIdDefaultableValue : DefaultableValue<string>
        {
            private readonly string _defaultValue;

            internal DiagramIdDefaultableValue(EFElement parent)
                : base(parent, AttributeId)
            {
                // TODO: this is a temporary fix so that the old edmx file (doesn't contain diagramid) could still be loaded.
                // This should be go away once we implement upgrade/diagram fixup logic.
                _defaultValue = Guid.NewGuid().ToString("N");
            }

            internal override string AttributeName
            {
                get { return AttributeId; }
            }

            public override string DefaultValue
            {
                get { return _defaultValue; }
            }
        }

        private class ZoomLevelDefaultableValue : DefaultableValue<int>
        {
            internal ZoomLevelDefaultableValue(EFElement parent)
                : base(parent, AttributeZoomLevel)
            {
            }

            internal override string AttributeName
            {
                get { return AttributeZoomLevel; }
            }

            public override int DefaultValue
            {
                get { return 100; }
            }
        }

        private class ShowGridDefaultableValue : DefaultableValue<bool>
        {
            internal ShowGridDefaultableValue(EFElement parent)
                : base(parent, AttributeShowGrid)
            {
            }

            internal override string AttributeName
            {
                get { return AttributeShowGrid; }
            }

            public override bool DefaultValue
            {
                get { return false; }
            }
        }

        private class SnapToGridDefaultableValue : DefaultableValue<bool>
        {
            internal SnapToGridDefaultableValue(EFElement parent)
                : base(parent, AttributeSnapToGrid)
            {
            }

            internal override string AttributeName
            {
                get { return AttributeSnapToGrid; }
            }

            public override bool DefaultValue
            {
                get { return true; }
            }
        }

        private class DisplayTypeDefaultableValue : DefaultableValue<bool>
        {
            internal DisplayTypeDefaultableValue(EFElement parent)
                : base(parent, AttributeDisplayType)
            {
            }

            internal override string AttributeName
            {
                get { return AttributeDisplayType; }
            }

            public override bool DefaultValue
            {
                get { return false; }
            }
        }
    }
}
