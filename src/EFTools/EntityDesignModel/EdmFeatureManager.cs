// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.VersioningFacade;

    internal static class EdmFeatureManager
    {
        /// <summary>
        ///     Returns the FeatureState for FunctionImports returning a ComplexType feature
        /// </summary>
        internal static FeatureState GetFunctionImportReturningComplexTypeFeatureState(Version schemaVersion)
        {
            Debug.Assert(EntityFrameworkVersion.IsValidVersion(schemaVersion), "Invalid schema version.");

            return schemaVersion > EntityFrameworkVersion.Version1
                       ? FeatureState.VisibleAndEnabled
                       : FeatureState.VisibleButDisabled;
        }

        /// <summary>
        ///     Whether enum feature is supported in the targeted schema version.
        /// </summary>
        internal static FeatureState GetEnumTypeFeatureState(Version schemaVersion)
        {
            Debug.Assert(EntityFrameworkVersion.IsValidVersion(schemaVersion), "Invalid schema version.");

            return schemaVersion > EntityFrameworkVersion.Version2
                       ? FeatureState.VisibleAndEnabled
                       : FeatureState.VisibleButDisabled;
        }

        /// <summary>
        ///     Returns the FeatureState for the EnumTypes feature
        /// </summary>
        internal static FeatureState GetEnumTypeFeatureState(EFArtifact artifact)
        {
            Debug.Assert(artifact != null, "artifact != null");

            return IsFeatureSupportedForWorkflowAndRuntime(
                artifact,
                (a, existingFs) => GetEnumTypeFeatureState(a.SchemaVersion));
        }

        /// <summary>
        ///     Returns the FeatureState for the 'Get Column Information' functionality for FunctionImports
        /// </summary>
        internal static FeatureState GetFunctionImportColumnInformationFeatureState(EFArtifact artifact)
        {
            Debug.Assert(artifact != null, "artifact != null");

            return IsFeatureSupportedForWorkflowAndRuntime(
                artifact,
                (a, existingFs) => existingFs);
        }

        /// <summary>
        ///     Return the FeatureState for the Function Import mapping feature.
        /// </summary>
        internal static FeatureState GetFunctionImportMappingFeatureState(Version schemaVersion)
        {
            Debug.Assert(EntityFrameworkVersion.IsValidVersion(schemaVersion), "Invalid schema version.");

            return schemaVersion > EntityFrameworkVersion.Version1
                       ? FeatureState.VisibleAndEnabled
                       : FeatureState.VisibleButDisabled;
        }

        /// <summary>
        ///     Returns the FeatureState for the exposed foreign keys in the conceptual model feature
        /// </summary>
        internal static FeatureState GetForeignKeysInModelFeatureState(Version schemaVersion)
        {
            Debug.Assert(EntityFrameworkVersion.IsValidVersion(schemaVersion), "Invalid schema version.");

            return schemaVersion == EntityFrameworkVersion.Version1
                       ? FeatureState.VisibleButDisabled
                       : FeatureState.VisibleAndEnabled;
        }

        /// <summary>
        ///     Return the FeatureState for the GenerateUpdateViews feature
        /// </summary>
        internal static FeatureState GetGenerateUpdateViewsFeatureState(Version schemaVersion)
        {
            Debug.Assert(EntityFrameworkVersion.IsValidVersion(schemaVersion), "Invalid schema version.");

            // This attribute was added in v2 to support Reporting Services
            return schemaVersion > EntityFrameworkVersion.Version1
                       ? FeatureState.VisibleAndEnabled
                       : FeatureState.VisibleButDisabled;
        }

        /// <summary>
        ///     Returns the FeatureState for the EntityContainers' TypeAccess attribute feature
        /// </summary>
        internal static FeatureState GetEntityContainerTypeAccessFeatureState(Version schemaVersion)
        {
            Debug.Assert(EntityFrameworkVersion.IsValidVersion(schemaVersion), "Invalid schema version.");

            return schemaVersion > EntityFrameworkVersion.Version1
                       ? FeatureState.VisibleAndEnabled
                       : FeatureState.VisibleButDisabled;
        }

        /// <summary>
        ///     Return the FeatureState for the LazyLoadingEnabled feature
        /// </summary>
        internal static FeatureState GetLazyLoadingFeatureState(Version schemaVersion)
        {
            Debug.Assert(EntityFrameworkVersion.IsValidVersion(schemaVersion), "Invalid schema version.");

            return schemaVersion > EntityFrameworkVersion.Version1
                       ? FeatureState.VisibleAndEnabled
                       : FeatureState.VisibleButDisabled;
        }

        /// <summary>
        ///     Return the FeatureState for the composable function imports feature
        /// </summary>
        internal static FeatureState GetComposableFunctionImportFeatureState(Version schemaVersion)
        {
            Debug.Assert(EntityFrameworkVersion.IsValidVersion(schemaVersion), "Invalid schema version.");

            return schemaVersion > EntityFrameworkVersion.Version2
                       ? FeatureState.VisibleAndEnabled
                       : FeatureState.VisibleButDisabled;
        }

        /// <summary>
        ///     Return the FeatureState for the UseStrongSpatialTypes feature.
        /// </summary>
        internal static FeatureState GetUseStrongSpatialTypesFeatureState(Version schemaVersion)
        {
            Debug.Assert(EntityFrameworkVersion.IsValidVersion(schemaVersion), "Invalid schema version.");

            return schemaVersion > EntityFrameworkVersion.Version2
                       ? FeatureState.VisibleAndEnabled
                       : FeatureState.VisibleButDisabled;
        }

        /// <summary>
        ///     Captures the common pattern:
        ///     1. Check if the workflow supports the feature
        ///     2. If the workflow allows the feature to be enabled, we may want to disable it based on the runtime.
        /// </summary>
        private static FeatureState IsFeatureSupportedForWorkflowAndRuntime(
            EFArtifact artifact,
            Func<EFArtifact, FeatureState, FeatureState> featureManagerFunction)
        {
            const FeatureState featureState = FeatureState.VisibleAndEnabled;
            Debug.Assert(artifact != null, "Not a valid EFArtifact");
            Debug.Assert(featureManagerFunction != null, "Not a valid featureManager function");

            // If a feature is deemed invisible within the workflow, there's nothing more we can do within the runtime.
            // NOTE: There are no scenarios yet for the case where a feature is disabled (but visible) and the runtime wants to set it to invisible.
            if (featureState.IsEnabled())
            {
                return featureManagerFunction(artifact, featureState);
            }

            return featureState;
        }
    }
}
