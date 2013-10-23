// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Windows.Forms;

    #region VirtualTreeDisplayDataMasks class

    /// <summary>
    ///     A structure specifying the types of information to return from IBranch.GetDisplayData
    /// </summary>
    internal struct VirtualTreeDisplayDataMasks
    {
        private VirtualTreeDisplayMasks myMask;
        private VirtualTreeDisplayStates myStateMask;

        /// <summary>
        ///     Create a new structure with the given display and state masks
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="stateMask"></param>
        public VirtualTreeDisplayDataMasks(VirtualTreeDisplayMasks mask, VirtualTreeDisplayStates stateMask)
        {
            myMask = mask;
            myStateMask = stateMask;
        }

        /// <summary>
        ///     The fields of the VirtualTreeDisplayData to populate
        /// </summary>
        public VirtualTreeDisplayMasks Mask
        {
            get { return myMask; }
            set { myMask = value; }
        }

        /// <summary>
        ///     The state settings to populate. Refines the VirtualTreeDisplayMasks.State setting.
        /// </summary>
        public VirtualTreeDisplayStates StateMask
        {
            get { return myStateMask; }
            set { myStateMask = value; }
        }

        #region Equals override and related functions

        /// <summary>
        ///     Equals override. Defers to Compare function.
        /// </summary>
        /// <param name="obj">An item to compare to this object</param>
        /// <returns>True if the items are equal</returns>
        public override bool Equals(object obj)
        {
            if (obj is VirtualTreeDisplayDataMasks)
            {
                return Compare(this, (VirtualTreeDisplayDataMasks)obj);
            }
            return false;
        }

        /// <summary>
        ///     GetHashCode override
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            // We're forced to override this with the Equals override.
            return base.GetHashCode();
        }

        /// <summary>
        ///     Equals operator. Defers to Compare.
        /// </summary>
        /// <param name="operand1">Left operand</param>
        /// <param name="operand2">Right operand</param>
        /// <returns></returns>
        public static bool operator ==(VirtualTreeDisplayDataMasks operand1, VirtualTreeDisplayDataMasks operand2)
        {
            return Compare(operand1, operand2);
        }

        /// <summary>
        ///     Compare two VirtualTreeDisplayDataMasks structures
        /// </summary>
        /// <param name="operand1">Left operand</param>
        /// <param name="operand2">Right operand</param>
        /// <returns>true if operands are equal</returns>
        public static bool Compare(VirtualTreeDisplayDataMasks operand1, VirtualTreeDisplayDataMasks operand2)
        {
            return operand1.myMask == operand2.myMask && operand1.myStateMask == operand2.myStateMask;
        }

        /// <summary>
        ///     Not equal operator. Defers to Compare.
        /// </summary>
        /// <param name="operand1">Left operand</param>
        /// <param name="operand2">Right operand</param>
        /// <returns></returns>
        public static bool operator !=(VirtualTreeDisplayDataMasks operand1, VirtualTreeDisplayDataMasks operand2)
        {
            return !Compare(operand1, operand2);
        }

        #endregion // Equals override and related functions
    }

    #endregion

    #region VirtualTreeDisplayData class

    /// <summary>
    ///     Structure returned by the IBranch.GetDisplayData method
    /// </summary>
    internal struct VirtualTreeDisplayData
    {
        private VirtualTreeDisplayStates myState;
        private ImageList myImageList;
        private ImageList myStateImageList;
        private short myImage;
        private short mySelectedImage;
        private short myOverlayIndex;
        private short myStateImage;
        private short myStateImagePadding;
        private bool myHideEmpty;
        private short myForceSelectStart;
        private short myForceSelectLength;
        private Color myForeColor;
        private Color myBackColor;
        private IList myOverlayIndices;

        /// <summary>
        ///     Empty display data, shows an item with no glyph or other features.
        /// </summary>
        public static readonly VirtualTreeDisplayData Empty = new VirtualTreeDisplayData(-1);

        /// <summary>
        ///     Create new display data using images from the tree default image list
        /// </summary>
        /// <param name="image">The image to use for an unselected item state. Use -1 for no image.</param>
        public VirtualTreeDisplayData(short image)
        {
            myForceSelectLength = myForceSelectStart = 0;
            myImageList = null;
            myStateImageList = null;
            myStateImage = -1;
            myOverlayIndex = -1;
            myOverlayIndices = null;
            myState = 0;
            myImage = mySelectedImage = image;
            myForeColor = Color.Empty;
            myBackColor = Color.Empty;
            myStateImagePadding = 0;
            myHideEmpty = false;
        }

        /// <summary>
        ///     Create new display data using images from the tree default image list
        /// </summary>
        /// <param name="image">The image to use for an unselected item state. Use -1 for no image.</param>
        /// <param name="selectedImage">The image to use for a selected item state</param>
        public VirtualTreeDisplayData(short image, short selectedImage)
        {
            myForceSelectLength = myForceSelectStart = 0;
            myImageList = null;
            myStateImageList = null;
            myStateImage = -1;
            myOverlayIndex = -1;
            myOverlayIndices = null;
            myState = 0;
            myImage = image;
            mySelectedImage = selectedImage;
            myForeColor = Color.Empty;
            myBackColor = Color.Empty;
            myStateImagePadding = 0;
            myHideEmpty = false;
        }

        /// <summary>
        ///     Create new display data using images from the provided image list
        /// </summary>
        /// <param name="image">The image to use for an unselected item state. Use -1 for no image.</param>
        /// <param name="selectedImage">The image to use for a selected item state</param>
        /// <param name="overlayIndex">The image to use as an overlay for the given image</param>
        /// <param name="imageList">The imagelist to pull images from</param>
        public VirtualTreeDisplayData(short image, short selectedImage, short overlayIndex, ImageList imageList)
        {
            myForceSelectLength = myForceSelectStart = 0;
            myState = 0;
            myImage = image;
            mySelectedImage = selectedImage;
            myImageList = imageList;
            myStateImageList = null;
            myStateImage = -1;
            myOverlayIndices = null;
            myForeColor = Color.Empty;
            myBackColor = Color.Empty;
            myOverlayIndex = overlayIndex;
            myStateImagePadding = 0;
            myHideEmpty = false;
        }

        /// <summary>
        ///     Create new display data using images from the provided image list
        /// </summary>
        /// <param name="image">The image to use for an unselected item state. Use -1 for no image.</param>
        /// <param name="selectedImage">The image to use for a selected item state</param>
        /// <param name="overlayIndex">The image to use as an overlay for the given image, or a bitfield into the overlayIndices list</param>
        /// <param name="overlayIndices">A integer list of indices in the image list that can be used as overlays. If this is not null, then each bit in the overlayIndex corresponds to an item in this list.</param>
        /// <param name="imageList">The imagelist to pull images from</param>
        public VirtualTreeDisplayData(short image, short selectedImage, short overlayIndex, IList overlayIndices, ImageList imageList)
        {
            myForceSelectLength = myForceSelectStart = 0;
            myState = 0;
            myImage = image;
            mySelectedImage = selectedImage;
            myImageList = imageList;
            myStateImageList = null;
            myStateImage = -1;
            myOverlayIndex = overlayIndex;
            myOverlayIndices = null;
            myForeColor = Color.Empty;
            myBackColor = Color.Empty;
            myStateImagePadding = 0;
            myHideEmpty = false;
            OverlayIndices = overlayIndices;
        }

        /// <summary>
        ///     Create new display data using images from the provided image list
        /// </summary>
        /// <param name="image">The image to use for an unselected item state. Use -1 for no image.</param>
        /// <param name="selectedImage">The image to use for a selected item state</param>
        /// <param name="overlayIndex">The image to use as an overlay for the given image</param>
        /// <param name="imageList">The imagelist to pull images from</param>
        /// <param name="state">Other display settings</param>
        public VirtualTreeDisplayData(
            short image, short selectedImage, short overlayIndex, ImageList imageList, VirtualTreeDisplayStates state)
        {
            myForceSelectLength = myForceSelectStart = 0;
            myImage = image;
            mySelectedImage = selectedImage;
            myImageList = imageList;
            myStateImageList = null;
            myStateImage = -1;
            myOverlayIndex = overlayIndex;
            myOverlayIndices = null;
            myState = state;
            myForeColor = Color.Empty;
            myBackColor = Color.Empty;
            myStateImagePadding = 0;
            myHideEmpty = false;
        }

        /// <summary>
        ///     Create new display data using images from the provided image list
        /// </summary>
        /// <param name="image">The image to use for an unselected item state. Use -1 for no image.</param>
        /// <param name="selectedImage">The image to use for a selected item state</param>
        /// <param name="overlayIndex">The image to use as an overlay for the given image</param>
        /// <param name="overlayIndices">A list of indices in the image list that can be used as overlays. If this is not null, then each bit in the overlayIndex corresponds to an item in this list.</param>
        /// <param name="imageList">The imagelist to pull images from</param>
        /// <param name="state">Other display settings</param>
        public VirtualTreeDisplayData(
            short image, short selectedImage, short overlayIndex, IList overlayIndices, ImageList imageList, VirtualTreeDisplayStates state)
        {
            myForceSelectLength = myForceSelectStart = 0;
            myImage = image;
            mySelectedImage = selectedImage;
            myImageList = imageList;
            myStateImageList = null;
            myStateImage = -1;
            myOverlayIndex = overlayIndex;
            myOverlayIndices = null;
            myState = state;
            myForeColor = Color.Empty;
            myBackColor = Color.Empty;
            myStateImagePadding = 0;
            myHideEmpty = false;
            OverlayIndices = overlayIndices;
        }

        /// <summary>
        ///     Create new display data using images from the tree default image list.
        /// </summary>
        /// <param name="image">The image to use for an unselected item state</param>
        /// <param name="selectedImage">The image to use for a selected item state</param>
        /// <param name="state">Other display settings</param>
        public VirtualTreeDisplayData(short image, short selectedImage, VirtualTreeDisplayStates state)
        {
            myForceSelectLength = myForceSelectStart = 0;
            myImageList = null;
            myStateImageList = null;
            myStateImage = -1;
            myOverlayIndex = -1;
            myOverlayIndices = null;
            myImage = image;
            mySelectedImage = selectedImage;
            myState = state;
            myForeColor = Color.Empty;
            myBackColor = Color.Empty;
            myStateImagePadding = 0;
            myHideEmpty = false;
        }

        /// <summary>
        ///     State information
        /// </summary>
        public VirtualTreeDisplayStates State
        {
            get { return myState; }
            set { myState = value; }
        }

        /// <summary>
        ///     Index for the normal (non-selected) image. Use -1 for no image.
        /// </summary>
        public short Image
        {
            get { return myImage; }
            set { myImage = value; }
        }

        /// <summary>
        ///     Selected image index
        /// </summary>
        public short SelectedImage
        {
            get { return mySelectedImage; }
            set { mySelectedImage = value; }
        }

        /// <summary>
        ///     Provide alternative to default image list, or null to use the default image list.
        /// </summary>
        public ImageList ImageList
        {
            get { return myImageList; }
            set { myImageList = value; }
        }

        /// <summary>
        ///     Start of part of item to always select (for showing search hits)
        /// </summary>
        public short ForceSelectStart
        {
            get { return myForceSelectStart; }
            set { myForceSelectStart = value; }
        }

        /// <summary>
        ///     Length of forced selection
        /// </summary>
        public short ForceSelectLength
        {
            get { return myForceSelectLength; }
            set { myForceSelectLength = value; }
        }

        /// <summary>
        ///     The index of the overlay glyph to apply to the image. If OverlayIndices is
        ///     set, then this comprises a bitfield into that array, giving a maximum of sixteen overlays.
        /// </summary>
        public short OverlayIndex
        {
            get { return myOverlayIndex; }
            set { myOverlayIndex = value; }
        }

        /// <summary>
        ///     A list of indices into the image list that should be used as overlays.
        ///     If this property is set, then the OverlayIndex value is a bitmask into
        ///     this array, with bit 1 corresponding to item 0, bit 2 to item 1, etc.
        ///     The OverlayIndices array can be at most 8 items long. The items are drawn
        ///     in reverse order, so the first item in the array is always drawn on top
        ///     of other overlays.
        /// </summary>
        public IList OverlayIndices
        {
            get { return myOverlayIndices; }
            set
            {
                if (value != null)
                {
                    if (value.Count == 0)
                    {
                        value = null;
                    }
                    else if (value.Count > 8)
                    {
                        throw new ArgumentOutOfRangeException(
                            "value", VirtualTreeStrings.GetString(VirtualTreeStrings.OverlayIndicesRangeExceptionDesc));
                    }
                }
                myOverlayIndices = value;
            }
        }

        /// <summary>
        ///     The index of the state image to apply. Returns -1 to not show the state image.
        /// </summary>
        public short StateImageIndex
        {
            get { return myStateImage; }
            set { myStateImage = value; }
        }

        /// <summary>
        ///     Provide alternative to default state image list, or null to use the default state image list.
        /// </summary>
        public ImageList StateImageList
        {
            get { return myStateImageList; }
            set { myStateImageList = value; }
        }

        /// <summary>
        ///     The item should be drawn gray. Provided as a shortcut for modifying the VirtualTreeDisplayStates.GrayText
        ///     value in the State property.
        /// </summary>
        public bool GrayText
        {
            get { return (State & VirtualTreeDisplayStates.GrayText) != 0; }

            set
            {
                if (value)
                {
                    State |= VirtualTreeDisplayStates.GrayText;
                }
                else
                {
                    State &= ~VirtualTreeDisplayStates.GrayText;
                }
            }
        }

        /// <summary>
        ///     The item should be drawn bold. Provided as a shortcut for modifying the VirtualTreeDisplayStates.Bold
        ///     value in the State property.
        /// </summary>
        public bool Bold
        {
            get { return (State & VirtualTreeDisplayStates.Bold) != 0; }

            set
            {
                if (value)
                {
                    State |= VirtualTreeDisplayStates.Bold;
                }
                else
                {
                    State &= ~VirtualTreeDisplayStates.Bold;
                }
            }
        }

        /// <summary>
        ///     The item should be drawn cut. Provided as a shortcut for modifying the VirtualTreeDisplayStates.Cut
        ///     value in the State property.
        /// </summary>
        public bool Cut
        {
            get { return (State & VirtualTreeDisplayStates.Cut) != 0; }

            set
            {
                if (value)
                {
                    State |= VirtualTreeDisplayStates.Cut;
                }
                else
                {
                    State &= ~VirtualTreeDisplayStates.Cut;
                }
            }
        }

        /// <summary>
        ///     An alternate back color for this item. Should be set in conjunction with
        ///     BackColor to guarantee compatible colors.
        /// </summary>
        public Color ForeColor
        {
            get { return myForeColor; }
            set { myForeColor = value; }
        }

        /// <summary>
        ///     An alternate back color for this item. Should be set in conjunction with
        ///     ForeColor to guarantee compatible colors.
        /// </summary>
        public Color BackColor
        {
            get { return myBackColor; }
            set { myBackColor = value; }
        }

        /// <summary>
        ///     Padding to apply after the state image is drawn
        /// </summary>
        public short StateImagePadding
        {
            get { return myStateImagePadding; }
            set { myStateImagePadding = value; }
        }

        /// <summary>
        ///     Don't show the text field and selection box if the text is empty.
        /// </summary>
        public bool HideEmpty
        {
            get { return myHideEmpty; }
            set { myHideEmpty = value; }
        }

        #region Equals override and related functions

        /// <summary>
        ///     Equals override. Defers to Compare function.
        /// </summary>
        /// <param name="obj">An item to compare to this object</param>
        /// <returns>True if the items are equal</returns>
        public override bool Equals(object obj)
        {
            Debug.Assert(false); // There is no need to compare these
            return false;
        }

        /// <summary>
        ///     GetHashCode override
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            // We're forced to override this with the Equals override.
            return base.GetHashCode();
        }

        /// <summary>
        ///     Equals operator. Defers to Compare.
        /// </summary>
        /// <param name="operand1">Left operand</param>
        /// <param name="operand2">Right operand</param>
        /// <returns>Always returns false, there is no need to compare VirtualTreeDisplayData structures</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand2")]
        public static bool operator ==(VirtualTreeDisplayData operand1, VirtualTreeDisplayData operand2)
        {
            Debug.Assert(false); // There is no need to compare these
            return false;
        }

        /// <summary>
        ///     Compare two VirtualTreeDisplayData structures
        /// </summary>
        /// <param name="operand1">Left operand</param>
        /// <param name="operand2">Right operand</param>
        /// <returns>Always returns false, there is no need to compare VirtualTreeDisplayData structures</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand2")]
        public static bool Compare(VirtualTreeDisplayData operand1, VirtualTreeDisplayData operand2)
        {
            Debug.Assert(false); // There is no need to compare these
            return false;
        }

        /// <summary>
        ///     Not equal operator. Defers to Compare.
        /// </summary>
        /// <param name="operand1">Left operand</param>
        /// <param name="operand2">Right operand</param>
        /// <returns>Always returns true, there is no need to compare VirtualTreeDisplayData structures</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand2")]
        public static bool operator !=(VirtualTreeDisplayData operand1, VirtualTreeDisplayData operand2)
        {
            Debug.Assert(false); // There is no need to compare these
            return true;
        }

        #endregion // Equals override and related functions
    }

    #endregion
}
