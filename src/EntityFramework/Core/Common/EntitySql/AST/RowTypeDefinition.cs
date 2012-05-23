namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    /// <summary>
    /// Represents an ast node for a row type definition.
    /// </summary>
    internal sealed class RowTypeDefinition : Node
    {
        private readonly NodeList<PropDefinition> _propDefList;

        /// <summary>
        /// Initializes row type definition using the property definitions.
        /// </summary>
        internal RowTypeDefinition(NodeList<PropDefinition> propDefList)
        {
            _propDefList = propDefList;
        }

        /// <summary>
        /// Returns property definitions.
        /// </summary>
        internal NodeList<PropDefinition> Properties
        {
            get { return _propDefList; }
        }
    }
}
