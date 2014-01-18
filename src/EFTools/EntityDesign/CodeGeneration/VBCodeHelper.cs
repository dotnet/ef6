// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.VisualBasic;

    /// <summary>
    /// Helper methods for generating Visual Basic code.
    /// </summary>
    public class VBCodeHelper : CodeHelper
    {
        private static readonly CodeDomProvider _codeProvider = new VBCodeProvider();

        /// <inheritdoc />
        protected override CodeDomProvider CodeProvider
        {
            get { return _codeProvider; }
        }

        internal override string TypeArgument(string value)
        {
            Debug.Assert(!string.IsNullOrEmpty(value), "value is null or empty.");

            return "(Of " + value + ")";
        }

        internal override string Literal(bool value)
        {
            return value ? "True" : "False";
        }

        internal override string BeginLambda(string control)
        {
            Debug.Assert(!string.IsNullOrEmpty(control), "control is null or empty.");

            return "Function(" + control + ") ";
        }

        /// <inheritdoc />
        protected override string Attribute(string attributeBody)
        {
            Debug.Assert(!string.IsNullOrEmpty(attributeBody), "attributeBody is null or empty.");

            return "<" + attributeBody + ">";
        }

        /// <inheritdoc />
        protected override string StringArray(IEnumerable<string> values)
        {
            Debug.Assert(values != null, "values is null.");

            return "{" + string.Join(", ", values.Select(Literal)) + "}";
        }

        /// <inheritdoc />
        protected override string AnonymousType(IEnumerable<string> properties)
        {
            Debug.Assert(properties != null, "properties is null.");

            return "New With {" + string.Join(", ", properties) + "}";
        }
    }
}
