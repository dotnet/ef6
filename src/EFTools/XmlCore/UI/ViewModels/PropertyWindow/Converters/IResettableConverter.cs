// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Converters
{
    /// <summary>
    ///     A single instance of a converter is usually cached and reused by the System.ComponentModel
    ///     infrastructure. This class allows for reinitialization of a converter before it gets reused
    ///     when the list of selected objects bound to the property grid changes.
    /// </summary>
    internal interface IResettableConverter
    {
        void Reset();
    }
}
