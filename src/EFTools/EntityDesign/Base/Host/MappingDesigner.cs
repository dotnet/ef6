// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Base.Host
{
    using System;
    using Microsoft.Data.Entity.Design.Base.Context;

    // <summary>
    //     The mapping designer class provides a designer.  Most
    //     of the designer API is actually provided through the
    //     context that is passed into the factory, so MappingDesigner
    //     is a fairly sparse class.
    // </summary>
    internal abstract class MappingDesigner : IDisposable
    {
        // <summary>
        //     Returns the editing context for the designer.  A MappingDesigner
        //     may be isolated across a process or a domain boundary, so you cannot
        //     assume that this context represents the actual editing context.  It
        //     may only represent a proxy.
        // </summary>
        internal abstract EditingContext Context { get; }

        // <summary>
        //     Returns the view representing the editable area of the
        //     designer.  This can be null if the context did not supply
        //     the providers necessary to create a view.
        // </summary>
        internal abstract /*UIElement*/ object GetView();

        // <summary>
        //     Implements IDisposable for the designer.
        // </summary>
        public abstract void Dispose();
    }
}
