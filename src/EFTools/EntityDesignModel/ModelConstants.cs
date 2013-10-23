// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Data.Entity.Core.Metadata.Edm;

    internal static class ModelConstants
    {
        internal static readonly string NoneValue = "(None)";
        internal static readonly string Multiplicity_One = "1";
        internal static readonly string Multiplicity_Many = "*";
        internal static readonly string Multiplicity_ZeroOrOne = "0..1";

        internal static readonly string ConcurrencyModeNone = "None";
        internal static readonly string ConcurrencyModeFixed = "Fixed";

        internal static readonly string CodeGenerationAccessPublic = "Public";
        internal static readonly string CodeGenerationAccessInternal = "Internal";
        internal static readonly string CodeGenerationAccessProtected = "Protected";
        internal static readonly string CodeGenerationAccessPrivate = "Private";

        internal static readonly string DefaultPropertyType = Enum.GetName(typeof(PrimitiveTypeKind), PrimitiveTypeKind.String);
        internal static readonly string BinaryPropertyType = Enum.GetName(typeof(PrimitiveTypeKind), PrimitiveTypeKind.Binary);
        internal static readonly string Int32PropertyType = Enum.GetName(typeof(PrimitiveTypeKind), PrimitiveTypeKind.Int32);
        internal static readonly string StringPropertyType = Enum.GetName(typeof(PrimitiveTypeKind), PrimitiveTypeKind.String);
        internal static readonly bool DefaultPropertyNullability = false;

        internal static readonly string FunctionScalarPropertyVersionOriginal = "Original";
        internal static readonly string FunctionScalarPropertyVersionCurrent = "Current";

        internal static readonly string StoreSchemaGeneratorSchemaAttributeName = "Schema";
        internal static readonly string StoreSchemaGeneratorNameAttributeName = "Name";
        internal static readonly string StoreSchemaGeneratorTypeAttributeName = "Type";
        internal static readonly string StoreSchemaGenTypeAttributeValueTables = "Tables";
        internal static readonly string StoreSchemaGenTypeAttributeValueViews = "Views";

        internal static readonly string OnDeleteAction_None = "None";
        internal static readonly string OnDeleteAction_Cascade = "Cascade";

        internal static readonly string StoreGeneratedPattern_None = "None";
        internal static readonly string StoreGeneratedPattern_Identity = "Identity";
        internal static readonly string StoreGeneratedPattern_Computed = "Computed";

        // don't i18n since we want this to be a valid identifier
        internal static readonly string DefaultStorageContainerFormat = "{0}Container";

        // don't i18n since we want this to be a valid identifier
        internal static readonly string DefaultStorageNamespaceFormat = "{0}.Store";

        // don't i18n since we want this to be a valid identifier
        internal static readonly string DefaultEntityContainerName = "Entities";

        // don't i18n since we want this to be a valid identifier
        internal static readonly string DefaultModelNamespace = "Model";

        /// <summary>
        ///     These names are used with &lt;ReferenceMetadata/&gt; elements
        /// </summary>
        internal static class MetadataNames
        {
            internal static readonly string StorageNamespace = "StorageNamespace";
            internal static readonly string StorageEntityContainer = "StorageEntityContainer";
            internal static readonly string ConceptualNamespace = "ConceptualNamespace";
            internal static readonly string ConceptualEntityContainer = "ConceptualEntityContainer";
        }
    }
}
