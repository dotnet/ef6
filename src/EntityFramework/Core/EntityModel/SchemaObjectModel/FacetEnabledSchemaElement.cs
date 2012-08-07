// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.EntityModel.SchemaObjectModel
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Xml;

    internal abstract class FacetEnabledSchemaElement : SchemaElement
    {
        protected SchemaType _type;
        protected string _unresolvedType;
        protected TypeUsageBuilder _typeUsageBuilder;

        #region Properties

        internal new Function ParentElement
        {
            get { return base.ParentElement as Function; }
        }

        internal SchemaType Type
        {
            get { return _type; }
        }

        internal virtual TypeUsage TypeUsage
        {
            get { return _typeUsageBuilder.TypeUsage; }
        }

        internal TypeUsageBuilder TypeUsageBuilder
        {
            get { return _typeUsageBuilder; }
        }

        internal bool HasUserDefinedFacets
        {
            get { return _typeUsageBuilder.HasUserDefinedFacets; }
        }

        internal string UnresolvedType
        {
            get { return _unresolvedType; }
            set { _unresolvedType = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        /// <param name="parentElement"> </param>
        internal FacetEnabledSchemaElement(Function parentElement)
            : base(parentElement)
        {
        }

        internal FacetEnabledSchemaElement(SchemaElement parentElement)
            : base(parentElement)
        {
        }

        internal override void ResolveTopLevelNames()
        {
            base.ResolveTopLevelNames();

            Debug.Assert(Type == null, "This must be resolved exactly once");

            if (Schema.ResolveTypeName(this, UnresolvedType, out _type))
            {
                if (Schema.DataModel == SchemaDataModelOption.ProviderManifestModel
                    && _typeUsageBuilder.HasUserDefinedFacets)
                {
                    var isInProviderManifest = Schema.DataModel == SchemaDataModelOption.ProviderManifestModel;
                    _typeUsageBuilder.ValidateAndSetTypeUsage((ScalarType)_type, !isInProviderManifest);
                }
            }
        }

        internal void ValidateAndSetTypeUsage(ScalarType scalar)
        {
            _typeUsageBuilder.ValidateAndSetTypeUsage(scalar, false);
        }

        internal void ValidateAndSetTypeUsage(EdmType edmType)
        {
            _typeUsageBuilder.ValidateAndSetTypeUsage(edmType, false);
        }

        #endregion

        protected override bool HandleAttribute(XmlReader reader)
        {
            if (base.HandleAttribute(reader))
            {
                return true;
            }
            else if (_typeUsageBuilder.HandleAttribute(reader))
            {
                return true;
            }

            return false;
        }
    }
}
