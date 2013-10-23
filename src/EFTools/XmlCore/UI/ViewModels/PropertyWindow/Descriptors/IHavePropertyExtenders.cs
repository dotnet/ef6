// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors
{
    using System.Collections.Generic;

    /// <summary>
    ///     this interface is to be implemented by descriptors and expandable property
    ///     objects that have property extenders. Property extenders allow for sharing
    ///     the implementation of properties that are common to different objects.
    /// </summary>
    internal interface IHavePropertyExtenders
    {
        IList<object> GetPropertyExtenders();
    }
}
