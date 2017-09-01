// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Collections.Generic;
    using System.Data.Entity.SqlServer.Resources;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Contains function stubs that expose SqlServer methods in Linq to Entities.
    /// </summary>
    public static class SqlFunctions
    {
        /// <summary>Returns the checksum of the values in a collection. Null values are ignored.</summary>
        /// <returns>The checksum computed from the input collection.</returns>
        /// <param name="arg">The collection of values over which the checksum is computed.</param>
        [DbFunction("SqlServer", "CHECKSUM_AGG")]
        public static Int32? ChecksumAggregate(IEnumerable<Int32> arg)
        {
            return BootstrapFunction(a => ChecksumAggregate(a), arg);
        }

        /// <summary>Returns the checksum of the values in a collection. Null values are ignored.</summary>
        /// <returns>The checksum computed from the input collection.</returns>
        /// <param name="arg">The collection of values over which the checksum is computed.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [DbFunction("SqlServer", "CHECKSUM_AGG")]
        public static Int32? ChecksumAggregate(IEnumerable<Int32?> arg)
        {
            return BootstrapFunction(a => ChecksumAggregate(a), arg);
        }

        private static TOut BootstrapFunction<TIn, TOut>(Expression<Func<IEnumerable<TIn>, TOut>> methodExpression, IEnumerable<TIn> arg)
        {
            var asQueryable = arg as IQueryable;
            if (asQueryable != null)
            {
                // We could use methodExpression directly here, but it seems marginally better (and consistent with
                // previous versions) to use a constant expression for the parameter.
                return asQueryable.Provider.Execute<TOut>(
                    Expression.Call(((MethodCallExpression)methodExpression.Body).Method, Expression.Constant(arg)));
            }

            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the ASCII code value of the left-most character of a character expression.</summary>
        /// <returns>The ASCII code of the first character in the input string.</returns>
        /// <param name="arg">A valid string.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ascii")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "ASCII")]
        public static Int32? Ascii(String arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the character that corresponds to the specified integer ASCII value.</summary>
        /// <returns>The character that corresponds to the specified ASCII value.</returns>
        /// <param name="arg">An ASCII code.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "CHAR")]
        public static String Char(Int32? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the starting position of one expression found within another expression.</summary>
        /// <returns>The starting position of  toFind  if it is found in  toSearch .</returns>
        /// <param name="toFind">The string expression to be found.</param>
        /// <param name="toSearch">The string expression to be searched.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "toFind")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "toSearch")]
        [DbFunction("SqlServer", "CHARINDEX")]
        public static Int32? CharIndex(String toFind, String toSearch)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the starting position of one expression found within another expression.</summary>
        /// <returns>The starting position of  toFind  if it is found in  toSearch .</returns>
        /// <param name="toFind">The string expression to be found.</param>
        /// <param name="toSearch">The string expression to be searched.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "toFind")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "toSearch")]
        [DbFunction("SqlServer", "CHARINDEX")]
        public static Int32? CharIndex(Byte[] toFind, Byte[] toSearch)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the starting position of one expression found within another expression.</summary>
        /// <returns>The starting position of  toFind  if it is found in  toSearch .</returns>
        /// <param name="toFind">The string expression to be found.</param>
        /// <param name="toSearch">The string expression to be searched.</param>
        /// <param name="startLocation">The character position in  toSearch  where searching begins.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "toFind")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "toSearch")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startLocation")]
        [DbFunction("SqlServer", "CHARINDEX")]
        public static Int32? CharIndex(String toFind, String toSearch, Int32? startLocation)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the starting position of one expression found within another expression.</summary>
        /// <returns>The starting position of  toFind  if it is found in  toSearch .</returns>
        /// <param name="toFind">The string expression to be found.</param>
        /// <param name="toSearch">The string expression to be searched.</param>
        /// <param name="startLocation">The character position in  toSearch  where searching begins.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "toFind")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "toSearch")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startLocation")]
        [DbFunction("SqlServer", "CHARINDEX")]
        public static Int32? CharIndex(Byte[] toFind, Byte[] toSearch, Int32? startLocation)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the starting position of one expression found within another expression.</summary>
        /// <returns>
        /// A <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int64" /> value that is the starting position of  toFind  if it is found in  toSearch .
        /// </returns>
        /// <param name="toFind">The string expression to be found.</param>
        /// <param name="toSearch">The string expression to be searched.</param>
        /// <param name="startLocation">The character position in  toSearch  where searching begins.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "toFind")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "toSearch")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startLocation")]
        [DbFunction("SqlServer", "CHARINDEX")]
        public static Int64? CharIndex(String toFind, String toSearch, Int64? startLocation)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the starting position of one expression found within another expression.</summary>
        /// <returns>The starting position of  toFind  if it is found in  toSearch .</returns>
        /// <param name="toFind">The string expression to be found.</param>
        /// <param name="toSearch">The string expression to be searched.</param>
        /// <param name="startLocation">The character position in  toSearch  where searching begins.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "toFind")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "toSearch")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startLocation")]
        [DbFunction("SqlServer", "CHARINDEX")]
        public static Int64? CharIndex(Byte[] toFind, Byte[] toSearch, Int64? startLocation)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns an integer value that indicates the difference between the SOUNDEX values of two character expressions.</summary>
        /// <returns>The SOUNDEX difference between the two strings.</returns>
        /// <param name="string1">The first string.</param>
        /// <param name="string2">The second string.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "string2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "string1")]
        [DbFunction("SqlServer", "DIFFERENCE")]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
        public static Int32? Difference(String string1, String string2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the Unicode character with the specified integer code, as defined by the Unicode standard.</summary>
        /// <returns>The character that corresponds to the input character code.</returns>
        /// <param name="arg">A character code.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "NCHAR")]
        public static String NChar(Int32? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the starting position of the first occurrence of a pattern in a specified expression, or zeros if the pattern is not found, on all valid text and character data types.</summary>
        /// <returns>The starting character position where the string pattern was found.</returns>
        /// <param name="stringPattern">A string pattern to search for.</param>
        /// <param name="target">The string to search.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stringPattern")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "target")]
        [DbFunction("SqlServer", "PATINDEX")]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
        public static Int32? PatIndex(String stringPattern, String target)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns a Unicode string with the delimiters added to make the input string a valid Microsoft SQL Server delimited identifier.</summary>
        /// <returns>The original string with brackets added.</returns>
        /// <param name="stringArg">The expression that quote characters will be added to.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stringArg")]
        [DbFunction("SqlServer", "QUOTENAME")]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
        public static String QuoteName(String stringArg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns a Unicode string with the delimiters added to make the input string a valid Microsoft SQL Server delimited identifier.</summary>
        /// <returns>The original string with the specified quote characters added.</returns>
        /// <param name="stringArg">The expression that quote characters will be added to.</param>
        /// <param name="quoteCharacter">The one-character string to use as the delimiter. It can be a single quotation mark ( ' ), a left or right bracket ( [ ] ), or a double quotation mark ( " ). If quote_character is not specified, brackets are used.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "quoteCharacter")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stringArg")]
        [DbFunction("SqlServer", "QUOTENAME")]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
        public static String QuoteName(String stringArg, String quoteCharacter)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Repeats a string value a specified number of times.</summary>
        /// <returns>The target string, repeated the number of times specified by  count .</returns>
        /// <param name="target">A valid string.</param>
        /// <param name="count">The value that specifies how many time to repeat  target .</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "count")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "target")]
        [DbFunction("SqlServer", "REPLICATE")]
        public static String Replicate(String target, Int32? count)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Converts an alphanumeric string to a four-character (SOUNDEX) code to find similar-sounding words or names.</summary>
        /// <returns>The SOUNDEX code of the input string.</returns>
        /// <param name="arg">A valid string.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "SOUNDEX")]
        public static String SoundCode(String arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns a string of repeated spaces.</summary>
        /// <returns>A string that consists of the specified number of spaces.</returns>
        /// <param name="arg1">The number of spaces. If negative, a null string is returned.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "SPACE")]
        public static String Space(Int32? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns character data converted from numeric data.</summary>
        /// <returns>The numeric input expression converted to a string.</returns>
        /// <param name="number">A numeric expression.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number")]
        [DbFunction("SqlServer", "STR")]
        public static String StringConvert(Double? number)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns character data converted from numeric data.</summary>
        /// <returns>The input expression converted to a string.</returns>
        /// <param name="number">A numeric expression.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number")]
        [DbFunction("SqlServer", "STR")]
        public static String StringConvert(Decimal? number)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns character data converted from numeric data.</summary>
        /// <returns>The numeric input expression converted to a string.</returns>
        /// <param name="number">A numeric expression.</param>
        /// <param name="length">The total length of the string. This includes decimal point, sign, digits, and spaces. The default is 10.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "length")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number")]
        [DbFunction("SqlServer", "STR")]
        public static String StringConvert(Double? number, Int32? length)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns character data converted from numeric data.</summary>
        /// <returns>The input expression converted to a string.</returns>
        /// <param name="number">A numeric expression.</param>
        /// <param name="length">The total length of the string. This includes decimal point, sign, digits, and spaces. The default is 10.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "length")]
        [DbFunction("SqlServer", "STR")]
        public static String StringConvert(Decimal? number, Int32? length)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns character data converted from numeric data.</summary>
        /// <returns>The numeric input expression converted to a string.</returns>
        /// <param name="number">A numeric expression.</param>
        /// <param name="length">The total length of the string. This includes decimal point, sign, digits, and spaces. The default is 10.</param>
        /// <param name="decimalArg">The number of places to the right of the decimal point.  decimal  must be less than or equal to 16. If  decimal  is more than 16 then the result is truncated to sixteen places to the right of the decimal point.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "length")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "decimalArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number")]
        [DbFunction("SqlServer", "STR")]
        public static String StringConvert(Double? number, Int32? length, Int32? decimalArg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns character data converted from numeric data.</summary>
        /// <returns>The input expression converted to a string.</returns>
        /// <param name="number">A numeric expression.</param>
        /// <param name="length">The total length of the string. This includes decimal point, sign, digits, and spaces. The default is 10.</param>
        /// <param name="decimalArg">The number of places to the right of the decimal point.  decimal  must be less than or equal to 16. If  decimal  is more than 16 then the result is truncated to sixteen places to the right of the decimal point.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "decimalArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "length")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number")]
        [DbFunction("SqlServer", "STR")]
        public static String StringConvert(Decimal? number, Int32? length, Int32? decimalArg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Inserts a string into another string. It deletes a specified length of characters in the target string at the start position and then inserts the second string into the target string at the start position.</summary>
        /// <returns>A string consisting of the two strings.</returns>
        /// <param name="stringInput">The target string.</param>
        /// <param name="start">The character position in  stringinput  where the replacement string is to be inserted.</param>
        /// <param name="length">The number of characters to delete from  stringInput . If  length  is longer than  stringInput , deletion occurs up to the last character in  stringReplacement .</param>
        /// <param name="stringReplacement">The substring to be inserted into  stringInput .</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "start")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "length")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stringInput")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stringReplacement")]
        [DbFunction("SqlServer", "STUFF")]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
        public static String Stuff(String stringInput, Int32? start, Int32? length, String stringReplacement)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the integer value, as defined by the Unicode standard, for the first character of the input expression.</summary>
        /// <returns>The character code for the first character in the input string.</returns>
        /// <param name="arg">A valid string.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "UNICODE")]
        public static Int32? Unicode(String arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>A mathematical function that returns the angle, in radians, whose cosine is the specified numerical value. This angle is called the arccosine.</summary>
        /// <returns>The angle, in radians, defined by the input cosine value.</returns>
        /// <param name="arg1">The cosine of an angle.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "ACOS")]
        public static Double? Acos(Double? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>A mathematical function that returns the angle, in radians, whose cosine is the specified numerical value. This angle is called the arccosine.</summary>
        /// <returns>An angle, measured in radians.</returns>
        /// <param name="arg1">The cosine of an angle.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "ACOS")]
        public static Double? Acos(Decimal? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>A mathematical function that returns the angle, in radians, whose sine is the specified numerical value. This angle is called the arcsine.</summary>
        /// <returns>An angle, measured in radians.</returns>
        /// <param name="arg">The sine of an angle.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "ASIN")]
        public static Double? Asin(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>A mathematical function that returns the angle, in radians, whose sine is the specified numerical value. This angle is called the arcsine.</summary>
        /// <returns>An angle, measured in radians.</returns>
        /// <param name="arg">The sine of an angle.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "ASIN")]
        public static Double? Asin(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>A mathematical function that returns the angle, in radians, whose tangent is the specified numerical value. This angle is called the arctangent.</summary>
        /// <returns>An angle, measured in radians.</returns>
        /// <param name="arg">The tangent of an angle.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "ATAN")]
        public static Double? Atan(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>A mathematical function that returns the angle, in radians, whose tangent is the specified numerical value. This angle is called the arctangent.</summary>
        /// <returns>An angle, measured in radians.</returns>
        /// <param name="arg">The tangent of an angle.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "ATAN")]
        public static Double? Atan(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the positive angle, in radians, between the positive x-axis and the ray from the origin through the point (x, y), where x and y are the two specified numerical values. The first parameter passed to the function is the y-value and the second parameter is the x-value.</summary>
        /// <returns>An angle, measured in radians.</returns>
        /// <param name="arg1">The y-coordinate of a point.</param>
        /// <param name="arg2">The x-coordinate of a point.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [DbFunction("SqlServer", "ATN2")]
        public static Double? Atan2(Double? arg1, Double? arg2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the positive angle, in radians, between the positive x-axis and the ray from the origin through the point (x, y), where x and y are the two specified numerical values. The first parameter passed to the function is the y-value and the second parameter is the x-value.</summary>
        /// <returns>An angle, measured in radians.</returns>
        /// <param name="arg1">The y-coordinate of a point.</param>
        /// <param name="arg2">The x-coordinate of a point.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "ATN2")]
        public static Double? Atan2(Decimal? arg1, Decimal? arg2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the trigonometric cosine of the specified angle, in radians, in the specified expression.</summary>
        /// <returns>The trigonometric cosine of the specified angle.</returns>
        /// <param name="arg">An angle, measured in radians.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "COS")]
        public static Double? Cos(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the trigonometric cosine of the specified angle, in radians, in the specified expression.</summary>
        /// <returns>The trigonometric cosine of the specified angle.</returns>
        /// <param name="arg">An angle, measured in radians.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "COS")]
        public static Double? Cos(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>A mathematical function that returns the trigonometric cotangent of the specified angle, in radians.</summary>
        /// <returns>The trigonometric cotangent of the specified angle.</returns>
        /// <param name="arg">An angle, measured in radians.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "COT")]
        public static Double? Cot(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>A mathematical function that returns the trigonometric cotangent of the specified angle, in radians.</summary>
        /// <returns>The trigonometric cotangent of the specified angle.</returns>
        /// <param name="arg">An angle, measured in radians.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "COT")]
        public static Double? Cot(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the corresponding angle in degrees for an angle specified in radians.</summary>
        /// <returns>The specified angle converted to degrees.</returns>
        /// <param name="arg1">An angle, measured in radians.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "DEGREES")]
        public static Int32? Degrees(Int32? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the corresponding angle in degrees for an angle specified in radians.</summary>
        /// <returns>The specified angle converted to degrees.</returns>
        /// <param name="arg1">An angle, measured in radians.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "DEGREES")]
        public static Int64? Degrees(Int64? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the corresponding angle in degrees for an angle specified in radians.</summary>
        /// <returns>The specified angle converted to degrees.</returns>
        /// <param name="arg1">An angle, measured in radians.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "DEGREES")]
        public static Decimal? Degrees(Decimal? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the corresponding angle in degrees for an angle specified in radians.</summary>
        /// <returns>The specified angle converted to degrees.</returns>
        /// <param name="arg1">An angle, measured in radians.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "DEGREES")]
        public static Double? Degrees(Double? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the exponential value of the specified float expression.</summary>
        /// <returns>The constant e raised to the power of the input value.</returns>
        /// <param name="arg">The input value.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Exp")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "EXP")]
        public static Double? Exp(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the exponential value of the specified float expression.</summary>
        /// <returns>The constant e raised to the power of the input value.</returns>
        /// <param name="arg">The input value.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Exp")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "EXP")]
        public static Double? Exp(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the natural logarithm of the specified input value.</summary>
        /// <returns>The natural logarithm of the input value.</returns>
        /// <param name="arg">A numeric expression.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "LOG")]
        public static Double? Log(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the natural logarithm of the specified input value.</summary>
        /// <returns>The natural logarithm of the input value.</returns>
        /// <param name="arg">A numeric expression.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "LOG")]
        public static Double? Log(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the base-10 logarithm of the specified input value.</summary>
        /// <returns>The base-10 logarithm of the input value.</returns>
        /// <param name="arg">A numeric expression.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "LOG10")]
        public static Double? Log10(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the base-10 logarithm of the specified input value.</summary>
        /// <returns>The base-10 logarithm of the input value.</returns>
        /// <param name="arg">A numeric expression.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "LOG10")]
        public static Double? Log10(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the constant value of pi.</summary>
        /// <returns>The numeric value of pi.</returns>
        [DbFunction("SqlServer", "PI")]
        public static Double? Pi()
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the radian measure corresponding to the specified angle in degrees.</summary>
        /// <returns>The radian measure of the specified angle.</returns>
        /// <param name="arg">The angle, measured in degrees</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "RADIANS")]
        public static Int32? Radians(Int32? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the radian measure corresponding to the specified angle in degrees.</summary>
        /// <returns>The radian measure of the specified angle.</returns>
        /// <param name="arg">The angle, measured in degrees</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "RADIANS")]
        public static Int64? Radians(Int64? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the radian measure corresponding to the specified angle in degrees.</summary>
        /// <returns>The radian measure of the specified angle.</returns>
        /// <param name="arg">The angle, measured in degrees.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "RADIANS")]
        public static Decimal? Radians(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the radian measure corresponding to the specified angle in degrees.</summary>
        /// <returns>The radian measure of the specified angle.</returns>
        /// <param name="arg">The angle, measured in degrees.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "RADIANS")]
        public static Double? Radians(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns a pseudo-random float value from 0 through 1, exclusive.</summary>
        /// <returns>The pseudo-random value.</returns>
        [DbFunction("SqlServer", "RAND")]
        public static Double? Rand()
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns a pseudo-random float value from 0 through 1, exclusive.</summary>
        /// <returns>The pseudo-random value.</returns>
        /// <param name="seed">The seed value. If  seed  is not specified, the SQL Server Database Engine assigns a seed value at random. For a specified seed value, the result returned is always the same.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "seed")]
        [DbFunction("SqlServer", "RAND")]
        public static Double? Rand(Int32? seed)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the positive (+1), zero (0), or negative (-1) sign of the specified expression.</summary>
        /// <returns>The sign of the input expression.</returns>
        /// <param name="arg">A numeric expression.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "SIGN")]
        public static Int32? Sign(Int32? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the positive (+1), zero (0), or negative (-1) sign of the specified expression.</summary>
        /// <returns>The sign of the input expression.</returns>
        /// <param name="arg">A numeric expression.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "SIGN")]
        public static Int64? Sign(Int64? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the positive (+1), zero (0), or negative (-1) sign of the specified expression.</summary>
        /// <returns>The sign of the input expression.</returns>
        /// <param name="arg">A numeric expression.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "SIGN")]
        public static Decimal? Sign(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the positive (+1), zero (0), or negative (-1) sign of the specified expression.</summary>
        /// <returns>The sign of the input expression.</returns>
        /// <param name="arg">A numeric expression.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "SIGN")]
        public static Double? Sign(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the trigonometric sine of the specified angle.</summary>
        /// <returns>The trigonometric sine of the input expression.</returns>
        /// <param name="arg">An angle, measured in radians.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "SIN")]
        public static Double? Sin(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the trigonometric sine of the specified angle.</summary>
        /// <returns>The trigonometric sine of the input expression.</returns>
        /// <param name="arg">An angle, measured in radians.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "SIN")]
        public static Double? Sin(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the square root of the specified number.</summary>
        /// <returns>The square root of the input value.</returns>
        /// <param name="arg">A numeric expression.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "SQRT")]
        public static Double? SquareRoot(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the square root of the specified number.</summary>
        /// <returns>The square root of the input value.</returns>
        /// <param name="arg">A numeric expression.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "SQRT")]
        public static Double? SquareRoot(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the square of the specified number.</summary>
        /// <returns>The square of the input value.</returns>
        /// <param name="arg1">A numeric expression.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "SQUARE")]
        public static Double? Square(Double? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the square of the specified number.</summary>
        /// <returns>The square of the input value.</returns>
        /// <param name="arg1">A numeric expression.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "SQUARE")]
        public static Double? Square(Decimal? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the trigonometric tangent of the input expression.</summary>
        /// <returns>The tangent of the input angle.</returns>
        /// <param name="arg">An angle, measured in radians.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "TAN")]
        public static Double? Tan(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the trigonometric tangent of the input expression.</summary>
        /// <returns>The tangent of the input angle.</returns>
        /// <param name="arg">An angle, measured in radians.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "TAN")]
        public static Double? Tan(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns a new datetime value based on adding an interval to the specified date.</summary>
        /// <returns>The new date.</returns>
        /// <param name="datePartArg">The part of the date to increment. </param>
        /// <param name="number">The value used to increment a date by a specified amount.</param>
        /// <param name="date">The date to increment.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "date")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [DbFunction("SqlServer", "DATEADD")]
        public static DateTime? DateAdd(String datePartArg, Double? number, DateTime? date)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns a new time span value based on adding an interval to the specified time span.</summary>
        /// <returns>The new time span.</returns>
        /// <param name="datePartArg">The part of the date to increment.</param>
        /// <param name="number">The value used to increment a date by a specified amount.</param>
        /// <param name="time">The time span to increment.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "time")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [DbFunction("SqlServer", "DATEADD")]
        public static TimeSpan? DateAdd(String datePartArg, Double? number, TimeSpan? time)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns a new date value based on adding an interval to the specified date.</summary>
        /// <returns>The new point in time, expressed as a date and time of day, relative to Coordinated Universal Time (UTC).</returns>
        /// <param name="datePartArg">The part of the date to increment.</param>
        /// <param name="number">The value used to increment a date by a specified amount.</param>
        /// <param name="dateTimeOffsetArg">The date to increment.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateTimeOffsetArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [DbFunction("SqlServer", "DATEADD")]
        public static DateTimeOffset? DateAdd(String datePartArg, Double? number, DateTimeOffset? dateTimeOffsetArg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns a new datetime value based on adding an interval to the specified date.</summary>
        /// <returns>
        /// A <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTime" /> value that is the new date.
        /// </returns>
        /// <param name="datePartArg">The part of the date to increment.</param>
        /// <param name="number">The value used to increment a date by a specified amount.</param>
        /// <param name="date">The date to increment.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "date")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number")]
        [DbFunction("SqlServer", "DATEADD")]
        public static DateTime? DateAdd(String datePartArg, Double? number, String date)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the count of the specified datepart boundaries crossed between the specified start date and end date.</summary>
        /// <returns>The number of time intervals between the two dates.</returns>
        /// <param name="datePartArg">The part of the date to calculate the differing number of time intervals.</param>
        /// <param name="startDate">The first date.</param>
        /// <param name="endDate">The second date.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [DbFunction("SqlServer", "DATEDIFF")]
        public static Int32? DateDiff(String datePartArg, DateTime? startDate, DateTime? endDate)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the count of the specified datepart boundaries crossed between the specified start date and end date.</summary>
        /// <returns>The number of time intervals between the two dates.</returns>
        /// <param name="datePartArg">The part of the date to calculate the differing number of time intervals.</param>
        /// <param name="startDate">The first date.</param>
        /// <param name="endDate">The second date.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate")]
        [DbFunction("SqlServer", "DATEDIFF")]
        public static Int32? DateDiff(String datePartArg, DateTimeOffset? startDate, DateTimeOffset? endDate)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the count of the specified datepart boundaries crossed between the specified start date and end date.</summary>
        /// <returns>The number of time intervals between the two dates.</returns>
        /// <param name="datePartArg">The part of the date to calculate the differing number of time intervals.</param>
        /// <param name="startDate">The first date.</param>
        /// <param name="endDate">The second date.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate")]
        [DbFunction("SqlServer", "DATEDIFF")]
        public static Int32? DateDiff(String datePartArg, TimeSpan? startDate, TimeSpan? endDate)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the count of the specified datepart boundaries crossed between the specified start date and end date.</summary>
        /// <returns>The number of time intervals between the two dates.</returns>
        /// <param name="datePartArg">The part of the date to calculate the differing number of time intervals.</param>
        /// <param name="startDate">The first date.</param>
        /// <param name="endDate">The second date.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate")]
        [DbFunction("SqlServer", "DATEDIFF")]
        public static Int32? DateDiff(String datePartArg, String startDate, DateTime? endDate)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the count of the specified datepart boundaries crossed between the specified start date and end date.</summary>
        /// <returns>The number of time intervals between the two dates.</returns>
        /// <param name="datePartArg">The part of the date to calculate the differing number of time intervals.</param>
        /// <param name="startDate">The first date.</param>
        /// <param name="endDate">The second date.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate")]
        [DbFunction("SqlServer", "DATEDIFF")]
        public static Int32? DateDiff(String datePartArg, String startDate, DateTimeOffset? endDate)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the count of the specified datepart boundaries crossed between the specified start date and end date.</summary>
        /// <returns>The value specifying the number of time intervals between the two dates.</returns>
        /// <param name="datePartArg">The part of the date to calculate the differing number of time intervals.</param>
        /// <param name="startDate">The first date.</param>
        /// <param name="endDate">The second date.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [DbFunction("SqlServer", "DATEDIFF")]
        public static Int32? DateDiff(String datePartArg, String startDate, TimeSpan? endDate)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the count of the specified datepart boundaries crossed between the specified start date and end date.</summary>
        /// <returns>The number of time intervals between the two dates.</returns>
        /// <param name="datePartArg">The part of the date to calculate the differing number of time intervals.</param>
        /// <param name="startDate">The first date.</param>
        /// <param name="endDate">The second date.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate")]
        [DbFunction("SqlServer", "DATEDIFF")]
        public static Int32? DateDiff(String datePartArg, TimeSpan? startDate, String endDate)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the count of the specified datepart boundaries crossed between the specified start date and end date.</summary>
        /// <returns>The number of time intervals between the two dates.</returns>
        /// <param name="datePartArg">The part of the date to calculate the differing number of time intervals.</param>
        /// <param name="startDate">The first date.</param>
        /// <param name="endDate">The second date.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [DbFunction("SqlServer", "DATEDIFF")]
        public static Int32? DateDiff(String datePartArg, DateTime? startDate, String endDate)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the count of the specified datepart boundaries crossed between the specified start date and end date.</summary>
        /// <returns>The number of time intervals between the two dates.</returns>
        /// <param name="datePartArg">The part of the date to calculate the differing number of time intervals.</param>
        /// <param name="startDate">The first date.</param>
        /// <param name="endDate">The second date.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [DbFunction("SqlServer", "DATEDIFF")]
        public static Int32? DateDiff(String datePartArg, DateTimeOffset? startDate, String endDate)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the count of the specified datepart boundaries crossed between the specified start date and end date.</summary>
        /// <returns>The number of time intervals between the two dates.</returns>
        /// <param name="datePartArg">The part of the date to calculate the differing number of time intervals.</param>
        /// <param name="startDate">The first date.</param>
        /// <param name="endDate">The second date.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate")]
        [DbFunction("SqlServer", "DATEDIFF")]
        public static Int32? DateDiff(String datePartArg, String startDate, String endDate)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the count of the specified datepart boundaries crossed between the specified start date and end date.</summary>
        /// <returns>The number of time intervals between the two dates.</returns>
        /// <param name="datePartArg">The part of the date to calculate the differing number of time intervals.</param>
        /// <param name="startDate">The first date.</param>
        /// <param name="endDate">The second date.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [DbFunction("SqlServer", "DATEDIFF")]
        public static Int32? DateDiff(String datePartArg, TimeSpan? startDate, DateTime? endDate)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the count of the specified datepart boundaries crossed between the specified start date and end date.</summary>
        /// <returns>The number of time intervals between the two dates.</returns>
        /// <param name="datePartArg">The part of the date to calculate the differing number of time intervals.</param>
        /// <param name="startDate">The first date.</param>
        /// <param name="endDate">The second date.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate")]
        [DbFunction("SqlServer", "DATEDIFF")]
        public static Int32? DateDiff(String datePartArg, TimeSpan? startDate, DateTimeOffset? endDate)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the count of the specified datepart boundaries crossed between the specified start date and end date.</summary>
        /// <returns>The number of time intervals between the two dates.</returns>
        /// <param name="datePartArg">The part of the date to calculate the differing number of time intervals.</param>
        /// <param name="startDate">The first date.</param>
        /// <param name="endDate">The second date.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [DbFunction("SqlServer", "DATEDIFF")]
        public static Int32? DateDiff(String datePartArg, DateTime? startDate, TimeSpan? endDate)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the count of the specified datepart boundaries crossed between the specified start date and end date.</summary>
        /// <returns>The number of time intervals between the two Dates.</returns>
        /// <param name="datePartArg">The part of the date to calculate the differing number of time intervals.</param>
        /// <param name="startDate">The first date.</param>
        /// <param name="endDate">The second date.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate")]
        [DbFunction("SqlServer", "DATEDIFF")]
        public static Int32? DateDiff(String datePartArg, DateTimeOffset? startDate, TimeSpan? endDate)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the count of the specified datepart boundaries crossed between the specified start date and end date.</summary>
        /// <returns>The number of time intervals between the two dates.</returns>
        /// <param name="datePartArg">The part of the date to calculate the differing number of time intervals.</param>
        /// <param name="startDate">The first date.</param>
        /// <param name="endDate">The second date.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [DbFunction("SqlServer", "DATEDIFF")]
        public static Int32? DateDiff(String datePartArg, DateTime? startDate, DateTimeOffset? endDate)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the count of the specified datepart boundaries crossed between the specified start date and end date.</summary>
        /// <returns>The number of time intervals between the two dates.</returns>
        /// <param name="datePartArg">The part of the date to calculate the differing number of time intervals.</param>
        /// <param name="startDate">The first date.</param>
        /// <param name="endDate">The second date.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate")]
        [DbFunction("SqlServer", "DATEDIFF")]
        public static Int32? DateDiff(String datePartArg, DateTimeOffset? startDate, DateTime? endDate)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns a character string that represents the specified datepart of the specified date.</summary>
        /// <returns>The specified part of the specified date.</returns>
        /// <param name="datePartArg">The part of the date to calculate the differing number of time intervals.</param>
        /// <param name="date">The date.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "date")]
        [DbFunction("SqlServer", "DATENAME")]
        public static String DateName(String datePartArg, DateTime? date)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns a character string that represents the specified datepart of the specified date.</summary>
        /// <returns>The specified part of the specified date.</returns>
        /// <param name="datePartArg">The part of the date to calculate the differing number of time intervals.</param>
        /// <param name="date">The date.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "date")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [DbFunction("SqlServer", "DATENAME")]
        public static String DateName(String datePartArg, String date)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns a character string that represents the specified datepart of the specified date.</summary>
        /// <returns>The specified part of the specified date.</returns>
        /// <param name="datePartArg">The part of the date to calculate the differing number of time intervals.</param>
        /// <param name="date">The date.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "date")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [DbFunction("SqlServer", "DATENAME")]
        public static String DateName(String datePartArg, TimeSpan? date)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns a character string that represents the specified datepart of the specified date.</summary>
        /// <returns>The specified part of the specified date.</returns>
        /// <param name="datePartArg">The part of the date to calculate the differing number of time intervals.</param>
        /// <param name="date">The date.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "date")]
        [DbFunction("SqlServer", "DATENAME")]
        public static String DateName(String datePartArg, DateTimeOffset? date)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns an integer that represents the specified datepart of the specified date.</summary>
        /// <returns>The the specified datepart of the specified date.</returns>
        /// <param name="datePartArg">The part of the date to return the value.</param>
        /// <param name="date">The date.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "date")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [DbFunction("SqlServer", "DATEPART")]
        public static Int32? DatePart(String datePartArg, DateTime? date)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns an integer that represents the specified datepart of the specified date.</summary>
        /// <returns>The specified datepart of the specified date.</returns>
        /// <param name="datePartArg">The part of the date to return the value.</param>
        /// <param name="date">The date.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "date")]
        [DbFunction("SqlServer", "DATEPART")]
        public static Int32? DatePart(String datePartArg, DateTimeOffset? date)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns an integer that represents the specified datepart of the specified date.</summary>
        /// <returns>The specified datepart of the specified date.</returns>
        /// <param name="datePartArg">The part of the date to return the value.</param>
        /// <param name="date">The date.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "date")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [DbFunction("SqlServer", "DATEPART")]
        public static Int32? DatePart(String datePartArg, String date)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns an integer that represents the specified datepart of the specified date.</summary>
        /// <returns>The specified datepart of the specified date.</returns>
        /// <param name="datePartArg">The part of the date to return the value.</param>
        /// <param name="date">The date.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "date")]
        [DbFunction("SqlServer", "DATEPART")]
        public static Int32? DatePart(String datePartArg, TimeSpan? date)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the current database system timestamp as a datetime value without the database time zone offset. This value is derived from the operating system of the computer on which the instance of SQL Server is running.</summary>
        /// <returns>The current database timestamp.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [DbFunction("SqlServer", "GETDATE")]
        public static DateTime? GetDate()
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the current database system timestamp as a datetime value. The database time zone offset is not included. This value represents the current UTC time (Coordinated Universal Time). This value is derived from the operating system of the computer on which the instance of SQL Server is running.</summary>
        /// <returns>The current database UTC timestamp.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [DbFunction("SqlServer", "GETUTCDATE")]
        public static DateTime? GetUtcDate()
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the number of bytes used to represent any expression.</summary>
        /// <returns>The number of bytes in the input value.</returns>
        /// <param name="arg">The value to be examined for data length.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "DATALENGTH")]
        public static Int32? DataLength(Boolean? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the number of bytes used to represent any expression.</summary>
        /// <returns>The number of bytes in the input value.</returns>
        /// <param name="arg">The value to be examined for data length.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "DATALENGTH")]
        public static Int32? DataLength(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the number of bytes used to represent any expression.</summary>
        /// <returns>The number of bytes in the input value.</returns>
        /// <param name="arg">The value to be examined for data length.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "DATALENGTH")]
        public static Int32? DataLength(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the number of bytes used to represent any expression.</summary>
        /// <returns>The number of bytes in the input value.</returns>
        /// <param name="arg">The value to be examined for data length.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "DATALENGTH")]
        public static Int32? DataLength(DateTime? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the number of bytes used to represent any expression.</summary>
        /// <returns>The number of bytes in the input value.</returns>
        /// <param name="arg">The value to be examined for data length.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "DATALENGTH")]
        public static Int32? DataLength(TimeSpan? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the number of bytes used to represent any expression.</summary>
        /// <returns>The number of bytes in the input value.</returns>
        /// <param name="arg">The value to be examined for data length.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "DATALENGTH")]
        public static Int32? DataLength(DateTimeOffset? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the number of bytes used to represent any expression.</summary>
        /// <returns>The number of bytes in the input value.</returns>
        /// <param name="arg">The value to be examined for data length.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "DATALENGTH")]
        public static Int32? DataLength(String arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the number of bytes used to represent any expression.</summary>
        /// <returns>The number of bytes in the input value.</returns>
        /// <param name="arg">The value to be examined for length.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "DATALENGTH")]
        public static Int32? DataLength(Byte[] arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the number of bytes used to represent any expression.</summary>
        /// <returns>The number of bytes in the input value.</returns>
        /// <param name="arg">The value to be examined for data length.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "DATALENGTH")]
        public static Int32? DataLength(Guid? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the checksum value computed over the input argument.</summary>
        /// <returns>The checksum computed over the input value.</returns>
        /// <param name="arg1">The value for which the checksum is calculated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(Boolean? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the checksum value computed over the input argument.</summary>
        /// <returns>The checksum computed over the input value.</returns>
        /// <param name="arg1">The value for which the checksum is calculated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(Double? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the checksum value computed over the input argument.</summary>
        /// <returns>The checksum computed over the input value.</returns>
        /// <param name="arg1">The value for which the checksum is calculated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(Decimal? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the checksum value computed over the input argument.</summary>
        /// <returns>The checksum computed over the input value.</returns>
        /// <param name="arg1">The value for which the checksum is calculated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(String arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the checksum value computed over the input argument.</summary>
        /// <returns>The checksum computed over the input values.</returns>
        /// <param name="arg1">The value for which the checksum is calculated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(DateTime? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the checksum value computed over the input argument.</summary>
        /// <returns>The checksum computed over the input value.</returns>
        /// <param name="arg1">The value for which the checksum is calculated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(TimeSpan? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the checksum value computed over the input argument.</summary>
        /// <returns>The checksum computed over the input value.</returns>
        /// <param name="arg1">The value for which the checksum is calculated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(DateTimeOffset? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the checksum value computed over the input argument.</summary>
        /// <returns>The checksum computed over the input value.</returns>
        /// <param name="arg1">The character array for which the checksum is calculated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(Byte[] arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the checksum value computed over the input argument.</summary>
        /// <returns>The checksum computed over the input value.</returns>
        /// <param name="arg1">The value for which the checksum is calculated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(Guid? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the checksum value computed over the input arguments.</summary>
        /// <returns>The checksum computed over the input values.</returns>
        /// <param name="arg1">The value for which the checksum is calculated.</param>
        /// <param name="arg2">The value for which the checksum is calculated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(Boolean? arg1, Boolean? arg2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the checksum value computed over the input arguments.</summary>
        /// <returns>The checksum computed over the input values.</returns>
        /// <param name="arg1">The value for which the checksum is calculated.</param>
        /// <param name="arg2">The value for which the checksum is calculated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(Double? arg1, Double? arg2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the checksum value computed over the input arguments.</summary>
        /// <returns>The checksum computed over the input values.</returns>
        /// <param name="arg1">The value for which the checksum is calculated.</param>
        /// <param name="arg2">The value for which the checksum is calculated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(Decimal? arg1, Decimal? arg2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the checksum value computed over the input arguments.</summary>
        /// <returns>The checksum computed over the input values.</returns>
        /// <param name="arg1">The value for which the checksum is calculated.</param>
        /// <param name="arg2">The value for which the checksum is calculated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(String arg1, String arg2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the checksum value computed over the input arguments.</summary>
        /// <returns>The checksum computed over the input values.</returns>
        /// <param name="arg1">The value for which the checksum is calculated.</param>
        /// <param name="arg2">The value for which the checksum is calculated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(DateTime? arg1, DateTime? arg2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the checksum value computed over the input arguments.</summary>
        /// <returns>The checksum computed over the input values.</returns>
        /// <param name="arg1">The value for which the checksum is calculated.</param>
        /// <param name="arg2">The value for which the checksum is calculated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(TimeSpan? arg1, TimeSpan? arg2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the checksum value computed over the input arguments.</summary>
        /// <returns>The checksum computed over the input values.</returns>
        /// <param name="arg1">The value for which the checksum is calculated.</param>
        /// <param name="arg2">The value for which the checksum is calculated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(DateTimeOffset? arg1, DateTimeOffset? arg2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the checksum value computed over the input arguments.</summary>
        /// <returns>The checksum computed over the input values.</returns>
        /// <param name="arg1">The character array for which the checksum is calculated.</param>
        /// <param name="arg2">The character array for which the checksum is calculated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(Byte[] arg1, Byte[] arg2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the checksum value computed over the input arguments.</summary>
        /// <returns>The checksum computed over the input values.</returns>
        /// <param name="arg1">The value for which the checksum is calculated.</param>
        /// <param name="arg2">The value for which the checksum is calculated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(Guid? arg1, Guid? arg2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the checksum value computed over the input arguments.</summary>
        /// <returns>The checksum computed over the input values.</returns>
        /// <param name="arg1">The value for which the checksum is calculated.</param>
        /// <param name="arg2">The value for which the checksum is calculated.</param>
        /// <param name="arg3">The value for which the checksum is calculated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg3")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(Boolean? arg1, Boolean? arg2, Boolean? arg3)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the checksum value computed over the input arguments.</summary>
        /// <returns>The checksum computed over the input values.</returns>
        /// <param name="arg1">The value for which the checksum is calculated.</param>
        /// <param name="arg2">The value for which the checksum is calculated.</param>
        /// <param name="arg3">The value for which the checksum is calculated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg3")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(Double? arg1, Double? arg2, Double? arg3)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the checksum value computed over the input arguments.</summary>
        /// <returns>The checksum computed over the input values.</returns>
        /// <param name="arg1">The value for which the checksum is calculated.</param>
        /// <param name="arg2">The value for which the checksum is calculated.</param>
        /// <param name="arg3">The value for which the checksum is calculated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg3")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(Decimal? arg1, Decimal? arg2, Decimal? arg3)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the checksum value computed over the input arguments.</summary>
        /// <returns>The checksum computed over the input values.</returns>
        /// <param name="arg1">The value for which the checksum is calculated.</param>
        /// <param name="arg2">The value for which the checksum is calculated.</param>
        /// <param name="arg3">The value for which the checksum is calculated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg3")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(String arg1, String arg2, String arg3)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the checksum value computed over the input arguments.</summary>
        /// <returns>The checksum computed over the input values.</returns>
        /// <param name="arg1">The value for which the checksum is calculated.</param>
        /// <param name="arg2">The value for which the checksum is calculated.</param>
        /// <param name="arg3">The value for which the checksum is calculated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg3")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(DateTime? arg1, DateTime? arg2, DateTime? arg3)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the checksum value computed over the input arguments.</summary>
        /// <returns>The checksum computed over the input values.</returns>
        /// <param name="arg1">The value for which the checksum is calculated.</param>
        /// <param name="arg2">The value for which the checksum is calculated.</param>
        /// <param name="arg3">The value for which the checksum is calculated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg3")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(DateTimeOffset? arg1, DateTimeOffset? arg2, DateTimeOffset? arg3)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the checksum value computed over the input arguments.</summary>
        /// <returns>The checksum computed over the input values.</returns>
        /// <param name="arg1">The value for which the checksum is calculated.</param>
        /// <param name="arg2">The value for which the checksum is calculated.</param>
        /// <param name="arg3">The value for which the checksum is calculated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg3")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(TimeSpan? arg1, TimeSpan? arg2, TimeSpan? arg3)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the checksum value computed over the input arguments.</summary>
        /// <returns>The checksum computed over the input values.</returns>
        /// <param name="arg1">The character array for which the checksum is calculated.</param>
        /// <param name="arg2">The character array for which the checksum is calculated.</param>
        /// <param name="arg3">The character array for which the checksum is calculated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg3")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(Byte[] arg1, Byte[] arg2, Byte[] arg3)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the checksum value computed over the input arguments.</summary>
        /// <returns>The checksum computed over the input values.</returns>
        /// <param name="arg1">The value for which the checksum is calculated.</param>
        /// <param name="arg2">The value for which the checksum is calculated.</param>
        /// <param name="arg3">The value for which the checksum is calculated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg3")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(Guid? arg1, Guid? arg2, Guid? arg3)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the current date and time. </summary>
        /// <returns>The current date and time.</returns>
        [DbFunction("SqlServer", "CURRENT_TIMESTAMP")]
        public static DateTime? CurrentTimestamp()
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the name of the current user.</summary>
        /// <returns>The name of the current user.</returns>
        [DbFunction("SqlServer", "CURRENT_USER")]
        public static String CurrentUser()
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the workstation name.</summary>
        /// <returns>The name of the workstation.</returns>
        [DbFunction("SqlServer", "HOST_NAME")]
        public static String HostName()
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns a database user name corresponding to a specified identification number.</summary>
        /// <returns>The user name.</returns>
        /// <param name="arg">A user ID.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "USER_NAME")]
        public static String UserName(Int32? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns a database user name corresponding to a specified identification number.</summary>
        /// <returns>The user name.</returns>
        [DbFunction("SqlServer", "USER_NAME")]
        public static String UserName()
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Indicates whether the input value is a valid numeric type.</summary>
        /// <returns>1 if the input expression is a valid numeric data type; otherwise, 0.</returns>
        /// <param name="arg">A string expression.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "ISNUMERIC")]
        public static Int32? IsNumeric(String arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Indicates whether the input value is a valid date or time.</summary>
        /// <returns>1 if the input expression is a valid date or time value of datetime or smalldatetime data types; otherwise, 0.</returns>
        /// <param name="arg">The tested value.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "ISDATE")]
        public static Int32? IsDate(String arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }
    }
}
