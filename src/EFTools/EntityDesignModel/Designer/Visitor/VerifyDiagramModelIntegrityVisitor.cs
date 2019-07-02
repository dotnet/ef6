// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Visitor
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Tools.Model.Diagram;

#if DEBUG

    internal class VerifyDiagramModelIntegrityVisitor : VerifyModelIntegrityVisitor
    {
        private readonly HashSet<BaseDiagramObject> _duplicateDiagramObjects = new HashSet<BaseDiagramObject>();
        private readonly HashSet<BaseDiagramObject> _orphanedDiagramObjects = new HashSet<BaseDiagramObject>();
        private readonly HashSet<BaseDiagramObject> _emptyDiagramObjects = new HashSet<BaseDiagramObject>();
        private readonly HashSet<string> _diagramObjectMonikers = new HashSet<string>();

        public VerifyDiagramModelIntegrityVisitor(
            bool checkDisposed, bool checkUnresolved, bool checkXObject, bool checkAnnotations, bool checkBindingIntegrity)
            : base(checkDisposed, checkUnresolved, checkXObject, checkAnnotations, checkBindingIntegrity)
        {
        }

        public VerifyDiagramModelIntegrityVisitor()
        {
        }

        /// <summary>
        ///     Gets a value indicating whether the EFObject should be visited.
        /// </summary>
        protected override bool ShouldVisit(EFObject efObject)
        {
            return (efObject is BaseDiagramObject);
        }

        internal override void Visit(IVisitable visitable)
        {
            base.Visit(visitable);

            var efObject = visitable as EFObject;
            Debug.Assert(efObject != null, "We are visiting an object: '" + visitable + "' that is not an EFObject");
            if (efObject != null
                && ShouldVisit(efObject))
            {
                var diagramObject = efObject as BaseDiagramObject;

                // If diagram object doesn't belong to a diagram, add to orphaned list.
                if (diagramObject.Diagram == null
                    && _orphanedDiagramObjects.Contains(diagramObject) == false)
                {
                    _orphanedDiagramObjects.Add(diagramObject);
                }

                if (diagramObject.ModelItem == null)
                {
                    _emptyDiagramObjects.Add(diagramObject);
                    return;
                }

                // Check for duplicate diagram objects. 
                // Diagram object is considered a duplicate if there exist another object that point to the same EFObject in the same diagram.
                // We construct the moniker for the diagramobject (combination of diagram name and target efobject name); if the moniker
                // exist in our list that means the diagram object is a duplicate.
                var diagramObjectMoniker = GetDiagramMoniker(diagramObject);

                if (_diagramObjectMonikers.Contains(diagramObjectMoniker) == false)
                {
                    _diagramObjectMonikers.Add(diagramObjectMoniker);
                }
                else
                {
                    _duplicateDiagramObjects.Add(diagramObject);
                }
            }
        }

        #region overrides

        internal override string AllSerializedErrors
        {
            get
            {
                var sb = new StringBuilder(base.AllSerializedErrors);

                sb.AppendLine("Diagram objects that are orphaned: ");
                sb.AppendLine(SerializeDiagramObject(_orphanedDiagramObjects));

                sb.AppendLine("Diagram objects that are duplicated: ");
                sb.AppendLine(SerializeDiagramObject(_duplicateDiagramObjects));

                // During undo redo: Diagram object could be restored before the corresponding escher object are restored.
                // TODO: we need to revisit this problem
                /*
                sb.AppendLine("Diagram objects that do not point to any Escher Model: ");
                sb.AppendLine(SerializeDiagramObject(_emptyDiagramObjects));
                */
                return sb.ToString();
            }
        }

        internal override int ErrorCount
        {
            get
            {
                var errorSum = base.ErrorCount;
                errorSum += _duplicateDiagramObjects.Count;
                errorSum += _orphanedDiagramObjects.Count;
                // During undo redo: Diagram object could be restored before the corresponding escher object are restored. 
                // TODO: we need to revisit this problem
                // errorSum += _emptyDiagramObjects.Count;
                return errorSum;
            }
        }

        #endregion

        private static string SerializeDiagramObject(ICollection<BaseDiagramObject> diagramObjects)
        {
            var sb = new StringBuilder();

            foreach (var diagramObject in diagramObjects)
            {
                sb.AppendLine(GetDiagramMoniker(diagramObject));
            }

            return sb.ToString();
        }

        /// <summary>
        ///     Construct the moniker for diagram object which is a combination of diagram name and
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private static string GetDiagramMoniker(BaseDiagramObject diagramObject)
        {
            var diagramName = (diagramObject.Diagram != null ? diagramObject.Diagram.Name : String.Empty);
            if (diagramObject is InheritanceConnector)
            {
                var entityType = diagramObject.ModelItem as ConceptualEntityType;

                Debug.Assert(entityType != null, "Inheritance connector should point to an instance of conceptual entity type.");
                if (entityType != null)
                {
                    Debug.Assert(entityType.SafeBaseType != null, "This entity type should be a derived type.");

                    if (entityType.SafeBaseType != null)
                    {
                        return diagramName + " : " + entityType.SafeBaseType.ToPrettyString() + " : " + entityType.ToPrettyString();
                    }
                }
                return string.Empty;
            }
            else if (diagramObject.ModelItem == null)
            {
                return diagramName + " : " + diagramObject.ToPrettyString();
            }
            else
            {
                return diagramName + " : " + diagramObject.ModelItem.ToPrettyString();
            }
        }
    }

#endif
}
