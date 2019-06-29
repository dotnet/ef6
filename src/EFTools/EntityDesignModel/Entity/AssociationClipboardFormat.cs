// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System;
    using System.Diagnostics;
    using System.Globalization;

    // Represents Association info stored in Clipboard
    [Serializable]
    internal class AssociationClipboardFormat : AnnotatableElementClipboardFormat
    {
        private readonly string _associationName;
        private readonly EntityTypeClipboardFormat _clipboardEntity1;
        private readonly EntityTypeClipboardFormat _clipboardEntity2;
        private readonly string _multiplicity1;
        private readonly string _multiplicity2;
        private readonly string _associationEndRole1;
        private readonly string _associationEndRole2;
        private readonly ReferentialConstraintClipboardFormat _referentialConstraint;

        internal AssociationClipboardFormat(
            Association association, EntityTypeClipboardFormat clipboardEntity1, EntityTypeClipboardFormat clipboardEntity2)
            : base(association)
        {
            _associationName = association.LocalName.Value;
            _clipboardEntity1 = clipboardEntity1;
            _clipboardEntity2 = clipboardEntity2;

            var associationEnds = association.AssociationEnds();
            Debug.Assert(
                associationEnds.Count == 2,
                String.Format(CultureInfo.CurrentCulture, "Invalid AssociationEnd counts for Association {0}", association.DisplayName));

            if (associationEnds.Count == 2)
            {
                _multiplicity1 = associationEnds[0].Multiplicity.Value;
                _multiplicity2 = associationEnds[1].Multiplicity.Value;
                _associationEndRole1 = associationEnds[0].Role.Value;
                _associationEndRole2 = associationEnds[1].Role.Value;
                if (association.ReferentialConstraint != null)
                {
                    _referentialConstraint = new ReferentialConstraintClipboardFormat(association.ReferentialConstraint);
                }
            }
            else
            {
                _multiplicity1 = String.Empty;
                _multiplicity2 = String.Empty;
                _associationEndRole1 = String.Empty;
                _associationEndRole2 = String.Empty;
            }
        }

        internal string AssociationName
        {
            get { return _associationName; }
        }

        internal EntityTypeClipboardFormat ClipboardEntity1
        {
            get { return _clipboardEntity1; }
        }

        internal EntityTypeClipboardFormat ClipboardEntity2
        {
            get { return _clipboardEntity2; }
        }

        internal string Multiplicity1
        {
            get { return _multiplicity1; }
        }

        internal string Multiplicity2
        {
            get { return _multiplicity2; }
        }

        internal string AssociationEndRole1
        {
            get { return _associationEndRole1; }
        }

        internal string AssociationEndRole2
        {
            get { return _associationEndRole2; }
        }

        internal ReferentialConstraintClipboardFormat ReferentialConstraint
        {
            get { return _referentialConstraint; }
        }
    }
}
