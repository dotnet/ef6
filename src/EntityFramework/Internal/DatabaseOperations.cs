// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     The methods here are called from multiple places with an ObjectContext that may have
    ///     been created in a variety of ways and ensure that the same code is run regardless of
    ///     how the context was created.
    /// </summary>
    internal class DatabaseOperations
    {
        #region Database operations

        /// <summary>
        ///     Used a delegate to do the actual creation once an ObjectContext has been obtained.
        ///     This is factored in this way so that we do the same thing regardless of how we get to
        ///     having an ObjectContext.
        ///     Note however that a context obtained from only a connection will have no model and so
        ///     will result in an empty database.
        /// </summary>
        public virtual bool Create(ObjectContext objectContext)
        {
            DebugCheck.NotNull(objectContext);

            objectContext.CreateDatabase();
            return true;
        }

        /// <summary>
        ///     Used a delegate to do the actual existence check once an ObjectContext has been obtained.
        ///     This is factored in this way so that we do the same thing regardless of how we get to
        ///     having an ObjectContext.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public virtual bool Exists(ObjectContext objectContext)
        {
            DebugCheck.NotNull(objectContext);

            return objectContext.DatabaseExists();
        }

        /// <summary>
        ///     Used a delegate to do the actual check/delete once an ObjectContext has been obtained.
        ///     This is factored in this way so that we do the same thing regardless of how we get to
        ///     having an ObjectContext.
        /// </summary>
        public virtual bool DeleteIfExists(ObjectContext objectContext)
        {
            DebugCheck.NotNull(objectContext);

            if (Exists(objectContext))
            {
                objectContext.DeleteDatabase();
                return true;
            }
            return false;
        }

        #endregion
    }
}
