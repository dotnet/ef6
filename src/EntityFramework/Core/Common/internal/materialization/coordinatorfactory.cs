namespace System.Data.Entity.Core.Common.Internal.Materialization
{
    using System.Collections.ObjectModel;

    /// <summary>
    /// An immutable class used to generate new coordinators. These coordinators are used
    /// at runtime to materialize results.
    /// </summary>
    internal abstract class CoordinatorFactory
    {
        #region Private Static Fields

        /// <summary>
        /// Function of shaper that returns true; one default case when there is no explicit predicate.
        /// </summary>
        private static readonly Func<Shaper, bool> _alwaysTrue = s => true;

        /// <summary>
        /// Function of shaper that returns false; one default case used when there is no explicit predicate.
        /// </summary>
        private static readonly Func<Shaper, bool> _alwaysFalse = s => false;

        #endregion

        #region "Public" Fields

        /// <summary>
        /// Gets depth of the reader (0 is top-level -- which incidentally doesn't
        /// require a coordinator...
        /// </summary>
        internal readonly int Depth;

        /// <summary>
        /// Indicates which state slot in the Shaper.State is expected to hold the
        /// value for this nested reader result.
        /// </summary>
        internal readonly int StateSlot;

        /// <summary>
        /// A function determining whether the current row has data for this nested result.
        /// </summary>
        internal readonly Func<Shaper, bool> HasData;

        /// <summary>
        /// A function setting key values. (the return value is irrelevant)
        /// </summary>
        internal readonly Func<Shaper, bool> SetKeys;

        /// <summary>
        /// A function returning true if key values match the previously set values.
        /// </summary>
        internal readonly Func<Shaper, bool> CheckKeys;

        /// <summary>
        /// Nested results below this (at depth + 1)
        /// </summary>
        internal readonly ReadOnlyCollection<CoordinatorFactory> NestedCoordinators;

        /// <summary>
        /// Indicates whether this is a leaf reader.
        /// </summary>
        internal readonly bool IsLeafResult;

        /// <summary>
        /// Indicates whether this coordinator can be managed by a simple enumerator. A simple enumerator
        /// returns a single element per row, so the following conditions disqualify the enumerator:
        /// nested collections, data discriminators (not all rows have data), keys (not all rows have new data).
        /// </summary>
        internal readonly bool IsSimple;

        /// <summary>
        /// For value-layer queries, the factories for all the records that we can potentially process
        /// at this level in the query result.
        /// </summary>
        internal readonly ReadOnlyCollection<RecordStateFactory> RecordStateFactories;

        #endregion

        #region Constructor

        protected CoordinatorFactory(
            int depth, int stateSlot, Func<Shaper, bool> hasData, Func<Shaper, bool> setKeys, Func<Shaper, bool> checkKeys,
            CoordinatorFactory[] nestedCoordinators, RecordStateFactory[] recordStateFactories)
        {
            Depth = depth;
            StateSlot = stateSlot;

            // figure out if there are any nested coordinators
            IsLeafResult = 0 == nestedCoordinators.Length;

            // if there is no explicit 'has data' discriminator, it means all rows contain data for the coordinator
            if (hasData == null)
            {
                HasData = _alwaysTrue;
            }
            else
            {
                HasData = hasData;
            }

            // if there is no explicit set key delegate, just return true (the value is not used anyways)
            if (setKeys == null)
            {
                SetKeys = _alwaysTrue;
            }
            else
            {
                SetKeys = setKeys;
            }

            // If there are no keys, it means different things depending on whether we are a leaf
            // coordinator or an inner (or 'driving') coordinator. For a leaf coordinator, it means
            // that every row is a new result. For an inner coordinator, it means that there is no
            // key to check. This should only occur where there is a SingleRowTable (in other words,
            // all rows are elements of a single child collection).
            if (checkKeys == null)
            {
                if (IsLeafResult)
                {
                    CheckKeys = _alwaysFalse; // every row is a new result (the keys don't match)
                }
                else
                {
                    CheckKeys = _alwaysTrue; // every row belongs to a single child collection
                }
            }
            else
            {
                CheckKeys = checkKeys;
            }
            NestedCoordinators = new ReadOnlyCollection<CoordinatorFactory>(nestedCoordinators);
            RecordStateFactories = new ReadOnlyCollection<RecordStateFactory>(recordStateFactories);

            // Determines whether this coordinator can be handled by a 'simple' enumerator. See IsSimple for details.
            IsSimple = IsLeafResult && null == checkKeys && null == hasData;
        }

        #endregion

        #region "Public" Surface Area

        /// <summary>
        /// Creates a buffer handling state needed by this coordinator.
        /// </summary>
        internal abstract Coordinator CreateCoordinator(Coordinator parent, Coordinator next);

        #endregion
    }
}
