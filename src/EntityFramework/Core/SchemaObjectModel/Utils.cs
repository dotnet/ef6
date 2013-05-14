// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.SchemaObjectModel
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Globalization;
    using System.Xml;
    using System.Xml.Schema;

    /// <summary>
    ///     Summary description for Utils.
    /// </summary>
    // make class internal when friend assemblies are available
    internal static class Utils
    {
        #region Static Methods

        internal static void ExtractNamespaceAndName(string qualifiedTypeName, out string namespaceName, out string name)
        {
            DebugCheck.NotEmpty(qualifiedTypeName);
            GetBeforeAndAfterLastPeriod(qualifiedTypeName, out namespaceName, out name);
        }

        internal static string ExtractTypeName(string qualifiedTypeName)
        {
            DebugCheck.NotEmpty(qualifiedTypeName);
            return GetEverythingAfterLastPeriod(qualifiedTypeName);
        }

        private static void GetBeforeAndAfterLastPeriod(string qualifiedTypeName, out string before, out string after)
        {
            var lastDot = qualifiedTypeName.LastIndexOf('.');
            if (lastDot < 0)
            {
                before = null;
                after = qualifiedTypeName;
            }
            else
            {
                before = qualifiedTypeName.Substring(0, lastDot);
                after = qualifiedTypeName.Substring(lastDot + 1);
            }
        }

        internal static string GetEverythingBeforeLastPeriod(string qualifiedTypeName)
        {
            var lastDot = qualifiedTypeName.LastIndexOf('.');
            if (lastDot < 0)
            {
                return null;
            }
            return qualifiedTypeName.Substring(0, lastDot);
        }

        private static string GetEverythingAfterLastPeriod(string qualifiedTypeName)
        {
            var lastDot = qualifiedTypeName.LastIndexOf('.');
            if (lastDot < 0)
            {
                return qualifiedTypeName;
            }

            return qualifiedTypeName.Substring(lastDot + 1);
        }

        public static bool GetString(Schema schema, XmlReader reader, out string value)
        {
            DebugCheck.NotNull(schema);
            DebugCheck.NotNull(reader);

            if (reader.SchemaInfo.Validity
                == XmlSchemaValidity.Invalid)
            {
                // an error has already been issued by the xsd validation
                value = null;
                return false;
            }

            value = reader.Value;

            if (string.IsNullOrEmpty(value))
            {
                schema.AddError(
                    ErrorCode.InvalidName, EdmSchemaErrorSeverity.Error, reader,
                    Strings.InvalidName(value, reader.Name));
                return false;
            }
            return true;
        }

        public static bool GetDottedName(Schema schema, XmlReader reader, out string name)
        {
            if (!GetString(schema, reader, out name))
            {
                return false;
            }

            return ValidateDottedName(schema, reader, name);
        }

        internal static bool ValidateDottedName(Schema schema, XmlReader reader, string name)
        {
            DebugCheck.NotNull(schema);
            DebugCheck.NotNull(reader);
            DebugCheck.NotEmpty(name);
            Debug.Assert(
                reader.SchemaInfo.Validity != XmlSchemaValidity.Invalid, "This method should not be called when the schema is invalid");

            if (schema.DataModel == SchemaDataModelOption.EntityDataModel)
            {
                // each part of the dotted name needs to be a valid name
                foreach (var namePart in name.Split('.'))
                {
                    if (!namePart.IsValidUndottedName())
                    {
                        schema.AddError(
                            ErrorCode.InvalidName, EdmSchemaErrorSeverity.Error, reader,
                            Strings.InvalidName(name, reader.Name));
                        return false;
                    }
                }
            }
            return true;
        }

        public static bool GetUndottedName(Schema schema, XmlReader reader, out string name)
        {
            DebugCheck.NotNull(schema);
            DebugCheck.NotNull(reader);

            if (reader.SchemaInfo.Validity == XmlSchemaValidity.Invalid)
            {
                // the xsd already put in an error
                name = null;
                return false;
            }

            name = reader.Value;
            if (string.IsNullOrEmpty(name))
            {
                schema.AddError(
                    ErrorCode.InvalidName, EdmSchemaErrorSeverity.Error, reader,
                    Strings.EmptyName(reader.Name));
                return false;
            }

            if (schema.DataModel == SchemaDataModelOption.EntityDataModel
                && !name.IsValidUndottedName())
            {
                schema.AddError(
                    ErrorCode.InvalidName, EdmSchemaErrorSeverity.Error, reader,
                    Strings.InvalidName(name, reader.Name));
                return false;
            }

            Debug.Assert(
                !(schema.DataModel == SchemaDataModelOption.EntityDataModel && name.IndexOf('.') >= 0),
                string.Format(CultureInfo.CurrentCulture, "{1} ({0}) is not valid. {1} cannot be qualified.", name, reader.Name));

            return true;
        }

        public static bool GetBool(Schema schema, XmlReader reader, out bool value)
        {
            DebugCheck.NotNull(schema);
            DebugCheck.NotNull(reader);

            if (reader.SchemaInfo.Validity
                == XmlSchemaValidity.Invalid)
            {
                value = true; // we have to set the value to something before returning.
                return false;
            }

            // do this in a try catch, just in case the attribute wasn't validated against an xsd:boolean
            try
            {
                value = reader.ReadContentAsBoolean();
                return true;
            }
            catch (XmlException)
            {
                // we already handled the valid and invalid cases, so it must be NotKnown now.
                Debug.Assert(reader.SchemaInfo.Validity == XmlSchemaValidity.NotKnown, "The schema validity must be NotKnown at this point");
                schema.AddError(
                    ErrorCode.BoolValueExpected, EdmSchemaErrorSeverity.Error, reader,
                    Strings.ValueNotUnderstood(reader.Value, reader.Name));
            }

            value = true; // we have to set the value to something before returning.
            return false;
        }

        public static bool GetInt(Schema schema, XmlReader reader, out int value)
        {
            DebugCheck.NotNull(schema);
            DebugCheck.NotNull(reader);

            if (reader.SchemaInfo.Validity
                == XmlSchemaValidity.Invalid)
            {
                // an error has already been issued by the xsd validation
                value = 0;
                ;
                return false;
            }

            var text = reader.Value;
            value = int.MinValue;

            if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            {
                return true;
            }

            schema.AddError(
                ErrorCode.IntegerExpected, EdmSchemaErrorSeverity.Error, reader,
                Strings.ValueNotUnderstood(reader.Value, reader.Name));
            return false;
        }

        public static bool GetByte(Schema schema, XmlReader reader, out byte value)
        {
            DebugCheck.NotNull(schema);
            DebugCheck.NotNull(reader);

            if (reader.SchemaInfo.Validity
                == XmlSchemaValidity.Invalid)
            {
                // an error has already been issued by the xsd validation
                value = 0;
                ;
                return false;
            }

            var text = reader.Value;
            value = byte.MinValue;

            if (byte.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            {
                return true;
            }

            schema.AddError(
                ErrorCode.ByteValueExpected, EdmSchemaErrorSeverity.Error, reader,
                Strings.ValueNotUnderstood(reader.Value, reader.Name));

            return false;
        }

        public static int CompareNames(string lhsName, string rhsName)
        {
            return string.Compare(lhsName, rhsName, StringComparison.Ordinal);
        }

        #endregion
    }
}
