namespace System.Data.Entity.Core.EntityModel.SchemaObjectModel
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Xml;

    /// <summary>
    /// Represents an DefiningQuery element.
    /// </summary>
    internal sealed class EntityContainerEntitySetDefiningQuery : SchemaElement
    {
        private string _query;

        /// <summary>
        /// Constructs an EntityContainerEntitySet
        /// </summary>
        /// <param name="parentElement">Reference to the schema element.</param>
        public EntityContainerEntitySetDefiningQuery(EntityContainerEntitySet parentElement)
            : base(parentElement)
        {
        }

        public string Query
        {
            get { return _query; }
        }

        protected override bool HandleText(XmlReader reader)
        {
            _query = reader.Value;
            return true;
        }

        internal override void Validate()
        {
            base.Validate();

            if (String.IsNullOrEmpty(_query))
            {
                AddError(
                    ErrorCode.EmptyDefiningQuery, EdmSchemaErrorSeverity.Error,
                    Strings.EmptyDefiningQuery);
            }
        }
    }
}
