// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>
    /// Represents a complex type mapping for a function import result.
    /// </summary>
    public sealed class FunctionImportComplexTypeMapping : FunctionImportStructuralTypeMapping
    {
        private readonly ComplexType _returnType;

        /// <summary>
        /// Initializes a new FunctionImportComplexTypeMapping instance.
        /// </summary>
        /// <param name="returnType">The return type.</param>
        /// <param name="properties">The property mappings for the result type of a function import.</param>
        public FunctionImportComplexTypeMapping(
            ComplexType returnType, 
            Collection<FunctionImportReturnTypePropertyMapping> properties)
            : this(
                Check.NotNull(returnType, "returnType"),
                Check.NotNull(properties, "properties"), 
                LineInfo.Empty)
        {
        }

        internal FunctionImportComplexTypeMapping(
            ComplexType returnType, Collection<FunctionImportReturnTypePropertyMapping> properties, LineInfo lineInfo)
            : base(properties, lineInfo)
        {
            DebugCheck.NotNull(returnType);
            DebugCheck.NotNull(properties);

            _returnType = returnType;
        }

        /// <summary>
        /// Ges the return type.
        /// </summary>
        public ComplexType ReturnType
        {
            get { return _returnType; }
        }
    }
}
