// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Database
{
    using System;

    internal interface IRawDataSchemaColumn : IDataSchemaObject
    {
        Type UrtType { get; }
        uint? Size { get; }
        bool IsNullable { get; }
        uint? Precision { get; }
        uint? Scale { get; }
        int ProviderDataType { get; }
        string NativeDataType { get; }
    }
}
