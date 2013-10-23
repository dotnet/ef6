// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.Dialogs
{
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    internal class NoHeaderTabControl : TabControl
    {
        private bool simpleMode;

        [DefaultValue(false)]
        public bool SimpleMode
        {
            get { return simpleMode; }
            set
            {
                simpleMode = value;
                RecreateHandle();
            }
        }

        private bool simpleModeInDesign;

        [DefaultValue(false)]
        public bool SimpleModeInDesign
        {
            get { return simpleModeInDesign; }
            set
            {
                simpleModeInDesign = value;
                RecreateHandle();
            }
        }

        public override Rectangle DisplayRectangle
        {
            get
            {
                if (simpleMode && (!DesignMode || simpleModeInDesign))
                {
                    return new Rectangle(0, 0, Width, Height);
                }
                else
                {
                    return base.DisplayRectangle;
                }
            }
        }
    }
}
