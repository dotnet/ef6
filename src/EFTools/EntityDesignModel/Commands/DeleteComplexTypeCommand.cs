// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Tools.XmlDesignerBase;

    internal class DeleteComplexTypeCommand : DeleteEFElementCommand
    {
        private readonly List<ComplexConceptualProperty> _complexPropertiesToResolve = new List<ComplexConceptualProperty>();

        public DeleteComplexTypeCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
        }

        /// <summary>
        ///     Deletes the passed in ComplexType
        /// </summary>
        /// <param name="complexType"></param>
        internal DeleteComplexTypeCommand(ComplexType complexType)
            : base(complexType)
        {
            CommandValidation.ValidateComplexType(complexType);
        }

        /// <summary>
        ///     We override this method because we need to do some extra things before
        ///     the normal PreInvoke gets called and our antiDeps are removed
        /// </summary>
        /// <param name="cpc"></param>
        protected override void PreInvoke(CommandProcessorContext cpc)
        {
            // remove the type of all related complex properties (so they won't get deleted)
            foreach (var property in EFElement.GetAntiDependenciesOfType<ComplexConceptualProperty>())
            {
                foreach (var cp in property.GetAntiDependenciesOfType<ComplexProperty>())
                {
                    // also delete all related ComplexProperty mappings
                    DeleteInTransaction(cpc, cp);
                }

                // rebind property.ComplexType to what it is bound to now. This adds a change to that SingleItemBinding
                // to the list of actions that this command takes (but note that its RefValue does not change).
                // This is important so that if Undo is called on this command that SingleItemBinding will be rebound back
                // to the re-added ComplexType. It is likely that this command will also delete ComplexProperty mappings (see just above).
                // If an Undo happens the resolve step for the ScalarProperty children of those mappings will fail if at
                // that time the property.ComplexType SingleItemBinding is not resolved.
                if (property.ComplexType.Target != null)
                {
                    // have to set to null and then reset because just setting to the existing value is shortcircuited out
                    property.ComplexType.SetRefName(null);
                    property.ComplexType.SetRefName(property.ComplexType.Target);
                    property.ComplexType.Rebind();
                }

                // unbind the property.ComplexType but leave the reference pointing to the soon to be non-existent ComplexType
                // so that the error message tells the user what has happened (and what they can do to fix it)
                property.ComplexType.Unbind();
                _complexPropertiesToResolve.Add(property);
            }

            base.PreInvoke(cpc);
        }

        /// <summary>
        ///     We override this method to do some specialized processing of FunctionImport antiDeps
        /// </summary>
        /// <param name="cpc"></param>
        protected override void RemoveAntiDeps(CommandProcessorContext cpc)
        {
            var cModel = EFElement.RuntimeModelRoot() as ConceptualEntityModel;

            if (cModel != null)
            {
                // If there is a FunctionImport which returns a complex type, set the FunctionImport return type to null.
                foreach (var fi in EFElement.GetAntiDependenciesOfType<FunctionImport>())
                {
                    CommandProcessor.InvokeSingleCommand(
                        cpc, new ChangeFunctionImportCommand(
                            cModel.FirstEntityContainer as ConceptualEntityContainer,
                            fi, fi.Function, fi.DisplayName, fi.IsComposable.Value, true,
                            Resources.NoneDisplayValueUsedForUX));
                }
            }

            // process the remaining antiDeps normally
            base.RemoveAntiDeps(cpc);
        }

        protected override void PostInvoke(CommandProcessorContext cpc)
        {
            base.PostInvoke(cpc);

            foreach (var complexProperty in _complexPropertiesToResolve)
            {
                complexProperty.State = EFElementState.Normalized;
                complexProperty.Resolve(complexProperty.Artifact.ArtifactSet);
            }
        }
    }
}
