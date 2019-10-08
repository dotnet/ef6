// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Specifies a mapping condition evaluated by comparing the value of 
    /// a property or column with a given value.
    /// </summary>
    public class ValueConditionMapping : ConditionPropertyMapping
    {
        /// <summary>
        /// Creates a ValueConditionMapping instance.
        /// </summary>
        /// <param name="propertyOrColumn">An EdmProperty that specifies a property or column.</param>
        /// <param name="value">An object that specifies the value to compare with.</param>
        public ValueConditionMapping(EdmProperty propertyOrColumn, object value)
            : base(Check.NotNull(propertyOrColumn, "propertyOrColumn"), Check.NotNull(value, "value"), null)
        {
        }

        /// <summary>
        /// Gets an object that specifies the value to check against.
        /// </summary>
        public new object Value
        {
            get { return base.Value; }
        }
    }
}
