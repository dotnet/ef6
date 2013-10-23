// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Eventing;

    internal class EDMXModelChangeEventArgs : ModelChangeEventArgs
    {
        private readonly EfiChangeGroup _efiChangeGroup;

        internal EDMXModelChangeEventArgs(EfiChangeGroup changeGroup)
        {
            _efiChangeGroup = changeGroup;
        }

        public override IEnumerable<ModelNodeChangeInfo> Changes
        {
            get
            {
                foreach (var c in _efiChangeGroup.Changes)
                {
                    var t = GetChangeTypeFromEfiChange(c);
                    var info = new ModelNodeChangeInfo(c.Changed, t);
                    yield return info;
                }
            }
        }

        internal static ModelNodeChangeType GetChangeTypeFromEfiChange(EfiChange c)
        {
            ModelNodeChangeType t;

            switch (c.Type)
            {
                case EfiChange.EfiChangeType.Create:
                    t = ModelNodeChangeType.Added;
                    break;
                case EfiChange.EfiChangeType.Delete:
                    t = ModelNodeChangeType.Deleted;
                    break;
                case EfiChange.EfiChangeType.Update:
                    t = ModelNodeChangeType.Changed;
                    break;
                default:
                    Debug.Fail("unexpected type of EfiChangeType");
                    throw new InvalidOperationException();
            }

            return t;
        }
    }
}
