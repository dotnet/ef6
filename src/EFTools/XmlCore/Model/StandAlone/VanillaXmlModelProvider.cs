// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.XmlDesignerBase.Model.StandAlone
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Threading;
    using System.Xml;
    using System.Xml.Linq;

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public class VanillaXmlModelProvider : XmlModelProvider
    {
        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public const int EmptyElementOffset = 3;

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public const int ElementStartTagOffset = 2;

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public const int ProcessingInstructionOffset = 3;

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public const int CommentOffset = 5;

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public const int CDataOffset = 10;

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public const int TextOffset = 1;

        private readonly Dictionary<Uri, SimpleXmlModel> _models;
        private readonly SimpleTransactionManager _txmanager;
        private readonly CommandFactory _factory;

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public VanillaXmlModelProvider()
        {
            _models = new Dictionary<Uri, SimpleXmlModel>();
            _txmanager = new SimpleTransactionManager();
            _factory = new CommandFactory();
        }

        #region XmlModelProvider

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="source">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <returns>This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public override XmlModel GetXmlModel(Uri source)
        {
            SimpleXmlModel model;
            if (!_models.TryGetValue(source, out model))
            {
                var doc = Build(source);
                model = _models[source] = new SimpleXmlModel(source, doc);
            }

            return model;
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="source">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        public override void CloseXmlModel(Uri source)
        {
            SimpleXmlModel model = null;
            if (_models.TryGetValue(source, out model))
            {
                _models.Remove(source);
                model.Dispose();
            }
            base.CloseXmlModel(source);
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="oldName">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <param name="newName">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <returns>This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</returns>
        public override bool RenameXmlModel(Uri oldName, Uri newName)
        {
            SimpleXmlModel model = null;
            if (_models.TryGetValue(oldName, out model))
            {
                model.SetName(newName);
                _models.Remove(oldName);
                _models.Add(newName, model);
                Debug.Assert(new Uri(model.Name) == newName);
                return true;
            }
            return false;
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public override IEnumerable<XmlModel> OpenXmlModels
        {
            get
            {
                foreach (var model in _models.Values)
                {
                    yield return model;
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="name">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <param name="userState">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <returns>This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</returns>
        public override XmlTransaction BeginTransaction(string name, object userState)
        {
            return _txmanager.BeginTransaction(this, name, CurrentTransaction as SimpleTransaction, userState);
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public override XmlTransaction CurrentTransaction
        {
            get { return _txmanager.GetCurrentTransaction(this); }
        }

        #endregion

        internal void FireTransactionCompleted(SimpleTransaction tx)
        {
            OnTransactionCompleted(new XmlTransactionEventArgs(tx, tx.DesignerTransaction));
        }

        internal CommandFactory CommandFactory
        {
            get { return _factory; }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="uri">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <returns>This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</returns>
        protected virtual XDocument Build(Uri uri)
        {
            return new AnnotatedTreeBuilder().Build(uri);
        }
    }

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public class SimpleXmlModel : XmlModel
    {
        private readonly XDocument _doc;
        private Uri _uri;

        internal SimpleXmlModel(Uri uri, XDocument doc)
        {
            _uri = uri;
            _doc = doc;
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public override XDocument Document
        {
            get { return _doc; }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <returns>This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</returns>
        public override bool CanEditXmlModel()
        {
            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public override string Name
        {
            get { return _uri.AbsoluteUri; }
        }

        internal void SetName(Uri uri)
        {
            _uri = uri;
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="xobject">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <returns>This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</returns>
        public override TextSpan GetTextSpan(XObject xobject)
        {
            var el = xobject as XElement;

            var ts = new TextSpan();

            if (el != null)
            {
                var etr = el.GetTextRange();
                if (etr != null)
                {
                    // subtract 1 from these, since VS uses zero-based column & line numbers
                    ts.iStartLine = etr.OpenStartLine - 1;
                    ts.iStartIndex = etr.OpenStartColumn - 1;
                    ts.iEndLine = etr.CloseEndLine - 1;
                    // don't subtrace 1 from here, since the XML editor treats the "closing" column is the first
                    // column after the ">" bracket
                    ts.iEndIndex = etr.CloseEndColumn;
                }
            }
            else
            {
                var atr = xobject.GetTextRange();
                if (atr != null)
                {
                    // subtract 1 from these, since VS uses zero-based column & line numbers
                    ts.iStartLine = atr.OpenStartLine - 1;
                    ts.iStartIndex = atr.OpenStartColumn - 1;
                    ts.iEndLine = atr.CloseEndLine - 1;
                    // don't subtrace 1 from here, since the XML editor treats the "closing" column is the first
                    // column after the ">" bracket
                    ts.iEndIndex = atr.CloseEndColumn;
                }
                else
                {
                    if (xobject.Parent != null
                        && xobject.Parent != xobject)
                    {
                        // fallback case.  If we couldn't get a text range for this element, use the one for the parent.
                        return GetTextSpan(xobject.Parent);
                    }
                }
            }

            return ts;
        }
    }

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public class SimpleTransaction : XmlTransaction
    {
        private readonly string name;
        private XmlTransactionStatus status;
        private readonly Dictionary<XDocument, SimpleTransactionLogger> resources;
        private readonly SimpleTransactionManager manager;
        private readonly object _userState;

        internal VanillaXmlModelProvider provider;
        internal SimpleTransaction parent;

        internal SimpleTransaction(
            VanillaXmlModelProvider provider, string name, SimpleTransaction parent, SimpleTransactionManager mgr, object userState)
        {
            this.provider = provider;
            this.name = name;
            this.parent = parent;
            manager = mgr;
            DesignerTransaction = true;

            resources = new Dictionary<XDocument, SimpleTransactionLogger>();
            status = XmlTransactionStatus.Active;
            _userState = userState;
        }

        #region XmlTransaction

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="disposing">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (status == XmlTransactionStatus.Active)
            {
                Rollback();
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public override XmlModelProvider Provider
        {
            get { return provider; }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public override string Name
        {
            get { return name; }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public override XmlTransaction Parent
        {
            get { return parent; }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public override XmlTransactionStatus Status
        {
            get { return status; }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public override object UserState
        {
            get { return _userState; }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public override object UndoUserState
        {
            get { return null; }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <returns>This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</returns>
        public override IEnumerable<IXmlChange> Changes()
        {
            foreach (var model in provider.OpenXmlModels)
            {
                foreach (var change in Changes(model))
                {
                    yield return change;
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="model">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <returns>This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</returns>
        public override IEnumerable<IXmlChange> Changes(XmlModel model)
        {
            var m = model as SimpleXmlModel;
            SimpleTransactionLogger logger;
            if (resources.TryGetValue(m.Document, out logger))
            {
                foreach (var cmd in logger.TxCommands)
                {
                    yield return cmd.Change;
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public override void Commit()
        {
            if (status == XmlTransactionStatus.Committed
                || status == XmlTransactionStatus.Aborted)
            {
                throw new XmlTransactionException(Resources.VanillaProvider_TxAlreadyCompleted);
            }

            try
            {
                if (parent != null)
                {
                    // This is a Child Tx
                    parent.AppendCommands(this);
                }

                status = XmlTransactionStatus.Committed;
            }
            catch
            {
                Rollback();
                throw;
            }

            manager.Commit(this);
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public override void Rollback()
        {
            if (status == XmlTransactionStatus.Committed
                || status == XmlTransactionStatus.Aborted)
            {
                throw new XmlTransactionException(Resources.VanillaProvider_TxAlreadyCompleted);
            }

            try
            {
                // If Store is null, then it means this is a Tx created by XmlEditor
                // In that case XmlEditor will probably drop the current tree anyways
                if (provider != null)
                {
                    // Client wants to rollback, so undo changes on XLINQ tree
                    UndoTransaction();
                }
            }
            catch
            {
                // TODO: Changes cannot be rolled back now, drop the current tree and re-parse whole document
            }
            finally
            {
                status = XmlTransactionStatus.Aborted;
                manager.Rollback(this);
            }
        }

        #endregion

        internal bool DesignerTransaction { get; set; }

        internal void FireCompleted()
        {
            provider.FireTransactionCompleted(this);
        }

        internal void EnlistResource(SimpleXmlModel model)
        {
            var doc = model.Document;
            resources.Add(doc, new SimpleTransactionLogger(doc, this));
        }

        internal void MakeInactive()
        {
            // Making a Tx Inactive means it will no longer listen to events on XLINQ tree
            foreach (var logger in resources.Values)
            {
                logger.Stop();
            }
        }

        internal void MakeActive()
        {
            // Making a Tx Inactive means it will start listening to events on XLINQ tree
            status = XmlTransactionStatus.Active;
            foreach (var logger in resources.Values)
            {
                logger.Start();
            }
        }

        internal void AppendCommands(SimpleTransaction childTransaction)
        {
            foreach (var doc in childTransaction.resources.Keys)
            {
                var logger = resources[doc];
                foreach (var cmd in childTransaction.resources[doc].TxCommands)
                {
                    logger.AddCommand(cmd);
                }

                foreach (var cmd in childTransaction.resources[doc].UndoCommands)
                {
                    logger.AddCommand(cmd);
                }
            }
        }

        internal int ModelsUpdated
        {
            get
            {
                var count = 0;
                foreach (var logger in resources.Values)
                {
                    if (logger.HasChanges)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        internal void UndoTransaction()
        {
            // Undo Transaction in oppposite order
            foreach (var logger in resources.Values)
            {
                var cmds = logger.UndoCommands;
                var count = cmds.Count;
                for (var i = count - 1; i >= 0; i--)
                {
                    var modelCmd = cmds[i];
                    modelCmd.Undo();
                }
            }
        }

        internal void LockForWrite(SimpleXmlModel doc)
        {
            Lock(doc, LockMode.Write);
        }

        private void Lock(SimpleXmlModel doc, LockMode mode)
        {
            if (Status == XmlTransactionStatus.Active)
            {
                if (mode == LockMode.Write)
                {
                    manager.SimpleLockManager.LockForWrite(this, doc);
                    EnlistResource(doc);
                }
                else
                {
                    manager.SimpleLockManager.LockForRead(this, doc);
                }
            }
        }
    }

    internal class SimpleTransactionLogger
    {
        private readonly XDocument model;
        private readonly SimpleTransaction tx;
        private SimpleXmlChange currentChange;
        private EventHandler<XObjectChangeEventArgs> beforeEvent;
        private EventHandler<XObjectChangeEventArgs> afterEvent;
        private readonly List<XmlModelCommand> undoCommands;
        private readonly List<XmlModelCommand> txCommands;
        private readonly CommandFactory cmdFactory;

        private readonly Dictionary<XObject, object> nodesAdded;
        private int committedPosition = 0;

        internal SimpleTransactionLogger(XDocument m, SimpleTransaction t)
        {
            model = m;
            tx = t;
            undoCommands = new List<XmlModelCommand>();
            txCommands = new List<XmlModelCommand>();

            nodesAdded = new Dictionary<XObject, object>();
            var provider = tx.Provider as VanillaXmlModelProvider;
            if ((provider == null || provider.CommandFactory == null))
            {
                cmdFactory = new CommandFactory();
            }
            else
            {
                cmdFactory = provider.CommandFactory;
            }
        }

        private void AddNode(XObject node)
        {
            if (!nodesAdded.ContainsKey(node))
            {
                nodesAdded[node] = node;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="node">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <param name="parent">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <returns>This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</returns>
        public bool IsAncestorAdded(XObject node, XContainer parent)
        {
            if (nodesAdded.ContainsKey(node))
            {
                return true;
            }
            for (var p = parent; p != null; p = p.Parent)
            {
                if (nodesAdded.ContainsKey(p))
                {
                    return true;
                }
            }
            return false;
        }

        internal void AddCommand(XmlModelCommand cmd)
        {
            var change = cmd.Change;
            var node = change.Node;

            if (change.Action == XObjectChange.Remove)
            {
                // If we are removing a node, then all changes to any descendent of this node
                // can be discarded since the whole thing will be ripped out of the buffer anyway.
                var rs = RemoveAllDescendentChanges(change);
                if (rs == RemoveStatus.FoundSelfAdd)
                {
                    /*
                     * 9/21/2007 - not sure what to do about the stuff below.  Seems specific to the editor
                     * so commenting it out.
                     * 
                    // then this remove is a no-op, unless the Add operation is one of those
                    // special ones where the buffer already contained the value - in this case
                    // the Remove operation is real, and the add operation is a no-op, otherwise
                    // they both cancel each other out.
                    string bufferValue = XmlEditorNoPushToBuffer(change.Node);
                    if (bufferValue == null) {
                        return;
                    }
                     */
                }
            }
            if (IsAncestorAdded(node, change.Parent))
            {
                // If the ancestor was added by this transaction, then we can ignore the child
                // command because the XLink insert will insert the entire thing into the buffer
                // including all changes already made by the child transaction.
                return;
            }

            switch (change.Action)
            {
                case XObjectChange.Add:
                    AddNode(node); // record the add operation.
                    break;
                case XObjectChange.Name:
                    Merge(cmd);
                    // Name changes must be done first, because an Add operation might 
                    // cause a short end tag <A/> to be converted to <A></A>, and then
                    // a rename after that of the same node would result in two edit 
                    // operations on the same end tag which is not supported by SourceModifier.
                    undoCommands.Insert(0, cmd);
                    txCommands.Insert(0, cmd);
                    return;
                case XObjectChange.Value:
                    Merge(cmd);
                    break;
                case XObjectChange.Remove:
                    break;
            }

            txCommands.Add(cmd);
        }

        private EventHandler<XObjectChangeEventArgs> BeforeEventHandler
        {
            get
            {
                if (beforeEvent == null)
                {
                    beforeEvent = OnBeforeChange;
                }
                return beforeEvent;
            }
        }

        private EventHandler<XObjectChangeEventArgs> AfterEventHandler
        {
            get
            {
                if (afterEvent == null)
                {
                    afterEvent = OnAfterChange;
                }
                return afterEvent;
            }
        }

        internal List<XmlModelCommand> UndoCommands
        {
            get { return undoCommands; }
        }

        internal List<XmlModelCommand> TxCommands
        {
            get { return txCommands; }
        }

        internal bool HasChanges
        {
            get { return TxCommands != null ? TxCommands.Count > 0 : false; }
        }

        internal void Start()
        {
            model.Changing += BeforeEventHandler;
            model.Changed += AfterEventHandler;
        }

        internal void Stop()
        {
            model.Changing -= BeforeEventHandler;
            model.Changed -= AfterEventHandler;
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private void OnBeforeChange(object sender, XObjectChangeEventArgs e)
        {
            var node = sender as XObject;

            // We do not allow editing DTDs through XmlModel
            if (node is XDocumentType)
            {
                var msg = String.Format(CultureInfo.CurrentCulture, Resources.VanillaProvider_DtdNodesReadOnly);
                throw new NotSupportedException(msg);
            }
            var action = e.ObjectChange;
            currentChange = null;

            XElement element = null;
            XAttribute attribute = null;
            XText text = null;
            XProcessingInstruction pi = null;
            XComment comment = null;

            switch (action)
            {
                case XObjectChange.Add:
                    var addChange = new AddNodeChangeInternal(node, action);
                    currentChange = addChange;
                    break;
                case XObjectChange.Remove:
                    var removeChange = new RemoveNodeChange(node, action);
                    removeChange.Parent = node.Parent;
                    if (removeChange.Parent == null)
                    {
                        removeChange.Parent = node.Document;
                    }
                    var attrib = (node as XAttribute);
                    if (attrib != null)
                    {
                        removeChange.NextNode = attrib.NextAttribute;
                    }
                    else
                    {
                        removeChange.NextNode = (node as XNode).NextNode;
                    }
                    currentChange = removeChange;
                    break;
                case XObjectChange.Name:
                    var nameChange = new NodeNameChange(node, action);
                    if ((element = node as XElement) != null)
                    {
                        nameChange.OldName = element.Name;
                    }
                    else if ((pi = node as XProcessingInstruction) != null)
                    {
                        nameChange.OldName = XName.Get(pi.Target);
                    }
                    else
                    {
                        Debug.Assert(false, "The name of something changed that we're not handling here!");
                        throw new NotSupportedException();
                    }
                    currentChange = nameChange;
                    break;
                case XObjectChange.Value:
                    var valueChange = new NodeValueChange(node, action);
                    if ((element = node as XElement) != null)
                    {
                        valueChange.OldValue = element.Value;
                    }
                    else if ((attribute = node as XAttribute) != null)
                    {
                        valueChange.OldValue = attribute.Value;
                    }
                    else if ((text = node as XText) != null)
                    {
                        valueChange.OldValue = text.Value;
                        if (text.Parent != null)
                        {
                            valueChange.Parent = text.Parent;
                        }
                        else
                        {
                            valueChange.Parent = text.Document;
                        }
                    }
                    else if ((comment = node as XComment) != null)
                    {
                        valueChange.OldValue = comment.Value;
                    }
                    else if ((pi = node as XProcessingInstruction) != null)
                    {
                        valueChange.OldValue = pi.Data;
                    }
                    else
                    {
                        Debug.Assert(false, "The value of something changed that we're not handling here!");
                        throw new NotSupportedException();
                    }
                    currentChange = valueChange;
                    break;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private void OnAfterChange(object sender, XObjectChangeEventArgs e)
        {
            var node = sender as XObject;
            var action = e.ObjectChange;
            XmlModelCommand commandToAdd = null;

            XElement element = null;
            XAttribute attribute = null;
            XText text = null;
            XProcessingInstruction pi = null;
            XComment comment = null;

            switch (action)
            {
                case XObjectChange.Add:
                    var addChange = currentChange as AddNodeChangeInternal;
                    addChange.Parent = node.Parent;
                    if (addChange.Parent == null)
                    {
                        addChange.Parent = node.Document;
                    }
                    var attrib = (node as XAttribute);
                    if (attrib != null)
                    {
                        addChange.NextNode = attrib.NextAttribute;
                        var stableAttr = attrib.PreviousAttribute;
                        while (stableAttr != null
                               && nodesAdded.ContainsKey(stableAttr))
                        {
                            stableAttr = stableAttr.PreviousAttribute;
                        }
                        addChange.lastStableAttribute = stableAttr;
                    }
                    else
                    {
                        addChange.NextNode = (node as XNode).NextNode;
                    }

                    commandToAdd = cmdFactory.CreateAddNodeCommand((VanillaXmlModelProvider)tx.Provider, addChange);
                    break;
                case XObjectChange.Remove:
                    var removeChange = currentChange as RemoveNodeChange;
                    commandToAdd = cmdFactory.CreateRemoveNodeCommand((VanillaXmlModelProvider)tx.Provider, removeChange);
                    break;
                case XObjectChange.Name:
                    var nameChange = currentChange as NodeNameChange;
                    if ((element = node as XElement) != null)
                    {
                        nameChange.NewName = element.Name;
                    }
                    else if ((pi = node as XProcessingInstruction) != null)
                    {
                        nameChange.NewName = XName.Get(pi.Target);
                    }
                    commandToAdd = cmdFactory.CreateSetNameCommand((VanillaXmlModelProvider)tx.Provider, nameChange);
                    break;
                case XObjectChange.Value:
                    var valueChange = currentChange as NodeValueChange;
                    if ((element = node as XElement) != null)
                    {
                        valueChange.NewValue = element.Value;
                    }
                    else if ((attribute = node as XAttribute) != null)
                    {
                        valueChange.NewValue = attribute.Value;
                    }
                    else if ((text = node as XText) != null)
                    {
                        valueChange.NewValue = text.Value;
                    }
                    else if ((comment = node as XComment) != null)
                    {
                        valueChange.NewValue = comment.Value;
                    }
                    else if ((pi = node as XProcessingInstruction) != null)
                    {
                        valueChange.NewValue = pi.Data;
                    }

                    commandToAdd = cmdFactory.CreateSetValueCommand((VanillaXmlModelProvider)tx.Provider, valueChange);
                    break;
            }
            AddCommand(commandToAdd);
            undoCommands.Add(commandToAdd); // always record full undo command list.
        }

        private void Merge(XmlModelCommand newCommand)
        {
            // Some commands have to be merged (for example if the name or value of the same
            // node is changed twice) because the SourceModifier is not designed to update 
            // stuff it just inserted into the buffer during the same transaction.
            List<XmlModelCommand> toRemove = null;
            foreach (var cmd in txCommands)
            {
                if (newCommand != cmd
                    && newCommand.Merge(cmd))
                {
                    if (toRemove == null)
                    {
                        toRemove = new List<XmlModelCommand>();
                    }
                    toRemove.Add(cmd);
                }
            }
            RemoveRange(txCommands, toRemove);
            // And remove this from undo list also, since we have edited the same command objects!
            RemoveRange(undoCommands, toRemove);
        }

        private void RemoveRange(List<XmlModelCommand> list, List<XmlModelCommand> toRemove)
        {
            if (toRemove != null)
            {
                foreach (var cmd in toRemove)
                {
                    list.Remove(cmd);
                    var change = cmd.Change;
                    var key = change.Node;
                    if (change.Action == XObjectChange.Add
                        &&
                        nodesAdded.ContainsKey(key))
                    {
                        nodesAdded.Remove(key);
                    }
                }
            }
        }

        private enum RemoveStatus
        {
            None,
            FoundSelfAdd
        }

        private RemoveStatus RemoveAllDescendentChanges(SimpleXmlChange newChange)
        {
            // This node is being removed, therefore any change to any child of this
            // node is now unnecessary, so we can filter them out.
            var rc = RemoveStatus.None;
            List<XmlModelCommand> toRemove = null;
            for (int i = committedPosition, n = txCommands.Count; i < n; i++)
            {
                var cmd = txCommands[i];
                var change = cmd.Change;
                if (IsDescendentOrSelf(change, newChange))
                {
                    if (change.Node == newChange.Node
                        && change.Action == XObjectChange.Add)
                    {
                        rc = RemoveStatus.FoundSelfAdd; // we are removing an add of the same node!
                    }
                    if (toRemove == null)
                    {
                        toRemove = new List<XmlModelCommand>();
                    }
                    toRemove.Add(cmd);
                }
            }
            RemoveRange(txCommands, toRemove);
            return rc;
        }

        private static bool IsDescendentOrSelf(SimpleXmlChange existingChange, SimpleXmlChange newChange)
        {
            var child = existingChange.Node;
            var parent = newChange.Node;
            if (child == parent)
            {
                if (child.Parent == null)
                {
                    //either two deletes
                    //or a node N was added and then deleted
                    if (existingChange.Parent == newChange.Parent)
                    {
                        return true;
                    }
                }
                else if (newChange.Parent == existingChange.Parent)
                {
                    //do not return true in case of move
                    return true;
                }
            }
            var pe = parent as XElement;
            if (pe != null)
            {
                for (var e = existingChange.Parent as XElement; e != null; e = e.Parent)
                {
                    if (e == pe)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    internal class SimpleTransactionManager : IDisposable
    {
        private readonly Dictionary<VanillaXmlModelProvider, SimpleTransaction> _currentTransaction;
        private readonly SimpleLockManager _lockManager;

        internal SimpleTransactionManager()
        {
            _currentTransaction = new Dictionary<VanillaXmlModelProvider, SimpleTransaction>();
            _lockManager = new SimpleLockManager();
        }

        ~SimpleTransactionManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_lockManager != null)
                {
                    _lockManager.Dispose();
                }
            }
        }

        internal SimpleLockManager SimpleLockManager
        {
            get { return _lockManager; }
        }

        // Methods
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal SimpleTransaction BeginTransaction(
            VanillaXmlModelProvider provider, string name, SimpleTransaction parent, object userState)
        {
            var tx = new SimpleTransaction(provider, name, parent, this, userState);

            if (parent != null)
            {
                // Unsubscribe events on parent transaction
                parent.MakeInactive();

                // Create SimpleTransactionLogger for each document in VanillaXmlModelProvider
                foreach (var model in provider.OpenXmlModels)
                {
                    tx.EnlistResource(model as SimpleXmlModel);
                }
            }
            else
            {
                // This is the top level transaction, so acquire all locks to avoid deadlocks
                lock (this)
                {
                    foreach (var model in provider.OpenXmlModels)
                    {
                        tx.LockForWrite(model as SimpleXmlModel);
                    }
                }
            }

            // Make this the current Active transaction
            SetCurrentTransaction(provider, tx);
            tx.MakeActive();

            return tx;
        }

        internal void Commit(SimpleTransaction tx)
        {
            tx.MakeInactive();
            var parent = tx.parent;

            // Unlock all resources held by this transaction
            if (parent != null)
            {
                parent.MakeActive();
            }
            _currentTransaction[tx.provider] = parent;

            // Fire Events and unlock Store only when  Top-most Tx commits
            if (parent == null)
            {
                SimpleLockManager.UnlockAll(tx);
                tx.FireCompleted();
            }
        }

        internal void Rollback(SimpleTransaction tx)
        {
            tx.MakeInactive();
            var parent = tx.parent;

            // Unlock all resources held by this transaction
            if (parent != null)
            {
                parent.MakeActive();
            }
            _currentTransaction[tx.provider] = parent;

            // Unlock Store only when Top-most Tx Completes
            if (parent == null)
            {
                SimpleLockManager.UnlockAll(tx);
            }

            // In case of Rollback we fire off event even for nested Tx
            tx.FireCompleted();
        }

        internal SimpleTransaction GetCurrentTransaction(VanillaXmlModelProvider provider)
        {
            SimpleTransaction current = null;
            if (_currentTransaction.ContainsKey(provider))
            {
                current = _currentTransaction[provider];
            }

            return current;
        }

        internal void SetCurrentTransaction(VanillaXmlModelProvider provider, SimpleTransaction tx)
        {
            if (_currentTransaction.ContainsKey(provider))
            {
                _currentTransaction[provider] = tx;
            }
            else
            {
                _currentTransaction.Add(provider, tx);
            }
        }
    }

    internal enum LockMode
    {
        Null,
        Read, // Read Mode
        Write, // Write Mode
        _Length // The number of LockModes
    }

    internal class SimpleLockManager : IDisposable
    {
        // Defines types of lock requests, which can be granted while holding other locks
        private static readonly bool[][] CompatibilityTable = new bool[(int)LockMode._Length][]
            {
                // Null
                new bool[(int)LockMode._Length] { true, true, true },
                // Read
                new bool[(int)LockMode._Length] { true, true, true },
                // Write
                new bool[(int)LockMode._Length] { true, false, false }
            };

        private readonly Dictionary<Object, ResourceEntry> _resourceTable = new Dictionary<Object, ResourceEntry>();

        ~SimpleLockManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var resourceEntry in _resourceTable.Values)
                {
                    resourceEntry.Dispose();
                }
            }
        }

        private sealed class ResourceEntry : IDisposable
        {
            private readonly Dictionary<LockMode, Dictionary<SimpleTransaction, SimpleTransaction>> _transactions;
            private LockMode _lockMode;
            private AutoResetEvent _autoResetEvent;

            public ResourceEntry()
            {
                _transactions = new Dictionary<LockMode, Dictionary<SimpleTransaction, SimpleTransaction>>();
                _lockMode = LockMode.Null;
            }

            ~ResourceEntry()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (_autoResetEvent != null)
                    {
                        _autoResetEvent.Dispose();
                    }
                }
            }

            internal bool Compatible(LockMode request)
            {
                return CompatibilityTable[(int)_lockMode][(int)request];
            }

            public Dictionary<SimpleTransaction, SimpleTransaction> GetTransactionList(LockMode mode)
            {
                Dictionary<SimpleTransaction, SimpleTransaction> tList = null;
                if (_transactions.ContainsKey(mode))
                {
                    tList = _transactions[mode];
                }

                return tList;
            }

            public void Register(SimpleTransaction context, LockMode request)
            {
                var transactionList = GetTransactionList(request);
                if (transactionList == null)
                {
                    transactionList = new Dictionary<SimpleTransaction, SimpleTransaction>();
                    _transactions[request] = transactionList;
                }

                // Add the transaction to the list for _request_ lock mode
                transactionList[context] = context;

                // Update the strongest lock mode, if necessary	  
                if (request > _lockMode)
                {
                    _lockMode = request;
                }

                if (_autoResetEvent != null)
                {
                    _autoResetEvent.Reset();
                }
            }

            /// <summary>
            /// Release a lock on the passed in transaction context with
            /// the specified mode. Return immediately if the context does
            /// not hold a lock for this mode.
            /// </summary>
            /// <param name="context">the context for which to remove the lock</param>
            /// <param name="request">the mode of the lock to be released</param>
            public void Unregister(SimpleTransaction context, LockMode request)
            {
                // First get the hash table for this lock mode
                var transactionList = GetTransactionList(request);

                if (transactionList == null
                    || !transactionList.ContainsKey(context))
                {
                    // This transaction wasn't registered, return immediately
                    return;
                }

                transactionList.Remove(context);

                for (var l = request; l > LockMode.Null; _lockMode = --l)
                {
                    // recalculate the strongest lock mode
                    var nextTransactionList = GetTransactionList(l);
                    if (nextTransactionList == null)
                    {
                        continue;
                    }
                    if (nextTransactionList.Count > 0)
                    {
                        break;
                    }
                }

                if (request > _lockMode)
                {
                    // if anyone was waiting for this lock, they should recheck
                    if (_autoResetEvent != null)
                    {
                        _autoResetEvent.Set();
                    }
                }
            }

            // Define a property for UnlockEvent
            public AutoResetEvent UnlockEvent
            {
                get
                {
                    /* Avoid race condition where two threads can create 
                       two different AutoResetEvent objects for one resource */
                    lock (this)
                    {
                        if (_autoResetEvent == null)
                        {
                            _autoResetEvent = new AutoResetEvent(false);
                        }
                    }
                    return _autoResetEvent;
                }
            }

            public LockMode GetExistingLockMode(SimpleTransaction txId)
            {
                for (var i = 0; i < (int)LockMode._Length; i++)
                {
                    var tList = GetTransactionList((LockMode)i);
                    if (tList != null
                        && tList.ContainsKey(txId))
                    {
                        return (LockMode)i;
                    }
                }
                return LockMode.Null;
            }
        }

        private ResourceEntry GetResoureEntry(object src)
        {
            ResourceEntry rEntry = null;
            if (_resourceTable.ContainsKey(src))
            {
                rEntry = _resourceTable[src];
            }

            return rEntry;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public void Lock(SimpleTransaction txId, object resource, LockMode mode)
        {
            ResourceEntry lockTarget = null;

            lock (_resourceTable)
            {
                lockTarget = GetResoureEntry(resource);

                if (lockTarget == null)
                {
                    lockTarget = new ResourceEntry();
                    _resourceTable[resource] = lockTarget;
                }
            }

            for (var c = 0;; c++)
            {
                if (c > 0)
                {
                    if (!lockTarget.UnlockEvent.WaitOne(5000, false))
                    {
                        throw new XmlTransactionException("DeadLock Detected...");
                    }
                }

                lock (lockTarget)
                {
                    var oldLockMode = lockTarget.GetExistingLockMode(txId);
                    if (oldLockMode != LockMode.Null)
                    {
                        lockTarget.Unregister(txId, oldLockMode);
                    }
                    if (lockTarget.Compatible(mode))
                    {
                        lockTarget.Register(txId, mode);
                        return;
                    }
                    if (oldLockMode != LockMode.Null)
                    {
                        lockTarget.Register(txId, oldLockMode);
                    }
                }
            }
        }

        public void LockForRead(SimpleTransaction txId, object doc)
        {
            Lock(txId, doc, LockMode.Read);
        }

        public void LockForWrite(SimpleTransaction txId, object doc)
        {
            Lock(txId, doc, LockMode.Write);
        }

        public void UnlockAll(SimpleTransaction txId)
        {
            ResourceEntry lockTarget = null;
            lock (_resourceTable)
            {
                IDictionaryEnumerator resenum = _resourceTable.GetEnumerator();

                while (resenum.MoveNext())
                {
                    lockTarget = resenum.Value as ResourceEntry;
                    if (lockTarget == null)
                    {
                        continue;
                    }
                    lock (lockTarget)
                    {
                        for (var lockMode = (int)LockMode.Read; lockMode < (int)LockMode._Length; lockMode++)
                        {
                            lockTarget.Unregister(txId, (LockMode)lockMode);
                        }
                    }
                }
            }
        }
    }

    #region Changes

    internal class SimpleXmlChange : IXmlChange
    {
        private readonly XObject node;
        private readonly XObjectChange action;

        public SimpleXmlChange(XObject n, XObjectChange a)
        {
            node = n;
            action = a;
        }

        // Properties
        public XObject Node
        {
            get { return node; }
        }

        public XObjectChange Action
        {
            get { return action; }
        }

        public XContainer Parent { get; set; }
    }

    internal class AddNodeChange : SimpleXmlChange, IXmlAddNodeChange
    {
        internal XObject nextNode;

        public AddNodeChange(XObject node, XObjectChange change)
            : base(node, change)
        {
        }

        // Properties
        public XObject NextNode
        {
            get { return nextNode; }
            set { nextNode = value; }
        }
    }

    internal class RemoveNodeChange : SimpleXmlChange, IXmlRemoveNodeChange
    {
        internal XObject nextNode;

        public RemoveNodeChange(XObject node, XObjectChange change)
            : base(node, change)
        {
        }

        // Properties
        public XObject NextNode
        {
            get { return nextNode; }
            set { nextNode = value; }
        }
    }

    internal class NodeNameChange : SimpleXmlChange, IXmlNodeNameChange
    {
        internal XName oldName;
        internal XName newName;

        public NodeNameChange(XObject node, XObjectChange change)
            : base(node, change)
        {
        }

        // Properties
        public XName OldName
        {
            get { return oldName; }
            internal set { oldName = value; }
        }

        public XName NewName
        {
            get { return newName; }
            internal set { newName = value; }
        }
    }

    internal class NodeValueChange : SimpleXmlChange, IXmlNodeValueChange
    {
        internal String oldValue;
        internal String newValue;

        public NodeValueChange(XObject node, XObjectChange change)
            : base(node, change)
        {
        }

        // Properties
        public String OldValue
        {
            get { return oldValue; }
            set { oldValue = value; }
        }

        public String NewValue
        {
            get { return newValue; }
            set { newValue = value; }
        }
    }

    #endregion

    #region Commands

    internal class CommandFactory
    {
        internal virtual AddNodeCommand CreateAddNodeCommand(
            VanillaXmlModelProvider provider, AddNodeChange change)
        {
            return new AddNodeCommand(change);
        }

        internal virtual RemoveNodeCommand CreateRemoveNodeCommand(
            VanillaXmlModelProvider provider, RemoveNodeChange change)
        {
            return new RemoveNodeCommand(change);
        }

        internal virtual NodeNameCommand CreateSetNameCommand(
            VanillaXmlModelProvider provider, NodeNameChange change)
        {
            return new NodeNameCommand(change);
        }

        internal virtual NodeValueCommmand CreateSetValueCommand(
            VanillaXmlModelProvider provider, NodeValueChange change)
        {
            return new NodeValueCommmand(change);
        }
    }

    internal abstract class ModelCommand
    {
        public ModelCommand()
        {
        }

        // Methods
        public abstract void Undo();
        public abstract void Redo();

        public virtual bool CanUndo(SimpleTransaction tx)
        {
            return true;
        }

        public virtual bool CanRedo(SimpleTransaction tx)
        {
            return true;
        }

        public abstract bool Merge(ModelCommand other);
    }

    internal abstract class XmlModelCommand : ModelCommand
    {
        private readonly SimpleXmlChange _change;

        public XmlModelCommand(SimpleXmlChange change)
            : base()
        {
            this._change = change;
        }

        // Properties
        public SimpleXmlChange Change
        {
            get { return _change; }
        }
    }

    internal class AddNodeCommand : XmlModelCommand
    {
        public AddNodeCommand(AddNodeChange change)
            : base(change)
        {
        }

        public override void Undo()
        {
            var c = (AddNodeChange)Change;
            Debug.Assert(c.Parent != null);
            var node = Change.Node;
            var xn = node as XNode;
            if (xn != null)
            {
                xn.Remove();
            }
            else
            {
                var a = (XAttribute)node;
                a.Remove();
            }
        }

        public override void Redo()
        {
            var c = (AddNodeChange)Change;
            var node = Change.Node;
            var xn = node as XNode;
            if (xn != null)
            {
                var nextSibling = c.NextNode as XNode;
                if (nextSibling != null)
                {
                    nextSibling.AddBeforeSelf(xn);
                }
                else
                {
                    c.Parent.Add(xn);
                }
            }
            else
            {
                // Attributes cannot remember their relative position in XLinq!
                var a = (XAttribute)node;
                c.Parent.Add(a);
            }
        }

        public override bool Merge(ModelCommand other)
        {
            return false;
        }
    }

    internal class RemoveNodeCommand : XmlModelCommand
    {
        // Remove is simply the reverse of Add.
        private readonly AddNodeCommand _add;

        public RemoveNodeCommand(RemoveNodeChange change)
            : base(change)
        {
            var ac = new AddNodeChange(change.Node, change.Action) { NextNode = change.NextNode, Parent = change.Parent };
            _add = new AddNodeCommand(ac);
        }

        public override void Undo()
        {
            _add.Redo();
        }

        public override void Redo()
        {
            _add.Undo();
        }

        public override bool Merge(ModelCommand other)
        {
            return false;
        }
    }

    internal class NodeNameCommand : XmlModelCommand
    {
        public NodeNameCommand(NodeNameChange change)
            : base(change)
        {
        }

        public override void Undo()
        {
            var nameChange = Change as NodeNameChange;
            SetName(nameChange.Node, nameChange.OldName);
        }

        public override void Redo()
        {
            var nameChange = Change as NodeNameChange;
            SetName(nameChange.Node, nameChange.NewName);
        }

        private static void SetName(XObject node, XName name)
        {
            switch (node.NodeType)
            {
                case XmlNodeType.Element:
                    ((XElement)node).Name = name;
                    break;
                case XmlNodeType.Attribute:
                    Debug.Assert(false, "This should never happen because attribute names cannot be changed");
                    break;
                case XmlNodeType.ProcessingInstruction:
                    ((XProcessingInstruction)node).Target = name.LocalName;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public override bool Merge(ModelCommand other)
        {
            var s = other as NodeNameCommand;
            if (s != null
                && s != this
                && s.Change.Node == Change.Node)
            {
                ((NodeNameChange)Change).OldName = ((NodeNameChange)s.Change).OldName;
                return true;
            }
            return false;
        }
    }

    internal class NodeValueCommmand : XmlModelCommand
    {
        public NodeValueCommmand(NodeValueChange change)
            : base(change)
        {
        }

        public override void Undo()
        {
            var valueChange = Change as NodeValueChange;
            SetValue(valueChange.Node, valueChange.OldValue);
        }

        public override void Redo()
        {
            var valueChange = Change as NodeValueChange;
            SetValue(valueChange.Node, valueChange.NewValue);
        }

        private static void SetValue(XObject node, string value)
        {
            switch (node.NodeType)
            {
                case XmlNodeType.Element:
                    ((XElement)node).Value = value;
                    break;
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                    ((XText)node).Value = value;
                    break;
                case XmlNodeType.ProcessingInstruction:
                    ((XProcessingInstruction)node).Data = value;
                    break;
                case XmlNodeType.Comment:
                    ((XComment)node).Value = value;
                    break;
                case XmlNodeType.Attribute:
                    ((XAttribute)node).Value = value;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public override bool Merge(ModelCommand other)
        {
            var s = other as NodeValueCommmand;
            if (s != null
                && s != this
                && s.Change.Node == Change.Node)
            {
                ((NodeValueChange)Change).OldValue = ((NodeValueChange)s.Change).OldValue;
                return true;
            }
            return false;
        }
    }

    //[CLSCompliant(false)]
    internal class AddNodeChangeInternal : AddNodeChange, IComparable
    {
        internal XNode CompareToObject;
        internal bool endTag = false;
        //this is used on a change adding attributes. When multiple attributes are added in a single tx
        //this field should point to the last attribute that was present before the tx started
        internal XAttribute lastStableAttribute;

        public AddNodeChangeInternal(XObject n, XObjectChange action)
            : base(n, action)
        {
            CompareToObject = n as XNode;
        }

        public int CompareTo(object otherObj)
        {
            var value = -1;
            var other = otherObj as AddNodeChangeInternal;
            if (other != null
                && other.GetType() == GetType())
            {
                if (CompareToObject == null)
                {
                    return (other.CompareToObject == null ? 0 : 1);
                }
                else if (other.CompareToObject == null)
                {
                    return -1;
                }
                else
                {
                    if (endTag && IsChildOf(CompareToObject, other.CompareToObject))
                    {
                        return 1; // then this end tag is after the other node.                       
                    }
                    if (other.endTag
                        && IsChildOf(other.CompareToObject, CompareToObject))
                    {
                        return -1; // then this node is before the other end tag .                       
                    }

                    // 0 if the nodes are equal; -1 if n1 is before n2; 1 if n1 is after n2.
                    return XNode.CompareDocumentOrder(CompareToObject, other.CompareToObject);
                }
            }

            return value;
        }

        internal static bool IsChildOf(XNode node, XNode child)
        {
            var e = child.Parent;
            while (e != null)
            {
                if (e == node)
                {
                    return true;
                }
                e = e.Parent;
            }
            return false;
        }
    }

    #endregion
}
