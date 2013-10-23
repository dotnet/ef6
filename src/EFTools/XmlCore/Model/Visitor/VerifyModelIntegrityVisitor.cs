// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Visitor
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Model.XLinqAnnotations;

#if DEBUG
    // Verify the integrity of the model by checking:
    // 1. each object X is not disposed
    // 2. each object is "resolved" or "resolveAttempted"
    // 3. each object has an XOwner
    // 4. the XOwner has a ModelItemAnnotaton
    // 5. the ModelItemAnnotation is the same EFObject as X
    // 6. each ItemBinding's target is not disposed.
    internal class VerifyModelIntegrityVisitor : Visitor
    {
        private readonly HashSet<EFObject> _disposedList = new HashSet<EFObject>();
        private readonly HashSet<EFObject> _unresolvedList = new HashSet<EFObject>();
        private readonly HashSet<EFObject> _noXObjectList = new HashSet<EFObject>();
        private readonly HashSet<XObject> _noMIAList = new HashSet<XObject>();
        private readonly HashSet<ItemBinding> _staleItemBindingList = new HashSet<ItemBinding>();
        private readonly Dictionary<XObject, EFObject> _incorrectMIAList = new Dictionary<XObject, EFObject>();

        private readonly bool _checkDisposed = true;
        private readonly bool _checkUnresolved = true;
        private readonly bool _checkXObject = true;
        private readonly bool _checkAnnotations = true;
        private readonly bool _checkBindingIntegrity = true;

        public VerifyModelIntegrityVisitor(
            bool checkDisposed, bool checkUnresolved, bool checkXObject, bool checkAnnotations, bool checkBindingIntegrity)
        {
            _checkDisposed = checkDisposed;
            _checkUnresolved = checkUnresolved;
            _checkXObject = checkXObject;
            _checkAnnotations = checkAnnotations;
            _checkBindingIntegrity = checkBindingIntegrity;
        }

        public VerifyModelIntegrityVisitor()
        {
        }

        internal virtual string AllSerializedErrors
        {
            get
            {
                var sb = new StringBuilder();
                if (_checkDisposed)
                {
                    sb.AppendLine("EFObjects that are incorrectly disposed: ");
                    sb.AppendLine(DisposedObjects);
                }
                if (_checkUnresolved)
                {
                    sb.AppendLine("EFObjects that are not resolved: ");
                    sb.AppendLine(UnresolvedObjects);
                }
                if (_checkXObject)
                {
                    sb.AppendLine("EFObjects that do not have XObjects: ");
                    sb.AppendLine(ObjectsWithNoXObjects);
                }
                if (_checkAnnotations)
                {
                    sb.AppendLine("XObjects without ModelItemAnnotations: ");
                    sb.AppendLine(XObjectsWithNoAnnotations);
                    sb.AppendLine("XObjects with incorrect ModelItemAnnotations: ");
                    sb.AppendLine(XObjectsWithIncorrectAnnotations);
                }
                if (_checkBindingIntegrity)
                {
                    sb.AppendLine("Binding with disposed target: ");
                    sb.AppendLine(StaleItemBindings);
                }
                return sb.ToString();
            }
        }

        internal virtual int ErrorCount
        {
            get
            {
                var errorSum = 0;
                if (_checkDisposed)
                {
                    errorSum += _disposedList.Count;
                }
                if (_checkUnresolved)
                {
                    errorSum += _unresolvedList.Count;
                }
                if (_checkXObject)
                {
                    errorSum += _noXObjectList.Count;
                }
                if (_checkAnnotations)
                {
                    errorSum += _noMIAList.Count + _incorrectMIAList.Count;
                }
                if (_checkBindingIntegrity)
                {
                    errorSum += _staleItemBindingList.Count;
                }
                return errorSum;
            }
        }

        internal string DisposedObjects
        {
            get
            {
                var sb = new StringBuilder();
                foreach (var efObject in _disposedList)
                {
                    sb.AppendLine(efObject.ToPrettyString());
                }
                return sb.ToString();
            }
        }

        internal string UnresolvedObjects
        {
            get
            {
                var sb = new StringBuilder();
                foreach (var efObject in _unresolvedList)
                {
                    sb.AppendLine(efObject.ToPrettyString());
                }
                return sb.ToString();
            }
        }

        internal string ObjectsWithNoXObjects
        {
            get
            {
                var sb = new StringBuilder();
                foreach (var efObject in _noXObjectList)
                {
                    sb.AppendLine(efObject.ToPrettyString());
                }
                return sb.ToString();
            }
        }

        internal string XObjectsWithNoAnnotations
        {
            get
            {
                var sb = new StringBuilder();
                foreach (var xobject in _noMIAList)
                {
                    sb.AppendLine(xobject.ToString());
                }
                return sb.ToString();
            }
        }

        internal string XObjectsWithIncorrectAnnotations
        {
            get
            {
                var sb = new StringBuilder();
                var xobjectEnum = _incorrectMIAList.GetEnumerator();
                while (xobjectEnum.MoveNext())
                {
                    sb.Append(
                        "XObject: '" + xobjectEnum.Current.Key + "' is incorrectly pointing to EFObject '" + xobjectEnum.Current.Value + "'");
                }
                return sb.ToString();
            }
        }

        internal string StaleItemBindings
        {
            get
            {
                var sb = new StringBuilder();
                foreach (var binding in _staleItemBindingList)
                {
                    sb.AppendLine(binding.ToPrettyString());
                }
                return sb.ToString();
            }
        }

        /// <summary>
        ///     Gets a value indicating whether the EFObject should be visited.
        /// </summary>
        protected virtual bool ShouldVisit(EFObject efObject)
        {
            return true;
        }

        /// <summary>
        ///     Gets a value indicating whether the EFObject is a valid ghost node.
        /// </summary>
        /// <remarks>
        ///     A valid ghost node will not cause an error if it has an incorrect or stale xlinq annotation.
        /// </remarks>
        protected virtual bool IsValidGhostNode(EFObject efObject)
        {
            return false;
        }

        internal override void Visit(IVisitable visitable)
        {
            var efObject = visitable as EFObject;
            Debug.Assert(efObject != null, "We are visiting an object: '" + visitable + "' that is not an EFObject");
            if (efObject != null
                && ShouldVisit(efObject))
            {
                if (CheckForDisposed(efObject))
                {
                    return;
                }

                if (CheckForUnresolved(efObject))
                {
                    return;
                }

                if (CheckForNoXObject(efObject))
                {
                    return;
                }

                if (CheckForMissingAnnotations(efObject))
                {
                    return;
                }

                CheckWhetherBindingTargetsAreDisposed(efObject as ItemBinding);
            }
        }

        private bool CheckForDisposed(EFObject efObject)
        {
            if (_checkDisposed && efObject.IsDisposed)
            {
                if (!_disposedList.Contains(efObject))
                {
                    _disposedList.Add(efObject);
                }
                return true;
            }

            return false;
        }

        private bool CheckForUnresolved(EFObject efObject)
        {
            var efElement = efObject as EFElement;
            var itemBinding = efObject as ItemBinding;
            var parentElement = efObject.Parent as EFElement;
            if (_checkUnresolved)
            {
                if (efElement != null)
                {
                    if (!(efElement.State == EFElementState.Resolved || efElement.State == EFElementState.ResolveAttempted))
                    {
                        if (!_unresolvedList.Contains(efObject))
                        {
                            _unresolvedList.Add(efObject);
                        }
                        return true;
                    }
                }
                else if (itemBinding != null)
                {
                    if (itemBinding.Parent.State == EFElementState.Resolved
                        && (itemBinding.Resolved == false || itemBinding.IsStatusUnknown))
                    {
                        var xattr = parentElement.GetAttribute(efObject.EFTypeName);
                        if (xattr == null)
                        {
                            return true;
                        }

                        if (!_unresolvedList.Contains(efObject))
                        {
                            _unresolvedList.Add(efObject);
                        }
                        return true;
                    }
                }
            }

            return false;
        }

        private bool CheckForNoXObject(EFObject efObject)
        {
            if (_checkXObject && efObject.XObject == null)
            {
                // some EFAttributes are not tied to XAttributes
                var efAttribute = efObject as EFAttribute;
                var parentElement = efObject.Parent as EFElement;
                if (efAttribute != null
                    && parentElement != null)
                {
                    var xattr = efAttribute.Namespace != null
                                    ? parentElement.GetAttribute(efObject.EFTypeName, efAttribute.Namespace.NamespaceName)
                                    : parentElement.GetAttribute(efObject.EFTypeName);
                    if (xattr == null)
                    {
                        return true;
                    }
                }

                if (!_noXObjectList.Contains(efObject))
                {
                    _noXObjectList.Add(efObject);
                }
                return true;
            }

            return false;
        }

        private bool CheckForMissingAnnotations(EFObject efObject)
        {
            if (_checkAnnotations)
            {
                var miaEFobject = ModelItemAnnotation.GetModelItem(efObject.XObject);
                var isArtifact = efObject is EFArtifact;
                if (miaEFobject == null
                    && !isArtifact)
                {
                    if (!_noMIAList.Contains(efObject.XObject))
                    {
                        _noMIAList.Add(efObject.XObject);
                    }
                    return true;
                }

                // Check for incorrect/stale annotations
                if (miaEFobject != efObject
                    && !isArtifact
                    && !IsValidGhostNode(efObject))
                {
                    if (!_incorrectMIAList.ContainsKey(efObject.XObject))
                    {
                        _incorrectMIAList.Add(efObject.XObject, miaEFobject);
                    }
                    return true;
                }
            }

            return false;
        }

        private void CheckWhetherBindingTargetsAreDisposed(ItemBinding itemBinding)
        {
            if (_checkBindingIntegrity && itemBinding != null)
            {
                if (itemBinding.IsBindingTargetDisposed
                    && !_staleItemBindingList.Contains(itemBinding))
                {
                    _staleItemBindingList.Add(itemBinding);
                }
            }
        }
    }

#endif
}
