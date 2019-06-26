// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.ComponentModel;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Returned by the Configuration method of <see cref="DbContext" /> to provide access to configuration
    /// options for the context.
    /// </summary>
    public class DbContextConfiguration
    {
        #region Construction and fields

        private readonly InternalContext _internalContext;

        // <summary>
        // Initializes a new instance of the <see cref="DbContextConfiguration" /> class.
        // </summary>
        // <param name="internalContext"> The internal context. </param>
        internal DbContextConfiguration(InternalContext internalContext)
        {
            DebugCheck.NotNull(internalContext);

            _internalContext = internalContext;
        }

        #endregion

        #region Hidden Object methods

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Gets the <see cref="Type" /> of the current instance.
        /// </summary>
        /// <returns>The exact runtime type of the current instance.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }

        #endregion

        #region Configuration options
        
        /// <summary>
        /// Gets or sets the value that determines whether SQL functions and commands should be always executed in a transaction.
        /// </summary>
        /// <remarks>
        /// This flag determines whether a new transaction will be started when methods such as <see cref="Database.ExecuteSqlCommand(string,object[])"/>
        /// are executed outside of a transaction.
        /// Note that this does not change the behavior of <see cref="DbContext.SaveChanges()"/>.
        /// </remarks>
        /// <value>
        /// The default transactional behavior.
        /// </value>
        public bool EnsureTransactionsForFunctionsAndCommands
        {
            get { return _internalContext.EnsureTransactionsForFunctionsAndCommands; } 
            set { _internalContext.EnsureTransactionsForFunctionsAndCommands = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether lazy loading of relationships exposed as
        /// navigation properties is enabled.  Lazy loading is enabled by default.
        /// </summary>
        /// <value>
        /// <c>true</c> if lazy loading is enabled; otherwise, <c>false</c> .
        /// </value>
        public bool LazyLoadingEnabled
        {
            get { return _internalContext.LazyLoadingEnabled; }
            set { _internalContext.LazyLoadingEnabled = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the framework will create instances of
        /// dynamically generated proxy classes whenever it creates an instance of an entity type.
        /// Note that even if proxy creation is enabled with this flag, proxy instances will only
        /// be created for entity types that meet the requirements for being proxied.
        /// Proxy creation is enabled by default.
        /// </summary>
        /// <value>
        /// <c>true</c> if proxy creation is enabled; otherwise, <c>false</c> .
        /// </value>
        public bool ProxyCreationEnabled
        {
            get { return _internalContext.ProxyCreationEnabled; }
            set { _internalContext.ProxyCreationEnabled = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether database null semantics are exhibited when comparing
        /// two operands, both of which are potentially nullable. The default value is false.
        /// 
        /// For example (operand1 == operand2) will be translated as:
        /// 
        /// (operand1 = operand2)
        /// 
        /// if UseDatabaseNullSemantics is true, respectively
        /// 
        /// (((operand1 = operand2) AND (NOT (operand1 IS NULL OR operand2 IS NULL))) OR ((operand1 IS NULL) AND (operand2 IS NULL)))
        /// 
        /// if UseDatabaseNullSemantics is false.
        /// </summary>
        /// <value>
        /// <c>true</c> if database null comparison behavior is enabled, otherwise <c>false</c> .
        /// </value>
        public bool UseDatabaseNullSemantics
        {
            get { return _internalContext.UseDatabaseNullSemantics; }
            set { _internalContext.UseDatabaseNullSemantics = value; }
        }

        /// <summary>
        /// By default expression like 
        /// .Select(x => NewProperty = func(x.Property)).Where(x => x.NewProperty == ...)
        /// are simplified to avoid nested SELECT
        /// In some cases, simplifing query with UDFs could caused to suboptimal plans due to calling UDF twice.
        /// Also some SQL functions aren't allow in WHERE clause.
        /// Disabling that behavior
        /// </summary>
        public bool DisableFilterOverProjectionSimplificationForCustomFunctions
        {
            get { return _internalContext.DisableFilterOverProjectionSimplificationForCustomFunctions; }
            set { _internalContext.DisableFilterOverProjectionSimplificationForCustomFunctions = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="DbChangeTracker.DetectChanges()" />
        /// method is called automatically by methods of <see cref="DbContext" /> and related classes.
        /// The default value is true.
        /// </summary>
        /// <value>
        /// <c>true</c> if should be called automatically; otherwise, <c>false</c>.
        /// </value>
        public bool AutoDetectChangesEnabled
        {
            get { return _internalContext.AutoDetectChangesEnabled; }
            set { _internalContext.AutoDetectChangesEnabled = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether tracked entities should be validated automatically when
        /// <see cref="DbContext.SaveChanges()" /> is invoked.
        /// The default value is true.
        /// </summary>
        public bool ValidateOnSaveEnabled
        {
            get { return _internalContext.ValidateOnSaveEnabled; }
            set { _internalContext.ValidateOnSaveEnabled = value; }
        }

        #endregion
    }
}
