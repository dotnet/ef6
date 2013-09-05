// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// An instance of a class derived from ObjectQueryState is used to model every instance of
    /// <see
    ///     cref="ObjectQuery{TResultType}" />
    /// .
    /// A different ObjectQueryState-derived class is used depending on whether the ObjectQuery is an Entity SQL,
    /// Linq to Entities, or compiled Linq to Entities query.
    /// </summary>
    internal abstract class ObjectQueryState
    {
        /// <summary>
        /// The <see cref="MergeOption" /> that should be used in the absence of an explicitly specified
        /// or user-specified merge option or a merge option inferred from the query definition itself.
        /// </summary>
        internal static readonly MergeOption DefaultMergeOption = MergeOption.AppendOnly;

        /// <summary>
        /// Generic MethodInfo used in the non-generic CreateQuery
        /// </summary>
        internal static readonly MethodInfo CreateObjectQueryMethod = typeof(ObjectQueryState).GetDeclaredMethod("CreateObjectQuery");

        /// <summary>
        /// The context of the ObjectQuery
        /// </summary>
        private readonly ObjectContext _context;

        /// <summary>
        /// The element type of this query, as a CLR type
        /// </summary>
        private readonly Type _elementType;

        /// <summary>
        /// The collection of parameters associated with the ObjectQuery
        /// </summary>
        private ObjectParameterCollection _parameters;

        /// <summary>
        /// The full-span specification
        /// </summary>
        private readonly Span _span;

        /// <summary>
        /// The user-specified default merge option
        /// </summary>
        private MergeOption? _userMergeOption;

        /// <summary>
        /// Indicates whether query caching is enabled for the implemented ObjectQuery.
        /// </summary>
        private bool _cachingEnabled = true;

        /// <summary>
        /// Optionally used by derived classes to record the most recently used <see cref="ObjectQueryExecutionPlan" />.
        /// </summary>
        protected ObjectQueryExecutionPlan _cachedPlan;

        /// <summary>
        /// Constructs a new <see cref="ObjectQueryState" /> instance that uses the specified context and parameters collection.
        /// </summary>
        /// <param name="elementType"> </param>
        /// <param name="context"> The ObjectContext to which the implemented ObjectQuery belongs </param>
        /// <param name="parameters"> </param>
        /// <param name="span"> </param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1614:ElementParameterDocumentationMustHaveText")]
        protected ObjectQueryState(Type elementType, ObjectContext context, ObjectParameterCollection parameters, Span span)
        {
            // Validate the element type
            DebugCheck.NotNull(elementType);

            // Validate the context
            DebugCheck.NotNull(context);

            // Parameters and Span are specifically allowed to be null

            _elementType = elementType;
            _context = context;
            _span = span;
            _parameters = parameters;
        }

        /// <summary>
        /// Constructs a new <see cref="ObjectQueryState" /> copying the state information from the specified
        /// <see cref="ObjectQuery" />.
        /// </summary>
        /// <param name="elementType"> The element type of the implemented ObjectQuery, as a CLR type. </param>
        /// <param name="query"> The ObjectQuery from which the state should be copied. </param>
        protected ObjectQueryState(Type elementType, ObjectQuery query)
            : this(elementType, query.Context, null, null)
        {
            _cachingEnabled = query.EnablePlanCaching;
            UserSpecifiedStreamingBehaviour = query.QueryState.UserSpecifiedStreamingBehaviour;
        }

        internal bool EffectiveStreamingBehaviour
        {
            get { return UserSpecifiedStreamingBehaviour ?? false; }
        }

        internal bool? UserSpecifiedStreamingBehaviour { get; set; }

        /// <summary>
        /// Gets the element type - the type of each result item - for this query as a CLR type instance.
        /// </summary>
        internal Type ElementType
        {
            get { return _elementType; }
        }

        /// <summary>
        /// Gets the ObjectContext with which the implemented ObjectQuery is associated
        /// </summary>
        internal ObjectContext ObjectContext
        {
            get { return _context; }
        }

        /// <summary>
        /// Gets the collection of parameters associated with the implemented ObjectQuery. May be null.
        /// Call <see cref="EnsureParameters" /> if a guaranteed non-null collection is required.
        /// </summary>
        internal ObjectParameterCollection Parameters
        {
            get { return _parameters; }
        }

        internal ObjectParameterCollection EnsureParameters()
        {
            if (_parameters == null)
            {
                _parameters = new ObjectParameterCollection(ObjectContext.Perspective);
                if (_cachedPlan != null)
                {
                    _parameters.SetReadOnly(true);
                }
            }

            return _parameters;
        }

        /// <summary>
        /// Gets the Span specification associated with the implemented ObjectQuery. May be null.
        /// </summary>
        internal Span Span
        {
            get { return _span; }
        }

        /// <summary>
        /// The merge option that this query considers currently 'in effect'. This may be a merge option set via the ObjectQuery.MergeOption
        /// property, or the merge option that applies to the currently cached execution plan, if any, or the global default merge option.
        /// </summary>
        internal MergeOption EffectiveMergeOption
        {
            get
            {
                if (_userMergeOption.HasValue)
                {
                    return _userMergeOption.Value;
                }

                var plan = _cachedPlan;
                if (plan != null)
                {
                    return plan.MergeOption;
                }

                return DefaultMergeOption;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating which <see cref="MergeOption" /> should be used when preparing this query for execution via
        /// <see cref="GetExecutionPlan(MergeOption?)" /> if no option is explicitly specified - for example during foreach-style enumeration.
        /// <see cref="ObjectQuery.MergeOption" /> sets this property on its underlying query state instance.
        /// </summary>
        internal MergeOption? UserSpecifiedMergeOption
        {
            get { return _userMergeOption; }
            set { _userMergeOption = value; }
        }

        /// <summary>
        /// Gets or sets a user-defined value indicating whether or not query caching is enabled for the implemented ObjectQuery.
        /// </summary>
        internal bool PlanCachingEnabled
        {
            get { return _cachingEnabled; }
            set { _cachingEnabled = value; }
        }

        /// <summary>
        /// Gets the result type - not just the element type - for this query as an EDM Type usage instance.
        /// </summary>
        internal TypeUsage ResultType
        {
            get
            {
                var plan = _cachedPlan;
                if (plan != null)
                {
                    return plan.ResultType;
                }
                else
                {
                    return GetResultType();
                }
            }
        }

        /// <summary>
        /// Sets the values the <see cref="PlanCachingEnabled" /> and <see cref="UserSpecifiedMergeOption" /> properties on
        /// <paramref name="other" /> to match the values of the corresponding properties on this instance.
        /// </summary>
        /// <param name="other"> The query state to which this instances settings should be applied. </param>
        internal void ApplySettingsTo(ObjectQueryState other)
        {
            other.PlanCachingEnabled = PlanCachingEnabled;
            other.UserSpecifiedMergeOption = UserSpecifiedMergeOption;

            // _cachedPlan is intentionally not copied over - since the parameters of 'other' would have to be locked as
            // soon as its execution plan was set, and that may not be appropriate at the time ApplySettingsTo is called. 
        }

        /// <summary>
        /// Must return <c>true</c> and set <paramref name="commandText" /> to a valid value
        /// if command text is available for this query; must return <c>false</c> otherwise.
        /// Implementations of this method must not throw exceptions.
        /// </summary>
        /// <param name="commandText"> The command text of this query, if available. </param>
        /// <returns>
        /// <c>true</c> if command text is available for this query and was successfully retrieved; otherwise <c>false</c> .
        /// </returns>
        internal abstract bool TryGetCommandText(out string commandText);

        /// <summary>
        /// Must return <c>true</c> and set <paramref name="expression" /> to a valid value if a
        /// LINQ Expression is available for this query; must return <c>false</c> otherwise.
        /// Implementations of this method must not throw exceptions.
        /// </summary>
        /// <param name="expression"> The LINQ Expression that defines this query, if available. </param>
        /// <returns>
        /// <c>true</c> if an Expression is available for this query and was successfully retrieved; otherwise <c>false</c> .
        /// </returns>
        internal abstract bool TryGetExpression(out Expression expression);

        /// <summary>
        /// Retrieves an <see cref="ObjectQueryExecutionPlan" /> that can be used to retrieve the results of this query using the specified merge option.
        /// If <paramref name="forMergeOption" /> is null, an appropriate default value will be used.
        /// </summary>
        /// <param name="forMergeOption"> The merge option which should be supported by the returned execution plan </param>
        /// <returns> an execution plan capable of retrieving the results of this query using the specified merge option </returns>
        internal abstract ObjectQueryExecutionPlan GetExecutionPlan(MergeOption? forMergeOption);

        /// <summary>
        /// Must returns a new ObjectQueryState instance that is a duplicate of this instance and additionally contains the specified Include path in its
        /// <see
        ///     cref="Span" />
        /// .
        /// </summary>
        /// <typeparam name="TElementType"> The element type of the source query on which Include was called </typeparam>
        /// <param name="sourceQuery"> The source query on which Include was called </param>
        /// <param name="includePath"> The new Include path to add </param>
        /// <returns> Must returns an ObjectQueryState that is a duplicate of this instance and additionally contains the specified Include path </returns>
        internal abstract ObjectQueryState Include<TElementType>(ObjectQuery<TElementType> sourceQuery, string includePath);

        /// <summary>
        /// Retrieves the result type of the query in terms of C-Space metadata. This method is called once, on-demand, if a call
        /// to <see cref="ObjectQuery.GetResultType" /> cannot be satisfied using cached type metadata or a currently cached execution plan.
        /// </summary>
        /// <returns>
        /// Must return a <see cref="TypeUsage" /> that describes the result typeof this query in terms of C-Space metadata
        /// </returns>
        protected abstract TypeUsage GetResultType();

        /// <summary>
        /// Helper method to return the first non-null merge option from the specified nullable merge options,
        /// or the <see cref="DefaultMergeOption" /> if the value of all specified nullable merge options is <c>null</c>.
        /// </summary>
        /// <param name="preferredMergeOptions"> The available nullable merge option values, in order of decreasing preference </param>
        /// <returns>
        /// the first non-null merge option; or the default merge option if the value of all
        /// <paramref
        ///     name="preferredMergeOptions" />
        /// is null
        /// </returns>
        protected static MergeOption EnsureMergeOption(params MergeOption?[] preferredMergeOptions)
        {
            foreach (var preferred in preferredMergeOptions)
            {
                if (preferred.HasValue)
                {
                    return preferred.Value;
                }
            }

            return DefaultMergeOption;
        }

        /// <summary>
        /// Helper method to return the first non-null merge option from the specified nullable merge options.
        /// </summary>
        /// <param name="preferredMergeOptions"> The available nullable merge option values, in order of decreasing preference </param>
        /// <returns>
        /// the first non-null merge option; or <c>null</c> if the value of all <paramref name="preferredMergeOptions" /> is null
        /// </returns>
        protected static MergeOption? GetMergeOption(params MergeOption?[] preferredMergeOptions)
        {
            foreach (var preferred in preferredMergeOptions)
            {
                if (preferred.HasValue)
                {
                    return preferred.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// Helper method to create a new ObjectQuery based on this query state instance.
        /// </summary>
        /// <returns>
        /// A new <see cref="ObjectQuery{TResultType}" /> - typed as <see cref="ObjectQuery" />
        /// </returns>
        public ObjectQuery CreateQuery()
        {
            Debug.Assert(CreateObjectQueryMethod != null, "Unable to retrieve ObjectQueryState.CreateObjectQuery<> method");

            var genericObjectQueryMethod = CreateObjectQueryMethod.MakeGenericMethod(_elementType);
            return (ObjectQuery)genericObjectQueryMethod.Invoke(this, new object[0]);
        }

        /// <summary>
        /// Helper method used to create an ObjectQuery based on an underlying ObjectQueryState instance.
        /// This method must be public to be reliably callable from <see cref="CreateObjectQuery" /> using reflection.
        /// Shouldn't be named CreateQuery to avoid ambiguity with reflection.
        /// </summary>
        /// <typeparam name="TResultType"> The required element type of the new ObjectQuery </typeparam>
        /// <returns> A new ObjectQuery based on the specified query state, with the specified element type </returns>
        public ObjectQuery<TResultType> CreateObjectQuery<TResultType>()
        {
            Debug.Assert(typeof(TResultType) == ElementType, "Element type mismatch");

            return new ObjectQuery<TResultType>(this);
        }
    }
}
