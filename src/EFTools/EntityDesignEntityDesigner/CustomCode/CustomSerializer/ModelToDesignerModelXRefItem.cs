// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.CustomSerializer
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.VisualStudio.Modeling;

    /// <summary>
    ///     This class contains XRef between DSL Model and Escher Model.
    /// </summary>
    internal class ModelToDesignerModelXRefItem : IDisposable
    {
        private Dictionary<EFObject, ModelElement> _modelToViewModel;
        private Dictionary<ModelElement, EFObject> _viewModelToModel;
        private bool _isDisposed;

        internal ModelToDesignerModelXRefItem()
        {
            _modelToViewModel = new Dictionary<EFObject, ModelElement>();
            _viewModelToModel = new Dictionary<ModelElement, EFObject>();
        }

        internal void Add(EFObject obj, ModelElement viewElement, EditingContext context)
        {
            var viewModel = viewElement as EntityDesignerViewModel;
            if (viewModel != null)
            {
                viewModel.EditingContext = context;
            }

            Remove(obj);
            Remove(viewElement);

            _modelToViewModel.Add(obj, viewElement);
            _viewModelToModel.Add(viewElement, obj);
        }

        internal void Remove(EFObject obj, ModelElement viewElement)
        {
            _modelToViewModel.Remove(obj);
            _viewModelToModel.Remove(viewElement);
        }

        internal void Remove(ModelElement viewElement)
        {
            EFObject efObject;

            if (_viewModelToModel.TryGetValue(viewElement, out efObject))
            {
                Remove(efObject, viewElement);
            }
        }

        internal void Remove(EFObject efObject)
        {
            ModelElement viewElement;

            if (_modelToViewModel.TryGetValue(efObject, out viewElement))
            {
                Remove(efObject, viewElement);
            }
        }

        internal ModelElement GetExisting(EFObject obj)
        {
            ModelElement result;
            _modelToViewModel.TryGetValue(obj, out result);
            return result;
        }

        internal EFObject GetExisting(ModelElement viewElement)
        {
            EFObject result;
            _viewModelToModel.TryGetValue(viewElement, out result);
            return result;
        }

        internal bool ContainsKey(EFObject obj)
        {
            return _modelToViewModel.ContainsKey(obj);
        }

        internal void Clear()
        {
            if (_modelToViewModel != null)
            {
                _modelToViewModel.Clear();
            }
            if (_viewModelToModel != null)
            {
                _viewModelToModel.Clear();
            }
        }

        internal ICollection<ModelElement> ReferencedViewElements
        {
            get
            {
                var objects = new List<ModelElement>();
                foreach (var melem in _viewModelToModel.Keys)
                {
                    objects.Add(melem);
                }

                return objects.AsReadOnly();
            }
        }

        #region IDisposable

        ~ModelToDesignerModelXRefItem()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing && !_isDisposed)
            {
                Clear();
                _modelToViewModel = null;
                _viewModelToModel = null;
                _isDisposed = true;
            }
        }

        #endregion
    }
}
