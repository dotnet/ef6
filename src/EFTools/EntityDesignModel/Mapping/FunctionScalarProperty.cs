// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class FunctionScalarProperty : EFElement
    {
        internal static readonly string ElementName = "ScalarProperty";

        internal static readonly string AttributeName = "Name";
        internal static readonly string AttributeParameterName = "ParameterName";
        internal static readonly string AttributeVersion = "Version";

        private SingleItemBinding<Property> _property;
        private SingleItemBinding<Parameter> _parameter;
        private DefaultableValue<string> _versionAttr;

        internal FunctionScalarProperty(EFElement parent, XElement element)
            : base(parent, element)
        {
            Debug.Assert(
                (parent as ModificationFunction) != null || parent is FunctionAssociationEnd || parent is FunctionComplexProperty,
                "parent should be a " + typeof(ModificationFunction).Name +
                " or a FunctionAssociationEnd or a FunctionComplexProperty");
        }

        internal override string EFTypeName
        {
            get { return ElementName; }
        }

        /// <summary>
        ///     NOTE: This will return null if the ScalarProperty is inside an AssociationEnd or ComplexProperty
        /// </summary>
        internal ModificationFunction ModificationFunction
        {
            get
            {
                var parent = Parent as ModificationFunction;
                return parent;
            }
        }

        /// <summary>
        ///     NOTE: This will return null if the ScalarProperty is inside a ModificationFunction or ComplexProperty
        /// </summary>
        internal FunctionAssociationEnd AssociationEnd
        {
            get
            {
                var parent = Parent as FunctionAssociationEnd;
                return parent;
            }
        }

        /// <summary>
        ///     NOTE: This will return null if the ScalarProperty is inside a ModificationFunction or AssociationEnd
        /// </summary>
        internal FunctionComplexProperty FunctionComplexProperty
        {
            get
            {
                var parent = Parent as FunctionComplexProperty;
                return parent;
            }
        }

        /// <summary>
        ///     Manages the content of the Name attribute
        /// </summary>
        internal SingleItemBinding<Property> Name
        {
            get
            {
                if (_property == null)
                {
                    _property = new SingleItemBinding<Property>(
                        this,
                        AttributeName,
                        FunctionPropertyMappingNameNormalizer.NameNormalizer
                        );
                }
                return _property;
            }
        }

        /// <summary>
        ///     Manages the content of the ParameterName attribute
        /// </summary>
        internal SingleItemBinding<Parameter> ParameterName
        {
            get
            {
                if (_parameter == null)
                {
                    _parameter = new SingleItemBinding<Parameter>(
                        this,
                        AttributeParameterName,
                        ParameterNameNormalizer.NameNormalizer
                        );
                }
                return _parameter;
            }
        }

        /// <summary>
        ///     Manages the content of the Version attribute
        /// </summary>
        internal DefaultableValue<string> Version
        {
            get
            {
                if (_versionAttr == null)
                {
                    _versionAttr = new VersionDefaultableValue(this);
                }
                return _versionAttr;
            }
        }

        private class VersionDefaultableValue : DefaultableValue<string>
        {
            internal VersionDefaultableValue(EFElement parent)
                : base(parent, AttributeVersion)
            {
            }

            internal override string AttributeName
            {
                get { return AttributeVersion; }
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
            s.Add(AttributeName);
            s.Add(AttributeParameterName);
            s.Add(AttributeVersion);
            return s;
        }
#endif

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
                yield return Name;
                yield return ParameterName;
                yield return Version;
            }
        }

        protected override void PreParse()
        {
            Debug.Assert(State != EFElementState.Parsed, "this object should not already be in the parsed state");

            ClearEFObject(_property);
            _property = null;

            ClearEFObject(_parameter);
            _parameter = null;

            ClearEFObject(_versionAttr);
            _versionAttr = null;

            base.PreParse();
        }

        internal override string DisplayName
        {
            get
            {
                return string.Format(
                    CultureInfo.CurrentCulture, "{0} <==> {1}",
                    Name.RefName,
                    ParameterName.RefName);
            }
        }

        protected override void DoResolve(EFArtifactSet artifactSet)
        {
            Name.Rebind();
            ParameterName.Rebind();

            if (Name.Status == BindingStatus.Known
                && ParameterName.Status == BindingStatus.Known)
            {
                State = EFElementState.Resolved;
            }
        }

        internal override DeleteEFElementCommand GetDeleteCommand()
        {
            var cmd = new DeleteFunctionScalarPropertyCommand(this);
            if (cmd == null)
            {
                // shouldn't happen, just to be safe
                throw new InvalidOperationException();
            }
            return cmd;
        }

        /// <summary>
        ///     Returns a list with all Properties that are mapped for this FunctionScalarProperty mapping tree
        ///     (i.e. including all properties mapped by the parent FunctionComplexProperties)
        /// </summary>
        /// <returns></returns>
        internal List<Property> GetMappedPropertiesList()
        {
            var properties = new List<Property>();
            properties.Add(Name.Target);
            var fcp = Parent as FunctionComplexProperty;
            while (fcp != null)
            {
                properties.Insert(0, fcp.Name.Target);
                fcp = fcp.Parent as FunctionComplexProperty;
            }

            return properties;
        }
    }
}
