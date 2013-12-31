// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Diagnostics;

    /// <summary>
    /// Represents a model configuration to mark a string or binary property as fixed length.
    /// </summary>
    public class FixedLengthConfiguration : IFluentConfiguration
    {
        /// <inheritdoc />
        public virtual string GetMethodChain(CodeHelper code)
        {
            Debug.Assert(code != null, "code is null.");

            return ".IsFixedLength()";
        }
    }
}
