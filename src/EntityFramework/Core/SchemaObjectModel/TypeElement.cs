// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.SchemaObjectModel
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Xml;

    /// <summary>
    ///     Responsible for parsing Type ProviderManifest
    ///     xml elements
    /// </summary>
    internal class TypeElement : SchemaType
    {
        private readonly PrimitiveType _primitiveType = new PrimitiveType();
        private readonly List<FacetDescriptionElement> _facetDescriptions = new List<FacetDescriptionElement>();

        public TypeElement(Schema parent)
            : base(parent)
        {
            _primitiveType.NamespaceName = Schema.Namespace;
        }

        protected override bool HandleElement(XmlReader reader)
        {
            if (base.HandleElement(reader))
            {
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.FacetDescriptionsElement))
            {
                SkipThroughElement(reader);
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.PrecisionElement))
            {
                HandlePrecisionElement(reader);
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.ScaleElement))
            {
                HandleScaleElement(reader);
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.MaxLengthElement))
            {
                HandleMaxLengthElement(reader);
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.UnicodeElement))
            {
                HandleUnicodeElement(reader);
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.FixedLengthElement))
            {
                HandleFixedLengthElement(reader);
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.SridElement))
            {
                HandleSridElement(reader);
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.IsStrictElement))
            {
                HandleIsStrictElement(reader);
                return true;
            }
            return false;
        }

        protected override bool HandleAttribute(XmlReader reader)
        {
            if (base.HandleAttribute(reader))
            {
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.PrimitiveTypeKindAttribute))
            {
                HandlePrimitiveTypeKindAttribute(reader);
                return true;
            }

            return false;
        }

        /////////////////////////////////////////////////////////////////////
        // Element Handlers

        /// <summary>
        ///     Handler for the Precision element
        /// </summary>
        /// <param name="reader"> xml reader currently positioned at Precision element </param>
        private void HandlePrecisionElement(XmlReader reader)
        {
            DebugCheck.NotNull(reader);
            var facetDescription = new ByteFacetDescriptionElement(this, DbProviderManifest.PrecisionFacetName);
            facetDescription.Parse(reader);

            _facetDescriptions.Add(facetDescription);
        }

        /// <summary>
        ///     Handler for the Scale element
        /// </summary>
        /// <param name="reader"> xml reader currently positioned at Scale element </param>
        private void HandleScaleElement(XmlReader reader)
        {
            DebugCheck.NotNull(reader);
            var facetDescription = new ByteFacetDescriptionElement(this, DbProviderManifest.ScaleFacetName);
            facetDescription.Parse(reader);
            _facetDescriptions.Add(facetDescription);
        }

        /// <summary>
        ///     Handler for the MaxLength element
        /// </summary>
        /// <param name="reader"> xml reader currently positioned at MaxLength element </param>
        private void HandleMaxLengthElement(XmlReader reader)
        {
            DebugCheck.NotNull(reader);
            var facetDescription = new IntegerFacetDescriptionElement(this, DbProviderManifest.MaxLengthFacetName);
            facetDescription.Parse(reader);
            _facetDescriptions.Add(facetDescription);
        }

        /// <summary>
        ///     Handler for the Unicode element
        /// </summary>
        /// <param name="reader"> xml reader currently positioned at Unicode element </param>
        private void HandleUnicodeElement(XmlReader reader)
        {
            DebugCheck.NotNull(reader);
            var facetDescription = new BooleanFacetDescriptionElement(this, DbProviderManifest.UnicodeFacetName);
            facetDescription.Parse(reader);
            _facetDescriptions.Add(facetDescription);
        }

        /// <summary>
        ///     Handler for the FixedLength element
        /// </summary>
        /// <param name="reader"> xml reader currently positioned at FixedLength element </param>
        private void HandleFixedLengthElement(XmlReader reader)
        {
            DebugCheck.NotNull(reader);
            var facetDescription = new BooleanFacetDescriptionElement(this, DbProviderManifest.FixedLengthFacetName);
            facetDescription.Parse(reader);
            _facetDescriptions.Add(facetDescription);
        }

        /// <summary>
        ///     Handler for the SRID element
        /// </summary>
        /// <param name="reader"> xml reader currently positioned at SRID element </param>
        private void HandleSridElement(XmlReader reader)
        {
            DebugCheck.NotNull(reader);
            var facetDescription = new SridFacetDescriptionElement(this, DbProviderManifest.SridFacetName);
            facetDescription.Parse(reader);
            _facetDescriptions.Add(facetDescription);
        }

        /// <summary>
        ///     Handler for the IsStrict element
        /// </summary>
        /// <param name="reader"> xml reader currently positioned at SRID element </param>
        private void HandleIsStrictElement(XmlReader reader)
        {
            DebugCheck.NotNull(reader);
            var facetDescription = new BooleanFacetDescriptionElement(this, DbProviderManifest.IsStrictFacetName);
            facetDescription.Parse(reader);
            _facetDescriptions.Add(facetDescription);
        }

        /////////////////////////////////////////////////////////////////////
        // Attribute Handlers

        /// <summary>
        ///     Handler for the PrimitiveTypeKind attribute
        /// </summary>
        /// <param name="reader"> xml reader currently positioned at Version attribute </param>
        private void HandlePrimitiveTypeKindAttribute(XmlReader reader)
        {
            DebugCheck.NotNull(reader);
            var value = reader.Value;
            try
            {
                _primitiveType.PrimitiveTypeKind = (PrimitiveTypeKind)Enum.Parse(typeof(PrimitiveTypeKind), value);
                _primitiveType.BaseType = MetadataItem.EdmProviderManifest.GetPrimitiveType(_primitiveType.PrimitiveTypeKind);
            }
            catch (ArgumentException)
            {
                AddError(
                    ErrorCode.InvalidPrimitiveTypeKind, EdmSchemaErrorSeverity.Error,
                    Strings.InvalidPrimitiveTypeKind(value));
            }
        }

        public override string Name
        {
            get { return _primitiveType.Name; }
            set { _primitiveType.Name = value; }
        }

        public PrimitiveType PrimitiveType
        {
            get { return _primitiveType; }
        }

        public IEnumerable<FacetDescription> FacetDescriptions
        {
            get
            {
                foreach (var element in _facetDescriptions)
                {
                    yield return element.FacetDescription;
                }
            }
        }

        internal override void ResolveTopLevelNames()
        {
            base.ResolveTopLevelNames();

            // Call validate on the facet descriptions
            foreach (var facetDescription in _facetDescriptions)
            {
                try
                {
                    facetDescription.CreateAndValidateFacetDescription(Name);
                }
                catch (ArgumentException e)
                {
                    AddError(
                        ErrorCode.InvalidFacetInProviderManifest,
                        EdmSchemaErrorSeverity.Error,
                        e.Message);
                }
            }
            // facet descriptions don't have any names to resolve
        }

        internal override void Validate()
        {
            base.Validate();

            if (!ValidateSufficientFacets())
            {
                // the next checks will fail, so get out
                // if we had errors
                return;
            }

            if (!ValidateInterFacetConsistency())
            {
                return;
            }
        }

        private bool ValidateInterFacetConsistency()
        {
            if (PrimitiveType.PrimitiveTypeKind
                == PrimitiveTypeKind.Decimal)
            {
                var precisionFacetDescription = Helper.GetFacet(FacetDescriptions, DbProviderManifest.PrecisionFacetName);
                var scaleFacetDescription = Helper.GetFacet(FacetDescriptions, DbProviderManifest.ScaleFacetName);

                if (precisionFacetDescription.MaxValue.Value
                    < scaleFacetDescription.MaxValue.Value)
                {
                    AddError(
                        ErrorCode.BadPrecisionAndScale,
                        EdmSchemaErrorSeverity.Error,
                        Strings.BadPrecisionAndScale(
                            precisionFacetDescription.MaxValue.Value,
                            scaleFacetDescription.MaxValue.Value));
                    return false;
                }
            }

            return true;
        }

        private bool ValidateSufficientFacets()
        {
            var baseType = _primitiveType.BaseType as PrimitiveType;
            // the base type will be an edm type
            // the edm type is the athority for which facets are required
            if (baseType == null)
            {
                // an error will already have been added for this
                return false;
            }

            var addedErrors = false;
            foreach (var systemFacetDescription in baseType.FacetDescriptions)
            {
                var providerFacetDescription = Helper.GetFacet(FacetDescriptions, systemFacetDescription.FacetName);
                if (providerFacetDescription == null)
                {
                    AddError(
                        ErrorCode.RequiredFacetMissing,
                        EdmSchemaErrorSeverity.Error,
                        Strings.MissingFacetDescription(
                            PrimitiveType.Name,
                            PrimitiveType.PrimitiveTypeKind,
                            systemFacetDescription.FacetName));
                    addedErrors = true;
                }
            }

            return !addedErrors;
        }
    }
}
