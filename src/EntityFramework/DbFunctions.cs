namespace System.Data.Entity
{
    using System.Collections.Generic;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Resources;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Provides common language runtime (CLR) methods that expose EDM canonical functions
    /// for use in <see cref="DbContext"/> or <see cref="ObjectContext"/> LINQ to Entities queries.
    /// </summary>
    /// <remarks>
    /// Note that this class was called EntityFunctions in some previous versions of Entity Framework.
    /// </remarks>
    public static class DbFunctions
    {
        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical StDev EDM function to calculate 
        /// the standard deviation of the collection. 
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="collection">The collection over which to perform the calculation.</param>
        /// <returns>The standard deviation.</returns>
        [DbFunction("Edm", "StDev")]
        public static double? StandardDeviation(IEnumerable<decimal> collection)
        {
            return BootstrapFunction<decimal, double?>(collection, (MethodInfo)MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical StDev EDM function to calculate 
        /// the standard deviation of the collection. 
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="collection">The collection over which to perform the calculation.</param>
        /// <returns>The standard deviation.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [DbFunction("Edm", "StDev")]
        public static double? StandardDeviation(IEnumerable<decimal?> collection)
        {
            return BootstrapFunction<decimal?, double?>(collection, (MethodInfo)MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical StDev EDM function to calculate 
        /// the standard deviation of the collection. 
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="collection">The collection over which to perform the calculation.</param>
        /// <returns>The standard deviation.</returns>
        [DbFunction("Edm", "StDev")]
        public static double? StandardDeviation(IEnumerable<double> collection)
        {
            return BootstrapFunction<double, double?>(collection, (MethodInfo)MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical StDev EDM function to calculate 
        /// the standard deviation of the collection. 
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="collection">The collection over which to perform the calculation.</param>
        /// <returns>The standard deviation.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [DbFunction("Edm", "StDev")]
        public static double? StandardDeviation(IEnumerable<double?> collection)
        {
            return BootstrapFunction<double?, Double?>(collection, (MethodInfo)MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical StDev EDM function to calculate 
        /// the standard deviation of the collection. 
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="collection">The collection over which to perform the calculation.</param>
        /// <returns>The standard deviation.</returns>
        [DbFunction("Edm", "StDev")]
        public static double? StandardDeviation(IEnumerable<int> collection)
        {
            return BootstrapFunction<int, double?>(collection, (MethodInfo)MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical StDev EDM function to calculate 
        /// the standard deviation of the collection. 
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="collection">The collection over which to perform the calculation.</param>
        /// <returns>The standard deviation.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [DbFunction("Edm", "StDev")]
        public static double? StandardDeviation(IEnumerable<int?> collection)
        {
            return BootstrapFunction<int?, double?>(collection, (MethodInfo)MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical StDev EDM function to calculate 
        /// the standard deviation of the collection. 
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="collection">The collection over which to perform the calculation.</param>
        /// <returns>The standard deviation.</returns>
        [DbFunction("Edm", "StDev")]
        public static double? StandardDeviation(IEnumerable<long> collection)
        {
            return BootstrapFunction<long, double?>(collection, (MethodInfo)MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical StDev EDM function to calculate 
        /// the standard deviation of the collection. 
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="collection">The collection over which to perform the calculation.</param>
        /// <returns>The standard deviation.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [DbFunction("Edm", "StDev")]
        public static double? StandardDeviation(IEnumerable<long?> collection)
        {
            return BootstrapFunction<long?, double?>(collection, (MethodInfo)MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical StDevP EDM function to calculate 
        /// the standard deviation for the population. 
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="collection">The collection over which to perform the calculation.</param>
        /// <returns>The standard deviation for the population.</returns>
        [DbFunction("Edm", "StDevP")]
        public static double? StandardDeviationP(IEnumerable<decimal> collection)
        {
            return BootstrapFunction<decimal, double?>(collection, (MethodInfo)MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical StDevP EDM function to calculate 
        /// the standard deviation for the population. 
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="collection">The collection over which to perform the calculation.</param>
        /// <returns>The standard deviation for the population.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [DbFunction("Edm", "StDevP")]
        public static double? StandardDeviationP(IEnumerable<decimal?> collection)
        {
            return BootstrapFunction<decimal?, double?>(collection, (MethodInfo)MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical StDevP EDM function to calculate 
        /// the standard deviation for the population. 
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="collection">The collection over which to perform the calculation.</param>
        /// <returns>The standard deviation for the population.</returns>
        [DbFunction("Edm", "StDevP")]
        public static double? StandardDeviationP(IEnumerable<double> collection)
        {
            return BootstrapFunction<double, double?>(collection, (MethodInfo)MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical StDevP EDM function to calculate 
        /// the standard deviation for the population. 
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="collection">The collection over which to perform the calculation.</param>
        /// <returns>The standard deviation for the population.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [DbFunction("Edm", "StDevP")]
        public static double? StandardDeviationP(IEnumerable<double?> collection)
        {
            return BootstrapFunction<double?, double?>(collection, (MethodInfo)MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical StDevP EDM function to calculate 
        /// the standard deviation for the population. 
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="collection">The collection over which to perform the calculation.</param>
        /// <returns>The standard deviation for the population.</returns>
        [DbFunction("Edm", "StDevP")]
        public static double? StandardDeviationP(IEnumerable<int> collection)
        {
            return BootstrapFunction<int, double?>(collection, (MethodInfo)MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical StDevP EDM function to calculate 
        /// the standard deviation for the population. 
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="collection">The collection over which to perform the calculation.</param>
        /// <returns>The standard deviation for the population.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [DbFunction("Edm", "StDevP")]
        public static double? StandardDeviationP(IEnumerable<int?> collection)
        {
            return BootstrapFunction<int?, double?>(collection, (MethodInfo)MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical StDevP EDM function to calculate 
        /// the standard deviation for the population. 
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="collection">The collection over which to perform the calculation.</param>
        /// <returns>The standard deviation for the population.</returns>
        [DbFunction("Edm", "StDevP")]
        public static double? StandardDeviationP(IEnumerable<long> collection)
        {
            return BootstrapFunction<long, double?>(collection, (MethodInfo)MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical StDevP EDM function to calculate 
        /// the standard deviation for the population. 
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="collection">The collection over which to perform the calculation.</param>
        /// <returns>The standard deviation for the population.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [DbFunction("Edm", "StDevP")]
        public static double? StandardDeviationP(IEnumerable<long?> collection)
        {
            return BootstrapFunction<long?, double?>(collection, (MethodInfo)MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical Var EDM function to calculate 
        /// the variance of the collection. 
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="collection">The collection over which to perform the calculation.</param>
        /// <returns>The variance.</returns>
        [DbFunction("Edm", "Var")]
        public static double? Var(IEnumerable<decimal> collection)
        {
            return BootstrapFunction<decimal, double?>(collection, (MethodInfo)MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical Var EDM function to calculate 
        /// the variance of the collection. 
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="collection">The collection over which to perform the calculation.</param>
        /// <returns>The variance.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [DbFunction("Edm", "Var")]
        public static double? Var(IEnumerable<decimal?> collection)
        {
            return BootstrapFunction<decimal?, double?>(collection, (MethodInfo)MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical Var EDM function to calculate 
        /// the variance of the collection. 
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="collection">The collection over which to perform the calculation.</param>
        /// <returns>The variance.</returns>
        [DbFunction("Edm", "Var")]
        public static double? Var(IEnumerable<double> collection)
        {
            return BootstrapFunction<double, double?>(collection, (MethodInfo)MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical Var EDM function to calculate 
        /// the variance of the collection. 
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="collection">The collection over which to perform the calculation.</param>
        /// <returns>The variance.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [DbFunction("Edm", "Var")]
        public static double? Var(IEnumerable<double?> collection)
        {
            return BootstrapFunction<double?, double?>(collection, (MethodInfo)MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical Var EDM function to calculate 
        /// the variance of the collection. 
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="collection">The collection over which to perform the calculation.</param>
        /// <returns>The variance.</returns>
        [DbFunction("Edm", "Var")]
        public static double? Var(IEnumerable<int> collection)
        {
            return BootstrapFunction<int, double?>(collection, (MethodInfo)MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical Var EDM function to calculate 
        /// the variance of the collection. 
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="collection">The collection over which to perform the calculation.</param>
        /// <returns>The variance.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [DbFunction("Edm", "Var")]
        public static double? Var(IEnumerable<int?> collection)
        {
            return BootstrapFunction<int?, double?>(collection, (MethodInfo)MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical Var EDM function to calculate 
        /// the variance of the collection. 
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="collection">The collection over which to perform the calculation.</param>
        /// <returns>The variance.</returns>
        [DbFunction("Edm", "Var")]
        public static double? Var(IEnumerable<long> collection)
        {
            return BootstrapFunction<long, double?>(collection, (MethodInfo)MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical Var EDM function to calculate 
        /// the variance of the collection. 
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="collection">The collection over which to perform the calculation.</param>
        /// <returns>The variance.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [DbFunction("Edm", "Var")]
        public static double? Var(IEnumerable<long?> collection)
        {
            return BootstrapFunction<long?, double?>(collection, (MethodInfo)MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical VarP EDM function to calculate 
        /// the variance for the population. 
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="collection">The collection over which to perform the calculation.</param>
        /// <returns>The variance for the population.</returns>
        [DbFunction("Edm", "VarP")]
        public static double? VarP(IEnumerable<decimal> collection)
        {
            return BootstrapFunction<decimal, double?>(collection, (MethodInfo)MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical VarP EDM function to calculate 
        /// the variance for the population. 
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="collection">The collection over which to perform the calculation.</param>
        /// <returns>The variance for the population.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [DbFunction("Edm", "VarP")]
        public static double? VarP(IEnumerable<decimal?> collection)
        {
            return BootstrapFunction<decimal?, double?>(collection, (MethodInfo)MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical VarP EDM function to calculate 
        /// the variance for the population. 
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="collection">The collection over which to perform the calculation.</param>
        /// <returns>The variance for the population.</returns>
        [DbFunction("Edm", "VarP")]
        public static double? VarP(IEnumerable<double> collection)
        {
            return BootstrapFunction<double, double?>(collection, (MethodInfo)MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical VarP EDM function to calculate 
        /// the variance for the population. 
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="collection">The collection over which to perform the calculation.</param>
        /// <returns>The variance for the population.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [DbFunction("Edm", "VarP")]
        public static double? VarP(IEnumerable<double?> collection)
        {
            return BootstrapFunction<double?, double?>(collection, (MethodInfo)MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical VarP EDM function to calculate 
        /// the variance for the population. 
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="collection">The collection over which to perform the calculation.</param>
        /// <returns>The variance for the population.</returns>
        [DbFunction("Edm", "VarP")]
        public static double? VarP(IEnumerable<int> collection)
        {
            return BootstrapFunction<int, double?>(collection, (MethodInfo)MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical VarP EDM function to calculate 
        /// the variance for the population. 
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="collection">The collection over which to perform the calculation.</param>
        /// <returns>The variance for the population.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [DbFunction("Edm", "VarP")]
        public static double? VarP(IEnumerable<int?> collection)
        {
            return BootstrapFunction<int?, double?>(collection, (MethodInfo)MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical VarP EDM function to calculate 
        /// the variance for the population. 
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="collection">The collection over which to perform the calculation.</param>
        /// <returns>The variance for the population.</returns>
        [DbFunction("Edm", "VarP")]
        public static double? VarP(IEnumerable<long> collection)
        {
            return BootstrapFunction<long, double?>(collection, (MethodInfo)MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical VarP EDM function to calculate 
        /// the variance for the population. 
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="collection">The collection over which to perform the calculation.</param>
        /// <returns>The variance for the population.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [DbFunction("Edm", "VarP")]
        public static double? VarP(IEnumerable<long?> collection)
        {
            return BootstrapFunction<long?, double?>(collection, (MethodInfo)MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical Left EDM function to return a given 
        /// number of the leftmost characters in a string.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="stringArgument">The input string.</param>
        /// <param name="length">The number of characters to return</param>
        /// <returns>A string containing the number of characters asked for from the left of the input string.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stringArgument")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "length")]
        [DbFunction("Edm", "Left")]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
        public static String Left(String stringArgument, long? length)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical Right EDM function to return a given 
        /// number of the rightmost characters in a string.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="stringArgument">The input string.</param>
        /// <param name="length">The number of characters to return</param>
        /// <returns>A string containing the number of characters asked for from the right of the input string.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "length")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stringArgument")]
        [DbFunction("Edm", "Right")]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
        public static String Right(String stringArgument, long? length)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical Reverse EDM function to return a given 
        /// string with the order of the characters reversed.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="stringArgument">The input string.</param>
        /// <returns>The input string with the order of the characters reversed.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stringArgument")]
        [DbFunction("Edm", "Reverse")]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
        public static String Reverse(String stringArgument)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical GetTotalOffsetMinutes EDM function to 
        /// return the number of minutes that the given date/time is offset from UTC. This is generally between +780
        /// and -780 (+ or - 13 hrs). 
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateTimeOffsetArgument">The date/time value to use.</param>
        /// <returns>The offset of the input from UTC.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateTimeOffsetArgument")]
        [DbFunction("Edm", "GetTotalOffsetMinutes")]
        public static int? GetTotalOffsetMinutes(DateTimeOffset? dateTimeOffsetArgument)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical TruncateTime EDM function to return
        /// the given date with the time portion cleared.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue">The date/time value to use.</param>
        /// <returns>The input date with the time portion cleared.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue")]
        [DbFunction("Edm", "TruncateTime")]
        public static DateTimeOffset? TruncateTime(DateTimeOffset? dateValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical TruncateTime EDM function to return
        /// the given date with the time portion cleared.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue">The date/time value to use.</param>
        /// <returns>The input date with the time portion cleared.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue")]
        [DbFunction("Edm", "TruncateTime")]
        public static DateTime? TruncateTime(DateTime? dateValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical CreateDateTime EDM function to 
        /// create a new <see cref="DateTime"/> object.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="year">The year.</param>
        /// <param name="month">The month (1-based).</param>
        /// <param name="day">The day (1-based).</param>
        /// <param name="hour">The hours.</param>
        /// <param name="minute">The minutes.</param>
        /// <param name="second">The seconds, including fractional parts of the seconds if desired.</param>
        /// <returns>The new date/time.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "minute")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "second")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "day")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "hour")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "year")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "month")]
        [DbFunction("Edm", "CreateDateTime")]
        public static DateTime? CreateDateTime(int? year, Int32? month, Int32? day, Int32? hour, Int32? minute, double? second)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical CreateDateTimeOffset EDM function to 
        /// create a new <see cref="DateTimeOffset"/> object.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="year">The year.</param>
        /// <param name="month">The month (1-based).</param>
        /// <param name="day">The day (1-based).</param>
        /// <param name="hour">The hours.</param>
        /// <param name="minute">The minutes.</param>
        /// <param name="second">The seconds, including fractional parts of the seconds if desired.</param>
        /// <param name="timeZoneOffset">The time zone offset part of the new date.</param>
        /// <returns>The new date/time.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "month")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeZoneOffset")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "second")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "hour")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "minute")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "day")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "year")]
        [DbFunction("Edm", "CreateDateTimeOffset")]
        public static DateTimeOffset? CreateDateTimeOffset(
            int? year, Int32? month, Int32? day, Int32? hour, Int32? minute, double? second, Int32? timeZoneOffset)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical CreateTime EDM function to 
        /// create a new <see cref="TimeSpan"/> object.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="hour">The hours.</param>
        /// <param name="minute">The minutes.</param>
        /// <param name="second">The seconds, including fractional parts of the seconds if desired.</param>
        /// <returns>The new time span.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "minute")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "hour")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "second")]
        [DbFunction("Edm", "CreateTime")]
        public static TimeSpan? CreateTime(int? hour, Int32? minute, double? second)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical AddYears EDM function to 
        /// add the given number of years to a date/time.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue">The input date/time.</param>
        /// <param name="addValue">The number of years to add.</param>
        /// <returns>A resulting date/time.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue")]
        [DbFunction("Edm", "AddYears")]
        public static DateTimeOffset? AddYears(DateTimeOffset? dateValue, int? addValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical AddYears EDM function to 
        /// add the given number of years to a date/time.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue">The input date/time.</param>
        /// <param name="addValue">The number of years to add.</param>
        /// <returns>A resulting date/time.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [DbFunction("Edm", "AddYears")]
        public static DateTime? AddYears(DateTime? dateValue, int? addValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical AddMonths EDM function to 
        /// add the given number of months to a date/time.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue">The input date/time.</param>
        /// <param name="addValue">The number of months to add.</param>
        /// <returns>A resulting date/time.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [DbFunction("Edm", "AddMonths")]
        public static DateTimeOffset? AddMonths(DateTimeOffset? dateValue, int? addValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical AddMonths EDM function to 
        /// add the given number of months to a date/time.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue">The input date/time.</param>
        /// <param name="addValue">The number of months to add.</param>
        /// <returns>A resulting date/time.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue")]
        [DbFunction("Edm", "AddMonths")]
        public static DateTime? AddMonths(DateTime? dateValue, int? addValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical AddDays EDM function to 
        /// add the given number of days to a date/time.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue">The input date/time.</param>
        /// <param name="addValue">The number of days to add.</param>
        /// <returns>A resulting date/time.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue")]
        [DbFunction("Edm", "AddDays")]
        public static DateTimeOffset? AddDays(DateTimeOffset? dateValue, int? addValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical AddDays EDM function to 
        /// add the given number of days to a date/time.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue">The input date/time.</param>
        /// <param name="addValue">The number of days to add.</param>
        /// <returns>A resulting date/time.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue")]
        [DbFunction("Edm", "AddDays")]
        public static DateTime? AddDays(DateTime? dateValue, int? addValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical AddHours EDM function to 
        /// add the given number of hours to a date/time.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue">The input date/time.</param>
        /// <param name="addValue">The number of hours to add.</param>
        /// <returns>A resulting date/time.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue")]
        [DbFunction("Edm", "AddHours")]
        public static DateTimeOffset? AddHours(DateTimeOffset? timeValue, int? addValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical AddHours EDM function to 
        /// add the given number of hours to a date/time.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue">The input date/time.</param>
        /// <param name="addValue">The number of hours to add.</param>
        /// <returns>A resulting date/time.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue")]
        [DbFunction("Edm", "AddHours")]
        public static DateTime? AddHours(DateTime? timeValue, int? addValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical AddHours EDM function to 
        /// add the given number of hours to a time span.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue">The input date/time.</param>
        /// <param name="addValue">The number of hours to add.</param>
        /// <returns>A resulting time span.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue")]
        [DbFunction("Edm", "AddHours")]
        public static TimeSpan? AddHours(TimeSpan? timeValue, int? addValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical AddMinutes EDM function to 
        /// add the given number of minutes to a date/time.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue">The input date/time.</param>
        /// <param name="addValue">The number of minutes to add.</param>
        /// <returns>A resulting date/time.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue")]
        [DbFunction("Edm", "AddMinutes")]
        public static DateTimeOffset? AddMinutes(DateTimeOffset? timeValue, int? addValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical AddMinutes EDM function to 
        /// add the given number of minutes to a date/time.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue">The input date/time.</param>
        /// <param name="addValue">The number of minutes to add.</param>
        /// <returns>A resulting date/time.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [DbFunction("Edm", "AddMinutes")]
        public static DateTime? AddMinutes(DateTime? timeValue, int? addValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical AddMinutes EDM function to 
        /// add the given number of minutes to a time span.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue">The input date/time.</param>
        /// <param name="addValue">The number of minutes to add.</param>
        /// <returns>A resulting time span.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [DbFunction("Edm", "AddMinutes")]
        public static TimeSpan? AddMinutes(TimeSpan? timeValue, int? addValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical AddSeconds EDM function to 
        /// add the given number of seconds to a date/time.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue">The input date/time.</param>
        /// <param name="addValue">The number of seconds to add.</param>
        /// <returns>A resulting date/time.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue")]
        [DbFunction("Edm", "AddSeconds")]
        public static DateTimeOffset? AddSeconds(DateTimeOffset? timeValue, int? addValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical AddSeconds EDM function to 
        /// add the given number of seconds to a date/time.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue">The input date/time.</param>
        /// <param name="addValue">The number of seconds to add.</param>
        /// <returns>A resulting date/time.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [DbFunction("Edm", "AddSeconds")]
        public static DateTime? AddSeconds(DateTime? timeValue, int? addValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical AddSeconds EDM function to 
        /// add the given number of seconds to a time span.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue">The input date/time.</param>
        /// <param name="addValue">The number of seconds to add.</param>
        /// <returns>A resulting time span.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [DbFunction("Edm", "AddSeconds")]
        public static TimeSpan? AddSeconds(TimeSpan? timeValue, int? addValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical AddMilliseconds EDM function to 
        /// add the given number of milliseconds to a date/time.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue">The input date/time.</param>
        /// <param name="addValue">The number of milliseconds  to add.</param>
        /// <returns>A resulting date/time.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [DbFunction("Edm", "AddMilliseconds")]
        public static DateTimeOffset? AddMilliseconds(DateTimeOffset? timeValue, int? addValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical AddMilliseconds EDM function to 
        /// add the given number of milliseconds to a date/time.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue">The input date/time.</param>
        /// <param name="addValue">The number of milliseconds  to add.</param>
        /// <returns>A resulting date/time.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [DbFunction("Edm", "AddMilliseconds")]
        public static DateTime? AddMilliseconds(DateTime? timeValue, int? addValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical AddMilliseconds EDM function to 
        /// add the given number of milliseconds to a time span.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue">The input date/time.</param>
        /// <param name="addValue">The number of milliseconds to add.</param>
        /// <returns>A resulting time span.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue")]
        [DbFunction("Edm", "AddMilliseconds")]
        public static TimeSpan? AddMilliseconds(TimeSpan? timeValue, int? addValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical AddMicroseconds EDM function to 
        /// add the given number of microseconds to a date/time.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue">The input date/time.</param>
        /// <param name="addValue">The number of microseconds  to add.</param>
        /// <returns>A resulting date/time.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [DbFunction("Edm", "AddMicroseconds")]
        public static DateTimeOffset? AddMicroseconds(DateTimeOffset? timeValue, int? addValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical AddMicroseconds EDM function to 
        /// add the given number of microseconds to a date/time.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue">The input date/time.</param>
        /// <param name="addValue">The number of microseconds  to add.</param>
        /// <returns>A resulting date/time.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [DbFunction("Edm", "AddMicroseconds")]
        public static DateTime? AddMicroseconds(DateTime? timeValue, int? addValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical AddMicroseconds EDM function to 
        /// add the given number of microseconds to a time span.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue">The input date/time.</param>
        /// <param name="addValue">The number of microseconds to add.</param>
        /// <returns>A resulting time span.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [DbFunction("Edm", "AddMicroseconds")]
        public static TimeSpan? AddMicroseconds(TimeSpan? timeValue, int? addValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical AddNanoseconds EDM function to 
        /// add the given number of nanoseconds to a date/time.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue">The input date/time.</param>
        /// <param name="addValue">The number of nanoseconds to add.</param>
        /// <returns>A resulting date/time.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue")]
        [DbFunction("Edm", "AddNanoseconds")]
        public static DateTimeOffset? AddNanoseconds(DateTimeOffset? timeValue, int? addValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical AddNanoseconds EDM function to 
        /// add the given number of nanoseconds to a date/time.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue">The input date/time.</param>
        /// <param name="addValue">The number of nanoseconds to add.</param>
        /// <returns>A resulting date/time.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [DbFunction("Edm", "AddNanoseconds")]
        public static DateTime? AddNanoseconds(DateTime? timeValue, int? addValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical AddNanoseconds EDM function to 
        /// add the given number of nanoseconds to a time span.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue">The input date/time.</param>
        /// <param name="addValue">The number of nanoseconds to add.</param>
        /// <returns>A resulting time span.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue")]
        [DbFunction("Edm", "AddNanoseconds")]
        public static TimeSpan? AddNanoseconds(TimeSpan? timeValue, int? addValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical DiffYears EDM function to 
        /// calculate the number of years between two date/times.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue1">The first date/time.</param>
        /// <param name="dateValue2">The second date/time.</param>
        /// <returns>The number of years between the first and second date/times.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue2")]
        [DbFunction("Edm", "DiffYears")]
        public static int? DiffYears(DateTimeOffset? dateValue1, DateTimeOffset? dateValue2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical DiffYears EDM function to 
        /// calculate the number of years between two date/times.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue1">The first date/time.</param>
        /// <param name="dateValue2">The second date/time.</param>
        /// <returns>The number of years between the first and second date/times.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue1")]
        [DbFunction("Edm", "DiffYears")]
        public static int? DiffYears(DateTime? dateValue1, DateTime? dateValue2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical DiffMonths EDM function to 
        /// calculate the number of months between two date/times.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue1">The first date/time.</param>
        /// <param name="dateValue2">The second date/time.</param>
        /// <returns>The number of months between the first and second date/times.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue1")]
        [DbFunction("Edm", "DiffMonths")]
        public static int? DiffMonths(DateTimeOffset? dateValue1, DateTimeOffset? dateValue2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical DiffMonths EDM function to 
        /// calculate the number of months between two date/times.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue1">The first date/time.</param>
        /// <param name="dateValue2">The second date/time.</param>
        /// <returns>The number of months between the first and second date/times.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue1")]
        [DbFunction("Edm", "DiffMonths")]
        public static int? DiffMonths(DateTime? dateValue1, DateTime? dateValue2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical DiffDays EDM function to 
        /// calculate the number of days between two date/times.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue1">The first date/time.</param>
        /// <param name="dateValue2">The second date/time.</param>
        /// <returns>The number of days between the first and second date/times.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue2")]
        [DbFunction("Edm", "DiffDays")]
        public static int? DiffDays(DateTimeOffset? dateValue1, DateTimeOffset? dateValue2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical DiffDays EDM function to 
        /// calculate the number of days between two date/times.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue1">The first date/time.</param>
        /// <param name="dateValue2">The second date/time.</param>
        /// <returns>The number of days between the first and second date/times.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateValue2")]
        [DbFunction("Edm", "DiffDays")]
        public static int? DiffDays(DateTime? dateValue1, DateTime? dateValue2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical DiffHours EDM function to 
        /// calculate the number of hours between two date/times.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue1">The first date/time.</param>
        /// <param name="dateValue2">The second date/time.</param>
        /// <returns>The number of hours between the first and second date/times.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1")]
        [DbFunction("Edm", "DiffHours")]
        public static int? DiffHours(DateTimeOffset? timeValue1, DateTimeOffset? timeValue2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical DiffHours EDM function to 
        /// calculate the number of hours between two date/times.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue1">The first date/time.</param>
        /// <param name="dateValue2">The second date/time.</param>
        /// <returns>The number of hours between the first and second date/times.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1")]
        [DbFunction("Edm", "DiffHours")]
        public static int? DiffHours(DateTime? timeValue1, DateTime? timeValue2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical DiffHours EDM function to 
        /// calculate the number of hours between two time spans.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue1">The first time span.</param>
        /// <param name="dateValue2">The second time span.</param>
        /// <returns>The number of hours between the first and second time spans.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1")]
        [DbFunction("Edm", "DiffHours")]
        public static int? DiffHours(TimeSpan? timeValue1, TimeSpan? timeValue2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical DiffMinutes EDM function to 
        /// calculate the number of minutes between two date/times.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue1">The first date/time.</param>
        /// <param name="dateValue2">The second date/time.</param>
        /// <returns>The number of minutes between the first and second date/times.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2")]
        [DbFunction("Edm", "DiffMinutes")]
        public static int? DiffMinutes(DateTimeOffset? timeValue1, DateTimeOffset? timeValue2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical DiffMinutes EDM function to 
        /// calculate the number of minutes between two date/times.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue1">The first date/time.</param>
        /// <param name="dateValue2">The second date/time.</param>
        /// <returns>The number of minutes between the first and second date/times.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1")]
        [DbFunction("Edm", "DiffMinutes")]
        public static int? DiffMinutes(DateTime? timeValue1, DateTime? timeValue2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical DiffMinutes EDM function to 
        /// calculate the number of minutes between two time spans.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue1">The first time span.</param>
        /// <param name="dateValue2">The second time span.</param>
        /// <returns>The number of minutes between the first and second time spans.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2")]
        [DbFunction("Edm", "DiffMinutes")]
        public static int? DiffMinutes(TimeSpan? timeValue1, TimeSpan? timeValue2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical DiffSeconds EDM function to 
        /// calculate the number of seconds between two date/times.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue1">The first date/time.</param>
        /// <param name="dateValue2">The second date/time.</param>
        /// <returns>The number of seconds between the first and second date/times.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2")]
        [DbFunction("Edm", "DiffSeconds")]
        public static int? DiffSeconds(DateTimeOffset? timeValue1, DateTimeOffset? timeValue2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical DiffSeconds EDM function to 
        /// calculate the number of seconds between two date/times.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue1">The first date/time.</param>
        /// <param name="dateValue2">The second date/time.</param>
        /// <returns>The number of seconds between the first and second date/times.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1")]
        [DbFunction("Edm", "DiffSeconds")]
        public static int? DiffSeconds(DateTime? timeValue1, DateTime? timeValue2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical DiffSeconds EDM function to 
        /// calculate the number of seconds between two time spans.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue1">The first time span.</param>
        /// <param name="dateValue2">The second time span.</param>
        /// <returns>The number of seconds between the first and second time spans.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2")]
        [DbFunction("Edm", "DiffSeconds")]
        public static int? DiffSeconds(TimeSpan? timeValue1, TimeSpan? timeValue2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical DiffMilliseconds EDM function to 
        /// calculate the number of milliseconds between two date/times.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue1">The first date/time.</param>
        /// <param name="dateValue2">The second date/time.</param>
        /// <returns>The number of milliseconds between the first and second date/times.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1")]
        [DbFunction("Edm", "DiffMilliseconds")]
        public static int? DiffMilliseconds(DateTimeOffset? timeValue1, DateTimeOffset? timeValue2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical DiffMilliseconds EDM function to 
        /// calculate the number of milliseconds between two date/times.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue1">The first date/time.</param>
        /// <param name="dateValue2">The second date/time.</param>
        /// <returns>The number of milliseconds between the first and second date/times.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2")]
        [DbFunction("Edm", "DiffMilliseconds")]
        public static int? DiffMilliseconds(DateTime? timeValue1, DateTime? timeValue2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical DiffMilliseconds EDM function to 
        /// calculate the number of milliseconds between two time spans.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue1">The first time span.</param>
        /// <param name="dateValue2">The second time span.</param>
        /// <returns>The number of milliseconds between the first and second time spans.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1")]
        [DbFunction("Edm", "DiffMilliseconds")]
        public static int? DiffMilliseconds(TimeSpan? timeValue1, TimeSpan? timeValue2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical DiffMicroseconds EDM function to 
        /// calculate the number of microseconds between two date/times.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue1">The first date/time.</param>
        /// <param name="dateValue2">The second date/time.</param>
        /// <returns>The number of microseconds between the first and second date/times.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1")]
        [DbFunction("Edm", "DiffMicroseconds")]
        public static int? DiffMicroseconds(DateTimeOffset? timeValue1, DateTimeOffset? timeValue2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical DiffMicroseconds EDM function to 
        /// calculate the number of microseconds between two date/times.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue1">The first date/time.</param>
        /// <param name="dateValue2">The second date/time.</param>
        /// <returns>The number of microseconds between the first and second date/times.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1")]
        [DbFunction("Edm", "DiffMicroseconds")]
        public static int? DiffMicroseconds(DateTime? timeValue1, DateTime? timeValue2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical DiffMicroseconds EDM function to 
        /// calculate the number of microseconds between two time spans.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue1">The first time span.</param>
        /// <param name="dateValue2">The second time span.</param>
        /// <returns>The number of microseconds between the first and second time spans.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1")]
        [DbFunction("Edm", "DiffMicroseconds")]
        public static int? DiffMicroseconds(TimeSpan? timeValue1, TimeSpan? timeValue2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical DiffNanoseconds EDM function to 
        /// calculate the number of nanoseconds between two date/times.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue1">The first date/time.</param>
        /// <param name="dateValue2">The second date/time.</param>
        /// <returns>The number of nanoseconds between the first and second date/times.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1")]
        [DbFunction("Edm", "DiffNanoseconds")]
        public static int? DiffNanoseconds(DateTimeOffset? timeValue1, DateTimeOffset? timeValue2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical DiffNanoseconds EDM function to 
        /// calculate the number of nanoseconds between two date/times.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue1">The first date/time.</param>
        /// <param name="dateValue2">The second date/time.</param>
        /// <returns>The number of nanoseconds between the first and second date/times.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1")]
        [DbFunction("Edm", "DiffNanoseconds")]
        public static int? DiffNanoseconds(DateTime? timeValue1, DateTime? timeValue2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical DiffNanoseconds EDM function to 
        /// calculate the number of nanoseconds between two time spans.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="dateValue1">The first time span.</param>
        /// <param name="dateValue2">The second time span.</param>
        /// <returns>The number of nanoseconds between the first and second time spans.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeValue2")]
        [DbFunction("Edm", "DiffNanoseconds")]
        public static int? DiffNanoseconds(TimeSpan? timeValue1, TimeSpan? timeValue2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical Truncate EDM function to 
        /// truncate the given value to the number of specified digits.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="value">The value to truncate.</param>
        /// <param name="digits">The number of digits to preserve.</param>
        /// <returns>The truncated value.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "digits")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
        [DbFunction("Edm", "Truncate")]
        public static double? Truncate(Double? value, int? digits)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method invokes the canonical Truncate EDM function to 
        /// truncate the given value to the number of specified digits.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function is translated to a corresponding function in the database.
        /// </remarks>
        /// <param name="value">The value to truncate.</param>
        /// <param name="digits">The number of digits to preserve.</param>
        /// <returns>The truncated value.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "digits")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
        [DbFunction("Edm", "Truncate")]
        public static decimal? Truncate(Decimal? value, int? digits)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method acts as an operator that ensures the input
        /// is treated as a Unicode string.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function impacts the way the LINQ query is translated to a query that can be run in the database.
        /// </remarks>
        /// <param name="value">The input string.</param>
        /// <returns>The input string treated as a Unicode string.</returns>
        public static string AsUnicode(string value)
        {
            return value;
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method acts as an operator that ensures the input
        /// is treated as a non-Unicode string.
        /// </summary>
        /// <remarks>
        /// You cannot call this function directly. This function can only appear within a LINQ to Entities query. 
        /// This function impacts the way the LINQ query is translated to a query that can be run in the database.
        /// </remarks>
        /// <param name="value">The input string.</param>
        /// <returns>The input string treated as a non-Unicode string.</returns>
        public static string AsNonUnicode(string value)
        {
            return value;
        }

        private static TOut BootstrapFunction<TIn, TOut>(IEnumerable<TIn> collection, MethodInfo method)
        {
            var asQueryable = collection as IQueryable;
            if (asQueryable != null)
            {
                return asQueryable.Provider.Execute<TOut>(Expression.Call(method, Expression.Constant(collection)));
            }

            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }
    }
}
