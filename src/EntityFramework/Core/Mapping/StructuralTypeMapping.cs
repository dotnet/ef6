// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.ObjectModel;

    /// <summary>
    /// StructuralTypeMapping
    /// </summary>
    public abstract class StructuralTypeMapping : MappingItem
    {
        /// <summary>
        /// Properties
        /// </summary>
        public abstract ReadOnlyCollection<PropertyMapping> Properties { get; }

        internal abstract void AddProperty(PropertyMapping propertyMapping);
        internal abstract void RemoveProperty(PropertyMapping propertyMapping);
    }
}
