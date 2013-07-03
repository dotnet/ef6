// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    internal interface INamedDataModelItem
    {
        string Name { get; }
        string Identity { get; }
    }
}
