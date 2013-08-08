// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Builders
{
    using System.ComponentModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Spatial;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Helper class that is used to configure a parameter.
    ///
    /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
    /// (such as the end user of an application). If input is accepted from such sources it should be validated 
    /// before being passed to these APIs to protect against SQL injection attacks etc.
    /// </summary>
    public class ParameterBuilder
    {
        /// <summary>
        /// Creates a new parameter definition to pass Binary data.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="maxLength"> The maximum allowable length of the array data. </param>
        /// <param name="fixedLength"> Value indicating whether or not all data should be padded to the maximum length. </param>
        /// <param name="defaultValue"> Constant value to use as the default value for this parameter. </param>
        /// <param name="defaultValueSql"> SQL expression used as the default value for this parameter. </param>
        /// <param name="name"> The name of the parameter. </param>
        /// <param name="storeType"> Provider specific data type to use for this parameter. </param>
        /// <param name="outParameter"> </param>
        /// <returns> The newly constructed parameter definition. </returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1614:ElementParameterDocumentationMustHaveText")]
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ParameterModel Binary(
            int? maxLength = null,
            bool? fixedLength = null,
            byte[] defaultValue = null,
            string defaultValueSql = null,
            string name = null,
            string storeType = null,
            bool outParameter = false)
        {
            return BuildParameter(
                PrimitiveTypeKind.Binary,
                defaultValue,
                defaultValueSql,
                maxLength,
                fixedLength: fixedLength,
                name: name,
                storeType: storeType,
                outParameter: outParameter);
        }

        /// <summary>
        /// Creates a new parameter definition to pass Boolean data.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="defaultValue"> Constant value to use as the default value for this parameter. </param>
        /// <param name="defaultValueSql"> SQL expression used as the default value for this parameter. </param>
        /// <param name="name"> The name of the parameter. </param>
        /// <param name="storeType"> Provider specific data type to use for this parameter. </param>
        /// <returns> The newly constructed parameter definition. </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ParameterModel Boolean(
            bool? defaultValue = null,
            string defaultValueSql = null,
            string name = null,
            string storeType = null,
            bool outParameter = false)
        {
            return BuildParameter(
                PrimitiveTypeKind.Boolean,
                defaultValue,
                defaultValueSql,
                name: name,
                storeType: storeType,
                outParameter: outParameter);
        }

        /// <summary>
        /// Creates a new parameter definition to pass Byte data.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="defaultValue"> Constant value to use as the default value for this parameter. </param>
        /// <param name="defaultValueSql"> SQL expression used as the default value for this parameter. </param>
        /// <param name="name"> The name of the parameter. </param>
        /// <param name="storeType"> Provider specific data type to use for this parameter. </param>
        /// <returns> The newly constructed parameter definition. </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ParameterModel Byte(
            byte? defaultValue = null,
            string defaultValueSql = null,
            string name = null,
            string storeType = null,
            bool outParameter = false)
        {
            return BuildParameter(
                PrimitiveTypeKind.Byte,
                defaultValue,
                defaultValueSql,
                name: name,
                storeType: storeType,
                outParameter: outParameter);
        }

        /// <summary>
        /// Creates a new parameter definition to pass DateTime data.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="precision"> The precision of the parameter. </param>
        /// <param name="defaultValue"> Constant value to use as the default value for this parameter. </param>
        /// <param name="defaultValueSql"> SQL expression used as the default value for this parameter. </param>
        /// <param name="name"> The name of the parameter. </param>
        /// <param name="storeType"> Provider specific data type to use for this parameter. </param>
        /// <returns> The newly constructed parameter definition. </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ParameterModel DateTime(
            byte? precision = null,
            DateTime? defaultValue = null,
            string defaultValueSql = null,
            string name = null,
            string storeType = null,
            bool outParameter = false)
        {
            return BuildParameter(
                PrimitiveTypeKind.DateTime,
                defaultValue,
                defaultValueSql,
                precision: precision,
                name: name,
                storeType: storeType,
                outParameter: outParameter);
        }

        /// <summary>
        /// Creates a new parameter definition to pass Decimal data.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="precision"> The numeric precision of the parameter. </param>
        /// <param name="scale"> The numeric scale of the parameter. </param>
        /// <param name="defaultValue"> Constant value to use as the default value for this parameter. </param>
        /// <param name="defaultValueSql"> SQL expression used as the default value for this parameter. </param>
        /// <param name="name"> The name of the parameter. </param>
        /// <param name="storeType"> Provider specific data type to use for this parameter. </param>
        /// <returns> The newly constructed parameter definition. </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ParameterModel Decimal(
            byte? precision = null,
            byte? scale = null,
            decimal? defaultValue = null,
            string defaultValueSql = null,
            string name = null,
            string storeType = null,
            bool outParameter = false)
        {
            return BuildParameter(
                PrimitiveTypeKind.Decimal,
                defaultValue,
                defaultValueSql,
                precision: precision,
                scale: scale,
                name: name,
                storeType: storeType,
                outParameter: outParameter);
        }

        /// <summary>
        /// Creates a new parameter definition to pass Double data.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="defaultValue"> Constant value to use as the default value for this parameter. </param>
        /// <param name="defaultValueSql"> SQL expression used as the default value for this parameter. </param>
        /// <param name="name"> The name of the parameter. </param>
        /// <param name="storeType"> Provider specific data type to use for this parameter. </param>
        /// <returns> The newly constructed parameter definition. </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ParameterModel Double(
            double? defaultValue = null,
            string defaultValueSql = null,
            string name = null,
            string storeType = null,
            bool outParameter = false)
        {
            return BuildParameter(
                PrimitiveTypeKind.Double,
                defaultValue,
                defaultValueSql,
                name: name,
                storeType: storeType,
                outParameter: outParameter);
        }

        /// <summary>
        /// Creates a new parameter definition to pass GUID data.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="defaultValue"> Constant value to use as the default value for this parameter. </param>
        /// <param name="defaultValueSql"> SQL expression used as the default value for this parameter. </param>
        /// <param name="name"> The name of the parameter. </param>
        /// <param name="storeType"> Provider specific data type to use for this parameter. </param>
        /// <returns> The newly constructed parameter definition. </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ParameterModel Guid(
            Guid? defaultValue = null,
            string defaultValueSql = null,
            string name = null,
            string storeType = null,
            bool outParameter = false)
        {
            return BuildParameter(
                PrimitiveTypeKind.Guid,
                defaultValue,
                defaultValueSql,
                name: name,
                storeType: storeType,
                outParameter: outParameter);
        }

        /// <summary>
        /// Creates a new parameter definition to pass Single data.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="defaultValue"> Constant value to use as the default value for this parameter. </param>
        /// <param name="defaultValueSql"> SQL expression used as the default value for this parameter. </param>
        /// <param name="name"> The name of the parameter. </param>
        /// <param name="storeType"> Provider specific data type to use for this parameter. </param>
        /// <returns> The newly constructed parameter definition. </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ParameterModel Single(
            float? defaultValue = null,
            string defaultValueSql = null,
            string name = null,
            string storeType = null,
            bool outParameter = false)
        {
            return BuildParameter(
                PrimitiveTypeKind.Single,
                defaultValue,
                defaultValueSql,
                name: name,
                storeType: storeType,
                outParameter: outParameter);
        }

        /// <summary>
        /// Creates a new parameter definition to pass Short data.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="defaultValue"> Constant value to use as the default value for this parameter. </param>
        /// <param name="defaultValueSql"> SQL expression used as the default value for this parameter. </param>
        /// <param name="name"> The name of the parameter. </param>
        /// <param name="storeType"> Provider specific data type to use for this parameter. </param>
        /// <returns> The newly constructed parameter definition. </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ParameterModel Short(
            short? defaultValue = null,
            string defaultValueSql = null,
            string name = null,
            string storeType = null,
            bool outParameter = false)
        {
            return BuildParameter(
                PrimitiveTypeKind.Int16,
                defaultValue,
                defaultValueSql,
                name: name,
                storeType: storeType,
                outParameter: outParameter);
        }

        /// <summary>
        /// Creates a new parameter definition to pass Integer data.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="defaultValue"> Constant value to use as the default value for this parameter. </param>
        /// <param name="defaultValueSql"> SQL expression used as the default value for this parameter. </param>
        /// <param name="name"> The name of the parameter. </param>
        /// <param name="storeType"> Provider specific data type to use for this parameter. </param>
        /// <returns> The newly constructed parameter definition. </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ParameterModel Int(
            int? defaultValue = null,
            string defaultValueSql = null,
            string name = null,
            string storeType = null,
            bool outParameter = false)
        {
            return BuildParameter(
                PrimitiveTypeKind.Int32,
                defaultValue,
                defaultValueSql,
                name: name,
                storeType: storeType,
                outParameter: outParameter);
        }

        /// <summary>
        /// Creates a new parameter definition to pass Long data.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="defaultValue"> Constant value to use as the default value for this parameter. </param>
        /// <param name="defaultValueSql"> SQL expression used as the default value for this parameter. </param>
        /// <param name="name"> The name of the parameter. </param>
        /// <param name="storeType"> Provider specific data type to use for this parameter. </param>
        /// <returns> The newly constructed parameter definition. </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ParameterModel Long(
            long? defaultValue = null,
            string defaultValueSql = null,
            string name = null,
            string storeType = null,
            bool outParameter = false)
        {
            return BuildParameter(
                PrimitiveTypeKind.Int64,
                defaultValue,
                defaultValueSql,
                name: name,
                storeType: storeType,
                outParameter: outParameter);
        }

        /// <summary>
        /// Creates a new parameter definition to pass String data.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="maxLength"> The maximum allowable length of the string data. </param>
        /// <param name="fixedLength"> Value indicating whether or not all data should be padded to the maximum length. </param>
        /// <param name="unicode"> Value indicating whether or not the parameter supports Unicode content. </param>
        /// <param name="defaultValue"> Constant value to use as the default value for this parameter. </param>
        /// <param name="defaultValueSql"> SQL expression used as the default value for this parameter. </param>
        /// <param name="name"> The name of the parameter. </param>
        /// <param name="storeType"> Provider specific data type to use for this parameter. </param>
        /// <returns> The newly constructed parameter definition. </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ParameterModel String(
            int? maxLength = null,
            bool? fixedLength = null,
            bool? unicode = null,
            string defaultValue = null,
            string defaultValueSql = null,
            string name = null,
            string storeType = null,
            bool outParameter = false)
        {
            return BuildParameter(
                PrimitiveTypeKind.String,
                defaultValue,
                defaultValueSql,
                maxLength,
                fixedLength: fixedLength,
                unicode: unicode,
                name: name,
                storeType: storeType,
                outParameter: outParameter);
        }

        /// <summary>
        /// Creates a new parameter definition to pass Time data.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="precision"> The precision of the parameter. </param>
        /// <param name="defaultValue"> Constant value to use as the default value for this parameter. </param>
        /// <param name="defaultValueSql"> SQL expression used as the default value for this parameter. </param>
        /// <param name="name"> The name of the parameter. </param>
        /// <param name="storeType"> Provider specific data type to use for this parameter. </param>
        /// <returns> The newly constructed parameter definition. </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ParameterModel Time(
            byte? precision = null,
            TimeSpan? defaultValue = null,
            string defaultValueSql = null,
            string name = null,
            string storeType = null,
            bool outParameter = false)
        {
            return BuildParameter(
                PrimitiveTypeKind.Time,
                defaultValue,
                defaultValueSql,
                precision: precision,
                name: name,
                storeType: storeType,
                outParameter: outParameter);
        }

        /// <summary>
        /// Creates a new parameter definition to pass DateTimeOffset data.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="precision"> The precision of the parameter. </param>
        /// <param name="defaultValue"> Constant value to use as the default value for this parameter. </param>
        /// <param name="defaultValueSql"> SQL expression used as the default value for this parameter. </param>
        /// <param name="name"> The name of the parameter. </param>
        /// <param name="storeType"> Provider specific data type to use for this parameter. </param>
        /// <returns> The newly constructed parameter definition. </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ParameterModel DateTimeOffset(
            byte? precision = null,
            DateTimeOffset? defaultValue = null,
            string defaultValueSql = null,
            string name = null,
            string storeType = null,
            bool outParameter = false)
        {
            return BuildParameter(
                PrimitiveTypeKind.DateTimeOffset,
                defaultValue,
                defaultValueSql,
                precision: precision,
                name: name,
                storeType: storeType,
                outParameter: outParameter);
        }

        /// <summary>
        /// Creates a new parameter definition to pass geography data.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="defaultValue"> Constant value to use as the default value for this parameter. </param>
        /// <param name="defaultValueSql"> SQL expression used as the default value for this parameter. </param>
        /// <param name="name"> The name of the parameter. </param>
        /// <param name="storeType"> Provider specific data type to use for this parameter. </param>
        /// <returns> The newly constructed parameter definition. </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ParameterModel Geography(
            DbGeography defaultValue = null,
            string defaultValueSql = null,
            string name = null,
            string storeType = null,
            bool outParameter = false)
        {
            return BuildParameter(
                PrimitiveTypeKind.Geography,
                defaultValue,
                defaultValueSql,
                name: name,
                storeType: storeType,
                outParameter: outParameter);
        }

        /// <summary>
        /// Creates a new parameter definition to pass geometry data.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="defaultValue"> Constant value to use as the default value for this parameter. </param>
        /// <param name="defaultValueSql"> SQL expression used as the default value for this parameter. </param>
        /// <param name="name"> The name of the parameter. </param>
        /// <param name="storeType"> Provider specific data type to use for this parameter. </param>
        /// <returns> The newly constructed parameter definition. </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ParameterModel Geometry(
            DbGeometry defaultValue = null,
            string defaultValueSql = null,
            string name = null,
            string storeType = null,
            bool outParameter = false)
        {
            return BuildParameter(
                PrimitiveTypeKind.Geometry,
                defaultValue,
                defaultValueSql,
                name: name,
                storeType: storeType,
                outParameter: outParameter);
        }

        private static ParameterModel BuildParameter(
            PrimitiveTypeKind primitiveTypeKind,
            object defaultValue,
            string defaultValueSql = null,
            int? maxLength = null,
            byte? precision = null,
            byte? scale = null,
            bool? unicode = null,
            bool? fixedLength = null,
            string name = null,
            string storeType = null,
            bool outParameter = false)
        {
            var parameter
                = new ParameterModel(primitiveTypeKind)
                      {
                          MaxLength = maxLength,
                          Precision = precision,
                          Scale = scale,
                          IsUnicode = unicode,
                          IsFixedLength = fixedLength,
                          DefaultValue = defaultValue,
                          DefaultValueSql = defaultValueSql,
                          Name = name,
                          StoreType = storeType,
                          IsOutParameter = outParameter
                      };

            return parameter;
        }

        #region Hide object members

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Gets the <see cref="Type" /> of the current instance.
        /// </summary>
        /// <returns>The exact runtime type of the current instance.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }

        /// <summary>
        /// Creates a shallow copy of the current <see cref="Object" />.
        /// </summary>
        /// <returns>A shallow copy of the current <see cref="Object" />.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected new object MemberwiseClone()
        {
            return base.MemberwiseClone();
        }

        #endregion
    }
}
