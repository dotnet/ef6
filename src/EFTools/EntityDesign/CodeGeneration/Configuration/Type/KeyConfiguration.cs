// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// Represents a model configuration to set the key of an entity.
    /// </summary>
    public class KeyConfiguration : IFluentConfiguration
    {
        private readonly ICollection<EdmProperty> _keyProperties = new List<EdmProperty>();

        /// <summary>
        /// Gets the properties used for the key of the entity.
        /// </summary>
        public ICollection<EdmProperty> KeyProperties
        {
            get { return _keyProperties; }
        }

        /// <inheritdoc />
        public virtual string GetMethodChain(CodeHelper code)
        {
            // TODO: Throw instead?
            Debug.Assert(code != null, "code is null.");
            Debug.Assert(_keyProperties.Count != 0, "_keyProperties is empty.");

            return ".HasKey(" + code.Lambda(_keyProperties) + ")";
        }
    }
}
