
#if IncludeUnusedEdmCode

namespace System.Data.Entity.Edm.Db
{
    /// <summary>
    /// Allows the construction and modification of a parameter to a <see cref="DbFunctionMetadata"/> function in a Database Metadata <see cref="DbSchemaMetadata"/> schema.
    /// </summary>
    internal class DbFunctionParameterMetadata 
        : DbNamedMetadataItem
    {
        internal override DbItemKind GetMetadataKind()
        {
            return DbItemKind.FunctionParameter;
        }

        /// <summary>
        /// Gets or sets a value indicating the <see cref="DbParameterMode"/> of the parameter.
        /// </summary>
        public virtual DbParameterMode Mode { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="DbFunctionTypeMetadata"/> that specifies the parameter type.
        /// </summary>
        public virtual DbFunctionTypeMetadata ParameterType { get; set; }
    }
}

#endif
