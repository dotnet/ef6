// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using Microsoft.VisualStudio.PlatformUI;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    [SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase")]
    internal static class ThemeUtils
    {
        public static readonly Color TransparentColor = Color.Magenta;

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static ImageList GetThemedImageList(Bitmap bitmap, ThemeResourceKey backgroundColorKey)
        {
            Debug.Assert(bitmap != null, "bitmap != null");
            bitmap.MakeTransparent(TransparentColor);
            var themedBitmap = ThemeBitmap(bitmap, VSColorTheme.GetThemedColor(backgroundColorKey));
            var imageList = new ImageList
                {
                    ColorDepth = ColorDepth.Depth32Bit,
                    ImageSize = new Size(16, 16)
                };
            imageList.Images.AddStrip(themedBitmap);
#if VS12
    // scales images as appropriate for screen resolution
            DpiHelper.LogicalToDeviceUnits(ref imageList);
#endif
            return imageList;
        }

        public static Bitmap GetThemedButtonImage(Bitmap bitmap, ThemeResourceKey backgroundColorKey)
        {
            return GetThemedButtonImage(bitmap, VSColorTheme.GetThemedColor(backgroundColorKey));
        }

        public static Bitmap GetThemedButtonImage(Bitmap bitmap, Color backgroundColor)
        {
            Debug.Assert(bitmap != null, "bitmap != null");
            bitmap.MakeTransparent(TransparentColor);
            var themedBitmap = ThemeBitmap(bitmap, backgroundColor);
#if VS12
    // scales images as appropriate for screen resolution
            DpiHelper.LogicalToDeviceUnits(ref themedBitmap);
#endif
            return themedBitmap;
        }

        private static Bitmap ThemeBitmap(Bitmap bitmap, Color backgroundColor)
        {
            Debug.Assert(bitmap != null, "bitmap != null");
            var uiShell5 = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SVsUIShell)) as IVsUIShell5;
            if (uiShell5 == null)
            {
                return bitmap;
            }
            var bitmapClone = (Bitmap)bitmap.Clone();
            var bitmapData
                = bitmapClone.LockBits(
                    new Rectangle(0, 0, bitmapClone.Width, bitmapClone.Height),
                    ImageLockMode.ReadWrite,
                    PixelFormat.Format32bppArgb);
            var size = Math.Abs(bitmapData.Stride) * bitmapData.Height;
            var bytes = new byte[size];
            Marshal.Copy(bitmapData.Scan0, bytes, 0, size);
            uiShell5.ThemeDIBits(
                (uint)size,
                bytes,
                (uint)bitmapData.Width,
                (uint)bitmapData.Height,
                bitmapData.Stride > 0,
                (uint)ColorTranslator.ToWin32(backgroundColor));
            Marshal.Copy(bytes, 0, bitmapData.Scan0, size);
            bitmapClone.UnlockBits(bitmapData);
            return bitmapClone;
        }
    }
}
