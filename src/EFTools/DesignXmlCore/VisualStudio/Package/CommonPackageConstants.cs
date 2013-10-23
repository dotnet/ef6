// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Package
{
    using System;

    internal static class CommonPackageConstants
    {
        // Note: MUST match guids.h
        internal static readonly Guid xmlEditorGuid = new Guid("FA3CD31E-987B-443A-9B81-186104E8DAC1");
        internal static readonly Guid xmlEditorGuid2 = new Guid("412B8852-4F21-413B-9B47-0C9751D3EBFB");
        internal static readonly Guid xmlEditorPackageGuid = new Guid("87569308-4813-40a0-9cd0-d7a30838ca3f");
        internal static readonly Guid xmlEditorLanguageService = new Guid("f6819a78-a205-47b5-be1c-675b3c7f0b8e");

        internal const string ctmenuResourceId = "ctmenu";
        internal const int ctmenuVersion = 1;
    };
}
