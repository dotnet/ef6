// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    // <summary>
    // The methods here are called from multiple places with an ObjectContext that may have
    // been created in a variety of ways and ensure that the same code is run regardless of
    // how the context was created.
    // </summary>
    internal class DatabaseOperations
    {
        #region Database operations

        // <summary>
        // Used a delegate to do the actual creation once an ObjectContext has been obtained.
        // This is factored in this way so that we do the same thing regardless of how we get to
        // having an ObjectContext.
        // Note however that a context obtained from only a connection will have no model and so
        // will result in an empty database.
        // </summary>
        public virtual bool Create(ObjectContext objectContext)
        {
            DebugCheck.NotNull(objectContext);

            objectContext.CreateDatabase();
            return true;
        }

        // <summary>
        // Used a delegate to do the actual existence check once an ObjectContext has been obtained.
        // This is factored in this way so that we do the same thing regardless of how we get to
        // having an ObjectContext.
        // </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public virtual bool Exists(DbConnection connection, int? commandTimeout, Lazy<StoreItemCollection> storeItemCollection)
        {
            DebugCheck.NotNull(connection);

            if (connection.State == ConnectionState.Open)
            {
                return true;
            }

            try
            {
                return DbProviderServices.GetProviderServices(connection)
                    .DatabaseExists(connection, commandTimeout, storeItemCollection);
            }
            catch
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
                try
                {
                    connection.Open();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        // <summary>
        // Used a delegate to do the actual check/delete once an ObjectContext has been obtained.
        // This is factored in this way so that we do the same thing regardless of how we get to
        // having an ObjectContext.
        // </summary>
        public virtual void Delete(ObjectContext objectContext)
        {
            DebugCheck.NotNull(objectContext);

            objectContext.DeleteDatabase();
        }

        #endregion
    }
}
