namespace System.Data.Entity.Core.Common.Internal.Materialization
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Objects.Internal;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Text;

    /// <summary>
    /// An immutable class used to generate new coordinators. These coordinators are used
    /// at runtime to materialize results.
    /// </summary>
    internal abstract class CoordinatorFactory
    {
        #region statics

        /// <summary>
        /// Function of shaper that returns true; one default case when there is no explicit predicate.
        /// </summary>
        private static readonly Func<Shaper, bool> _alwaysTrue = s => true;

        /// <summary>
        /// Function of shaper that returns false; one default case used when there is no explicit predicate.
        /// </summary>
        private static readonly Func<Shaper, bool> _alwaysFalse = s => false;

        #endregion

        #region state

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

        #region constructor

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

        #region "public" surface area

        /// <summary>
        /// Creates a buffer handling state needed by this coordinator.
        /// </summary>
        internal abstract Coordinator CreateCoordinator(Coordinator parent, Coordinator next);

        #endregion
    }

    /// <summary>
    /// Typed <see cref="CoordinatorFactory"/>
    /// </summary>
    internal sealed class CoordinatorFactory<TElement> : CoordinatorFactory
    {
        #region state

        /// <summary>
        /// Reads a single element of the result from the given reader state object, returning the
        /// result as a wrapped entity.  May be null if the element is not available as a wrapped entity.
        /// </summary>
        internal readonly Func<Shaper, IEntityWrapper> WrappedElement;

        /// <summary>
        /// Reads a single element of the result from the given reader state object.
        /// May be null if the element is available as a wrapped entity instead.
        /// </summary>
        internal readonly Func<Shaper, TElement> Element;

        /// <summary>
        /// Same as Element but uses slower patterns to provide better exception messages (e.g.
        /// using reader.GetValue + type check rather than reader.GetInt32)
        /// </summary>
        internal readonly Func<Shaper, TElement> ElementWithErrorHandling;

        /// <summary>
        /// Initializes the collection storing results from this coordinator.
        /// </summary>
        internal readonly Func<Shaper, ICollection<TElement>> InitializeCollection;

        /// <summary>
        /// Description of this CoordinatorFactory, used for debugging only; while this is not  
        /// needed in retail code, it is pretty important because it's the only description we'll 
        /// have once we compile the Expressions; debugging a problem with retail bits would be 
        /// pretty hard without this.
        /// </summary>
        private readonly string Description;

        #endregion

        #region constructor

        public CoordinatorFactory(
            int depth, int stateSlot, Expression hasData, Expression setKeys, Expression checkKeys, CoordinatorFactory[] nestedCoordinators,
            Expression element, Expression elementWithErrorHandling, Expression initializeCollection,
            RecordStateFactory[] recordStateFactories)
            : base(
                depth, stateSlot, CompilePredicate(hasData), CompilePredicate(setKeys), CompilePredicate(checkKeys), nestedCoordinators,
                recordStateFactories)
        {
            // If we are in a case where a wrapped entity is available, then use it; otherwise use the raw element.
            // However, in both cases, use the raw element for the error handling case where what we care about is
            // getting the appropriate exception message.
            if (typeof(IEntityWrapper).IsAssignableFrom(element.Type))
            {
                WrappedElement = Translator.Compile<IEntityWrapper>(element);
                elementWithErrorHandling = Translator.Emit_UnwrapAndEnsureType(elementWithErrorHandling, typeof(TElement));
            }
            else
            {
                Element = Translator.Compile<TElement>(element);
            }
            ElementWithErrorHandling = Translator.Compile<TElement>(elementWithErrorHandling);
            InitializeCollection = null == initializeCollection
                                       ? s => new List<TElement>()
                                       : Translator.Compile<ICollection<TElement>>(initializeCollection);

            Description = new StringBuilder()
                .Append("HasData: ")
                .AppendLine(DescribeExpression(hasData))
                .Append("SetKeys: ")
                .AppendLine(DescribeExpression(setKeys))
                .Append("CheckKeys: ")
                .AppendLine(DescribeExpression(checkKeys))
                .Append("Element: ")
                .AppendLine(DescribeExpression(element))
                .Append("ElementWithExceptionHandling: ")
                .AppendLine(DescribeExpression(elementWithErrorHandling))
                .Append("InitializeCollection: ")
                .AppendLine(DescribeExpression(initializeCollection))
                .ToString();
        }

        #endregion

        #region expression helpers

        /// <summary>
        /// Return the compiled expression for the predicate
        /// </summary>
        private static Func<Shaper, bool> CompilePredicate(Expression predicate)
        {
            Func<Shaper, bool> result;
            if (null == predicate)
            {
                result = null;
            }
            else
            {
                result = Translator.Compile<bool>(predicate);
            }
            return result;
        }

        /// <summary>
        /// Returns a string representation of the expression
        /// </summary>
        private static string DescribeExpression(Expression expression)
        {
            string result;
            if (null == expression)
            {
                result = "undefined";
            }
            else
            {
                result = expression.ToString();
            }
            return result;
        }

        #endregion

        #region "public" surface area

        /// <summary>
        /// Create a coordinator used for materialization of collections. Unlike the CoordinatorFactory,
        /// the Coordinator contains mutable state.
        /// </summary>
        internal override Coordinator CreateCoordinator(Coordinator parent, Coordinator next)
        {
            return new Coordinator<TElement>(this, parent, next);
        }

        /// <summary>
        /// Returns the "default" record state (that is, the one we use for PreRead/PastEnd reader states
        /// </summary>
        internal RecordState GetDefaultRecordState(Shaper<RecordState> shaper)
        {
            RecordState result = null;
            if (RecordStateFactories.Count > 0)
            {
                // CONSIDER(SteveSta): We're relying upon having the default for polymorphic types be 
                // the first item in the list; that sounds kind of risky.
                result = (RecordState)shaper.State[RecordStateFactories[0].StateSlotNumber];
                Debug.Assert(null != result, "did you initialize the record states?");
                result.ResetToDefaultState();
            }
            return result;
        }

        public override string ToString()
        {
            return Description;
        }

        #endregion
    }
}
