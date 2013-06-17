// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.ObjectModel;

    internal abstract class StructuralTypeMapping
    {
        public abstract ReadOnlyCollection<StoragePropertyMapping> Properties { get; }

        internal abstract void AddProperty(StoragePropertyMapping propertyMapping);
        internal abstract void RemoveProperty(StoragePropertyMapping propertyMapping);
    }
}
