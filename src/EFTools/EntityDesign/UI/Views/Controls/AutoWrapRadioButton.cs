// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;

    // <summary>
    //     This helper class is used to override default behavior of a radio button which tries to fit all contents in 1 line.
    // </summary>
    internal class AutoWrapRadioButton : RadioButton
    {
        private Size _cachedTextSize = Size.Empty;
        private readonly Dictionary<Size, Size> _preferredSizeHash = new Dictionary<Size, Size>(3); // set initial size to 3

        public AutoWrapRadioButton()
        {
            AutoSize = true;
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            CacheTextSize();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            CacheTextSize();
        }

        private void CacheTextSize()
        {
            //When the text has changed, the _preferredSizeHash is not valid...
            _preferredSizeHash.Clear();

            if (String.IsNullOrEmpty(Text))
            {
                _cachedTextSize = Size.Empty;
            }
            else
            {
                // Cache the text size value because measuring text can be expensive.
                _cachedTextSize = TextRenderer.MeasureText(Text, Font, new Size(Int32.MaxValue, Int32.MaxValue), TextFormatFlags.WordBreak);
            }
        }

        // <summary>
        //     Retrieves the size of a rectangular area into which a control can be fitted.
        // </summary>
        // <param name="proposedSize">The constraining size; the size for the control should fit into.</param>
        public override Size GetPreferredSize(Size proposedSize)
        {
            var prefSize = base.GetPreferredSize(proposedSize);

            // if the actual control size is greater than the constraining size, recalculate the size because there is a possibility of wrapping. 
            if (!String.IsNullOrEmpty(Text)
                && prefSize.Width > proposedSize.Width)
            {
                // calculate the borders and paddings.
                var bordersAndPadding = prefSize - _cachedTextSize;
                // calculate the new constraint size for our control.
                var newConstraints = proposedSize - bordersAndPadding;
                // check the cache first if we have the preferred size given a constraint (TextRenderer.MeasureText could be expensive).
                if (_preferredSizeHash.ContainsKey(newConstraints))
                {
                    prefSize = _preferredSizeHash[newConstraints];
                }
                else
                {
                    prefSize = bordersAndPadding + TextRenderer.MeasureText(Text, Font, newConstraints, TextFormatFlags.WordBreak);
                    _preferredSizeHash[newConstraints] = prefSize;
                }
            }
            return prefSize;
        }
    }
}
