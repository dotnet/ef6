// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    /// <summary>
    /// Represents a model configuration that can be applied using data annotations.
    /// </summary>
    public interface IAttributeConfiguration : IConfiguration
    {
        /// <summary>
        /// Gets the body of the data annotation attribute to apply the configuration.
        /// </summary>
        /// <param name="code">The helper used to generate code.</param>
        /// <returns>The body of the attribute.</returns>
        string GetAttributeBody(CodeHelper code);
    }
}
