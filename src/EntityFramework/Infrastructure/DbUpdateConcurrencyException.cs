// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Core;
    using System.Data.Entity.Internal;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception thrown by <see cref="DbContext" /> when it was expected that SaveChanges for an entity would
    /// result in a database update but in fact no rows in the database were affected.  This usually indicates
    /// that the database has been concurrently updated such that a concurrency token that was expected to match
    /// did not actually match.
    /// Note that state entries referenced by this exception are not serialized due to security and accesses to
    /// the state entries after serialization will return null.
    /// </summary>
    [Serializable]
    public class DbUpdateConcurrencyException : DbUpdateException
    {
        #region Fields and constructors

        // <summary>
        // Initializes a new instance of the <see cref="DbUpdateConcurrencyException" /> class.
        // </summary>
        // <param name="context"> The context. </param>
        // <param name="innerException"> The inner exception. </param>
        internal DbUpdateConcurrencyException(InternalContext context, OptimisticConcurrencyException innerException)
            : base(context, innerException, involvesIndependentAssociations: false)
        {
        }

        #endregion

        #region Required by FxCop

        /// <summary>
        /// Initializes a new instance of the <see cref="DbUpdateException" /> class.
        /// </summary>
        public DbUpdateConcurrencyException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbUpdateException" /> class.
        /// </summary>
        /// <param name="message"> The message. </param>
        public DbUpdateConcurrencyException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbUpdateException" /> class.
        /// </summary>
        /// <param name="message"> The message. </param>
        /// <param name="innerException"> The inner exception. </param>
        public DbUpdateConcurrencyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the DbUpdateConcurrencyException class with the specified serialization information and context.
        /// </summary>
        /// <param name="info"> The data necessary to serialize or deserialize an object. </param>
        /// <param name="context"> Description of the source and destination of the specified serialized stream. </param>
        protected DbUpdateConcurrencyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion
    }
}
