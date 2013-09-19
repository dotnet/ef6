// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>
    /// Represents an eSQL metadata member expression classified as <see cref="MetadataMemberClass.FunctionGroup" />.
    /// </summary>
    internal sealed class MetadataFunctionGroup : MetadataMember
    {
        internal MetadataFunctionGroup(string name, IList<EdmFunction> functionMetadata)
            : base(MetadataMemberClass.FunctionGroup, name)
        {
            DebugCheck.NotNull(functionMetadata);
            Debug.Assert(functionMetadata.Count > 0, "FunctionMetadata must not be null or empty");

            FunctionMetadata = functionMetadata;
        }

        internal override string MetadataMemberClassName
        {
            get { return FunctionGroupClassName; }
        }

        internal static string FunctionGroupClassName
        {
            get { return Strings.LocalizedFunction; }
        }

        internal readonly IList<EdmFunction> FunctionMetadata;
    }
}
