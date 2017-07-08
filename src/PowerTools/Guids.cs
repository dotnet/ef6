// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace Microsoft.DbContextPackage
{
    using System;

    internal static class GuidList
    {
        public const string guidDbContextPackagePkgString = "F0A7D01D-4834-44C3-99B2-5907A0701906";
        public const string guidDbContextPackageCmdSetString = "c769a05d-8d51-4919-bfe6-5f35a0eaf27e";

        public static readonly Guid guidDbContextPackageCmdSet = new Guid(guidDbContextPackageCmdSetString);
    }
}