// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.MappingDetails
{
    using System.Windows.Forms;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.VisualStudio.PlatformUI;

    internal static class MappingDetailsImages
    {
        public static readonly short ICONS_TABLE = 0;
        public static readonly short ICONS_FUNCTION = 1;
        public static readonly short ICONS_PROPERTY = 2;
        public static readonly short ICONS_CONDITION = 3;
        public static readonly short ICONS_PARAMETER = 4;
        public static readonly short ICONS_FOLDER = 5;
        public static readonly short ICONS_PROPERTY_KEY = 6;
        public static readonly short ICONS_COLUMN = 7;
        public static readonly short ICONS_RESULT_BINDING = 8;
        public static readonly short ICONS_COLUMN_KEY = 9;
        public static readonly short ICONS_COMPLEX_PROPERTY = 10;

        public static readonly short ARROWS_LEFT = 0;
        public static readonly short ARROWS_BOTH = 1;
        public static readonly short ARROWS_RIGHT = 2;

        public static readonly short TOOLBAR_TABLE = 0;
        public static readonly short TOOLBAR_SPROCS = 1;

        private static ImageList _imageListIcons;
        private static ImageList _imageListArrows;
        private static ImageList _imageListToolbar;

        public static ImageList GetToolbarImageList()
        {
            return _imageListToolbar
                   ?? (_imageListToolbar
                       = ThemeUtils.GetThemedImageList(
                           Resources.MappingDetailsCommandStrip,
                           EnvironmentColors.CommandBarOptionsBackgroundColorKey));
        }

        public static ImageList GetIconsImageList()
        {
            return _imageListIcons
                   ?? (_imageListIcons
                       = ThemeUtils.GetThemedImageList(
                           Resources.MappingDetailsIconsImageList,
                           TreeViewColors.BackgroundColorKey));
        }

        public static ImageList GetArrowsImageList()
        {
            return _imageListArrows
                   ?? (_imageListArrows
                       = ThemeUtils.GetThemedImageList(
                           Resources.MappingDetailsArrowsImageList,
                           TreeViewColors.BackgroundColorKey));
        }

        public static void InvalidateCache()
        {
            _imageListIcons = null;
            _imageListArrows = null;
            _imageListToolbar = null;
        }
    }
}
