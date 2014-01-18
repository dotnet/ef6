// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.CSharp;

    /// <summary>
    /// Helper methods for generating C# code.
    /// </summary>
    public class CSharpCodeHelper : CodeHelper
    {
        private static readonly CodeDomProvider _codeProvider = new CSharpCodeProvider();

        /// <inheritdoc />
        protected override CodeDomProvider CodeProvider
        {
            get { return _codeProvider; }
        }

        internal override string TypeArgument(string value)
        {
            Debug.Assert(!string.IsNullOrEmpty(value), "value is null or empty.");

            return "<" + value + ">";
        }

        internal override string Literal(bool value)
        {
            return value ? "true" : "false";
        }

        /// <inheritdoc />
        protected override string AnonymousType(IEnumerable<string> properties)
        {
            Debug.Assert(properties != null, "properties is null.");

            return "new { " + string.Join(", ", properties) + " }";
        }

        internal override string BeginLambda(string control)
        {
            Debug.Assert(!string.IsNullOrEmpty(control), "control is null or empty.");

            return control + " => ";
        }

        /// <inheritdoc />
        protected override string Attribute(string attributeBody)
        {
            Debug.Assert(!string.IsNullOrEmpty(attributeBody), "attributeBody is null or empty.");

            return "[" + attributeBody + "]";
        }

        /// <inheritdoc />
        protected override string StringArray(IEnumerable<string> values)
        {
            Debug.Assert(values != null, "values is null.");

            return "new[] { " + string.Join(", ", values.Select(Literal)) + " }";
        }
    }
}
