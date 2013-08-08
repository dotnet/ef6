// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Globalization;

    internal sealed class FieldDescriptor : PropertyDescriptor
    {
        private readonly EdmProperty _property;
        private readonly Type _fieldType;
        private readonly Type _itemType;
        private readonly bool _isReadOnly;

        /// <summary>
        /// For testing purpuses only.
        /// </summary>
        internal FieldDescriptor(string propertyName)
            : base(propertyName, null)
        {
        }

        /// <summary>
        /// Construct a new instance of the FieldDescriptor class that describes a property
        /// on items of the supplied type.
        /// </summary>
        /// <param name="itemType"> Type of object whose property is described by this FieldDescriptor. </param>
        /// <param name="isReadOnly">
        /// <b>True</b> if property value on item can be modified; otherwise <b>false</b> .
        /// </param>
        /// <param name="property"> EdmProperty that describes the property on the item. </param>
        internal FieldDescriptor(Type itemType, bool isReadOnly, EdmProperty property)
            : base(property.Name, null)
        {
            _itemType = itemType;
            _property = property;
            _isReadOnly = isReadOnly;
            _fieldType = DetermineClrType(_property.TypeUsage);
            Debug.Assert(_fieldType != null, "FieldDescriptor's CLR type has unexpected value of null.");
        }

        /// <summary>
        /// Determine a CLR Type to use a property descriptro form an EDM TypeUsage
        /// </summary>
        /// <param name="typeUsage"> The EDM TypeUsage containing metadata about the type </param>
        /// <returns> A CLR type that represents that EDM type </returns>
        private Type DetermineClrType(TypeUsage typeUsage)
        {
            Type result = null;
            var edmType = typeUsage.EdmType;

            switch (edmType.BuiltInTypeKind)
            {
                case BuiltInTypeKind.EntityType:
                case BuiltInTypeKind.ComplexType:
                    result = edmType.ClrType;
                    break;

                case BuiltInTypeKind.RefType:
                    result = typeof(EntityKey);
                    break;

                case BuiltInTypeKind.CollectionType:
                    var elementTypeUse = ((CollectionType)edmType).TypeUsage;
                    result = DetermineClrType(elementTypeUse);
                    result = typeof(IEnumerable<>).MakeGenericType(result);
                    break;

                case BuiltInTypeKind.PrimitiveType:
                case BuiltInTypeKind.EnumType:
                    result = edmType.ClrType;
                    Facet nullable;
                    if (result.IsValueType
                        &&
                        typeUsage.Facets.TryGetValue(DbProviderManifest.NullableFacetName, false, out nullable)
                        && ((bool)nullable.Value))
                    {
                        result = typeof(Nullable<>).MakeGenericType(result);
                    }
                    break;

                case BuiltInTypeKind.RowType:
                    result = typeof(IDataRecord);
                    break;

                default:
                    Debug.Fail(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "The type {0} was not the expected scalar, enumeration, collection, structural, nominal, or reference type.",
                            edmType.GetType()));
                    break;
            }

            return result;
        }

        /// <summary>
        /// Get <see cref="EdmProperty" /> instance associated with this field descriptor.
        /// </summary>
        /// <value>
        /// The <see cref="EdmProperty" /> instance associated with this field descriptor, or null if there is no EDM property association.
        /// </value>
        internal EdmProperty EdmProperty
        {
            get { return _property; }
        }

        public override Type ComponentType
        {
            get { return _itemType; }
        }

        public override bool IsReadOnly
        {
            get { return _isReadOnly; }
        }

        public override Type PropertyType
        {
            get { return _fieldType; }
        }

        public override bool CanResetValue(object item)
        {
            return false;
        }

        public override object GetValue(object item)
        {
            Check.NotNull(item, "item");

            if (!_itemType.IsAssignableFrom(item.GetType()))
            {
                throw new ArgumentException(Strings.ObjectView_IncompatibleArgument);
            }

            object propertyValue;

            var dbDataRecord = item as DbDataRecord;
            if (dbDataRecord != null)
            {
                propertyValue = (dbDataRecord.GetValue(dbDataRecord.GetOrdinal(_property.Name)));
            }
            else
            {
                propertyValue = DelegateFactory.GetValue(_property, item);
            }

            return propertyValue;
        }

        public override void ResetValue(object item)
        {
            throw new NotSupportedException();
        }

        public override void SetValue(object item, object value)
        {
            Check.NotNull(item, "item");

            if (!_itemType.IsAssignableFrom(item.GetType()))
            {
                throw new ArgumentException(Strings.ObjectView_IncompatibleArgument);
            }
            if (!_isReadOnly)
            {
                DelegateFactory.SetValue(_property, item, value);
            } // if not entity it must be readonly
            else
            {
                throw new InvalidOperationException(Strings.ObjectView_WriteOperationNotAllowedOnReadOnlyBindingList);
            }
        }

        public override bool ShouldSerializeValue(object item)
        {
            return false;
        }

        public override bool IsBrowsable
        {
            get { return true; }
        }
    }
}
