// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Extensibility
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Defines an EventArgs type that allows extenders of the Entity Designer to drive selection programmatically
    /// </summary>
    public class ChangeEntityDesignerSelectionEventArgs : EventArgs
    {
        internal IEnumerable<string> SelectionIdentifiers { get; private set; }
        internal IServiceProvider LayerServiceProvider { get; private set; }

        /// <summary>
        ///     Instantiate an ChangeEntityDesignerSelectionEventArgs. The 'SelectionIdentifier' in this
        ///     case is a delimited string that corresponds to the hierarchy of the selection from the root.
        ///     For example, to select a property 'Foo' in entity type 'Bar', the SelectionIdentifier would be:
        ///     Foo.Bar.
        /// </summary>
        /// <param name="layerServiceProvider">Service Provider provided by the layer extension</param>
        /// <param name="selectionIdentifiers">A set of string identifiers to drive selection in the Entity Designer</param>
        public ChangeEntityDesignerSelectionEventArgs(IServiceProvider layerServiceProvider, IEnumerable<string> selectionIdentifiers)
        {
            LayerServiceProvider = layerServiceProvider;
            SelectionIdentifiers = selectionIdentifiers;
        }
    }
}
