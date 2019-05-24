// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Data.Entity.Core.Objects.DataClasses;

    /// <summary>
    /// Defines options that affect the behavior of the ObjectContext.
    /// </summary>
    public sealed class ObjectContextOptions
    {
        internal ObjectContextOptions()
        {
            ProxyCreationEnabled = true;
            EnsureTransactionsForFunctionsAndCommands = true;
        }

        /// <summary>
        /// Gets or sets the value that determines whether SQL functions and commands should be always executed in a transaction.
        /// </summary>
        /// <remarks>
        /// This flag determines whether a new transaction will be started when methods such as <see cref="ObjectContext.ExecuteFunction"/>
        /// and <see cref="ObjectContext.ExecuteStoreCommand(string,object[])"/> are executed outside of a transaction.
        /// Note that this does not change the behavior of <see cref="ObjectContext.SaveChanges()"/>.
        /// </remarks>
        /// <value>
        /// The default transactional behavior.
        /// </value>
        public bool EnsureTransactionsForFunctionsAndCommands { get; set; }

        /// <summary>Gets or sets a Boolean value that determines whether related objects are loaded automatically when a navigation property is accessed.</summary>
        /// <returns>true if lazy loading is enabled; otherwise, false.</returns>
        public bool LazyLoadingEnabled { get; set; }

        /// <summary>Gets or sets a Boolean value that determines whether proxy instances are created for custom data classes that are persistence ignorant.</summary>
        /// <returns>true if proxies are created; otherwise, false. The default value is true.</returns>
        public bool ProxyCreationEnabled { get; set; }

        /// <summary>Gets or sets a Boolean value that determines whether to use the legacy PreserveChanges behavior.</summary>
        /// <returns>true if the legacy PreserveChanges behavior should be used; otherwise, false.</returns>
        public bool UseLegacyPreserveChangesBehavior { get; set; }

        /// <summary>Gets or sets a Boolean value that determines whether to use the consistent NullReference behavior.</summary>
        /// <remarks>
        /// If this flag is set to false then setting the Value property of the <see cref="EntityReference{TElement}" /> for an
        /// FK relationship to null when it is already null will have no effect. When this flag is set to true, then
        /// setting the value to null will always cause the FK to be nulled and the relationship to be deleted
        /// even if the value is currently null. The default value is false when using ObjectContext and true
        /// when using DbContext.
        /// </remarks>
        /// <returns>true if the consistent NullReference behavior should be used; otherwise, false.</returns>
        public bool UseConsistentNullReferenceBehavior { get; set; }

        /// <summary>Gets or sets a Boolean value that determines whether to use the C# NullComparison behavior.</summary>
        /// <remarks>
        /// This flag determines whether C# behavior should be exhibited when comparing null values in LinqToEntities.
        /// If this flag is set, then any equality comparison between two operands, both of which are potentially
        /// nullable, will be rewritten to show C# null comparison semantics. As an example:
        /// (operand1 = operand2) will be rewritten as
        /// (((operand1 = operand2) AND NOT (operand1 IS NULL OR operand2 IS NULL)) || (operand1 IS NULL &amp;&amp; operand2 IS NULL))
        /// The default value is false when using <see cref="ObjectContext" />.
        /// </remarks>
        /// <returns>true if the C# NullComparison behavior should be used; otherwise, false.</returns>
        public bool UseCSharpNullComparisonBehavior { get; set; }

        public bool DisableFilterOverProjectionSimplificationForCustomFunctions { get; set; }
    }
}
