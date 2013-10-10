// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.DbContextPackage.Utilities
{
    interface IViewGenerator
    {
        string ContextTypeName { get; set; }
        string MappingHashValue { get; set; }
        dynamic Views { get; set; }

        string TransformText();
    }

    partial class CSharpViewGenerator : IViewGenerator
    {
    }

    partial class VBViewGenerator : IViewGenerator
    {
    }
}
