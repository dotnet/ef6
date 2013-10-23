// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System.Collections.Generic;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class DeleteEnumTypeCommand : DeleteEFElementCommand
    {
        private readonly List<ConceptualProperty> _propertiesToResolve = new List<ConceptualProperty>();

        /// <summary>
        ///     Deletes the passed in EnumType
        /// </summary>
        /// <param name="complexType"></param>
        internal DeleteEnumTypeCommand(EnumType enumType)
            : base(enumType)
        {
            CommandValidation.ValidateEnumType(enumType);
        }

        /// <summary>
        ///     We override this method because we need to do some extra things before
        ///     the normal PreInvoke gets called and our antiDeps are removed
        /// </summary>
        /// <param name="cpc"></param>
        protected override void PreInvoke(CommandProcessorContext cpc)
        {
            // Unbind all ConceptualProperty that reference the enum type so they will not get deleted.
            foreach (var property in EFElement.GetAntiDependenciesOfType<ConceptualProperty>())
            {
                property.UnbindEnumType();
                ;
                _propertiesToResolve.Add(property);
            }

            base.PreInvoke(cpc);
        }

        protected override void PostInvoke(CommandProcessorContext cpc)
        {
            base.PostInvoke(cpc);

            foreach (var property in _propertiesToResolve)
            {
                property.State = EFElementState.Normalized;
                property.Resolve(property.Artifact.ArtifactSet);
            }
        }
    }
}
