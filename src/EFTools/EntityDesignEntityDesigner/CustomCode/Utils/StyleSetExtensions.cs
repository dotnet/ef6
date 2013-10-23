// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.CustomCode.Utils
{
    using System.Drawing;
    using Microsoft.VisualStudio.Modeling.Diagrams;

    /// <summary>
    ///     Helper extension method for StyleSets
    /// </summary>
    internal static class StyleSetExtensions
    {
        /// <summary>
        ///     Overrides brush colors in one step.
        /// </summary>
        /// <param name="styleSet"></param>
        /// <param name="resourceId"></param>
        /// <param name="color"></param>
        public static void OverrideBrushColor(this StyleSet styleSet, StyleSetResourceId resourceId, Color color)
        {
            var settings = styleSet.GetOverriddenBrushSettings(resourceId) ?? new BrushSettings();
            settings.Color = color;
            styleSet.OverrideBrush(resourceId, settings);
        }

        /// <summary>
        ///     Overrides pen colors in one step.
        /// </summary>
        /// <param name="styleSet"></param>
        /// <param name="resourceId"></param>
        /// <param name="color"></param>
        public static void OverridePenColor(this StyleSet styleSet, StyleSetResourceId resourceId, Color color)
        {
            var settings = styleSet.GetOverriddenPenSettings(resourceId) ?? new PenSettings();
            settings.Color = color;
            styleSet.OverridePen(resourceId, settings);
        }
    }
}
