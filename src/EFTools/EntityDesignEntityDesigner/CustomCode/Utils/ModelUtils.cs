// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using EDMModelHelper = Microsoft.Data.Entity.Design.Model.ModelHelper;

namespace Microsoft.Data.Entity.Design.EntityDesigner.Utils
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Windows.Forms;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.EntityDesigner.CustomSerializer;
    using Microsoft.Data.Entity.Design.EntityDesigner.Properties;
    using Microsoft.Data.Entity.Design.EntityDesigner.View;
    using Microsoft.VisualStudio.Modeling;
    using Microsoft.VisualStudio.Modeling.Diagrams;

    internal static class ModelUtils
    {
        /// <summary>
        ///     Returns true if the store is serializing, false otherwise
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        internal static bool IsSerializing(Store store)
        {
            var serializing = false;

            if ((store != null)
                && (store.TransactionManager != null)
                && (store.TransactionManager.CurrentTransaction != null))
            {
                serializing = store.TransactionManager.CurrentTransaction.IsSerializing;
            }

            return serializing;
        }

        /// <summary>
        ///     Returns the name of Current Active Tx on that Store, otherwise return null
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        internal static Transaction GetCurrentTx(Store store)
        {
            Transaction tx = null;

            if ((store != null)
                && (store.TransactionManager != null))
            {
                tx = store.TransactionManager.CurrentTransaction;
            }

            return tx;
        }

        /// <summary>
        ///     Exports the diagram as an image
        /// </summary>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.FileDialog.set_Filter(System.String)")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "tif")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "png")]
        internal static void ExportAsImage(EntityDesignerDiagram diagram)
        {
            if (diagram != null)
            {
                using (var dlg = new SaveFileDialog())
                {
                    dlg.Title = Resources.ExportAsImageTitle;
                    dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    dlg.Filter = Resources.ImageFormatBmp + "|*.bmp|" +
                                 Resources.ImageFormatJpeg + "|*.jpg|" +
                                 Resources.ImageFormatGif + "|*.gif|" +
                                 Resources.ImageFormatPng + "|*.png|" +
                                 Resources.ImageFormatTiff + "|*.tif";
                    dlg.FileName = Resources.ExportImage_DefaultFileName;
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        var childShapes = diagram.NestedChildShapes;
                        Debug.Assert(childShapes != null && childShapes.Count > 0, "Diagram '" + diagram.Title + "' is empty");

                        if (childShapes != null
                            && childShapes.Count > 0)
                        {
                            var bmp = diagram.CreateBitmap(childShapes, Diagram.CreateBitmapPreference.FavorSmallSizeOverClarity);

                            var imageFormat = ImageFormat.Bmp;
                            var fi = new FileInfo(dlg.FileName);
                            if (fi.Extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase))
                            {
                                imageFormat = ImageFormat.Jpeg;
                            }
                            else if (fi.Extension.Equals(".gif", StringComparison.OrdinalIgnoreCase))
                            {
                                imageFormat = ImageFormat.Gif;
                            }
                            else if (fi.Extension.Equals(".png", StringComparison.OrdinalIgnoreCase))
                            {
                                imageFormat = ImageFormat.Png;
                            }
                            else if (fi.Extension.Equals(".tif", StringComparison.OrdinalIgnoreCase))
                            {
                                imageFormat = ImageFormat.Tiff;
                            }

                            using (var fs = new FileStream(dlg.FileName, FileMode.Create, FileAccess.ReadWrite))
                            {
                                bmp.Save(fs, imageFormat);
                            }
                        }
                    }
                }
            }
        }

        internal static bool IsUniqueName(ModelElement elementToCheck, string proposedName, EditingContext context)
        {
            if (string.IsNullOrEmpty(proposedName)
                || elementToCheck == null)
            {
                return false;
            }

            var xref = ModelToDesignerModelXRef.GetModelToDesignerModelXRef(context);
            var modelItem = xref.GetExisting(elementToCheck);
            if (modelItem != null)
            {
                string msg;
                return EDMModelHelper.IsUniqueNameForExistingItem(modelItem, proposedName, true, out msg);
            }

            return true;
        }
    }
}
