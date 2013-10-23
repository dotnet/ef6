// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Base.Shell
{
    using System.Windows.Forms;

    internal class TreeGridDesignerWatermarkInfo
    {
        internal TreeGridDesignerWatermarkInfo(string text, params LinkData[] linkDatum)
        {
            WatermarkText = text;
            WatermarkLinkData = linkDatum;
        }

        internal string WatermarkText { get; private set; }
        internal LinkData[] WatermarkLinkData { get; private set; }

        internal class LinkData
        {
            internal LinkData(int linkStart, int linkLength, LinkLabelLinkClickedEventHandler handler)
            {
                LinkStart = linkStart;
                LinkLength = linkLength;
                LinkClickedHandler = handler;
            }

            public int LinkStart { get; private set; }
            public int LinkLength { get; private set; }
            public LinkLabelLinkClickedEventHandler LinkClickedHandler { get; private set; }
        }
    }
}
