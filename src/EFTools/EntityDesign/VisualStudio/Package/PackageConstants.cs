// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Package
{
    using System;

    internal static class PackageConstants
    {
        // MUST match guids.h

        // << these GUIDs must be kept in sync with those used by the DSL
        public const string guidEscherPkgString = "8889e051-b7f9-4781-bb33-2a36a9bdb3a5";
        public const string guidEscherEditorFactoryString = "c99aea30-8e36-4515-b76f-496f5a48a6aa";
        public const string guidEscherCmdSetString = "11ac0a76-365e-490d-abad-e44e52897c7d";
        public const string guidExplorerWindowString = "A34B1C5D-6D37-4a0c-A8B0-99F8E8158B48";

        // This has to be a const because it is used in an attribute.
        public const string guidLogicalViewString = "{ab6778a7-2644-4467-bd57-154f50f3dae5}";

        public static readonly Guid guidLogicalView = new Guid(guidLogicalViewString);

        public static readonly Guid guidEscherPkg = new Guid(guidEscherPkgString);
        public static readonly Guid guidEscherEditorFactory = new Guid(guidEscherEditorFactoryString);
        public static readonly Guid guidEscherCmdSet = new Guid(guidEscherCmdSetString);

        public static readonly Guid guidExplorerWindow = new Guid(guidExplorerWindowString);

        public const string DefaultDiagramExtension = ".diagram";

        public const int menuidExplorer = 0x10001;

        public const int cmdIdExplorerAddScalarPropertyBase = 0x1F00;
        public const int cmdIdExplorerAddComplexPropertyBase = 0x2000;
        public const int cmdIdLayerCommandsBase = 0x3000;
        public const int cmdIdLayerRefactoringCommandsBase = 0x4000;
        // <<
    };
}
