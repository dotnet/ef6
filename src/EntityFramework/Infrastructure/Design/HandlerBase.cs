// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Design
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Base handler type. Handlers aren't required to use this exact type. Only the
    /// namespace, name, and member signatures need to be the same. This also applies to
    /// handler contracts types
    /// </summary>
    public abstract class HandlerBase : MarshalByRefObject
    {
        /// <summary>
        /// Indicates whether the specified contract is implemented by this handler.
        /// </summary>
        /// <param name="interfaceName">The full name of the contract interface.</param>
        /// <returns><c>True</c> if the contract is implemented, otherwise <c>false</c>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public virtual bool ImplementsContract(string interfaceName)
        {
            Type interfaceType;
            try
            {
                interfaceType = Type.GetType(interfaceName, throwOnError: true);
            }
            catch
            {
                return false;
            }

            return interfaceType.IsAssignableFrom(GetType());
        }
        
        public override object InitializeLifetimeService()
        {
            // Leaks but fixes https://github.com/dotnet/ef6/issues/96
            return null;
        }
    }
}
