// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System.Diagnostics;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class RenameConceptualAliasCommand : Command
    {
        private readonly ConceptualEntityModel _conceptualEntityModel;
        private readonly string _newAlias;

        internal RenameConceptualAliasCommand(ConceptualEntityModel conceptualEntityModel, string newAlias)
        {
            _conceptualEntityModel = conceptualEntityModel;
            _newAlias = newAlias;
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            if (null == _conceptualEntityModel)
            {
                Debug.Fail("Null ConceptualEntityModel");
                return;
            }

            var artifact = _conceptualEntityModel.Artifact;
            if (null == artifact)
            {
                Debug.Fail("Null Artifact");
                return;
            }

            var artifactSet = artifact.ArtifactSet;
            if (null == artifactSet)
            {
                Debug.Fail("Null ArtifactSet");
                return;
            }

            // check to see if the new alias is valid
            if (_conceptualEntityModel.Alias.IsValidValue(_newAlias))
            {
                var previousAlias = _conceptualEntityModel.Alias.Value;
                // its not an error not to have a previous Alias - if so just
                // update the attribute itself
                if (!string.IsNullOrEmpty(previousAlias))
                {
                    // lookup the namespace that the Alias maps to
                    var previousConceptualNamespace = _conceptualEntityModel.Namespace.Value;
                    if (string.IsNullOrEmpty(previousConceptualNamespace))
                    {
                        Debug.Fail("Null or empty conceptual namespace");
                        return;
                    }

                    // find all Symbols in the ArtifactSet which have first part
                    // equal to existing previousConceptualNamespace (if it was
                    // referred to in the XML as 'Alias.XXX' then it will be in
                    // the symbol table as 'Namespace.XXX')
                    var allElementsWithConceptualNamespaceSymbol =
                        artifactSet.GetElementsContainingFirstSymbolPart(previousConceptualNamespace);

                    // change all references which include the alias to the new alias
                    foreach (var element in allElementsWithConceptualNamespaceSymbol)
                    {
                        var itemBindings = element.GetDependentBindings();
                        foreach (var itemBinding in itemBindings)
                        {
                            if (null == itemBinding.GetParentOfType(typeof(ConceptualEntityModel)))
                            {
                                // itemBinding is not in the Conceptual Model - conceptual 
                                // Alias can only be referenced within the Conceptual Model
                                // so ignore this ItemBinding
                                continue;
                            }

                            itemBinding.UpdateRefNameAliases(previousAlias, _newAlias);
                        }
                    }
                }

                // now update the alias attribute itself
                _conceptualEntityModel.Alias.Value = _newAlias;

                // symbols need to be recalculated throughout the 
                // CSDL, bindings need to be rebound ...
                XmlModelHelper.NormalizeAndResolve(_conceptualEntityModel);
            }
            else
            {
                // if not a valid namespace, throw an error message
                var msg = string.Format(CultureInfo.CurrentCulture, Resources.INVALID_NC_NAME_CHAR, _newAlias);
                throw new CommandValidationFailedException(msg);
            }
        }
    }
}
