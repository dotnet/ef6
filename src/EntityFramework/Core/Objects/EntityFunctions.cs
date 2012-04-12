namespace System.Data.Entity.Core.Objects
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Resources;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Contains function stubs that expose Edm methods in Linq to Entities.
    /// </summary>
    public static partial class EntityFunctions
    {
        /// <summary>
        /// Proxy for the function Edm.StDev
        /// </summary>
        [EdmFunction("Edm", "StDev")]
        public static Double? StandardDeviation(IEnumerable<Decimal> collection)
        {
            var objectQuerySource = collection as ObjectQuery<Decimal>;
            if (objectQuerySource != null)
            {
                return
                    ((IQueryable)objectQuerySource).Provider.Execute<Double?>(
                        Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.StDev
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [EdmFunction("Edm", "StDev")]
        public static Double? StandardDeviation(IEnumerable<Decimal?> collection)
        {
            var objectQuerySource = collection as ObjectQuery<Decimal?>;
            if (objectQuerySource != null)
            {
                return
                    ((IQueryable)objectQuerySource).Provider.Execute<Double?>(
                        Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.StDev
        /// </summary>
        [EdmFunction("Edm", "StDev")]
        public static Double? StandardDeviation(IEnumerable<Double> collection)
        {
            var objectQuerySource = collection as ObjectQuery<Double>;
            if (objectQuerySource != null)
            {
                return
                    ((IQueryable)objectQuerySource).Provider.Execute<Double?>(
                        Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.StDev
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [EdmFunction("Edm", "StDev")]
        public static Double? StandardDeviation(IEnumerable<Double?> collection)
        {
            var objectQuerySource = collection as ObjectQuery<Double?>;
            if (objectQuerySource != null)
            {
                return
                    ((IQueryable)objectQuerySource).Provider.Execute<Double?>(
                        Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.StDev
        /// </summary>
        [EdmFunction("Edm", "StDev")]
        public static Double? StandardDeviation(IEnumerable<Int32> collection)
        {
            var objectQuerySource = collection as ObjectQuery<Int32>;
            if (objectQuerySource != null)
            {
                return
                    ((IQueryable)objectQuerySource).Provider.Execute<Double?>(
                        Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.StDev
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [EdmFunction("Edm", "StDev")]
        public static Double? StandardDeviation(IEnumerable<Int32?> collection)
        {
            var objectQuerySource = collection as ObjectQuery<Int32?>;
            if (objectQuerySource != null)
            {
                return
                    ((IQueryable)objectQuerySource).Provider.Execute<Double?>(
                        Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.StDev
        /// </summary>
        [EdmFunction("Edm", "StDev")]
        public static Double? StandardDeviation(IEnumerable<Int64> collection)
        {
            var objectQuerySource = collection as ObjectQuery<Int64>;
            if (objectQuerySource != null)
            {
                return
                    ((IQueryable)objectQuerySource).Provider.Execute<Double?>(
                        Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.StDev
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [EdmFunction("Edm", "StDev")]
        public static Double? StandardDeviation(IEnumerable<Int64?> collection)
        {
            var objectQuerySource = collection as ObjectQuery<Int64?>;
            if (objectQuerySource != null)
            {
                return
                    ((IQueryable)objectQuerySource).Provider.Execute<Double?>(
                        Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.StDevP
        /// </summary>
        [EdmFunction("Edm", "StDevP")]
        public static Double? StandardDeviationP(IEnumerable<Decimal> collection)
        {
            var objectQuerySource = collection as ObjectQuery<Decimal>;
            if (objectQuerySource != null)
            {
                return
                    ((IQueryable)objectQuerySource).Provider.Execute<Double?>(
                        Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.StDevP
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [EdmFunction("Edm", "StDevP")]
        public static Double? StandardDeviationP(IEnumerable<Decimal?> collection)
        {
            var objectQuerySource = collection as ObjectQuery<Decimal?>;
            if (objectQuerySource != null)
            {
                return
                    ((IQueryable)objectQuerySource).Provider.Execute<Double?>(
                        Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.StDevP
        /// </summary>
        [EdmFunction("Edm", "StDevP")]
        public static Double? StandardDeviationP(IEnumerable<Double> collection)
        {
            var objectQuerySource = collection as ObjectQuery<Double>;
            if (objectQuerySource != null)
            {
                return
                    ((IQueryable)objectQuerySource).Provider.Execute<Double?>(
                        Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.StDevP
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [EdmFunction("Edm", "StDevP")]
        public static Double? StandardDeviationP(IEnumerable<Double?> collection)
        {
            var objectQuerySource = collection as ObjectQuery<Double?>;
            if (objectQuerySource != null)
            {
                return
                    ((IQueryable)objectQuerySource).Provider.Execute<Double?>(
                        Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.StDevP
        /// </summary>
        [EdmFunction("Edm", "StDevP")]
        public static Double? StandardDeviationP(IEnumerable<Int32> collection)
        {
            var objectQuerySource = collection as ObjectQuery<Int32>;
            if (objectQuerySource != null)
            {
                return
                    ((IQueryable)objectQuerySource).Provider.Execute<Double?>(
                        Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.StDevP
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [EdmFunction("Edm", "StDevP")]
        public static Double? StandardDeviationP(IEnumerable<Int32?> collection)
        {
            var objectQuerySource = collection as ObjectQuery<Int32?>;
            if (objectQuerySource != null)
            {
                return
                    ((IQueryable)objectQuerySource).Provider.Execute<Double?>(
                        Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.StDevP
        /// </summary>
        [EdmFunction("Edm", "StDevP")]
        public static Double? StandardDeviationP(IEnumerable<Int64> collection)
        {
            var objectQuerySource = collection as ObjectQuery<Int64>;
            if (objectQuerySource != null)
            {
                return
                    ((IQueryable)objectQuerySource).Provider.Execute<Double?>(
                        Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.StDevP
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [EdmFunction("Edm", "StDevP")]
        public static Double? StandardDeviationP(IEnumerable<Int64?> collection)
        {
            var objectQuerySource = collection as ObjectQuery<Int64?>;
            if (objectQuerySource != null)
            {
                return
                    ((IQueryable)objectQuerySource).Provider.Execute<Double?>(
                        Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.Var
        /// </summary>
        [EdmFunction("Edm", "Var")]
        public static Double? Var(IEnumerable<Decimal> collection)
        {
            var objectQuerySource = collection as ObjectQuery<Decimal>;
            if (objectQuerySource != null)
            {
                return
                    ((IQueryable)objectQuerySource).Provider.Execute<Double?>(
                        Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.Var
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [EdmFunction("Edm", "Var")]
        public static Double? Var(IEnumerable<Decimal?> collection)
        {
            var objectQuerySource = collection as ObjectQuery<Decimal?>;
            if (objectQuerySource != null)
            {
                return
                    ((IQueryable)objectQuerySource).Provider.Execute<Double?>(
                        Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.Var
        /// </summary>
        [EdmFunction("Edm", "Var")]
        public static Double? Var(IEnumerable<Double> collection)
        {
            var objectQuerySource = collection as ObjectQuery<Double>;
            if (objectQuerySource != null)
            {
                return
                    ((IQueryable)objectQuerySource).Provider.Execute<Double?>(
                        Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.Var
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [EdmFunction("Edm", "Var")]
        public static Double? Var(IEnumerable<Double?> collection)
        {
            var objectQuerySource = collection as ObjectQuery<Double?>;
            if (objectQuerySource != null)
            {
                return
                    ((IQueryable)objectQuerySource).Provider.Execute<Double?>(
                        Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.Var
        /// </summary>
        [EdmFunction("Edm", "Var")]
        public static Double? Var(IEnumerable<Int32> collection)
        {
            var objectQuerySource = collection as ObjectQuery<Int32>;
            if (objectQuerySource != null)
            {
                return
                    ((IQueryable)objectQuerySource).Provider.Execute<Double?>(
                        Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.Var
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [EdmFunction("Edm", "Var")]
        public static Double? Var(IEnumerable<Int32?> collection)
        {
            var objectQuerySource = collection as ObjectQuery<Int32?>;
            if (objectQuerySource != null)
            {
                return
                    ((IQueryable)objectQuerySource).Provider.Execute<Double?>(
                        Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.Var
        /// </summary>
        [EdmFunction("Edm", "Var")]
        public static Double? Var(IEnumerable<Int64> collection)
        {
            var objectQuerySource = collection as ObjectQuery<Int64>;
            if (objectQuerySource != null)
            {
                return
                    ((IQueryable)objectQuerySource).Provider.Execute<Double?>(
                        Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.Var
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [EdmFunction("Edm", "Var")]
        public static Double? Var(IEnumerable<Int64?> collection)
        {
            var objectQuerySource = collection as ObjectQuery<Int64?>;
            if (objectQuerySource != null)
            {
                return
                    ((IQueryable)objectQuerySource).Provider.Execute<Double?>(
                        Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.VarP
        /// </summary>
        [EdmFunction("Edm", "VarP")]
        public static Double? VarP(IEnumerable<Decimal> collection)
        {
            var objectQuerySource = collection as ObjectQuery<Decimal>;
            if (objectQuerySource != null)
            {
                return
                    ((IQueryable)objectQuerySource).Provider.Execute<Double?>(
                        Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.VarP
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [EdmFunction("Edm", "VarP")]
        public static Double? VarP(IEnumerable<Decimal?> collection)
        {
            var objectQuerySource = collection as ObjectQuery<Decimal?>;
            if (objectQuerySource != null)
            {
                return
                    ((IQueryable)objectQuerySource).Provider.Execute<Double?>(
                        Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.VarP
        /// </summary>
        [EdmFunction("Edm", "VarP")]
        public static Double? VarP(IEnumerable<Double> collection)
        {
            var objectQuerySource = collection as ObjectQuery<Double>;
            if (objectQuerySource != null)
            {
                return
                    ((IQueryable)objectQuerySource).Provider.Execute<Double?>(
                        Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.VarP
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [EdmFunction("Edm", "VarP")]
        public static Double? VarP(IEnumerable<Double?> collection)
        {
            var objectQuerySource = collection as ObjectQuery<Double?>;
            if (objectQuerySource != null)
            {
                return
                    ((IQueryable)objectQuerySource).Provider.Execute<Double?>(
                        Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.VarP
        /// </summary>
        [EdmFunction("Edm", "VarP")]
        public static Double? VarP(IEnumerable<Int32> collection)
        {
            var objectQuerySource = collection as ObjectQuery<Int32>;
            if (objectQuerySource != null)
            {
                return
                    ((IQueryable)objectQuerySource).Provider.Execute<Double?>(
                        Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.VarP
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [EdmFunction("Edm", "VarP")]
        public static Double? VarP(IEnumerable<Int32?> collection)
        {
            var objectQuerySource = collection as ObjectQuery<Int32?>;
            if (objectQuerySource != null)
            {
                return
                    ((IQueryable)objectQuerySource).Provider.Execute<Double?>(
                        Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.VarP
        /// </summary>
        [EdmFunction("Edm", "VarP")]
        public static Double? VarP(IEnumerable<Int64> collection)
        {
            var objectQuerySource = collection as ObjectQuery<Int64>;
            if (objectQuerySource != null)
            {
                return
                    ((IQueryable)objectQuerySource).Provider.Execute<Double?>(
                        Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.VarP
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [EdmFunction("Edm", "VarP")]
        public static Double? VarP(IEnumerable<Int64?> collection)
        {
            var objectQuerySource = collection as ObjectQuery<Int64?>;
            if (objectQuerySource != null)
            {
                return
                    ((IQueryable)objectQuerySource).Provider.Execute<Double?>(
                        Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.Left
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stringArgument")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "length")]
        [EdmFunction("Edm", "Left")]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
        public static String Left(String stringArgument, Int64? length)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.Right
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "length")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stringArgument")]
        [EdmFunction("Edm", "Right")]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
        public static String Right(String stringArgument, Int64? length)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.Reverse
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stringArgument")]
        [EdmFunction("Edm", "Reverse")]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
        public static String Reverse(String stringArgument)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.GetTotalOffsetMinutes
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateTimeOffsetArgument")]
        [EdmFunction("Edm", "GetTotalOffsetMinutes")]
        public static Int32? GetTotalOffsetMinutes(DateTimeOffset? dateTimeOffsetArgument)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.TruncateTime
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue")]
        [EdmFunction("Edm", "TruncateTime")]
        public static DateTimeOffset? TruncateTime(DateTimeOffset? dateValue)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.TruncateTime
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue")]
        [EdmFunction("Edm", "TruncateTime")]
        public static DateTime? TruncateTime(DateTime? dateValue)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.CreateDateTime
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "minute")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "second")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "day")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "hour")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "year")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "month")]
        [EdmFunction("Edm", "CreateDateTime")]
        public static DateTime? CreateDateTime(Int32? year, Int32? month, Int32? day, Int32? hour, Int32? minute, Double? second)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.CreateDateTimeOffset
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "month")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeZoneOffset")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "second")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "hour")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "minute")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "day")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "year")]
        [EdmFunction("Edm", "CreateDateTimeOffset")]
        public static DateTimeOffset? CreateDateTimeOffset(
            Int32? year, Int32? month, Int32? day, Int32? hour, Int32? minute, Double? second, Int32? timeZoneOffset)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.CreateTime
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "minute")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "hour")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "second")]
        [EdmFunction("Edm", "CreateTime")]
        public static TimeSpan? CreateTime(Int32? hour, Int32? minute, Double? second)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddYears
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue")]
        [EdmFunction("Edm", "AddYears")]
        public static DateTimeOffset? AddYears(DateTimeOffset? dateValue, Int32? addValue)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddYears
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [EdmFunction("Edm", "AddYears")]
        public static DateTime? AddYears(DateTime? dateValue, Int32? addValue)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddMonths
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [EdmFunction("Edm", "AddMonths")]
        public static DateTimeOffset? AddMonths(DateTimeOffset? dateValue, Int32? addValue)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddMonths
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue")]
        [EdmFunction("Edm", "AddMonths")]
        public static DateTime? AddMonths(DateTime? dateValue, Int32? addValue)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddDays
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue")]
        [EdmFunction("Edm", "AddDays")]
        public static DateTimeOffset? AddDays(DateTimeOffset? dateValue, Int32? addValue)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddDays
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue")]
        [EdmFunction("Edm", "AddDays")]
        public static DateTime? AddDays(DateTime? dateValue, Int32? addValue)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddHours
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue")]
        [EdmFunction("Edm", "AddHours")]
        public static DateTimeOffset? AddHours(DateTimeOffset? timeValue, Int32? addValue)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddHours
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue")]
        [EdmFunction("Edm", "AddHours")]
        public static DateTime? AddHours(DateTime? timeValue, Int32? addValue)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddHours
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue")]
        [EdmFunction("Edm", "AddHours")]
        public static TimeSpan? AddHours(TimeSpan? timeValue, Int32? addValue)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddMinutes
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue")]
        [EdmFunction("Edm", "AddMinutes")]
        public static DateTimeOffset? AddMinutes(DateTimeOffset? timeValue, Int32? addValue)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddMinutes
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [EdmFunction("Edm", "AddMinutes")]
        public static DateTime? AddMinutes(DateTime? timeValue, Int32? addValue)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddMinutes
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [EdmFunction("Edm", "AddMinutes")]
        public static TimeSpan? AddMinutes(TimeSpan? timeValue, Int32? addValue)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddSeconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue")]
        [EdmFunction("Edm", "AddSeconds")]
        public static DateTimeOffset? AddSeconds(DateTimeOffset? timeValue, Int32? addValue)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddSeconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [EdmFunction("Edm", "AddSeconds")]
        public static DateTime? AddSeconds(DateTime? timeValue, Int32? addValue)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddSeconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [EdmFunction("Edm", "AddSeconds")]
        public static TimeSpan? AddSeconds(TimeSpan? timeValue, Int32? addValue)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddMilliseconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [EdmFunction("Edm", "AddMilliseconds")]
        public static DateTimeOffset? AddMilliseconds(DateTimeOffset? timeValue, Int32? addValue)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddMilliseconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [EdmFunction("Edm", "AddMilliseconds")]
        public static DateTime? AddMilliseconds(DateTime? timeValue, Int32? addValue)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddMilliseconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue")]
        [EdmFunction("Edm", "AddMilliseconds")]
        public static TimeSpan? AddMilliseconds(TimeSpan? timeValue, Int32? addValue)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddMicroseconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [EdmFunction("Edm", "AddMicroseconds")]
        public static DateTimeOffset? AddMicroseconds(DateTimeOffset? timeValue, Int32? addValue)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddMicroseconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [EdmFunction("Edm", "AddMicroseconds")]
        public static DateTime? AddMicroseconds(DateTime? timeValue, Int32? addValue)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddMicroseconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [EdmFunction("Edm", "AddMicroseconds")]
        public static TimeSpan? AddMicroseconds(TimeSpan? timeValue, Int32? addValue)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddNanoseconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue")]
        [EdmFunction("Edm", "AddNanoseconds")]
        public static DateTimeOffset? AddNanoseconds(DateTimeOffset? timeValue, Int32? addValue)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddNanoseconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [EdmFunction("Edm", "AddNanoseconds")]
        public static DateTime? AddNanoseconds(DateTime? timeValue, Int32? addValue)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddNanoseconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue")]
        [EdmFunction("Edm", "AddNanoseconds")]
        public static TimeSpan? AddNanoseconds(TimeSpan? timeValue, Int32? addValue)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffYears
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue2")]
        [EdmFunction("Edm", "DiffYears")]
        public static Int32? DiffYears(DateTimeOffset? dateValue1, DateTimeOffset? dateValue2)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffYears
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue1")]
        [EdmFunction("Edm", "DiffYears")]
        public static Int32? DiffYears(DateTime? dateValue1, DateTime? dateValue2)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffMonths
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue1")]
        [EdmFunction("Edm", "DiffMonths")]
        public static Int32? DiffMonths(DateTimeOffset? dateValue1, DateTimeOffset? dateValue2)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffMonths
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue1")]
        [EdmFunction("Edm", "DiffMonths")]
        public static Int32? DiffMonths(DateTime? dateValue1, DateTime? dateValue2)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffDays
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue2")]
        [EdmFunction("Edm", "DiffDays")]
        public static Int32? DiffDays(DateTimeOffset? dateValue1, DateTimeOffset? dateValue2)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffDays
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue2")]
        [EdmFunction("Edm", "DiffDays")]
        public static Int32? DiffDays(DateTime? dateValue1, DateTime? dateValue2)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffHours
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1")]
        [EdmFunction("Edm", "DiffHours")]
        public static Int32? DiffHours(DateTimeOffset? timeValue1, DateTimeOffset? timeValue2)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffHours
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1")]
        [EdmFunction("Edm", "DiffHours")]
        public static Int32? DiffHours(DateTime? timeValue1, DateTime? timeValue2)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffHours
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1")]
        [EdmFunction("Edm", "DiffHours")]
        public static Int32? DiffHours(TimeSpan? timeValue1, TimeSpan? timeValue2)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffMinutes
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2")]
        [EdmFunction("Edm", "DiffMinutes")]
        public static Int32? DiffMinutes(DateTimeOffset? timeValue1, DateTimeOffset? timeValue2)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffMinutes
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1")]
        [EdmFunction("Edm", "DiffMinutes")]
        public static Int32? DiffMinutes(DateTime? timeValue1, DateTime? timeValue2)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffMinutes
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2")]
        [EdmFunction("Edm", "DiffMinutes")]
        public static Int32? DiffMinutes(TimeSpan? timeValue1, TimeSpan? timeValue2)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffSeconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2")]
        [EdmFunction("Edm", "DiffSeconds")]
        public static Int32? DiffSeconds(DateTimeOffset? timeValue1, DateTimeOffset? timeValue2)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffSeconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1")]
        [EdmFunction("Edm", "DiffSeconds")]
        public static Int32? DiffSeconds(DateTime? timeValue1, DateTime? timeValue2)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffSeconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2")]
        [EdmFunction("Edm", "DiffSeconds")]
        public static Int32? DiffSeconds(TimeSpan? timeValue1, TimeSpan? timeValue2)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffMilliseconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1")]
        [EdmFunction("Edm", "DiffMilliseconds")]
        public static Int32? DiffMilliseconds(DateTimeOffset? timeValue1, DateTimeOffset? timeValue2)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffMilliseconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2")]
        [EdmFunction("Edm", "DiffMilliseconds")]
        public static Int32? DiffMilliseconds(DateTime? timeValue1, DateTime? timeValue2)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffMilliseconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1")]
        [EdmFunction("Edm", "DiffMilliseconds")]
        public static Int32? DiffMilliseconds(TimeSpan? timeValue1, TimeSpan? timeValue2)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffMicroseconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1")]
        [EdmFunction("Edm", "DiffMicroseconds")]
        public static Int32? DiffMicroseconds(DateTimeOffset? timeValue1, DateTimeOffset? timeValue2)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffMicroseconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1")]
        [EdmFunction("Edm", "DiffMicroseconds")]
        public static Int32? DiffMicroseconds(DateTime? timeValue1, DateTime? timeValue2)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffMicroseconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1")]
        [EdmFunction("Edm", "DiffMicroseconds")]
        public static Int32? DiffMicroseconds(TimeSpan? timeValue1, TimeSpan? timeValue2)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffNanoseconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1")]
        [EdmFunction("Edm", "DiffNanoseconds")]
        public static Int32? DiffNanoseconds(DateTimeOffset? timeValue1, DateTimeOffset? timeValue2)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffNanoseconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1")]
        [EdmFunction("Edm", "DiffNanoseconds")]
        public static Int32? DiffNanoseconds(DateTime? timeValue1, DateTime? timeValue2)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffNanoseconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2")]
        [EdmFunction("Edm", "DiffNanoseconds")]
        public static Int32? DiffNanoseconds(TimeSpan? timeValue1, TimeSpan? timeValue2)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.Truncate
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "digits")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
        [EdmFunction("Edm", "Truncate")]
        public static Double? Truncate(Double? value, Int32? digits)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.Truncate
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "digits")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
        [EdmFunction("Edm", "Truncate")]
        public static Decimal? Truncate(Decimal? value, Int32? digits)
        {
            throw EntityUtil.NotSupported(Strings.ELinq_EdmFunctionDirectCall);
        }
    }
}
