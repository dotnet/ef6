// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Model = Microsoft.Data.Entity.Design.Model.Entity;

namespace Microsoft.Data.Entity.Design.EntityDesigner.ViewModel
{
    using System;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.VisualStudio.Modeling;

    /// <summary>
    ///     DSL allows us to override the functionality that picks a unique name for a new, nameable domain object.
    /// </summary>
    internal class NameableItemNameProvider : ElementNameProvider
    {
        public override void SetUniqueName(ModelElement element, ModelElement container, DomainRoleInfo embeddedDomainRole, string baseName)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            if (baseName == null)
            {
                throw new ArgumentNullException("baseName");
            }
            if (embeddedDomainRole == null)
            {
                throw new ArgumentNullException("embeddedDomainRole");
            }

            Debug.Assert(DomainProperty.PropertyType == typeof(string), "Why isn't the inherent domain property type a string?");

            // we want to make sure that we pick a unique name for the property that does not conflict with any other property name in the owning
            // entity type's inheritance tree (if there is one).
            var property = element as Property;
            if (property != null
                && property.EntityType != null)
            {
                var viewModel = property.EntityType.EntityDesignerViewModel;
                if (viewModel.ModelXRef != null)
                {
                    // use the xref to obtain the EntityType EFObject that represents this view model element in order to query it
                    var modelEntityType = viewModel.ModelXRef.GetExisting(property.EntityType) as ConceptualEntityType;
                    Debug.Assert(modelEntityType != null, "Where is the model EFObject associated with this view model element?");
                    if (modelEntityType != null)
                    {
                        baseName = property is ScalarProperty
                                       ? Model.Entity.Property.DefaultPropertyName
                                       : ComplexConceptualProperty.DefaultComplexPropertyName;

                        property.Name = ModelHelper.GetUniqueConceptualPropertyName(
                            propertyNameCandidate: baseName,
                            entityType: modelEntityType,
                            alwaysAddSuffix: true);
                        return;
                    }
                }
            }

            base.SetUniqueName(element, container, embeddedDomainRole, baseName);
        }
    }
}
