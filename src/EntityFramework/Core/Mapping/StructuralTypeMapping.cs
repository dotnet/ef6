// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.ObjectModel;

    /// <summary>
    /// Specifies a structural type mapping.
    /// </summary>
    public abstract class StructuralTypeMapping : MappingItem
    {
        /// <summary>
        /// Gets a read-only collection of property mappings.
        /// </summary>
        public abstract ReadOnlyCollection<PropertyMapping> Properties { get; }

        /// <summary>
        /// Gets a read-only collection of property mapping conditions.
        /// </summary>
        public abstract ReadOnlyCollection<ConditionPropertyMapping> Conditions { get; }

        /// <summary>
        /// Adds a property mapping.
        /// </summary>
        /// <param name="propertyMapping">The property mapping to be added.</param>
        public abstract void AddProperty(PropertyMapping propertyMapping);

        /// <summary>
        /// Removes a property mapping.
        /// </summary>
        /// <param name="propertyMapping">The property mapping to be removed.</param>
        public abstract void RemoveProperty(PropertyMapping propertyMapping);

        /// <summary>
        /// Adds a property mapping condition.
        /// </summary>
        /// <param name="propertyMapping">The property mapping condition to be added.</param>
        public abstract void AddCondition(ConditionPropertyMapping condition);

        /// <summary>
        /// Removes a property mapping condition.
        /// </summary>
        /// <param name="propertyMapping">The property mapping condition to be removed.</param>
        public abstract void RemoveCondition(ConditionPropertyMapping condition);
    }
}
