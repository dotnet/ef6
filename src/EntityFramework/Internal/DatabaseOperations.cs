// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Objects;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

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
            Contract.Requires(objectContext != null);

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
            Contract.Requires(objectContext != null);

            try
            {
                return objectContext.DatabaseExists();
            }
            catch (Exception)
            {
                // In situations where the user does not have access to the master database
                // the above DatabaseExists call fails and throws an exception.  Rather than
                // just let that exception escape to the caller we instead try a different
                // approach to see if the database really does exist or not.  The approach
                // is to try to open a connection to the database.  If this succeeds then
                // we know that the database exists.  If it fails then the database may
                // not exist or there may be some other issue connecting to it.  In either
                // case for the purpose of this call we assume that it does not exist and
                // return false since this functionally gives the best experience in most
                // scenarios.
                if (objectContext.Connection.State
                    == ConnectionState.Open)
                {
                    return true;
                }
                try
                {
                    objectContext.Connection.Open();
                    return true;
                }
                catch (EntityException)
                {
                    return false;
                }
                finally
                {
                    objectContext.Connection.Close();
                }
            }
        }

        /// <summary>
        ///     Used a delegate to do the actual check/delete once an ObjectContext has been obtained.
        ///     This is factored in this way so that we do the same thing regardless of how we get to
        ///     having an ObjectContext.
        /// </summary>
        public virtual bool DeleteIfExists(ObjectContext objectContext)
        {
            Contract.Requires(objectContext != null);

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
