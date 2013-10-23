// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Package
{
    using System;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.UI;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors;

    /// <summary>
    ///     This is a simple selection container object that
    ///     wraps the designers selection system.
    /// </summary>
    internal class EntityDesignSelectionContainer<T> : SelectionContainer<T>
        where T : Selection
    {
        internal EntityDesignSelectionContainer(IServiceProvider shellServices, EditingContext editingContext)
            : base(shellServices, editingContext, PackageManager.Package)
        {
        }

        protected override ObjectDescriptor GetObjectDescriptor(EFElement obj, EditingContext editingContext)
        {
            return PropertyWindowViewModel.GetObjectDescriptor(obj, editingContext, true);
        }
    }
}
