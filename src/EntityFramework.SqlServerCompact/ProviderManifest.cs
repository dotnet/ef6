// The classes here have been pulled from the devdiv source depot and modified for SQLCE.
// Original class is DbProviderManifest in System.Data.Common namespace.
//

namespace System.Data.Entity.SqlServerCompact
{
    internal class ProviderManifest
    {
        /// <summary>Value to pass to GetInformation to get the StoreSchemaDefinition</summary>
        public static readonly string StoreSchemaDefinition = "StoreSchemaDefinition";

        /// <summary>Value to pass to GetInformation to get the StoreSchemaMapping</summary>
        public static readonly string StoreSchemaMapping = "StoreSchemaMapping";

        /// <summary>Value to pass to GetInformation to get the ConceptualSchemaDefinition</summary>
        public static readonly string ConceptualSchemaDefinition = "ConceptualSchemaDefinition";

        // System Facet Info
        /// <summary>
        /// Name of the MaxLength Facet
        /// </summary>
        internal const string MaxLengthFacetName = "MaxLength";

        /// <summary>
        /// Name of the Unicode Facet
        /// </summary>
        internal const string UnicodeFacetName = "Unicode";

        /// <summary>
        /// Name of the FixedLength Facet
        /// </summary>
        internal const string FixedLengthFacetName = "FixedLength";

        /// <summary>
        /// Name of the Precision Facet
        /// </summary>
        internal const string PrecisionFacetName = "Precision";

        /// <summary>
        /// Name of the Scale Facet
        /// </summary>
        internal const string ScaleFacetName = "Scale";

        /// <summary>
        /// Name of the Nullable Facet
        /// </summary>
        internal const string NullableFacetName = "Nullable";

        /// <summary>
        /// Name of the DefaultValue Facet
        /// </summary>
        internal const string DefaultValueFacetName = "DefaultValue";

        /// <summary>
        /// Name of the Collation Facet
        /// </summary>
        internal const string CollationFacetName = "Collation";
    }
}
