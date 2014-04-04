// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Data.Entity.Design.Model.Visitor;

    internal static class XmlModelHelper
    {
        internal static void NormalizeAndResolve(EFContainer efContainer)
        {
            if (efContainer == null)
            {
                return;
            }

            // revert this item and its children to the Parsed State, if we don't do this
            // then they will be skipped by the other visitors. Also Unbind so resolving will
            // rebind any references.
            var stateChangingVisitor = new StateChangingVisitor(EFElementState.Parsed);
            stateChangingVisitor.Traverse(efContainer);
            var unbindingVisitor = new UnbindingVisitor();
            unbindingVisitor.Traverse(efContainer);

            ModelManager.NormalizeItem(efContainer);
            efContainer.Artifact.ModelManager.ResolveItem(efContainer);
        }

        internal static void RebindItemBindings(IEnumerable<ItemBinding> itemBindings)
        {
            // first unbind any of these bindings
            foreach (var ib in itemBindings)
            {
                if (!ib.IsDisposed)
                {
                    ib.Unbind();

                    if (ib.Parent.State == EFElementState.Normalized
                        || ib.Parent.State == EFElementState.ResolveAttempted
                        || ib.Parent.State == EFElementState.Resolved)
                    {
                        ib.Parent.State = EFElementState.Normalized;
                    }
                    else if (ib.Parent.State == EFElementState.Parsed)
                    {
                        // we need to normalize this
                        ib.Parent.Normalize();
                    }
                    else
                    {
                        Debug.Fail("Unexpected state of model element.  Model has not been parsed, normalized or resolved.");
                    }

                    Debug.Assert(
                        ib.Parent.State == EFElementState.Normalized, "Unexpected state of elmenet.  Expected state to be normalized.");
                }
            }

            while (true)
            {
                var nResolved = 0;
                var nUnResolved = 0;
                foreach (var ib in itemBindings)
                {
                    if (!ib.IsDisposed)
                    {
                        if (!ib.Resolved)
                        {
                            // we rebind the parent, since that will re-set the Parent's state to Resolved
                            ib.Parent.Resolve(ib.Artifact.ArtifactSet);

                            if (ib.Resolved)
                            {
                                nResolved++;
                            }
                            else
                            {
                                nUnResolved++;
                            }
                        }
                    }
                }
                // we didn't resolve anything, or we have nothing left to resolve, we can break here.
                if (nResolved == 0
                    || nUnResolved == 0)
                {
                    break;
                }
            }
        }

        internal static bool IsUniqueNameInsideContainer(
            EFContainer container, string proposedName, bool uniquenessIsCaseSensitive, HashSet<EFObject> childEFObjectsToIgnore = null)
        {
            if (string.IsNullOrEmpty(proposedName)
                ||
                container == null)
            {
                return false;
            }

            var isUniqueName = true;

            var comparisonType =
                uniquenessIsCaseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;
            foreach (var child in container.Children)
            {
                var nameableChild = child as EFNameableItem;
                if (nameableChild != null
                    &&
                    (childEFObjectsToIgnore != null ? !childEFObjectsToIgnore.Contains(nameableChild) : true))
                {
                    if (nameableChild.LocalName.Value.Equals(proposedName, comparisonType))
                    {
                        isUniqueName = false;
                        break;
                    }
                }
            }

            return isUniqueName;
        }

        /// <summary>
        ///     Find an EFObject given a delimited name identifier. For example,
        ///     to find the property 'SomeProperty' within the EntityType 'SomeEntity',
        ///     the delimitedNameIdentifier will be 'SomeProperty.SomeEntity' and the
        ///     startingObject will be the artifact
        /// </summary>
        /// <param name="delimitedNameIdentifier"></param>
        /// <returns></returns>
        internal static EFNameableItem FindNameableItemViaIdentifier(EFContainer excludedAncestor, string delimitedNameIdentifier)
        {
            var ids = new Queue<string>(delimitedNameIdentifier.Split('.'));
            return FindNameableItemViaIdentifierInternal(excludedAncestor, ids);
        }

        private static EFNameableItem FindNameableItemViaIdentifierInternal(EFContainer currentContainer, Queue<string> nameQueue)
        {
            if (currentContainer != null)
            {
                var namePart = nameQueue.Dequeue();
                foreach (var childObject in currentContainer.Children.OfType<EFObject>())
                {
                    var nameableItem = childObject as EFNameableItem;
                    var itemBinding = childObject as ItemBinding;
                    var foundNamePartMatch = false;

                    // first examine the name. We can use the name as the type or the actual name.
                    if (namePart.StartsWith("*", StringComparison.Ordinal)
                        && namePart.EndsWith("*", StringComparison.Ordinal))
                    {
                        var trimmedNamePart = namePart.Trim('*');
                        if (itemBinding != null
                            && itemBinding.ResolvedTargets.FirstOrDefault() != null
                            &&
                            itemBinding.ResolvedTargets.FirstOrDefault().GetType().Name == trimmedNamePart)
                        {
                            foundNamePartMatch = true;
                            nameableItem = itemBinding.ResolvedTargets.FirstOrDefault() as EFNameableItem;
                        }
                    }
                    else if (nameableItem != null
                             && nameableItem.LocalName.Value.Equals(namePart, StringComparison.CurrentCulture))
                    {
                        foundNamePartMatch = true;
                    }

                    // if we have found a match, either recurse if there are more names or return this one.
                    if (foundNamePartMatch)
                    {
                        if (nameQueue.Count > 0)
                        {
                            return FindNameableItemViaIdentifierInternal(nameableItem, nameQueue);
                        }
                        else
                        {
                            return nameableItem;
                        }
                    }
                }
            }
            return null;
        }
    }
}
