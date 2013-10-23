// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Model = Microsoft.Data.Entity.Design.Model.Entity;

namespace Microsoft.Data.Entity.Design.EntityDesigner.CustomSerializer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.VisualStudio.Modeling;
    using Association = Microsoft.Data.Entity.Design.Model.Entity.Association;
    using EntityType = Microsoft.Data.Entity.Design.EntityDesigner.ViewModel.EntityType;
    using NavigationProperty = Microsoft.Data.Entity.Design.Model.Entity.NavigationProperty;

    internal class ModelToDesignerModelXRef : ContextItem
    {
        private readonly Dictionary<Partition, ModelToDesignerModelXRefItem> _globalMapModelAndViewModel =
            new Dictionary<Partition, ModelToDesignerModelXRefItem>();

        #region static methods

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static ModelElement CreateModelElementForEFObjectType(EFObject obj, Partition partition)
        {
            ModelElement modelElement = null;
            var t = obj.GetType();
            if (t == typeof(ConceptualEntityModel))
            {
                modelElement = new EntityDesignerViewModel(partition);
            }
            else if (t == typeof(ConceptualEntityType))
            {
                modelElement = new EntityType(partition);
            }
            else if (t == typeof(ConceptualProperty))
            {
                modelElement = new ScalarProperty(partition);
            }
            else if (t == typeof(ComplexConceptualProperty))
            {
                modelElement = new ComplexProperty(partition);
            }
            else if (t == typeof(Association))
            {
                modelElement = new ViewModel.Association(partition);
            }
            else if (t == typeof(EntityTypeBaseType))
            {
                modelElement = new Inheritance(partition);
            }
            else if (t == typeof(NavigationProperty))
            {
                modelElement = new ViewModel.NavigationProperty(partition);
            }

            return modelElement;
        }

        internal static ModelElement CreateConnectorModelElementForEFObjectType(EFObject obj, ModelElement end1, ModelElement end2)
        {
            ModelElement modelElement = null;
            var t = obj.GetType();
            var et1 = end1 as EntityType;
            var et2 = end2 as EntityType;
            if (t == typeof(Association))
            {
                Debug.Assert(et1 != null && et2 != null, "Unexpected end type for Association model element");
                modelElement = new ViewModel.Association(et1, et2);
            }
            else if (t == typeof(EntityTypeBaseType))
            {
                Debug.Assert(et1 != null && et2 != null, "Unexpected end type for Inheritance model element");
                modelElement = new Inheritance(et1, et2);
            }

            return modelElement;
        }

        internal static ModelToDesignerModelXRef GetModelToDesignerModelXRef(EditingContext context)
        {
            // Update EFObject to ModelElement cross reference so that Search Results can later access it
            var xref = context.Items.GetValue<ModelToDesignerModelXRef>();
            if (xref == null)
            {
                xref = new ModelToDesignerModelXRef();
                context.Items.SetValue(xref);
            }
            return xref;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal static ModelToDesignerModelXRefItem GetModelToDesignerModelXRefItem(EditingContext context, Partition partition)
        {
            var xref = GetModelToDesignerModelXRef(context);
            if (xref._globalMapModelAndViewModel.ContainsKey(partition) == false)
            {
                xref._globalMapModelAndViewModel[partition] = new ModelToDesignerModelXRefItem();
            }
            return xref._globalMapModelAndViewModel[partition];
        }

        internal static ModelElement GetNewOrExisting(EditingContext context, EFObject obj, Partition partition)
        {
            ModelElement result;

            var xref = GetModelToDesignerModelXRef(context);
            result = xref.GetExisting(obj, partition);
            if (result == null)
            {
                result = CreateModelElementForEFObjectType(obj, partition);
                if (result != null)
                {
                    xref.Add(obj, result, context);
                }
            }

            Debug.Assert(result != null);
            return result;
        }

        internal static ModelElement GetNewOrExisting(EditingContext context, EFObject obj, ModelElement end1, ModelElement end2)
        {
            Debug.Assert(end1.Partition == end2.Partition, "ModelElements are not in the same partition");

            ModelElement result;
            var xref = GetModelToDesignerModelXRef(context);
            result = xref.GetExisting(obj, end1.Partition);
            if (result == null)
            {
                result = CreateConnectorModelElementForEFObjectType(obj, end1, end2);
                if (result != null)
                {
                    xref.Add(obj, result, context);
                }
            }

            Debug.Assert(result != null);
            return result;
        }

        internal static IList<ModelElement> GetExisting(EditingContext context, EFObject obj)
        {
            IList<ModelElement> result;

            var xref = GetModelToDesignerModelXRef(context);
            result = xref.GetExisting(obj);

            Debug.Assert(result != null);
            return result;
        }

        internal static ModelElement GetExisting(EditingContext context, EFObject obj, Partition partition)
        {
            var xref = GetModelToDesignerModelXRef(context);
            return xref.GetExisting(obj, partition);
        }

        #endregion

        #region Constructor

        #endregion

        internal override Type ItemType
        {
            get { return typeof(ModelToDesignerModelXRef); }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal void Add(EFObject obj, ModelElement viewElement, EditingContext context)
        {
            if (_globalMapModelAndViewModel.ContainsKey(viewElement.Partition) == false)
            {
                _globalMapModelAndViewModel[viewElement.Partition] = new ModelToDesignerModelXRefItem();
            }
            _globalMapModelAndViewModel[viewElement.Partition].Add(obj, viewElement, context);
        }

        internal void Remove(EFObject obj, ModelElement viewElement)
        {
            Debug.Assert(
                _globalMapModelAndViewModel.ContainsKey(viewElement.Partition), "There is no modelToViewModel map for the partition");
            if (_globalMapModelAndViewModel.ContainsKey(viewElement.Partition))
            {
                _globalMapModelAndViewModel[viewElement.Partition].Remove(obj, viewElement);
            }
        }

        internal IList<ModelElement> GetExisting(EFObject obj)
        {
            IList<ModelElement> list = new List<ModelElement>();
            foreach (var map in _globalMapModelAndViewModel.Values)
            {
                var result = map.GetExisting(obj);
                if (result != null)
                {
                    list.Add(result);
                }
            }
            return list;
        }

        internal ModelElement GetExisting(EFObject obj, Partition partition)
        {
            if (_globalMapModelAndViewModel.ContainsKey(partition))
            {
                return _globalMapModelAndViewModel[partition].GetExisting(obj);
            }
            return null;
        }

        internal EFObject GetExisting(ModelElement viewElement)
        {
            if (_globalMapModelAndViewModel.ContainsKey(viewElement.Partition))
            {
                return _globalMapModelAndViewModel[viewElement.Partition].GetExisting(viewElement);
            }
            return null;
        }

        internal void Clear()
        {
            foreach (var xrefItem in _globalMapModelAndViewModel.Values)
            {
                xrefItem.Clear();
            }
            _globalMapModelAndViewModel.Clear();
        }

        internal void ClearXRefItem(Partition partition)
        {
            if (_globalMapModelAndViewModel.ContainsKey(partition))
            {
                _globalMapModelAndViewModel.Remove(partition);
            }
        }

        internal ICollection<ModelElement> ReferencedViewElements
        {
            get
            {
                var objects = new List<ModelElement>();
                foreach (var xrefItem in _globalMapModelAndViewModel.Values)
                {
                    objects.AddRange(xrefItem.ReferencedViewElements);
                }
                return objects.AsReadOnly();
            }
        }
    }
}
