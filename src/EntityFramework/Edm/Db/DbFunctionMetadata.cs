#if IncludeUnusedEdmCode

namespace System.Data.Entity.Edm.Db
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Internal;

    /// <summary>
    /// Allows the construction and modification of function metadata in a Database Metadata <see cref="DbSchemaMetadata"/>.
    /// </summary>
    internal class DbFunctionMetadata 
        : DbSchemaMetadataItem
    {
        private readonly BackingList<DbFunctionParameterMetadata> parametersList = new BackingList<DbFunctionParameterMetadata>();

        internal override DbItemKind GetMetadataKind()
        {
            return DbItemKind.Function;
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the function can be invoked as an aggregate.
        /// </summary>
        public virtual bool IsAggregate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the function should be invoked without parentheses, for example as 'FunctionName' rather than as 'FunctionName()'.
        /// </summary>
        public virtual bool IsNiladic { get; set; }

        /// <summary>
        /// Gets a value indicating that the function is considered 'built-in' to the store.
        /// </summary>
        public virtual bool IsBuiltIn { get; set; }

        /// <summary>
        /// Gets or sets the optional command text that defines the function.
        /// </summary>
        public virtual string CommandText { get; set; }
                
        /// <summary>
        /// Gets or sets the parameter type semantics, represented by a <see cref="DbParameterTypeSemantics"/> value, that should be applied when considering this function during overload resolution.
        /// </summary>
        public virtual DbParameterTypeSemantics ParameterSemantics { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the function can be composed with other operations.
        /// </summary>
        public virtual bool IsComposable { get; set; }

        /// <summary>
        /// Gets or sets an optional <see cref="DbFunctionTypeMetadata"/> value that specifies the return type of the function.
        /// </summary>
        public virtual DbFunctionTypeMetadata ReturnType { get; set; }

        /// <summary>
        /// Gets or sets the collection of <see cref="DbFunctionParameterMetadata"/> instances that specify the parameters defined by the function. 
        /// </summary>
        public virtual IList<DbFunctionParameterMetadata> Parameters { get { return this.parametersList.EnsureValue(); } set { this.parametersList.SetValue(value); } }

        internal bool HasParameters { get { return this.parametersList.HasValue; } } 
    }
}

#endif