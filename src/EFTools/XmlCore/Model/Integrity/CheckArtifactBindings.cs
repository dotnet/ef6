// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Integrity
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Visitor;

    /// <summary>
    ///     At this point, this is a very basic check that rebinds all bindings in an EFArtifact.
    /// </summary>
    internal class CheckArtifactBindings : IIntegrityCheck
    {
        private readonly CommandProcessorContext _context;

        internal CheckArtifactBindings(CommandProcessorContext context)
        {
            _context = context;
        }

        public bool IsEqual(IIntegrityCheck otherCheck)
        {
            // only allow one of these checks to run.  
            return (otherCheck is CheckArtifactBindings);
        }

        public void Invoke()
        {
            try
            {
                XmlModelHelper.RebindItemBindings(_context.GetBindingsForRebind());
                // just clear these out now that we are done with them.  Probably shouldn't matter, but a 
                // bit more efficient in case _context is somehow re-used.
                _context.ClearBindingsForRebind();
            }
            catch (Exception e)
            {
                Debug.Fail(e.Message);
                throw;
            }
        }

        /// <summary>
        ///     Schedule the given set of bindings for rebinding when the transaction completes
        /// </summary>
        /// <param name="cpc"></param>
        /// <param name="bindingsToRebind"></param>
        internal static void ScheduleBindingsForRebind(CommandProcessorContext cpc, ICollection<ItemBinding> bindingsToRebind)
        {
            Debug.Assert(cpc != null);
            Debug.Assert(bindingsToRebind != null);

            if (bindingsToRebind.Count > 0)
            {
                var check = new CheckArtifactBindings(cpc);
                cpc.AddIntegrityCheck(check);

                foreach (var ib in bindingsToRebind)
                {
                    cpc.AddBindingForRebind(ib);
                }
            }
        }

        /// <summary>
        ///     Schedule unknown or undefined bindings in the given artifact set for rebinding when the transaction completes
        /// </summary>
        /// <param name="cpc"></param>
        /// <param name="artifactSet"></param>
        internal static void ScheduleUnknownBindingsForRebind(CommandProcessorContext cpc, EFArtifactSet artifactSet)
        {
            Debug.Assert(cpc != null);
            Debug.Assert(artifactSet != null);

            var ubv = new UnknownBindingVisitor();
            foreach (var artifact in artifactSet.Artifacts)
            {
                ubv.Traverse(artifact);
            }

            ScheduleBindingsForRebind(cpc, ubv.UnknownBindings);
        }

        internal static void ScheduleChildAntiDependenciesForRebinding(CommandProcessorContext cpc, EFObject efObject)
        {
            // identify any binding that was referencing this symbol, and add it to the list of things to rebind.
            var visitor = new AntiDependencyCollectorVisitor();
            visitor.Traverse(efObject);
            ScheduleBindingsForRebind(cpc, visitor.AntiDependencyBindings);
        }

        /// <summary>
        ///     This class will traverse from the starting node, visiting all children accumulating
        ///     bindings whose state is unknown or undefined.
        /// </summary>
        private class UnknownBindingVisitor : Visitor
        {
            private readonly HashSet<ItemBinding> _unknownBindings = new HashSet<ItemBinding>();

            internal ICollection<ItemBinding> UnknownBindings
            {
                get { return _unknownBindings; }
            }

            internal override void Visit(IVisitable visitable)
            {
                var ib = visitable as ItemBinding;
                if (ib != null)
                {
                    if (ib.IsStatusUnknown)
                    {
                        _unknownBindings.Add(ib);
                    }
                }
            }
        }
    }
}
