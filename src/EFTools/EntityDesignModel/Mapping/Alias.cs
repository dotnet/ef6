// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class Alias : EFElement
    {
        internal static readonly string ElementName = "Alias";
        internal static readonly string AttributeKey = "Key";
        internal static readonly string AttributeValue = "Value";

        private DefaultableValue<string> _keyAttr;
        private SingleItemBinding<BaseEntityModel> _value;

        internal Alias(EFElement parent, XElement element)
            : base(parent, element)
        {
            Debug.Assert(parent is MappingModel, "parent should be a MappingModel");
        }

        internal MappingModel MappingModel
        {
            get
            {
                var parent = Parent as MappingModel;
                Debug.Assert(parent != null, "this.Parent should be a MappingModel");
                return parent;
            }
        }

        /// <summary>
        ///     Manages the content of the Key attribute
        /// </summary>
        internal DefaultableValue<string> Key
        {
            get
            {
                if (_keyAttr == null)
                {
                    _keyAttr = new KeyDefaultableValue(this);
                }
                return _keyAttr;
            }
        }

        private class KeyDefaultableValue : DefaultableValue<string>
        {
            internal KeyDefaultableValue(EFElement parent)
                : base(parent, AttributeKey)
            {
            }

            internal override string AttributeName
            {
                get { return AttributeKey; }
            }

            public override string DefaultValue
            {
                get { return String.Empty; }
            }
        }

        /// <summary>
        ///     A bindable reference to the EntityModel pointed to by this alias
        /// </summary>
        internal SingleItemBinding<BaseEntityModel> Value
        {
            get
            {
                if (_value == null)
                {
                    _value = new SingleItemBinding<BaseEntityModel>(
                        this,
                        AttributeValue,
                        null);
                }
                return _value;
            }
        }

#if DEBUG
        internal override ICollection<string> MyAttributeNames()
        {
            var s = base.MyAttributeNames();
            s.Add(AttributeKey);
            s.Add(AttributeValue);
            return s;
        }
#endif

        protected override void PreParse()
        {
            Debug.Assert(State != EFElementState.Parsed, "this object should not already be in the parsed state");

            ClearEFObject(_keyAttr);
            _keyAttr = null;
            ClearEFObject(_value);
            _value = null;
            base.PreParse();
        }

        protected override void PostParse(ICollection<XName> unprocessedElements)
        {
            if (String.IsNullOrEmpty(GetAttributeValue(AttributeKey)))
            {
                Artifact.AddParseErrorForObject(
                    this, Resources.ModelParse_AliasElementMissingKeyAttribute, ErrorCodes.ModelParse_AliasElementMissingKeyAttribute);
            }

            if (String.IsNullOrEmpty(GetAttributeValue(AttributeValue)))
            {
                Artifact.AddParseErrorForObject(
                    this, Resources.ModelParse_AliasElementMissingValueAttribute, ErrorCodes.ModelParse_AliasElementMissingValueAttribute);
            }

            base.PostParse(unprocessedElements);
        }

        internal override string DisplayName
        {
            get { return DisplayNameInternal(true); }
        }

        internal override string NonLocalizedDisplayName
        {
            get { return DisplayNameInternal(false); }
        }

        private string DisplayNameInternal(bool localize)
        {
            string resource;
            if (localize)
            {
                resource = Resources.MappingModel_AliasDisplayName;
            }
            else
            {
                resource = "{0}:{1}";
            }
            return string.Format(
                CultureInfo.CurrentCulture,
                resource, Key.Value, Value.RefName);
        }

        protected override void DoResolve(EFArtifactSet artifactSet)
        {
            Value.Rebind();
            if (Value.Status == BindingStatus.Known)
            {
                State = EFElementState.Resolved;
            }
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
                yield return Key;
                yield return Value;
            }
        }
    }
}
