// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.VersioningFacade;

    /// <summary>
    ///     Instantiated via an XElement and a version, this class will retrieve
    ///     all annotations under the XElement given an annotation namespace.
    /// </summary>
    internal class AnnotationManager
    {
        private XElement _xElement;
        private HashSet<string> _reservedNamespaces;

        internal AnnotationManager(XElement xelement, Version version)
        {
            Load(xelement, version);
        }

        internal void Load(XElement xelement, Version version)
        {
            _reservedNamespaces = new HashSet<string>();
            _xElement = xelement;
            foreach (var n in SchemaManager.GetAllNamespacesForVersion(version))
            {
                _reservedNamespaces.Add(n);
            }
        }

        internal void UpdateAttributeName(string namespaceName, string oldName, string newName)
        {
            Debug.Assert(!String.IsNullOrEmpty(oldName), "Attribute name for AnnotationManager.UpdateAttributeName is null or empty");

            if (!String.IsNullOrEmpty(oldName))
            {
                var xattr = GetAnnotation<XAttribute>(namespaceName, oldName);
                if (xattr != null)
                {
                    var parent = xattr.Parent;
                    if (parent != null)
                    {
                        var existingNs = xattr.Name.Namespace;
                        var existingvalue = xattr.Value;
                        xattr.Remove();
                        parent.Add(new XAttribute(existingNs + newName, existingvalue));
                    }
                }
            }
        }

        /// <summary>
        ///     Update, Create, or Delete an annotation that is represented by an attribute.
        ///     This also handles defaults.
        /// </summary>
        /// <param name="namespaceName"></param>
        /// <param name="name"></param>
        /// <param name="newValue">Pass in null to delete</param>
        /// <param name="isDefault"></param>
        internal void UpdateAttributeValue(string namespaceName, string name, string newValue, bool isDefault)
        {
            Debug.Assert(!String.IsNullOrEmpty(name), "Attribute name for AnnotationManager.UpdateAttributeValue is null or empty");

            if (!String.IsNullOrEmpty(name))
            {
                var xattr = GetAnnotation<XAttribute>(namespaceName, name);
                if (xattr != null)
                {
                    if (newValue == null || isDefault)
                    {
                        xattr.Remove();
                    }
                    else if (xattr.Value != newValue)
                    {
                        xattr.Value = newValue;
                    }
                }
                else if (!isDefault
                         && newValue != null)
                {
                    xattr = new XAttribute(XName.Get(name, namespaceName), newValue);
                    _xElement.Add(xattr);
                }
            }
        }

        internal void RemoveAttribute(string namespaceName, string name)
        {
            Debug.Assert(!String.IsNullOrEmpty(name), "Attribute name for AnnotationManager.RemoveAttribute is null or empty");
            if (!String.IsNullOrEmpty(name))
            {
                var xattr = GetAnnotation<XAttribute>(namespaceName, name);
                if (xattr != null)
                {
                    xattr.Remove();
                }
            }
        }

        internal IEnumerable<XObject> GetAnnotations()
        {
            foreach (var xa in _xElement.Attributes())
            {
                // EFRuntime doesn't namespace-qualify most of their attributes, so we just skip them here
                if (xa.Name != null
                    && String.IsNullOrEmpty(xa.Name.NamespaceName) == false)
                {
                    if (_reservedNamespaces.Contains(xa.Name.NamespaceName) == false)
                    {
                        yield return xa;
                    }
                }
            }

            foreach (var xe in _xElement.Elements())
            {
                if (xe.Name.NamespaceName != null
                    && _reservedNamespaces.Contains(xe.Name.NamespaceName) == false)
                {
                    yield return xe;
                }
            }
        }

        internal IEnumerable<T> GetAnnotations<T>(string namespaceName) where T : XObject
        {
            return GetAnnotations().OfType<T>().Where(
                xo =>
                    {
                        var xa = xo as XAttribute;
                        var xe = xo as XElement;
                        if (xa != null)
                        {
                            return xa.Name.NamespaceName == namespaceName;
                        }
                        if (xe != null)
                        {
                            return xe.Name.NamespaceName == namespaceName;
                        }
                        return false;
                    });
        }

        internal T GetAnnotation<T>(string namespaceName, string name) where T : XObject
        {
            return GetAnnotations<T>(namespaceName).Where(
                xo =>
                    {
                        var xa = xo as XAttribute;
                        var xe = xo as XElement;
                        if (xa != null)
                        {
                            return xa.Name.LocalName == name;
                        }
                        if (xe != null)
                        {
                            return xe.Name.LocalName == name;
                        }
                        return false;
                    }).SingleOrDefault();
        }

        internal void RemoveAnnotations<T>(string namespaceName) where T : XObject
        {
            IEnumerable<T> annotations = new List<T>(GetAnnotations<T>(namespaceName));
            foreach (var annotation in annotations.OfType<XAttribute>())
            {
                annotation.Remove();
            }
            foreach (var annotation in annotations.OfType<XElement>())
            {
                annotation.Remove();
            }
        }
    }
}
