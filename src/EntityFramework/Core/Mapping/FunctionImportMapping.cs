// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Represents a mapping from a model function import to a store composable or non-composable function.
    /// </summary>
    public abstract class FunctionImportMapping : MappingItem
    {
        private readonly EdmFunction _functionImport;
        private readonly EdmFunction _targetFunction;

        internal FunctionImportMapping(EdmFunction functionImport, EdmFunction targetFunction)
        {
            DebugCheck.NotNull(functionImport);
            DebugCheck.NotNull(targetFunction);

            _functionImport = functionImport;
            _targetFunction = targetFunction;
        }

        /// <summary>
        /// Gets model function (or source of the mapping)
        /// </summary>
        public EdmFunction FunctionImport 
        {
            get { return _functionImport; }
        }

        /// <summary>
        /// Gets store function (or target of the mapping)
        /// </summary>
        public EdmFunction TargetFunction
        {
            get { return _targetFunction; }
        }
    }
}
