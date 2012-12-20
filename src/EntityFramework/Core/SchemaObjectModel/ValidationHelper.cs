// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.SchemaObjectModel
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>
    ///     Helper methods used for Schema Object Model (validation) validation.
    /// </summary>
    internal static class ValidationHelper
    {
        /// <summary>
        ///     Validates whether facets are declared correctly.
        /// </summary>
        /// <param name="element"> Schema element being validated. Must not be null. </param>
        /// <param name="type"> Resolved type (from declaration on the element). Possibly null. </param>
        /// <param name="typeUsageBuilder"> TypeUsageBuilder for the current element. Must not be null. </param>
        internal static void ValidateFacets(SchemaElement element, SchemaType type, TypeUsageBuilder typeUsageBuilder)
        {
            DebugCheck.NotNull(element);
            DebugCheck.NotNull(typeUsageBuilder);

            if (type != null)
            {
                var schemaEnumType = type as SchemaEnumType;
                if (schemaEnumType != null)
                {
                    typeUsageBuilder.ValidateEnumFacets(schemaEnumType);
                }
                else if (!(type is ScalarType)
                         && typeUsageBuilder.HasUserDefinedFacets)
                {
                    Debug.Assert(!(type is SchemaEnumType), "Note that enums should have already been handled.");

                    // Non-scalar type should not have Facets. 
                    element.AddError(
                        ErrorCode.FacetOnNonScalarType, EdmSchemaErrorSeverity.Error, Strings.FacetsOnNonScalarType(type.FQName));
                }
            }
            else
            {
                if (typeUsageBuilder.HasUserDefinedFacets)
                {
                    // Type attribute not specified but facets exist.
                    element.AddError(
                        ErrorCode.IncorrectlyPlacedFacet, EdmSchemaErrorSeverity.Error, Strings.FacetDeclarationRequiresTypeAttribute);
                }
            }
        }

        /// <summary>
        ///     Validated whether a type is declared correctly.
        /// </summary>
        /// <param name="element"> Schema element being validated. Must not be null. </param>
        /// <param name="type"> Resolved type (from declaration on the element). Possibly null. </param>
        /// <param name="typeSubElement"> Child schema element. Possibly null. </param>
        /// <remarks>
        ///     For some elements (e.g. ReturnType) we allow the type to be defined inline in an attribute on the element itself or
        ///     by using nested elements. These definitions are mutually exclusive.
        /// </remarks>
        internal static void ValidateTypeDeclaration(SchemaElement element, SchemaType type, SchemaElement typeSubElement)
        {
            DebugCheck.NotNull(element);

            if (type == null
                && typeSubElement == null)
            {
                //Type not declared as either attribute or subelement
                element.AddError(ErrorCode.TypeNotDeclared, EdmSchemaErrorSeverity.Error, Strings.TypeMustBeDeclared);
            }

            if (type != null
                && typeSubElement != null)
            {
                //Both attribute and sub-element declarations exist
                element.AddError(
                    ErrorCode.TypeDeclaredAsAttributeAndElement, EdmSchemaErrorSeverity.Error, Strings.TypeDeclaredAsAttributeAndElement);
            }
        }

        /// <summary>
        ///     Validate that reference type is an entity type.
        /// </summary>
        /// <param name="element"> Schema element being validated. Must not be null. </param>
        /// <param name="type"> Resolved type (from declaration on the element). Possibly null. </param>
        internal static void ValidateRefType(SchemaElement element, SchemaType type)
        {
            DebugCheck.NotNull(element);

            if (type != null
                && !(type is SchemaEntityType))
            {
                // Ref type refers to non entity type.
                element.AddError(
                    ErrorCode.ReferenceToNonEntityType, EdmSchemaErrorSeverity.Error, Strings.ReferenceToNonEntityType(type.FQName));
            }
        }
    }
}
