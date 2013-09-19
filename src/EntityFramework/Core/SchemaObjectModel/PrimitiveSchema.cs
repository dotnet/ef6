// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.SchemaObjectModel
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Linq;
    using System.Xml;

    /// <summary>
    /// The virtual schema for primitive data types
    /// </summary>
    internal class PrimitiveSchema : Schema
    {
        public PrimitiveSchema(SchemaManager schemaManager)
            : base(schemaManager)
        {
            Schema = this;

            var providerManifest = ProviderManifest;
            if (providerManifest == null)
            {
                AddError(
                    new EdmSchemaError(
                        Strings.FailedToRetrieveProviderManifest,
                        (int)ErrorCode.FailedToRetrieveProviderManifest,
                        EdmSchemaErrorSeverity.Error));
            }
            else
            {
                IList<PrimitiveType> primitiveTypes = providerManifest.GetStoreTypes();

                // EDM Spatial types are only available to V3 and above CSDL.
                if (schemaManager.DataModel == SchemaDataModelOption.EntityDataModel
                    &&
                    schemaManager.SchemaVersion < XmlConstants.EdmVersionForV3)
                {
                    primitiveTypes = primitiveTypes.Where(t => !Helper.IsSpatialType(t))
                                                   .ToList();
                }

                foreach (var entry in primitiveTypes)
                {
                    TryAddType(new ScalarType(this, entry.Name, entry), false /*doNotAddErrorForEmptyName*/);
                }
            }
        }

        /// <summary>
        /// Returns the alias that can be used for type in this
        /// Namespace instead of the entire namespace name
        /// </summary>
        internal override string Alias
        {
            get { return ProviderManifest.NamespaceName; }
        }

        /// <summary>
        /// Returns the TypeAuthority that is driving this schema
        /// </summary>
        internal override string Namespace
        {
            get
            {
                if (ProviderManifest != null)
                {
                    return ProviderManifest.NamespaceName;
                }
                return string.Empty;
            }
        }

        protected override bool HandleAttribute(XmlReader reader)
        {
            // don't call the base, we don't have any attributes
            return false;
        }
    }
}
