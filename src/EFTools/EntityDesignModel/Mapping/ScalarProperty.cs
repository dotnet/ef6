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

    internal class ScalarProperty : PropertyMappingBase
    {
        internal static readonly string ElementName = "ScalarProperty";

        internal ScalarProperty(EFElement parent, XElement element)
            : base(parent, element)
        {
            Debug.Assert(
                parent is MappingFragment || parent is EndProperty || parent is ComplexProperty,
                "parent should be a " + typeof(MappingFragment).Name +
                " or an EndProperty or a ComplexProperty");
        }

        /// <summary>
        ///     This will return NULL if this is not in an EndProperty
        /// </summary>
        internal EndProperty EndProperty
        {
            get
            {
                var parent = Parent as EndProperty;
                return parent;
            }
        }

        /// <summary>
        ///     This will return NULL if this is not in an ComplexProperty
        /// </summary>
        internal ComplexProperty ComplexProperty
        {
            get { return Parent as ComplexProperty; }
        }

        internal override string DisplayName
        {
            get
            {
                return string.Format(
                    CultureInfo.CurrentCulture, "{0} <==> {1}",
                    Name.RefName,
                    ColumnName.RefName);
            }
        }

        protected override void DoResolve(EFArtifactSet artifactSet)
        {
            base.DoResolve(artifactSet);

            // a scalar property requires that both sides be Resolved
            if (Name.Status == BindingStatus.Known
                && ColumnName.Status == BindingStatus.Known)
            {
                State = EFElementState.Resolved;
            }
        }

        internal override DeleteEFElementCommand GetDeleteCommand()
        {
            DeleteEFElementCommand cmd = new DeleteScalarPropertyCommand(this);
            if (cmd == null)
            {
                // shouldn't happen, just to be safe
                throw new InvalidOperationException();
            }
            return cmd;
        }

        /// <summary>
        ///     Returns the top most ComplexProperty that this ScalarProperty lives in (null if the parent isn't a ComplexProperty)
        /// </summary>
        /// <returns></returns>
        internal ComplexProperty GetTopMostComplexProperty()
        {
            EFObject efObject = this;
            while (efObject.Parent is ComplexProperty)
            {
                efObject = efObject.Parent;
            }
            return efObject as ComplexProperty;
        }

        /// <summary>
        ///     Returns a list with all Properties that are mapped for this ScalarProperty mapping tree
        ///     (i.e. including all properties mapped by the parent ComplexProperties)
        /// </summary>
        /// <returns></returns>
        internal List<Property> GetMappedPropertiesList()
        {
            var properties = new List<Property>();
            properties.Add(Name.Target);
            var complexProperty = Parent as ComplexProperty;
            while (complexProperty != null)
            {
                properties.Insert(0, complexProperty.Name.Target);
                complexProperty = complexProperty.Parent as ComplexProperty;
            }

            return properties;
        }

        /// <summary>
        ///     Returns a list of all parent ComplexProperties (might be empty)
        /// </summary>
        /// <param name="orderDescending">If true, the resulting list will contain the root ComplexProperty, its child, etc.</param>
        /// <returns></returns>
        internal List<ComplexProperty> GetParentComplexProperties(bool orderDescending = true)
        {
            var properties = new List<ComplexProperty>();
            var complexProperty = Parent as ComplexProperty;
            while (complexProperty != null)
            {
                if (orderDescending)
                {
                    properties.Insert(0, complexProperty);
                }
                else
                {
                    properties.Add(complexProperty);
                }
                complexProperty = complexProperty.Parent as ComplexProperty;
            }

            return properties;
        }
    }
}
