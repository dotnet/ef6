// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Xml.Linq;
    using Microsoft.Data.Tools.XmlDesignerBase.Base.Util;
    using Microsoft.Data.Entity.Design.Model.XLinqAnnotations;
    using Microsoft.Data.Entity.Design.VersioningFacade;

    /// <summary>
    ///     Helper class that store the information where a property should be inserted.
    /// </summary>
    internal class InsertPropertyPosition
    {
        internal PropertyBase InsertAtProperty;
        internal bool InsertBefore; // Flag whether to insert before or insert after InsertAt.

        internal InsertPropertyPosition(PropertyBase insertAtProperty, bool insertBefore)
        {
            Debug.Assert(insertAtProperty != null, "Parameter insertAt cannot be null.");
            InsertBefore = insertBefore;
            InsertAtProperty = insertAtProperty;
        }
    }

    internal abstract class PropertyBase : DocumentableAnnotatableElement
    {
        internal const string AttributeGetter = "GetterAccess";
        internal const string AttributeSetter = "SetterAccess";

        private DefaultableValue<string> _getterAttr;
        private DefaultableValue<string> _setterAttr;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        protected PropertyBase(EFElement parent, XElement element, InsertPropertyPosition insertPosition)
            : base(parent, element)
        {
            // Unfortunately, the XElement is created in the EFObject constructor. When the base class called EntityType's GetXLinqInsertPosition,
            // the Property's InsertPosition is not set. As a work around, we need to manually move the Property here. 
            // This is not efficient as it could be because it involves removing and adding the property's XElement.
            // Currently, the only thing that use this functionality is CopyAndPaste. 
            // We might want to consider change this when other scenarios depend on the functionality.
            if (insertPosition != null)
            {
                MoveTo(insertPosition);
            }
        }

        /// <summary>
        ///     Manages the content of the Getter attribute
        /// </summary>
        internal DefaultableValue<string> Getter
        {
            get
            {
                if (_getterAttr == null)
                {
                    _getterAttr = new GetterDefaultableValue(this);
                }
                return _getterAttr;
            }
        }

        /// <summary>
        ///     Return the parent as EntityType.
        ///     Return null if the parent is not an EntityType.
        /// </summary>
        internal EntityType EntityType
        {
            get
            {
                var entityType = Parent as EntityType;
                return entityType;
            }
        }

        /// <summary>
        ///     Manages the content of the Setter attribute
        /// </summary>
        internal DefaultableValue<string> Setter
        {
            get
            {
                if (_setterAttr == null)
                {
                    _setterAttr = new SetterDefaultableValue(this);
                }
                return _setterAttr;
            }
        }

        private class SetterDefaultableValue : DefaultableValue<string>
        {
            internal SetterDefaultableValue(EFElement parent)
                : base(parent, AttributeSetter, SchemaManager.GetCodeGenerationNamespaceName())
            {
            }

            internal override string AttributeName
            {
                get { return AttributeSetter; }
            }

            public override string DefaultValue
            {
                get { return ModelConstants.CodeGenerationAccessPublic; }
            }
        }

        private class GetterDefaultableValue : DefaultableValue<string>
        {
            internal GetterDefaultableValue(EFElement parent)
                : base(parent, AttributeGetter, SchemaManager.GetCodeGenerationNamespaceName())
            {
            }

            internal override string AttributeName
            {
                get { return AttributeGetter; }
            }

            public override string DefaultValue
            {
                get { return ModelConstants.CodeGenerationAccessPublic; }
            }
        }

#if DEBUG
        internal override ICollection<string> MyAttributeNames()
        {
            var s = base.MyAttributeNames();
            s.Add(AttributeGetter);
            s.Add(AttributeSetter);
            return s;
        }
#endif

        protected override void PreParse()
        {
            Debug.Assert(State != EFElementState.Parsed, "this object should not already be in the parsed state");

            ClearEFObject(_getterAttr);
            _getterAttr = null;

            ClearEFObject(_setterAttr);
            _setterAttr = null;

            base.PreParse();
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
                yield return Getter;
                yield return Setter;
            }
        }

        /// <summary>
        ///     Store the information where the property will be inserted.
        /// </summary>
        internal InsertPropertyPosition InsertPosition { get; set; }

        /// <summary>
        ///     Return the next sibling of the property in the order of property's XElement in the XDocument.
        ///     Note that this is different from the order of the model property in the model entity-type property list.
        /// </summary>
        internal PropertyBase NextSiblingInPropertyXElementOrder
        {
            get
            {
                var element = XElement.ElementsAfterSelf().FirstOrDefault(xe => xe.Name.LocalName == EFTypeName);
                if (element != null)
                {
                    var property = ModelItemAnnotation.GetModelItem(element) as PropertyBase;
                    Debug.Assert(property != null, "Could not find EFElement for XElement: " + element);
                    return property;
                }

                return null;
            }
        }

        /// <summary>
        ///     Return the previous sibling of the property in the order of property's XElement in the XDocument.
        ///     Note that this is different from the order of the model property in the model entity-type property list.
        /// </summary>
        internal PropertyBase PreviousSiblingInPropertyXElementOrder
        {
            get
            {
                var element = XElement.ElementsBeforeSelf().LastOrDefault(xe => xe.Name.LocalName == EFTypeName);
                if (element != null)
                {
                    var property = ModelItemAnnotation.GetModelItem(element) as PropertyBase;
                    Debug.Assert(property != null, "Could not find EFElement for XElement: " + element);
                    return property;
                }
                return null;
            }
        }

        /// <summary>
        ///     Move Property's XElement before the specified position.
        ///     If position parameter is null, the property XElement will be moved to the last position.
        /// </summary>
        internal void MoveTo(InsertPropertyPosition position)
        {
            Debug.Assert(
                PreviousSiblingInPropertyXElementOrder != null || NextSiblingInPropertyXElementOrder != null,
                "Why do we need to move the property if it is the only property?");
            Debug.Assert(position != null, "InsertPropertyPosition parameter is null.");
            if (position != null)
            {
                // Check if the InsertPropertyPosition's InsertAt is not null.
                Debug.Assert(position.InsertAtProperty != null, "Why InsertPropertyPosition's InsertAt is null?");
                if (position.InsertAtProperty != null)
                {
                    // Instead of re-parenting the property's XElement, we are going to clone the XElement, insert the clone and delete the old XElement.
                    // This is a workaround for an XML editor bug where re-parenting an element causes asserts.

                    // First create the new XElement.
                    var tempDoc = XDocument.Parse(XElement.ToString(SaveOptions.None), LoadOptions.None);
                    var newPropertyXElement = tempDoc.Root;
                    newPropertyXElement.Remove();

                    // Remove known namespaces from the element since the namespaces are already set in the parent node.
                    // This is workaround because XDocument automatically appends the default namespace in the property XElement.
                    foreach (var a in newPropertyXElement.Attributes())
                    {
                        if (a.IsNamespaceDeclaration
                            && (a.Value == SchemaManager.GetCSDLNamespaceName(Artifact.SchemaVersion)
                                || a.Value == SchemaManager.GetSSDLNamespaceName(Artifact.SchemaVersion)
                                || a.Value == SchemaManager.GetAnnotationNamespaceName()))
                        {
                            a.Remove();
                        }
                    }

                    var toBeDeleteElement = XElement;

                    // format the XML we just parsed so that the XElement will have the same indenting.
                    Utils.FormatXML(newPropertyXElement, GetIndentLevel());

                    // Call method that will insert the XElement to the specified location.
                    InsertPosition = position;
                    AddXElementToParent(newPropertyXElement);

                    // Re-establish the links between EFElement and XElement.
                    SetXObject(newPropertyXElement);
                    Debug.Assert(
                        XElement == newPropertyXElement,
                        "Unexpected XElement value. Expected:" + newPropertyXElement + " , Actual:" + XElement);

                    ModelItemAnnotation.SetModelItem(newPropertyXElement, this);
                    Debug.Assert(
                        ModelItemAnnotation.GetModelItem(newPropertyXElement) == this,
                        "The new XElement should contain annotation to the model property.");

                    // Delete both old XElement and the preceding whitespace.
                    // Preceding whitespace is preferred over trailing whitespace because we don't want to remove the last property's trailing white-space since
                    // it has different indent level than the rest (see EFElement's EnsureFirstNodeWhitespaceSeparation method).
                    var precedingNewLine = toBeDeleteElement.PreviousNode as XText;
                    while (precedingNewLine != null
                           && String.IsNullOrWhiteSpace(precedingNewLine.Value))
                    {
                        var toBeDeletedWhiteSpace = precedingNewLine;
                        precedingNewLine = precedingNewLine.PreviousNode as XText;
                        toBeDeletedWhiteSpace.Remove();
                    }
                    toBeDeleteElement.Remove();

#if DEBUG
                    // Assert if the property is not moved to the correct location.
                    if (position.InsertBefore)
                    {
                        Debug.Assert(
                            position.InsertAtProperty == NextSiblingInPropertyXElementOrder,
                            "Expected next sibling property: " + position.InsertAtProperty.DisplayName + " , Actual next sibling:"
                            + NextSiblingInPropertyXElementOrder.DisplayName);
                    }
                    else
                    {
                        Debug.Assert(
                            position.InsertAtProperty == PreviousSiblingInPropertyXElementOrder,
                            "Expected previous sibling property: " + position.InsertAtProperty.DisplayName + " , Actual previous sibling:"
                            + PreviousSiblingInPropertyXElementOrder.DisplayName);
                    }
#endif
                }
            }
        }
    }
}
