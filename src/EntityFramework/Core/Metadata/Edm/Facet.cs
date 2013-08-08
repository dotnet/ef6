// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Globalization;

    /// <summary>
    /// Class for representing a Facet object
    /// This object is Immutable (not just set to readonly) and
    /// some parts of the system are depending on that behavior
    /// </summary>
    [DebuggerDisplay("{Name,nq}={Value}")]
    public class Facet : MetadataItem
    {
        internal Facet()
        {
        }

        /// <summary>
        /// The constructor for constructing a Facet object with the facet description and a value
        /// </summary>
        /// <param name="facetDescription"> The object describing this facet </param>
        /// <param name="value"> The value of the facet </param>
        /// <exception cref="System.ArgumentNullException">Thrown if facetDescription argument is null</exception>
        private Facet(FacetDescription facetDescription, object value)
            : base(MetadataFlags.Readonly)
        {
            Check.NotNull(facetDescription, "facetDescription");

            _facetDescription = facetDescription;
            _value = value;
        }

        /// <summary>
        /// Creates a Facet instance with the specified value for the given
        /// facet description.
        /// </summary>
        /// <param name="facetDescription"> The object describing this facet </param>
        /// <param name="value"> The value of the facet </param>
        /// <exception cref="System.ArgumentNullException">Thrown if facetDescription argument is null</exception>
        internal static Facet Create(FacetDescription facetDescription, object value)
        {
            return Create(facetDescription, value, false);
        }

        /// <summary>
        /// Creates a Facet instance with the specified value for the given
        /// facet description.
        /// </summary>
        /// <param name="facetDescription"> The object describing this facet </param>
        /// <param name="value"> The value of the facet </param>
        /// <param name="bypassKnownValues"> true to bypass caching and known values; false otherwise. </param>
        /// <exception cref="System.ArgumentNullException">Thrown if facetDescription argument is null</exception>
        internal static Facet Create(FacetDescription facetDescription, object value, bool bypassKnownValues)
        {
            DebugCheck.NotNull(facetDescription);

            if (!bypassKnownValues)
            {
                // Reuse facets with a null value.
                if (ReferenceEquals(value, null))
                {
                    return facetDescription.NullValueFacet;
                }

                // Reuse facets with a default value.
                if (Equals(facetDescription.DefaultValue, value))
                {
                    return facetDescription.DefaultValueFacet;
                }

                // Special case boolean facets.
                if (facetDescription.FacetType.Identity == "Edm.Boolean")
                {
                    var boolValue = (bool)value;
                    return facetDescription.GetBooleanFacet(boolValue);
                }
            }

            var result = new Facet(facetDescription, value);

            // Check the type of the value only if we know what the correct CLR type is
            if (value != null
                && !Helper.IsUnboundedFacetValue(result)
                && !Helper.IsVariableFacetValue(result)
                && result.FacetType.ClrType != null)
            {
                var valueType = value.GetType();
                Debug.Assert(
                    valueType == result.FacetType.ClrType
                    || result.FacetType.ClrType.IsAssignableFrom(valueType),
                    string.Format(
                        CultureInfo.CurrentCulture, "The facet {0} has type {1}, but a value of type {2} was supplied.", result.Name,
                        result.FacetType.ClrType, valueType)
                    );
            }

            return result;
        }

        /// <summary>
        /// The object describing this facet.
        /// </summary>
        private readonly FacetDescription _facetDescription;

        /// <summary>
        /// The value assigned to this facet.
        /// </summary>
        private readonly object _value;

        /// <summary>
        /// Gets the built-in type kind for this <see cref="T:System.Data.Entity.Core.Metadata.Edm.Facet" />.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.Entity.Core.Metadata.Edm.BuiltInTypeKind" /> object that represents the built-in type kind for this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.Facet" />
        /// .
        /// </returns>
        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.Facet; }
        }

        /// <summary>
        /// Gets the description of this <see cref="T:System.Data.Entity.Core.Metadata.Edm.Facet" />.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Data.Entity.Core.Metadata.Edm.FacetDescription" /> object that represents the description of this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.Facet" />
        /// .
        /// </returns>
        public FacetDescription Description
        {
            get { return _facetDescription; }
        }

        /// <summary>
        /// Gets the name of this <see cref="T:System.Data.Entity.Core.Metadata.Edm.Facet" />.
        /// </summary>
        /// <returns>
        /// The name of this <see cref="T:System.Data.Entity.Core.Metadata.Edm.Facet" />.
        /// </returns>
        [MetadataProperty(PrimitiveTypeKind.String, false)]
        public virtual String Name
        {
            get { return _facetDescription.FacetName; }
        }

        /// <summary>
        /// Gets the type of this <see cref="T:System.Data.Entity.Core.Metadata.Edm.Facet" />.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Data.Entity.Core.Metadata.Edm.EdmType" /> object that represents the type of this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.Facet" />
        /// .
        /// </returns>
        [MetadataProperty(BuiltInTypeKind.EdmType, false)]
        public EdmType FacetType
        {
            get { return _facetDescription.FacetType; }
        }

        /// <summary>
        /// Gets the value of this <see cref="T:System.Data.Entity.Core.Metadata.Edm.Facet" />.
        /// </summary>
        /// <returns>
        /// The value of this <see cref="T:System.Data.Entity.Core.Metadata.Edm.Facet" />.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">Thrown if the Facet instance is in ReadOnly state</exception>
        [MetadataProperty(typeof(Object), false)]
        public virtual Object Value
        {
            get { return _value; }
        }

        /// <summary>
        /// Gets the identity for this item as a string
        /// </summary>
        internal override string Identity
        {
            get { return _facetDescription.FacetName; }
        }

        /// <summary>Gets a value indicating whether the value of the facet is unbounded.</summary>
        /// <returns>true if the value of the facet is unbounded; otherwise, false.</returns>
        public bool IsUnbounded
        {
            get { return ReferenceEquals(Value, EdmConstants.UnboundedValue); }
        }

        /// <summary>
        /// Returns the name of this <see cref="T:System.Data.Entity.Core.Metadata.Edm.Facet" />.
        /// </summary>
        /// <returns>
        /// The name of this <see cref="T:System.Data.Entity.Core.Metadata.Edm.Facet" />.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
