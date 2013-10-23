// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System.Diagnostics;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Validation;

    internal class RenameConceptualNamespaceCommand : Command
    {
        private readonly ConceptualEntityModel _conceptualEntityModel;
        private readonly string _newNamespace;

        internal RenameConceptualNamespaceCommand(ConceptualEntityModel conceptualEntityModel, string newNamespace)
        {
            _conceptualEntityModel = conceptualEntityModel;
            _newNamespace = newNamespace;
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

            // make sure this name doesn't conflict with an EntityContainer name
            foreach (var bec in _conceptualEntityModel.EntityContainers())
            {
                if (_newNamespace == bec.LocalName.Value)
                {
                    var msg = string.Format(
                        CultureInfo.CurrentCulture, Resources.EntityContainerNameConflictsWithNamespaceName, _newNamespace);
                    throw new CommandValidationFailedException(msg);
                }
            }

            // check to see if the new namespace is valid
            if (EscherAttributeContentValidator.GetInstance(artifact.SchemaVersion)
                .IsValidAttributeValue(_newNamespace, _conceptualEntityModel.Namespace))
            {
                var previousConceptualNamespace = _conceptualEntityModel.Namespace.Value;
                if (string.IsNullOrEmpty(previousConceptualNamespace))
                {
                    Debug.Fail("Null or empty conceptual namespace");
                    return;
                }

                // find all Symbols in the ArtifactSet which have first part
                // equal to existing previousConceptualNamespace
                var allElementsWithConceptualNamespaceSymbol =
                    artifactSet.GetElementsContainingFirstSymbolPart(previousConceptualNamespace);

                // change all references which include the namespace to the new namespace
                foreach (var element in allElementsWithConceptualNamespaceSymbol)
                {
                    var itemBindings = element.GetDependentBindings();
                    foreach (var itemBinding in itemBindings)
                    {
                        itemBinding.UpdateRefNameNamespaces(
                            previousConceptualNamespace, _newNamespace);
                    }
                }

                // now update the namespace attribute itself
                _conceptualEntityModel.Namespace.Value = _newNamespace;

                // symbols need to be recalculated throughout the 
                // CSDL, MSL & DesignerInfo sections, and bindings need to be rebound ...
                XmlModelHelper.NormalizeAndResolve(_conceptualEntityModel);
                if (artifact.MappingModel() != null)
                {
                    XmlModelHelper.NormalizeAndResolve(artifact.MappingModel());
                }
                if (artifact.DesignerInfo() != null)
                {
                    XmlModelHelper.NormalizeAndResolve(artifact.DesignerInfo());
                }
            }
            else
            {
                // if not a valid namespace, throw an error message
                var msg = string.Format(CultureInfo.CurrentCulture, Resources.InvalidNamespaceName, _newNamespace);
                throw new CommandValidationFailedException(msg);
            }
        }
    }
}
