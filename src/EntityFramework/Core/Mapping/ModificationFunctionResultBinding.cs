// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Globalization;

    /// <summary>
    /// Defines a binding from a named result set column to a member taking the value.
    /// </summary>
    public sealed class ModificationFunctionResultBinding : MappingItem
    {
        private string _columnName;
        private readonly EdmProperty _property;

        /// <summary>
        /// Initializes a new ModificationFunctionResultBinding instance.
        /// </summary>
        /// <param name="columnName">The name of the column to bind from the function result set.</param>
        /// <param name="property">The property to be set on the entity.</param>
        public ModificationFunctionResultBinding(string columnName, EdmProperty property)
        {
            Check.NotNull(columnName, "columnName");
            Check.NotNull(property, "property");

            _columnName = columnName;
            _property = property;
        }

        /// <summary>
        /// Gets the name of the column to bind from the function result set. 
        /// </summary>
        // We use a string value rather than EdmMember, since there is no metadata for function result sets.
        public string ColumnName
        {
            get { return _columnName;  }

            internal set
            {
                DebugCheck.NotNull(value);
                Debug.Assert(!IsReadOnly);

                _columnName = value;
            }
        }

        /// <summary>
        /// Gets the property to be set on the entity.
        /// </summary>
        public EdmProperty Property
        {
            get { return _property;  }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return String.Format(
                CultureInfo.InvariantCulture,
                "{0}->{1}", ColumnName, Property);
        }
    }
}
