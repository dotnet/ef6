// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.Refactoring
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    ///     UI class represents a preview change tree node.
    ///     This node change be used for a header node, a file node or a change node.
    ///     PreviewEngine will call back this node, get information from it and display
    ///     it on UI.
    /// </summary>
    internal sealed class PreviewChangesNode
    {
        private readonly VSTREEDISPLAYDATA _displayData;
        private PreviewChangesNode _parent;
        private List<PreviewChangesNode> _childList;
        private readonly string _displayText = string.Empty;
        private readonly string _tooltipText = string.Empty;
        private Guid _languageServiceID = Guid.Empty;
        private __PREVIEWCHANGESITEMCHECKSTATE _checkState;

        public PreviewChangesNode(
            string displayText,
            VSTREEDISPLAYDATA displayData,
            string tooltipText,
            List<PreviewChangesNode> childList,
            ChangeProposal changeProposal)
        {
            _displayText = displayText;
            _displayData = displayData;
            _tooltipText = tooltipText;
            _childList = childList;
            ChangeProposal = changeProposal;
            if (changeProposal != null
                && changeProposal.Included)
            {
                _checkState = __PREVIEWCHANGESITEMCHECKSTATE.PCCS_Checked;
            }
            else
            {
                _checkState = __PREVIEWCHANGESITEMCHECKSTATE.PCCS_Unchecked;
            }
        }

        /// <summary>
        ///     The display data that controls how the node will be displayed
        ///     Including
        /// </summary>
        public VSTREEDISPLAYDATA DisplayData
        {
            get { return _displayData; }
        }

        /// <summary>
        ///     The list of child nodes
        /// </summary>
        public List<PreviewChangesNode> ChildList
        {
            get { return _childList; }
            set { _childList = value; }
        }

        /// <summary>
        ///     Is the node expandable.
        /// </summary>
        public bool IsExpandable
        {
            get
            {
                if (ChildList != null
                    && ChildList.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        ///     The display text of this node
        /// </summary>
        public string DisplayText
        {
            get { return _displayText; }
        }

        /// <summary>
        ///     The tooltip text of this node
        /// </summary>
        public string TooltipText
        {
            get { return _tooltipText; }
        }

        /// <summary>
        ///     ChangeProposal for this PreviewChangesNode.
        ///     If this is a file node or group node, this property can be null.
        /// </summary>
        public ChangeProposal ChangeProposal { get; internal set; }

        /// <summary>
        ///     Language service will be used on the text buffer in preview window
        ///     when clicking this node.
        /// </summary>
        public Guid LanguageServiceID
        {
            get { return _languageServiceID; }
            set { _languageServiceID = value; }
        }

        /// <summary>
        ///     Show check box for this preview node or not
        /// </summary>
        public bool ShowCheckBox { get; set; }

        /// <summary>
        ///     The check status of this node.
        ///     If this node represents one change, then its check state value come from
        ///     the related ChangeProposal.Included property.
        ///     If this node represents a file node, its check state is calculated according
        ///     to the state of all its child nodes.  The value of the state will be checked/unchecked/PartialChecked.
        ///     If this node is a group node, its check state is also calculated according
        ///     to the state of all its child nodes.
        /// </summary>
        public __PREVIEWCHANGESITEMCHECKSTATE CheckState
        {
            get { return _checkState; }
            set
            {
                // Set the checked state the current node and its children
                SetChecked(value);

                // Set the checked state of the current node's parent
                SetParentChecked();
            }
        }

        /// <summary>
        ///     Set the checked state of the current preview node and all of its child nodes
        /// </summary>
        /// <param name="checkState"></param>
        private void SetChecked(__PREVIEWCHANGESITEMCHECKSTATE checkState)
        {
            _checkState = checkState;

            if (ChangeProposal != null)
            {
                ChangeProposal.Included = (checkState == __PREVIEWCHANGESITEMCHECKSTATE.PCCS_Checked);
            }

            // Recurse to children
            if (_childList != null
                && _childList.Count > 0)
            {
                foreach (var child in _childList)
                {
                    child.SetChecked(checkState);
                }
            }
        }

        /// <summary>
        ///     Set the parent to checked, unchecked or partially checked depending on the checked states of its children
        /// </summary>
        /// <param name="checkState"></param>
        private void SetParentChecked()
        {
            if (_parent != null)
            {
                _parent.ComputeCheckedState();

                // Recurse to parents
                _parent.SetParentChecked();
            }
        }

        /// <summary>
        ///     Add child node to this node.
        /// </summary>
        /// <param name="node"></param>
        public void AddChildNode(PreviewChangesNode node)
        {
            if (_childList == null)
            {
                _childList = new List<PreviewChangesNode>();
            }
            node._parent = this;
            _childList.Add(node);

            ComputeCheckedState();
        }

        /// <summary>
        ///     Add a list of child nodes.
        /// </summary>
        /// <param name="nodes"></param>
        public void AddChildNodes(IList<PreviewChangesNode> nodes)
        {
            if (nodes != null
                && nodes.Count > 0)
            {
                if (_childList == null)
                {
                    _childList = new List<PreviewChangesNode>();
                }
                foreach (var node in nodes)
                {
                    AddChildNode(node);
                }
            }
        }

        private void ComputeCheckedState()
        {
            // Set CheckedState to checked, unchecked or partial depending on checked state of child nodes            
            if (_childList != null
                && _childList.Count > 0)
            {
                var childCount = _childList.Count;
                _checkState = _childList[0]._checkState;

                for (var childIndex = 1; childIndex < childCount; childIndex++)
                {
                    // If initial state is checked, any other child state is unchecked or partial, 
                    // then this node state should be partial.
                    // If initial state is unchecked, any other child state is checked or partial,
                    // this node state will be partial.
                    if (_childList[childIndex]._checkState != _checkState)
                    {
                        _checkState = __PREVIEWCHANGESITEMCHECKSTATE.PCCS_PartiallyChecked;
                        break;
                    }
                }
            }

            if (ChangeProposal != null)
            {
                ChangeProposal.Included = (_checkState == __PREVIEWCHANGESITEMCHECKSTATE.PCCS_Checked)
                                          || (_checkState == __PREVIEWCHANGESITEMCHECKSTATE.PCCS_PartiallyChecked);
            }
        }
    }
}
