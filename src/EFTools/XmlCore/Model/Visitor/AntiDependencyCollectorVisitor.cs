// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Visitor
{
    using System.Collections.Generic;

    /// <summary>
    ///     This class will traverse from the starting node, visiting all children, and accumulate
    ///     all ItemBindings that point to the child, or ItemBindings that point to any duplicate symbols
    ///     of a child.
    /// </summary>
    internal class AntiDependencyCollectorVisitor : Visitor
    {
        private readonly HashSet<ItemBinding> _antiDeps = new HashSet<ItemBinding>();

        internal HashSet<ItemBinding> AntiDependencyBindings
        {
            get { return _antiDeps; }
        }

        internal override void Visit(IVisitable visitable)
        {
            var ni = visitable as EFNameableItem;
            if (ni != null)
            {
                // if this is a nameable item, include any deps for any other elements that have the same normalized name
                foreach (EFObject efobj in ni.Artifact.ArtifactSet.GetSymbolList(ni.NormalizedName))
                {
                    foreach (var antiDep in efobj.GetDependentBindings())
                    {
                        _antiDeps.Add(antiDep);
                    }
                }
            }
            else
            {
                var efobj = visitable as EFObject;
                foreach (var antiDep in efobj.GetDependentBindings())
                {
                    _antiDeps.Add(antiDep);
                }
            }
        }
    }
}
