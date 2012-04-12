namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    /// <summary>
    /// Represents an ast node for a collection type definition.
    /// </summary>
    internal sealed class CollectionTypeDefinition : Node
    {
        private readonly Node _elementTypeDef;

        /// <summary>
        /// Initializes collection type definition using the element type definition.
        /// </summary>
        internal CollectionTypeDefinition(Node elementTypeDef)
        {
            _elementTypeDef = elementTypeDef;
        }

        /// <summary>
        /// Returns collection element type defintion.
        /// </summary>
        internal Node ElementTypeDef
        {
            get { return _elementTypeDef; }
        }
    }

    /// <summary>
    /// Represents an ast node for a reference type definition.
    /// </summary>
    internal sealed class RefTypeDefinition : Node
    {
        private readonly Node _refTypeIdentifier;

        /// <summary>
        /// Initializes reference type definition using the referenced type identifier.
        /// </summary>
        internal RefTypeDefinition(Node refTypeIdentifier)
        {
            _refTypeIdentifier = refTypeIdentifier;
        }

        /// <summary>
        /// Returns referenced type identifier.
        /// </summary>
        internal Node RefTypeIdentifier
        {
            get { return _refTypeIdentifier; }
        }
    }

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

    /// <summary>
    /// Represents an ast node for a property definition (name/type)
    /// </summary>
    internal sealed class PropDefinition : Node
    {
        private readonly Identifier _name;
        private readonly Node _typeDefExpr;

        /// <summary>
        /// Initializes property definition using the name and the type definition.
        /// </summary>
        /// <param name="identifier"></param>
        internal PropDefinition(Identifier name, Node typeDefExpr)
        {
            _name = name;
            _typeDefExpr = typeDefExpr;
        }

        /// <summary>
        /// Returns property name.
        /// </summary>
        internal Identifier Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Returns property type.
        /// </summary>
        internal Node Type
        {
            get { return _typeDefExpr; }
        }
    }
}
