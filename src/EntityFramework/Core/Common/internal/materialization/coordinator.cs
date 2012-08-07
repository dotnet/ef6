// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Internal.Materialization
{
    /// <summary>
    ///     A coordinator is responsible for tracking state and processing result in a root or nested query
    ///     result collection. The coordinator exists within a graph, and knows its Parent, (First)Child,
    ///     and Next sibling. This allows the Shaper to use the coordinator as a simple state machine when
    ///     consuming store reader results.
    /// </summary>
    internal abstract class Coordinator
    {
        #region State

        /// <summary>
        ///     The factory used to generate this coordinator instance. Contains delegates used
        ///     by the Shaper during result enumeration.
        /// </summary>
        internal readonly CoordinatorFactory CoordinatorFactory;

        /// <summary>
        ///     Parent coordinator (the coordinator producing rows containing this collection).
        ///     If this is the root, null.
        /// </summary>
        internal readonly Coordinator Parent;

        /// <summary>
        ///     First coordinator for nested results below this collection. When reading a new row
        ///     for this coordinator, we walk down to the Child.
        /// 
        ///     NOTE:: this cannot be readonly because we can't know both the parent and the child
        ///     at initialization time; we set the Child in the parent's constructor.
        /// </summary>
        public Coordinator Child { get; protected set; }

        /// <summary>
        ///     Next coordinator at this depth. Once we're done consuming results for this reader,
        ///     we move on to this.Next.
        /// </summary>
        internal readonly Coordinator Next;

        /// <summary>
        ///     Indicates whether data has been read for the collection being aggregated or yielded
        ///     by this coordinator.
        /// </summary>
        public bool IsEntered { get; protected set; }

        /// <summary>
        ///     Indicates whether this is the top level coordinator for a query.
        /// </summary>
        internal bool IsRoot
        {
            get { return null == Parent; }
        }

        #endregion

        protected Coordinator(CoordinatorFactory coordinatorFactory, Coordinator parent, Coordinator next)
        {
            CoordinatorFactory = coordinatorFactory;
            Parent = parent;
            Next = next;
        }

        #region "Public" Surface Area

        /// <summary>
        ///     Registers this hierarchy of coordinators in the given shaper.
        /// </summary>
        internal void Initialize(Shaper shaper)
        {
            ResetCollection(shaper);

            // Add this coordinator to the appropriate state slot in the 
            // shaper so that it is available to materialization delegates.
            shaper.State[CoordinatorFactory.StateSlot] = this;

            if (null != Child)
            {
                Child.Initialize(shaper);
            }
            if (null != Next)
            {
                Next.Initialize(shaper);
            }
        }

        /// <summary>
        ///     Determines the maximum depth of this subtree.
        /// </summary>
        internal int MaxDistanceToLeaf()
        {
            var maxDistance = 0;
            var child = Child;
            while (null != child)
            {
                maxDistance = Math.Max(maxDistance, child.MaxDistanceToLeaf() + 1);
                child = child.Next;
            }
            return maxDistance;
        }

        /// <summary>
        ///     This method is called when the current collection is finished and it's time to move to the next collection.
        ///     Recursively initializes children and siblings as well.
        /// </summary>
        internal abstract void ResetCollection(Shaper shaper);

        /// <summary>
        ///     Precondition: the current row has data for the coordinator.
        ///     Side-effects: updates keys currently stored in state and updates IsEntered if a new value is encountered.
        ///     Determines whether the row contains the next element in this collection.
        /// </summary>
        internal bool HasNextElement(Shaper shaper)
        {
            // check if this row contains a new element for this coordinator
            var result = false;

            if (!IsEntered
                || !CoordinatorFactory.CheckKeys(shaper))
            {
                // remember initial keys values
                CoordinatorFactory.SetKeys(shaper);
                IsEntered = true;
                result = true;
            }

            return result;
        }

        /// <summary>
        ///     Precondition: the current row has data and contains a new element for the coordinator.
        ///     Reads the next element in this collection.
        /// </summary>
        internal abstract void ReadNextElement(Shaper shaper);

        #endregion
    }
}
