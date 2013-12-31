// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Diagnostics;

    /// <summary>
    /// Represents a model configuration to set the maximum length of a string or binary property.
    /// </summary>
    public class MaxLengthConfiguration : IAttributeConfiguration, IFluentConfiguration
    {
        /// <summary>
        /// Gets or sets the maximum length for the property.
        /// </summary>
        public int MaxLength { get; set; }

        /// <inheritdoc />
        public virtual string GetAttributeBody(CodeHelper code)
        {
            Debug.Assert(code != null, "code is null.");

            return "MaxLength(" + code.Literal(MaxLength) + ")";
        }

        /// <inheritdoc />
        public virtual string GetMethodChain(CodeHelper code)
        {
            Debug.Assert(code != null, "code is null.");

            return ".HasMaxLength(" + code.Literal(MaxLength) + ")";
        }
    }
}