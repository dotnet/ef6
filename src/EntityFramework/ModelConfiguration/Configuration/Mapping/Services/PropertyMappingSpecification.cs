// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Configuration.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Diagnostics.Contracts;

    internal class PropertyMappingSpecification
    {
        private readonly EdmEntityType _entityType;
        private readonly IList<EdmProperty> _propertyPath;
        private readonly IList<DbColumnCondition> _conditions;
        private readonly bool _isDefaultDiscriminatorCondition;

        public PropertyMappingSpecification(
            EdmEntityType entityType,
            IList<EdmProperty> propertyPath,
            IList<DbColumnCondition> conditions,
            bool isDefaultDiscriminatorCondition)
        {
            Contract.Requires(entityType != null);

            _entityType = entityType;
            _propertyPath = propertyPath;
            _conditions = conditions;
            _isDefaultDiscriminatorCondition = isDefaultDiscriminatorCondition;
        }

        public EdmEntityType EntityType
        {
            get { return _entityType; }
        }

        public IList<EdmProperty> PropertyPath
        {
            get { return _propertyPath; }
        }

        public IList<DbColumnCondition> Conditions
        {
            get { return _conditions; }
        }

        public bool IsDefaultDiscriminatorCondition
        {
            get { return _isDefaultDiscriminatorCondition; }
        }
    }
}
