// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Internal.Materialization
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Objects.Internal;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    ///     Typed <see cref="Coordinator" />
    /// </summary>
    internal class Coordinator<T> : Coordinator
    {
        #region State

        internal readonly CoordinatorFactory<T> TypedCoordinatorFactory;

        /// <summary>
        ///     Exposes the Current element that has been materialized (and is being populated) by this coordinator.
        /// </summary>
        internal virtual T Current
        {
            get { return _current; }
        }

        private T _current;

        /// <summary>
        ///     For ObjectResult, aggregates all elements for in the nested collection handled by this coordinator.
        /// </summary>
        private ICollection<T> _elements;

        /// <summary>
        ///     For ObjectResult, aggregates all elements as wrapped entities for in the nested collection handled by this coordinator.
        /// </summary>
        private List<IEntityWrapper> _wrappedElements;

        /// <summary>
        ///     Delegate called when the current nested collection has been consumed. This is necessary in Span
        ///     scenarios where an EntityCollection RelatedEnd is populated only when all related entities have
        ///     been materialized.  This version of the close handler works with wrapped entities.
        /// </summary>
        private Action<Shaper, List<IEntityWrapper>> _handleClose;

        /// <summary>
        ///     For nested, object-layer coordinators we want to collect all the elements we find and handle them
        ///     when the root coordinator advances.  Otherwise we just want to return them as we find them.
        /// </summary>
        private readonly bool IsUsingElementCollection;

        #endregion

        internal Coordinator(CoordinatorFactory<T> coordinatorFactory, Coordinator parent, Coordinator next)
            : base(coordinatorFactory, parent, next)
        {
            TypedCoordinatorFactory = coordinatorFactory;

            // generate all children
            Coordinator nextChild = null;
            foreach (var nestedCoordinator in coordinatorFactory.NestedCoordinators.Reverse())
            {
                // last child processed is first child...
                Child = nestedCoordinator.CreateCoordinator(this, nextChild);
                nextChild = Child;
            }

            IsUsingElementCollection = !IsRoot && (typeof(T) != typeof(RecordState));
        }

        #region "Public" Surface Area

        internal override void ResetCollection(Shaper shaper)
        {
            // Check to see if anyone has registered for notification when the current coordinator
            // is reset.
            if (null != _handleClose)
            {
                _handleClose(shaper, _wrappedElements);
                _handleClose = null;
            }

            // Reset is entered for this collection.
            IsEntered = false;

            if (IsUsingElementCollection)
            {
                _elements = TypedCoordinatorFactory.InitializeCollection(shaper);
                _wrappedElements = new List<IEntityWrapper>();
            }

            if (null != Child)
            {
                Child.ResetCollection(shaper);
            }
            if (null != Next)
            {
                Next.ResetCollection(shaper);
            }
        }

        internal override void ReadNextElement(Shaper shaper)
        {
            T element;
            IEntityWrapper wrappedElement = null;
            try
            {
                if (TypedCoordinatorFactory.WrappedElement == null)
                {
                    element = TypedCoordinatorFactory.Element(shaper);
                }
                else
                {
                    wrappedElement = TypedCoordinatorFactory.WrappedElement(shaper);
                    // This cast may throw, in which case it will be immediately caught
                    // and the error handling expression will be used to get the appropriate error message.
                    element = (T)wrappedElement.Entity;
                }
            }
            catch (Exception e)
            {
                if (e.IsCatchableExceptionType()
                    && !shaper.Reader.IsClosed)
                {
                    // Some errors can occur while a close handler is registered.  This clears
                    // out the handler so that ElementWithErrorHandling will report the correct
                    // error rather than asserting on the missing close handler.
                    ResetCollection(shaper);
                    // call a variation of the "Element" delegate with more detailed
                    // error handling (to produce a better exception message)
                    element = TypedCoordinatorFactory.ElementWithErrorHandling(shaper);
                }

                // rethrow
                throw;
            }
            if (IsUsingElementCollection)
            {
                _elements.Add(element);
                if (wrappedElement != null)
                {
                    _wrappedElements.Add(wrappedElement);
                }
            }
            else
            {
                _current = element;
            }
        }

        /// <summary>
        ///     Sets the delegate called when this collection is closed.  This close handler works on
        ///     a collection of wrapped entities, rather than on the raw entity objects.
        /// </summary>
        internal void RegisterCloseHandler(Action<Shaper, List<IEntityWrapper>> closeHandler)
        {
            Debug.Assert(null == _handleClose, "more than one handler for a collection close 'event'");
            _handleClose = closeHandler;
        }

        /// <summary>
        ///     Called when we're disposing the enumerator;
        /// </summary>
        internal void SetCurrentToDefault()
        {
            _current = default(T);
        }

        #endregion

        #region Runtime Callable Code

        // Code in this section is called from the delegates produced by the Translator.  It may  
        // not show up if you search using Find All References

        /// <summary>
        ///     Returns a handle to the element aggregator for this nested collection.
        /// </summary>
        private IEnumerable<T> GetElements()
        {
            return _elements;
        }

        #endregion
    }
}
