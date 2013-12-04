// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Reflection;

[assembly: AssemblyTitle("Microsoft.Data.Entity.Design.Extensibility")]
[assembly: AssemblyDescription("Microsoft.Data.Entity.Design.Extensibility.dll")]
[assembly: CLSCompliant(false)]

// if !VS11 use SharedAssemblyInfo to define the properties below instead.
// if VS11 we want a fixed version because any extensions customers have written target the 11.1 version
// already shipped and we don't want to break them when we ship a new version of the EFTools MSI.
#if VS11
[assembly: AssemblyCompany("Microsoft Corporation")]
[assembly: AssemblyCopyright("© Microsoft Corporation.  All rights reserved.")]
[assembly: AssemblyProduct("Microsoft Entity Framework")]
[assembly: System.Runtime.InteropServices.ComVisible(false)]
[assembly: System.Resources.NeutralResourcesLanguage("en-US")]
#if !BUILD_GENERATED_VERSION
[assembly: AssemblyVersion("11.1.0.0")]
[assembly: AssemblyFileVersion("11.2.0.0")]
#endif
#endif
