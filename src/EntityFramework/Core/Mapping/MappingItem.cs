// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Diagnostics;

    /// <summary>
    /// Base class for items in the mapping space (DataSpace.CSSpace)
    /// </summary>
    public abstract class MappingItem
    {
        private bool _readOnly;
        private readonly List<MetadataProperty> _annotations = new List<MetadataProperty>();

        internal bool IsReadOnly
        {
            get { return _readOnly; }
        }

        internal IList<MetadataProperty> Annotations
        {
            get { return _annotations; }
        }

        internal virtual void SetReadOnly()
        {
            _annotations.TrimExcess();

            _readOnly = true;
        }

        internal void ThrowIfReadOnly()
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException(Strings.OperationOnReadOnlyItem);
            }
        }

        internal static void SetReadOnly(MappingItem item)
        {
            if (item != null)
            {
                item.SetReadOnly();
            }
        }

        internal static void SetReadOnly(IEnumerable<MappingItem> items)
        {
            if (items == null)
            {
                return;
            }

            foreach (var item in items)
            {
                SetReadOnly(item);
            }
        }
    }
}
