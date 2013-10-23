// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Validation;

    internal class Condition : PropertyMappingBase
    {
        internal static readonly string ElementName = "Condition";
        internal static readonly string AttributeIsNull = "IsNull";
        internal static readonly string AttributeValue = "Value";
        internal static readonly string IsNullConstant = "true";
        internal static readonly string IsNotNullConstant = "false";

        private DefaultableValue<string> _isNullAttr;
        private DefaultableValue<string> _valueAttr;

        internal Condition(EFElement parent, XElement element)
            : base(parent, element)
        {
            Debug.Assert(
                parent is MappingFragment || parent is AssociationSetMapping || parent is ComplexProperty || parent == null,
                "parent should be a MappingFragment or an AssociationSetMapping or a ComplexProperty (or null for testing)");
        }

        /// <summary>
        ///     This will return NULL if this is not in an AssociationSetMapping
        /// </summary>
        internal AssociationSetMapping AssociationSetMapping
        {
            get
            {
                var parent = Parent as AssociationSetMapping;
                return parent;
            }
        }

        /// <summary>
        ///     Returns the bool value of the IsNull attribute
        /// </summary>
        internal DefaultableValue<string> IsNull
        {
            get
            {
                if (_isNullAttr == null)
                {
                    _isNullAttr = new IsNullDefaultableValue(this);
                }
                return _isNullAttr;
            }
        }

        private class IsNullDefaultableValue : DefaultableValue<string>
        {
            internal IsNullDefaultableValue(EFElement parent)
                : base(parent, AttributeIsNull)
            {
            }

            internal override string AttributeName
            {
                get { return AttributeIsNull; }
            }

            public override string DefaultValue
            {
                get { return null; }
            }
        }

        /// <summary>
        ///     Returns the string value of the Value attribute
        /// </summary>
        internal DefaultableValue<string> Value
        {
            get
            {
                if (_valueAttr == null)
                {
                    _valueAttr = new ValueDefaultableValue(this);
                }
                return _valueAttr;
            }
        }

        private class ValueDefaultableValue : DefaultableValue<string>
        {
            internal ValueDefaultableValue(EFElement parent)
                : base(parent, AttributeValue)
            {
            }

            internal override string AttributeName
            {
                get { return AttributeValue; }
            }

            public override string DefaultValue
            {
                get { return null; }
            }
        }

#if DEBUG
        internal override ICollection<string> MyAttributeNames()
        {
            var s = base.MyAttributeNames();
            s.Add(AttributeIsNull);
            s.Add(AttributeValue);
            return s;
        }
#endif

        protected override void PreParse()
        {
            Debug.Assert(State != EFElementState.Parsed, "this object should not already be in the parsed state");

            ClearEFObject(_isNullAttr);
            _isNullAttr = null;
            ClearEFObject(_valueAttr);
            _valueAttr = null;

            base.PreParse();
        }

        private string DisplayNameInternal(bool localize)
        {
            string resource;
            if (localize)
            {
                resource = Resources.MappingModel_ConditionDisplayName;
            }
            else
            {
                resource = "Condition on {0}";
            }

            if (Name.RefName != null)
            {
                return string.Format(
                    CultureInfo.CurrentCulture,
                    resource,
                    Name.RefName);
            }
            else if (ColumnName.RefName != null)
            {
                return string.Format(
                    CultureInfo.CurrentCulture,
                    resource,
                    ColumnName.RefName);
            }
            else if (localize)
            {
                return base.DisplayName;
            }
            else
            {
                return base.NonLocalizedDisplayName;
            }
        }

        internal override string DisplayName
        {
            get { return DisplayNameInternal(true); }
        }

        internal override string NonLocalizedDisplayName
        {
            get { return DisplayNameInternal(false); }
        }

        protected override void DoResolve(EFArtifactSet artifactSet)
        {
            base.DoResolve(artifactSet);

            // a given condition can only have mapping to a property (E-Space) or a column (S-Space)
            // but not to both
            if (Name.Status == BindingStatus.Known
                && ColumnName.Status == BindingStatus.Known)
            {
                var errorInfo = new ErrorInfo(
                    ErrorInfo.Severity.ERROR, Resources.RESOLVE_CONDITION_BOUND_TO_PROP_AND_COLUMN, this,
                    ErrorCodes.RESOLVE_CONDITION_BOUND_TO_PROP_AND_COLUMN, ErrorClass.ResolveError);
                artifactSet.AddError(errorInfo);
            }
            else if (Name.Status == BindingStatus.Known
                     || ColumnName.Status == BindingStatus.Known)
            {
                State = EFElementState.Resolved;
            }
        }

        internal override DeleteEFElementCommand GetDeleteCommand()
        {
            DeleteEFElementCommand cmd = new DeleteConditionCommand(this);
            if (cmd == null)
            {
                // shouldn't happen, just to be safe
                throw new InvalidOperationException();
            }
            return cmd;
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
                yield return IsNull;
                yield return Value;
            }
        }
    }
}
