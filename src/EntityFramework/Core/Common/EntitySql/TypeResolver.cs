// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Represents eSQL type and namespace name resolver.
    /// </summary>
    internal sealed class TypeResolver
    {
        private readonly Perspective _perspective;
        private readonly ParserOptions _parserOptions;
        private readonly Dictionary<string, MetadataNamespace> _aliasedNamespaces;
        private readonly HashSet<MetadataNamespace> _namespaces;

        /// <summary>
        /// name -> list(overload)
        /// </summary>
        private readonly Dictionary<string, List<InlineFunctionInfo>> _functionDefinitions;

        private bool _includeInlineFunctions;
        private bool _resolveLeftMostUnqualifiedNameAsNamespaceOnly;

        /// <summary>
        /// Initializes TypeResolver instance
        /// </summary>
        internal TypeResolver(Perspective perspective, ParserOptions parserOptions)
        {
            DebugCheck.NotNull(perspective);

            _perspective = perspective;
            _parserOptions = parserOptions;
            _aliasedNamespaces = new Dictionary<string, MetadataNamespace>(parserOptions.NameComparer);
            _namespaces = new HashSet<MetadataNamespace>(
                MetadataMember.CreateMetadataMemberNameEqualityComparer(parserOptions.NameComparer));
            _functionDefinitions = new Dictionary<string, List<InlineFunctionInfo>>(parserOptions.NameComparer);
            _includeInlineFunctions = true;
            _resolveLeftMostUnqualifiedNameAsNamespaceOnly = false;
        }

        /// <summary>
        /// Returns perspective.
        /// </summary>
        internal Perspective Perspective
        {
            get { return _perspective; }
        }

        /// <summary>
        /// Returns namespace imports.
        /// </summary>
        internal ICollection<MetadataNamespace> NamespaceImports
        {
            get { return _namespaces; }
        }

        /// <summary>
        /// Returns <see cref="TypeUsage" /> for <see cref="PrimitiveTypeKind.String" />.
        /// </summary>
        internal static TypeUsage StringType
        {
            get { return MetadataWorkspace.GetCanonicalModelTypeUsage(PrimitiveTypeKind.String); }
        }

        /// <summary>
        /// Returns <see cref="TypeUsage" /> for <see cref="PrimitiveTypeKind.Boolean" />.
        /// </summary>
        internal static TypeUsage BooleanType
        {
            get { return MetadataWorkspace.GetCanonicalModelTypeUsage(PrimitiveTypeKind.Boolean); }
        }

        /// <summary>
        /// Returns <see cref="TypeUsage" /> for <see cref="PrimitiveTypeKind.Int64" />.
        /// </summary>
        internal static TypeUsage Int64Type
        {
            get { return MetadataWorkspace.GetCanonicalModelTypeUsage(PrimitiveTypeKind.Int64); }
        }

        /// <summary>
        /// Adds an aliased namespace import.
        /// </summary>
        internal void AddAliasedNamespaceImport(string alias, MetadataNamespace @namespace, ErrorContext errCtx)
        {
            if (_aliasedNamespaces.ContainsKey(alias))
            {
                var message = Strings.NamespaceAliasAlreadyUsed(alias);
                throw EntitySqlException.Create(errCtx, message, null);
            }

            _aliasedNamespaces.Add(alias, @namespace);
        }

        /// <summary>
        /// Adds a non-aliased namespace import.
        /// </summary>
        internal void AddNamespaceImport(MetadataNamespace @namespace, ErrorContext errCtx)
        {
            if (_namespaces.Contains(@namespace))
            {
                var message = Strings.NamespaceAlreadyImported(@namespace.Name);
                throw EntitySqlException.Create(errCtx, message, null);
            }

            _namespaces.Add(@namespace);
        }

        #region Inline function declarations

        /// <summary>
        /// Declares inline function in the query local metadata.
        /// </summary>
        internal void DeclareInlineFunction(string name, InlineFunctionInfo functionInfo)
        {
            DebugCheck.NotEmpty(name);
            DebugCheck.NotNull(functionInfo);

            List<InlineFunctionInfo> overloads;
            if (!_functionDefinitions.TryGetValue(name, out overloads))
            {
                overloads = new List<InlineFunctionInfo>();
                _functionDefinitions.Add(name, overloads);
            }

            //
            // Check overload uniqueness.
            //
            if (overloads.Exists(
                overload =>
                overload.Parameters.Select(p => p.ResultType).SequenceEqual(
                    functionInfo.Parameters.Select(p => p.ResultType), TypeUsageStructuralComparer.Instance)))
            {
                var errCtx = functionInfo.FunctionDefAst.ErrCtx;
                var message = Strings.DuplicatedInlineFunctionOverload(name);
                throw EntitySqlException.Create(errCtx, message, null);
            }

            overloads.Add(functionInfo);
        }

        private sealed class TypeUsageStructuralComparer : IEqualityComparer<TypeUsage>
        {
            private static readonly TypeUsageStructuralComparer _instance = new TypeUsageStructuralComparer();

            private TypeUsageStructuralComparer()
            {
            }

            public static TypeUsageStructuralComparer Instance
            {
                get { return _instance; }
            }

            public bool Equals(TypeUsage x, TypeUsage y)
            {
                return TypeSemantics.IsStructurallyEqual(x, y);
            }

            public int GetHashCode(TypeUsage obj)
            {
                Debug.Fail("Not implemented");
                return 0;
            }
        }

        #endregion

        internal IDisposable EnterFunctionNameResolution(bool includeInlineFunctions)
        {
            var savedIncludeInlineFunctions = _includeInlineFunctions;
            _includeInlineFunctions = includeInlineFunctions;
            return new Disposer(delegate { _includeInlineFunctions = savedIncludeInlineFunctions; });
        }

        internal IDisposable EnterBackwardCompatibilityResolution()
        {
            Debug.Assert(!_resolveLeftMostUnqualifiedNameAsNamespaceOnly, "EnterBackwardCompatibilityResolution() is not reentrant.");
            _resolveLeftMostUnqualifiedNameAsNamespaceOnly = true;
            return new Disposer(
                delegate
                    {
                        Debug.Assert(
                            _resolveLeftMostUnqualifiedNameAsNamespaceOnly, "_resolveLeftMostUnqualifiedNameAsNamespaceOnly must be true.");
                        _resolveLeftMostUnqualifiedNameAsNamespaceOnly = false;
                    });
        }

        internal MetadataMember ResolveMetadataMemberName(string[] name, ErrorContext errCtx)
        {
            DebugCheck.NotNull(name);
            Debug.Assert(name.Length > 0, "name must not be empty");

            MetadataMember metadataMember;
            if (name.Length == 1)
            {
                metadataMember = ResolveUnqualifiedName(name[0], false /* partOfQualifiedName */, errCtx);
            }
            else
            {
                metadataMember = ResolveFullyQualifiedName(name, name.Length, errCtx);
            }
            Debug.Assert(metadataMember != null, "metadata member name resolution must not return null");

            return metadataMember;
        }

        internal MetadataMember ResolveMetadataMemberAccess(MetadataMember qualifier, string name, ErrorContext errCtx)
        {
            var fullName = GetFullName(qualifier.Name, name);
            if (qualifier.MetadataMemberClass
                == MetadataMemberClass.Namespace)
            {
                //
                // Try resolving as a type.
                //
                MetadataType type;
                if (TryGetTypeFromMetadata(fullName, out type))
                {
                    return type;
                }

                //
                // Try resolving as a function.
                //
                MetadataFunctionGroup function;
                if (TryGetFunctionFromMetadata(qualifier.Name, name, out function))
                {
                    return function;
                }

                //
                // Otherwise, resolve as a namespace.
                //
                return new MetadataNamespace(fullName);
            }
            else if (qualifier.MetadataMemberClass
                     == MetadataMemberClass.Type)
            {
                var type = (MetadataType)qualifier;
                if (TypeSemantics.IsEnumerationType(type.TypeUsage))
                {
                    EnumMember member;
                    if (_perspective.TryGetEnumMember(
                        (EnumType)type.TypeUsage.EdmType, name, _parserOptions.NameComparisonCaseInsensitive /*ignoreCase*/, out member))
                    {
                        Debug.Assert(member != null, "member != null");
                        Debug.Assert(
                            _parserOptions.NameComparer.Equals(name, member.Name), "_parserOptions.NameComparer.Equals(name, member.Name)");
                        return new MetadataEnumMember(fullName, type.TypeUsage, member);
                    }
                    else
                    {
                        var message = Strings.NotAMemberOfType(name, qualifier.Name);
                        throw EntitySqlException.Create(errCtx, message, null);
                    }
                }
            }

            var message1 = Strings.InvalidMetadataMemberClassResolution(
                qualifier.Name, qualifier.MetadataMemberClassName, MetadataNamespace.NamespaceClassName);
            throw EntitySqlException.Create(errCtx, message1, null);
        }

        internal MetadataMember ResolveUnqualifiedName(string name, bool partOfQualifiedName, ErrorContext errCtx)
        {
            DebugCheck.NotEmpty(name);

            //
            // In the case of Name1.Name2...NameN and if backward compatibility mode is on, then resolve Name1 as namespace only, ignore any other possible resolutions.
            //
            var resolveAsNamespaceOnly = partOfQualifiedName && _resolveLeftMostUnqualifiedNameAsNamespaceOnly;

            //
            // In the case of Name1.Name2...NameN, ignore functions while resolving Name1: functions don't have members.
            //
            var includeFunctions = !partOfQualifiedName;

            //
            // Try resolving as an inline function.
            //
            InlineFunctionGroup inlineFunctionGroup;
            if (!resolveAsNamespaceOnly
                &&
                includeFunctions
                && TryGetInlineFunction(name, out inlineFunctionGroup))
            {
                return inlineFunctionGroup;
            }

            //
            // Try resolving as a namespace alias.
            //
            MetadataNamespace aliasedNamespaceImport;
            if (_aliasedNamespaces.TryGetValue(name, out aliasedNamespaceImport))
            {
                return aliasedNamespaceImport;
            }

            if (!resolveAsNamespaceOnly)
            {
                //
                // Try resolving as a type or functionGroup in the global namespace or as an imported member.
                // Throw if ambiguous.
                //
                MetadataType type = null;
                MetadataFunctionGroup functionGroup = null;

                if (!TryGetTypeFromMetadata(name, out type))
                {
                    if (includeFunctions)
                    {
                        //
                        // If name looks like a multipart identifier, try resolving it in the global namespace.
                        // Escaped multipart identifiers usually appear in views: select [NS1.NS2.Product](...) from ...
                        //
                        var multipart = name.Split('.');
                        if (multipart.Length > 1
                            && multipart.All(p => p.Length > 0))
                        {
                            var functionName = multipart[multipart.Length - 1];
                            var namespaceName = name.Substring(0, name.Length - functionName.Length - 1);
                            TryGetFunctionFromMetadata(namespaceName, functionName, out functionGroup);
                        }
                    }
                }

                //
                // Try resolving as an imported member.
                //
                MetadataNamespace importedMemberNamespace = null;
                foreach (var namespaceImport in _namespaces)
                {
                    var fullName = GetFullName(namespaceImport.Name, name);

                    MetadataType importedType;
                    if (TryGetTypeFromMetadata(fullName, out importedType))
                    {
                        if (type == null
                            && functionGroup == null)
                        {
                            type = importedType;
                            importedMemberNamespace = namespaceImport;
                        }
                        else
                        {
                            throw AmbiguousMetadataMemberName(errCtx, name, namespaceImport, importedMemberNamespace);
                        }
                    }

                    MetadataFunctionGroup importedFunctionGroup;
                    if (includeFunctions && TryGetFunctionFromMetadata(namespaceImport.Name, name, out importedFunctionGroup))
                    {
                        if (type == null
                            && functionGroup == null)
                        {
                            functionGroup = importedFunctionGroup;
                            importedMemberNamespace = namespaceImport;
                        }
                        else
                        {
                            throw AmbiguousMetadataMemberName(errCtx, name, namespaceImport, importedMemberNamespace);
                        }
                    }
                }
                if (type != null)
                {
                    return type;
                }
                if (functionGroup != null)
                {
                    return functionGroup;
                }
            }

            //
            // Otherwise, resolve as a namespace.
            //
            return new MetadataNamespace(name);
        }

        private MetadataMember ResolveFullyQualifiedName(string[] name, int length, ErrorContext errCtx)
        {
            Debug.Assert(name != null && length > 1 && length <= name.Length, "name must not be empty");

            //
            // Resolve N in N.R
            //
            MetadataMember left;
            if (length == 2)
            {
                //
                // If N is a single name, ignore functions: functions don't have members.
                //
                left = ResolveUnqualifiedName(name[0], true /* partOfQualifiedName */, errCtx);
            }
            else
            {
                left = ResolveFullyQualifiedName(name, length - 1, errCtx);
            }

            //
            // Get R in N.R
            //
            var rightName = name[length - 1];
            Debug.Assert(!String.IsNullOrEmpty(rightName), "rightName must not be empty");

            //
            // Resolve R in the context of N
            //
            return ResolveMetadataMemberAccess(left, rightName, errCtx);
        }

        private static Exception AmbiguousMetadataMemberName(ErrorContext errCtx, string name, MetadataNamespace ns1, MetadataNamespace ns2)
        {
            var message = Strings.AmbiguousMetadataMemberName(name, ns1.Name, ns2 != null ? ns2.Name : null);
            throw EntitySqlException.Create(errCtx, message, null);
        }

        /// <summary>
        /// Try get type from the model using the fully qualified name.
        /// </summary>
        private bool TryGetTypeFromMetadata(string typeFullName, out MetadataType type)
        {
            TypeUsage typeUsage;
            if (_perspective.TryGetTypeByName(typeFullName, _parserOptions.NameComparisonCaseInsensitive /* ignore case */, out typeUsage))
            {
                type = new MetadataType(typeFullName, typeUsage);
                return true;
            }
            else
            {
                type = null;
                return false;
            }
        }

        /// <summary>
        /// Try get function from the model using the fully qualified name.
        /// </summary>
        internal bool TryGetFunctionFromMetadata(string namespaceName, string functionName, out MetadataFunctionGroup functionGroup)
        {
            IList<EdmFunction> functionMetadata;
            if (_perspective.TryGetFunctionByName(
                namespaceName, functionName, _parserOptions.NameComparisonCaseInsensitive /* ignore case */, out functionMetadata))
            {
                functionGroup = new MetadataFunctionGroup(GetFullName(namespaceName, functionName), functionMetadata);
                return true;
            }
            else
            {
                functionGroup = null;
                return false;
            }
        }

        /// <summary>
        /// Try get function from the local metadata using the fully qualified name.
        /// </summary>
        private bool TryGetInlineFunction(string functionName, out InlineFunctionGroup inlineFunctionGroup)
        {
            List<InlineFunctionInfo> inlineFunctionMetadata;
            if (_includeInlineFunctions && _functionDefinitions.TryGetValue(functionName, out inlineFunctionMetadata))
            {
                inlineFunctionGroup = new InlineFunctionGroup(functionName, inlineFunctionMetadata);
                return true;
            }
            else
            {
                inlineFunctionGroup = null;
                return false;
            }
        }

        /// <summary>
        /// Builds a dot-separated multipart identifier off the provided <paramref name="names" />.
        /// </summary>
        internal static string GetFullName(params string[] names)
        {
            DebugCheck.NotNull(names);
            Debug.Assert(names.Length > 0, "names must not be null or empty");

            return String.Join(".", names);
        }
    }
}
