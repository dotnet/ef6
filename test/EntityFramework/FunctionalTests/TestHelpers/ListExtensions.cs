// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity
{
    using System.Collections.Generic;

    public static class ListExtensions
    {
        public static T AddAndReturn<T>(this List<T> list, T elementToAdd)
        {
            list.Add(elementToAdd);
            return elementToAdd;
        }
    }
}