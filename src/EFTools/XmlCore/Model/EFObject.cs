// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using EFVisitor = Microsoft.Data.Entity.Design.Model.Visitor.Visitor;

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Model.Visitor;
    using Microsoft.Data.Entity.Design.Model.XLinqAnnotations;
    using Microsoft.Data.Tools.XmlDesignerBase.Model;

    internal abstract class EFObject : IVisitable, IDisposable
    {
        private string _identity;
        private readonly EFContainer _parent;
        private XObject _xobject;

        /// <summary>
        ///     Bit Field use this to track various boolean states of this object.
        /// </summary>
        private byte _stateField = 0;

        /// <summary>
        ///     States to use in _stateField
        /// </summary>
        private static byte IS_DISPOSED_STATE = 0x01;

        private static byte IS_DELETING_STATE = 0x02;
        private static byte IS_CONSTRUCTION_COMPLETED = 0x04;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        protected EFObject(EFContainer parent, XObject xobject)
        {
            // note that it is OK if parent is null here; derived classes should assert
            // in their own constructor if they want to ensure that parent is not null
            _parent = parent;

            if (xobject == null)
            {
                // Incase parent is null, elements will be created without any namespace
                // the implementation of AddToXLinq will add the model item annotation.
                AddToXlinq();
            }
            else
            {
                _xobject = xobject;
                ModelItemAnnotation.SetModelItem(_xobject, this);
            }

            Debug.Assert(_xobject == null || ModelItemAnnotation.GetModelItem(_xobject) != null, "xobject didn't have a model annotation!");

            SetState(IS_CONSTRUCTION_COMPLETED);
        }

        // testing only
        internal EFObject()
        {
        }

        [SuppressMessage("Microsoft.Performance", "CA1821:RemoveEmptyFinalizers")]
        ~EFObject()
        {
            Debug.Assert(
                IsDisposed,
                string.Format(CultureInfo.InvariantCulture,
                    "Undisposed EFObject of type {0}: {1} :: {2}, {3}",
                    GetType().FullName,
                    ToPrettyString(),
                    Parent != null && Parent.Artifact != null ? Parent.Artifact.Uri : null,
                    Parent != null ? Parent.SemanticName : null));
        }

        public void Delete()
        {
            Delete(true);
        }

        internal void Delete(bool deleteXObject)
        {
            if (!IsDeleting)
            {
                SetState(IS_DELETING_STATE);
                OnDelete(deleteXObject);
                RemoveErrors();
                Dispose();
            }
        }

        private void SetState(byte b)
        {
            _stateField = (byte)(_stateField | b);
        }

        private bool HasState(byte b)
        {
            return (_stateField & b) == b;
        }

        internal bool IsDisposed
        {
            get { return HasState(IS_DISPOSED_STATE); }
        }

        private bool IsDeleting
        {
            get { return HasState(IS_DELETING_STATE); }
        }

        protected bool IsConstructionCompleted
        {
            get { return HasState(IS_CONSTRUCTION_COMPLETED); }
        }

        protected void RemoveErrors()
        {
            if (Artifact != null
                && Artifact.ArtifactSet != null)
            {
                Artifact.ArtifactSet.RemoveErrorsForEFObject(this);
            }
        }

        protected abstract void OnDelete(bool deleteXObject);

        /// <summary>
        ///     This returns a collection of ItemBindings which are anti-
        ///     dependencies to this EFObject.
        /// </summary>
        /// <returns></returns>
        internal ICollection<ItemBinding> GetDependentBindings()
        {
            var bindings = new List<ItemBinding>();
            foreach (var efObject in GetAntiDependencies())
            {
                var binding = efObject as ItemBinding;
                if (binding != null)
                {
                    bindings.Add(binding);
                }
            }

            return bindings;
        }

        /// <summary>
        ///     Performs management functions to the objects Dependent Bindings.
        /// </summary>
        /// <param name="clear">Pass true to cause the binding to be cleared, this will remove the XAttribute</param>
        /// <param name="rebind">Pass true to cause the binding to rebind</param>
        internal void ManageDependentBindings(bool clear, bool rebind)
        {
            foreach (var binding in GetDependentBindings())
            {
                if (clear)
                {
                    binding.SetRefName(null);
                }

                if (clear || rebind)
                {
                    binding.Rebind();
                }
            }
        }

        internal ICollection<EFObject> GetAntiDependencies()
        {
            var artifactSet = Artifact.ModelManager.GetArtifactSet(Artifact.Uri);
            Debug.Assert(artifactSet != null);
            if (artifactSet != null)
            {
                return artifactSet.GetAntiDependencies(this);
            }
            return new List<EFObject>(0);
        }

        internal ICollection<T> GetAntiDependenciesOfType<T>() where T : EFObject
        {
            var list = new HashSet<T>();

            foreach (var antiDep in GetAntiDependencies())
            {
                T typedAntiDep = null;
                if (antiDep is EFAttribute
                    &&
                    antiDep.Parent != null)
                {
                    typedAntiDep = antiDep.Parent as T;
                }
                else if (antiDep is EFElement)
                {
                    typedAntiDep = antiDep as T;
                }

                if (typedAntiDep != null
                    &&
                    list.Contains(typedAntiDep) == false)
                {
                    list.Add(typedAntiDep);
                }
            }

            return list;
        }

        /// <summary>
        ///     Walks up the tree and returns the first parent that is of the Type passed in.
        /// </summary>
        /// <param name="type">The type of parent to find.</param>
        /// <returns></returns>
        internal EFObject GetParentOfType(Type type)
        {
            if (GetType() == type
                || type.IsAssignableFrom(GetType()))
            {
                return this;
            }

            var item = Parent;
            while (item != null)
            {
                if (item.GetType() == type
                    || type.IsAssignableFrom(item.GetType()))
                {
                    return item;
                }
                item = item.Parent;
            }

            return null;
        }

        internal virtual string EFTypeName
        {
            get { return GetType().Name; }
        }

        internal virtual string Identity
        {
            get
            {
                if (_identity == null)
                {
                    _identity = Artifact.Uri + ":" +
                                EFTypeName + ":" +
                                ModelAnnotation.GetNextIdentity(Artifact.XObject);
                }
                return _identity;
            }
        }

        //#if DEBUG
        //        // uncomment this section for debuggin-purposes only
        //
        //        /// <summary>
        //        /// Debug-only field that is used to help differentiate instances of objects
        //        /// </summary>
        //        private Guid _domainId = Guid.NewGuid();
        //
        //        /// <summary>
        //        /// This is the unique ID for this particual in-memory instance of this EFObject.  This
        //        /// is mainly useful for debugging purposes.
        //        /// </summary>
        //        /// 
        //        internal Guid DomainId
        //        {
        //            get { return _domainId; } 
        //        }
        //#endif

        internal abstract string SemanticName { get; }

        protected abstract void AddToXlinq();

        protected abstract void RemoveFromXlinq();

        // virtual for testing
        internal virtual EFContainer Parent
        {
            get { return _parent; }
            set
            {
                Debug.Fail(
                    "ERROR!  Do Not re-set the parent after of an object after creation.  You should copy the XML, and create a new one.  This causes problems with the XML Editor");
            }
        }

        internal XObject XObject
        {
            get { return _xobject; }
        }

        // change back to protected once we can remove the XML Editor workaround
        internal void SetXObject(XObject xobject)
        {
            _xobject = xobject;
        }

        /// <summary>
        ///     The Uri of the file/resource where this item is persisted
        /// </summary>
        internal virtual Uri Uri
        {
            get { return Artifact.Uri; }
        }

        /// <summary>
        ///     Walks up the tree and returns the first item without a parent.
        /// </summary>
        /// <returns></returns>
        internal EFContainer GetRoot()
        {
            if (Parent == null)
            {
                var root = this as EFContainer;
                Debug.Assert(root != null, "Unexpected! root is not an instance of EFContainer.  Something is wrong.");
                return root;
            }

            var item = Parent;
            while (item.Parent != null)
            {
                item = item.Parent;
            }

            return item;
        }

        internal virtual EFArtifact Artifact
        {
            get
            {
                var root = GetRoot();
                var a = root as EFArtifact;
                Debug.Assert(a != null, "Unexpected null artifact for item");
                return a;
            }
        }

        public virtual IEnumerable<IVisitable> Accept(EFVisitor visitor)
        {
            visitor.Visit(this);
            return new List<IVisitable>(0).AsReadOnly();
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                Dispose(true);
                SetState(IS_DISPOSED_STATE);
                GC.SuppressFinalize(this);
            }
        }

        protected abstract void Dispose(bool disposing);

        internal abstract string ToPrettyString();

        internal TextSpan GetTextSpan()
        {
            if (XObject == null)
            {
                Debug.Fail(
                    "We are trying to get a TextSpan for a Model element that doesn't have a valid XLinq node: " +
                    EFTypeName);
                return new TextSpan();
            }
            else
            {
                return Artifact.XmlModelProvider.GetTextSpanForXObject(XObject, Artifact.Uri);
            }
        }

        // virtual for testing
        internal virtual int GetLineNumber()
        {
            var textSpan = GetTextSpan();
            return textSpan.iStartLine;
        }

        // virtual for testing
        internal virtual int GetColumnNumber()
        {
            var textSpan = GetTextSpan();
            return textSpan.iStartIndex;
        }

        internal int GetIndentLevel()
        {
            if (Parent == null)
            {
                return 0;
            }
            else
            {
                var numParents = 0;
                // use this.Parent.XObject instead of this.XObject.Parent in case this is called before 
                // the xobject for this node has been created.
                var parent = Parent.XObject as XElement;
                while (parent != null)
                {
                    numParents += 1;
                    parent = parent.Parent;
                }
                return numParents;
            }
        }
    }
}
