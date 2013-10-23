// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Diagnostics;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Model.XLinqAnnotations;

    internal abstract class EFAttribute : EFObject
    {
        protected XNamespace _namespace = null;

        protected EFAttribute(EFContainer parent, XAttribute xattribute)
            : base(parent, xattribute)
        {
        }

        internal XAttribute XAttribute
        {
            get { return XObject as XAttribute; }
        }

        internal virtual XNamespace Namespace
        {
            get { return _namespace; }
            set { _namespace = value; }
        }

        internal string GetXAttributeValue()
        {
            if (XAttribute != null)
            {
                return XAttribute.Value;
            }
            return null;
        }

        internal static string GetXAttributeValue(XAttribute attribute)
        {
            var attr = ModelItemAnnotation.GetModelItem(attribute) as EFAttribute;
            Debug.Assert(attr != null);
            if (attr != null)
            {
                return attr.GetXAttributeValue();
            }
            return null;
        }

        internal void SetXAttributeValue(string newValue)
        {
            var oldValue = GetXAttributeValue();

            // if no change then just return
            if (newValue == oldValue)
            {
                return;
            }

            if (newValue == null)
            {
                RemoveFromXlinq();
            }
            else
            {
                if (XAttribute == null)
                {
                    AddToXlinq(newValue);
                }
                else
                {
                    XAttribute.Value = newValue;
                }
            }
        }

        internal static void SetXAttributeValue(XAttribute attribute, string newValue)
        {
            var attr = ModelItemAnnotation.GetModelItem(attribute) as EFAttribute;
            Debug.Assert(attr != null);
            if (attr != null)
            {
                attr.SetXAttributeValue(newValue);
            }
        }

        /// <summary>
        ///     NOTE: this is called from the EFObject c'tor so only that class is fully instantiated!
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        protected override void AddToXlinq()
        {
            AddToXlinq(String.Empty);
        }

        protected virtual void AddToXlinq(string attributeValue)
        {
            Debug.Assert(XObject == null, "Object already serialized");

            // sometimes we want to lazy create the attribute when it's first set
            if (!string.IsNullOrEmpty(EFTypeName))
            {
                Debug.Assert(
                    Parent != null && Parent.XContainer != null, "Can't serialize this if the Parent or it's XContainer is missing");

                XAttribute attribute = null;
                if (Namespace != null)
                {
                    attribute = new XAttribute(Namespace + EFTypeName, attributeValue);
                }
                else
                {
                    attribute = new XAttribute(EFTypeName, attributeValue);
                }

                SetXObject(attribute);
                ModelItemAnnotation.SetModelItem(attribute, this);

                // always append attributes
                Parent.XContainer.Add(attribute);
            }
        }

        protected override void RemoveFromXlinq()
        {
            if (XAttribute != null)
            {
                Debug.Assert(XAttribute.Parent != null);
                XAttribute.Remove();
                SetXObject(null);
            }
        }

        protected override void OnDelete(bool deleteXObject)
        {
            if (deleteXObject)
            {
                RemoveFromXlinq();
            }
        }
    }
}
