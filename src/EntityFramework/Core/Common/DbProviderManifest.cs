// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common
{
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml;

    /// <summary>
    ///     Metadata Interface for all CLR types types
    /// </summary>
    public abstract class DbProviderManifest
    {
        /// <summary>
        ///     Value to pass to GetInformation to get the StoreSchemaDefinition
        /// </summary>
        public const string StoreSchemaDefinition = "StoreSchemaDefinition";

        /// <summary>
        ///     Value to pass to GetInformation to get the StoreSchemaMapping
        /// </summary>
        public const string StoreSchemaMapping = "StoreSchemaMapping";

        /// <summary>
        ///     Value to pass to GetInformation to get the ConceptualSchemaDefinition
        /// </summary>
        public const string ConceptualSchemaDefinition = "ConceptualSchemaDefinition";

        /// <summary>
        ///     Value to pass to GetInformation to get the StoreSchemaDefinitionVersion3
        /// </summary>
        public const string StoreSchemaDefinitionVersion3 = "StoreSchemaDefinitionVersion3";

        /// <summary>
        ///     Value to pass to GetInformation to get the StoreSchemaMappingVersion3
        /// </summary>
        public const string StoreSchemaMappingVersion3 = "StoreSchemaMappingVersion3";

        /// <summary>
        ///     Value to pass to GetInformation to get the ConceptualSchemaDefinitionVersion3
        /// </summary>
        public const string ConceptualSchemaDefinitionVersion3 = "ConceptualSchemaDefinitionVersion3";

        // System Facet Info
        /// <summary>
        ///     Name of the MaxLength Facet
        /// </summary>
        public const string MaxLengthFacetName = "MaxLength";

        /// <summary>
        ///     Name of the Unicode Facet
        /// </summary>
        public const string UnicodeFacetName = "Unicode";

        /// <summary>
        ///     Name of the FixedLength Facet
        /// </summary>
        public const string FixedLengthFacetName = "FixedLength";

        /// <summary>
        ///     Name of the Precision Facet
        /// </summary>
        public const string PrecisionFacetName = "Precision";

        /// <summary>
        ///     Name of the Scale Facet
        /// </summary>
        public const string ScaleFacetName = "Scale";

        /// <summary>
        ///     Name of the Nullable Facet
        /// </summary>
        public const string NullableFacetName = "Nullable";

        /// <summary>
        ///     Name of the DefaultValue Facet
        /// </summary>
        public const string DefaultValueFacetName = "DefaultValue";

        /// <summary>
        ///     Name of the Collation Facet
        /// </summary>
        public const string CollationFacetName = "Collation";

        /// <summary>
        ///     Name of the SRID Facet
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Srid")]
        public const string SridFacetName = "SRID";

        /// <summary>
        ///     Name of the IsStrict Facet
        /// </summary>
        public const string IsStrictFacetName = "IsStrict";

        /// <summary>
        ///     Returns the namespace used by this provider manifest
        /// </summary>
        public abstract string NamespaceName { get; }

        /// <summary>
        ///     Return the set of types supported by the store
        /// </summary>
        /// <returns> A collection of primitive types </returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public abstract ReadOnlyCollection<PrimitiveType> GetStoreTypes();

        /// <summary>
        ///     Returns all the edm functions supported by the provider manifest.
        /// </summary>
        /// <returns> A collection of edm functions. </returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public abstract ReadOnlyCollection<EdmFunction> GetStoreFunctions();

        /// <summary>
        ///     Returns all the FacetDescriptions for a particular edmType
        /// </summary>
        /// <param name="edmType"> the edmType to return FacetDescriptions for </param>
        /// <returns> The FacetDescriptions for the edmType given </returns>
        public abstract ReadOnlyCollection<FacetDescription> GetFacetDescriptions(EdmType edmType);

        /// <summary>
        ///     This method allows a provider writer to take a edmType and a set of facets
        ///     and reason about what the best mapped equivalent edmType in EDM would be.
        /// </summary>
        /// <param name="storeType"> A TypeUsage encapsulating a store edmType and a set of facets </param>
        /// <returns> A TypeUsage encapsulating an EDM edmType and a set of facets </returns>
        public abstract TypeUsage GetEdmType(TypeUsage storeType);

        /// <summary>
        ///     This method allows a provider writer to take a edmType and a set of facets
        ///     and reason about what the best mapped equivalent edmType in the store would be.
        /// </summary>
        /// <param name="storeType"> A TypeUsage encapsulating an EDM edmType and a set of facets </param>
        /// <returns> A TypeUsage encapsulating a store edmType and a set of facets </returns>
        public abstract TypeUsage GetStoreType(TypeUsage edmType);

        /// <summary>
        ///     Providers should override this to return information specific to their provider.
        ///     This method should never return null.
        /// </summary>
        /// <param name="informationType"> The name of the information to be retrieved. </param>
        /// <returns> An XmlReader at the begining of the information requested. </returns>
        protected abstract XmlReader GetDbInformation(string informationType);

        /// <summary>
        ///     Gets framework and provider specific information
        ///     This method should never return null.
        /// </summary>
        /// <param name="informationType"> The name of the information to be retrieved. </param>
        /// <returns> An XmlReader at the begining of the information requested. </returns>
        public XmlReader GetInformation(string informationType)
        {
            XmlReader reader = null;
            try
            {
                reader = GetDbInformation(informationType);
            }
            catch (Exception e)
            {
                // we should not be wrapping all exceptions
                if (e.IsCatchableExceptionType())
                {
                    // we don't want folks to have to know all the various types of exceptions that can 
                    // occur, so we just rethrow a ProviderIncompatibleException and make whatever we caught  
                    // the inner exception of it.
                    throw new ProviderIncompatibleException(Strings.EntityClient_FailedToGetInformation(informationType), e);
                }
                throw;
            }
            if (reader == null)
            {
                // if the provider returned null for the conceptual schema definition, return the default one
                if (informationType == ConceptualSchemaDefinitionVersion3
                    || informationType == ConceptualSchemaDefinition)
                {
                    return DbProviderServices.GetConceptualSchemaDefinition(informationType);
                }

                throw new ProviderIncompatibleException(Strings.ProviderReturnedNullForGetDbInformation(informationType));
            }
            return reader;
        }

        /// <summary>
        ///     Does the provider support escaping strings to be used as patterns in a Like expression.
        ///     If the provider overrides this method to return true, <cref = "EscapeLikeArgument" /> should
        ///     also be overridden.
        /// </summary>
        /// <param name="escapeCharacter"> If the provider supports escaping, the character that would be used as the escape character </param>
        /// <returns> True, if this provider supports escaping strings to be used as patterns in a Like expression, false otherwise. The default implementation returns false. </returns>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "0#")]
        public virtual bool SupportsEscapingLikeArgument(out char escapeCharacter)
        {
            escapeCharacter = default(char);
            return false;
        }

        /// <summary>
        ///     Provider writers should override this method to returns the argument with the wildcards and the escape
        ///     character escaped.  This method is only used if <cref = "SupportsEscapingLikeArgument" /> returns true.
        /// </summary>
        /// <param name="argument"> The argument to be escaped </param>
        /// <returns> The argument with the wildcards and the escape character escaped </returns>
        public virtual string EscapeLikeArgument(string argument)
        {
            Check.NotNull(argument, "argument");

            throw new ProviderIncompatibleException(Strings.ProviderShouldOverrideEscapeLikeArgument);
        }
    }
}
