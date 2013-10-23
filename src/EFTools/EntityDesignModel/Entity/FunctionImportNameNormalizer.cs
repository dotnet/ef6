// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    internal static class FunctionImportNameNormalizer
    {
        internal static NormalizedName NameNormalizer(EFElement parent, string refName)
        {
            Debug.Assert(parent != null, "parent should not be null");

            if (refName == null)
            {
                return null;
            }

            var entityContainerName = string.Empty;

            var parentFunctionImport = parent as FunctionImport;
            var parentFunctionImportMapping = parent as FunctionImportMapping;

            Symbol symbol = null;

            // are we trying to normalize the name of actual FunctionImport in the EC?
            if (parentFunctionImport != null)
            {
                var ec = parentFunctionImport.Parent as BaseEntityContainer;
                if (ec != null)
                {
                    entityContainerName = ec.EntityContainerName;
                }
            }
            else if (parentFunctionImportMapping != null)
            {
                var ecm = parentFunctionImportMapping.Parent as EntityContainerMapping;
                if (ecm != null)
                {
                    entityContainerName = ecm.CdmEntityContainer.RefName;
                }
            }

            if (!string.IsNullOrEmpty(entityContainerName))
            {
                symbol = new Symbol(entityContainerName, refName);
            }

            if (symbol == null)
            {
                symbol = new Symbol(refName);
            }

            var normalizedName = new NormalizedName(symbol, null, null, refName);
            return normalizedName;
        }
    }
}
