// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Indicates that the given method is a proxy for an EDM function.
    /// </summary>
    /// <remarks>
    /// Note that this class was called EdmFunctionAttribute in some previous versions of Entity Framework.
    /// </remarks>
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes")]
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class DbFunctionAttribute : Attribute
    {
        private readonly string _namespaceName;
        private readonly string _functionName;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.DbFunctionAttribute" /> class.
        /// </summary>
        /// <param name="namespaceName">The namespace of the mapped-to function.</param>
        /// <param name="functionName">The name of the mapped-to function.</param>
        public DbFunctionAttribute(string namespaceName, string functionName)
        {
            Check.NotEmpty(namespaceName, "namespaceName");
            Check.NotEmpty(functionName, "functionName");

            _namespaceName = namespaceName;
            _functionName = functionName;
        }

        /// <summary>The namespace of the mapped-to function.</summary>
        /// <returns>The namespace of the mapped-to function.</returns>
        public string NamespaceName
        {
            get { return _namespaceName; }
        }

        /// <summary>The name of the mapped-to function.</summary>
        /// <returns>The name of the mapped-to function.</returns>
        public string FunctionName
        {
            get { return _functionName; }
        }
    }
}
