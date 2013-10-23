// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System;
    using System.Diagnostics;

    // Represents Property info stored in Clipboard
    [Serializable]
    internal class NavigationPropertyClipboardFormat : PropertyBaseClipboardFormat
    {
        private readonly string _relationship;
        private readonly string _fromRole;
        private readonly string _toRole;

        internal NavigationPropertyClipboardFormat(NavigationProperty property)
            : base(property)
        {
            _relationship = _fromRole = _toRole = String.Empty;
            Debug.Assert(
                property != null,
                "Null property is passed in to NavigationPropertyClipboardFormat constructor.");

            if (property != null)
            {
                // Navigation property might not have corresponding association.
                if (property.Relationship != null
                    && property.Relationship.Target != null)
                {
                    _relationship = property.Relationship.Target.LocalName.Value;

                    Debug.Assert(
                        property.FromRole != null,
                        "Navigation Property's FromRole attribute should not be null if the property's association is set. Property Name:"
                        + property.DisplayName + ", Association name: " + property.Relationship.ToPrettyString());

                    if (property.FromRole != null)
                    {
                        _fromRole = property.FromRole.RefName;
                    }

                    Debug.Assert(
                        property.ToRole != null,
                        "Navigation Property's ToRole attribute should not be null if the property's association is set. Property Name:"
                        + property.DisplayName + ", Association name: " + property.Relationship.ToPrettyString());

                    if (property.ToRole != null)
                    {
                        _toRole = property.ToRole.RefName;
                    }
                }
            }
        }

        internal string TraceString()
        {
            return "[" + typeof(NavigationPropertyClipboardFormat).Name +
                   " name=" + PropertyName +
                   ", getter=" + GetterAccessModifier +
                   ", setter=" + SetterAccessModifier +
                   ", relationship=" + _relationship +
                   ", fromRole=" + _fromRole +
                   ", toRole=" + _toRole +
                   "]";
        }

        [ClipboardPropertyMap(NavigationProperty.AttributeRelationship)]
        internal string Relationship
        {
            get { return _relationship; }
        }

        [ClipboardPropertyMap(NavigationProperty.AttributeFromRole)]
        internal string FromRole
        {
            get { return _fromRole; }
        }

        [ClipboardPropertyMap(NavigationProperty.AttributeToRole)]
        internal string ToRole
        {
            get { return _toRole; }
        }
    }
}
