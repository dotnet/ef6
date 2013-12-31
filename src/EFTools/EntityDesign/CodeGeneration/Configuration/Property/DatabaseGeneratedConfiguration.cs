// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.CodeGeneration.Extensions;

    /// <summary>
    /// Represents a model configuration to set the database generated option of a property.
    /// </summary>
    public class DatabaseGeneratedConfiguration : IAttributeConfiguration, IFluentConfiguration
    {
        /// <summary>
        /// Gets or sets the pattern used to generate values for the property in the database.
        /// </summary>
        public StoreGeneratedPattern StoreGeneratedPattern { get; set; }

        /// <inheritdoc />
        public virtual string GetAttributeBody(CodeHelper code)
        {
            Debug.Assert(code != null, "code is null.");

            return "DatabaseGenerated(DatabaseGeneratedOption."
                + StoreGeneratedPattern.ToDatabaseGeneratedOption()
                + ")";
        }

        /// <inheritdoc />
        public virtual string GetMethodChain(CodeHelper code)
        {
            Debug.Assert(code != null, "code is null.");

            return ".HasDatabaseGeneratedOption(DatabaseGeneratedOption."
                + StoreGeneratedPattern.ToDatabaseGeneratedOption()
                + ")";
        }
    }
}
