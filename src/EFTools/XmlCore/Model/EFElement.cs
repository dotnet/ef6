// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.XLinqAnnotations;

    internal abstract class EFElement : EFContainer
    {
        internal const string AttributeMergeMode = "MergeMode";

        internal static char[] Whitespaces = { ' ', '\t', '\r', '\n' };
        internal static char[] NsSeparator = { ':' };
        internal static char[] SemanticSeparator = { '/' };

        protected EFElement(EFContainer parent, XElement element)
            : base(parent, element)
        {
        }

        #region EFElement Members

        internal override string SemanticName
        {
            get
            {
                if (Parent == null)
                {
                    return EFTypeName;
                }

                var count = 0;
                foreach (var child in Parent.Children)
                {
                    if (child == this)
                    {
                        break;
                    }
                    if (child.EFTypeName == EFTypeName)
                    {
                        ++count;
                    }
                }

                return Parent.SemanticName + "/" + EFTypeName + "[" + count + "]";
            }
        }

        internal XElement XElement
        {
            get { return XObject as XElement; }
        }

        internal virtual string DisplayName
        {
            get
            {
                var name = this as EFNameableItem;
                if (name != null)
                {
                    return name.LocalName.Value;
                }
                else
                {
                    var normal = this as EFNormalizableItem;
                    if (normal != null)
                    {
                        return normal.NormalizedNameExternal;
                    }
                    else
                    {
                        return EFTypeName;
                    }
                }
            }
        }

        /// <summary>
        ///     This is can be used to get a non-localized version of the DisplayName property.
        ///     This should only be used for testing purposes when writing localized strings to bsl files
        ///     is problematic.
        /// </summary>
        internal virtual string NonLocalizedDisplayName
        {
            get { return DisplayName; }
        }

        internal static int EFElementDisplayNameComparison(EFElement elem1, EFElement elem2)
        {
            return String.Compare(elem1.DisplayName, elem2.DisplayName, StringComparison.CurrentCulture);
        }

        // true if this element can contain DocumentationElement
        internal virtual bool HasDocumentationElement
        {
            get { return false; }
        }

        internal virtual EFContainer DocumentationEFContainer
        {
            get { return null; }
        }

        /// <summary>
        ///     A "ghost node" is a node in the EFObject hierarchy that does not have an explicit XElement in the xml file.
        ///     The edm files allow some syntactic short-hand to make authoring files a bit easier.  This means that there
        ///     are *implied* nodes in our model hierarchy.  For example, this
        ///     <EntityTypeMapping TableName="xyz">...</EntityTypeMapping>
        ///     And this
        ///     <EntityTypeMapping>
        ///         <MappingFragment TableName="xyz">...</MappingFragment>
        ///     </EntityTypeMapping>
        ///     are both valid and mean the same thing semantically.  In both cases, our EFObject tree has a MappingFragment
        ///     node.  In the first example, the MappingFragment node is a "ghost-node" because it is implicit (ie, there is no MappingFragment element in XML).
        /// </summary>
        internal bool IsGhostNode
        {
            get
            {
                // see if we are a ghost item; the msl spec lets you assume the existence
                // of some items, but we always want to create them in our model.  these
                // assumed or ghost items will all point to the real item's XElement.
                if (XObject != null
                    && Parent != null)
                {
                    if (XObject == Parent.XObject)
                    {
                        Debug.Assert(
                            GetType().Name == "EntityTypeMapping" ||
                            GetType().Name == "MappingFragment");
                        return true;
                    }
                }
                return false;
            }
        }

        protected EFElement GetGhostChild()
        {
            foreach (var o in Children)
            {
                var e = o as EFElement;
                if (e != null)
                {
                    if (e.IsGhostNode)
                    {
#if DEBUG
                        var childElementCount = 0;
                        foreach (var child in Children)
                        {
                            var el = child as EFElement;
                            if (el != null)
                            {
                                childElementCount++;
                            }
                        }
                        Debug.Assert(childElementCount == 1, "Unexpected child count for parent of a ghost node");
#endif
                        return e;
                    }
                }
            }
            return null;
        }

        /// <summary>
        ///     A helper function to get an attribute value from the encapsulated XLinq node
        ///     in an easy way that ignores namespaces.
        /// </summary>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        internal XAttribute GetAttribute(string attributeName)
        {
            if (XElement != null)
            {
                var attr = XElement.FirstAttribute;
                while (attr != null)
                {
                    if (attr.Name.LocalName == attributeName)
                    {
                        return attr;
                    }
                    attr = attr.NextAttribute;
                }
            }

            return null;
        }

        /// <summary>
        ///     A helper function to get an attribute value from the encapsulated XLinq node
        ///     paying attention to the attribute's namespace.
        /// </summary>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        internal XAttribute GetAttribute(string attributeName, string attributeNamespace)
        {
            var attr = XElement.FirstAttribute;
            while (attr != null)
            {
                if (attr.Name.LocalName == attributeName
                    &&
                    attr.Name.NamespaceName == attributeNamespace)
                {
                    return attr;
                }
                attr = attr.NextAttribute;
            }

            return null;
        }

        /// <summary>
        ///     Property which allows access to the MergeMode. We go directly to XLinq so that we
        ///     can save ourselves of the perf hit of including this into the model for every EFElement
        ///     as well as preserving assumptions about an element's children
        /// </summary>
        internal string MergeModeValue
        {
            get { return GetAttributeValue(AttributeMergeMode) ?? MergeModeDefaultValue; }
            set { SetAttributeValue(Artifact.ModelManager.GetRootNamespace(this), AttributeMergeMode, value); }
        }

        #endregion

        #region overridables

        public override string ToString()
        {
            return DisplayName;
        }

        internal override string ToPrettyString()
        {
            return DisplayName;
        }

        // we unfortunately get a warning from the compiler when we use the "base" keyword in "iterator" types generated by using the
        // "yield return" keyword.  By adding this method, I was able to get around this.  Unfortunately, I wasn't able to figure out
        // a way to implement this once and have derived classes share the implementation (since the "base" keyword is resolved at 
        // compile-time and not at runtime.
        private IEnumerable<EFObject> BaseChildren
        {
            get { return base.Children; }
        }

        internal override IEnumerable<EFObject> Children
        {
            get
            {
                foreach (var efobj in BaseChildren)
                {
                    yield return efobj;
                }
            }
        }

        // only used in DEBUG to identify xml attributes that we aren't processing
        internal virtual ICollection<string> MyAttributeNames()
        {
            ICollection<string> s = new HashSet<string>();
            return s;
        }

        // only used in DEBUG to identify xml elements that we aren't processing
        internal virtual ICollection<string> MyChildElementNames()
        {
            return new HashSet<string>();
        }

        /// <summary>
        ///     The EFObject tree is built based on the XLinq tree
        /// </summary>
        internal override void Parse(ICollection<XName> unprocessedElements)
        {
            State = EFElementState.ParseAttempted;

            PreParse();
            DoParse(unprocessedElements);
            PostParse(unprocessedElements);
        }

        protected virtual void PreParse()
        {
            State = EFElementState.ParseAttempted;
        }

        protected virtual void PostParse(ICollection<XName> unprocessedElements)
        {
#if DEBUG
            PopulateUnprocessedElementsCollection(unprocessedElements);
#endif
            State = EFElementState.Parsed;
        }

        protected virtual void DoParse(ICollection<XName> unprocessedElements)
        {
            foreach (var element in XElement.Elements())
            {
                ParseSingleElement(unprocessedElements, element);
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "unprocessedElements")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void PopulateUnprocessedElementsCollection(ICollection<XName> unprocessedElements)
        {
#if DEBUG
            // TODO: Investigate if the populated collection is used - if not remove this code

            // accumulate any unprocessed attributes.  This is only done during debug because it is a sanity check 
            // that we aren't missing anything.
            var processedAttributes = MyAttributeNames();

            XNamespace[] xmlNamespaces =
                {
                    "http://schemas.microsoft.com/ado/2006/04/edm",
                    "http://schemas.microsoft.com/ado/2008/09/edm",
                    "http://schemas.microsoft.com/ado/2009/11/edm",
                    "urn:schemas-microsoft-com:windows:storage:mapping:CS",
                    "http://schemas.microsoft.com/ado/2008/09/mapping/cs",
                    "http://schemas.microsoft.com/ado/2009/11/mapping/cs",
                    "http://schemas.microsoft.com/ado/2006/04/edm/ssdl",
                    "http://schemas.microsoft.com/ado/2009/02/edm/ssdl",
                    "http://schemas.microsoft.com/ado/2009/11/edm/ssdl",
                };

            if (!IsGhostNode)
            {
                // if this is a ghost-node, then attributes are consumed by the parent
                foreach (var a in XElement.Attributes())
                {
                    if (!processedAttributes.Contains(a.Name.LocalName))
                    {
                        if (xmlNamespaces.Contains(a.Name.NamespaceName))
                        {
                            var xname = XName.Get(XElement.Name.LocalName + "." + a.Name.LocalName, XElement.Name.NamespaceName);
                            if (!unprocessedElements.Contains(xname))
                            {
                                unprocessedElements.Add(xname);
                            }
                        }
                    }
                }
            }

            if (GetGhostChild() == null)
            {
                // if this is a parent of a ghost node, then elements are consumed by the child
                var processedElements = MyChildElementNames();
                foreach (var e in XElement.Elements())
                {
                    if (!processedElements.Contains(e.Name.LocalName))
                    {
                        if (xmlNamespaces.Contains(e.Name.NamespaceName))
                        {
                            if (!unprocessedElements.Contains(e.Name))
                            {
                                unprocessedElements.Add(e.Name);
                            }
                        }
                    }
                }
            }
#endif
        }

        /// <summary>
        ///     Once this method completes, all items that derive from EFNameableItem will have loaded
        ///     their normalized names into the symbol table (references to these items will NOT have these
        ///     references normalized)
        /// </summary>
        internal override void Normalize()
        {
            PreNormalize();
            DoNormalize();
            PostNormalize();
        }

        protected virtual void PreNormalize()
        {
            AssertSafeToNormalize();
            State = EFElementState.NormalizeAttempted;
        }

        protected virtual void PostNormalize()
        {
            Debug.Assert(
                State == EFElementState.NormalizeAttempted || State == EFElementState.Normalized,
                "Element of type " + GetType().Name + " has unexpected element state of " + State + " after normalize()");
        }

        protected virtual void DoNormalize()
        {
            // default normalize just sets the state to normlized.
            State = EFElementState.Normalized;
        }

        /// <summary>
        ///     References to EFNameableItems will be normalized and an attempt is made to bind these references
        ///     to any found items (that is, the normalized reference is found in the symbol table).
        /// </summary>
        internal override void Resolve(EFArtifactSet artifactSet)
        {
            PreResolve();
            DoResolve(artifactSet);
            PostResolve();
        }

        protected virtual void PreResolve()
        {
            AssertSafeToResolve();
            State = EFElementState.ResolveAttempted;
        }

        protected virtual void PostResolve()
        {
            Debug.Assert(
                State == EFElementState.ResolveAttempted || State == EFElementState.Resolved,
                "Element of type " + GetType().Name + " has unexpected element state of " + State + " after resolve()");
        }

        protected virtual void DoResolve(EFArtifactSet artifactSet)
        {
            State = EFElementState.Resolved;
        }

        /// <summary>
        ///     NOTE: this is called from the EFObject c'tor so only that class is fully instantiated!
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        protected override void AddToXlinq()
        {
            State = EFElementState.ParseAttempted;

            Debug.Assert(XObject == null, "Object already serialized");
            Debug.Assert(!string.IsNullOrEmpty(EFTypeName), "Can't create XElement without the name");
            Debug.Assert(Parent != null && Parent.XContainer != null, "Can't serialize this if the Parent or it's XContainer is missing");

            var ns = Artifact.ModelManager.GetRootNamespace(Parent);
            var element = new XElement(ns == null ? EFTypeName : ns + EFTypeName);

            SetXObject(element);
            ModelItemAnnotation.SetModelItem(element, this);

            AddXElementToParent(element);

            State = EFElementState.Parsed;
        }

        protected override void OnDelete(bool deleteXObject)
        {
            if (IsGhostNode)
            {
                // set xobject to null, so we won't remove it from xlinq
                SetXObject(null);
            }

            base.OnDelete(deleteXObject);
        }

        protected override void RemoveFromXlinq()
        {
            if (XElement != null)
            {
                Debug.Assert(XElement.Parent != null);
                XElement.Remove();
                SetXObject(null);
            }
        }

        internal virtual DeleteEFElementCommand GetDeleteCommand()
        {
            var cmd = new DeleteEFElementCommand(this);
            if (cmd == null)
            {
                // shouldn't happen, just to be safe
                throw new InvalidOperationException();
            }
            return cmd;
        }

        #endregion

        /// <summary>
        ///     A helper function to get an attribute value from the encapsulated XLinq node
        ///     in an easy way that ignores namespaces.
        /// </summary>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        protected string GetAttributeValue(string attributeName)
        {
            var attr = XElement.FirstAttribute;
            while (attr != null)
            {
                if (attr.Name.LocalName == attributeName)
                {
                    return attr.Value;
                }
                attr = attr.NextAttribute;
            }

            return null;
        }

        /// <summary>
        ///     This will set the attribute's value or add the attribute if it doesn't exist
        /// </summary>
        /// <param name="attributeName"></param>
        /// <param name="attributeValue"></param>
        internal void SetAttributeValue(XNamespace namespaceName, string attributeName, string attributeValue)
        {
            var attr = GetAttribute(attributeName);
            if (attr == null)
            {
                attr = new XAttribute(namespaceName + attributeName, attributeValue);
                XContainer.Add(attr);
            }
            else
            {
                if (attr.Value == attributeValue)
                {
                    return;
                }

                if (attributeValue == null)
                {
                    attr.Remove();
                }
                else
                {
                    attr.Value = attributeValue;
                }
            }
        }

        internal void AddXElementToParent(XElement element)
        {
            XNode insertPosition;
            bool insertBefore;
            Parent.GetXLinqInsertPosition(this, out insertPosition, out insertBefore);

            if (insertPosition == null)
            {
                Parent.XContainer.Add(element);
                EnsureFirstNodeWhitespaceSeparation(element);
            }
            else
            {
                // If insertPosition is not null, Check if insertPosition property has a parent since the AddBeforeSelf/AddAfterSelf will fail if the parent is null.
                Debug.Assert(
                    insertPosition.Parent != null,
                    "We can't insert the node before/after the insertPosition node since it doesn't have parent");
                if (insertPosition.Parent != null)
                {
                    if (insertBefore)
                    {
                        insertPosition.AddBeforeSelf(element);
                        EnsureWhitespaceBeforeNode(insertPosition);
                    }
                    else
                    {
                        insertPosition.AddAfterSelf(element);
                        EnsureWhitespaceBeforeNode(element);
                    }
                }
            }
        }

        /// <summary>
        ///     Ensures the whitespace separation between two nodes.  Appropriate whitespace is added before the given node
        /// </summary>
        /// <param name="first"></param>
        private XText EnsureWhitespaceBeforeNode(XNode node)
        {
            Debug.Assert(
                node.Parent != null && node.Parent.Elements().Count() > 0,
                "Call EnsureFirstNodeWhitespaceSeparation when the added element is the first node");

            var newLine = new XText(Environment.NewLine + new string(' ', GetIndentLevel() * 2));
            node.AddBeforeSelf(newLine);
            return newLine;
        }

        /// <summary>
        ///     Ensures the whitespace separation for the first child element added to a node.
        /// </summary>
        /// <param name="element"></param>
        private void EnsureFirstNodeWhitespaceSeparation(XNode element)
        {
#if DEBUG
            if (element.Parent != null)
            {
                Debug.Assert(element.Parent.Elements().Count() == 1, "Unexpected count of elements!");
            }
            else
            {
                // if first.Parent is null, then this is the root element in a document
                Debug.Assert(element.Document.Root == element, "element had null parent, but is not the root element of the document");
            }
#endif

            var precedingNewLine = new XText(Environment.NewLine + new string(' ', GetIndentLevel() * 2));
            var trailingNewLine = new XText(Environment.NewLine + new string(' ', Parent.GetIndentLevel() * 2));
            element.AddBeforeSelf(precedingNewLine);
            element.AddAfterSelf(trailingNewLine);
        }
    }
}
