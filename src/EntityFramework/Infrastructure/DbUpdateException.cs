namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Generic;
    using System.Data.Entity.Core;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Exception thrown by <see cref = "DbContext" /> when the saving of changes to the database fails.
    ///     Note that update issues that involve independent associations will result in
    ///     an <see cref = "DbIndependentAssociationUpdateException"/ and not an instance of this exception. Note that state entries referenced by this exception are not serialized due to security and accesses to the state entries after serialization will return null.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors",
        Justification = "SerializeObjectState used instead")]
    [Serializable]
    public class DbUpdateException : DataException
    {
        #region Fields and constructors

        [NonSerialized]
        private readonly InternalContext _internalContext;

        [NonSerialized]
        private DbUpdateExceptionState _state;

        /// <summary>
        ///     Initializes a new instance of the <see cref = "DbUpdateException" /> class.
        /// </summary>
        /// <param name = "internalContext">The internal context.</param>
        /// <param name = "innerException">The inner exception.</param>
        internal DbUpdateException(
            InternalContext internalContext, UpdateException innerException, bool involvesIndependentAssociations)
            : base(
                involvesIndependentAssociations
                    ? Strings.DbContext_IndependentAssociationUpdateException
                    : innerException.Message,
                innerException)
        {
            _internalContext = internalContext;
            _state.InvolvesIndependentAssociations = involvesIndependentAssociations;

            SubscribeToSerializeObjectState();
        }

        #endregion

        #region Access to state entries

        /// <summary>
        ///     Gets <see cref = "DbEntityEntry" /> objects that represents the entities that could not
        ///     be saved to the database.
        /// </summary>
        /// <returns>The entries representing the entities that could not be saved.</returns>
        public IEnumerable<DbEntityEntry> Entries
        {
            get
            {
                // We do all of this checking because of all the FxCop-required constructors
                // that allow the exception object to be in virtually any state.
                var innerAsUpdateException = InnerException as UpdateException;
                if (_state.InvolvesIndependentAssociations || _internalContext == null || innerAsUpdateException == null
                    || innerAsUpdateException.StateEntries == null)
                {
                    return Enumerable.Empty<DbEntityEntry>();
                }

                Contract.Assert(
                    !innerAsUpdateException.StateEntries.Any(e => e.Entity == null),
                    "Should not have stubs or relationship entries with this exception type.");

                return innerAsUpdateException.StateEntries.Select(
                    e => new DbEntityEntry(new InternalEntityEntry(_internalContext, new StateEntryAdapter(e))));
            }
        }

        #endregion

        #region Required by FxCop

        /// <summary>
        ///     Initializes a new instance of the <see cref = "DbUpdateException" /> class.
        /// </summary>
        public DbUpdateException()
        {
            SubscribeToSerializeObjectState();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref = "DbUpdateException" /> class.
        /// </summary>
        /// <param name = "message">The message.</param>
        public DbUpdateException(string message)
            : base(message)
        {
            SubscribeToSerializeObjectState();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref = "DbUpdateException" /> class.
        /// </summary>
        /// <param name = "message">The message.</param>
        /// <param name = "innerException">The inner exception.</param>
        public DbUpdateException(string message, Exception innerException)
            : base(message, innerException)
        {
            SubscribeToSerializeObjectState();
        }

        /// <summary>
        ///     Subscribes the SerializeObjectState event.
        /// </summary>
        private void SubscribeToSerializeObjectState()
        {
            SerializeObjectState += (exception, eventArgs) => eventArgs.AddSerializedState(_state);
        }

        /// <summary>
        ///     Holds exception state that will be serialized when the exception is serialized.
        /// </summary>
        [Serializable]
        private struct DbUpdateExceptionState : ISafeSerializationData
        {
            /// <summary>
            ///     Gets or sets a value indicating whether the exception involved independent associations.
            /// </summary>
            public bool InvolvesIndependentAssociations { get; set; }

            /// <summary>
            ///     Completes the deserialization.
            /// </summary>
            /// <param name = "deserialized">The deserialized object.</param>
            public void CompleteDeserialization(object deserialized)
            {
                ((DbUpdateException)deserialized)._state = this;
            }
        }

        #endregion
    }
}
