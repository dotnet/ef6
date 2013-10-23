// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails
{
    using System;
    using Microsoft.Data.Entity.Design.Base.Context;

    /// <summary>
    ///     Contains the ViewModel to support the Mapping Details View.  The root node may
    ///     be driving either an entity mapping UI or an association mapping UI.
    /// </summary>
    internal class MappingViewModel : IDisposable
    {
        private MappingEFElement _rootNode;

        internal MappingViewModel(EditingContext editingContext, MappingEFElement rootNode)
        {
            EditingContext = editingContext;
            _rootNode = rootNode;
        }

        public void Dispose()
        {
            if (_rootNode != null)
            {
                _rootNode.Dispose();
                _rootNode = null;
            }
        }

        internal EditingContext EditingContext { get; set; }

        internal MappingEFElement RootNode
        {
            get { return _rootNode; }
            set { _rootNode = value; }
        }
    }
}
