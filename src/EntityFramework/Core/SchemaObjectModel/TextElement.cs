// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.SchemaObjectModel
{
    using System.Xml;

    /// <summary>
    /// Summary description for Documentation.
    /// </summary>
    internal sealed class TextElement : SchemaElement
    {
        #region Instance Fields

        #endregion

        #region Public Methods

        public TextElement(SchemaElement parentElement)
            : base(parentElement)
        {
        }

        #endregion

        #region Public Properties

        public string Value { get; private set; }

        #endregion

        #region Protected Properties

        protected override bool HandleText(XmlReader reader)
        {
            TextElementTextHandler(reader);
            return true;
        }

        #endregion

        #region Private Methods

        private void TextElementTextHandler(XmlReader reader)
        {
            var text = reader.Value;
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            if (string.IsNullOrEmpty(Value))
            {
                Value = text;
            }
            else
            {
                Value += text;
            }
        }

        #endregion
    }
}
