// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.Refactoring
{
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TextManager.Interop;

    [Guid(RefactoringGuids.RefactoringPreviewChangesListString)]
    internal class PreviewChangesList : IVsPreviewChangesList, IVsLiteTreeList
    {
        private readonly IList<PreviewChangesNode> _changeList;
        private readonly PreviewData _previewData;
        private readonly PreviewBuffer _previewBuffer;

        public PreviewChangesList(IList<PreviewChangesNode> changeList, PreviewData previewData, PreviewBuffer previewBuffer)
        {
            _changeList = changeList;
            if (_changeList == null)
            {
                _changeList = new List<PreviewChangesNode>();
            }
            _previewData = previewData;
            _previewBuffer = previewBuffer;
        }

        #region IVsSimplePreviewChangesList Members

        /// <summary>
        ///     Retrieve information to draw the item.
        /// </summary>
        /// <param name="index">The index of the item.</param>
        /// <param name="pData">The display data to set.  Note: the array size is always 1</param>
        public int GetDisplayData(uint index, VSTREEDISPLAYDATA[] pData)
        {
            ArgumentValidation.CheckForOutOfRangeException(index, 0, _changeList.Count - 1);
            var previewNode = _changeList[(int)index];
            var displayData = previewNode.DisplayData;
            if (previewNode.ShowCheckBox)
            {
                displayData.State = displayData.State | ((uint)previewNode.CheckState) << 12;
            }
            pData[0] = displayData;
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     Is this item expandable?  Not called if TF_NOEXPANSION is set
        /// </summary>
        /// <param name="index"></param>
        /// <param name="pfExpandable"></param>
        /// <returns></returns>
        public int GetExpandable(uint index, out int pfExpandable)
        {
            ArgumentValidation.CheckForOutOfRangeException(index, 0, _changeList.Count - 1);

            pfExpandable = _changeList[(int)index].IsExpandable ? 1 : 0;
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     An item has been expanded, get the next list
        /// </summary>
        /// <param name="index"></param>
        /// <param name="pfCanRecurse"></param>
        /// <param name="ppIVsSimplePreviewChangesList"></param>
        /// <returns></returns>
        public int GetExpandedList(uint index, out int pfCanRecurse, out IVsLiteTreeList pptlNode)
        {
            ArgumentValidation.CheckForOutOfRangeException(index, 0, _changeList.Count - 1);

            pfCanRecurse = 0;
            if (!_changeList[(int)index].IsExpandable)
            {
                pptlNode = null;
                return VSConstants.E_NOTIMPL;
            }
            else
            {
                var previewChangesList = new PreviewChangesList(_changeList[(int)index].ChildList, _previewData, _previewBuffer);
                pptlNode = previewChangesList;
                return VSConstants.S_OK;
            }
        }

        /// <summary>
        ///     Flag of the display data
        /// </summary>
        /// <param name="pFlags"></param>
        /// <returns></returns>
        public int GetFlags(out uint pFlags)
        {
            pFlags = 0;
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     Count of items in this list
        /// </summary>
        /// <param name="pCount"></param>
        /// <returns></returns>
        public int GetItemCount(out uint pCount)
        {
            pCount = (uint)_changeList.Count;
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     Get list changes.
        /// </summary>
        /// <param name="pcChanges"></param>
        /// <param name="prgListChanges"></param>
        /// <returns></returns>
        public int GetListChanges(ref uint pcChanges, VSTREELISTITEMCHANGE[] prgListChanges)
        {
            // Currently we do not have list changes, do not need to implement this.
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        ///     Get a pointer to the main text for the list item.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="tto"></param>
        /// <param name="pbstrText"></param>
        /// <returns></returns>
        public int GetText(uint index, VSTREETEXTOPTIONS tto, out string ppszText)
        {
            ArgumentValidation.CheckForOutOfRangeException(index, 0, _changeList.Count - 1);

            ppszText = _changeList[(int)index].DisplayText;
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     Get a pointer to the tip text for the list item. If you want tiptext to be same as TTO_DISPLAYTEXT, you can
        ///     E_NOTIMPL this call.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="eTipType"></param>
        /// <param name="pbstrText"></param>
        /// <returns></returns>
        public int GetTipText(uint index, VSTREETOOLTIPTYPE eTipType, out string ppszText)
        {
            ArgumentValidation.CheckForOutOfRangeException(index, 0, _changeList.Count - 1);

            ppszText = _changeList[(int)index].TooltipText;
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     Called during a ReAlign command if TF_CANTRELOCATE isn't set.  Return
        ///     E_FAIL if the list can't be located, in which case the list will be discarded.
        /// </summary>
        /// <param name="pIVsSimplePreviewChangesListChild"></param>
        /// <param name="piIndex"></param>
        /// <returns></returns>
        public int LocateExpandedList(IVsLiteTreeList ExpandedList, out uint iIndex)
        {
            // We do not need to do anything on LocateExpandedList.
            iIndex = 0;
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        ///     Called when a list is collapsed by the user.
        /// </summary>
        /// <param name="ptca"></param>
        /// <returns></returns>
        public int OnClose(VSTREECLOSEACTIONS[] ptca)
        {
            // We do not need to do anything on close.
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        ///     Used to diaplay changes in text view.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="pIUnknownTextView"></param>
        /// <returns></returns>
        public int OnRequestSource(uint index, object pIUnknownTextView)
        {
            ArgumentValidation.CheckForOutOfRangeException(index, 0, _changeList.Count - 1);

            var vsTextView = pIUnknownTextView as IVsTextView;
            if (vsTextView == null)
            {
                return VSConstants.E_NOINTERFACE;
            }
            var node = _changeList[(int)index];
            var file = _previewData.GetFileChange(node);
            _previewBuffer.DisplayPreview(vsTextView, file, node);
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     Toggles the state of the given item (may be more than two states)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="ptscr"></param>
        /// <returns></returns>
        public int ToggleState(uint index, out uint ptscr)
        {
            ArgumentValidation.CheckForOutOfRangeException(index, 0, _changeList.Count - 1);

            var node = _changeList[(int)index];
            if (node.CheckState != __PREVIEWCHANGESITEMCHECKSTATE.PCCS_None)
            {
                var checkState = __PREVIEWCHANGESITEMCHECKSTATE.PCCS_Checked;
                if (node.CheckState == __PREVIEWCHANGESITEMCHECKSTATE.PCCS_Checked)
                {
                    checkState = __PREVIEWCHANGESITEMCHECKSTATE.PCCS_Unchecked;
                }
                node.CheckState = checkState;
                ptscr = (int)_VSTREESTATECHANGEREFRESH.TSCR_ENTIRE;
                _previewBuffer.RefreshTextView();
            }
            else
            {
                ptscr = (int)_VSTREESTATECHANGEREFRESH.TSCR_NONE;
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     Update counter
        /// </summary>
        /// <param name="pCurUpdate"></param>
        /// <param name="pgrfChanges"></param>
        /// <returns></returns>
        public int UpdateCounter(out uint pCurUpdate, out uint pgrfChanges)
        {
            pCurUpdate = 0;
            pgrfChanges = (uint)_VSTREEITEMCHANGESMASK.TCT_NOCHANGE;
            return VSConstants.S_OK;
        }

        #endregion
    }
}
