// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using LegacyMetadata = System.Data.Metadata.Edm;

namespace Microsoft.Data.Entity.Design.VersioningFacade.LegacyProviderWrapper
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    internal class TypeUsageHelper
    {
        internal static readonly object LegacyVariableValue;
        internal static readonly object LegacyUnboundedValue;
        internal static readonly object VariableValue;
        internal static readonly object UnboundedValue;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static TypeUsageHelper()
        {
            var edmConstantsType =
                typeof(EdmType)
                    .Assembly
                    .GetType("System.Data.Entity.Core.Metadata.Edm.EdmConstants", throwOnError: true, ignoreCase: false);

            VariableValue =
                edmConstantsType
                    .GetField("VariableValue", BindingFlags.Static | BindingFlags.NonPublic)
                    .GetValue(null);

            UnboundedValue =
                edmConstantsType
                    .GetField("UnboundedValue", BindingFlags.Static | BindingFlags.NonPublic)
                    .GetValue(null);

            var legacyEdmConstantsType =
                typeof(LegacyMetadata.EdmType)
                    .Assembly
                    .GetType("System.Data.Metadata.Edm.EdmConstants", throwOnError: true, ignoreCase: false);

            LegacyVariableValue =
                legacyEdmConstantsType
                    .GetField("VariableValue", BindingFlags.Static | BindingFlags.NonPublic)
                    .GetValue(null);

            LegacyUnboundedValue =
                legacyEdmConstantsType
                    .GetField("UnboundedValue", BindingFlags.Static | BindingFlags.NonPublic)
                    .GetValue(null);
        }
    }
}
