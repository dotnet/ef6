// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Internal.Materialization
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Objects.Internal;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    /// <summary>
    ///     Typed <see cref="CoordinatorFactory" />
    /// </summary>
    internal class CoordinatorFactory<TElement> : CoordinatorFactory
    {
        #region State

        /// <summary>
        ///     Reads a single element of the result from the given reader state object, returning the
        ///     result as a wrapped entity.  May be null if the element is not available as a wrapped entity.
        /// </summary>
        internal readonly Func<Shaper, IEntityWrapper> WrappedElement;

        /// <summary>
        ///     Reads a single element of the result from the given reader state object.
        ///     May be null if the element is available as a wrapped entity instead.
        /// </summary>
        internal readonly Func<Shaper, TElement> Element;

        /// <summary>
        ///     Same as Element but uses slower patterns to provide better exception messages (e.g.
        ///     using reader.GetValue + type check rather than reader.GetInt32)
        /// </summary>
        internal readonly Func<Shaper, TElement> ElementWithErrorHandling;

        /// <summary>
        ///     Initializes the collection storing results from this coordinator.
        /// </summary>
        internal readonly Func<Shaper, ICollection<TElement>> InitializeCollection;

        /// <summary>
        ///     Description of this CoordinatorFactory, used for debugging only; while this is not
        ///     needed in retail code, it is pretty important because it's the only description we'll
        ///     have once we compile the Expressions; debugging a problem with retail bits would be
        ///     pretty hard without this.
        /// </summary>
        private readonly string Description;

        #endregion

        #region Constructors

        /// <summary>
        ///     Used for testing.
        /// </summary>
        /// <param name="depth"> </param>
        /// <param name="stateSlot"> </param>
        /// <param name="hasData"> Can be null. </param>
        /// <param name="setKeys"> Can be null. </param>
        /// <param name="checkKeys"> Can be null. </param>
        /// <param name="nestedCoordinators"> </param>
        /// <param name="element">
        ///     Supply null if <paramref name="wrappedElement" /> isn't null.
        /// </param>
        /// <param name="wrappedElement">
        ///     Supply null if <paramref name="element" /> isn't null.
        /// </param>
        /// <param name="elementWithErrorHandling"> Should return the unwrapped entity. </param>
        /// <param name="initializeCollection"> Can be null. </param>
        /// <param name="recordStateFactories"> </param>
        internal CoordinatorFactory(
            int depth,
            int stateSlot,
            Expression<Func<Shaper, bool>> hasData,
            Expression<Func<Shaper, bool>> setKeys,
            Expression<Func<Shaper, bool>> checkKeys,
            CoordinatorFactory[] nestedCoordinators,
            Expression<Func<Shaper, TElement>> element,
            Expression<Func<Shaper, IEntityWrapper>> wrappedElement,
            Expression<Func<Shaper, TElement>> elementWithErrorHandling,
            Expression<Func<Shaper, ICollection<TElement>>> initializeCollection,
            RecordStateFactory[] recordStateFactories)
            : base(
                depth,
                stateSlot,
                CompilePredicate(hasData),
                CompilePredicate(setKeys),
                CompilePredicate(checkKeys),
                nestedCoordinators,
                recordStateFactories)
        {
            Debug.Assert(depth >= 0);
            Debug.Assert(stateSlot >= 0);
            DebugCheck.NotNull(nestedCoordinators);
            DebugCheck.NotNull(recordStateFactories);
            DebugCheck.NotNull(elementWithErrorHandling);

            Debug.Assert((element == null) != (wrappedElement == null));

            // If we are in a case where a wrapped entity is available, then use it; otherwise use the raw element.
            // However, in both cases, use the raw element for the error handling case where what we care about is
            // getting the appropriate exception message.

            WrappedElement = wrappedElement == null ? null : wrappedElement.Compile();
            Element = element == null ? null : element.Compile();
            ElementWithErrorHandling = elementWithErrorHandling.Compile();
            InitializeCollection = null == initializeCollection
                                       ? s => new List<TElement>()
                                       : initializeCollection.Compile();

            Description = new StringBuilder()
                .Append("HasData: ")
                .AppendLine(DescribeExpression(hasData))
                .Append("SetKeys: ")
                .AppendLine(DescribeExpression(setKeys))
                .Append("CheckKeys: ")
                .AppendLine(DescribeExpression(checkKeys))
                .Append("Element: ")
                .AppendLine(element == null ? DescribeExpression(wrappedElement) : DescribeExpression(element))
                .Append("ElementWithExceptionHandling: ")
                .AppendLine(DescribeExpression(elementWithErrorHandling))
                .Append("InitializeCollection: ")
                .AppendLine(DescribeExpression(initializeCollection))
                .ToString();
        }

        // Asserts MemberAccess to skip visbility check.  
        // This means that that security checks are skipped. Before calling this
        // method you must ensure that you've done a TestComple on expressions provided
        // by the user to ensure the compilation doesn't violate them.
        //[SuppressMessage("Microsoft.Security", "CA2128")]
        [SecuritySafeCritical]
        [ReflectionPermission(SecurityAction.Assert, MemberAccess = true)]
        public CoordinatorFactory(
            int depth,
            int stateSlot,
            Expression hasData,
            Expression setKeys,
            Expression checkKeys,
            CoordinatorFactory[] nestedCoordinators,
            Expression element,
            Expression elementWithErrorHandling,
            Expression initializeCollection,
            RecordStateFactory[] recordStateFactories)
            : this(
                depth,
                stateSlot,
                CodeGenEmitter.BuildShaperLambda<bool>(hasData),
                CodeGenEmitter.BuildShaperLambda<bool>(setKeys),
                CodeGenEmitter.BuildShaperLambda<bool>(checkKeys),
                nestedCoordinators,
                typeof(IEntityWrapper).IsAssignableFrom(element.Type)
                    ? null
                    : CodeGenEmitter.BuildShaperLambda<TElement>(element),
                typeof(IEntityWrapper).IsAssignableFrom(element.Type)
                    ? CodeGenEmitter.BuildShaperLambda<IEntityWrapper>(element)
                    : null,
                CodeGenEmitter.BuildShaperLambda<TElement>(
                    typeof(IEntityWrapper).IsAssignableFrom(element.Type)
                        ? CodeGenEmitter.Emit_UnwrapAndEnsureType(elementWithErrorHandling, typeof(TElement))
                        : elementWithErrorHandling),
                CodeGenEmitter.BuildShaperLambda<ICollection<TElement>>(initializeCollection),
                recordStateFactories)
        {
        }

        #endregion

        #region Expression Helpers

        /// <summary>
        ///     Return the compiled expression for the predicate
        /// </summary>
        private static Func<Shaper, bool> CompilePredicate(Expression<Func<Shaper, bool>> predicate)
        {
            Func<Shaper, bool> result;
            if (null == predicate)
            {
                result = null;
            }
            else
            {
                result = predicate.Compile();
            }
            return result;
        }

        /// <summary>
        ///     Returns a string representation of the expression
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

        #region "Public" Surface Area

        /// <summary>
        ///     Create a coordinator used for materialization of collections. Unlike the CoordinatorFactory,
        ///     the Coordinator contains mutable state.
        /// </summary>
        internal override Coordinator CreateCoordinator(Coordinator parent, Coordinator next)
        {
            return new Coordinator<TElement>(this, parent, next);
        }

        /// <summary>
        ///     Returns the "default" record state (that is, the one we use for PreRead/PastEnd reader states
        /// </summary>
        internal RecordState GetDefaultRecordState(Shaper<RecordState> shaper)
        {
            RecordState result = null;
            if (RecordStateFactories.Count > 0)
            {
                // CONSIDER: We're relying upon having the default for polymorphic types be 
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
