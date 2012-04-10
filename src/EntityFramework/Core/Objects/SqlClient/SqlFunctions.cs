using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Entity.Core.Objects.SqlClient
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Contains function stubs that expose SqlServer methods in Linq to Entities.
    /// </summary>
    public static class SqlFunctions
    {
        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM_AGG
        /// </summary>
        [EdmFunction("SqlServer", "CHECKSUM_AGG")]
        public static System.Int32? ChecksumAggregate(IEnumerable<System.Int32> arg)
        {
            ObjectQuery<System.Int32> objectQuerySource = arg as ObjectQuery<System.Int32>;
            if (objectQuerySource != null)
            {
                return ((IQueryable)objectQuerySource).Provider.Execute<System.Int32?>(Expression.Call((MethodInfo)MethodInfo.GetCurrentMethod(),Expression.Constant(arg)));
            }
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM_AGG
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), EdmFunction("SqlServer", "CHECKSUM_AGG")]
        public static System.Int32? ChecksumAggregate(IEnumerable<System.Int32?> arg)
        {
            ObjectQuery<System.Int32?> objectQuerySource = arg as ObjectQuery<System.Int32?>;
            if (objectQuerySource != null)
            {
                return ((IQueryable)objectQuerySource).Provider.Execute<System.Int32?>(Expression.Call((MethodInfo)MethodInfo.GetCurrentMethod(),Expression.Constant(arg)));
            }
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.ASCII
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ascii"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "ASCII")]
        public static System.Int32? Ascii(System.String arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHAR
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "CHAR")]
        public static System.String Char(System.Int32? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHARINDEX
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "target"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "toSearch"), EdmFunction("SqlServer", "CHARINDEX")]
        public static System.Int32? CharIndex(System.String toSearch, System.String target)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHARINDEX
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "toSearch"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "target"), EdmFunction("SqlServer", "CHARINDEX")]
        public static System.Int32? CharIndex(System.Byte[] toSearch, System.Byte[] target)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHARINDEX
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "target"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "toSearch"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startLocation"), EdmFunction("SqlServer", "CHARINDEX")]
        public static System.Int32? CharIndex(System.String toSearch, System.String target, System.Int32? startLocation)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHARINDEX
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startLocation"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "target"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "toSearch"), EdmFunction("SqlServer", "CHARINDEX")]
        public static System.Int32? CharIndex(System.Byte[] toSearch, System.Byte[] target, System.Int32? startLocation)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHARINDEX
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "target"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "toSearch"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startLocation"), EdmFunction("SqlServer", "CHARINDEX")]
        public static System.Int64? CharIndex(System.String toSearch, System.String target, System.Int64? startLocation)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHARINDEX
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "toSearch"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "target"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startLocation"), EdmFunction("SqlServer", "CHARINDEX")]
        public static System.Int64? CharIndex(System.Byte[] toSearch, System.Byte[] target, System.Int64? startLocation)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DIFFERENCE
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "string2"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "string1"), EdmFunction("SqlServer", "DIFFERENCE")]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
        public static System.Int32? Difference(System.String string1, System.String string2)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.NCHAR
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "NCHAR")]
        public static System.String NChar(System.Int32? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.PATINDEX
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stringPattern"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "target"), EdmFunction("SqlServer", "PATINDEX")]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
        public static System.Int32? PatIndex(System.String stringPattern, System.String target)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.QUOTENAME
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stringArg"), EdmFunction("SqlServer", "QUOTENAME")]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
        public static System.String QuoteName(System.String stringArg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.QUOTENAME
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "quoteCharacter"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stringArg"), EdmFunction("SqlServer", "QUOTENAME")]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
        public static System.String QuoteName(System.String stringArg, System.String quoteCharacter)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.REPLICATE
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "count"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "target"), EdmFunction("SqlServer", "REPLICATE")]
        public static System.String Replicate(System.String target, System.Int32? count)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.SOUNDEX
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "SOUNDEX")]
        public static System.String SoundCode(System.String arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.SPACE
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), EdmFunction("SqlServer", "SPACE")]
        public static System.String Space(System.Int32? arg1)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.STR
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number"), EdmFunction("SqlServer", "STR")]
        public static System.String StringConvert(System.Double? number)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.STR
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number"), EdmFunction("SqlServer", "STR")]
        public static System.String StringConvert(System.Decimal? number)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.STR
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "length"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number"), EdmFunction("SqlServer", "STR")]
        public static System.String StringConvert(System.Double? number, System.Int32? length)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.STR
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "length"), EdmFunction("SqlServer", "STR")]
        public static System.String StringConvert(System.Decimal? number, System.Int32? length)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.STR
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "length"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "decimalArg"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number"), EdmFunction("SqlServer", "STR")]
        public static System.String StringConvert(System.Double? number, System.Int32? length, System.Int32? decimalArg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.STR
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "decimalArg"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "length"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number"), EdmFunction("SqlServer", "STR")]
        public static System.String StringConvert(System.Decimal? number, System.Int32? length, System.Int32? decimalArg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.STUFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "start"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "length"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stringInput"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stringReplacement"), EdmFunction("SqlServer", "STUFF")]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
        public static System.String Stuff(System.String stringInput, System.Int32? start, System.Int32? length, System.String stringReplacement)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.UNICODE
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "UNICODE")]
        public static System.Int32? Unicode(System.String arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.ACOS
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), EdmFunction("SqlServer", "ACOS")]
        public static System.Double? Acos(System.Double? arg1)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.ACOS
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), EdmFunction("SqlServer", "ACOS")]
        public static System.Double? Acos(System.Decimal? arg1)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.ASIN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "ASIN")]
        public static System.Double? Asin(System.Double? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.ASIN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "ASIN")]
        public static System.Double? Asin(System.Decimal? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.ATAN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "ATAN")]
        public static System.Double? Atan(System.Double? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.ATAN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "ATAN")]
        public static System.Double? Atan(System.Decimal? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.ATN2
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2"), EdmFunction("SqlServer", "ATN2")]
        public static System.Double? Atan2(System.Double? arg1, System.Double? arg2)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.ATN2
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), EdmFunction("SqlServer", "ATN2")]
        public static System.Double? Atan2(System.Decimal? arg1, System.Decimal? arg2)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.COS
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "COS")]
        public static System.Double? Cos(System.Double? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.COS
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "COS")]
        public static System.Double? Cos(System.Decimal? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.COT
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "COT")]
        public static System.Double? Cot(System.Double? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.COT
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "COT")]
        public static System.Double? Cot(System.Decimal? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DEGREES
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), EdmFunction("SqlServer", "DEGREES")]
        public static System.Int32? Degrees(System.Int32? arg1)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DEGREES
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), EdmFunction("SqlServer", "DEGREES")]
        public static System.Int64? Degrees(System.Int64? arg1)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DEGREES
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), EdmFunction("SqlServer", "DEGREES")]
        public static System.Decimal? Degrees(System.Decimal? arg1)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DEGREES
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), EdmFunction("SqlServer", "DEGREES")]
        public static System.Double? Degrees(System.Double? arg1)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.EXP
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Exp"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "EXP")]
        public static System.Double? Exp(System.Double? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.EXP
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Exp"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "EXP")]
        public static System.Double? Exp(System.Decimal? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.LOG
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "LOG")]
        public static System.Double? Log(System.Double? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.LOG
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "LOG")]
        public static System.Double? Log(System.Decimal? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.LOG10
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "LOG10")]
        public static System.Double? Log10(System.Double? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.LOG10
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "LOG10")]
        public static System.Double? Log10(System.Decimal? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.PI
        /// </summary>
        [EdmFunction("SqlServer", "PI")]
        public static System.Double? Pi()
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.RADIANS
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "RADIANS")]
        public static System.Int32? Radians(System.Int32? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.RADIANS
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "RADIANS")]
        public static System.Int64? Radians(System.Int64? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.RADIANS
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "RADIANS")]
        public static System.Decimal? Radians(System.Decimal? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.RADIANS
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "RADIANS")]
        public static System.Double? Radians(System.Double? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.RAND
        /// </summary>
        [EdmFunction("SqlServer", "RAND")]
        public static System.Double? Rand()
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.RAND
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "seed"), EdmFunction("SqlServer", "RAND")]
        public static System.Double? Rand(System.Int32? seed)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.SIGN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "SIGN")]
        public static System.Int32? Sign(System.Int32? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.SIGN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "SIGN")]
        public static System.Int64? Sign(System.Int64? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.SIGN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "SIGN")]
        public static System.Decimal? Sign(System.Decimal? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.SIGN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "SIGN")]
        public static System.Double? Sign(System.Double? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.SIN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "SIN")]
        public static System.Double? Sin(System.Decimal? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.SIN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "SIN")]
        public static System.Double? Sin(System.Double? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.SQRT
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "SQRT")]
        public static System.Double? SquareRoot(System.Double? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.SQRT
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "SQRT")]
        public static System.Double? SquareRoot(System.Decimal? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.SQUARE
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), EdmFunction("SqlServer", "SQUARE")]
        public static System.Double? Square(System.Double? arg1)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.SQUARE
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), EdmFunction("SqlServer", "SQUARE")]
        public static System.Double? Square(System.Decimal? arg1)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.TAN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "TAN")]
        public static System.Double? Tan(System.Double? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.TAN
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "TAN")]
        public static System.Double? Tan(System.Decimal? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEADD
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "date"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg"), EdmFunction("SqlServer", "DATEADD")]
        public static System.DateTime? DateAdd(System.String datePartArg, System.Double? number, System.DateTime? date)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEADD
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "time"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg"), EdmFunction("SqlServer", "DATEADD")]
        public static System.TimeSpan? DateAdd(System.String datePartArg, System.Double? number, System.TimeSpan? time)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEADD
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateTimeOffsetArg"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg"), EdmFunction("SqlServer", "DATEADD")]
        public static System.DateTimeOffset? DateAdd(System.String datePartArg, System.Double? number, System.DateTimeOffset? dateTimeOffsetArg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEADD
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "date"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "number"), EdmFunction("SqlServer", "DATEADD")]
        public static System.DateTime? DateAdd(System.String datePartArg, System.Double? number, System.String date)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEDIFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg"), EdmFunction("SqlServer", "DATEDIFF")]
        public static System.Int32? DateDiff(System.String datePartArg, System.DateTime? startDate, System.DateTime? endDate)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEDIFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate"), EdmFunction("SqlServer", "DATEDIFF")]
        public static System.Int32? DateDiff(System.String datePartArg, System.DateTimeOffset? startDate, System.DateTimeOffset? endDate)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEDIFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate"), EdmFunction("SqlServer", "DATEDIFF")]
        public static System.Int32? DateDiff(System.String datePartArg, System.TimeSpan? startDate, System.TimeSpan? endDate)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEDIFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate"), EdmFunction("SqlServer", "DATEDIFF")]
        public static System.Int32? DateDiff(System.String datePartArg, System.String startDate, System.DateTime? endDate)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEDIFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate"), EdmFunction("SqlServer", "DATEDIFF")]
        public static System.Int32? DateDiff(System.String datePartArg, System.String startDate, System.DateTimeOffset? endDate)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEDIFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg"), EdmFunction("SqlServer", "DATEDIFF")]
        public static System.Int32? DateDiff(System.String datePartArg, System.String startDate, System.TimeSpan? endDate)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEDIFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate"), EdmFunction("SqlServer", "DATEDIFF")]
        public static System.Int32? DateDiff(System.String datePartArg, System.TimeSpan? startDate, System.String endDate)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEDIFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg"), EdmFunction("SqlServer", "DATEDIFF")]
        public static System.Int32? DateDiff(System.String datePartArg, System.DateTime? startDate, System.String endDate)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEDIFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg"), EdmFunction("SqlServer", "DATEDIFF")]
        public static System.Int32? DateDiff(System.String datePartArg, System.DateTimeOffset? startDate, System.String endDate)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEDIFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate"), EdmFunction("SqlServer", "DATEDIFF")]
        public static System.Int32? DateDiff(System.String datePartArg, System.String startDate, System.String endDate)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEDIFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg"), EdmFunction("SqlServer", "DATEDIFF")]
        public static System.Int32? DateDiff(System.String datePartArg, System.TimeSpan? startDate, System.DateTime? endDate)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEDIFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate"), EdmFunction("SqlServer", "DATEDIFF")]
        public static System.Int32? DateDiff(System.String datePartArg, System.TimeSpan? startDate, System.DateTimeOffset? endDate)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEDIFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg"), EdmFunction("SqlServer", "DATEDIFF")]
        public static System.Int32? DateDiff(System.String datePartArg, System.DateTime? startDate, System.TimeSpan? endDate)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEDIFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate"), EdmFunction("SqlServer", "DATEDIFF")]
        public static System.Int32? DateDiff(System.String datePartArg, System.DateTimeOffset? startDate, System.TimeSpan? endDate)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEDIFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg"), EdmFunction("SqlServer", "DATEDIFF")]
        public static System.Int32? DateDiff(System.String datePartArg, System.DateTime? startDate, System.DateTimeOffset? endDate)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEDIFF
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "endDate"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startDate"), EdmFunction("SqlServer", "DATEDIFF")]
        public static System.Int32? DateDiff(System.String datePartArg, System.DateTimeOffset? startDate, System.DateTime? endDate)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATENAME
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "date"), EdmFunction("SqlServer", "DATENAME")]
        public static System.String DateName(System.String datePartArg, System.DateTime? date)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATENAME
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "date"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg"), EdmFunction("SqlServer", "DATENAME")]
        public static System.String DateName(System.String datePartArg, System.String date)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATENAME
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "date"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg"), EdmFunction("SqlServer", "DATENAME")]
        public static System.String DateName(System.String datePartArg, System.TimeSpan? date)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATENAME
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "date"), EdmFunction("SqlServer", "DATENAME")]
        public static System.String DateName(System.String datePartArg, System.DateTimeOffset? date)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEPART
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "date"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg"), EdmFunction("SqlServer", "DATEPART")]
        public static System.Int32? DatePart(System.String datePartArg, System.DateTime? date)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEPART
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "date"), EdmFunction("SqlServer", "DATEPART")]
        public static System.Int32? DatePart(System.String datePartArg, System.DateTimeOffset? date)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEPART
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "date"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg"), EdmFunction("SqlServer", "DATEPART")]
        public static System.Int32? DatePart(System.String datePartArg, System.String date)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATEPART
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "datePartArg"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "date"), EdmFunction("SqlServer", "DATEPART")]
        public static System.Int32? DatePart(System.String datePartArg, System.TimeSpan? date)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.GETDATE
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate"), EdmFunction("SqlServer", "GETDATE")]
        public static System.DateTime? GetDate()
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.GETUTCDATE
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate"), EdmFunction("SqlServer", "GETUTCDATE")]
        public static System.DateTime? GetUtcDate()
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATALENGTH
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "DATALENGTH")]
        public static System.Int32? DataLength(System.Boolean? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATALENGTH
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "DATALENGTH")]
        public static System.Int32? DataLength(System.Double? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATALENGTH
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "DATALENGTH")]
        public static System.Int32? DataLength(System.Decimal? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATALENGTH
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "DATALENGTH")]
        public static System.Int32? DataLength(System.DateTime? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATALENGTH
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "DATALENGTH")]
        public static System.Int32? DataLength(System.TimeSpan? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATALENGTH
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "DATALENGTH")]
        public static System.Int32? DataLength(System.DateTimeOffset? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATALENGTH
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "DATALENGTH")]
        public static System.Int32? DataLength(System.String arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATALENGTH
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "DATALENGTH")]
        public static System.Int32? DataLength(System.Byte[] arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.DATALENGTH
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "DATALENGTH")]
        public static System.Int32? DataLength(System.Guid? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), EdmFunction("SqlServer", "CHECKSUM")]
        public static System.Int32? Checksum(System.Boolean? arg1)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), EdmFunction("SqlServer", "CHECKSUM")]
        public static System.Int32? Checksum(System.Double? arg1)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), EdmFunction("SqlServer", "CHECKSUM")]
        public static System.Int32? Checksum(System.Decimal? arg1)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), EdmFunction("SqlServer", "CHECKSUM")]
        public static System.Int32? Checksum(System.String arg1)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), EdmFunction("SqlServer", "CHECKSUM")]
        public static System.Int32? Checksum(System.DateTime? arg1)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), EdmFunction("SqlServer", "CHECKSUM")]
        public static System.Int32? Checksum(System.TimeSpan? arg1)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), EdmFunction("SqlServer", "CHECKSUM")]
        public static System.Int32? Checksum(System.DateTimeOffset? arg1)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), EdmFunction("SqlServer", "CHECKSUM")]
        public static System.Int32? Checksum(System.Byte[] arg1)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), EdmFunction("SqlServer", "CHECKSUM")]
        public static System.Int32? Checksum(System.Guid? arg1)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), EdmFunction("SqlServer", "CHECKSUM")]
        public static System.Int32? Checksum(System.Boolean? arg1, System.Boolean? arg2)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2"), EdmFunction("SqlServer", "CHECKSUM")]
        public static System.Int32? Checksum(System.Double? arg1, System.Double? arg2)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2"), EdmFunction("SqlServer", "CHECKSUM")]
        public static System.Int32? Checksum(System.Decimal? arg1, System.Decimal? arg2)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2"), EdmFunction("SqlServer", "CHECKSUM")]
        public static System.Int32? Checksum(System.String arg1, System.String arg2)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), EdmFunction("SqlServer", "CHECKSUM")]
        public static System.Int32? Checksum(System.DateTime? arg1, System.DateTime? arg2)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), EdmFunction("SqlServer", "CHECKSUM")]
        public static System.Int32? Checksum(System.TimeSpan? arg1, System.TimeSpan? arg2)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), EdmFunction("SqlServer", "CHECKSUM")]
        public static System.Int32? Checksum(System.DateTimeOffset? arg1, System.DateTimeOffset? arg2)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), EdmFunction("SqlServer", "CHECKSUM")]
        public static System.Int32? Checksum(System.Byte[] arg1, System.Byte[] arg2)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2"), EdmFunction("SqlServer", "CHECKSUM")]
        public static System.Int32? Checksum(System.Guid? arg1, System.Guid? arg2)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg3"), EdmFunction("SqlServer", "CHECKSUM")]
        public static System.Int32? Checksum(System.Boolean? arg1, System.Boolean? arg2, System.Boolean? arg3)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg3"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2"), EdmFunction("SqlServer", "CHECKSUM")]
        public static System.Int32? Checksum(System.Double? arg1, System.Double? arg2, System.Double? arg3)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg3"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), EdmFunction("SqlServer", "CHECKSUM")]
        public static System.Int32? Checksum(System.Decimal? arg1, System.Decimal? arg2, System.Decimal? arg3)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg3"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), EdmFunction("SqlServer", "CHECKSUM")]
        public static System.Int32? Checksum(System.String arg1, System.String arg2, System.String arg3)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg3"), EdmFunction("SqlServer", "CHECKSUM")]
        public static System.Int32? Checksum(System.DateTime? arg1, System.DateTime? arg2, System.DateTime? arg3)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg3"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2"), EdmFunction("SqlServer", "CHECKSUM")]
        public static System.Int32? Checksum(System.DateTimeOffset? arg1, System.DateTimeOffset? arg2, System.DateTimeOffset? arg3)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg3"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), EdmFunction("SqlServer", "CHECKSUM")]
        public static System.Int32? Checksum(System.TimeSpan? arg1, System.TimeSpan? arg2, System.TimeSpan? arg3)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg3"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2"), EdmFunction("SqlServer", "CHECKSUM")]
        public static System.Int32? Checksum(System.Byte[] arg1, System.Byte[] arg2, System.Byte[] arg3)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CHECKSUM
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg2"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg3"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg1"), EdmFunction("SqlServer", "CHECKSUM")]
        public static System.Int32? Checksum(System.Guid? arg1, System.Guid? arg2, System.Guid? arg3)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CURRENT_TIMESTAMP
        /// </summary>
        [EdmFunction("SqlServer", "CURRENT_TIMESTAMP")]
        public static System.DateTime? CurrentTimestamp()
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.CURRENT_USER
        /// </summary>
        [EdmFunction("SqlServer", "CURRENT_USER")]
        public static System.String CurrentUser()
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.HOST_NAME
        /// </summary>
        [EdmFunction("SqlServer", "HOST_NAME")]
        public static System.String HostName()
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.USER_NAME
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "USER_NAME")]
        public static System.String UserName(System.Int32? arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.USER_NAME
        /// </summary>
        [EdmFunction("SqlServer", "USER_NAME")]
        public static System.String UserName()
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.ISNUMERIC
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "ISNUMERIC")]
        public static System.Int32? IsNumeric(System.String arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function SqlServer.ISDATE
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arg"), EdmFunction("SqlServer", "ISDATE")]
        public static System.Int32? IsDate(System.String arg)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

    }
}
