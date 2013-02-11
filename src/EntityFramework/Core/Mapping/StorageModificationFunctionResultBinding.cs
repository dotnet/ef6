// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Globalization;

    /// <summary>
    ///     Defines a binding from a named result set column to a member taking the value.
    /// </summary>
    internal sealed class StorageModificationFunctionResultBinding
    {
        internal StorageModificationFunctionResultBinding(string columnName, EdmProperty property)
        {
            DebugCheck.NotNull(columnName);
            DebugCheck.NotNull(property);

            ColumnName = columnName;
            Property = property;
        }

        /// <summary>
        ///     Gets the name of the column to bind from the function result set. We use a string
        ///     value rather than EdmMember, since there is no metadata for function result sets.
        /// </summary>
        internal string ColumnName;

        /// <summary>
        ///     Gets the property to be set on the entity.
        /// </summary>
        internal readonly EdmProperty Property;

        public override string ToString()
        {
            return String.Format(
                CultureInfo.InvariantCulture,
                "{0}->{1}", ColumnName, Property);
        }
    }
}
