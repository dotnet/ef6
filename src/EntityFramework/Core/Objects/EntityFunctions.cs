using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Entity.Core.Objects
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Contains function stubs that expose Edm methods in Linq to Entities.
    /// </summary>
    public static partial class EntityFunctions
    {
        /// <summary>
        /// Proxy for the function Edm.StDev
        /// </summary>
        [EdmFunction("Edm", "StDev")]
        public static System.Double? StandardDeviation(IEnumerable<System.Decimal> collection)
        {
            ObjectQuery<System.Decimal> objectQuerySource = collection as ObjectQuery<System.Decimal>;
            if (objectQuerySource != null)
            {
                return ((IQueryable)objectQuerySource).Provider.Execute<System.Double?>(Expression.Call((MethodInfo)MethodInfo.GetCurrentMethod(),Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.StDev
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), EdmFunction("Edm", "StDev")]
        public static System.Double? StandardDeviation(IEnumerable<System.Decimal?> collection)
        {
            ObjectQuery<System.Decimal?> objectQuerySource = collection as ObjectQuery<System.Decimal?>;
            if (objectQuerySource != null)
            {
                return ((IQueryable)objectQuerySource).Provider.Execute<System.Double?>(Expression.Call((MethodInfo)MethodInfo.GetCurrentMethod(),Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.StDev
        /// </summary>
        [EdmFunction("Edm", "StDev")]
        public static System.Double? StandardDeviation(IEnumerable<System.Double> collection)
        {
            ObjectQuery<System.Double> objectQuerySource = collection as ObjectQuery<System.Double>;
            if (objectQuerySource != null)
            {
                return ((IQueryable)objectQuerySource).Provider.Execute<System.Double?>(Expression.Call((MethodInfo)MethodInfo.GetCurrentMethod(),Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.StDev
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), EdmFunction("Edm", "StDev")]
        public static System.Double? StandardDeviation(IEnumerable<System.Double?> collection)
        {
            ObjectQuery<System.Double?> objectQuerySource = collection as ObjectQuery<System.Double?>;
            if (objectQuerySource != null)
            {
                return ((IQueryable)objectQuerySource).Provider.Execute<System.Double?>(Expression.Call((MethodInfo)MethodInfo.GetCurrentMethod(),Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.StDev
        /// </summary>
        [EdmFunction("Edm", "StDev")]
        public static System.Double? StandardDeviation(IEnumerable<System.Int32> collection)
        {
            ObjectQuery<System.Int32> objectQuerySource = collection as ObjectQuery<System.Int32>;
            if (objectQuerySource != null)
            {
                return ((IQueryable)objectQuerySource).Provider.Execute<System.Double?>(Expression.Call((MethodInfo)MethodInfo.GetCurrentMethod(),Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.StDev
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), EdmFunction("Edm", "StDev")]
        public static System.Double? StandardDeviation(IEnumerable<System.Int32?> collection)
        {
            ObjectQuery<System.Int32?> objectQuerySource = collection as ObjectQuery<System.Int32?>;
            if (objectQuerySource != null)
            {
                return ((IQueryable)objectQuerySource).Provider.Execute<System.Double?>(Expression.Call((MethodInfo)MethodInfo.GetCurrentMethod(),Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.StDev
        /// </summary>
        [EdmFunction("Edm", "StDev")]
        public static System.Double? StandardDeviation(IEnumerable<System.Int64> collection)
        {
            ObjectQuery<System.Int64> objectQuerySource = collection as ObjectQuery<System.Int64>;
            if (objectQuerySource != null)
            {
                return ((IQueryable)objectQuerySource).Provider.Execute<System.Double?>(Expression.Call((MethodInfo)MethodInfo.GetCurrentMethod(),Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.StDev
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), EdmFunction("Edm", "StDev")]
        public static System.Double? StandardDeviation(IEnumerable<System.Int64?> collection)
        {
            ObjectQuery<System.Int64?> objectQuerySource = collection as ObjectQuery<System.Int64?>;
            if (objectQuerySource != null)
            {
                return ((IQueryable)objectQuerySource).Provider.Execute<System.Double?>(Expression.Call((MethodInfo)MethodInfo.GetCurrentMethod(),Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.StDevP
        /// </summary>
        [EdmFunction("Edm", "StDevP")]
        public static System.Double? StandardDeviationP(IEnumerable<System.Decimal> collection)
        {
            ObjectQuery<System.Decimal> objectQuerySource = collection as ObjectQuery<System.Decimal>;
            if (objectQuerySource != null)
            {
                return ((IQueryable)objectQuerySource).Provider.Execute<System.Double?>(Expression.Call((MethodInfo)MethodInfo.GetCurrentMethod(),Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.StDevP
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), EdmFunction("Edm", "StDevP")]
        public static System.Double? StandardDeviationP(IEnumerable<System.Decimal?> collection)
        {
            ObjectQuery<System.Decimal?> objectQuerySource = collection as ObjectQuery<System.Decimal?>;
            if (objectQuerySource != null)
            {
                return ((IQueryable)objectQuerySource).Provider.Execute<System.Double?>(Expression.Call((MethodInfo)MethodInfo.GetCurrentMethod(),Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.StDevP
        /// </summary>
        [EdmFunction("Edm", "StDevP")]
        public static System.Double? StandardDeviationP(IEnumerable<System.Double> collection)
        {
            ObjectQuery<System.Double> objectQuerySource = collection as ObjectQuery<System.Double>;
            if (objectQuerySource != null)
            {
                return ((IQueryable)objectQuerySource).Provider.Execute<System.Double?>(Expression.Call((MethodInfo)MethodInfo.GetCurrentMethod(),Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.StDevP
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), EdmFunction("Edm", "StDevP")]
        public static System.Double? StandardDeviationP(IEnumerable<System.Double?> collection)
        {
            ObjectQuery<System.Double?> objectQuerySource = collection as ObjectQuery<System.Double?>;
            if (objectQuerySource != null)
            {
                return ((IQueryable)objectQuerySource).Provider.Execute<System.Double?>(Expression.Call((MethodInfo)MethodInfo.GetCurrentMethod(),Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.StDevP
        /// </summary>
        [EdmFunction("Edm", "StDevP")]
        public static System.Double? StandardDeviationP(IEnumerable<System.Int32> collection)
        {
            ObjectQuery<System.Int32> objectQuerySource = collection as ObjectQuery<System.Int32>;
            if (objectQuerySource != null)
            {
                return ((IQueryable)objectQuerySource).Provider.Execute<System.Double?>(Expression.Call((MethodInfo)MethodInfo.GetCurrentMethod(),Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.StDevP
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), EdmFunction("Edm", "StDevP")]
        public static System.Double? StandardDeviationP(IEnumerable<System.Int32?> collection)
        {
            ObjectQuery<System.Int32?> objectQuerySource = collection as ObjectQuery<System.Int32?>;
            if (objectQuerySource != null)
            {
                return ((IQueryable)objectQuerySource).Provider.Execute<System.Double?>(Expression.Call((MethodInfo)MethodInfo.GetCurrentMethod(),Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.StDevP
        /// </summary>
        [EdmFunction("Edm", "StDevP")]
        public static System.Double? StandardDeviationP(IEnumerable<System.Int64> collection)
        {
            ObjectQuery<System.Int64> objectQuerySource = collection as ObjectQuery<System.Int64>;
            if (objectQuerySource != null)
            {
                return ((IQueryable)objectQuerySource).Provider.Execute<System.Double?>(Expression.Call((MethodInfo)MethodInfo.GetCurrentMethod(),Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.StDevP
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), EdmFunction("Edm", "StDevP")]
        public static System.Double? StandardDeviationP(IEnumerable<System.Int64?> collection)
        {
            ObjectQuery<System.Int64?> objectQuerySource = collection as ObjectQuery<System.Int64?>;
            if (objectQuerySource != null)
            {
                return ((IQueryable)objectQuerySource).Provider.Execute<System.Double?>(Expression.Call((MethodInfo)MethodInfo.GetCurrentMethod(),Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.Var
        /// </summary>
        [EdmFunction("Edm", "Var")]
        public static System.Double? Var(IEnumerable<System.Decimal> collection)
        {
            ObjectQuery<System.Decimal> objectQuerySource = collection as ObjectQuery<System.Decimal>;
            if (objectQuerySource != null)
            {
                return ((IQueryable)objectQuerySource).Provider.Execute<System.Double?>(Expression.Call((MethodInfo)MethodInfo.GetCurrentMethod(),Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.Var
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), EdmFunction("Edm", "Var")]
        public static System.Double? Var(IEnumerable<System.Decimal?> collection)
        {
            ObjectQuery<System.Decimal?> objectQuerySource = collection as ObjectQuery<System.Decimal?>;
            if (objectQuerySource != null)
            {
                return ((IQueryable)objectQuerySource).Provider.Execute<System.Double?>(Expression.Call((MethodInfo)MethodInfo.GetCurrentMethod(),Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.Var
        /// </summary>
        [EdmFunction("Edm", "Var")]
        public static System.Double? Var(IEnumerable<System.Double> collection)
        {
            ObjectQuery<System.Double> objectQuerySource = collection as ObjectQuery<System.Double>;
            if (objectQuerySource != null)
            {
                return ((IQueryable)objectQuerySource).Provider.Execute<System.Double?>(Expression.Call((MethodInfo)MethodInfo.GetCurrentMethod(),Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.Var
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), EdmFunction("Edm", "Var")]
        public static System.Double? Var(IEnumerable<System.Double?> collection)
        {
            ObjectQuery<System.Double?> objectQuerySource = collection as ObjectQuery<System.Double?>;
            if (objectQuerySource != null)
            {
                return ((IQueryable)objectQuerySource).Provider.Execute<System.Double?>(Expression.Call((MethodInfo)MethodInfo.GetCurrentMethod(),Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.Var
        /// </summary>
        [EdmFunction("Edm", "Var")]
        public static System.Double? Var(IEnumerable<System.Int32> collection)
        {
            ObjectQuery<System.Int32> objectQuerySource = collection as ObjectQuery<System.Int32>;
            if (objectQuerySource != null)
            {
                return ((IQueryable)objectQuerySource).Provider.Execute<System.Double?>(Expression.Call((MethodInfo)MethodInfo.GetCurrentMethod(),Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.Var
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), EdmFunction("Edm", "Var")]
        public static System.Double? Var(IEnumerable<System.Int32?> collection)
        {
            ObjectQuery<System.Int32?> objectQuerySource = collection as ObjectQuery<System.Int32?>;
            if (objectQuerySource != null)
            {
                return ((IQueryable)objectQuerySource).Provider.Execute<System.Double?>(Expression.Call((MethodInfo)MethodInfo.GetCurrentMethod(),Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.Var
        /// </summary>
        [EdmFunction("Edm", "Var")]
        public static System.Double? Var(IEnumerable<System.Int64> collection)
        {
            ObjectQuery<System.Int64> objectQuerySource = collection as ObjectQuery<System.Int64>;
            if (objectQuerySource != null)
            {
                return ((IQueryable)objectQuerySource).Provider.Execute<System.Double?>(Expression.Call((MethodInfo)MethodInfo.GetCurrentMethod(),Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.Var
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), EdmFunction("Edm", "Var")]
        public static System.Double? Var(IEnumerable<System.Int64?> collection)
        {
            ObjectQuery<System.Int64?> objectQuerySource = collection as ObjectQuery<System.Int64?>;
            if (objectQuerySource != null)
            {
                return ((IQueryable)objectQuerySource).Provider.Execute<System.Double?>(Expression.Call((MethodInfo)MethodInfo.GetCurrentMethod(),Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.VarP
        /// </summary>
        [EdmFunction("Edm", "VarP")]
        public static System.Double? VarP(IEnumerable<System.Decimal> collection)
        {
            ObjectQuery<System.Decimal> objectQuerySource = collection as ObjectQuery<System.Decimal>;
            if (objectQuerySource != null)
            {
                return ((IQueryable)objectQuerySource).Provider.Execute<System.Double?>(Expression.Call((MethodInfo)MethodInfo.GetCurrentMethod(),Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.VarP
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), EdmFunction("Edm", "VarP")]
        public static System.Double? VarP(IEnumerable<System.Decimal?> collection)
        {
            ObjectQuery<System.Decimal?> objectQuerySource = collection as ObjectQuery<System.Decimal?>;
            if (objectQuerySource != null)
            {
                return ((IQueryable)objectQuerySource).Provider.Execute<System.Double?>(Expression.Call((MethodInfo)MethodInfo.GetCurrentMethod(),Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.VarP
        /// </summary>
        [EdmFunction("Edm", "VarP")]
        public static System.Double? VarP(IEnumerable<System.Double> collection)
        {
            ObjectQuery<System.Double> objectQuerySource = collection as ObjectQuery<System.Double>;
            if (objectQuerySource != null)
            {
                return ((IQueryable)objectQuerySource).Provider.Execute<System.Double?>(Expression.Call((MethodInfo)MethodInfo.GetCurrentMethod(),Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.VarP
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), EdmFunction("Edm", "VarP")]
        public static System.Double? VarP(IEnumerable<System.Double?> collection)
        {
            ObjectQuery<System.Double?> objectQuerySource = collection as ObjectQuery<System.Double?>;
            if (objectQuerySource != null)
            {
                return ((IQueryable)objectQuerySource).Provider.Execute<System.Double?>(Expression.Call((MethodInfo)MethodInfo.GetCurrentMethod(),Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.VarP
        /// </summary>
        [EdmFunction("Edm", "VarP")]
        public static System.Double? VarP(IEnumerable<System.Int32> collection)
        {
            ObjectQuery<System.Int32> objectQuerySource = collection as ObjectQuery<System.Int32>;
            if (objectQuerySource != null)
            {
                return ((IQueryable)objectQuerySource).Provider.Execute<System.Double?>(Expression.Call((MethodInfo)MethodInfo.GetCurrentMethod(),Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.VarP
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), EdmFunction("Edm", "VarP")]
        public static System.Double? VarP(IEnumerable<System.Int32?> collection)
        {
            ObjectQuery<System.Int32?> objectQuerySource = collection as ObjectQuery<System.Int32?>;
            if (objectQuerySource != null)
            {
                return ((IQueryable)objectQuerySource).Provider.Execute<System.Double?>(Expression.Call((MethodInfo)MethodInfo.GetCurrentMethod(),Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.VarP
        /// </summary>
        [EdmFunction("Edm", "VarP")]
        public static System.Double? VarP(IEnumerable<System.Int64> collection)
        {
            ObjectQuery<System.Int64> objectQuerySource = collection as ObjectQuery<System.Int64>;
            if (objectQuerySource != null)
            {
                return ((IQueryable)objectQuerySource).Provider.Execute<System.Double?>(Expression.Call((MethodInfo)MethodInfo.GetCurrentMethod(),Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.VarP
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), EdmFunction("Edm", "VarP")]
        public static System.Double? VarP(IEnumerable<System.Int64?> collection)
        {
            ObjectQuery<System.Int64?> objectQuerySource = collection as ObjectQuery<System.Int64?>;
            if (objectQuerySource != null)
            {
                return ((IQueryable)objectQuerySource).Provider.Execute<System.Double?>(Expression.Call((MethodInfo)MethodInfo.GetCurrentMethod(),Expression.Constant(collection)));
            }
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.Left
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stringArgument"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "length"), EdmFunction("Edm", "Left")]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
        public static System.String Left(System.String stringArgument, System.Int64? length)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.Right
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "length"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stringArgument"), EdmFunction("Edm", "Right")]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
        public static System.String Right(System.String stringArgument, System.Int64? length)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.Reverse
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stringArgument"), EdmFunction("Edm", "Reverse")]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
        public static System.String Reverse(System.String stringArgument)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.GetTotalOffsetMinutes
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateTimeOffsetArgument"), EdmFunction("Edm", "GetTotalOffsetMinutes")]
        public static System.Int32? GetTotalOffsetMinutes(System.DateTimeOffset? dateTimeOffsetArgument)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.TruncateTime
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue"), EdmFunction("Edm", "TruncateTime")]
        public static System.DateTimeOffset? TruncateTime(System.DateTimeOffset? dateValue)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.TruncateTime
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue"), EdmFunction("Edm", "TruncateTime")]
        public static System.DateTime? TruncateTime(System.DateTime? dateValue)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.CreateDateTime
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "minute"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "second"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "day"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "hour"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "year"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "month"), EdmFunction("Edm", "CreateDateTime")]
        public static System.DateTime? CreateDateTime(System.Int32? year, System.Int32? month, System.Int32? day, System.Int32? hour, System.Int32? minute, System.Double? second)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.CreateDateTimeOffset
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "month"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeZoneOffset"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "second"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "hour"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "minute"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "day"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "year"), EdmFunction("Edm", "CreateDateTimeOffset")]
        public static System.DateTimeOffset? CreateDateTimeOffset(System.Int32? year, System.Int32? month, System.Int32? day, System.Int32? hour, System.Int32? minute, System.Double? second, System.Int32? timeZoneOffset)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.CreateTime
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "minute"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "hour"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "second"), EdmFunction("Edm", "CreateTime")]
        public static System.TimeSpan? CreateTime(System.Int32? hour, System.Int32? minute, System.Double? second)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddYears
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue"), EdmFunction("Edm", "AddYears")]
        public static System.DateTimeOffset? AddYears(System.DateTimeOffset? dateValue, System.Int32? addValue)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddYears
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue"), EdmFunction("Edm", "AddYears")]
        public static System.DateTime? AddYears(System.DateTime? dateValue, System.Int32? addValue)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddMonths
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue"), EdmFunction("Edm", "AddMonths")]
        public static System.DateTimeOffset? AddMonths(System.DateTimeOffset? dateValue, System.Int32? addValue)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddMonths
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue"), EdmFunction("Edm", "AddMonths")]
        public static System.DateTime? AddMonths(System.DateTime? dateValue, System.Int32? addValue)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddDays
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue"), EdmFunction("Edm", "AddDays")]
        public static System.DateTimeOffset? AddDays(System.DateTimeOffset? dateValue, System.Int32? addValue)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddDays
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue"), EdmFunction("Edm", "AddDays")]
        public static System.DateTime? AddDays(System.DateTime? dateValue, System.Int32? addValue)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddHours
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue"), EdmFunction("Edm", "AddHours")]
        public static System.DateTimeOffset? AddHours(System.DateTimeOffset? timeValue, System.Int32? addValue)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddHours
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue"), EdmFunction("Edm", "AddHours")]
        public static System.DateTime? AddHours(System.DateTime? timeValue, System.Int32? addValue)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddHours
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue"), EdmFunction("Edm", "AddHours")]
        public static System.TimeSpan? AddHours(System.TimeSpan? timeValue, System.Int32? addValue)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddMinutes
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue"), EdmFunction("Edm", "AddMinutes")]
        public static System.DateTimeOffset? AddMinutes(System.DateTimeOffset? timeValue, System.Int32? addValue)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddMinutes
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue"), EdmFunction("Edm", "AddMinutes")]
        public static System.DateTime? AddMinutes(System.DateTime? timeValue, System.Int32? addValue)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddMinutes
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue"), EdmFunction("Edm", "AddMinutes")]
        public static System.TimeSpan? AddMinutes(System.TimeSpan? timeValue, System.Int32? addValue)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddSeconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue"), EdmFunction("Edm", "AddSeconds")]
        public static System.DateTimeOffset? AddSeconds(System.DateTimeOffset? timeValue, System.Int32? addValue)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddSeconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue"), EdmFunction("Edm", "AddSeconds")]
        public static System.DateTime? AddSeconds(System.DateTime? timeValue, System.Int32? addValue)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddSeconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue"), EdmFunction("Edm", "AddSeconds")]
        public static System.TimeSpan? AddSeconds(System.TimeSpan? timeValue, System.Int32? addValue)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddMilliseconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue"), EdmFunction("Edm", "AddMilliseconds")]
        public static System.DateTimeOffset? AddMilliseconds(System.DateTimeOffset? timeValue, System.Int32? addValue)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddMilliseconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue"), EdmFunction("Edm", "AddMilliseconds")]
        public static System.DateTime? AddMilliseconds(System.DateTime? timeValue, System.Int32? addValue)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddMilliseconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue"), EdmFunction("Edm", "AddMilliseconds")]
        public static System.TimeSpan? AddMilliseconds(System.TimeSpan? timeValue, System.Int32? addValue)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddMicroseconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue"), EdmFunction("Edm", "AddMicroseconds")]
        public static System.DateTimeOffset? AddMicroseconds(System.DateTimeOffset? timeValue, System.Int32? addValue)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddMicroseconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue"), EdmFunction("Edm", "AddMicroseconds")]
        public static System.DateTime? AddMicroseconds(System.DateTime? timeValue, System.Int32? addValue)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddMicroseconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue"), EdmFunction("Edm", "AddMicroseconds")]
        public static System.TimeSpan? AddMicroseconds(System.TimeSpan? timeValue, System.Int32? addValue)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddNanoseconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue"), EdmFunction("Edm", "AddNanoseconds")]
        public static System.DateTimeOffset? AddNanoseconds(System.DateTimeOffset? timeValue, System.Int32? addValue)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddNanoseconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue"), EdmFunction("Edm", "AddNanoseconds")]
        public static System.DateTime? AddNanoseconds(System.DateTime? timeValue, System.Int32? addValue)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.AddNanoseconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue"), EdmFunction("Edm", "AddNanoseconds")]
        public static System.TimeSpan? AddNanoseconds(System.TimeSpan? timeValue, System.Int32? addValue)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffYears
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue1"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue2"), EdmFunction("Edm", "DiffYears")]
        public static System.Int32? DiffYears(System.DateTimeOffset? dateValue1, System.DateTimeOffset? dateValue2)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffYears
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue2"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue1"), EdmFunction("Edm", "DiffYears")]
        public static System.Int32? DiffYears(System.DateTime? dateValue1, System.DateTime? dateValue2)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffMonths
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue2"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue1"), EdmFunction("Edm", "DiffMonths")]
        public static System.Int32? DiffMonths(System.DateTimeOffset? dateValue1, System.DateTimeOffset? dateValue2)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffMonths
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue2"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue1"), EdmFunction("Edm", "DiffMonths")]
        public static System.Int32? DiffMonths(System.DateTime? dateValue1, System.DateTime? dateValue2)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffDays
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue1"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue2"), EdmFunction("Edm", "DiffDays")]
        public static System.Int32? DiffDays(System.DateTimeOffset? dateValue1, System.DateTimeOffset? dateValue2)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffDays
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue1"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue2"), EdmFunction("Edm", "DiffDays")]
        public static System.Int32? DiffDays(System.DateTime? dateValue1, System.DateTime? dateValue2)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffHours
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1"), EdmFunction("Edm", "DiffHours")]
        public static System.Int32? DiffHours(System.DateTimeOffset? timeValue1, System.DateTimeOffset? timeValue2)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffHours
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1"), EdmFunction("Edm", "DiffHours")]
        public static System.Int32? DiffHours(System.DateTime? timeValue1, System.DateTime? timeValue2)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffHours
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1"), EdmFunction("Edm", "DiffHours")]
        public static System.Int32? DiffHours(System.TimeSpan? timeValue1, System.TimeSpan? timeValue2)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffMinutes
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2"), EdmFunction("Edm", "DiffMinutes")]
        public static System.Int32? DiffMinutes(System.DateTimeOffset? timeValue1, System.DateTimeOffset? timeValue2)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffMinutes
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1"), EdmFunction("Edm", "DiffMinutes")]
        public static System.Int32? DiffMinutes(System.DateTime? timeValue1, System.DateTime? timeValue2)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffMinutes
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2"), EdmFunction("Edm", "DiffMinutes")]
        public static System.Int32? DiffMinutes(System.TimeSpan? timeValue1, System.TimeSpan? timeValue2)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffSeconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2"), EdmFunction("Edm", "DiffSeconds")]
        public static System.Int32? DiffSeconds(System.DateTimeOffset? timeValue1, System.DateTimeOffset? timeValue2)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffSeconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1"), EdmFunction("Edm", "DiffSeconds")]
        public static System.Int32? DiffSeconds(System.DateTime? timeValue1, System.DateTime? timeValue2)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffSeconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2"), EdmFunction("Edm", "DiffSeconds")]
        public static System.Int32? DiffSeconds(System.TimeSpan? timeValue1, System.TimeSpan? timeValue2)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffMilliseconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1"), EdmFunction("Edm", "DiffMilliseconds")]
        public static System.Int32? DiffMilliseconds(System.DateTimeOffset? timeValue1, System.DateTimeOffset? timeValue2)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffMilliseconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2"), EdmFunction("Edm", "DiffMilliseconds")]
        public static System.Int32? DiffMilliseconds(System.DateTime? timeValue1, System.DateTime? timeValue2)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffMilliseconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1"), EdmFunction("Edm", "DiffMilliseconds")]
        public static System.Int32? DiffMilliseconds(System.TimeSpan? timeValue1, System.TimeSpan? timeValue2)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffMicroseconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1"), EdmFunction("Edm", "DiffMicroseconds")]
        public static System.Int32? DiffMicroseconds(System.DateTimeOffset? timeValue1, System.DateTimeOffset? timeValue2)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffMicroseconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1"), EdmFunction("Edm", "DiffMicroseconds")]
        public static System.Int32? DiffMicroseconds(System.DateTime? timeValue1, System.DateTime? timeValue2)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffMicroseconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1"), EdmFunction("Edm", "DiffMicroseconds")]
        public static System.Int32? DiffMicroseconds(System.TimeSpan? timeValue1, System.TimeSpan? timeValue2)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffNanoseconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1"), EdmFunction("Edm", "DiffNanoseconds")]
        public static System.Int32? DiffNanoseconds(System.DateTimeOffset? timeValue1, System.DateTimeOffset? timeValue2)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffNanoseconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1"), EdmFunction("Edm", "DiffNanoseconds")]
        public static System.Int32? DiffNanoseconds(System.DateTime? timeValue1, System.DateTime? timeValue2)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.DiffNanoseconds
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2"), EdmFunction("Edm", "DiffNanoseconds")]
        public static System.Int32? DiffNanoseconds(System.TimeSpan? timeValue1, System.TimeSpan? timeValue2)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.Truncate
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "digits"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value"), EdmFunction("Edm", "Truncate")]
        public static System.Double? Truncate(System.Double? value, System.Int32? digits)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

        /// <summary>
        /// Proxy for the function Edm.Truncate
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "digits"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value"), EdmFunction("Edm", "Truncate")]
        public static System.Decimal? Truncate(System.Decimal? value, System.Int32? digits)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Resources.Strings.ELinq_EdmFunctionDirectCall);
        }

    }
}
