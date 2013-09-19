// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Data.Spatial;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SampleEntityFrameworkProvider
{
    internal static class SqlTypes
    {
        static SqlTypes()
        {
            // find the latest version of Microsoft.SqlServer.Types assembly that contains Sql spatial types
            var preferredSqlTypesAssemblies = new[] 
            {                
                "Microsoft.SqlServer.Types, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91",
                "Microsoft.SqlServer.Types, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91",
            };

            Assembly sqlTypesAssembly = null;
            foreach (string assemblyFullName in preferredSqlTypesAssemblies)
            {
                AssemblyName asmName = new AssemblyName(assemblyFullName);
                try
                {
                    sqlTypesAssembly = Assembly.Load(asmName);
                    break;
                }
                catch (FileNotFoundException)
                {
                }
                catch (FileLoadException)
                {
                }
            }

            if (sqlTypesAssembly == null)
            {
                throw new InvalidOperationException("Microsoft.SqlServer.Types assembly not found");
            }

            SqlGeographyType = sqlTypesAssembly.GetType("Microsoft.SqlServer.Types.SqlGeography", throwOnError: true);
            SqlGeometryType = sqlTypesAssembly.GetType("Microsoft.SqlServer.Types.SqlGeometry", throwOnError: true);

            SqlCharsType = SqlGeometryType.GetMethod("STAsText", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null).ReturnType;
            SqlStringType = SqlCharsType.Assembly.GetType("System.Data.SqlTypes.SqlString", throwOnError: true);
            SqlBytesType = SqlCharsType.Assembly.GetType("System.Data.SqlTypes.SqlBytes", throwOnError: true);
            SqlXmlType = SqlCharsType.Assembly.GetType("System.Data.SqlTypes.SqlXml", throwOnError: true);
        }

        public static Type SqlGeographyType { get; private set; }
        public static Type SqlGeometryType { get; private set; }
        public static Type SqlCharsType { get; private set; }
        public static Type SqlStringType { get; private set; }
        public static Type SqlBytesType { get; private set; }
        public static Type SqlXmlType { get; private set; }

        public static object SqlStringFromString(string value)
        {
            return SqlTypes.SqlStringType.GetConstructor(new Type[] { typeof(string) }).Invoke(new object[] { value });
        }

        public static object SqlCharsFromString(string value)
        {
            return SqlTypes.SqlCharsType.GetConstructor(new Type[] { SqlStringType }).Invoke(new object[] { SqlStringFromString(value) });
        }

        public static object SqlBytesFromByteArray(byte[] value)
        {
            return SqlTypes.SqlBytesType.GetConstructor(new Type[] { typeof(byte[]) }).Invoke(new object[] { value });
        }

        public static  object SqlXmlFromString(string value)
        {
            using(var reader = XmlReader.Create(new StringReader(value)))
            {
                return SqlTypes.SqlXmlType.GetConstructor(new Type[] { typeof(XmlReader) }).Invoke(new object[] { reader });
            }
        }

        public static object ConvertToSqlTypesGeography(DbGeography geography)
        {
            Debug.Assert(geography != null, "geography != null");

            var providerValue = geography.ProviderValue;
            if (providerValue == null || providerValue.GetType() == SqlGeographyType)
            {
                return providerValue;
            }
         
            // DbGeography value created by a different spatial services
            throw new NotSupportedException("DbGeography values not backed by Sql Server spatial types are not supported.");
        }

        public static object ConvertToSqlTypesGeometry(DbGeometry geometry)
        {
            var providerValue = geometry.ProviderValue;
            if (providerValue == null || providerValue.GetType() == SqlGeometryType)
            {
                return providerValue;
            }

            // DbGeography value created by a different spatial services
            throw new NotSupportedException("DbGeometry values not backed by Sql Server spatial types are not supported.");
        }
    }
}
