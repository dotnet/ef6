// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Serialization.Xml.Internal.Csdl
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm.Internal;
    using System.Data.Entity.Edm.Parsing.Xml.Internal.Csdl;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Xml;

    internal sealed class EdmModelCsdlSerializationVisitor : EdmModelVisitor
    {
        private readonly double _edmVersion;
        private readonly EdmModelCsdlSchemaWriter _schemaWriter;
        private AssociationType _currentAssociationType;

        internal EdmModelCsdlSerializationVisitor(XmlWriter xmlWriter, double edmVersion)
        {
            _edmVersion = edmVersion;
            _schemaWriter = new EdmModelCsdlSchemaWriter(xmlWriter, _edmVersion);
        }

        internal void Visit(EdmModel edmModel)
        {
            Contract.Assert(edmModel.Namespaces.Count == 1, "Expected exactly 1 namespace");

            var namespaceName = edmModel.Namespaces.First().Name;

            _schemaWriter.WriteSchemaElementHeader(namespaceName);

            base.VisitEdmModel(edmModel);

            _schemaWriter.WriteEndElement();
        }

        protected override void VisitEdmEntityContainer(EntityContainer item)
        {
            _schemaWriter.WriteEntityContainerElementHeader(item);
            base.VisitEdmEntityContainer(item);
            _schemaWriter.WriteEndElement();
        }

        protected override void VisitEdmAssociationSet(AssociationSet item)
        {
            _schemaWriter.WriteAssociationSetElementHeader(item);
            base.VisitEdmAssociationSet(item);
            if (item.SourceSet != null)
            {
                _schemaWriter.WriteAssociationSetEndElement(item.SourceSet, item.ElementType.SourceEnd.Name);
            }
            if (item.TargetSet != null)
            {
                _schemaWriter.WriteAssociationSetEndElement(item.TargetSet, item.ElementType.TargetEnd.Name);
            }
            _schemaWriter.WriteEndElement();
        }

        protected override void VisitEdmEntitySet(EntitySet item)
        {
            _schemaWriter.WriteEntitySetElementHeader(item);
            base.VisitEdmEntitySet(item);
            _schemaWriter.WriteEndElement();
        }

        protected override void VisitEdmEntityType(EntityType item)
        {
            _schemaWriter.WriteEntityTypeElementHeader(item);
            base.VisitEdmEntityType(item);
            _schemaWriter.WriteEndElement();
        }

        protected override void VisitEdmEnumType(EnumType item)
        {
            _schemaWriter.WriteEnumTypeElementHeader(item);
            base.VisitEdmEnumType(item);
            _schemaWriter.WriteEndElement();
        }

        protected override void VisitEdmEnumTypeMember(EnumMember item)
        {
            _schemaWriter.WriteEnumTypeMemberElementHeader(item);
            base.VisitEdmEnumTypeMember(item);
            _schemaWriter.WriteEndElement();
        }

        protected override void VisitDeclaredKeyProperties(
            EntityType entityType, IEnumerable<EdmProperty> properties)
        {
            if (properties.Count() > 0)
            {
                _schemaWriter.WriteDelaredKeyPropertiesElementHeader();
                foreach (var keyProperty in properties)
                {
                    _schemaWriter.WriteDelaredKeyPropertyRefElement(keyProperty);
                }
                _schemaWriter.WriteEndElement();
            }
        }

        protected override void VisitEdmProperty(EdmProperty item)
        {
            _schemaWriter.WritePropertyElementHeader(item);
            base.VisitEdmProperty(item);
            _schemaWriter.WriteEndElement();
        }

        protected override void VisitEdmNavigationProperty(NavigationProperty item)
        {
            _schemaWriter.WriteNavigationPropertyElementHeader(item);
            base.VisitEdmNavigationProperty(item);
            _schemaWriter.WriteEndElement();
        }

        protected override void VisitComplexType(ComplexType item)
        {
            _schemaWriter.WriteComplexTypeElementHeader(item);
            base.VisitComplexType(item);
            _schemaWriter.WriteEndElement();
        }

        protected override void VisitEdmAssociationType(AssociationType item)
        {
            _currentAssociationType = item;
            _schemaWriter.WriteAssociationTypeElementHeader(item);
            base.VisitEdmAssociationType(item);
            _schemaWriter.WriteEndElement();
        }

        protected override void VisitEdmAssociationEnd(AssociationEndMember item)
        {
            _schemaWriter.WriteAssociationEndElementHeader(item);
            if (item.DeleteBehavior
                != OperationAction.None)
            {
                _schemaWriter.WriteOperationActionElement(CsdlConstants.Element_OnDelete, item.DeleteBehavior);
            }
            VisitMetadataItem(item);
            _schemaWriter.WriteEndElement();
        }

        protected override void VisitEdmAssociationConstraint(ReferentialConstraint item)
        {
            _schemaWriter.WriteReferentialConstraintElementHeader();
            _schemaWriter.WriteReferentialConstraintRoleElement(
                CsdlConstants.Element_Principal,
                item.PrincipalEnd(_currentAssociationType),
                item.PrincipalEnd(_currentAssociationType).GetEntityType().GetValidKey());
            _schemaWriter.WriteReferentialConstraintRoleElement(
                CsdlConstants.Element_Dependent, item.DependentEnd, item.ToProperties);
            VisitMetadataItem(item);
            _schemaWriter.WriteEndElement();
        }
    }
}
