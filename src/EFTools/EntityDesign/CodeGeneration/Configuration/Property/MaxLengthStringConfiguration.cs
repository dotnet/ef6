// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Diagnostics;

    /// <summary>
    /// Represents a model configuration to set the maximum length of a string property.
    /// </summary>
    public class MaxLengthStringConfiguration : MaxLengthConfiguration
    {
        /// <inheritdoc />
        public override string GetAttributeBody(CodeHelper code)
        {
            Debug.Assert(code != null, "code is null.");

            return "StringLength(" + code.Literal(MaxLength) + ")";
        }
    }
}

