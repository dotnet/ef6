// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Diagnostics;

    internal class Binding<T> : Binding, IDisposable
        where T : EFNormalizableItem, IDisposable
    {
        private readonly ItemBinding _parent;
        private readonly BindingStatus _status = BindingStatus.None;
        private readonly T _target;
        private bool _isDisposed;

        internal Binding(ItemBinding parent, BindingStatus status, T target)
        {
            _parent = parent;
            _status = status;
            _target = target;

            if (_parent != null)
            {
                var artifactSet = _parent.Artifact.ArtifactSet;
                if (_target != null)
                {
                    artifactSet.AddDependency(_parent, _target);
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1821:RemoveEmptyFinalizers")]
        ~Binding()
        {
            Debug.Assert(_isDisposed = true, "A Binding class did not have Dispose() called on it!");
        }

        /// <summary>
        ///     Returns the target value of the reference, i.e. the EFElement object
        ///     being referred to.  Returns null unless the reference's status is
        ///     Known or Duplicate.
        /// </summary>
        internal T Target
        {
            get { return _target; }
        }

        internal override BindingStatus Status
        {
            get { return _status; }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                if (_parent != null)
                {
                    var artifactSet = _parent.Artifact.ArtifactSet;
                    if (_target != null)
                    {
                        artifactSet.RemoveDependency(_parent, Target);
                    }
                }
                _isDisposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
}
