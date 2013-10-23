// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors
{
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;

    /// <summary>
    ///     Used to announce that a component can provide default values for the
    ///     property descriptors it contains
    /// </summary>
    internal interface IPropertyDescriptorDefaultsProvider
    {
        object GetDescriptorDefaultValue(string propertyDescriptorMethodName);
    }

    /// <summary>
    ///     this class is defined so that all generic types defined by derived types
    ///     have a common base type
    /// </summary>
    internal abstract class ObjectDescriptor : IPropertyDescriptorDefaultsProvider
    {
        /// <summary>
        ///     returns the EFObject object that is wrapped by the descriptor
        /// </summary>
        /// <returns></returns>
        public abstract EFObject WrappedItem { get; }

        public abstract EditingContext EditingContext { get; }

        internal virtual void Initialize(EFObject obj, EditingContext editingContext)
        {
            Initialize(obj, editingContext, true);
        }

        internal abstract void Initialize(EFObject obj, EditingContext editingContext, bool runningInVS);

        public abstract object GetDescriptorDefaultValue(string propertyDescriptorMethodName);
    }
}
