// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.ExtraLazyLoading
{
    using System.Collections.Generic;

    public static class CollectionExtensions
    {
        public static bool IsLoaded<T>(this ICollection<T> collection)
        {
            var asHasIsLoaded = collection as IHasIsLoaded;
            return asHasIsLoaded != null ? asHasIsLoaded.IsLoaded : true;
        }

        public static void SetLoaded<T>(this ICollection<T> collection, bool isLoaded)
        {
            var asHasIsLoaded = collection as IHasIsLoaded;
            if (asHasIsLoaded != null)
            {
                asHasIsLoaded.IsLoaded = isLoaded;
            }
        }
    }
}
