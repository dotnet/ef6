// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Database
{
    using System;
    using System.Data;

    internal interface IRawDataSchemaParameter : IDataSchemaObject
    {
        Type UrtType { get; }
        ParameterDirection Direction { get; }
        int Size { get; }
        int Precision { get; }
        int Scale { get; }
        int ProviderDataType { get; }
        string NativeDataType { get; }
    }
}
