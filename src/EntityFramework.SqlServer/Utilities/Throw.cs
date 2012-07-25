// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.SqlServer.Utilities
{
    internal static class Throw
    {
        /// <summary>
        ///     Checks whether the given value is null and throws ArgumentNullException if it is.
        ///     This method should only be used in places where Code Contracts are compiled out in the
        ///     release build but we still need public surface null-checking, such as where a public
        ///     abstract class is implemented by an internal concrete class.
        /// </summary>
        public static void IfNull<T>(T value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }
    }
}
