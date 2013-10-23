// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    internal static class ParameterNameNormalizer
    {
        internal static NormalizedName NameNormalizer(EFElement parent, string refName)
        {
            Debug.Assert(parent != null, "parent should not be null");

            if (refName == null)
            {
                return null;
            }

            Symbol symbol = null;

            var fi = parent as FunctionImport;
            var func = parent as Function;
            var fsp = parent as FunctionScalarProperty;
            var modFunc = parent as ModificationFunction;

            if (fi != null)
            {
                // if we are in the CSDL, build it's normalized name based on the EC name
                // FunctionImports live in the entity container
                var ec = fi.Parent as BaseEntityContainer;
                if (ec != null)
                {
                    symbol = new Symbol(ec.EntityContainerName, fi.LocalName.Value, refName);
                }
            }
            else if (func != null)
            {
                // if we are in the SSDL, then build the name based on the namespace
                // Functions are top level types like EntityType and Association
                symbol = new Symbol(((BaseEntityModel)func.Parent).NamespaceValue, func.LocalName.Value, refName);
            }
            else if (fsp != null)
            {
                // this FunctionScalarProperty could be right under the function, nested inside an AssociationEnd, 
                // or N levels deep inside a complex type hierarchy
                var mod = fsp.GetParentOfType(typeof(ModificationFunction)) as ModificationFunction;
                Debug.Assert(mod != null, "Failed to get a pointer to the ModificationFunction");

                if (mod != null)
                {
                    // there is a reference to this param in a function mapping, these always
                    // point back to a parameter in a function, i.e., something in the SSDL
                    symbol = GetSymbolBasedOnModificationFunction(parent.Artifact.ArtifactSet, mod, refName);
                }
            }
            else if (null != modFunc)
            {
                symbol = GetSymbolBasedOnModificationFunction(parent.Artifact.ArtifactSet, modFunc, refName);
            }

            if (symbol == null)
            {
                symbol = new Symbol(refName);
            }

            var normalizedName = new NormalizedName(symbol, null, null, refName);
            return normalizedName;
        }

        private static Symbol GetSymbolBasedOnModificationFunction(
            EFArtifactSet artifactSet, ModificationFunction mod, string refName)
        {
            Debug.Assert(mod != null, "GetSymbolBasedOnModificationFunction passed a null ModificationFunction");

            Symbol symbol = null;
            if (mod.FunctionName.Status == BindingStatus.Known)
            {
                symbol = new Symbol(mod.FunctionName.Target.NormalizedName, refName);

                Debug.Assert(artifactSet != null, "GetSymbolBasedOnModificationFunction passed a null EFArtifactSet");
                if (null != artifactSet)
                {
                    var item = artifactSet.LookupSymbol(symbol);
                    if (item == null)
                    {
                        symbol = null;
                    }
                }
            }

            return symbol;
        }
    }
}
