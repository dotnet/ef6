// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Data.Entity.Core.Objects.DataClasses;

    /// <summary>
    ///     Defines options that affect the behavior of the ObjectContext.
    /// </summary>
    public sealed class ObjectContextOptions
    {
        internal ObjectContextOptions()
        {
            ProxyCreationEnabled = true;
        }

        /// <summary>
        ///     Get or set boolean that determines if related ends can be loaded on demand
        ///     when they are accessed through a navigation property.
        /// </summary>
        /// <value> True if related ends can be loaded on demand; otherwise false. </value>
        public bool LazyLoadingEnabled { get; set; }

        /// <summary>
        ///     Get or set boolean that determines whether proxy instances will be create
        ///     for CLR types with a corresponding proxy type.
        /// </summary>
        /// <value> True if proxy instances should be created; otherwise false to create "normal" instances of the type. </value>
        public bool ProxyCreationEnabled { get; set; }

        /// <summary>
        ///     Get or set a boolean that determines whether to use the legacy MergeOption.PreserveChanges behavior
        ///     when querying for entities using MergeOption.PreserveChanges
        /// </summary>
        /// <value> True if the legacy MergeOption.PreserveChanges behavior should be used; otherwise false. </value>
        public bool UseLegacyPreserveChangesBehavior { get; set; }

        /// <summary>
        ///     If this flag is set to false then setting the Value property of the <see cref="EntityReference{TEntity}" /> for an
        ///     FK relationship to null when it is already null will have no effect. When this flag is set to true, then
        ///     setting the value to null will always cause the FK to be nulled and the relationship to be deleted
        ///     even if the value is currently null. The default value is false when using ObjectContext and true
        ///     when using DbContext.
        /// </summary>
        public bool UseConsistentNullReferenceBehavior { get; set; }

        /// <summary>
        ///     This flag determines whether C# behavior should be exhibited when comparing null values in LinqToEntities.
        ///     If this flag is set, then any equality comparison between two operands, both of which are potentially
        ///     nullable, will be rewritten to show C# null comparison semantics. As an example:
        ///     (operand1 = operand2) will be rewritten as
        ///     (((operand1 = operand2) AND NOT (operand1 IS NULL OR operand2 IS NULL)) || (operand1 IS NULL && operand2 IS NULL))
        ///     The default value is false when using <see cref="ObjectContext" />.
        /// </summary>
        public bool UseCSharpNullComparisonBehavior { get; set; }
    }
}
