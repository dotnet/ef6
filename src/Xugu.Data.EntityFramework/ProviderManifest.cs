// Copyright © 2008, 2018, Oracle and/or its affiliates. All rights reserved.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License, version 2.0, as
// published by the Free Software Foundation.
//
// This program is also distributed with certain software (including
// but not limited to OpenSSL) that is licensed under separate terms,
// as designated in a particular file or component or in included license
// documentation.  The authors of Xugu hereby grant you an
// additional permission to link the program and your derivative works
// with the separately licensed software that they have included with
// Xugu.
//
// Without limiting anything contained in the foregoing, this file,
// which is part of Xugu Connector/NET, is also subject to the
// Universal FOSS Exception, version 1.0, a copy of which can be found at
// http://oss.oracle.com/licenses/universal-foss-exception.
//
// This program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU General Public License, version 2.0, for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software Foundation, Inc.,
// 51 Franklin St, Fifth Floor, Boston, MA 02110-1301  USA

using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm;


namespace XuguClient
{
  public class XGProviderManifest : DbXmlEnabledProviderManifest
  {
    string manifestToken;
	
	internal const char LikeEscapeChar = '\u005c';
    internal const string LikeEscapeCharToString = "\u005c";

    public XGProviderManifest(string version)
      : base(GetManifest())
    {
      manifestToken = version;
    }

    private static XmlReader GetManifest()
    {
      return GetXmlResource("Xugu.Data.EntityFramework.Properties.ProviderManifest.xml");
    }

    protected override XmlReader GetDbInformation(string informationType)
    {
      if (informationType == DbProviderManifest.StoreSchemaDefinition)
      {
        return GetStoreSchemaDescription();
      }

      if (informationType == DbProviderManifest.StoreSchemaMapping)
      {
        return GetStoreSchemaMapping();
      }

      throw new ProviderIncompatibleException(String.Format("The provider returned null for the informationType '{0}'.", informationType));
    }

    private XmlReader GetStoreSchemaMapping()
    {
      return GetMappingResource("SchemaMapping.msl");
    }

    private XmlReader GetStoreSchemaDescription()
    {
      //double version = double.Parse(manifestToken, CultureInfo.InvariantCulture);

      //if (version < 12.0) throw new NotSupportedException("Your version of Xugu is not currently supported");
      return GetMappingResource("SchemaDefinition-12.0.ssdl");
    }

    public override TypeUsage GetEdmType(TypeUsage storeType)
    {
      if (storeType == null)
      {
        throw new ArgumentNullException("storeType");
      }

      string storeTypeName = storeType.EdmType.Name.ToLowerInvariant();

      if (!base.StoreTypeNameToEdmPrimitiveType.ContainsKey(storeTypeName))
      {
        throw new ArgumentException(String.Format("The underlying provider does not support the type '{0}'.", storeTypeName));
      }

      PrimitiveType edmPrimitiveType = base.StoreTypeNameToEdmPrimitiveType[storeTypeName];

      if (edmPrimitiveType.PrimitiveTypeKind == PrimitiveTypeKind.Binary)
      {
        return TypeUsage.CreateBinaryTypeUsage(edmPrimitiveType, false);
      }

      if (edmPrimitiveType.PrimitiveTypeKind == PrimitiveTypeKind.String)
      {
        Facet facet;
        if (storeType.Facets.TryGetValue("MaxLength", false, out facet) && !facet.IsUnbounded && facet.Value != null)
          return TypeUsage.CreateStringTypeUsage(edmPrimitiveType, false, false, (int)facet.Value);
        else
          return TypeUsage.CreateStringTypeUsage(edmPrimitiveType, false, false);
      }

      return TypeUsage.CreateDefaultTypeUsage(edmPrimitiveType);
    }

    private const int CHAR_MAXLEN = 255;
    private const int VARCHAR_MAXLEN = 65535;
    private const int MEDIUMTEXT_MAXLEN = 16777215;
    private const int clob_MAXLEN = 1073741823;

    private const int BINARY_MAXLEN = 255;
    private const int VARBINARY_MAXLEN = 65535;
    private const int MEDIUMBLOB_MAXLEN = 16777215;
    private const int LONGBLOB_MAXLEN = 2147483647;

    internal const int DEFAULT_DECIMAL_PRECISION = 10;
    internal const int DEFAULT_DECIMAL_SCALE = 2;

    public override TypeUsage GetStoreType(TypeUsage edmType)
    {
      if (edmType == null)
        throw new ArgumentNullException("edmType");

      Debug.Assert(edmType.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType);

      PrimitiveType primitiveType = edmType.EdmType as PrimitiveType;
      if (primitiveType == null)
        throw new ArgumentException(String.Format(Xugu.Data.EntityFramework.Properties.Resources.TypeNotSupported, edmType));

      ReadOnlyMetadataCollection<Facet> facets = edmType.Facets;

      switch (primitiveType.PrimitiveTypeKind)
      {
        case PrimitiveTypeKind.Boolean:
          return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["bool"]);

        case PrimitiveTypeKind.SByte:
          return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["tinyint"]);

        case PrimitiveTypeKind.Int16:
          return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["smallint"]);

        case PrimitiveTypeKind.Int32:
          return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["int"]);

        case PrimitiveTypeKind.Int64:
          return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["bigint"]);

        case PrimitiveTypeKind.Guid:
          return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["guid"]);

        case PrimitiveTypeKind.Double:
          return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["double"]);

        case PrimitiveTypeKind.Single:
          return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["float"]);

        case PrimitiveTypeKind.Decimal:
          {
            byte precision = DEFAULT_DECIMAL_PRECISION;
            byte scale = DEFAULT_DECIMAL_SCALE;
            Facet facet;

            if (edmType.Facets.TryGetValue("Precision", false, out facet))
            {
              if (!facet.IsUnbounded && facet.Value != null)
                precision = (byte)facet.Value;
            }

            if (edmType.Facets.TryGetValue("Scale", false, out facet))
            {
              if (!facet.IsUnbounded && facet.Value != null )
                scale = (byte)facet.Value;
            }

            return TypeUsage.CreateDecimalTypeUsage(StoreTypeNameToStorePrimitiveType["numeric"], precision, scale);
          }

        case PrimitiveTypeKind.Binary:
          {
            bool isFixedLength = null != facets["FixedLength"].Value && (bool)facets["FixedLength"].Value;
            Facet f = facets["MaxLength"];
            bool isMaxLength = f.IsUnbounded || null == f.Value || (int)f.Value > MEDIUMBLOB_MAXLEN;
            int maxLength = !isMaxLength ? (int)f.Value : LONGBLOB_MAXLEN;

            // now this applies for both isFixedLength and !isFixedLength
            string typeName = "blob";

            return TypeUsage.CreateBinaryTypeUsage(StoreTypeNameToStorePrimitiveType[typeName], isFixedLength, maxLength);
          }

        case PrimitiveTypeKind.String:
          {
            string typeName = String.Empty;
            bool isFixedLength = null != facets["FixedLength"].Value && (bool)facets["FixedLength"].Value;
            int maxLenghtValue;

            Facet maxLengthFacet = facets["MaxLength"];
            if (isFixedLength)
            {
              typeName = "char";
              if (maxLengthFacet.Value != null && Int32.TryParse(maxLengthFacet.Value.ToString(), out maxLenghtValue) && maxLenghtValue <= CHAR_MAXLEN)
                return TypeUsage.CreateStringTypeUsage(StoreTypeNameToStorePrimitiveType[typeName], false, isFixedLength, (int)maxLengthFacet.Value);
              else if (maxLengthFacet.Value != null && maxLengthFacet.Value.ToString() == "Max")
                return TypeUsage.CreateStringTypeUsage(StoreTypeNameToStorePrimitiveType[typeName], false, isFixedLength, CHAR_MAXLEN);
              else
                return TypeUsage.CreateStringTypeUsage(StoreTypeNameToStorePrimitiveType[typeName], false, isFixedLength);
            }
            else
            {
              typeName = "varchar";
              if (maxLengthFacet.Value != null && Int32.TryParse(maxLengthFacet.Value.ToString(), out maxLenghtValue))
              {
                if (maxLenghtValue > VARCHAR_MAXLEN) typeName = "clob";
                return TypeUsage.CreateStringTypeUsage(StoreTypeNameToStorePrimitiveType[typeName], false, isFixedLength, (int)maxLengthFacet.Value);
              }
              else
                return TypeUsage.CreateStringTypeUsage(StoreTypeNameToStorePrimitiveType["clob"], false, isFixedLength, LONGBLOB_MAXLEN);
            }
          }

        //case PrimitiveTypeKind.DateTimeOffset:
        //  return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["timestamp"]);
        case PrimitiveTypeKind.DateTime:
          return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["datetime"]);
        case PrimitiveTypeKind.Time:
          return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["time"]);
        default:
          throw new NotSupportedException(String.Format(Xugu.Data.EntityFramework.Properties.Resources.NoStoreTypeForEdmType, edmType, primitiveType.PrimitiveTypeKind));
      }
    }

    private static XmlReader GetXmlResource(string resourceName)
    {
        Assembly executingAssembly = Assembly.GetExecutingAssembly();
        Stream stream = executingAssembly.GetManifestResourceStream(resourceName);
        return XmlReader.Create(stream);
    }

    private static XmlReader GetMappingResource(string resourceBaseName)
    {
      string rez = GetResourceAsString(
          String.Format("Xugu.Data.EntityFramework.Properties.{0}", resourceBaseName));

      StringReader sr = new StringReader(rez);
      return XmlReader.Create(sr);

    }

    private static string GetResourceAsString(string resourceName)
    {
      Assembly executingAssembly = Assembly.GetExecutingAssembly();
      Stream s = executingAssembly.GetManifestResourceStream(resourceName);
      StreamReader sr = new StreamReader(s);
      string resourceAsString = sr.ReadToEnd();
      sr.Close();
      s.Close();
      return resourceAsString;
    }
	
    public override bool SupportsEscapingLikeArgument(out char escapeCharacter)
    {
      escapeCharacter = LikeEscapeChar;
      return true;
    }
    
    public override string EscapeLikeArgument(string argument)
    {

      bool usedEscapeCharacter;      
      return EscapeLikeArgument(argument, out usedEscapeCharacter);
    }
    
    internal static string EscapeLikeArgument(string argument, out bool usedEscapeChar)
    {     
      usedEscapeChar = false;
      if (argument == null)
        return string.Empty;

      if (!(argument.Contains("%") || argument.Contains("_") || argument.Contains(LikeEscapeCharToString)))
      {
        return argument;
      }
      var sb = new StringBuilder(argument.Length);
      foreach (var c in argument)
      {
        if (c == '%' || c == '_' || c == LikeEscapeChar)
        {
          sb.Append(LikeEscapeChar);
          usedEscapeChar = true;
        }
        sb.Append(c);
      }
      return sb.ToString();
    }
	
    public override bool SupportsInExpression()
    {
      return true;
    }
  }
}
