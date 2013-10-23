// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.Model.Validation;

    internal static class EFNormalizableItemDefaults
    {
        internal static NormalizedName DefaultNameNormalizerForEDM(EFElement parent, string refName)
        {
            var model = ModelHelper.GetBaseModelRoot(parent);
            NormalizedName normalizedName = null;
            if (model != null)
            {
                string potentialAliasOrNamespacePart = null;
                string nonAliasOrNamespacePart = null;
                SeparateRefNameIntoParts(refName, out potentialAliasOrNamespacePart, out nonAliasOrNamespacePart);

                // does the name start with the schema's namespace or alias?
                if (!string.IsNullOrEmpty(potentialAliasOrNamespacePart))
                {
                    refName = nonAliasOrNamespacePart;
                    var symbol = new Symbol(model.NamespaceValue, refName);

                    if (potentialAliasOrNamespacePart.Equals(model.NamespaceValue, StringComparison.CurrentCultureIgnoreCase))
                    {
                        // it starts with the namespace
                        normalizedName = new NormalizedName(symbol, null, model.NamespaceValue, refName);
                    }
                    else if (potentialAliasOrNamespacePart.Equals(model.AliasValue, StringComparison.CurrentCultureIgnoreCase))
                    {
                        // it starts with the alias
                        normalizedName = new NormalizedName(symbol, model.AliasValue, null, refName);
                    }
                }
                else
                {
                    // now, the name doesn't start with the alias or the namespace, so tack on the namespace
                    var symbol = new Symbol(model.NamespaceValue, refName);
                    normalizedName = new NormalizedName(symbol, null, null, refName);
                }
            }
            else
            {
                var symbol = new Symbol(refName);
                normalizedName = new NormalizedName(symbol, null, null, refName);
            }
            return normalizedName;
        }

        internal static NormalizedName DefaultNameNormalizerForMSL(EFElement parent, string refName)
        {
            var model = MappingModel.GetMappingRoot(parent);

            string potentialAliasOrNamespacePart = null;
            string nonAliasOrNamespacePart = null;
            SeparateRefNameIntoParts(refName, out potentialAliasOrNamespacePart, out nonAliasOrNamespacePart);

            Symbol symbol = null;
            NormalizedName normalizedName = null;

            if (!string.IsNullOrEmpty(potentialAliasOrNamespacePart))
            {
                // see if our type name starts with any of the defined aliases
                var startsWithAlias = false;

                foreach (var a in model.Aliases())
                {
                    if (potentialAliasOrNamespacePart.Equals(a.Key.Value, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (a.State == EFElementState.Parsed)
                        {
                            // alias is only in a parsed state, skip for now and don't add an error
                            // we'll end up looping around again later once its been resolved
                            return new NormalizedName(new Symbol(refName), null, null, refName);
                        }

                        if (a.State != EFElementState.Resolved
                            || a.Value.Status != BindingStatus.Known)
                        {
                            var msg = string.Format(CultureInfo.CurrentCulture, Resources.RESOLVE_UNRESOLVED_ALIAS, refName);
                            var artifactSet = parent.Artifact.ModelManager.GetArtifactSet(parent.Artifact.Uri);
                            var errorInfo = new ErrorInfo(
                                ErrorInfo.Severity.ERROR, msg, parent, ErrorCodes.RESOLVE_UNRESOLVED_ALIAS, ErrorClass.ResolveError);
                            artifactSet.AddError(errorInfo);
                            return new NormalizedName(new Symbol(String.Empty), null, null, refName);
                        }

                        // in the symbol replace alias with the full namespace
                        symbol = new Symbol(a.Value.Target.Namespace.Value, nonAliasOrNamespacePart);
                        normalizedName = new NormalizedName(symbol, a.Key.Value, null, nonAliasOrNamespacePart);

                        startsWithAlias = true;
                        break;
                    }
                }

                if (startsWithAlias == false
                    && model.Artifact.ConceptualModel() != null
                    && model.Artifact.StorageModel() != null)
                {
                    var conceptualNamespace = model.Artifact.ConceptualModel().Namespace.Value;
                    var storageNamespace = model.Artifact.StorageModel().Namespace.Value;
                    var currentNamespace = string.Empty;

                    var convertIt = false;

                    var startsWithConceptualNamespace = false;
                    var startsWithStorageNamespace = false;

                    if (potentialAliasOrNamespacePart.Equals(conceptualNamespace, StringComparison.CurrentCultureIgnoreCase))
                    {
                        startsWithConceptualNamespace = true;
                    }

                    if (potentialAliasOrNamespacePart.Equals(storageNamespace, StringComparison.CurrentCultureIgnoreCase))
                    {
                        startsWithStorageNamespace = true;
                    }

                    if (startsWithConceptualNamespace && startsWithStorageNamespace)
                    {
                        // in this case, the two namespaces start with the same thing and we got a match
                        // on both; for example if there is pubsModel & pubsModel.Store and we are checking
                        // on a string 'pubsModel.Store.Customer'; whichever is longer is the real one
                        if (conceptualNamespace.Length > storageNamespace.Length)
                        {
                            currentNamespace = conceptualNamespace;
                            convertIt = true;
                        }
                        else
                        {
                            currentNamespace = storageNamespace;
                            convertIt = true;
                        }
                    }
                    else if (startsWithConceptualNamespace)
                    {
                        currentNamespace = conceptualNamespace;
                        convertIt = true;
                    }
                    else if (startsWithStorageNamespace)
                    {
                        currentNamespace = storageNamespace;
                        convertIt = true;
                    }

                    if (convertIt)
                    {
                        // convert to our normalized name format
                        symbol = new Symbol(currentNamespace, nonAliasOrNamespacePart);
                        normalizedName = new NormalizedName(symbol, null, currentNamespace, nonAliasOrNamespacePart);
                    }
                }
            }

            if (symbol == null)
            {
                // either there was no Alias or Namespace part or it didn't
                // match any of the known Aliases or Namespaces
                symbol = new Symbol(refName);
                normalizedName = new NormalizedName(symbol, null, null, refName);
            }

            return normalizedName;
        }

        internal static NormalizedName DefaultNameNormalizerForDesigner(EFElement parent, string refName)
        {
            return DefaultNameNormalizerForEDM(parent.Artifact.ConceptualModel(), refName);
        }

        internal static void SeparateRefNameIntoParts(
            string refName, out string potentialAliasOrNamespacePart, out string nonAliasOrNamespacePart)
        {
            // Use runtime algorithm:
            // Assume that alias or namespace reference is up to _last_ index of 
            // Symbol.VALID_RUNTIME_SEPARATOR in refName. Check against all Aliases. 
            // If it matches use that Alias, otherwise check against all Namespaces.
            // If it matches use that. Otherwise define a symbol with no Alias or Namespace part.
            potentialAliasOrNamespacePart = null;
            nonAliasOrNamespacePart = refName;

            // if there is no Symbol.VALID_RUNTIME_SEPARATOR, then symbol has no Alias or Namespace part
            var indexOfLastSeparatorChar = refName.LastIndexOf(Symbol.VALID_RUNTIME_SEPARATOR);
            if (-1 < indexOfLastSeparatorChar)
            {
                potentialAliasOrNamespacePart = refName.Substring(0, indexOfLastSeparatorChar);
                if (indexOfLastSeparatorChar + 1 < refName.Length)
                {
                    nonAliasOrNamespacePart = refName.Substring(indexOfLastSeparatorChar + 1);
                }
                else
                {
                    // non-valid refName ending with Symbol.VALID_RUNTIME_SEPARATOR
                    nonAliasOrNamespacePart = string.Empty;
                    Debug.Fail("Non-valid refName (" + refName + ") ends with '" + Symbol.VALID_RUNTIME_SEPARATOR + "'");
                }
            }
        }
    }
}
