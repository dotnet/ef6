// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Common.EntitySql
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.EntitySql.AST;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Text;

    /// <summary>
    /// Error reporting Helper
    /// </summary>
    internal static class CqlErrorHelper
    {
        /// <summary>
        /// Reports function overload resolution error.
        /// </summary>
        internal static void ReportFunctionOverloadError(MethodExpr functionExpr, EdmFunction functionType, List<TypeUsage> argTypes)
        {
            var strDelim = "";
            var sb = new StringBuilder();
            sb.Append(functionType.Name).Append("(");
            for (var i = 0; i < argTypes.Count; i++)
            {
                sb.Append(strDelim);
                sb.Append(argTypes[i] != null ? argTypes[i].EdmType.FullName : "NULL");
                strDelim = ", ";
            }
            sb.Append(")");

            Func<object, object, object, string> formatString;
            if (TypeSemantics.IsAggregateFunction(functionType))
            {
                formatString = TypeHelpers.IsCanonicalFunction(functionType)
                                   ? Strings.NoCanonicalAggrFunctionOverloadMatch
                                   : (Func<object, object, object, string>)Strings.NoAggrFunctionOverloadMatch;
            }
            else
            {
                formatString = TypeHelpers.IsCanonicalFunction(functionType)
                                   ? Strings.NoCanonicalFunctionOverloadMatch
                                   : (Func<object, object, object, string>)Strings.NoFunctionOverloadMatch;
            }

            throw EntitySqlException.Create(
                functionExpr.ErrCtx.CommandText,
                formatString(functionType.NamespaceName, functionType.Name, sb.ToString()),
                functionExpr.ErrCtx.InputPosition,
                Strings.CtxFunction(functionType.Name),
                false,
                null);
        }

        /// <summary>
        /// provides error feedback for aliases already used in a given context
        /// </summary>
        /// <param name="aliasName"></param>
        /// <param name="errCtx"></param>
        /// <param name="contextMessage"></param>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId =
                "System.Data.Entity.Core.EntityUtil.EntitySqlError(System.Data.Entity.Core.Common.EntitySql.ErrorContext,System.String)")]
        internal static void ReportAliasAlreadyUsedError(string aliasName, ErrorContext errCtx, string contextMessage)
        {
            throw EntitySqlException.Create(
                errCtx, String.Format(CultureInfo.InvariantCulture, "{0} {1}", Strings.AliasNameAlreadyUsed(aliasName), contextMessage),
                null);
        }

        /// <summary>
        /// Reports incompatible type error
        /// </summary>
        /// <param name="errCtx"></param>
        /// <param name="leftType"></param>
        /// <param name="rightType"></param>
        internal static void ReportIncompatibleCommonType(ErrorContext errCtx, TypeUsage leftType, TypeUsage rightType)
        {
            //
            // 'navigate' through the type structure in order to find where the incompability is
            //
            ReportIncompatibleCommonType(errCtx, leftType, rightType, leftType, rightType);

            //
            // if we hit this point, throw the generic incompatible type error message
            //
            throw EntitySqlException.Create(errCtx, Strings.ArgumentTypesAreIncompatible(leftType.Identity, rightType.Identity), null);
        }

        /// <summary>
        /// navigates through the type structure to find where the incompatibility happens
        /// </summary>
        /// <param name="errCtx"></param>
        /// <param name="rootLeftType"></param>
        /// <param name="rootRightType"></param>
        /// <param name="leftType"></param>
        /// <param name="rightType"></param>
        private static void ReportIncompatibleCommonType(
            ErrorContext errCtx, TypeUsage rootLeftType, TypeUsage rootRightType, TypeUsage leftType, TypeUsage rightType)
        {
            TypeUsage commonType = null;
            var isRootType = (rootLeftType == leftType);
            var errorMessage = String.Empty;

            if (leftType.EdmType.BuiltInTypeKind
                != rightType.EdmType.BuiltInTypeKind)
            {
                throw EntitySqlException.Create(
                    errCtx, Strings.TypeKindMismatch(
                        GetReadableTypeKind(leftType),
                        GetReadableTypeName(leftType),
                        GetReadableTypeKind(rightType),
                        GetReadableTypeName(rightType)), null);
            }

            switch (leftType.EdmType.BuiltInTypeKind)
            {
                case BuiltInTypeKind.RowType:
                    var leftRow = (RowType)leftType.EdmType;
                    var rightRow = (RowType)rightType.EdmType;

                    if (leftRow.Members.Count
                        != rightRow.Members.Count)
                    {
                        if (isRootType)
                        {
                            errorMessage = Strings.InvalidRootRowType(
                                GetReadableTypeName(leftRow),
                                GetReadableTypeName(rightRow));
                        }
                        else
                        {
                            errorMessage = Strings.InvalidRowType(
                                GetReadableTypeName(leftRow),
                                GetReadableTypeName(rootLeftType),
                                GetReadableTypeName(rightRow),
                                GetReadableTypeName(rootRightType));
                        }

                        throw EntitySqlException.Create(errCtx, errorMessage, null);
                    }

                    for (var i = 0; i < leftRow.Members.Count; i++)
                    {
                        ReportIncompatibleCommonType(
                            errCtx, rootLeftType, rootRightType, leftRow.Members[i].TypeUsage, rightRow.Members[i].TypeUsage);
                    }
                    break;

                case BuiltInTypeKind.CollectionType:
                case BuiltInTypeKind.RefType:
                    ReportIncompatibleCommonType(
                        errCtx,
                        rootLeftType,
                        rootRightType,
                        TypeHelpers.GetElementTypeUsage(leftType),
                        TypeHelpers.GetElementTypeUsage(rightType));
                    break;

                case BuiltInTypeKind.EntityType:
                    if (!TypeSemantics.TryGetCommonType(leftType, rightType, out commonType))
                    {
                        if (isRootType)
                        {
                            errorMessage = Strings.InvalidEntityRootTypeArgument(
                                GetReadableTypeName(leftType),
                                GetReadableTypeName(rightType));
                        }
                        else
                        {
                            errorMessage = Strings.InvalidEntityTypeArgument(
                                GetReadableTypeName(leftType),
                                GetReadableTypeName(rootLeftType),
                                GetReadableTypeName(rightType),
                                GetReadableTypeName(rootRightType));
                        }
                        throw EntitySqlException.Create(errCtx, errorMessage, null);
                    }
                    break;

                case BuiltInTypeKind.ComplexType:
                    var leftComplex = (ComplexType)leftType.EdmType;
                    var rightComplex = (ComplexType)rightType.EdmType;
                    if (leftComplex.Members.Count
                        != rightComplex.Members.Count)
                    {
                        if (isRootType)
                        {
                            errorMessage = Strings.InvalidRootComplexType(
                                GetReadableTypeName(leftComplex),
                                GetReadableTypeName(rightComplex));
                        }
                        else
                        {
                            errorMessage = Strings.InvalidComplexType(
                                GetReadableTypeName(leftComplex),
                                GetReadableTypeName(rootLeftType),
                                GetReadableTypeName(rightComplex),
                                GetReadableTypeName(rootRightType));
                        }
                        throw EntitySqlException.Create(errCtx, errorMessage, null);
                    }

                    for (var i = 0; i < leftComplex.Members.Count; i++)
                    {
                        ReportIncompatibleCommonType(
                            errCtx,
                            rootLeftType,
                            rootRightType,
                            leftComplex.Members[i].TypeUsage,
                            rightComplex.Members[i].TypeUsage);
                    }
                    break;

                default:
                    if (!TypeSemantics.TryGetCommonType(leftType, rightType, out commonType))
                    {
                        if (isRootType)
                        {
                            errorMessage = Strings.InvalidPlaceholderRootTypeArgument(
                                GetReadableTypeKind(leftType),
                                GetReadableTypeName(leftType),
                                GetReadableTypeKind(rightType),
                                GetReadableTypeName(rightType));
                        }
                        else
                        {
                            errorMessage = Strings.InvalidPlaceholderTypeArgument(
                                GetReadableTypeKind(leftType),
                                GetReadableTypeName(leftType),
                                GetReadableTypeName(rootLeftType),
                                GetReadableTypeKind(rightType),
                                GetReadableTypeName(rightType),
                                GetReadableTypeName(rootRightType));
                        }
                        throw EntitySqlException.Create(errCtx, errorMessage, null);
                    }
                    break;
            }
        }

        #region Private Type Name Helpers

        private static string GetReadableTypeName(TypeUsage type)
        {
            return GetReadableTypeName(type.EdmType);
        }

        private static string GetReadableTypeName(EdmType type)
        {
            if (type.BuiltInTypeKind == BuiltInTypeKind.RowType ||
                type.BuiltInTypeKind == BuiltInTypeKind.CollectionType
                ||
                type.BuiltInTypeKind == BuiltInTypeKind.RefType)
            {
                return type.Name;
            }
            return type.FullName;
        }

        private static string GetReadableTypeKind(TypeUsage type)
        {
            return GetReadableTypeKind(type.EdmType);
        }

        private static string GetReadableTypeKind(EdmType type)
        {
            var typeKindName = String.Empty;
            switch (type.BuiltInTypeKind)
            {
                case BuiltInTypeKind.RowType:
                    typeKindName = Strings.LocalizedRow;
                    break;
                case BuiltInTypeKind.CollectionType:
                    typeKindName = Strings.LocalizedCollection;
                    break;
                case BuiltInTypeKind.RefType:
                    typeKindName = Strings.LocalizedReference;
                    break;
                case BuiltInTypeKind.EntityType:
                    typeKindName = Strings.LocalizedEntity;
                    break;
                case BuiltInTypeKind.ComplexType:
                    typeKindName = Strings.LocalizedComplex;
                    break;
                case BuiltInTypeKind.PrimitiveType:
                    typeKindName = Strings.LocalizedPrimitive;
                    break;
                default:
                    typeKindName = type.BuiltInTypeKind.ToString();
                    break;
            }
            return typeKindName + " " + Strings.LocalizedType;
        }

        #endregion
    }
}
