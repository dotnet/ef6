// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Specifies a mapping condition evaluated by checking whether the value 
    /// of the a property/column is null or not null.
    /// </summary>
    public class IsNullConditionMapping : ConditionPropertyMapping
    {
        /// <summary>
        /// Creates an IsNullConditionMapping instance.
        /// </summary>
        /// <param name="propertyOrColumn">An EdmProperty that specifies a property or column.</param>
        /// <param name="isNull">A boolean that indicates whether to perform a null or a not-null check.</param>
        public IsNullConditionMapping(EdmProperty propertyOrColumn, bool isNull)
            : base(Check.NotNull(propertyOrColumn, "propertyOrColumn"), null, isNull)
        {
        }

        /// <summary>
        /// Gets a bool that specifies whether the condition is evaluated by performing a null check
        /// or a not-null check.
        /// </summary>
        public new bool IsNull
        {
            get { return (bool)base.IsNull; }
        }
    }
}
