// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.Model.VisualStudio
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Xml.Linq;
    using Microsoft.Data.Tools.XmlDesignerBase.Model;
    using Microsoft.VisualStudio.XmlEditor;
    using XmlModel = Microsoft.Data.Tools.XmlDesignerBase.Model.XmlModel;

    internal sealed class VSXmlTransaction : XmlTransaction
    {
        private readonly XmlEditingScope _editorTransaction;
        private readonly VSXmlModelProvider _provider;

        private readonly Dictionary<XmlModelChange, IXmlChange> _changeMap =
            new Dictionary<XmlModelChange, IXmlChange>();

        public VSXmlTransaction(
            VSXmlModelProvider provider,
            XmlEditingScope editorTransaction)
        {
            _provider = provider;
            _editorTransaction = editorTransaction;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    _editorTransaction.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override XmlModelProvider Provider
        {
            get { return _provider; }
        }

        public override string Name
        {
            get { return _editorTransaction.Name; }
        }

        public override XmlTransaction Parent
        {
            get
            {
                var parentTx =
                    _provider.GetTransaction(_editorTransaction.Parent);
                return parentTx;
            }
        }

        public override object UserState
        {
            get { return _editorTransaction.UserState; }
        }

        public override object UndoUserState
        {
            get
            {
                object undoUserState = null;
                if (_editorTransaction.UndoScope != null)
                {
                    undoUserState = _editorTransaction.UndoScope.UserState;
                }
                return undoUserState;
            }
        }

        public override XmlTransactionStatus Status
        {
            get
            {
                var status = XmlTransactionStatus.Aborted;
                switch (_editorTransaction.Status)
                {
                    case XmlEditingScopeStatus.Reverted:
                        status = XmlTransactionStatus.Aborted;
                        break;
                    case XmlEditingScopeStatus.Active:
                        status = XmlTransactionStatus.Active;
                        break;
                    case XmlEditingScopeStatus.Completed:
                        status = XmlTransactionStatus.Committed;
                        break;
                    default:
                        status = XmlTransactionStatus.Aborted;
                        break;
                }
                return status;
            }
        }

        public override IEnumerable<IXmlChange> Changes()
        {
            foreach (var model in _provider.OpenXmlModels)
            {
                foreach (var change in Changes(model))
                {
                    yield return change;
                }
            }
        }

        public override IEnumerable<IXmlChange> Changes(XmlModel model)
        {
            var vsXmlModel = model as VSXmlModel;
            Debug.Assert(vsXmlModel != null);
            if (vsXmlModel != null)
            {
                var internalModel = vsXmlModel.XmlModel;
                foreach (var modelChange in _editorTransaction.Changes(internalModel))
                {
                    yield return GetXmlChange(modelChange);
                }
            }
        }

        public override void Commit()
        {
            _editorTransaction.Complete();
        }

        public override void Rollback()
        {
            _editorTransaction.Revert();
        }

        private IXmlChange GetXmlChange(XmlModelChange modelChange)
        {
            IXmlChange result = null;
            if (_changeMap.TryGetValue(modelChange, out result))
            {
                return result;
            }

            if (result == null)
            {
                var addChange = modelChange as AddNodeChange;
                if (addChange != null)
                {
                    result = new VSXmlAddNodeChange(addChange);
                }
            }

            if (result == null)
            {
                var removeChange = modelChange as RemoveNodeChange;
                if (removeChange != null)
                {
                    result = new VSXmlRemoveNodeChange(removeChange);
                }
            }

            if (result == null)
            {
                var nodeNameChange = modelChange as NodeNameChange;
                if (nodeNameChange != null)
                {
                    result = new VSXmlNodeNameChange(nodeNameChange);
                }
            }

            if (result == null)
            {
                var nodeValueChange = modelChange as NodeValueChange;
                if (nodeValueChange != null)
                {
                    result = new VSXmlNodeValueChange(nodeValueChange);
                }
            }

            if (result != null)
            {
                _changeMap[modelChange] = result;
            }

            return result;
        }
    }

    internal class VSXmlChange : IXmlChange
    {
        private readonly XObject _node;
        private readonly XObjectChange _action;

        internal VSXmlChange(XmlModelChange modelChange)
        {
            _node = modelChange.Node;
            _action = modelChange.Action;
        }

        public XObject Node
        {
            get { return _node; }
        }

        public XObjectChange Action
        {
            get { return _action; }
        }
    }

    internal class VSXmlAddNodeChange : VSXmlChange, IXmlAddNodeChange
    {
        private readonly XObject _nextNode;
        private readonly XContainer _parent;

        internal VSXmlAddNodeChange(AddNodeChange modelChange)
            : base(modelChange)
        {
            _nextNode = modelChange.NextNode;
            _parent = modelChange.Parent;
        }

        public XObject NextNode
        {
            get { return _nextNode; }
        }

        public XContainer Parent
        {
            get { return _parent; }
        }
    }

    internal class VSXmlRemoveNodeChange : VSXmlChange, IXmlRemoveNodeChange
    {
        private readonly XObject _nextNode;
        private readonly XContainer _parent;

        internal VSXmlRemoveNodeChange(RemoveNodeChange modelChange)
            : base(modelChange)
        {
            _nextNode = modelChange.NextNode;
            _parent = modelChange.Parent;
        }

        public XObject NextNode
        {
            get { return _nextNode; }
        }

        public XContainer Parent
        {
            get { return _parent; }
        }
    }

    internal class VSXmlNodeNameChange : VSXmlChange, IXmlNodeNameChange
    {
        private readonly XName _oldName;
        private readonly XName _newName;

        internal VSXmlNodeNameChange(NodeNameChange modelChange)
            : base(modelChange)
        {
            _oldName = modelChange.OldName;
            _newName = modelChange.NewName;
        }

        public XName OldName
        {
            get { return _oldName; }
        }

        public XName NewName
        {
            get { return _newName; }
        }
    }

    internal class VSXmlNodeValueChange : VSXmlChange, IXmlNodeValueChange
    {
        private readonly string _oldValue;
        private readonly string _newValue;

        internal VSXmlNodeValueChange(NodeValueChange modelChange)
            : base(modelChange)
        {
            _oldValue = modelChange.OldValue;
            _newValue = modelChange.NewValue;
        }

        public string OldValue
        {
            get { return _oldValue; }
        }

        public string NewValue
        {
            get { return _newValue; }
        }
    }
}
