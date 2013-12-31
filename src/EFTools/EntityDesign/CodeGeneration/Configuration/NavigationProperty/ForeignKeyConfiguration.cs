// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// Represents a model configuration to set the foreign key properties of an association.
    /// </summary>
    public class ForeignKeyConfiguration : IFluentConfiguration
    {
        private readonly ICollection<EdmProperty> _properties = new List<EdmProperty>();

        /// <summary>
        /// Gets the properties used for the foreign key of the association.
        /// </summary>
        public ICollection<EdmProperty> Properties
        {
            get { return _properties; }
        }

        /// <inheritdoc />
        public virtual string GetMethodChain(CodeHelper code)
        {
            Debug.Assert(code != null, "code is null.");
            Debug.Assert(_properties.Count != 0, "_properties is empty.");

            return ".HasForeignKey(" + code.Lambda(_properties) + ")";
        }
    }
}
