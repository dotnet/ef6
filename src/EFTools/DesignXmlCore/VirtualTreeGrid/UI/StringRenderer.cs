// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Text;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using Microsoft.Data.Entity.Design.VisualStudio;

    /// <summary>
    ///     Provides methods for rendering and measuring strings using GDI or GDI+.
    /// </summary>
    internal static class StringRenderer
    {
        public static Size MeasureString(bool useCompatibleTextRendering, Graphics graphics, string text, Font font, StringFormat format)
        {
            return MeasureString(useCompatibleTextRendering, graphics, text, font, RectangleF.Empty, format);
        }

        public static Size MeasureString(
            bool useCompatibleTextRendering, Graphics graphics, string text, Font font, RectangleF bounds, StringFormat format)
        {
            if (string.IsNullOrEmpty(text))
            {
                return Size.Empty;
            }

            if (useCompatibleTextRendering)
            {
                return GdiPlusStringRenderer.MeasureString(graphics, text, font, bounds, format);
            }
            else
            {
                return GdiStringRenderer.MeasureString(graphics, text, font, Rectangle.Round(bounds), format);
            }
        }

        public static void DrawString(
            bool useCompatibleTextRendering, Graphics graphics, string text, Font font, Brush foreBrush, Color foreColor, RectangleF bounds,
            StringFormat format)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            if (useCompatibleTextRendering)
            {
                GdiPlusStringRenderer.DrawString(graphics, text, font, foreBrush, bounds, format);
            }
            else
            {
                GdiStringRenderer.DrawString(graphics, text, font, foreColor, Rectangle.Round(bounds), format);
            }
        }

        /// <summary>
        ///     Provides methods for rendering and measuring strings using GDI+.
        /// </summary>
        private static class GdiPlusStringRenderer
        {
            public static Size MeasureString(Graphics graphics, string text, Font font, RectangleF bounds, StringFormat format)
            {
                var size = Size.Empty;

                using (format = new StringFormat(format))
                {
                    if (bounds.IsEmpty)
                    {
                        bounds = new RectangleF(0, 0, float.MaxValue, float.MaxValue);

                        // We need to clear RightToLeft and set StringAlignment.Near, otherwise we get an incorrect measurement.
                        format.FormatFlags &= ~StringFormatFlags.DirectionRightToLeft;
                        format.Alignment = StringAlignment.Near;
                        format.LineAlignment = StringAlignment.Near;
                    }

                    try
                    {
                        // Measure text
                        format.SetMeasurableCharacterRanges(new[] { new CharacterRange(0, text.Length) });
                        var regions = graphics.MeasureCharacterRanges(text, font, bounds, format);

                        // Need to use Right and Bottom to account for leadingi
                        var rect = regions[0].GetBounds(graphics);
                        size = new Size((int)Math.Ceiling(rect.Right), (int)Math.Ceiling(rect.Bottom));

                        // Remain compatible with Graphics.MeasureString
                        size.Height += 1;
                    }
                    catch (ExternalException ex)
                    {
                        // eat the exception when the text is too long.
                        if (ex.ErrorCode != NativeMethods.E_FAIL)
                        {
                            throw;
                        }

                        size = Size.Empty;
                    }
                }

                return size;
            }

            public static void DrawString(
                Graphics graphics, string text, Font font, Brush foreBrush, RectangleF bounds, StringFormat format)
            {
                try
                {
                    graphics.DrawString(text, font, foreBrush, bounds, format);
                }
                catch (ExternalException ex)
                {
                    // eat the exception when the text is too long.
                    if (ex.ErrorCode != NativeMethods.E_FAIL)
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        ///     Provides methods for rendering and measuring strings using GDI
        /// </summary>
        private static class GdiStringRenderer
        {
            public static Size MeasureString(Graphics graphics, string text, Font font, Rectangle bounds, StringFormat format)
            {
                var size = bounds.Size;
                if (bounds.IsEmpty)
                {
                    size = new Size(int.MaxValue, int.MaxValue);
                }

                // Measure
                size = TextRenderer.MeasureText(graphics, text, font, size, CreateTextFormatFlagsForGdi(format));

                // Remain compatible with Graphics.MeasureString
                size.Height += 1;

                return size;
            }

            public static void DrawString(Graphics graphics, string text, Font font, Color foreColor, Rectangle bounds, StringFormat format)
            {
                TextRenderer.DrawText(graphics, text, font, bounds, foreColor, CreateTextFormatFlagsForGdi(format));
            }

            private static TextFormatFlags CreateTextFormatFlagsForGdi(StringFormat format)
            {
                var flags = TextFormatFlags.PreserveGraphicsTranslateTransform // Respect both GDI+'s transform 
                            | TextFormatFlags.PreserveGraphicsClipping // and active clipping
                            | TextFormatFlags.WordBreak; // This is default behavior for GDI+

                flags |= TranslateFormatFlagsForGdi(format.FormatFlags);
                flags |= TranslateAlignmentForGdi(format.Alignment, true);
                flags |= TranslateAlignmentForGdi(format.LineAlignment, false);
                flags |= TranslateTrimmingForGdi(format.Trimming);
                flags |= TranslateHotkeyPrefixForGdi(format.HotkeyPrefix);

                return flags;
            }

            // Note: The following GDI+ StringFormat -> GDI TextFormatFlags mappings are taken from
            // http://msdn.microsoft.com/msdnmag/issues/06/03/TextRendering/default.aspx?fig=true#fig4
            private static TextFormatFlags TranslateFormatFlagsForGdi(StringFormatFlags formatFlags)
            {
                TextFormatFlags flags = 0;

                // Note: FitBlackBox is actually misnamed and is really NoFitBlackBox
                if ((formatFlags & StringFormatFlags.FitBlackBox) == StringFormatFlags.FitBlackBox)
                {
                    flags |= TextFormatFlags.NoPadding;
                }

                if ((formatFlags & StringFormatFlags.DirectionRightToLeft) == StringFormatFlags.DirectionRightToLeft)
                {
                    flags |= TextFormatFlags.RightToLeft;
                }

                if ((formatFlags & StringFormatFlags.NoClip) == StringFormatFlags.NoClip)
                {
                    flags |= TextFormatFlags.NoClipping;
                }

                if ((formatFlags & StringFormatFlags.LineLimit) == StringFormatFlags.LineLimit)
                {
                    flags |= TextFormatFlags.TextBoxControl;
                }

                if ((formatFlags & StringFormatFlags.NoWrap) == StringFormatFlags.NoWrap)
                {
                    flags |= TextFormatFlags.SingleLine;
                }

                return flags;
            }

            private static TextFormatFlags TranslateAlignmentForGdi(StringAlignment alignment, bool horizontal)
            {
                switch (alignment)
                {
                    case StringAlignment.Near:
                        return horizontal ? TextFormatFlags.Left : TextFormatFlags.Top;

                    case StringAlignment.Center:
                        return horizontal ? TextFormatFlags.HorizontalCenter : TextFormatFlags.VerticalCenter;

                    case StringAlignment.Far:
                        return horizontal ? TextFormatFlags.Right : TextFormatFlags.Bottom;
                }

                Debug.Fail("Unknown StringAlignment");
                return 0;
            }

            private static TextFormatFlags TranslateTrimmingForGdi(StringTrimming trimming)
            {
                switch (trimming)
                {
                    case StringTrimming.None:
                        return 0;

                    case StringTrimming.Character: // There is no equivalent in GDI
                    case StringTrimming.EllipsisCharacter:
                        return TextFormatFlags.EndEllipsis;

                    case StringTrimming.Word: // There is no equivalent in GDI
                    case StringTrimming.EllipsisWord:
                        return TextFormatFlags.WordEllipsis;

                    case StringTrimming.EllipsisPath:
                        return TextFormatFlags.PathEllipsis;
                }

                Debug.Fail("Unknown StringTrimming");
                return 0;
            }

            private static TextFormatFlags TranslateHotkeyPrefixForGdi(HotkeyPrefix prefix)
            {
                switch (prefix)
                {
                    case HotkeyPrefix.Show:
                        return 0;

                    case HotkeyPrefix.Hide:
                        return TextFormatFlags.HidePrefix;

                    case HotkeyPrefix.None:
                        return TextFormatFlags.NoPrefix;
                }

                Debug.Fail("Unknown HotkeyPrefix");
                return 0;
            }
        }
    }
}
