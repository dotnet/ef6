// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Generic;
    using System.Data.Entity.Core;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception thrown by <see cref="DbContext" /> when the saving of changes to the database fails.
    /// Note that state entries referenced by this exception are not serialized due to security and accesses to the
    /// state entries after serialization will return null.
    /// </summary>
    [Serializable]
    public class DbUpdateException : DataException
    {
        #region Fields and constructors

        [NonSerialized]
        private readonly InternalContext _internalContext;

        private readonly bool _involvesIndependentAssociations;

        // <summary>
        // Initializes a new instance of the <see cref="DbUpdateException" /> class.
        // </summary>
        // <param name="internalContext"> The internal context. </param>
        // <param name="innerException"> The inner exception. </param>
        internal DbUpdateException(
            InternalContext internalContext, UpdateException innerException, bool involvesIndependentAssociations)
            : base(
                involvesIndependentAssociations
                    ? Strings.DbContext_IndependentAssociationUpdateException
                    : innerException.Message,
                innerException)
        {
            _internalContext = internalContext;
            _involvesIndependentAssociations = involvesIndependentAssociations;
        }

        #endregion

        #region Access to state entries

        /// <summary>
        /// Gets <see cref="DbEntityEntry" /> objects that represents the entities that could not
        /// be saved to the database.
        /// </summary>
        /// <returns> The entries representing the entities that could not be saved. </returns>
        public IEnumerable<DbEntityEntry> Entries
        {
            get
            {
                // We do all of this checking because of all the FxCop-required constructors
                // that allow the exception object to be in virtually any state.
                var innerAsUpdateException = InnerException as UpdateException;
                if (_involvesIndependentAssociations
                    || _internalContext == null
                    || innerAsUpdateException == null
                    || innerAsUpdateException.StateEntries == null)
                {
                    return Enumerable.Empty<DbEntityEntry>();
                }

                Debug.Assert(
                    !innerAsUpdateException.StateEntries.Any(e => e.Entity == null),
                    "Should not have stubs or relationship entries with this exception type.");

                return innerAsUpdateException.StateEntries.Select(
                    e => new DbEntityEntry(new InternalEntityEntry(_internalContext, new StateEntryAdapter(e))));
            }
        }

        #endregion

        #region Required by FxCop

        /// <summary>
        /// Initializes a new instance of the <see cref="DbUpdateException" /> class.
        /// </summary>
        public DbUpdateException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbUpdateException" /> class.
        /// </summary>
        /// <param name="message"> The message. </param>
        public DbUpdateException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbUpdateException" /> class.
        /// </summary>
        /// <param name="message"> The message. </param>
        /// <param name="innerException"> The inner exception. </param>
        public DbUpdateException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the DbUpdateException class with the specified serialization information and context.
        /// </summary>
        /// <param name="info"> The data necessary to serialize or deserialize an object. </param>
        /// <param name="context"> Description of the source and destination of the specified serialized stream. </param>
        protected DbUpdateException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _involvesIndependentAssociations = info.GetBoolean("InvolvesIndependentAssociations");
        }

        /// <summary>
        /// Sets the <see cref="SerializationInfo" /> with information about the exception.
        /// </summary>
        /// <param name="info"> The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown. </param>
        /// <param name="context"> The <see cref="StreamingContext" /> that contains contextual information about the source or destination. </param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("InvolvesIndependentAssociations", _involvesIndependentAssociations);
        }

        #endregion
    }
}
