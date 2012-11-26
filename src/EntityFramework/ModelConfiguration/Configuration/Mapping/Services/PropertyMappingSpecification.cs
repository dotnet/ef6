// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    
    using System.Diagnostics.Contracts;

    internal class PropertyMappingSpecification
    {
        private readonly EntityType _entityType;
        private readonly IList<EdmProperty> _propertyPath;
        private readonly IList<StorageConditionPropertyMapping> _conditions;
        private readonly bool _isDefaultDiscriminatorCondition;

        public PropertyMappingSpecification(
            EntityType entityType,
            IList<EdmProperty> propertyPath,
            IList<StorageConditionPropertyMapping> conditions,
            bool isDefaultDiscriminatorCondition)
        {
            Contract.Requires(entityType != null);

            _entityType = entityType;
            _propertyPath = propertyPath;
            _conditions = conditions;
            _isDefaultDiscriminatorCondition = isDefaultDiscriminatorCondition;
        }

        public EntityType EntityType
        {
            get { return _entityType; }
        }

        public IList<EdmProperty> PropertyPath
        {
            get { return _propertyPath; }
        }

        public IList<StorageConditionPropertyMapping> Conditions
        {
            get { return _conditions; }
        }

        public bool IsDefaultDiscriminatorCondition
        {
            get { return _isDefaultDiscriminatorCondition; }
        }
    }
}
