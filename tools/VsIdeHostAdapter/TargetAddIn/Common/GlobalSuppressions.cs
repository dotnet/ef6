// <copyright file="GlobalSuppressions.cs" company="Microsoft" owner="kouvel">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     FxCop suppressions for the project
// </summary>

using System;
using System.Diagnostics.CodeAnalysis;

[module: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Vs", Scope = "module", Target = "microsoft.visualstudio.qualitytools.hostadapters.vsideaddin.dll", Justification = "Public VS IDE host adapter classes also use 'Vs' instead of 'VS', and they can't be renamed, so suppressing for consistency")]
[module: SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId = "Addin", Scope = "resource", Target = "Microsoft.VisualStudio.TestTools.HostAdapters.VsIde.Resources.resources", Justification = "Referring to the class, which is public and cannot be renamed")]
[module: SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId = "testrunconfig", Scope = "resource", Target = "Microsoft.VisualStudio.TestTools.HostAdapters.VsIde.Resources.resources", Justification = "File extension")]
