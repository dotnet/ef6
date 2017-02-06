// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.SchemaObjectModel
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Globalization;
    using System.Xml;

    // <summary>
    // Summary description for Association.
    // </summary>
    [DebuggerDisplay(
        "Name={Name}, Relationship={_unresolvedRelationshipName}, FromRole={_unresolvedFromEndRole}, ToRole={_unresolvedToEndRole}")]
    internal sealed class NavigationProperty : Property
    {
        private string _unresolvedFromEndRole;
        private string _unresolvedToEndRole;
        private string _unresolvedRelationshipName;
        private IRelationshipEnd _fromEnd;
        private IRelationshipEnd _toEnd;
        private IRelationship _relationship;

        public NavigationProperty(SchemaEntityType parent)
            : base(parent)
        {
        }

        public new SchemaEntityType ParentElement
        {
            get { return base.ParentElement as SchemaEntityType; }
        }

        internal IRelationship Relationship
        {
            get { return _relationship; }
        }

        internal IRelationshipEnd ToEnd
        {
            get { return _toEnd; }
        }

        internal IRelationshipEnd FromEnd
        {
            get { return _fromEnd; }
        }

        // <summary>
        // Gets the Type of the property
        // </summary>
        public override SchemaType Type
        {
            get
            {
                if (_toEnd == null
                    || _toEnd.Type == null)
                {
                    return null;
                }

                return _toEnd.Type;
            }
        }

        protected override bool HandleAttribute(XmlReader reader)
        {
            if (base.HandleAttribute(reader))
            {
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.Relationship))
            {
                HandleAssociationAttribute(reader);
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.FromRole))
            {
                HandleFromRoleAttribute(reader);
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.ToRole))
            {
                HandleToRoleAttribute(reader);
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.ContainsTarget))
            {
                // EF does not support this EDM 3.0 attribute, so ignore it.
                return true;
            }

            return false;
        }

        internal override void ResolveTopLevelNames()
        {
            base.ResolveTopLevelNames();

            SchemaType element;
            if (!Schema.ResolveTypeName(this, _unresolvedRelationshipName, out element))
            {
                return;
            }

            _relationship = element as IRelationship;
            if (_relationship == null)
            {
                AddError(
                    ErrorCode.BadNavigationProperty, EdmSchemaErrorSeverity.Error,
                    Strings.BadNavigationPropertyRelationshipNotRelationship(_unresolvedRelationshipName));
                return;
            }

            var foundBothEnds = true;
            if (!_relationship.TryGetEnd(_unresolvedFromEndRole, out _fromEnd))
            {
                AddError(
                    ErrorCode.BadNavigationProperty, EdmSchemaErrorSeverity.Error,
                    Strings.BadNavigationPropertyUndefinedRole(_unresolvedFromEndRole, _relationship.FQName));
                foundBothEnds = false;
            }

            if (!_relationship.TryGetEnd(_unresolvedToEndRole, out _toEnd))
            {
                AddError(
                    ErrorCode.BadNavigationProperty, EdmSchemaErrorSeverity.Error,
                    Strings.BadNavigationPropertyUndefinedRole(_unresolvedToEndRole, _relationship.FQName));

                foundBothEnds = false;
            }

            if (foundBothEnds && _fromEnd == _toEnd)
            {
                AddError(
                    ErrorCode.BadNavigationProperty, EdmSchemaErrorSeverity.Error,
                    Strings.BadNavigationPropertyRolesCannotBeTheSame);
            }
        }

        internal override void Validate()
        {
            base.Validate();

            Debug.Assert(
                _fromEnd != null && _toEnd != null,
                "FromEnd and ToEnd must not be null in Validate. ResolveNames must have resolved it or added error");

            if (_fromEnd.Type != ParentElement)
            {
                AddError(
                    ErrorCode.BadNavigationProperty, EdmSchemaErrorSeverity.Error,
                    Strings.BadNavigationPropertyBadFromRoleType(
                        Name,
                        _fromEnd.Type.FQName, _fromEnd.Name, _relationship.FQName, ParentElement.FQName));
            }
        }

        #region Private Methods

        private void HandleToRoleAttribute(XmlReader reader)
        {
            _unresolvedToEndRole = HandleUndottedNameAttribute(reader, _unresolvedToEndRole);
        }

        private void HandleFromRoleAttribute(XmlReader reader)
        {
            _unresolvedFromEndRole = HandleUndottedNameAttribute(reader, _unresolvedFromEndRole);
        }

        private void HandleAssociationAttribute(XmlReader reader)
        {
            Debug.Assert(
                _unresolvedRelationshipName == null, string.Format(CultureInfo.CurrentCulture, "{0} is already defined", reader.Name));

            string association;
            if (!Utils.GetDottedName(Schema, reader, out association))
            {
                return;
            }

            _unresolvedRelationshipName = association;
        }

        #endregion
    }
}
