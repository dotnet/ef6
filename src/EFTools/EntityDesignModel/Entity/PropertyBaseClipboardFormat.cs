// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;

    [Serializable]
    internal abstract class PropertyBaseClipboardFormat : AnnotatableElementClipboardFormat
    {
        private string _propertyName;
        private string _setter;
        private readonly string _getter;

        public PropertyBaseClipboardFormat(PropertyBase property)
            : base(property)
        {
            _propertyName = property.LocalName.Value;
            _setter = property.Setter.IsDefaulted ? String.Empty : property.Setter.Value;
            _getter = property.Getter.IsDefaulted ? String.Empty : property.Getter.Value;
            ValidateAttributes(property);
        }

        // Validate the Property attributes members are represented.
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "property")]
        private void ValidateAttributes(PropertyBase property)
        {
#if DEBUG
            ICollection<string> attributesCollection = new List<string>();
            foreach (var propertyInfo in GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                var attributes =
                    propertyInfo.GetCustomAttributes(typeof(ClipboardPropertyMapAttribute), false) as ClipboardPropertyMapAttribute[];
                if (attributes != null
                    && attributes.Length > 0)
                {
                    Debug.Assert(
                        attributes.Length == 1,
                        String.Format(
                            CultureInfo.CurrentCulture, "There exist more than 1 instance of {0} for {1}", "ClipboardPropertyMapAttribute",
                            propertyInfo.Name));
                    Debug.Assert(
                        !attributes[0].IsExcluded,
                        String.Format(CultureInfo.CurrentCulture, "Attribute {0} is misplaced.", attributes[0].AttributeName));
                    attributesCollection.Add(attributes[0].AttributeName);
                }
            }

            var excludedAttributes =
                GetType().GetCustomAttributes(typeof(ClipboardPropertyMapAttribute), false) as ClipboardPropertyMapAttribute[];
            if (excludedAttributes != null)
            {
                foreach (var excludedAttribute in excludedAttributes)
                {
                    Debug.Assert(
                        excludedAttribute.IsExcluded,
                        String.Format(CultureInfo.CurrentCulture, "Attribute {0} is misplaced.", excludedAttribute.AttributeName));
                    attributesCollection.Add(excludedAttribute.AttributeName);
                }
            }

            foreach (var attributeName in property.MyAttributeNames())
            {
                Debug.Assert(
                    attributesCollection.Contains(attributeName),
                    String.Format(CultureInfo.CurrentCulture, "Clipboard format for {0} does not exist", attributeName));
            }
#endif
        }

        [ClipboardPropertyMap(PropertyBase.AttributeGetter)]
        internal string GetterAccessModifier
        {
            get { return _getter; }
            set { _setter = value; }
        }

        [ClipboardPropertyMap(PropertyBase.AttributeSetter)]
        internal string SetterAccessModifier
        {
            get { return _setter; }
            set { _setter = value; }
        }

        [ClipboardPropertyMap(EFNameableItem.AttributeName)]
        internal string PropertyName
        {
            get { return _propertyName; }
            set { _propertyName = value; }
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
    internal sealed class ClipboardPropertyMapAttribute : Attribute
    {
        private readonly string _attributeName;
        private readonly bool _isExcluded;

        internal ClipboardPropertyMapAttribute(string attributeName)
            : this(attributeName, false)
        {
        }

        internal ClipboardPropertyMapAttribute(string attributeName, bool isExcluded)
        {
            _attributeName = attributeName;
            _isExcluded = isExcluded;
        }

        internal string AttributeName
        {
            get { return _attributeName; }
        }

        internal bool IsExcluded
        {
            get { return _isExcluded; }
        }
    }
}
