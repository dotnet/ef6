// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Objects.SqlClient
{
    using System.Collections.Generic;
    using System.Data.Entity.Resources;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Contains function stubs that expose SqlServer methods in Linq to Entities.
    /// </summary>
    public static class SqlFunctions
    {
        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM_AGG
        /// </summary>
        [DbFunction("SqlServer", "CHECKSUM_AGG")]
        public static Int32? ChecksumAggregate(IEnumerable<Int32> arg)
        {
            var objectQuerySource = arg as ObjectQuery<Int32>;
            if (objectQuerySource != null)
            {
                return
                    ((IQueryable)objectQuerySource).Provider.Execute<Int32?>(
                        Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(arg)));
            }
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM_AGG
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [DbFunction("SqlServer", "CHECKSUM_AGG")]
        public static Int32? ChecksumAggregate(IEnumerable<Int32?> arg)
        {
            var objectQuerySource = arg as ObjectQuery<Int32?>;
            if (objectQuerySource != null)
            {
                return
                    ((IQueryable)objectQuerySource).Provider.Execute<Int32?>(
                        Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(arg)));
            }
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.ASCII
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ascii")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "ASCII")]
        public static Int32? Ascii(String arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHAR
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "CHAR")]
        public static String Char(Int32? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHARINDEX
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "target")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "toSearch")]
        [DbFunction("SqlServer", "CHARINDEX")]
        public static Int32? CharIndex(String toSearch, String target)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHARINDEX
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "toSearch")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "target")]
        [DbFunction("SqlServer", "CHARINDEX")]
        public static Int32? CharIndex(Byte[] toSearch, Byte[] target)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHARINDEX
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "target")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "toSearch")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startLocation")]
        [DbFunction("SqlServer", "CHARINDEX")]
        public static Int32? CharIndex(String toSearch, String target, Int32? startLocation)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHARINDEX
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startLocation")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "target")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "toSearch")]
        [DbFunction("SqlServer", "CHARINDEX")]
        public static Int32? CharIndex(Byte[] toSearch, Byte[] target, Int32? startLocation)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHARINDEX
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "target")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "toSearch")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startLocation")]
        [DbFunction("SqlServer", "CHARINDEX")]
        public static Int64? CharIndex(String toSearch, String target, Int64? startLocation)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHARINDEX
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "toSearch")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "target")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startLocation")]
        [DbFunction("SqlServer", "CHARINDEX")]
        public static Int64? CharIndex(Byte[] toSearch, Byte[] target, Int64? startLocation)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DIFFERENCE
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "string2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "string1")]
        [DbFunction("SqlServer", "DIFFERENCE")]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
        public static Int32? Difference(String string1, String string2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.NCHAR
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "NCHAR")]
        public static String NChar(Int32? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.PATINDEX
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stringPattern")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "target")]
        [DbFunction("SqlServer", "PATINDEX")]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
        public static Int32? PatIndex(String stringPattern, String target)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.QUOTENAME
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stringArg")]
        [DbFunction("SqlServer", "QUOTENAME")]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
        public static String QuoteName(String stringArg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.QUOTENAME
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "quoteCharacter")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stringArg")]
        [DbFunction("SqlServer", "QUOTENAME")]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
        public static String QuoteName(String stringArg, String quoteCharacter)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.REPLICATE
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "count")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "target")]
        [DbFunction("SqlServer", "REPLICATE")]
        public static String Replicate(String target, Int32? count)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.SOUNDEX
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "SOUNDEX")]
        public static String SoundCode(String arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.SPACE
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "SPACE")]
        public static String Space(Int32? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.STR
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number")]
        [DbFunction("SqlServer", "STR")]
        public static String StringConvert(Double? number)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.STR
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number")]
        [DbFunction("SqlServer", "STR")]
        public static String StringConvert(Decimal? number)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.STR
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "length")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number")]
        [DbFunction("SqlServer", "STR")]
        public static String StringConvert(Double? number, Int32? length)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.STR
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "length")]
        [DbFunction("SqlServer", "STR")]
        public static String StringConvert(Decimal? number, Int32? length)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.STR
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "length")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "decimalArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number")]
        [DbFunction("SqlServer", "STR")]
        public static String StringConvert(Double? number, Int32? length, Int32? decimalArg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.STR
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "decimalArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "length")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number")]
        [DbFunction("SqlServer", "STR")]
        public static String StringConvert(Decimal? number, Int32? length, Int32? decimalArg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.STUFF
        /// </summary>
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

        /// <summary>
        /// Proxy for the function SqlServer.UNICODE
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "UNICODE")]
        public static Int32? Unicode(String arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.ACOS
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "ACOS")]
        public static Double? Acos(Double? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.ACOS
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "ACOS")]
        public static Double? Acos(Decimal? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.ASIN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "ASIN")]
        public static Double? Asin(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.ASIN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "ASIN")]
        public static Double? Asin(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.ATAN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "ATAN")]
        public static Double? Atan(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.ATAN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "ATAN")]
        public static Double? Atan(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.ATN2
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [DbFunction("SqlServer", "ATN2")]
        public static Double? Atan2(Double? arg1, Double? arg2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.ATN2
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "ATN2")]
        public static Double? Atan2(Decimal? arg1, Decimal? arg2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.COS
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "COS")]
        public static Double? Cos(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.COS
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "COS")]
        public static Double? Cos(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.COT
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "COT")]
        public static Double? Cot(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.COT
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "COT")]
        public static Double? Cot(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DEGREES
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "DEGREES")]
        public static Int32? Degrees(Int32? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DEGREES
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "DEGREES")]
        public static Int64? Degrees(Int64? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DEGREES
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "DEGREES")]
        public static Decimal? Degrees(Decimal? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DEGREES
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "DEGREES")]
        public static Double? Degrees(Double? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.EXP
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Exp")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "EXP")]
        public static Double? Exp(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.EXP
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Exp")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "EXP")]
        public static Double? Exp(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.LOG
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "LOG")]
        public static Double? Log(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.LOG
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "LOG")]
        public static Double? Log(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.LOG10
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "LOG10")]
        public static Double? Log10(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.LOG10
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "LOG10")]
        public static Double? Log10(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.PI
        /// </summary>
        [DbFunction("SqlServer", "PI")]
        public static Double? Pi()
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.RADIANS
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "RADIANS")]
        public static Int32? Radians(Int32? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.RADIANS
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "RADIANS")]
        public static Int64? Radians(Int64? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.RADIANS
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "RADIANS")]
        public static Decimal? Radians(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.RADIANS
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "RADIANS")]
        public static Double? Radians(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.RAND
        /// </summary>
        [DbFunction("SqlServer", "RAND")]
        public static Double? Rand()
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.RAND
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "seed")]
        [DbFunction("SqlServer", "RAND")]
        public static Double? Rand(Int32? seed)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.SIGN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "SIGN")]
        public static Int32? Sign(Int32? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.SIGN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "SIGN")]
        public static Int64? Sign(Int64? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.SIGN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "SIGN")]
        public static Decimal? Sign(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.SIGN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "SIGN")]
        public static Double? Sign(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.SIN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "SIN")]
        public static Double? Sin(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.SIN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "SIN")]
        public static Double? Sin(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.SQRT
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "SQRT")]
        public static Double? SquareRoot(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.SQRT
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "SQRT")]
        public static Double? SquareRoot(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.SQUARE
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "SQUARE")]
        public static Double? Square(Double? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.SQUARE
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "SQUARE")]
        public static Double? Square(Decimal? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.TAN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "TAN")]
        public static Double? Tan(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.TAN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "TAN")]
        public static Double? Tan(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEADD
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "date")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [DbFunction("SqlServer", "DATEADD")]
        public static DateTime? DateAdd(String datePartArg, Double? number, DateTime? date)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEADD
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "time")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [DbFunction("SqlServer", "DATEADD")]
        public static TimeSpan? DateAdd(String datePartArg, Double? number, TimeSpan? time)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEADD
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateTimeOffsetArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [DbFunction("SqlServer", "DATEADD")]
        public static DateTimeOffset? DateAdd(String datePartArg, Double? number, DateTimeOffset? dateTimeOffsetArg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEADD
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "date")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number")]
        [DbFunction("SqlServer", "DATEADD")]
        public static DateTime? DateAdd(String datePartArg, Double? number, String date)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEDIFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [DbFunction("SqlServer", "DATEDIFF")]
        public static Int32? DateDiff(String datePartArg, DateTime? startDate, DateTime? endDate)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEDIFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate")]
        [DbFunction("SqlServer", "DATEDIFF")]
        public static Int32? DateDiff(String datePartArg, DateTimeOffset? startDate, DateTimeOffset? endDate)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEDIFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate")]
        [DbFunction("SqlServer", "DATEDIFF")]
        public static Int32? DateDiff(String datePartArg, TimeSpan? startDate, TimeSpan? endDate)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEDIFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate")]
        [DbFunction("SqlServer", "DATEDIFF")]
        public static Int32? DateDiff(String datePartArg, String startDate, DateTime? endDate)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEDIFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate")]
        [DbFunction("SqlServer", "DATEDIFF")]
        public static Int32? DateDiff(String datePartArg, String startDate, DateTimeOffset? endDate)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEDIFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [DbFunction("SqlServer", "DATEDIFF")]
        public static Int32? DateDiff(String datePartArg, String startDate, TimeSpan? endDate)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEDIFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate")]
        [DbFunction("SqlServer", "DATEDIFF")]
        public static Int32? DateDiff(String datePartArg, TimeSpan? startDate, String endDate)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEDIFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [DbFunction("SqlServer", "DATEDIFF")]
        public static Int32? DateDiff(String datePartArg, DateTime? startDate, String endDate)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEDIFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [DbFunction("SqlServer", "DATEDIFF")]
        public static Int32? DateDiff(String datePartArg, DateTimeOffset? startDate, String endDate)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEDIFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate")]
        [DbFunction("SqlServer", "DATEDIFF")]
        public static Int32? DateDiff(String datePartArg, String startDate, String endDate)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEDIFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [DbFunction("SqlServer", "DATEDIFF")]
        public static Int32? DateDiff(String datePartArg, TimeSpan? startDate, DateTime? endDate)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEDIFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate")]
        [DbFunction("SqlServer", "DATEDIFF")]
        public static Int32? DateDiff(String datePartArg, TimeSpan? startDate, DateTimeOffset? endDate)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEDIFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [DbFunction("SqlServer", "DATEDIFF")]
        public static Int32? DateDiff(String datePartArg, DateTime? startDate, TimeSpan? endDate)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEDIFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate")]
        [DbFunction("SqlServer", "DATEDIFF")]
        public static Int32? DateDiff(String datePartArg, DateTimeOffset? startDate, TimeSpan? endDate)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEDIFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [DbFunction("SqlServer", "DATEDIFF")]
        public static Int32? DateDiff(String datePartArg, DateTime? startDate, DateTimeOffset? endDate)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEDIFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate")]
        [DbFunction("SqlServer", "DATEDIFF")]
        public static Int32? DateDiff(String datePartArg, DateTimeOffset? startDate, DateTime? endDate)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATENAME
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "date")]
        [DbFunction("SqlServer", "DATENAME")]
        public static String DateName(String datePartArg, DateTime? date)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATENAME
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "date")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [DbFunction("SqlServer", "DATENAME")]
        public static String DateName(String datePartArg, String date)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATENAME
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "date")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [DbFunction("SqlServer", "DATENAME")]
        public static String DateName(String datePartArg, TimeSpan? date)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATENAME
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "date")]
        [DbFunction("SqlServer", "DATENAME")]
        public static String DateName(String datePartArg, DateTimeOffset? date)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEPART
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "date")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [DbFunction("SqlServer", "DATEPART")]
        public static Int32? DatePart(String datePartArg, DateTime? date)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEPART
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "date")]
        [DbFunction("SqlServer", "DATEPART")]
        public static Int32? DatePart(String datePartArg, DateTimeOffset? date)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEPART
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "date")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [DbFunction("SqlServer", "DATEPART")]
        public static Int32? DatePart(String datePartArg, String date)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEPART
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "date")]
        [DbFunction("SqlServer", "DATEPART")]
        public static Int32? DatePart(String datePartArg, TimeSpan? date)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.GETDATE
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [DbFunction("SqlServer", "GETDATE")]
        public static DateTime? GetDate()
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.GETUTCDATE
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [DbFunction("SqlServer", "GETUTCDATE")]
        public static DateTime? GetUtcDate()
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATALENGTH
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "DATALENGTH")]
        public static Int32? DataLength(Boolean? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATALENGTH
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "DATALENGTH")]
        public static Int32? DataLength(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATALENGTH
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "DATALENGTH")]
        public static Int32? DataLength(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATALENGTH
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "DATALENGTH")]
        public static Int32? DataLength(DateTime? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATALENGTH
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "DATALENGTH")]
        public static Int32? DataLength(TimeSpan? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATALENGTH
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "DATALENGTH")]
        public static Int32? DataLength(DateTimeOffset? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATALENGTH
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "DATALENGTH")]
        public static Int32? DataLength(String arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATALENGTH
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "DATALENGTH")]
        public static Int32? DataLength(Byte[] arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATALENGTH
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "DATALENGTH")]
        public static Int32? DataLength(Guid? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(Boolean? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(Double? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(Decimal? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(String arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(DateTime? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(TimeSpan? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(DateTimeOffset? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(Byte[] arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(Guid? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(Boolean? arg1, Boolean? arg2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(Double? arg1, Double? arg2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(Decimal? arg1, Decimal? arg2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(String arg1, String arg2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(DateTime? arg1, DateTime? arg2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(TimeSpan? arg1, TimeSpan? arg2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(DateTimeOffset? arg1, DateTimeOffset? arg2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(Byte[] arg1, Byte[] arg2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(Guid? arg1, Guid? arg2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg3")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(Boolean? arg1, Boolean? arg2, Boolean? arg3)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg3")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(Double? arg1, Double? arg2, Double? arg3)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg3")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(Decimal? arg1, Decimal? arg2, Decimal? arg3)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg3")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(String arg1, String arg2, String arg3)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg3")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(DateTime? arg1, DateTime? arg2, DateTime? arg3)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg3")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(DateTimeOffset? arg1, DateTimeOffset? arg2, DateTimeOffset? arg3)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg3")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(TimeSpan? arg1, TimeSpan? arg2, TimeSpan? arg3)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg3")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(Byte[] arg1, Byte[] arg2, Byte[] arg3)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg3")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServer", "CHECKSUM")]
        public static Int32? Checksum(Guid? arg1, Guid? arg2, Guid? arg3)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CURRENT_TIMESTAMP
        /// </summary>
        [DbFunction("SqlServer", "CURRENT_TIMESTAMP")]
        public static DateTime? CurrentTimestamp()
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CURRENT_USER
        /// </summary>
        [DbFunction("SqlServer", "CURRENT_USER")]
        public static String CurrentUser()
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.HOST_NAME
        /// </summary>
        [DbFunction("SqlServer", "HOST_NAME")]
        public static String HostName()
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.USER_NAME
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "USER_NAME")]
        public static String UserName(Int32? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.USER_NAME
        /// </summary>
        [DbFunction("SqlServer", "USER_NAME")]
        public static String UserName()
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.ISNUMERIC
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "ISNUMERIC")]
        public static Int32? IsNumeric(String arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.ISDATE
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServer", "ISDATE")]
        public static Int32? IsDate(String arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }
    }
}
