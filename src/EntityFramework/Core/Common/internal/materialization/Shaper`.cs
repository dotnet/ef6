// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Internal.Materialization
{
    using System.Collections;
    using System.Data.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Typed Shaper. Includes logic to enumerate results and wraps the _rootCoordinator,
    /// which includes materializer delegates for the root query collection.
    /// </summary>
    internal class Shaper<T> : Shaper
    {
        #region Private Fields

        /// <summary>
        /// Which type of query is this, object layer (true) or value layer (false)
        /// </summary>
        private readonly bool _isObjectQuery;

        /// <summary>
        /// Keeps track of whether we've completed processing or not.
        /// </summary>
        private bool _isActive;

        /// <summary>
        /// The enumerator we're using to read data; really only populated for value
        /// layer queries.
        /// </summary>
        private IDbEnumerator<T> _rootEnumerator;

        /// <summary>
        /// Is the reader owned by the EF or was it supplied by the user?
        /// </summary>
        private readonly bool _readerOwned;

        /// <summary>
        /// Should the connection be released when the shaper is disposed
        /// </summary>
        private readonly bool _shouldReleaseConnection;

        #endregion

        internal Shaper(
            DbDataReader reader, ObjectContext context, MetadataWorkspace workspace, MergeOption mergeOption,
            int stateCount, CoordinatorFactory<T> rootCoordinatorFactory, bool readerOwned, bool useSpatialReader,
            bool shouldReleaseConnection)
            : base(reader, context, workspace, mergeOption, stateCount, useSpatialReader)
        {
            DebugCheck.NotNull(rootCoordinatorFactory);

            RootCoordinator = (Coordinator<T>)rootCoordinatorFactory.CreateCoordinator(parent: null, next: null);
            _isObjectQuery = !(typeof(T) == typeof(RecordState));
            _isActive = true;
            RootCoordinator.Initialize(this);
            _readerOwned = readerOwned;
            _shouldReleaseConnection = shouldReleaseConnection;
        }

        #region "Public" Surface Area

        /// <summary>
        /// Events raised when the shaper has finished enumerating results. Useful for callback
        /// to set parameter values.
        /// </summary>
        internal event EventHandler OnDone;

        /// <summary>
        /// Used to handle the read-ahead requirements of value-layer queries.  This
        /// field indicates the status of the current value of the _rootEnumerator; when
        /// a bridge data reader "accepts responsibility" for the current value, it sets
        /// this to false.
        /// </summary>
        internal bool DataWaiting { get; set; }

        /// <summary>
        /// Shapers and Coordinators work together in harmony to materialize the data
        /// from the store; the shaper contains the state, the coordinator contains the
        /// code.
        /// </summary>
        internal readonly Coordinator<T> RootCoordinator;

        /// <summary>
        /// The enumerator that the value-layer bridge will use to read data; all nested
        /// data readers need to use the same enumerator, so we put it on the Shaper, since
        /// that is something that all the nested data readers (and data records) have access
        /// to -- it prevents us from having to pass two objects around.
        /// </summary>
        internal IDbEnumerator<T> RootEnumerator
        {
            get
            {
                if (_rootEnumerator == null)
                {
                    InitializeRecordStates(RootCoordinator.CoordinatorFactory);
                    _rootEnumerator = GetEnumerator();
                }
                return _rootEnumerator;
            }
        }

        /// <summary>
        /// Initialize the RecordStateFactory objects in their StateSlots.
        /// </summary>
        private void InitializeRecordStates(CoordinatorFactory coordinatorFactory)
        {
            foreach (var recordStateFactory in coordinatorFactory.RecordStateFactories)
            {
                State[recordStateFactory.StateSlotNumber] = recordStateFactory.Create(coordinatorFactory);
            }

            foreach (var nestedCoordinatorFactory in coordinatorFactory.NestedCoordinators)
            {
                InitializeRecordStates(nestedCoordinatorFactory);
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public virtual IDbEnumerator<T> GetEnumerator()
        {
            // we can use a simple enumerator if there are no nested results, no keys and no "has data"
            // discriminator
            if (RootCoordinator.CoordinatorFactory.IsSimple)
            {
                return new SimpleEnumerator(this);
            }
            else
            {
                var rowEnumerator = new RowNestedResultEnumerator(this);

                if (_isObjectQuery)
                {
                    return new ObjectQueryNestedEnumerator(rowEnumerator);
                }
                else
                {
                    return (IDbEnumerator<T>)(new RecordStateEnumerator(rowEnumerator));
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Called when enumeration of results has completed.
        /// </summary>
        private void Finally()
        {
            if (_isActive)
            {
                _isActive = false;

                if (_readerOwned)
                {
                    // I'd prefer not to special case this, but value-layer behavior is that you
                    // must explicitly close the data reader; if we automatically dispose of the
                    // reader here, we won't have that behavior.
                    if (_isObjectQuery)
                    {
                        Reader.Dispose();
                    }

                    // This case includes when the ObjectResult is disposed before it 
                    // created an ObjectQueryEnumeration; at this time, the connection can be released
                    if (Context != null && _shouldReleaseConnection)
                    {
                        Context.ReleaseConnection();
                    }
                }

                if (null != OnDone)
                {
                    OnDone(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// Reads the next row from the store. If there is a failure, throws an exception message
        /// in some scenarios (note that we respond to failure rather than anticipate failure,
        /// avoiding repeated checks in the inner materialization loop)
        /// </summary>
        private bool StoreRead()
        {
            bool readSucceeded;
            try
            {
                readSucceeded = Reader.Read();
            }
            catch (Exception e)
            {
                HandleReaderException(e);

                throw;
            }
            return readSucceeded;
        }

#if !NET40

        private async Task<bool> StoreReadAsync(CancellationToken cancellationToken)
        {
            bool readSucceeded;
            try
            {
                readSucceeded = await Reader.ReadAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            }
            catch (Exception e)
            {
                HandleReaderException(e);

                throw;
            }
            return readSucceeded;
        }

#endif

        private void HandleReaderException(Exception e)
        {
            // wrap exception if necessary
            if (e.IsCatchableEntityExceptionType())
            {
                // check if the reader is closed; if so, throw friendlier exception
                if (Reader.IsClosed)
                {
                    throw new EntityCommandExecutionException((Strings.ADP_DataReaderClosed("Read")), e);
                }

                throw new EntityCommandExecutionException(Strings.EntityClient_StoreReaderFailed, e);
            }
        }

        /// <summary>
        /// Notify ObjectContext that we are about to start materializing an element
        /// </summary>
        private void StartMaterializingElement()
        {
            if (Context != null)
            {
                Context.InMaterialization = true;
                InitializeForOnMaterialize();
            }
        }

        /// <summary>
        /// Notify ObjectContext that we are finished materializing the element
        /// </summary>
        private void StopMaterializingElement()
        {
            if (Context != null)
            {
                Context.InMaterialization = false;
                RaiseMaterializedEvents();
            }
        }

        #endregion

        #region Simple Enumerator

        /// <summary>
        /// Optimized enumerator for queries not including nested results.
        /// </summary>
        private class SimpleEnumerator : IDbEnumerator<T>
        {
            private readonly Shaper<T> _shaper;

            internal SimpleEnumerator(Shaper<T> shaper)
            {
                _shaper = shaper;
            }

            public T Current
            {
                get { return _shaper.RootCoordinator.Current; }
            }

            object IEnumerator.Current
            {
                get { return _shaper.RootCoordinator.Current; }
            }

#if !NET40

            object IDbAsyncEnumerator.Current
            {
                get { return _shaper.RootCoordinator.Current; }
            }

#endif

            public void Dispose()
            {
                // Technically, calling GC.SuppressFinalize is not required because the class does not
                // have a finalizer, but it does no harm, protects against the case where a finalizer is added
                // in the future, and prevents an FxCop warning.
                GC.SuppressFinalize(this);
                // For backwards compatibility, we set the current value to the
                // default value, so you can still call Current.
                _shaper.RootCoordinator.SetCurrentToDefault();
                _shaper.Finally();
            }

            public bool MoveNext()
            {
                if (!_shaper._isActive)
                {
                    return false;
                }
                if (_shaper.StoreRead())
                {
                    try
                    {
                        _shaper.StartMaterializingElement();
                        _shaper.RootCoordinator.ReadNextElement(_shaper);
                    }
                    finally
                    {
                        _shaper.StopMaterializingElement();
                    }
                    return true;
                }
                Dispose();
                return false;
            }

#if !NET40

            public async Task<bool> MoveNextAsync(CancellationToken cancellationToken)
            {
                if (!_shaper._isActive)
                {
                    return false;
                }
                if (await _shaper.StoreReadAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    try
                    {
                        _shaper.StartMaterializingElement();
                        _shaper.RootCoordinator.ReadNextElement(_shaper);
                    }
                    finally
                    {
                        _shaper.StopMaterializingElement();
                    }
                    return true;
                }
                Dispose();
                return false;
            }

#endif

            public void Reset()
            {
                throw new NotSupportedException();
            }
        }

        #endregion

        #region Nested Enumerator

        /// <summary>
        /// Enumerates (for each row in the input) an array of all coordinators producing new elements. The array
        /// contains a position for each 'depth' in the result. A null value in any position indicates that no new
        /// results were produced for the given row at the given depth. It is possible for a row to contain no
        /// results for any row.
        /// </summary>
        private class RowNestedResultEnumerator : IDbEnumerator<Coordinator[]>
        {
            private readonly Shaper<T> _shaper;
            private readonly Coordinator[] _current;

            internal RowNestedResultEnumerator(Shaper<T> shaper)
            {
                _shaper = shaper;
                _current = new Coordinator[_shaper.RootCoordinator.MaxDistanceToLeaf() + 1];
            }

            public Coordinator[] Current
            {
                get { return _current; }
            }

            object IEnumerator.Current
            {
                get { return _current; }
            }

#if !NET40

            object IDbAsyncEnumerator.Current
            {
                get { return _current; }
            }

#endif

            public void Dispose()
            {
                // Technically, calling GC.SuppressFinalize is not required because the class does not
                // have a finalizer, but it does no harm, protects against the case where a finalizer is added
                // in the future, and prevents an FxCop warning.
                GC.SuppressFinalize(this);
                _shaper.Finally();
            }

            public bool MoveNext()
            {
                try
                {
                    _shaper.StartMaterializingElement();

                    if (!_shaper.StoreRead())
                    {
                        // Reset all collections
                        RootCoordinator.ResetCollection(_shaper);
                        return false;
                    }

                    MaterializeRow();
                }
                finally
                {
                    _shaper.StopMaterializingElement();
                }

                return true;
            }

#if !NET40

            public async Task<bool> MoveNextAsync(CancellationToken cancellationToken)
            {
                try
                {
                    _shaper.StartMaterializingElement();

                    if (!await _shaper.StoreReadAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                    {
                        // Reset all collections
                        RootCoordinator.ResetCollection(_shaper);
                        return false;
                    }

                    MaterializeRow();
                }
                finally
                {
                    _shaper.StopMaterializingElement();
                }

                return true;
            }

#endif

            private void MaterializeRow()
            {
                Coordinator currentCoordinator = _shaper.RootCoordinator;

                var depth = 0;
                var haveInitializedChildren = false;
                for (; depth < _current.Length; depth++)
                {
                    // find a coordinator at this depth that currently has data (if any)
                    while (currentCoordinator != null
                           && !currentCoordinator.CoordinatorFactory.HasData(_shaper))
                    {
                        currentCoordinator = currentCoordinator.Next;
                    }
                    if (null == currentCoordinator)
                    {
                        break;
                    }

                    // check if this row contains a new element for this coordinator
                    if (currentCoordinator.HasNextElement(_shaper))
                    {
                        // if we have children and haven't initialized them yet, do so now
                        if (!haveInitializedChildren
                            && null != currentCoordinator.Child)
                        {
                            currentCoordinator.Child.ResetCollection(_shaper);
                        }
                        haveInitializedChildren = true;

                        // read the next element
                        currentCoordinator.ReadNextElement(_shaper);

                        // place the coordinator in the result array to indicate there is a new
                        // element at this depth
                        _current[depth] = currentCoordinator;
                    }
                    else
                    {
                        // clear out the coordinator in result array to indicate there is no new
                        // element at this depth
                        _current[depth] = null;
                    }

                    // move to child (in the next iteration we deal with depth + 1
                    currentCoordinator = currentCoordinator.Child;
                }

                // clear out all positions below the depth we reached before we ran out of data
                for (; depth < _current.Length; depth++)
                {
                    _current[depth] = null;
                }
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            internal Coordinator<T> RootCoordinator
            {
                get { return _shaper.RootCoordinator; }
            }
        }

        /// <summary>
        /// Wraps RowNestedResultEnumerator and yields results appropriate to an ObjectQuery instance. In particular,
        /// root level elements (T) are returned only after aggregating all child elements.
        /// </summary>
        private class ObjectQueryNestedEnumerator : IDbEnumerator<T>
        {
            private readonly RowNestedResultEnumerator _rowEnumerator;
            private T _previousElement;
            private State _state;

            internal ObjectQueryNestedEnumerator(RowNestedResultEnumerator rowEnumerator)
            {
                _rowEnumerator = rowEnumerator;
                _previousElement = default(T);
                _state = State.Start;
            }

            public T Current
            {
                get { return _previousElement; }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

#if !NET40

            object IDbAsyncEnumerator.Current
            {
                get { return Current; }
            }

#endif

            public void Dispose()
            {
                // Technically, calling GC.SuppressFinalize is not required because the class does not
                // have a finalizer, but it does no harm, protects against the case where a finalizer is added
                // in the future, and prevents an FxCop warning.
                GC.SuppressFinalize(this);
                _rowEnumerator.Dispose();
            }

            public bool MoveNext()
            {
                // See the documentation for enum State to understand the behaviors and requirements
                // for each state.
                switch (_state)
                {
                    case State.Start:
                        if (TryReadToNextElement())
                        {
                            // if there's an element in the reader...
                            ReadElement();
                        }
                        else
                        {
                            // no data at all...
                            _state = State.NoRows;
                        }
                        break;
                    case State.Reading:
                        ReadElement();
                        break;
                    case State.NoRowsLastElementPending:
                        // nothing to do but move to the next state...
                        _state = State.NoRows;
                        break;
                }

                bool result;
                if (_state == State.NoRows)
                {
                    _previousElement = default(T);
                    result = false;
                }
                else
                {
                    result = true;
                }

                return result;
            }

#if !NET40

            public async Task<bool> MoveNextAsync(CancellationToken cancellationToken)
            {
                // See the documentation for enum State to understand the behaviors and requirements
                // for each state.
                switch (_state)
                {
                    case State.Start:
                        if (await TryReadToNextElementAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                        {
                            // if there's an element in the reader...
                            await ReadElementAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                        }
                        else
                        {
                            // no data at all...
                            _state = State.NoRows;
                        }
                        break;
                    case State.Reading:
                        await ReadElementAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                        break;
                    case State.NoRowsLastElementPending:
                        // nothing to do but move to the next state...
                        _state = State.NoRows;
                        break;
                }

                bool result;
                if (_state == State.NoRows)
                {
                    _previousElement = default(T);
                    result = false;
                }
                else
                {
                    result = true;
                }

                return result;
            }

#endif

            /// <summary>
            /// Requires: the row is currently positioned at the start of an element.
            /// Reads all rows in the element and sets up state for the next element (if any).
            /// </summary>
            private void ReadElement()
            {
                // remember the element we're currently reading
                _previousElement = _rowEnumerator.RootCoordinator.Current;

                // now we need to read to the next element (or the end of the
                // reader) so that we can return the first element
                if (TryReadToNextElement())
                {
                    // we're positioned at the start of the next element (which
                    // corresponds to the 'reading' state)
                    _state = State.Reading;
                }
                else
                {
                    // we're positioned at the end of the reader
                    _state = State.NoRowsLastElementPending;
                }
            }

#if !NET40

            private async Task ReadElementAsync(CancellationToken cancellationToken)
            {
                // remember the element we're currently reading
                _previousElement = _rowEnumerator.RootCoordinator.Current;

                // now we need to read to the next element (or the end of the
                // reader) so that we can return the first element
                if (await TryReadToNextElementAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    // we're positioned at the start of the next element (which
                    // corresponds to the 'reading' state)
                    _state = State.Reading;
                }
                else
                {
                    // we're positioned at the end of the reader
                    _state = State.NoRowsLastElementPending;
                }
            }

#endif

            /// <summary>
            /// Reads rows until the start of a new element is found. If no element
            /// is found before all rows are consumed, returns false.
            /// </summary>
            private bool TryReadToNextElement()
            {
                while (_rowEnumerator.MoveNext())
                {
                    // if we hit a new element, return true
                    if (_rowEnumerator.Current[0] != null)
                    {
                        return true;
                    }
                }
                return false;
            }

#if !NET40

            private async Task<bool> TryReadToNextElementAsync(CancellationToken cancellationToken)
            {
                while (await _rowEnumerator.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    // if we hit a new element, return true
                    if (_rowEnumerator.Current[0] != null)
                    {
                        return true;
                    }
                }
                return false;
            }

#endif

            public void Reset()
            {
                _rowEnumerator.Reset();
            }

            /// <summary>
            /// Describes the state of this enumerator with respect to the _rowEnumerator
            /// it wraps.
            /// </summary>
            private enum State
            {
                /// <summary>
                /// No rows have been read yet
                /// </summary>
                Start,

                /// <summary>
                /// Positioned at the start of a new root element. The previous element must
                /// be stored in _previousElement. We read ahead in this manner so that
                /// the previous element is fully populated (all of its children loaded)
                /// before returning.
                /// </summary>
                Reading,

                /// <summary>
                /// Positioned past the end of the rows. The last element in the enumeration
                /// has not yet been returned to the user however, and is stored in _previousElement.
                /// </summary>
                NoRowsLastElementPending,

                /// <summary>
                /// Positioned past the end of the rows. The last element has been returned to
                /// the user.
                /// </summary>
                NoRows,
            }
        }

        /// <summary>
        /// Wraps RowNestedResultEnumerator and yields results appropriate to an EntityReader instance. In particular,
        /// yields RecordState whenever a new element becomes available at any depth in the result hierarchy.
        /// </summary>
        private class RecordStateEnumerator : IDbEnumerator<RecordState>
        {
            private readonly RowNestedResultEnumerator _rowEnumerator;
            private RecordState _current;

            /// <summary>
            /// Gets depth of coordinator we're currently consuming. If _depth == -1, it means we haven't started
            /// to consume the next row yet.
            /// </summary>
            private int _depth;

            private bool _readerConsumed;

            internal RecordStateEnumerator(RowNestedResultEnumerator rowEnumerator)
            {
                _rowEnumerator = rowEnumerator;
                _current = null;
                _depth = -1;
                _readerConsumed = false;
            }

            public RecordState Current
            {
                get { return _current; }
            }

            object IEnumerator.Current
            {
                get { return _current; }
            }

#if !NET40

            object IDbAsyncEnumerator.Current
            {
                get { return _current; }
            }

#endif

            public void Dispose()
            {
                // Technically, calling GC.SuppressFinalize is not required because the class does not
                // have a finalizer, but it does no harm, protects against the case where a finalizer is added
                // in the future, and prevents an FxCop warning.
                GC.SuppressFinalize(this);
                _rowEnumerator.Dispose();
            }

            public bool MoveNext()
            {
                if (!_readerConsumed)
                {
                    while (true)
                    {
                        // keep on cycling until we find a result
                        if (-1 == _depth
                            || _rowEnumerator.Current.Length == _depth)
                        {
                            // time to move to the next row...
                            if (!_rowEnumerator.MoveNext())
                            {
                                // no more rows...
                                _current = null;
                                _readerConsumed = true;
                                break;
                            }

                            _depth = 0;
                        }

                        // check for results at the current depth
                        var currentCoordinator = _rowEnumerator.Current[_depth];
                        if (null != currentCoordinator)
                        {
                            _current = ((Coordinator<RecordState>)currentCoordinator).Current;
                            _depth++;
                            break;
                        }

                        _depth++;
                    }
                }

                return !_readerConsumed;
            }

#if !NET40

            public async Task<bool> MoveNextAsync(CancellationToken cancellationToken)
            {
                if (!_readerConsumed)
                {
                    while (true)
                    {
                        // keep on cycling until we find a result
                        if (-1 == _depth
                            || _rowEnumerator.Current.Length == _depth)
                        {
                            // time to move to the next row...
                            if (!await _rowEnumerator.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                            {
                                // no more rows...
                                _current = null;
                                _readerConsumed = true;
                                break;
                            }

                            _depth = 0;
                        }

                        // check for results at the current depth
                        var currentCoordinator = _rowEnumerator.Current[_depth];
                        if (null != currentCoordinator)
                        {
                            _current = ((Coordinator<RecordState>)currentCoordinator).Current;
                            _depth++;
                            break;
                        }

                        _depth++;
                    }
                }

                return !_readerConsumed;
            }

#endif

            public void Reset()
            {
                _rowEnumerator.Reset();
            }
        }

        #endregion
    }
}
