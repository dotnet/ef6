// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Diagnostics;

    /// <summary>
    /// Represents a model configuration to set the precision of a temporal property.
    /// </summary>
    public class PrecisionDateTimeConfiguration : IFluentConfiguration
    {
        /// <summary>
        /// Gets or sets the precision of the property.
        /// </summary>
        public byte Precision { get; set; }

        /// <inheritdoc />
        public virtual string GetMethodChain(CodeHelper code)
        {
            Debug.Assert(code != null, "code is null.");

            return ".HasPrecision(" + code.Literal(Precision) + ")";
        }
    }
}
