// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

// TODO: SDE Merge - Make assembly SecurityTransaprent again
//[assembly: SecurityTransparent]

[assembly: SecurityCritical]
[assembly: ComCompatibleVersion(1, 0, 3300, 0)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: CompilationRelaxations(8)]
[assembly: AllowPartiallyTrustedCallers]
[assembly: SecurityRules(SecurityRuleSet.Level1, SkipVerificationInFullTrust = true)]
[assembly: AssemblyTitle("EntityFramework.dll")]
[assembly: AssemblyDescription("EntityFramework.dll")]
[assembly: AssemblyDefaultAlias("EntityFramework.dll")]
[assembly: AssemblyProduct("Microsoft® .NET Framework")]
