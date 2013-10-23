// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using EFVisitor = Microsoft.Data.Entity.Design.Model.Visitor.Visitor;

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Model.Visitor;

    internal abstract class EFContainer : EFObject
    {
        private EFElementState _state = EFElementState.None;

        private static readonly ICollection<EFObject> _readOnlyCollection = new List<EFObject>().AsReadOnly();

        protected EFContainer(EFContainer parent, XContainer xcontainer)
            : base(parent, xcontainer)
        {
        }

        /// <summary>
        ///     Return an IEnumerable to iterate over all child nodes of this container.
        ///     <para>
        ///         Subclass overrides should use the "yield return" keyword to avoid making
        ///         unnecessary temporary collections.
        ///     </para>
        /// </summary>
        internal virtual IEnumerable<EFObject> Children
        {
            get { return _readOnlyCollection; }
        }

        internal XContainer XContainer
        {
            get { return XObject as XContainer; }
        }

        internal EFElementState State
        {
            get { return _state; }
            set { _state = value; }
        }

        public override IEnumerable<IVisitable> Accept(EFVisitor visitor)
        {
            visitor.Visit(this);

            return Children.Where(child => child != null);
        }

        /// <summary>
        ///     You can't call base.Dispose(bool) since its an abstract method on EFObject
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                var children = Children;
                foreach (var o in children)
                {
                    if (o != null)
                    {
                        o.Dispose();
                    }
                }
            }
        }

        protected static void ClearEFObject(EFObject efObjectToClear)
        {
            if (efObjectToClear != null)
            {
                efObjectToClear.Dispose();
            }
        }

        protected static void ClearEFObjectCollection<T>(ICollection<T> efObjectsToClear) where T : EFObject
        {
            if (efObjectsToClear != null)
            {
                foreach (EFObject o in efObjectsToClear)
                {
                    o.Dispose();
                }
            }
            efObjectsToClear.Clear();
        }

        internal abstract void Parse(ICollection<XName> unprocessedElements);
        internal abstract void Normalize();
        internal abstract void Resolve(EFArtifactSet artifactSet);

        internal virtual bool ParseSingleElement(ICollection<XName> unprocessedElements, XElement element)
        {
            return false;
        }

        /// <summary>
        ///     This method can be called by any external change, such as undo/redo
        /// </summary>
        internal virtual bool ReparseSingleElement(ICollection<XName> unprocessedElements, XElement element)
        {
            return ParseSingleElement(unprocessedElements, element);
        }

        protected override void OnDelete(bool deleteXObject)
        {
            // we need to make a copy of the Children to avoid modifying the Children collection while iterating over it
            // also, we should delete the Children in reverse so that on an undo of this we preserve as close as possible the state of the XML
            var childrenCopy = new List<EFObject>();
            foreach (var child in Children.Reverse())
            {
                childrenCopy.Add(child);
            }

            // we delete the children before the parent otherwise these changes won't get recorded in the Xml transaction
            foreach (var child in childrenCopy)
            {
                child.Delete(deleteXObject);
            }

            if (deleteXObject)
            {
                RemoveFromXlinq();
            }

            if (Parent != null)
            {
                Parent.OnChildDeleted(this);
            }
        }

        protected virtual void OnChildDeleted(EFContainer efContainer)
        {
        }

        /// <summary>
        ///     Get a child EFNameableItem by normalized name
        /// </summary>
        /// <param name="normalizedName"></param>
        /// <returns></returns>
        internal EFNameableItem GetFirstNamedChildByNormalizedName(string normalizedName)
        {
            foreach (var child in Children)
            {
                var nameable = child as EFNameableItem;
                if (nameable != null)
                {
                    if (nameable.NormalizedName != null)
                    {
                        if (nameable.NormalizedName.Equals(normalizedName))
                        {
                            return nameable;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        ///     Get a child EFNameableItem by local name, case-sensitive
        /// </summary>
        /// <param name="localName"></param>
        /// <returns></returns>
        internal EFNameableItem GetFirstNamedChildByLocalName(string localName)
        {
            return GetFirstNamedChildByLocalName(localName, false);
        }

        /// <summary>
        ///     Get a child EFNameableItem by local name using the passed in comparison type
        /// </summary>
        /// <param name="localName"></param>
        /// <returns></returns>
        internal EFNameableItem GetFirstNamedChildByLocalName(string localName, bool ignoreCase)
        {
            foreach (var child in Children)
            {
                var nameable = child as EFNameableItem;
                if (nameable != null)
                {
                    if (nameable.LocalName.Value != null)
                    {
                        var comparisonType = (ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture);
                        if (nameable.LocalName.Value.Equals(localName, comparisonType))
                        {
                            return nameable;
                        }
                    }
                }
            }
            return null;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "Do refer to instance properties but only in Debug")]
        protected void AssertSafeToNormalize()
        {
            Debug.Assert(
                State == EFElementState.Parsed ||
                State == EFElementState.Normalized ||
                State == EFElementState.NormalizeAttempted ||
                State == EFElementState.ResolveAttempted ||
                State == EFElementState.Resolved);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "Do refer to instance properties but only in Debug")]
        protected void AssertSafeToResolve()
        {
            Debug.Assert(
                State == EFElementState.Normalized ||
                State == EFElementState.ResolveAttempted ||
                State == EFElementState.Resolved);
        }

        // This will be called from the child EFObject's constructor, so not all of the member variables may be hooked up yet. 
        // be careful.  De-referencing certain fields may trigger NREs.  Default implementation will append new child elements as the last 
        // element.
        internal virtual void GetXLinqInsertPosition(EFElement child, out XNode insertAt, out bool insertBefore)
        {
            insertAt = XContainer.Elements().LastOrDefault();
            insertBefore = false;
        }

        protected XElement FirstChildXElementOrNull()
        {
            return XContainer.Elements().FirstOrDefault();
        }

        protected internal virtual string MergeModeDefaultValue
        {
            get { return XmlModelConstants.MergeMode_TwoWay; }
        }
    }
}
