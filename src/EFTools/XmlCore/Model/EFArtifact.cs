// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.Model.Validation;
    using Microsoft.Data.Entity.Design.Model.Visitor;
    using Microsoft.Data.Entity.Design.Model.XLinqAnnotations;
    using Microsoft.Data.Tools.XmlDesignerBase.Model;
    using Microsoft.Data.Entity.Design.Common;

    internal abstract class EFArtifact : EFContainer
    {
        private Uri _uri;
        private readonly ModelManager _modelManager;
        private readonly XmlModelProvider _xmlModelProvider;
        private ErrorClass _isValidityDirtyErrorClassMask = 0;
        protected object _extendedObject;

        /// <summary>
        ///     Flag indicating if the document should be opened in the designer.  A document is "designer-safe" if it doesn't have a certain
        ///     class of errors when loading.
        /// </summary>
        private bool? _isDesignerSafe;

        /// <summary>
        ///     Flag indicating if the document requires reloading.  This should be used by views to determine if a document needs reloaded.
        /// </summary>
        private bool _requiresReloading;

        private Dictionary<EFObject, ICollection<ErrorInfo>> _errors;

#if DEBUG
        // any elements xml names that have been parsed into an EFElement. 
        // This is only used in DEBUG builds to catch elements that have been added to the EDM file formats that our parser doesn't handle. 
        internal readonly HashSet<XName> UnprocessedElements = new HashSet<XName>();
#endif

        internal object ExtendedObject
        {
            get { return _extendedObject; }
        }

        /// <summary>
        ///     True if this artifact was constructed purely for code generation purposes. This means that if this artifact is cached inside the ModelManager
        ///     it needs to be disposed before the designer opens the artifact.
        /// </summary>
        internal bool IsCodeGenArtifact { get; set; }

        /// <summary>
        ///     Constructs an EFArtifact for the passed in URI.
        ///     Note: this class will call Dispose() on the provided (or created) XmlModelProvider when it Dispose(true) is called
        /// </summary>
        /// <param name="modelManager">A reference of ModelManager</param>
        /// <param name="uri">The URI to the EDMX file that this artifact will load</param>
        /// <param name="xmlModelProvider">If you pass null, then you must derive from this class and implement CreateModelProvider().</param>
        internal EFArtifact(ModelManager modelManager, Uri uri, XmlModelProvider xmlModelProvider)
            : base(null, null)
        {
            Debug.Assert(modelManager != null, "You need to pass in a valid ModelManager reference");
            Debug.Assert(uri != null, "You need to pass in a valid URI");
            Debug.Assert(xmlModelProvider != null, "xmlModelProvider != null");

            _modelManager = modelManager;
            _uri = uri;
            _xmlModelProvider = xmlModelProvider;
        }

        internal override string ToPrettyString()
        {
            return _uri.AbsolutePath;
        }

        internal virtual void Init()
        {
            Debug.Assert(_xmlModelProvider != null, "An XmlModelProvider must be set prior to calling Init()");

            _xmlModelProvider.TransactionCompleted += HandleXmlModelTransactionCompleted;
            _xmlModelProvider.UndoRedoCompleted += HandleXmlModelUndoRedoCompleted;

            var xmlModel = _xmlModelProvider.GetXmlModel(_uri);
            Debug.Assert(xmlModel != null);

            SetXObject(xmlModel.Document);

            SetValidityDirtyForErrorClass(ErrorClass.All, true);
        }

        internal bool IsDirty { get; set; }

        internal ModelManager ModelManager
        {
            get { return _modelManager; }
        }

        // virtual to allow mocking
        internal virtual EFArtifactSet ArtifactSet
        {
            get { return ModelManager.GetArtifactSet(Uri); }
        }

        /// <summary>
        ///     If artifact is being reloaded
        /// </summary>
        public bool IsArtifactReloading { get; protected set; }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    _xmlModelProvider.TransactionCompleted -= HandleXmlModelTransactionCompleted;
                    _xmlModelProvider.UndoRedoCompleted -= HandleXmlModelUndoRedoCompleted;
                    _xmlModelProvider.CloseXmlModel(_uri);
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        ///     Sets a flag indicating that this artifact should be reloaded before it is used.
        /// </summary>
        internal bool RequireDelayedReload
        {
            get { return _requiresReloading; }
            set { _requiresReloading = value; }
        }

        internal void RenameArtifact(Uri newUri)
        {
            // Model Manager should have this artifact tied to newUri _already_ in its hashmap
            Debug.Assert(
                this == ModelManager.GetArtifact(newUri),
                "Model Manager should update its artifactsByUri map before calling Artifact.RenameArtifact");
            if (this == ModelManager.GetArtifact(newUri))
            {
                _xmlModelProvider.RenameXmlModel(_uri, newUri);
                _uri = newUri;
            }
        }

        /// <summary>
        ///     This method will reload the artifact from the XLinq tree. This *DOES NOT* do a 'deep reloading', i.e. reloading from disk.
        /// </summary>
        internal virtual void ReloadArtifact()
        {
            try
            {
                IsArtifactReloading = true;

                // clear out the artifact set of our information
                ArtifactSet.RemoveArtifact(this);
                ArtifactSet.Add(this);

                // reparse this artifact
                State = EFElementState.None;
                Parse(new List<XName>());
                if (State == EFElementState.Parsed)
                {
                    XmlModelHelper.NormalizeAndResolve(this);
                }

                // this will do some analysis to determine if the artifact is safe for the designer, or should be displayed in the xml editor
                DetermineIfArtifactIsDesignerSafe();

                FireArtifactReloadedEvent();

                _requiresReloading = false;
                IsDirty = false;
            }
            finally
            {
                IsArtifactReloading = false;
            }
        }

        // this is overloaded in VSArtifact
        internal virtual void FireArtifactReloadedEvent()
        {
        }

        internal override void Normalize()
        {
            State = EFElementState.Normalized;
        }

        internal override void Resolve(EFArtifactSet artifactSet)
        {
            State = EFElementState.Resolved;
        }

        internal virtual void OnLoaded()
        {
        }

        internal override Uri Uri
        {
            get { return _uri; }
        }

        internal XmlModelProvider XmlModelProvider
        {
            get { return _xmlModelProvider; }
        }

        // virtual to allow mocking
        internal virtual XDocument XDocument
        {
            get { return XObject as XDocument; }
        }

        internal override string SemanticName
        {
            get { return Uri.ToString(); }
        }

        /// <summary>
        ///     This is an intentional no-op since we won't have the model loaded until we
        ///     are into the body of the c'tor.
        /// </summary>
        /// <returns></returns>
        protected override void AddToXlinq()
        {
        }

        protected override void RemoveFromXlinq()
        {
            throw new InvalidOperationException();
        }

        internal EfiTransaction CurrentEfiTransaction
        {
            get
            {
                var xmlTx = XmlModelProvider.CurrentTransaction;
                return xmlTx != null ? xmlTx.UserState as EfiTransaction : null;
            }
        }

        private void HandleXmlModelTransactionCompleted(object sender, XmlTransactionEventArgs xmlTransactionEventArgs)
        {
            OnBeforeHandleXmlModelTransactionCompleted(sender, xmlTransactionEventArgs);

            EfiChangeGroup changeGroup;
            OnHandleXmlModelTransactionCompleted(
                sender, xmlTransactionEventArgs,
                false, // send 'false' since this is the normal handler
                out changeGroup);

            OnAfterHandleXmlModelTransactionCompleted(sender, xmlTransactionEventArgs, changeGroup);
        }

        private void HandleXmlModelUndoRedoCompleted(object sender, XmlTransactionEventArgs xmlTransactionEventArgs)
        {
            OnBeforeHandleXmlModelTransactionCompleted(sender, xmlTransactionEventArgs);

            EfiChangeGroup changeGroup;
            OnHandleXmlModelTransactionCompleted(
                sender, xmlTransactionEventArgs,
                true, // send 'true' since this is the undo handler
                out changeGroup);

            OnAfterHandleXmlModelTransactionCompleted(sender, xmlTransactionEventArgs, changeGroup);
        }

        protected virtual void OnBeforeHandleXmlModelTransactionCompleted(object sender, XmlTransactionEventArgs xmlTransactionEventArgs)
        {
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected virtual void OnHandleXmlModelTransactionCompleted(
            object sender, XmlTransactionEventArgs xmlTransactionEventArgs, bool isUndoOrRedo, out EfiChangeGroup changeGroup)
        {
            // If this is an undo/redo XML transaction there is no EfiTransaction, thus the artifact will not
            // be made dirty as necessary. We will have to do it manually here.
            var efiTransaction = xmlTransactionEventArgs.Transaction.UserState as EfiTransaction;
            if (efiTransaction == null && isUndoOrRedo)
            {
                Artifact.IsDirty = true;
            }

            // When an XML transaction completes it could either be a normal, undo, or redo transaction.
            // In all cases we will need to clear the "validity" of the artifact so that any successive
            // validations will not short-circuit.
            // Ideally we can skip this in the event of any major error that causes the reloading
            // of the artifact but we'll be safe.
            SetValidityDirtyForErrorClass(ErrorClass.All, true);

            // the change group to send back to the caller
            changeGroup = null;

            // if the transaction is aborting, drop and reload
            if (xmlTransactionEventArgs.Transaction.Status == XmlTransactionStatus.Aborted)
            {
                ReloadArtifact();
                return;
            }

            if (efiTransaction != null)
            {
                changeGroup = ProcessDesignerChange(xmlTransactionEventArgs, efiTransaction);
                if (changeGroup != null)
                {
                    ModelManager.RecordChangeGroup(changeGroup);
                }
            }
            else
            {
                // TODO: when we want SxS again, we should handle these operations in addition to undo/redo
                if (isUndoOrRedo)
                {
                    try
                    {
                        changeGroup = ProcessUndoRedoChanges(xmlTransactionEventArgs);
                        if (changeGroup != null)
                        {
                            ModelManager.RecordChangeGroup(changeGroup);

                            // we have to manually route the change groups here because we can't rely on ProcessUndoRedoChange to do it since nothing
                            // gets updated in the Xml Model
                            ModelManager.RouteChangeGroups();
                        }
                    }
                    catch (ChangeProcessingFailedException)
                    {
                        ReloadArtifact();
                    }
                    catch (Exception e)
                    {
                        Debug.Fail("Unexpected exception caught while processing undo/redo", e.Message);
                        ReloadArtifact();
                    }
                }
            }
        }

        protected virtual void OnAfterHandleXmlModelTransactionCompleted(
            object sender, XmlTransactionEventArgs xmlTransactionEventArgs, EfiChangeGroup changeGroup)
        {
        }

        internal class ExternalXMLModelChange
        {
            private readonly IXmlChange _xmlChange;
            private readonly EFObject _changedEFObject;
            private EFObject _parentOfChangedEFObject;
            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
            private readonly ExpectEFObjectParentForXObject _expectEFObjectParentForXObject;

            public delegate bool ExpectEFObjectParentForXObject(XObject xobject);

            public ExternalXMLModelChange(IXmlChange xmlChange, ExpectEFObjectParentForXObject assertDelegate)
            {
                _xmlChange = xmlChange;
                _changedEFObject = ModelItemAnnotation.GetModelItem(_xmlChange.Node);
                _expectEFObjectParentForXObject = assertDelegate;
                ResolveParentEFObject();
            }

            public IXmlChange XmlChange
            {
                get { return _xmlChange; }
            }

            public XObjectChange Action
            {
                get { return _xmlChange.Action; }
            }

            public XObject XNode
            {
                get { return _xmlChange.Node; }
            }

            public EFObject ChangedEFObject
            {
                get { return _changedEFObject; }
            }

            public EFObject Parent
            {
                get { return _parentOfChangedEFObject; }
            }

            internal bool IsAnnotationChange(HashSet<string> nonAnnotationNamespaces)
            {
                // extract the node's namespace (if any)
                var ns = string.Empty;
                if (XNode.NodeType == XmlNodeType.Element)
                {
                    ns = ((XElement)XNode).Name.Namespace.NamespaceName;
                }
                else if (XNode.NodeType == XmlNodeType.Attribute)
                {
                    ns = ((XAttribute)XNode).Name.Namespace.NamespaceName;
                }

                // if the node being edited is not in the list, move on to the next one
                // this is an undo/redo of a annotation from an extension
                if (string.IsNullOrEmpty(ns) == false
                    && nonAnnotationNamespaces.Count > 0
                    && nonAnnotationNamespaces.Contains(ns) == false
                    && ChangedEFObject == Parent)
                {
                    return true;
                }
                return false;
            }

            private void ResolveParentEFObject()
            {
                var parent = _xmlChange.Node.Parent;

                if (parent != null)
                {
                    // get the parent EFObject for the changed object
                    _parentOfChangedEFObject = ModelItemAnnotation.GetModelItem(parent);
                }

                // in case of a 'delete' change, then the changed XObject's parent will be null so we should try to
                // resolve the parent through our model tree.                       
                if (_parentOfChangedEFObject == null)
                {
                    // try another way to get the parent
                    if (ChangedEFObject != null)
                    {
                        _parentOfChangedEFObject = ChangedEFObject.Parent;
                    }
                }

#if DEBUG
                if (_parentOfChangedEFObject == null)
                {
                    if (_expectEFObjectParentForXObject != null
                        && _expectEFObjectParentForXObject(_xmlChange.Node))
                    {
                        Debug.Fail("Why weren't we able to find the parent of the changed EFObject?");
                    }
                }
#endif
            }
        }

        /// <summary>
        ///     Returns the list of namespaces that the designer "cares" about
        ///     Any undo/redo changes for elements or attributes not in this list are ignored
        /// </summary>
        /// <returns>A set of namespace strings converted to lower case, or an empty list to process all items</returns>
        protected internal virtual HashSet<string> GetNamespaces()
        {
            return new HashSet<string>();
        }

        private EfiChangeGroup ProcessUndoRedoChanges(XmlTransactionEventArgs xmlTransactionEventArgs)
        {
            using (var tx = new EfiTransaction(this, EfiTransactionOriginator.UndoRedoOriginatorId, xmlTransactionEventArgs.Transaction))
            {
                var changeGroup = tx.ChangeGroup;
                var undoChanges = new List<ExternalXMLModelChange>();
                var containersToRenormalize = new List<EFContainer>();
                var bindingsForRebind = new List<ItemBinding>();
                var namespaces = GetNamespaces();

                // filter the changes received from XmlEditor and resolve changed EFObjects and their parents
                foreach (var xmlChange in xmlTransactionEventArgs.Transaction.Changes())
                {
                    // determine how to process this edit
                    if (xmlChange.Node.NodeType == XmlNodeType.Element
                        || xmlChange.Node.NodeType == XmlNodeType.Attribute
                        || xmlChange.Node.NodeType == XmlNodeType.Document)
                    {
                        if (xmlChange.Node.NodeType == XmlNodeType.Element
                            && xmlChange.Action == XObjectChange.Value)
                        {
                            var nodeValueChange = xmlChange as IXmlNodeValueChange;
                            Debug.Assert(
                                nodeValueChange != null
                                && string.IsNullOrEmpty(nodeValueChange.OldValue)
                                && string.IsNullOrEmpty(nodeValueChange.NewValue));

                            // in cases where we are undoing the addition of all children to an element A, the Xml Model
                            // will forcibly add an empty string to the element A so that a "short" end tag "/>" is not created
                            // in this case, the change we will receive is a set value change on an element; we ignore this
                        }
                        else
                        {
                            var emc = new ExternalXMLModelChange(xmlChange, ExpectEFObjectForXObject);
                            if (emc.IsAnnotationChange(namespaces))
                            {
                                continue;
                            }

                            undoChanges.Add(emc);
                        }
                    }
                }

                // go through the undo changes and make changes to the model
                foreach (var undoModelChange in undoChanges)
                {
                    // Should ignore other artifact changes.
                    if (undoModelChange.ChangedEFObject.Artifact != this)
                    {
                        continue;
                    }

                    if (undoModelChange.Action == XObjectChange.Remove)
                    {
                        ProcessExternalRemoveChange(changeGroup, bindingsForRebind, undoModelChange);
                    }
                    else
                    {
                        ProcessExternalAddOrUpdateChange(changeGroup, containersToRenormalize, undoModelChange);
                    }
                }

                // because of the order in which elements were possibly added, certain ItemBindings were not resolved;
                // therefore we have to step through the normalized EFElements again and normalize/resolve so their child
                // ItemBindings will get resolved.
                foreach (var container in containersToRenormalize)
                {
                    XmlModelHelper.NormalizeAndResolve(container);
                }
                XmlModelHelper.RebindItemBindings(bindingsForRebind);

#if DEBUG
                var visitor = GetVerifyModelIntegrityVisitor();
                visitor.Traverse(this);

                if (visitor.ErrorCount > 0)
                {
                    Debug.WriteLine("Model Integrity Verifier found " + visitor.ErrorCount + " error(s):");
                    Debug.WriteLine(visitor.AllSerializedErrors);
                    Debug.Assert(
                        false, "Model Integrity Verifier found " + visitor.ErrorCount + " error(s). See the Debug console for details.");
                }
#endif
                return changeGroup;
            }
        }

#if DEBUG
        protected virtual VerifyModelIntegrityVisitor GetVerifyModelIntegrityVisitor()
        {
            return null;
        }

        internal virtual VerifyModelIntegrityVisitor GetVerifyModelIntegrityVisitor(
            bool checkDisposed, bool checkUnresolved, bool checkXObject, bool checkAnnotations, bool checkBindingIntegrity)
        {
            return null;
        }
#endif

        /// <summary>
        ///     use this method to prevent asserts from firing during undo/redo when we're unable to locate an EFObject for certain XObjects that have no
        ///     corresponding model element.
        /// </summary>
        /// <param name="xobject"></param>
        protected internal virtual bool ExpectEFObjectForXObject(XObject xobject)
        {
            return true;
        }

        private static void ProcessExternalRemoveChange(
            EfiChangeGroup changeGroup, IList<ItemBinding> bindingsForRebind, ExternalXMLModelChange modelChange)
        {
            var changedEFObject = modelChange.ChangedEFObject;
            var parentEFObject = modelChange.Parent;
            if (changedEFObject != null)
            {
                var staleItemBinding = changedEFObject as ItemBinding;
                var staleDefaultableValue = changedEFObject as DefaultableValue;

                var parentEFContainer = parentEFObject as EFContainer;
                Debug.Assert(parentEFContainer != null, "parentEfObject was not an EFContainer!");
                if (staleItemBinding != null)
                {
                    // if this is an itembinding, then we have to directly null out the xobject and rebind since the refname is a "symlink" to the xattribute
                    foreach (var child in parentEFContainer.Children)
                    {
                        var updatedItemBinding = child as ItemBinding;
                        if (updatedItemBinding != null
                            && updatedItemBinding.EFTypeName == staleItemBinding.EFTypeName)
                        {
                            updatedItemBinding.SetXObject(null);
                            updatedItemBinding.Rebind();
                            changedEFObject = updatedItemBinding;
                            break;
                        }
                    }
                }
                else if (staleDefaultableValue != null)
                {
                    foreach (var child in parentEFContainer.Children)
                    {
                        var updatedDefaultableValue = child as DefaultableValue;
                        if (updatedDefaultableValue != null
                            && updatedDefaultableValue.EFTypeName == staleDefaultableValue.EFTypeName)
                        {
                            updatedDefaultableValue.SetXObject(null);
                            changedEFObject = updatedDefaultableValue;
                            break;
                        }
                    }
                }
                else
                {
                    // Find all the dependent binding.
                    var visitor = new AntiDependencyCollectorVisitor();
                    visitor.Traverse(changedEFObject);
                    foreach (var binding in visitor.AntiDependencyBindings)
                    {
                        if (!bindingsForRebind.Contains(binding))
                        {
                            bindingsForRebind.Add(binding);
                        }
                    }
                    // Delete(false) because it has already been removed from XLinq tree
                    changedEFObject.Delete(false);
                }

                // record the change so views get updated
                changeGroup.RecordModelChange(EfiChange.EfiChangeType.Delete, changedEFObject, String.Empty, String.Empty, String.Empty);
            }
            else
            {
                throw new ChangeProcessingFailedException();
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void ProcessExternalAddOrUpdateChange(
            EfiChangeGroup changeGroup, List<EFContainer> containersToRenormalize, ExternalXMLModelChange undoModelChange)
        {
            var changedEFObject = undoModelChange.ChangedEFObject;
            var parentEFObject = undoModelChange.Parent;

            if (parentEFObject != null)
            {
                var parentEFContainer = parentEFObject as EFContainer;
                Debug.Assert(parentEFContainer != null, "parentEFObject was not an EFContainer!");

                // if this is an "add" for an element, then the parent will parse the new child.
                // if this is an "add" for an attribute, then we have to directly set the XObject
                // if this is an "update" then we will just record it in our model
                if (parentEFContainer != null
                    &&
                    undoModelChange.Action == XObjectChange.Add)
                {
                    var newXElement = undoModelChange.XNode as XElement;

                    if (newXElement != null
                        &&
                        parentEFContainer.ReparseSingleElement(new List<XName>(), newXElement))
                    {
                        var newEFObject = ModelItemAnnotation.GetModelItem(undoModelChange.XNode);
                        Debug.Assert(newEFObject != null, "Couldn't find the ModelItemAnnotation for the newly created XLinq node");
                        if (newEFObject != null)
                        {
                            var newEFElement = newEFObject as EFElement;
                            Debug.Assert(newEFElement != null, "If the XObject was an XElement, then we should have an EFElement as well");
                            if (newEFElement != null)
                            {
                                XmlModelHelper.NormalizeAndResolve(newEFElement);
                            }

                            // we need to rediscover the XLinq annotation to pick up the new EFObject that just got created
                            changedEFObject = ModelItemAnnotation.GetModelItem(undoModelChange.XNode);
                        }
                    }
                    else if (undoModelChange.XNode is XAttribute)
                    {
                        var staleItemBinding = changedEFObject as ItemBinding;
                        var staleDefaultableValue = changedEFObject as DefaultableValue;

                        // item bindings might have gotten added after their parents got added in which case the MIA will be stale.
                        // we have to step through the new parent, set the xobject manually, rebind, and reset the annotation on the xlinq node.
                        if (staleItemBinding != null)
                        {
                            foreach (var child in parentEFContainer.Children)
                            {
                                var updatedItemBinding = child as ItemBinding;
                                if (updatedItemBinding != null
                                    && updatedItemBinding.EFTypeName == staleItemBinding.EFTypeName)
                                {
                                    updatedItemBinding.SetXObject(undoModelChange.XNode);
                                    updatedItemBinding.Rebind();
                                    changedEFObject = updatedItemBinding;
                                }
                            }
                        }

                            // for defaultable values that got added after parents were added, we have to discover the actual EFObject in the Escher tree,
                            // set the xobject, and reset the annotation on the existing xlinq node
                        else if (staleDefaultableValue != null)
                        {
                            foreach (var child in parentEFContainer.Children)
                            {
                                var updatedDefaultableValue = child as DefaultableValue;
                                if (updatedDefaultableValue != null
                                    && updatedDefaultableValue.EFTypeName == staleDefaultableValue.EFTypeName)
                                {
                                    updatedDefaultableValue.SetXObject(undoModelChange.XNode);
                                    changedEFObject = updatedDefaultableValue;
                                }
                            }
                        }

                        ModelItemAnnotation.SetModelItem(undoModelChange.XNode, changedEFObject);
                    }
                }
                else if (undoModelChange.Action == XObjectChange.Value)
                {
                    Debug.Assert(undoModelChange.XNode is XAttribute, "The only 'value' change Escher supports is to XAttributes");
                    if (undoModelChange.XNode is XAttribute)
                    {
                        var existingItemBinding = changedEFObject as ItemBinding;
                        if (existingItemBinding != null)
                        {
                            existingItemBinding.Rebind();
                        }

                        // we have to normalize and resolve the parents of DefaultableValues
                        // because this change could affect the parent's RefName, affecting SingleItemBindings
                        var defaultableValue = changedEFObject as DefaultableValue;
                        if (defaultableValue != null)
                        {
                            XmlModelHelper.NormalizeAndResolve(parentEFContainer);
#if DEBUG
                            var xattr = undoModelChange.XNode as XAttribute;
                            var defaultableValueValue = defaultableValue.ObjectValue as string;
                            if (defaultableValueValue != null)
                            {
                                // verify that the defaultableValue's value & the XAttribute's value are the same
                                Debug.Assert(xattr.Value == defaultableValueValue);
                            }
#endif
                        }
                    }
                }
                else
                {
                    Debug.Assert(false, "Encountered a non-valid type of change to the XML: " + undoModelChange.Action.ToString());
                    throw new ChangeProcessingFailedException();
                }

                // if an object's state is unresolved, then queue it for re-normalization. This occurs
                // for example, if itembinding changes occur before the 'add' changes for their targeted objects
                var itemBinding = changedEFObject as ItemBinding;
                if ((itemBinding != null && false == itemBinding.Resolved)
                    || parentEFContainer.State != EFElementState.Resolved)
                {
                    containersToRenormalize.Add(parentEFContainer);
                }
                CheckObjectToRenormalize(changedEFObject, ref containersToRenormalize);

                // now tell the views that a new item has been created; this will happen for both an 'add' and a 'change'
                string oldValue = null;
                string newValue = null;
                string property = null;
                GetOldAndNewValues(undoModelChange.XmlChange, out property, out oldValue, out newValue);

                changeGroup.RecordModelChange(GetChangeType(undoModelChange.XmlChange), changedEFObject, property, oldValue, newValue);
            }
            else
            {
                // we got a change event on a root-node in the document.  
                // This will cause an NRE when looking for the model-item annotation,
                // so throw this exception to cause the caller to reload the doc
                throw new ChangeProcessingFailedException();
            }
        }

        private void CheckObjectToRenormalize(EFObject efObject, ref List<EFContainer> containersToRenormalize)
        {
            var efContainer = efObject as EFContainer;
            if (efContainer != null)
            {
                if (efContainer.State != EFElementState.Resolved)
                {
                    containersToRenormalize.Add(efContainer);
                }

                foreach (var child in efContainer.Children)
                {
                    CheckObjectToRenormalize(child, ref containersToRenormalize);
                }
            }
        }

        private static EfiChangeGroup ProcessDesignerChange(XmlTransactionEventArgs xmlTransactionEventArgs, EfiTransaction efiTransaction)
        {
            // if the transaction has one of our transactions in the UserState, then 
            // this will not be null
            var changeGroup = efiTransaction.ChangeGroup;

            // Here, we need to call RecordModelChange for each change that occurred in the XmlTransaction
            // Since the XmlTransaction started in our model, the model already reflects these changes.
            foreach (var xmlChange in xmlTransactionEventArgs.Transaction.Changes())
            {
                Debug.Assert(xmlChange.Node != null);

                if (xmlChange.Node.NodeType == XmlNodeType.Element
                    || xmlChange.Node.NodeType == XmlNodeType.Attribute
                    || xmlChange.Node.NodeType == XmlNodeType.Document)
                {
                    var efObject = ModelItemAnnotation.GetModelItem(xmlChange.Node);
                    if (efObject != null)
                    {
                        if (xmlChange.Node.NodeType == XmlNodeType.Element
                            && xmlChange.Action == XObjectChange.Value)
                        {
                            var nodeValueChange = xmlChange as IXmlNodeValueChange;
                            Debug.Assert(
                                nodeValueChange != null
                                && string.IsNullOrEmpty(nodeValueChange.OldValue)
                                && string.IsNullOrEmpty(nodeValueChange.NewValue));

                            // at times (like when the last child is removed), the XLinq tree will 
                            // collapse whitespace nodes and we will get 'set value' on the 
                            // preceeding element, we can ignore these
                            continue;
                        }

                        string oldValue = null;
                        string newValue = null;
                        string property = null;
                        GetOldAndNewValues(xmlChange, out property, out oldValue, out newValue);

                        changeGroup.RecordModelChange(GetChangeType(xmlChange), efObject, property, oldValue, newValue);
                    }
                }
            }

            return changeGroup;
        }

        private static EfiChange.EfiChangeType GetChangeType(IXmlChange xmlChange)
        {
            switch (xmlChange.Action)
            {
                case XObjectChange.Add:
                    return EfiChange.EfiChangeType.Create;
                case XObjectChange.Remove:
                    return EfiChange.EfiChangeType.Delete;
                case XObjectChange.Name:
                case XObjectChange.Value:
                    return EfiChange.EfiChangeType.Update;
                default:
                    Debug.Assert(false, "Unexpected type of XObjectChange");
                    return EfiChange.EfiChangeType.Update;
            }
        }

        private static void GetOldAndNewValues(IXmlChange xmlChange, out string property, out string oldValue, out string newValue)
        {
            if (xmlChange.Action == XObjectChange.Name)
            {
                var nodeNameChange = xmlChange as IXmlNodeNameChange;
                newValue = nodeNameChange.NewName.LocalName;
                oldValue = nodeNameChange.OldName.LocalName;
                property = string.Empty;
            }
            else if (xmlChange.Action == XObjectChange.Value)
            {
                var nodeValueChange = xmlChange as IXmlNodeValueChange;
                var xattr = nodeValueChange.Node as XAttribute;
                Debug.Assert(xattr != null);
                property = xattr.Name.LocalName;
                newValue = nodeValueChange.NewValue;
                oldValue = nodeValueChange.OldValue;
            }
            else
            {
                newValue = String.Empty;
                oldValue = String.Empty;
                property = String.Empty;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors",
            Justification = "Minimizing changes, bugs tracking to refactor these")]
        [SuppressMessage("Microsoft.Design", "CA1064:ExceptionsShouldBePublic")]
        [Serializable]
        internal class ChangeProcessingFailedException : Exception
        {
        }

        /// <summary>
        ///     Returns the XObject for the given line & column number.
        /// </summary>
        internal XObject FindXObjectForLineAndColumn(int lineNumber, int columnNumber)
        {
            var xmlModel = XmlModelProvider.GetXmlModel(Uri);
            return xmlModel.GetXObject(lineNumber, columnNumber);
        }

        // virtual to allow mocking
        internal virtual EFObject FindEFObjectForLineAndColumn(int lineNumber, int columnNumber)
        {
            var o = FindXObjectForLineAndColumn(lineNumber, columnNumber);

            Debug.Assert(o != null || (lineNumber == 0 && columnNumber == 0), "Unexpected null value for XObject");

            if (o != null)
            {
                var mia = ModelItemAnnotation.GetModelItem(o);
                while (mia == null
                       && o != null)
                {
                    // in some cases, we have an XObject without a corresponding EFObject.
                    // here we find the first enclosing EFObject.
                    o = o.Parent;
                    if (o != null)
                    {
                        mia = ModelItemAnnotation.GetModelItem(o);
                    }
                }

                if (mia != null)
                {
                    return mia;
                }
            }

            return this;
        }

        /// <summary>
        ///     This method checks to see if *any* of the ErrorClasses in the ErrorClass
        ///     mask have a dirty validity
        /// </summary>
        internal bool IsValidityDirtyForErrorClass(ErrorClass errorClassMask)
        {
            return (_isValidityDirtyErrorClassMask & errorClassMask) != 0;
        }

        internal void SetValidityDirtyForErrorClass(ErrorClass errorClass, bool isDirty)
        {
            if (isDirty)
            {
                _isValidityDirtyErrorClassMask |= errorClass;
            }
            else
            {
                _isValidityDirtyErrorClassMask &= ~errorClass;
            }
        }

        protected internal bool IsDesignerSafe
        {
            get
            {
                if (_isDesignerSafe == null)
                {
                    DetermineIfArtifactIsDesignerSafe();
                }
                Debug.Assert(_isDesignerSafe != null, "Why didn't we set _isDesignerSafe in DetermineIfArtifactIsDesignerSafe()?");
                return _isDesignerSafe.Value;
            }
            protected set { _isDesignerSafe = value; }
        }

        /// <summary>
        ///     This will do analysis to determine if a document should be opened
        ///     only in the XmlEditor.
        /// </summary>
        internal abstract void DetermineIfArtifactIsDesignerSafe();

        /// <summary>
        ///     This will be called in the CommandProcessor and can contain custom
        ///     logic to short-circuit any model edits. This is useful for example,
        ///     to query the VS SCC provider.
        /// </summary>
        internal virtual bool CanEditArtifact()
        {
            if (XmlModelProvider != null)
            {
                var xmlModel = XmlModelProvider.GetXmlModel(Uri);
                if (xmlModel != null)
                {
                    return xmlModel.CanEditXmlModel();
                }
            }

            Debug.Fail("In trying to determine whether we CanEditArtifact(), we could not find the XML model attached to this EFArtifact");
            return false;
        }

        internal void AddParseErrorForObject(EFObject obj, string errorMessage, int errorCode)
        {
            var ei = new ErrorInfo(ErrorInfo.Severity.ERROR, errorMessage, obj, errorCode, ErrorClass.ParseError);
            AddParseErrorForObject(obj, ei);
        }

        internal void AddParseErrorForObject(EFObject obj, ErrorInfo errorInfo)
        {
            Debug.Assert(errorInfo.ErrorClass == ErrorClass.ParseError, "Unexpected error class added to EFObject");
            Debug.Assert(errorInfo.Item == obj, "ErrorInfo for wrong object added to EFObject");

            if (_errors == null)
            {
                _errors = new Dictionary<EFObject, ICollection<ErrorInfo>>();
            }

            ICollection<ErrorInfo> errorCollection;
            if (!_errors.TryGetValue(obj, out errorCollection))
            {
                errorCollection = new List<ErrorInfo>();
                _errors.Add(obj, errorCollection);
            }

            errorCollection.Add(errorInfo);
        }

        internal IEnumerable<ErrorInfo> GetAllParseErrorsForArtifact()
        {
            if (_errors != null)
            {
                foreach (var c in _errors.Values)
                {
                    foreach (var ei in c)
                    {
                        yield return ei;
                    }
                }
            }
        }

        internal void RemoveParseErrorsForObject(EFObject obj)
        {
            if (_errors == null)
            {
                return;
            }

            if (_errors.ContainsKey(obj))
            {
                _errors.Remove(obj);
            }
        }

        /// <summary>
        ///     Returns an enum representing the language type to use for code generation
        /// </summary>
        internal virtual LangEnum LanguageForCodeGeneration
        {
            get { return LangEnum.CSharp; }
        }        

        /// <summary>
        ///     Return schema version
        /// </summary>
        internal abstract Version SchemaVersion { get; }
    }
}
