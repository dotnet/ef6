// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServerCompact
{
    using System.Data.Entity.SqlServerCompact.Resources;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Contains function stubs that expose SqlServerCe methods in Linq to Entities.
    /// </summary>
    public static class SqlCeFunctions
    {
        /// <summary>
        /// Proxy for the function SqlServerCe.CHARINDEX
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "target")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "toSearch")]
        [DbFunction("SqlServerCe", "CHARINDEX")]
        public static Int32? CharIndex(String toSearch, String target)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.CHARINDEX
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "toSearch")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "target")]
        [DbFunction("SqlServerCe", "CHARINDEX")]
        public static Int32? CharIndex(Byte[] toSearch, Byte[] target)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.CHARINDEX
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "target")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "toSearch")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startLocation")]
        [DbFunction("SqlServerCe", "CHARINDEX")]
        public static Int32? CharIndex(String toSearch, String target, Int32? startLocation)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.CHARINDEX
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startLocation")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "target")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "toSearch")]
        [DbFunction("SqlServerCe", "CHARINDEX")]
        public static Int32? CharIndex(Byte[] toSearch, Byte[] target, Int32? startLocation)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.CHARINDEX
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "target")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "toSearch")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startLocation")]
        [DbFunction("SqlServerCe", "CHARINDEX")]
        public static Int64? CharIndex(String toSearch, String target, Int64? startLocation)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.CHARINDEX
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "toSearch")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "target")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startLocation")]
        [DbFunction("SqlServerCe", "CHARINDEX")]
        public static Int64? CharIndex(Byte[] toSearch, Byte[] target, Int64? startLocation)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.NCHAR
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "NCHAR")]
        public static String NChar(Int32? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.PATINDEX
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stringPattern")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "target")]
        [DbFunction("SqlServerCe", "PATINDEX")]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
        public static Int32? PatIndex(String stringPattern, String target)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.REPLICATE
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "count")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "target")]
        [DbFunction("SqlServerCe", "REPLICATE")]
        public static String Replicate(String target, Int32? count)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.SPACE
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServerCe", "SPACE")]
        public static String Space(Int32? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.STR
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number")]
        [DbFunction("SqlServerCe", "STR")]
        public static String StringConvert(Double? number)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.STR
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number")]
        [DbFunction("SqlServerCe", "STR")]
        public static String StringConvert(Decimal? number)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.STR
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "length")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number")]
        [DbFunction("SqlServerCe", "STR")]
        public static String StringConvert(Double? number, Int32? length)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.STR
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "length")]
        [DbFunction("SqlServerCe", "STR")]
        public static String StringConvert(Decimal? number, Int32? length)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.STR
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "length")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "decimalArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number")]
        [DbFunction("SqlServerCe", "STR")]
        public static String StringConvert(Double? number, Int32? length, Int32? decimalArg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.STR
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "decimalArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "length")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number")]
        [DbFunction("SqlServerCe", "STR")]
        public static String StringConvert(Decimal? number, Int32? length, Int32? decimalArg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.STUFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "start")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "length")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stringInput")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stringReplacement")]
        [DbFunction("SqlServerCe", "STUFF")]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
        public static String Stuff(String stringInput, Int32? start, Int32? length, String stringReplacement)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.UNICODE
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "UNICODE")]
        public static Int32? Unicode(String arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.ACOS
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServerCe", "ACOS")]
        public static Double? Acos(Double? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.ACOS
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServerCe", "ACOS")]
        public static Double? Acos(Decimal? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.ASIN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "ASIN")]
        public static Double? Asin(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.ASIN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "ASIN")]
        public static Double? Asin(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.ATAN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "ATAN")]
        public static Double? Atan(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.ATAN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "ATAN")]
        public static Double? Atan(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.ATN2
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [DbFunction("SqlServerCe", "ATN2")]
        public static Double? Atan2(Double? arg1, Double? arg2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.ATN2
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServerCe", "ATN2")]
        public static Double? Atan2(Decimal? arg1, Decimal? arg2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.COS
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "COS")]
        public static Double? Cos(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.COS
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "COS")]
        public static Double? Cos(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.COT
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "COT")]
        public static Double? Cot(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.COT
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "COT")]
        public static Double? Cot(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.DEGREES
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServerCe", "DEGREES")]
        public static Int32? Degrees(Int32? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.DEGREES
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServerCe", "DEGREES")]
        public static Int64? Degrees(Int64? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.DEGREES
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServerCe", "DEGREES")]
        public static Decimal? Degrees(Decimal? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.DEGREES
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1")]
        [DbFunction("SqlServerCe", "DEGREES")]
        public static Double? Degrees(Double? arg1)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.EXP
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Exp")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "EXP")]
        public static Double? Exp(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.EXP
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Exp")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "EXP")]
        public static Double? Exp(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.LOG
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "LOG")]
        public static Double? Log(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.LOG
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "LOG")]
        public static Double? Log(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.LOG10
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "LOG10")]
        public static Double? Log10(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.LOG10
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "LOG10")]
        public static Double? Log10(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.PI
        /// </summary>
        [DbFunction("SqlServerCe", "PI")]
        public static Double? Pi()
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.RADIANS
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "RADIANS")]
        public static Int32? Radians(Int32? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.RADIANS
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "RADIANS")]
        public static Int64? Radians(Int64? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.RADIANS
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "RADIANS")]
        public static Decimal? Radians(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.RADIANS
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "RADIANS")]
        public static Double? Radians(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.RAND
        /// </summary>
        [DbFunction("SqlServerCe", "RAND")]
        public static Double? Rand()
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.RAND
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "seed")]
        [DbFunction("SqlServerCe", "RAND")]
        public static Double? Rand(Int32? seed)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.SIGN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "SIGN")]
        public static Int32? Sign(Int32? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.SIGN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "SIGN")]
        public static Int64? Sign(Int64? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.SIGN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "SIGN")]
        public static Decimal? Sign(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.SIGN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "SIGN")]
        public static Double? Sign(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.SIN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "SIN")]
        public static Double? Sin(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.SIN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "SIN")]
        public static Double? Sin(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.SQRT
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "SQRT")]
        public static Double? SquareRoot(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.SQRT
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "SQRT")]
        public static Double? SquareRoot(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.TAN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "TAN")]
        public static Double? Tan(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.TAN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "TAN")]
        public static Double? Tan(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.DATEADD
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "date")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [DbFunction("SqlServerCe", "DATEADD")]
        public static DateTime? DateAdd(String datePartArg, Double? number, DateTime? date)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.DATEADD
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "date")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number")]
        [DbFunction("SqlServerCe", "DATEADD")]
        public static DateTime? DateAdd(String datePartArg, Double? number, String date)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.DATEDIFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [DbFunction("SqlServerCe", "DATEDIFF")]
        public static Int32? DateDiff(String datePartArg, DateTime? startDate, DateTime? endDate)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.DATEDIFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate")]
        [DbFunction("SqlServerCe", "DATEDIFF")]
        public static Int32? DateDiff(String datePartArg, String startDate, DateTime? endDate)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.DATEDIFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [DbFunction("SqlServerCe", "DATEDIFF")]
        public static Int32? DateDiff(String datePartArg, DateTime? startDate, String endDate)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.DATEDIFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate")]
        [DbFunction("SqlServerCe", "DATEDIFF")]
        public static Int32? DateDiff(String datePartArg, String startDate, String endDate)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.DATENAME
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "date")]
        [DbFunction("SqlServerCe", "DATENAME")]
        public static String DateName(String datePartArg, DateTime? date)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.DATENAME
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "date")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [DbFunction("SqlServerCe", "DATENAME")]
        public static String DateName(String datePartArg, String date)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.DATEPART
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "date")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [DbFunction("SqlServerCe", "DATEPART")]
        public static Int32? DatePart(String datePartArg, DateTime? date)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.DATEPART
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "date")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg")]
        [DbFunction("SqlServerCe", "DATEPART")]
        public static Int32? DatePart(String datePartArg, String date)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.GETDATE
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [DbFunction("SqlServerCe", "GETDATE")]
        public static DateTime? GetDate()
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.DATALENGTH
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "DATALENGTH")]
        public static Int32? DataLength(Boolean? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.DATALENGTH
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "DATALENGTH")]
        public static Int32? DataLength(Double? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.DATALENGTH
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "DATALENGTH")]
        public static Int32? DataLength(Decimal? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.DATALENGTH
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "DATALENGTH")]
        public static Int32? DataLength(DateTime? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.DATALENGTH
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "DATALENGTH")]
        public static Int32? DataLength(String arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.DATALENGTH
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "DATALENGTH")]
        public static Int32? DataLength(Byte[] arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServerCe.DATALENGTH
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg")]
        [DbFunction("SqlServerCe", "DATALENGTH")]
        public static Int32? DataLength(Guid? arg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

    }
}
