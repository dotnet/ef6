// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.Controls
{
    using System.Windows.Forms;

    // <summary>
    //     This class overrides the default behavior of the Up and Down arrow (cursor) keys on a radio button to call an event handler. This
    //     allows users of this control to react to those events themselves to override the default focusing behavior which would otherwise be taken.
    // </summary>
    internal class ArrowOverrideRadioButton : RadioButton
    {
        internal event ArrowPressedEventHandler ArrowPressed;

        protected override bool IsInputKey(Keys key)
        {
            var eventHandler = ArrowPressed;
            switch (key)
            {
                case Keys.Down:
                    if (eventHandler != null)
                    {
                        eventHandler(this, key);
                    }
                    return true;

                case Keys.Up:
                    if (eventHandler != null)
                    {
                        eventHandler(this, key);
                    }
                    return true;

                default:
                    return base.IsInputKey(key);
            }
        }
    }

    internal delegate void ArrowPressedEventHandler(object sender, Keys key);
}
