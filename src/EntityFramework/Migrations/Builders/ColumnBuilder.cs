// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Builders
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Hierarchy;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Spatial;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Helper class that is used to configure a column.
    ///
    /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
    /// (such as the end user of an application). If input is accepted from such sources it should be validated 
    /// before being passed to these APIs to protect against SQL injection attacks etc.
    /// </summary>
    public class ColumnBuilder
    {
        /// <summary>
        /// Creates a new column definition to store Binary data.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="nullable"> Value indicating whether or not the column allows null values. </param>
        /// <param name="maxLength"> The maximum allowable length of the array data. </param>
        /// <param name="fixedLength"> Value indicating whether or not all data should be padded to the maximum length. </param>
        /// <param name="defaultValue"> Constant value to use as the default value for this column. </param>
        /// <param name="defaultValueSql"> SQL expression used as the default value for this column. </param>
        /// <param name="timestamp"> Value indicating whether or not this column should be configured as a timestamp. </param>
        /// <param name="name"> The name of the column. </param>
        /// <param name="storeType"> Provider specific data type to use for this column. </param>
        /// <param name="annotations"> Custom annotations usually from the Code First model. </param>
        /// <returns> The newly constructed column definition. </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ColumnModel Binary(
            bool? nullable = null,
            int? maxLength = null,
            bool? fixedLength = null,
            byte[] defaultValue = null,
            string defaultValueSql = null,
            bool timestamp = false,
            string name = null,
            string storeType = null,
            IDictionary<string, AnnotationValues> annotations = null)
        {
            return BuildColumn(
                PrimitiveTypeKind.Binary,
                nullable,
                defaultValue,
                defaultValueSql,
                maxLength,
                fixedLength: fixedLength,
                timestamp: timestamp,
                name: name,
                storeType: storeType,
                annotations: annotations);
        }

        /// <summary>
        /// Creates a new column definition to store Boolean data.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="nullable"> Value indicating whether or not the column allows null values. </param>
        /// <param name="defaultValue"> Constant value to use as the default value for this column. </param>
        /// <param name="defaultValueSql"> SQL expression used as the default value for this column. </param>
        /// <param name="name"> The name of the column. </param>
        /// <param name="storeType"> Provider specific data type to use for this column. </param>
        /// <param name="annotations"> Custom annotations usually from the Code First model. </param>
        /// <returns> The newly constructed column definition. </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ColumnModel Boolean(
            bool? nullable = null,
            bool? defaultValue = null,
            string defaultValueSql = null,
            string name = null,
            string storeType = null,
            IDictionary<string, AnnotationValues> annotations = null)
        {
            return BuildColumn(
                PrimitiveTypeKind.Boolean,
                nullable,
                defaultValue,
                defaultValueSql,
                name: name,
                storeType: storeType,
                annotations: annotations);
        }

        /// <summary>
        /// Creates a new column definition to store Byte data.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="nullable"> Value indicating whether or not the column allows null values. </param>
        /// <param name="identity"> Value indicating whether or not the database will generate values for this column during insert. </param>
        /// <param name="defaultValue"> Constant value to use as the default value for this column. </param>
        /// <param name="defaultValueSql"> SQL expression used as the default value for this column. </param>
        /// <param name="name"> The name of the column. </param>
        /// <param name="storeType"> Provider specific data type to use for this column. </param>
        /// <param name="annotations"> Custom annotations usually from the Code First model. </param>
        /// <returns> The newly constructed column definition. </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ColumnModel Byte(
            bool? nullable = null,
            bool identity = false,
            byte? defaultValue = null,
            string defaultValueSql = null,
            string name = null,
            string storeType = null,
            IDictionary<string, AnnotationValues> annotations = null)
        {
            return BuildColumn(
                PrimitiveTypeKind.Byte,
                nullable,
                defaultValue,
                defaultValueSql,
                identity: identity,
                name: name,
                storeType: storeType,
                annotations: annotations);
        }

        /// <summary>
        /// Creates a new column definition to store DateTime data.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="nullable"> Value indicating whether or not the column allows null values. </param>
        /// <param name="precision"> The precision of the column. </param>
        /// <param name="defaultValue"> Constant value to use as the default value for this column. </param>
        /// <param name="defaultValueSql"> SQL expression used as the default value for this column. </param>
        /// <param name="name"> The name of the column. </param>
        /// <param name="storeType"> Provider specific data type to use for this column. </param>
        /// <param name="annotations"> Custom annotations usually from the Code First model. </param>
        /// <returns> The newly constructed column definition. </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ColumnModel DateTime(
            bool? nullable = null,
            byte? precision = null,
            DateTime? defaultValue = null,
            string defaultValueSql = null,
            string name = null,
            string storeType = null,
            IDictionary<string, AnnotationValues> annotations = null)
        {
            return BuildColumn(
                PrimitiveTypeKind.DateTime,
                nullable,
                defaultValue,
                defaultValueSql,
                precision: precision,
                name: name,
                storeType: storeType,
                annotations: annotations);
        }

        /// <summary>
        /// Creates a new column definition to store Decimal data.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="nullable"> Value indicating whether or not the column allows null values. </param>
        /// <param name="precision"> The numeric precision of the column. </param>
        /// <param name="scale"> The numeric scale of the column. </param>
        /// <param name="defaultValue"> Constant value to use as the default value for this column. </param>
        /// <param name="defaultValueSql"> SQL expression used as the default value for this column. </param>
        /// <param name="name"> The name of the column. </param>
        /// <param name="storeType"> Provider specific data type to use for this column. </param>
        /// <param name="identity"> Value indicating whether or not the database will generate values for this column during insert. </param>
        /// <param name="annotations"> Custom annotations usually from the Code First model. </param>
        /// <returns> The newly constructed column definition. </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ColumnModel Decimal(
            bool? nullable = null,
            byte? precision = null,
            byte? scale = null,
            decimal? defaultValue = null,
            string defaultValueSql = null,
            string name = null,
            string storeType = null,
            bool identity = false,
            IDictionary<string, AnnotationValues> annotations = null)
        {
            return BuildColumn(
                PrimitiveTypeKind.Decimal,
                nullable,
                defaultValue,
                defaultValueSql,
                precision: precision,
                scale: scale,
                name: name,
                storeType: storeType,
                identity: identity,
                annotations: annotations);
        }

        /// <summary>
        /// Creates a new column definition to store Double data.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="nullable"> Value indicating whether or not the column allows null values. </param>
        /// <param name="defaultValue"> Constant value to use as the default value for this column. </param>
        /// <param name="defaultValueSql"> SQL expression used as the default value for this column. </param>
        /// <param name="name"> The name of the column. </param>
        /// <param name="storeType"> Provider specific data type to use for this column. </param>
        /// <param name="annotations"> Custom annotations usually from the Code First model. </param>
        /// <returns> The newly constructed column definition. </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ColumnModel Double(
            bool? nullable = null,
            double? defaultValue = null,
            string defaultValueSql = null,
            string name = null,
            string storeType = null,
            IDictionary<string, AnnotationValues> annotations = null)
        {
            return BuildColumn(
                PrimitiveTypeKind.Double,
                nullable,
                defaultValue,
                defaultValueSql,
                name: name,
                storeType: storeType,
                annotations: annotations);
        }

        /// <summary>
        /// Creates a new column definition to store GUID data.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="nullable"> Value indicating whether or not the column allows null values. </param>
        /// <param name="identity"> Value indicating whether or not the database will generate values for this column during insert. </param>
        /// <param name="defaultValue"> Constant value to use as the default value for this column. </param>
        /// <param name="defaultValueSql"> SQL expression used as the default value for this column. </param>
        /// <param name="name"> The name of the column. </param>
        /// <param name="storeType"> Provider specific data type to use for this column. </param>
        /// <param name="annotations"> Custom annotations usually from the Code First model. </param>
        /// <returns> The newly constructed column definition. </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ColumnModel Guid(
            bool? nullable = null,
            bool identity = false,
            Guid? defaultValue = null,
            string defaultValueSql = null,
            string name = null,
            string storeType = null,
            IDictionary<string, AnnotationValues> annotations = null)
        {
            return BuildColumn(
                PrimitiveTypeKind.Guid,
                nullable,
                defaultValue,
                defaultValueSql,
                identity: identity,
                name: name,
                storeType: storeType,
                annotations: annotations);
        }

        /// <summary>
        /// Creates a new column definition to store Single data.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="nullable"> Value indicating whether or not the column allows null values. </param>
        /// <param name="defaultValue"> Constant value to use as the default value for this column. </param>
        /// <param name="defaultValueSql"> SQL expression used as the default value for this column. </param>
        /// <param name="name"> The name of the column. </param>
        /// <param name="storeType"> Provider specific data type to use for this column. </param>
        /// <param name="annotations"> Custom annotations usually from the Code First model. </param>
        /// <returns> The newly constructed column definition. </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ColumnModel Single(
            bool? nullable = null,
            float? defaultValue = null,
            string defaultValueSql = null,
            string name = null,
            string storeType = null,
            IDictionary<string, AnnotationValues> annotations = null)
        {
            return BuildColumn(
                PrimitiveTypeKind.Single,
                nullable,
                defaultValue,
                defaultValueSql,
                name: name,
                storeType: storeType,
                annotations: annotations);
        }

        /// <summary>
        /// Creates a new column definition to store Short data.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="nullable"> Value indicating whether or not the column allows null values. </param>
        /// <param name="identity"> Value indicating whether or not the database will generate values for this column during insert. </param>
        /// <param name="defaultValue"> Constant value to use as the default value for this column. </param>
        /// <param name="defaultValueSql"> SQL expression used as the default value for this column. </param>
        /// <param name="name"> The name of the column. </param>
        /// <param name="storeType"> Provider specific data type to use for this column. </param>
        /// <param name="annotations"> Custom annotations usually from the Code First model. </param>
        /// <returns> The newly constructed column definition. </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ColumnModel Short(
            bool? nullable = null,
            bool identity = false,
            short? defaultValue = null,
            string defaultValueSql = null,
            string name = null,
            string storeType = null,
            IDictionary<string, AnnotationValues> annotations = null)
        {
            return BuildColumn(
                PrimitiveTypeKind.Int16,
                nullable,
                defaultValue,
                defaultValueSql,
                identity: identity,
                name: name,
                storeType: storeType,
                annotations: annotations);
        }

        /// <summary>
        /// Creates a new column definition to store Integer data.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="nullable"> Value indicating whether or not the column allows null values. </param>
        /// <param name="identity"> Value indicating whether or not the database will generate values for this column during insert. </param>
        /// <param name="defaultValue"> Constant value to use as the default value for this column. </param>
        /// <param name="defaultValueSql"> SQL expression used as the default value for this column. </param>
        /// <param name="name"> The name of the column. </param>
        /// <param name="storeType"> Provider specific data type to use for this column. </param>
        /// <param name="annotations"> Custom annotations usually from the Code First model. </param>
        /// <returns> The newly constructed column definition. </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ColumnModel Int(
            bool? nullable = null,
            bool identity = false,
            int? defaultValue = null,
            string defaultValueSql = null,
            string name = null,
            string storeType = null,
            IDictionary<string, AnnotationValues> annotations = null)
        {
            return BuildColumn(
                PrimitiveTypeKind.Int32,
                nullable,
                defaultValue,
                defaultValueSql,
                identity: identity,
                name: name,
                storeType: storeType,
                annotations: annotations);
        }

        /// <summary>
        /// Creates a new column definition to store Long data.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="nullable"> Value indicating whether or not the column allows null values. </param>
        /// <param name="identity"> Value indicating whether or not the database will generate values for this column during insert. </param>
        /// <param name="defaultValue"> Constant value to use as the default value for this column. </param>
        /// <param name="defaultValueSql"> SQL expression used as the default value for this column. </param>
        /// <param name="name"> The name of the column. </param>
        /// <param name="storeType"> Provider specific data type to use for this column. </param>
        /// <param name="annotations"> Custom annotations usually from the Code First model. </param>
        /// <returns> The newly constructed column definition. </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ColumnModel Long(
            bool? nullable = null,
            bool identity = false,
            long? defaultValue = null,
            string defaultValueSql = null,
            string name = null,
            string storeType = null,
            IDictionary<string, AnnotationValues> annotations = null)
        {
            return BuildColumn(
                PrimitiveTypeKind.Int64,
                nullable,
                defaultValue,
                defaultValueSql,
                identity: identity,
                name: name,
                storeType: storeType,
                annotations: annotations);
        }

        /// <summary>
        /// Creates a new column definition to store String data.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="nullable"> Value indicating whether or not the column allows null values. </param>
        /// <param name="maxLength"> The maximum allowable length of the string data. </param>
        /// <param name="fixedLength"> Value indicating whether or not all data should be padded to the maximum length. </param>
        /// <param name="unicode"> Value indicating whether or not the column supports Unicode content. </param>
        /// <param name="defaultValue"> Constant value to use as the default value for this column. </param>
        /// <param name="defaultValueSql"> SQL expression used as the default value for this column. </param>
        /// <param name="name"> The name of the column. </param>
        /// <param name="storeType"> Provider specific data type to use for this column. </param>
        /// <param name="annotations"> Custom annotations usually from the Code First model. </param>
        /// <returns> The newly constructed column definition. </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ColumnModel String(
            bool? nullable = null,
            int? maxLength = null,
            bool? fixedLength = null,
            bool? unicode = null,
            string defaultValue = null,
            string defaultValueSql = null,
            string name = null,
            string storeType = null,
            IDictionary<string, AnnotationValues> annotations = null)
        {
            return BuildColumn(
                PrimitiveTypeKind.String,
                nullable,
                defaultValue,
                defaultValueSql,
                maxLength,
                fixedLength: fixedLength,
                unicode: unicode,
                name: name,
                storeType: storeType,
                annotations: annotations);
        }

        /// <summary>
        /// Creates a new column definition to store Time data.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="nullable"> Value indicating whether or not the column allows null values. </param>
        /// <param name="precision"> The precision of the column. </param>
        /// <param name="defaultValue"> Constant value to use as the default value for this column. </param>
        /// <param name="defaultValueSql"> SQL expression used as the default value for this column. </param>
        /// <param name="name"> The name of the column. </param>
        /// <param name="storeType"> Provider specific data type to use for this column. </param>
        /// <param name="annotations"> Custom annotations usually from the Code First model. </param>
        /// <returns> The newly constructed column definition. </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ColumnModel Time(
            bool? nullable = null,
            byte? precision = null,
            TimeSpan? defaultValue = null,
            string defaultValueSql = null,
            string name = null,
            string storeType = null,
            IDictionary<string, AnnotationValues> annotations = null)
        {
            return BuildColumn(
                PrimitiveTypeKind.Time,
                nullable,
                defaultValue,
                defaultValueSql,
                precision: precision,
                name: name,
                storeType: storeType,
                annotations: annotations);
        }

        /// <summary>
        /// Creates a new column definition to store DateTimeOffset data.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="nullable"> Value indicating whether or not the column allows null values. </param>
        /// <param name="precision"> The precision of the column. </param>
        /// <param name="defaultValue"> Constant value to use as the default value for this column. </param>
        /// <param name="defaultValueSql"> SQL expression used as the default value for this column. </param>
        /// <param name="name"> The name of the column. </param>
        /// <param name="storeType"> Provider specific data type to use for this column. </param>
        /// <param name="annotations"> Custom annotations usually from the Code First model. </param>
        /// <returns> The newly constructed column definition. </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ColumnModel DateTimeOffset(
            bool? nullable = null,
            byte? precision = null,
            DateTimeOffset? defaultValue = null,
            string defaultValueSql = null,
            string name = null,
            string storeType = null,
            IDictionary<string, AnnotationValues> annotations = null)
        {
            return BuildColumn(
                PrimitiveTypeKind.DateTimeOffset,
                nullable,
                defaultValue,
                defaultValueSql,
                precision: precision,
                name: name,
                storeType: storeType,
                annotations: annotations);
        }

        /// <summary>
        /// Creates a new column definition to store hierarchyid data.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="nullable"> Value indicating whether or not the column allows null values. </param>
        /// <param name="defaultValue"> Constant value to use as the default value for this column. </param>
        /// <param name="defaultValueSql"> SQL expression used as the default value for this column. </param>
        /// <param name="name"> The name of the column. </param>
        /// <param name="storeType"> Provider specific data type to use for this column. </param>
        /// <param name="annotations"> Custom annotations usually from the Code First model. </param>
        /// <returns> The newly constructed column definition. </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ColumnModel HierarchyId(
            bool? nullable = null,
            HierarchyId defaultValue = null,
            string defaultValueSql = null,
            string name = null,
            string storeType = null,
            IDictionary<string, AnnotationValues> annotations = null)
        {
            return BuildColumn(
                PrimitiveTypeKind.HierarchyId,
                nullable,
                defaultValue,
                defaultValueSql,
                name: name,
                storeType: storeType,
                annotations: annotations);
        }

        /// <summary>
        /// Creates a new column definition to store geography data.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="nullable"> Value indicating whether or not the column allows null values. </param>
        /// <param name="defaultValue"> Constant value to use as the default value for this column. </param>
        /// <param name="defaultValueSql"> SQL expression used as the default value for this column. </param>
        /// <param name="name"> The name of the column. </param>
        /// <param name="storeType"> Provider specific data type to use for this column. </param>
        /// <param name="annotations"> Custom annotations usually from the Code First model. </param>
        /// <returns> The newly constructed column definition. </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ColumnModel Geography(
            bool? nullable = null,
            DbGeography defaultValue = null,
            string defaultValueSql = null,
            string name = null,
            string storeType = null,
            IDictionary<string, AnnotationValues> annotations = null)
        {
            return BuildColumn(
                PrimitiveTypeKind.Geography,
                nullable,
                defaultValue,
                defaultValueSql,
                name: name,
                storeType: storeType,
                annotations: annotations);
        }

        /// <summary>
        /// Creates a new column definition to store geometry data.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="nullable"> Value indicating whether or not the column allows null values. </param>
        /// <param name="defaultValue"> Constant value to use as the default value for this column. </param>
        /// <param name="defaultValueSql"> SQL expression used as the default value for this column. </param>
        /// <param name="name"> The name of the column. </param>
        /// <param name="storeType"> Provider specific data type to use for this column. </param>
        /// <param name="annotations"> Custom annotations usually from the Code First model. </param>
        /// <returns> The newly constructed column definition. </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ColumnModel Geometry(
            bool? nullable = null,
            DbGeometry defaultValue = null,
            string defaultValueSql = null,
            string name = null,
            string storeType = null,
            IDictionary<string, AnnotationValues> annotations = null)
        {
            return BuildColumn(
                PrimitiveTypeKind.Geometry,
                nullable,
                defaultValue,
                defaultValueSql,
                name: name,
                storeType: storeType,
                annotations: annotations);
        }

        private static ColumnModel BuildColumn(
            PrimitiveTypeKind primitiveTypeKind,
            bool? nullable,
            object defaultValue,
            string defaultValueSql = null,
            int? maxLength = null,
            byte? precision = null,
            byte? scale = null,
            bool? unicode = null,
            bool? fixedLength = null,
            bool identity = false,
            bool timestamp = false,
            string name = null,
            string storeType = null,
            IDictionary<string, AnnotationValues> annotations = null)
        {
            var column
                = new ColumnModel(primitiveTypeKind)
                    {
                        IsNullable = nullable,
                        MaxLength = maxLength,
                        Precision = precision,
                        Scale = scale,
                        IsUnicode = unicode,
                        IsFixedLength = fixedLength,
                        IsIdentity = identity,
                        DefaultValue = defaultValue,
                        DefaultValueSql = defaultValueSql,
                        IsTimestamp = timestamp,
                        Name = name,
                        StoreType = storeType,
                        Annotations = annotations
                    };

            return column;
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
