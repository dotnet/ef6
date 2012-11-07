// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Linq;

    [DebuggerDisplay("{Column.Name}")]
    internal class ColumnMapping
    {
        private readonly EdmProperty _column;
        private readonly List<PropertyMappingSpecification> _propertyMappings;

        public ColumnMapping(EdmProperty column)
        {
            Contract.Requires(column != null);
            _column = column;
            _propertyMappings = new List<PropertyMappingSpecification>();
        }

        public EdmProperty Column
        {
            get { return _column; }
        }

        public IList<PropertyMappingSpecification> PropertyMappings
        {
            get { return _propertyMappings; }
        }

        public void AddMapping(
            EntityType entityType,
            IList<EdmProperty> propertyPath,
            IEnumerable<DbColumnCondition> conditions,
            bool isDefaultDiscriminatorCondition)
        {
            _propertyMappings.Add(
                new PropertyMappingSpecification(
                    entityType, propertyPath, conditions.ToList(), isDefaultDiscriminatorCondition));
        }
    }
}
