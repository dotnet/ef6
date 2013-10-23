// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Common
{
    using System;
    using Microsoft.Data.Tools.XmlDesignerBase.Common;

    /// <summary>
    ///     Common validation routines for argument validation.
    /// </summary>
    internal static class ArgumentValidation
    {
        /// <summary>
        ///     Check if the variable is an empty string
        /// </summary>
        /// <param name="variable">The value to check</param>
        /// <param name="variableName">The name of the variable being checked</param>
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        public static void CheckForEmptyString(string variable, string variableName)
        {
            CheckForNullReference(variable, variableName);
            CheckForNullReference(variableName, "variableName");
            if (variable.Length == 0)
            {
                throw new ArgumentException(
                    CommonResourceUtil.GetString(CommonResource.ExceptionEmptyString, variableName));
            }
        }

        /// <summary>
        ///     Check if the variable is null
        /// </summary>
        /// <param name="variable">The value to check</param>
        /// <param name="variableName">The name of the variable being checked</param>
        /// <exception cref="ArgumentNullException" />
        public static void CheckForNullReference(object variable, string variableName)
        {
            if (variableName == null)
            {
                throw new ArgumentNullException("variableName");
            }

            if (null == variable)
            {
                throw new ArgumentNullException(variableName);
            }
        }

        /// <summary>
        ///     Check variable to determine if it is within, or equal to, the min and max range.
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <param name="min">'value' must be equal to or greater then this value</param>
        /// <param name="max">'value' must be equal to or less then this value</param>
        /// <exception cref="ArgumentOutOfRangeException" />
        public static void CheckForOutOfRangeException(long value, long min, long max)
        {
            if (value < min
                || value > max)
            {
                throw new ArgumentOutOfRangeException(
                    CommonResourceUtil.GetString(
                        CommonResource.ExceptionIndexOutOfRange,
                        value,
                        min,
                        max));
            }
        }
    }
}
