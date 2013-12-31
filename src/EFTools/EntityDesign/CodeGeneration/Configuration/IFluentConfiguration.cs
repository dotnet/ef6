// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    /// <summary>
    /// Represents a model configuration that can be applied using the Code First Fluent API.
    /// </summary>
    public interface IFluentConfiguration : IConfiguration
    {
        /// <summary>
        /// Gets the Fluent API method chain to apply the configuration.
        /// </summary>
        /// <param name="code">The helper used to generate code.</param>
        /// <returns>The method chain.</returns>
        string GetMethodChain(CodeHelper code);
    }
}
