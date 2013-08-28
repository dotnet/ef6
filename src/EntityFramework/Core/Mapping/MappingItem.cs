// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    /// <summary>
    /// ILegacyMappingItem
    /// </summary>
    internal interface ILegacyMappingItem
    {
        string TypeFullName { get; }
    }

    /// <summary>
    /// MappingItem
    /// </summary>
    public abstract class MappingItem : ILegacyMappingItem
    {
        string ILegacyMappingItem.TypeFullName
        {
            get { return TypeFullName; }
        }

        internal virtual string TypeFullName 
        {
            get { return GetType().Namespace + ".Storage" + GetType().Name; }
        }
    }
}
