// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.SchemaObjectModel
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Text;
    using System.Xml;

    internal class TypeRefElement : ModelFunctionTypeElement
    {
        #region constructor

        internal TypeRefElement(SchemaElement parentElement)
            : base(parentElement)
        {
        }

        #endregion

        protected override bool HandleAttribute(XmlReader reader)
        {
            if (base.HandleAttribute(reader))
            {
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.TypeElement))
            {
                HandleTypeAttribute(reader);
                return true;
            }

            return false;
        }

        protected void HandleTypeAttribute(XmlReader reader)
        {
            DebugCheck.NotNull(reader);

            string type;
            if (!Utils.GetString(Schema, reader, out type))
            {
                return;
            }

            if (!Utils.ValidateDottedName(Schema, reader, type))
            {
                return;
            }

            _unresolvedType = type;
        }

        internal override bool ResolveNameAndSetTypeUsage(
            Converter.ConversionCache convertedItemCache, Dictionary<SchemaElement, GlobalItem> newGlobalItems)
        {
            if (_type is ScalarType) //Create and store type usage for scalar type
            {
                _typeUsageBuilder.ValidateAndSetTypeUsage(_type as ScalarType, false);
                _typeUsage = _typeUsageBuilder.TypeUsage;
                return true;
            }
            else //Try to resolve edm type. If not now, it will resolve in the second pass
            {
                var edmType = (EdmType)Converter.LoadSchemaElement(_type, _type.Schema.ProviderManifest, convertedItemCache, newGlobalItems);
                if (edmType != null)
                {
                    _typeUsageBuilder.ValidateAndSetTypeUsage(edmType, false); //use typeusagebuilder so dont lose facet information
                    _typeUsage = _typeUsageBuilder.TypeUsage;
                }

                return _typeUsage != null;
            }
        }

        internal override void WriteIdentity(StringBuilder builder)
        {
            Debug.Assert(UnresolvedType != null && !UnresolvedType.Trim().Equals(String.Empty));
            builder.Append(UnresolvedType);
        }

        internal override TypeUsage GetTypeUsage()
        {
            Debug.Assert(_typeUsage != null);
            return _typeUsage;
        }

        internal override void Validate()
        {
            base.Validate();

            ValidationHelper.ValidateFacets(this, _type, _typeUsageBuilder);
        }
    }
}
