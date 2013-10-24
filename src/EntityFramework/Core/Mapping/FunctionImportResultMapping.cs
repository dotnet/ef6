// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Represents a result mapping for a function import.
    /// </summary>
    public sealed class FunctionImportResultMapping : MappingItem
    {
        private readonly List<FunctionImportStructuralTypeMapping> _typeMappings
            = new List<FunctionImportStructuralTypeMapping>();

        /// <summary>
        /// Gets the type mappings.
        /// </summary>
        public ReadOnlyCollection<FunctionImportStructuralTypeMapping> TypeMappings
        {
            get { return new ReadOnlyCollection<FunctionImportStructuralTypeMapping>(_typeMappings); }
        }

        /// <summary>
        /// Adds a type mapping.
        /// </summary>
        /// <param name="typeMapping">The type mapping to add.</param>
        public void AddTypeMapping(FunctionImportStructuralTypeMapping typeMapping)
        {
            Check.NotNull(typeMapping, "typeMapping");
            ThrowIfReadOnly();

            _typeMappings.Add(typeMapping);
        }

        /// <summary>
        /// Removes a type mapping.
        /// </summary>
        /// <param name="typeMapping">The type mapping to remove.</param>
        public void RemoveTypeMapping(FunctionImportStructuralTypeMapping typeMapping)
        {
            Check.NotNull(typeMapping, "typeMapping");
            ThrowIfReadOnly();

            _typeMappings.Remove(typeMapping);
        }

        internal override void SetReadOnly()
        {
            _typeMappings.TrimExcess();

            SetReadOnly(_typeMappings);

            base.SetReadOnly();
        }

        internal List<FunctionImportStructuralTypeMapping> SourceList
        {
            get { return _typeMappings; }
        }
    }
}
