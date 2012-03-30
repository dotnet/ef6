namespace System.Data.Entity.Edm.Serialization.Xml.Internal.Csdl
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Internal;
    using System.Data.Entity.Edm.Parsing.Xml.Internal.Csdl;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Xml;

    internal sealed class EdmModelCsdlSerializationVisitor : EdmModelVisitor
    {
        private readonly double _edmVersion;
        private readonly EdmModelCsdlSchemaWriter _schemaWriter;
        private EdmAssociationType _currentAssociationType;

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

        protected override void VisitEdmEntityContainer(EdmEntityContainer item)
        {
            _schemaWriter.WriteEntityContainerElementHeader(item);
            base.VisitEdmEntityContainer(item);
            _schemaWriter.WriteEndElement();
        }

        protected override void VisitEdmAssociationSet(EdmAssociationSet item)
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

        protected override void VisitEdmEntitySet(EdmEntitySet item)
        {
            _schemaWriter.WriteEntitySetElementHeader(item);
            base.VisitEdmEntitySet(item);
            _schemaWriter.WriteEndElement();
        }

        protected override void VisitEdmEntityType(EdmEntityType item)
        {
            _schemaWriter.WriteEntityTypeElementHeader(item);
            base.VisitEdmEntityType(item);
            _schemaWriter.WriteEndElement();
        }

        protected override void VisitEdmEnumType(EdmEnumType item)
        {
            _schemaWriter.WriteEnumTypeElementHeader(item);
            base.VisitEdmEnumType(item);
            _schemaWriter.WriteEndElement();
        }

        protected override void VisitEdmEnumTypeMember(EdmEnumTypeMember item)
        {
            _schemaWriter.WriteEnumTypeMemberElementHeader(item);
            base.VisitEdmEnumTypeMember(item);
            _schemaWriter.WriteEndElement();
        }

        protected override void VisitDeclaredKeyProperties(
            EdmEntityType entityType, IEnumerable<EdmProperty> properties)
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

        protected override void VisitEdmNavigationProperty(EdmNavigationProperty item)
        {
            _schemaWriter.WriteNavigationPropertyElementHeader(item);
            base.VisitEdmNavigationProperty(item);
            _schemaWriter.WriteEndElement();
        }

        protected override void VisitComplexType(EdmComplexType item)
        {
            _schemaWriter.WriteComplexTypeElementHeader(item);
            base.VisitComplexType(item);
            _schemaWriter.WriteEndElement();
        }

        protected override void VisitEdmAssociationType(EdmAssociationType item)
        {
            _currentAssociationType = item;
            _schemaWriter.WriteAssociationTypeElementHeader(item);
            base.VisitEdmAssociationType(item);
            _schemaWriter.WriteEndElement();
        }

        protected override void VisitEdmAssociationEnd(EdmAssociationEnd item)
        {
            _schemaWriter.WriteAssociationEndElementHeader(item);
            if (item.DeleteAction.HasValue
                && item.DeleteAction.Value != EdmOperationAction.None)
            {
                _schemaWriter.WriteOperationActionElement(CsdlConstants.Element_OnDelete, item.DeleteAction.Value);
            }
            VisitEdmNamedMetadataItem(item);
            _schemaWriter.WriteEndElement();
        }

        protected override void VisitEdmAssociationConstraint(EdmAssociationConstraint item)
        {
            _schemaWriter.WriteReferentialConstraintElementHeader();
            _schemaWriter.WriteReferentialConstraintRoleElement(
                CsdlConstants.Element_Principal,
                item.PrincipalEnd(_currentAssociationType),
                item.PrincipalEnd(_currentAssociationType).EntityType.GetValidKey());
            _schemaWriter.WriteReferentialConstraintRoleElement(
                CsdlConstants.Element_Dependent, item.DependentEnd, item.DependentProperties);
            VisitEdmMetadataItem(item);
            _schemaWriter.WriteEndElement();
        }
    }
}
