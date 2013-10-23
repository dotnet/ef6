// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System;
    using System.Collections.Generic;

    // Represents ReferentialConstraint info stored in Clipboard
    [Serializable]
    internal class ReferentialConstraintClipboardFormat : AnnotatableElementClipboardFormat
    {
        private readonly string _principalEntityName;
        private readonly string _principalRole;
        private readonly List<string> _principalProperties;
        private readonly string _dependentRole;
        private readonly List<string> _dependentProperties;

        public ReferentialConstraintClipboardFormat(ReferentialConstraint referentialConstraint)
            : base(referentialConstraint)
        {
            _principalRole = referentialConstraint.Principal.Role.RefName;
            _principalProperties = new List<string>(referentialConstraint.Principal.PropertyRefs.Count);
            foreach (var propertyRef in referentialConstraint.Principal.PropertyRefs)
            {
                _principalProperties.Add(propertyRef.Name.RefName);
                if (_principalEntityName == null
                    && propertyRef.Name.Target.EntityType != null)
                {
                    _principalEntityName = propertyRef.Name.Target.EntityType.LocalName.Value;
                }
            }
            _dependentRole = referentialConstraint.Dependent.Role.RefName;
            _dependentProperties = new List<string>(referentialConstraint.Dependent.PropertyRefs.Count);
            foreach (var propertyRef in referentialConstraint.Dependent.PropertyRefs)
            {
                _dependentProperties.Add(propertyRef.Name.RefName);
            }
        }

        public string PrincipalEntityName
        {
            get { return _principalEntityName; }
        }

        public string PrincipalRole
        {
            get { return _principalRole; }
        }

        public IList<string> PrincipalProperties
        {
            get { return _principalProperties; }
        }

        public string DependentRole
        {
            get { return _dependentRole; }
        }

        public IList<string> DependentProperties
        {
            get { return _dependentProperties; }
        }
    }
}
