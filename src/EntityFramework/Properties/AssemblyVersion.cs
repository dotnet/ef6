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

#if !NET40

// In EF 4.1-4.3, these attributes were all in the System.ComponentModel.DataAnnotations
// namespace of EntityFramework.dll.
//
// In EF 5+ for .NET 4, all but MaxLength and MinLength were moved to the
// System.ComponentModel.DataAnnotations.Schema namespace and remained in EntityFramework.dll.
//
// In .NET 4.5, these attributes were moved into the .NET Framework as part of the
// System.ComponentModel.DataAnnotations.dll assembly. Hence in EF 5+ for .NET 4.5 and later,
// the type forwarding below forwards from the EntityFramework.dll for EF 5+ on .NET 4 to
// System.ComponentModel.DataAnnotations.dll in .NET 4.5 or later.

[assembly: TypeForwardedTo(typeof(System.ComponentModel.DataAnnotations.MaxLengthAttribute))]
[assembly: TypeForwardedTo(typeof(System.ComponentModel.DataAnnotations.MinLengthAttribute))]
[assembly: TypeForwardedTo(typeof(System.ComponentModel.DataAnnotations.Schema.ColumnAttribute))]
[assembly: TypeForwardedTo(typeof(System.ComponentModel.DataAnnotations.Schema.ComplexTypeAttribute))]
[assembly: TypeForwardedTo(typeof(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedAttribute))]
[assembly: TypeForwardedTo(typeof(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption))]
[assembly: TypeForwardedTo(typeof(System.ComponentModel.DataAnnotations.Schema.ForeignKeyAttribute))]
[assembly: TypeForwardedTo(typeof(System.ComponentModel.DataAnnotations.Schema.InversePropertyAttribute))]
[assembly: TypeForwardedTo(typeof(System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute))]
[assembly: TypeForwardedTo(typeof(System.ComponentModel.DataAnnotations.Schema.TableAttribute))]

#endif
