// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.DataClasses
{
    /// <summary>
    ///     Indicates that the given method is a proxy for an EDM function.
    /// </summary>
    /// <remarks>
    ///     Note that this attribute has been replaced by the <see cref="DbFunctionAttribute"/> starting with EF6.
    /// </remarks>
    [Obsolete("This attribute has been replaced by System.Data.Entity.DbFunctionAttribute.")]
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class EdmFunctionAttribute : DbFunctionAttribute
    {
        /// <summary>
        ///     Creates a new DbFunctionAttribute instance.
        /// </summary>
        /// <param name="namespaceName"> The namespace name of the EDM function represented by the attributed method. </param>
        /// <param name="functionName"> The function name of the EDM function represented by the attributed method. </param>
        public EdmFunctionAttribute(string namespaceName, string functionName)
            : base(namespaceName, functionName)
        {
        }
    }
}
