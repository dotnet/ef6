// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql
{
    using System.Collections.Generic;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>
    ///     Represents an eSQL metadata member expression classified as <see cref="MetadataMemberClass.InlineFunctionGroup" />.
    /// </summary>
    internal sealed class InlineFunctionGroup : MetadataMember
    {
        internal InlineFunctionGroup(string name, IList<InlineFunctionInfo> functionMetadata)
            : base(MetadataMemberClass.InlineFunctionGroup, name)
        {
            DebugCheck.NotNull(functionMetadata);
            Debug.Assert(functionMetadata.Count > 0, "FunctionMetadata must not be null or empty");

            FunctionMetadata = functionMetadata;
        }

        internal override string MetadataMemberClassName
        {
            get { return InlineFunctionGroupClassName; }
        }

        internal static string InlineFunctionGroupClassName
        {
            get { return Strings.LocalizedInlineFunction; }
        }

        internal readonly IList<InlineFunctionInfo> FunctionMetadata;
    }
}
