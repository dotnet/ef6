// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.SchemaObjectModel
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Xml;

    /// <summary>
    ///     Summary description for Documentation.
    /// </summary>
    internal sealed class DocumentationElement : SchemaElement
    {
        #region Instance Fields

        private readonly Documentation _metdataDocumentation = new Documentation();

        #endregion

        #region Public Methods

        /// <summary>
        /// </summary>
        /// <param name="parentElement"> </param>
        public DocumentationElement(SchemaElement parentElement)
            : base(parentElement)
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Returns the wrapped metaDocumentation instance
        /// </summary>
        public Documentation MetadataDocumentation
        {
            get
            {
                _metdataDocumentation.SetReadOnly();
                return _metdataDocumentation;
            }
        }

        #endregion

        #region Protected Properties

        protected override bool HandleElement(XmlReader reader)
        {
            if (base.HandleElement(reader))
            {
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.Summary))
            {
                HandleSummaryElement(reader);
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.LongDescription))
            {
                HandleLongDescriptionElement(reader);
                return true;
            }
            return false;
        }

        #endregion

        #region Private Methods

        protected override bool HandleText(XmlReader reader)
        {
            var text = reader.Value;
            if (!string.IsNullOrWhiteSpace(text))
            {
                AddError(ErrorCode.UnexpectedXmlElement, EdmSchemaErrorSeverity.Error, Strings.InvalidDocumentationBothTextAndStructure);
            }
            return true;
        }

        /// <summary>
        /// </summary>
        /// <param name="reader"> </param>
        private void HandleSummaryElement(XmlReader reader)
        {
            var text = new TextElement(this);

            text.Parse(reader);

            _metdataDocumentation.Summary = text.Value;
        }

        /// <summary>
        /// </summary>
        /// <param name="reader"> </param>
        private void HandleLongDescriptionElement(XmlReader reader)
        {
            var text = new TextElement(this);

            text.Parse(reader);

            _metdataDocumentation.LongDescription = text.Value;
        }

        #endregion
    }
}
