// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

// <summary>
//     FxCop suppressions for the project
// </summary>

[module:
    SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Vs", Scope = "module",
        Target = "microsoft.visualstudio.qualitytools.vsidetesthostframework.dll",
        Justification =
            "Public VS IDE host adapter classes also use 'Vs' instead of 'VS', and they can't be renamed, so suppressing for consistency")]
