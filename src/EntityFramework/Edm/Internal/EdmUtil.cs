namespace System.Data.Entity.Edm.Internal
{
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;

    internal static class EdmUtil
    {
        internal static NotSupportedException NotSupported(string message)
        {
            return new NotSupportedException(message);
        }

        internal static bool EqualsOrdinal(this string string1, string string2)
        {
            return string.Equals(string1, string2, StringComparison.Ordinal);
        }

        internal static bool TryGetPrimitiveTypeKindFromString(string value, out EdmPrimitiveTypeKind typeKind)
        {
            Contract.Assert(value != null, "Ensure value is non-null before calling PrimitiveTypeKindFromString");

            switch (value)
            {
                case "Binary":
                    typeKind = EdmPrimitiveTypeKind.Binary;
                    return true;

                case "Boolean":
                    typeKind = EdmPrimitiveTypeKind.Boolean;
                    return true;

                case "Byte":
                    typeKind = EdmPrimitiveTypeKind.Byte;
                    return true;

                case "DateTime":
                    typeKind = EdmPrimitiveTypeKind.DateTime;
                    return true;

                case "DateTimeOffset":
                    typeKind = EdmPrimitiveTypeKind.DateTimeOffset;
                    return true;

                case "Decimal":
                    typeKind = EdmPrimitiveTypeKind.Decimal;
                    return true;

                case "Double":
                    typeKind = EdmPrimitiveTypeKind.Double;
                    return true;

                case "Guid":
                    typeKind = EdmPrimitiveTypeKind.Guid;
                    return true;

                case "Single":
                    typeKind = EdmPrimitiveTypeKind.Single;
                    return true;

                case "SByte":
                    typeKind = EdmPrimitiveTypeKind.SByte;
                    return true;

                case "Int16":
                    typeKind = EdmPrimitiveTypeKind.Int16;
                    return true;

                case "Int32":
                    typeKind = EdmPrimitiveTypeKind.Int32;
                    return true;

                case "Int64":
                    typeKind = EdmPrimitiveTypeKind.Int64;
                    return true;

                case "String":
                    typeKind = EdmPrimitiveTypeKind.String;
                    return true;

                case "Time":
                    typeKind = EdmPrimitiveTypeKind.Time;
                    return true;

                case "Geometry":
                    typeKind = EdmPrimitiveTypeKind.Geometry;
                    return true;

                case "Geography":
                    typeKind = EdmPrimitiveTypeKind.Geography;
                    return true;

                default:
                    typeKind = default(EdmPrimitiveTypeKind);
                    return false;
            }
        }

        internal static bool IsValidDataModelItemName(string name)
        {
            return IsValidUndottedName(name);
        }

        internal static bool IsValidQualifiedItemName(string name)
        {
            return IsValidDottedName(name);
        }

        #region Extremely Dubious SOM Utility methods that we should strongly consider removing

        // this is what we should be doing for CDM schemas
        // the RegEx for valid identifiers are taken from the C# Language Specification (2.4.2 Identifiers)
        // (except that we exclude _ as a valid starting character).
        // This results in a somewhat smaller set of identifier from what System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier
        // allows. Not all identifiers allowed by IsValidLanguageIndependentIdentifier are valid in C#.IsValidLanguageIndependentIdentifier allows:
        //    Mn, Mc, and Pc as a leading character (which the spec and C# (at least for some Mn and Mc characters) do not allow)
        //    characters that Char.GetUnicodeCategory says are in Nl and Cf but which the RegEx does not accept (and which C# does allow).
        //
        // we could create the StartCharacterExp and OtherCharacterExp dynamically to force inclusion of the missing Nl and Cf characters...
        private const string StartCharacterExp = @"[\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Lm}\p{Nl}]";
        private const string OtherCharacterExp = @"[\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Lm}\p{Nl}\p{Mn}\p{Mc}\p{Nd}\p{Pc}\p{Cf}]";
        private const string NameExp = StartCharacterExp + OtherCharacterExp + "{0,}";
        //private static Regex ValidDottedName=new Regex(@"^"+NameExp+@"(\."+NameExp+@"){0,}$",RegexOptions.Singleline);
        private static readonly Regex UndottedNameValidator = new Regex(
            @"^" + NameExp + @"$", RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        ///     Parsing code taken from System.dll's System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(string) method to avoid LinkDemand needed to call this method
        /// </summary>
        private static bool IsValidLanguageIndependentIdentifier(string value)
        {
            var isTypeName = false;

            var nextMustBeStartChar = true;
            if (value.Length == 0)
            {
                return false;
            }
            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                switch (char.GetUnicodeCategory(c))
                {
                    case UnicodeCategory.UppercaseLetter:
                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.TitlecaseLetter:
                    case UnicodeCategory.ModifierLetter:
                    case UnicodeCategory.OtherLetter:
                    case UnicodeCategory.LetterNumber:
                        {
                            nextMustBeStartChar = false;
                            continue;
                        }
                    case UnicodeCategory.NonSpacingMark:
                    case UnicodeCategory.SpacingCombiningMark:
                    case UnicodeCategory.DecimalDigitNumber:
                    case UnicodeCategory.ConnectorPunctuation:
                        if (!nextMustBeStartChar || (c == '_'))
                        {
                            break;
                        }
                        return false;

                    default:
                        goto Label_008C;
                }
                nextMustBeStartChar = false;
                continue;
                Label_008C:
                if (!isTypeName || !IsSpecialTypeChar(c, ref nextMustBeStartChar))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsSpecialTypeChar(char ch, ref bool nextMustBeStartChar)
        {
            switch (ch)
            {
                case '[':
                case ']':
                case '$':
                case '&':
                case '*':
                case '+':
                case ',':
                case '-':
                case '.':
                case ':':
                case '<':
                case '>':
                    nextMustBeStartChar = true;
                    return true;

                case '`':
                    return true;
            }
            return false;
        }

        /// <summary>
        /// </summary>
        /// <param name = "name"> </param>
        /// <returns> </returns>
        private static bool IsValidUndottedName(string name)
        {
            // CodeGenerator.IsValidLanguageIndependentIdentifier does demand a FullTrust Link
            // but this is safe since the function only walks over the string no risk is introduced
            return (!string.IsNullOrEmpty(name) &&
                    UndottedNameValidator.IsMatch(name) &&
                    IsValidLanguageIndependentIdentifier(name));
        }

        private static bool IsValidDottedName(string name)
        {
            // each part of the dotted name needs to be a valid name
            foreach (var namePart in name.Split('.'))
            {
                if (!IsValidUndottedName(namePart))
                {
                    return false;
                }
            }
            return true;
        }

        public static string StripInvalidCharacters(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                // The case where the value is null or whitespace is treated as a special case
                // of a string with no invalid characters and hence the consistent return type
                // for other input of this type is to return the empty string.
                return String.Empty;
            }

            var builder = new StringBuilder(value.Length);
            var nextMustBeStartChar = true;
            foreach (var c in value)
            {
                if (c == '.')
                {
                    if (!nextMustBeStartChar)
                    {
                        builder.Append(c);
                    }
                    continue;
                }

                switch (Char.GetUnicodeCategory(c))
                {
                    case UnicodeCategory.UppercaseLetter:
                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.TitlecaseLetter:
                    case UnicodeCategory.ModifierLetter:
                    case UnicodeCategory.OtherLetter:
                    case UnicodeCategory.LetterNumber:
                        {
                            nextMustBeStartChar = false;
                            builder.Append(c);
                            break;
                        }
                    case UnicodeCategory.NonSpacingMark:
                    case UnicodeCategory.SpacingCombiningMark:
                    case UnicodeCategory.DecimalDigitNumber:
                    case UnicodeCategory.ConnectorPunctuation:
                        if (!nextMustBeStartChar)
                        {
                            builder.Append(c);
                        }
                        break;

                    default:
                        break;
                }
            }
            return builder.ToString();
        }

        #endregion
    }
}