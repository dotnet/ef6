// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents information about a property of an entity.
    ///
    /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
    /// (such as the end user of an application). If input is accepted from such sources it should be validated 
    /// before being passed to these APIs to protect against SQL injection attacks etc.
    /// </summary>
    public abstract class PropertyModel
    {
        private readonly PrimitiveTypeKind _type;
        private TypeUsage _typeUsage;

        /// <summary>
        /// Initializes a new instance of the PropertyModel class.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="type"> The data type for this property model. </param>
        /// <param name="typeUsage"> Additional details about the data type. This includes details such as maximum length, nullability etc. </param>
        protected PropertyModel(PrimitiveTypeKind type, TypeUsage typeUsage)
        {
            _type = type;
            _typeUsage = typeUsage;
        }

        /// <summary>
        /// Gets the data type for this property model.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public virtual PrimitiveTypeKind Type
        {
            get { return _type; }
        }

        /// <summary>
        /// Gets additional details about the data type of this property model.
        /// This includes details such as maximum length, nullability etc.
        /// </summary>
        public TypeUsage TypeUsage
        {
            get { return _typeUsage ?? (_typeUsage = BuildTypeUsage()); }
        }

        /// <summary>
        /// Gets or sets the name of the property model.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets a provider specific data type to use for this property model.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        public virtual string StoreType { get; set; }

        /// <summary>
        /// Gets or sets the maximum length for this property model.
        /// Only valid for array data types.
        /// </summary>
        public virtual int? MaxLength { get; set; }

        /// <summary>
        /// Gets or sets the precision for this property model.
        /// Only valid for decimal data types.
        /// </summary>
        public virtual byte? Precision { get; set; }

        /// <summary>
        /// Gets or sets the scale for this property model.
        /// Only valid for decimal data types.
        /// </summary>
        public virtual byte? Scale { get; set; }

        /// <summary>
        /// Gets or sets a constant value to use as the default value for this property model.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        public virtual object DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets a SQL expression used as the default value for this property model.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        public virtual string DefaultValueSql { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if this property model is fixed length.
        /// Only valid for array data types.
        /// </summary>
        public virtual bool? IsFixedLength { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if this property model supports Unicode characters.
        /// Only valid for textual data types.
        /// </summary>
        public virtual bool? IsUnicode { get; set; }

        private TypeUsage BuildTypeUsage()
        {
            var primitiveType = PrimitiveType.GetEdmPrimitiveType(Type);

            if (Type == PrimitiveTypeKind.Binary)
            {
                if (MaxLength != null)
                {
                    return TypeUsage.CreateBinaryTypeUsage(
                        primitiveType,
                        IsFixedLength ?? false,
                        MaxLength.Value);
                }

                return TypeUsage.CreateBinaryTypeUsage(
                    primitiveType,
                    IsFixedLength ?? false);
            }

            if (Type == PrimitiveTypeKind.String)
            {
                if (MaxLength != null)
                {
                    return TypeUsage.CreateStringTypeUsage(
                        primitiveType,
                        IsUnicode ?? true,
                        IsFixedLength ?? false,
                        MaxLength.Value);
                }

                return TypeUsage.CreateStringTypeUsage(
                    primitiveType,
                    IsUnicode ?? true,
                    IsFixedLength ?? false);
            }

            if (Type == PrimitiveTypeKind.DateTime)
            {
                return TypeUsage.CreateDateTimeTypeUsage(primitiveType, Precision);
            }

            if (Type == PrimitiveTypeKind.DateTimeOffset)
            {
                return TypeUsage.CreateDateTimeOffsetTypeUsage(primitiveType, Precision);
            }

            if (Type == PrimitiveTypeKind.Decimal)
            {
                if ((Precision != null)
                    || (Scale != null))
                {
                    return TypeUsage.CreateDecimalTypeUsage(
                        primitiveType,
                        Precision ?? 18,
                        Scale ?? 0);
                }

                return TypeUsage.CreateDecimalTypeUsage(primitiveType);
            }

            return (Type == PrimitiveTypeKind.Time)
                       ? TypeUsage.CreateTimeTypeUsage(primitiveType, Precision)
                       : TypeUsage.CreateDefaultTypeUsage(primitiveType);
        }
    }
}
