// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Utilities;

    public static class ObservableCollectionExtensions
    {
        /// <summary>
        ///     Returns an <see cref="BindingList{T}" /> implementation that stays in sync with the given
        ///     <see cref="ObservableCollection{T}" />.
        /// </summary>
        /// <typeparam name="T"> The element type. </typeparam>
        /// <param name="source"> The collection that the binding list will stay in sync with. </param>
        /// <returns> The binding list. </returns>
        public static BindingList<T> ToBindingList<T>(this ObservableCollection<T> source) where T : class
        {
            Check.NotNull(source, "source");

            var asLocalView = source as DbLocalView<T>;
            return asLocalView != null ? asLocalView.BindingList : new ObservableBackedBindingList<T>(source);
        }
    }
}
