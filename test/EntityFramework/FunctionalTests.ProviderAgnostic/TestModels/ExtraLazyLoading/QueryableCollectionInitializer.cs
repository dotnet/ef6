// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.ExtraLazyLoading
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Linq;

    public class QueryableCollectionInitializer : CachingCollectionInitializer
    {
        public override object CreateCollection<TElement>(DbCollectionEntry collectionEntry)
        {
            return new QueryableCollection<TElement>(
                (ICollection<TElement>)collectionEntry.CurrentValue,
                collectionEntry.Query().Cast<TElement>());
        }
    }
}
