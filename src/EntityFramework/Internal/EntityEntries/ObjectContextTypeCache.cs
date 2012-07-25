// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Internal
{
    using System.Collections.Concurrent;
    using System.Data.Entity.Core.Objects;

    internal static class ObjectContextTypeCache
    {
        private static readonly ConcurrentDictionary<Type, Type> _typeCache = new ConcurrentDictionary<Type, Type>();

        public static Type GetObjectType(Type type)
        {
            return _typeCache.GetOrAdd(type, ObjectContext.GetObjectType);
        }
    }
}
