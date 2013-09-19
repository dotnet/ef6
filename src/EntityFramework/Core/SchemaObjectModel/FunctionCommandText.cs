// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.SchemaObjectModel
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Xml;

    /// <summary>
    /// Represents an CommandText element.
    /// </summary>
    internal sealed class FunctionCommandText : SchemaElement
    {
        private string _commandText;

        /// <summary>
        /// Constructs an FunctionCommandText
        /// </summary>
        /// <param name="parentElement"> Reference to the schema element. </param>
        public FunctionCommandText(Function parentElement)
            : base(parentElement)
        {
        }

        public string CommandText
        {
            get { return _commandText; }
        }

        protected override bool HandleText(XmlReader reader)
        {
            _commandText = reader.Value;
            return true;
        }

        internal override void Validate()
        {
            base.Validate();

            if (String.IsNullOrEmpty(_commandText))
            {
                AddError(
                    ErrorCode.EmptyCommandText, EdmSchemaErrorSeverity.Error,
                    Strings.EmptyCommandText);
            }
        }
    }
}
