// Guids.cs
// MUST match guids.h
namespace Microsoft.DbContextPackage
{
    using System;

    internal static class GuidList
    {
        public const string guidDbContextPackagePkgString = "2b119c79-9836-46e2-b5ed-eb766cebbf7c";
        public const string guidDbContextPackageCmdSetString = "c769a05d-8d51-4919-bfe6-5f35a0eaf27e";

        public static readonly Guid guidDbContextPackageCmdSet = new Guid(guidDbContextPackageCmdSetString);
    }
}