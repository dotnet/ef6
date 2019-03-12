// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Imaging.Interop;
    using Microsoft.VisualStudio.Shell.Interop;
    using GelUtils = Microsoft.Internal.VisualStudio.PlatformUI.Utilities;

    internal class ImageManifestUtils
    {
        // The GUID/IDs below must be kept in sync with those defined in the imagemanifest file
        internal static readonly Guid ImageManifestAssetsGuid = new Guid("5f262f83-4628-4ac5-8e74-69e9b42794cc");
        internal static readonly ImageMoniker CodeFirstFromDatabaseImageMoniker =
            new ImageMoniker { Guid = ImageManifestAssetsGuid, Id = 0 };
        internal static readonly ImageMoniker DatabaseImageMoniker =
            new ImageMoniker { Guid = ImageManifestAssetsGuid, Id = 1 };
        internal static readonly ImageMoniker EmptyModelImageMoniker =
            new ImageMoniker { Guid = ImageManifestAssetsGuid, Id = 2 };
        internal static readonly ImageMoniker EmptyModelCodeFirstImageMoniker =
            new ImageMoniker { Guid = ImageManifestAssetsGuid, Id = 3 };
        internal static readonly ImageMoniker DatabaseSchemaImageMoniker =
            new ImageMoniker { Guid = ImageManifestAssetsGuid, Id = 4 };
        internal static readonly ImageMoniker DbAddedItemsImageMoniker =
            new ImageMoniker { Guid = ImageManifestAssetsGuid, Id = 5 };
        internal static readonly ImageMoniker DbDeletedItemsImageMoniker =
            new ImageMoniker { Guid = ImageManifestAssetsGuid, Id = 6 };
        internal static readonly ImageMoniker DBStoredProcsImageMoniker =
            new ImageMoniker { Guid = ImageManifestAssetsGuid, Id = 7 };
        internal static readonly ImageMoniker DbTablesImageMoniker =
            new ImageMoniker { Guid = ImageManifestAssetsGuid, Id = 8 };
        internal static readonly ImageMoniker DbUpdatedItemsImageMoniker =
            new ImageMoniker { Guid = ImageManifestAssetsGuid, Id = 9 };
        internal static readonly ImageMoniker DbViewsImageMoniker =
            new ImageMoniker { Guid = ImageManifestAssetsGuid, Id = 10 };
        internal static readonly ImageMoniker DeletedItemImageMoniker =
            new ImageMoniker { Guid = ImageManifestAssetsGuid, Id = 11 };
        internal static readonly ImageMoniker ErrorImageMoniker =
            new ImageMoniker { Guid = ImageManifestAssetsGuid, Id = 12 };
        internal static readonly ImageMoniker InformationImageMoniker =
            new ImageMoniker { Guid = ImageManifestAssetsGuid, Id = 13 };
        internal static readonly ImageMoniker PageIconImageMoniker =
            new ImageMoniker { Guid = ImageManifestAssetsGuid, Id = 14 };
        internal static readonly ImageMoniker StoredProcImageMoniker =
            new ImageMoniker { Guid = ImageManifestAssetsGuid, Id = 15 };
        internal static readonly ImageMoniker TableImageMoniker =
            new ImageMoniker { Guid = ImageManifestAssetsGuid, Id = 16 };
        internal static readonly ImageMoniker ViewImageMoniker =
            new ImageMoniker { Guid = ImageManifestAssetsGuid, Id = 17 };

        private readonly IVsImageService2 _imageService;
        private readonly ImageAttributes _defaultImageAttributes = new ImageAttributes
            {
                StructSize = Marshal.SizeOf(typeof(ImageAttributes)),
                ImageType = (uint) _UIImageType.IT_Bitmap,
                Format = (uint)_UIDataFormat.DF_WinForms,
                LogicalWidth = 16,
                LogicalHeight = 16,
                Background = (uint)Color.Magenta.ToArgb(), // Desired RGBA color, if you don't use this, don't set IAF_Background
                Flags = unchecked((uint)(_ImageAttributesFlags.IAF_RequiredFlags | _ImageAttributesFlags.IAF_Background))
            };

        private ImageManifestUtils()
        {
            _imageService = (IVsImageService2)
                Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SVsImageService));
        }

        public static ImageManifestUtils Instance { get; } = new ImageManifestUtils();

        public Bitmap GetBitmap(ImageMoniker moniker, int dpi, int? size = null)
        {
            var imageAttributes = _defaultImageAttributes;
            imageAttributes.Dpi = dpi;
            if (size.HasValue)
            {
                imageAttributes.LogicalHeight = imageAttributes.LogicalWidth = size.Value;
            }

            var uiObj = _imageService.GetImage(moniker, imageAttributes);
            Debug.Assert(uiObj != null, typeof(ImageManifestUtils).Name
                + " could not find image with moniker {" + moniker.Guid + "," + moniker.Id + "}");

            return (Bitmap)GelUtils.GetObjectData(uiObj);
        }
    }
}
