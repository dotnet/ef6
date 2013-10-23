// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    /// <summary>
    ///     Used directly by the EdmFeatureManager to test whether a feature
    ///     is supported for a given runtime and indirectly by the BehaviorService to test whether
    ///     a feature is supported for a given workflow.
    /// </summary>
    internal enum FeatureState
    {
        /// <summary>
        ///     A feature is fully visible and enabled
        /// </summary>
        VisibleAndEnabled,

        /// <summary>
        ///     A feature is fully visible but not enabled for user interaction.
        ///     UIs that understand this value should display tooltips, messages to the user.
        /// </summary>
        VisibleButDisabled,

        /// <summary>
        ///     A feature is neither visible nor enabled for user interaction.
        /// </summary>
        Invisible
    }

    /// <summary>
    ///     These extension methods allow the simplicity of setting enum values as states and the simplicity
    ///     of checking mutually exclusive states (enabled/invisible) on clients.
    /// </summary>
    internal static class FeatureSupportedStateExtensions
    {
        internal static bool IsEnabled(this FeatureState state)
        {
            return state == FeatureState.VisibleAndEnabled;
        }

        internal static bool IsVisible(this FeatureState state)
        {
            return state != FeatureState.Invisible;
        }
    }
}
