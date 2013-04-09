// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Serialization
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Xml;

    internal sealed class EdmSerializationVisitor : EdmModelVisitor
    {
        private readonly EdmXmlSchemaWriter _schemaWriter;

        public EdmSerializationVisitor(XmlWriter xmlWriter, double edmVersion, bool serializeDefaultNullability = false)
            : this(new EdmXmlSchemaWriter(xmlWriter, edmVersion, serializeDefaultNullability))
        {
        }

        public EdmSerializationVisitor(EdmXmlSchemaWriter schemaWriter)
        {
            DebugCheck.NotNull(schemaWriter);

            _schemaWriter = schemaWriter;
        }

        public void Visit(EdmModel edmModel)
        {
            DebugCheck.NotNull(edmModel);

            var namespaceName
                = edmModel
                    .NamespaceNames
                    .DefaultIfEmpty("Empty")
                    .Single();

            _schemaWriter.WriteSchemaElementHeader(namespaceName);

            VisitEdmModel(edmModel);

            _schemaWriter.WriteEndElement();
        }

        public void Visit(EdmModel edmModel, string provider, string providerManifestToken)
        {
            DebugCheck.NotNull(edmModel);
            DebugCheck.NotEmpty(provider);
            DebugCheck.NotEmpty(providerManifestToken);

            Visit(edmModel, edmModel.Containers.Single().Name + "Schema", provider, providerManifestToken);
        }

        public void Visit(EdmModel edmModel, string namespaceName, string provider, string providerManifestToken)
        {
            DebugCheck.NotNull(edmModel);
            DebugCheck.NotEmpty(namespaceName);
            DebugCheck.NotEmpty(provider);
            DebugCheck.NotEmpty(providerManifestToken);

            _schemaWriter.WriteSchemaElementHeader(namespaceName, provider, providerManifestToken);

            VisitEdmModel(edmModel);

            _schemaWriter.WriteEndElement();
        }

        protected override void VisitEdmEntityContainer(EntityContainer item)
        {
            _schemaWriter.WriteEntityContainerElementHeader(item);
            base.VisitEdmEntityContainer(item);
            _schemaWriter.WriteEndElement();
        }

        protected internal override void VisitEdmFunction(EdmFunction item)
        {
            _schemaWriter.WriteFunctionElementHeader(item);
            base.VisitEdmFunction(item);
            _schemaWriter.WriteEndElement();
        }

        protected internal override void VisitFunctionParameter(FunctionParameter functionParameter)
        {
            _schemaWriter.WriteFunctionParameterHeader(functionParameter);
            base.VisitFunctionParameter(functionParameter);
            _schemaWriter.WriteEndElement();
        }

        protected internal override void VisitFunctionReturnParameter(FunctionParameter returnParameter)
        {
            if (returnParameter.TypeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.RowType)
            {
                _schemaWriter.WriteFunctionReturnTypeElementHeader();
                _schemaWriter.WriteCollectionTypeElementHeader();
                base.VisitFunctionReturnParameter(returnParameter);
                _schemaWriter.WriteEndElement();
                _schemaWriter.WriteEndElement();
            }
            else
            {
                Debug.Assert(
                    returnParameter.TypeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType,
                    "Unsupported return parameter type");

                base.VisitFunctionReturnParameter(returnParameter);
            }
        }

        protected override void VisitEdmAssociationSet(AssociationSet item)
        {
            _schemaWriter.WriteAssociationSetElementHeader(item);
            base.VisitEdmAssociationSet(item);
            if (item.SourceSet != null)
            {
                _schemaWriter.WriteAssociationSetEndElement(item.SourceSet, item.SourceEnd.Name);
            }
            if (item.TargetSet != null)
            {
                _schemaWriter.WriteAssociationSetEndElement(item.TargetSet, item.TargetEnd.Name);
            }
            _schemaWriter.WriteEndElement();
        }

        protected internal override void VisitEdmEntitySet(EntitySet item)
        {
            _schemaWriter.WriteEntitySetElementHeader(item);
            _schemaWriter.WriteDefiningQuery(item);
            base.VisitEdmEntitySet(item);
            _schemaWriter.WriteEndElement();
        }

        protected internal override void VisitFunctionImport(EdmFunction functionImport)
        {
            _schemaWriter.WriteFunctionImportElementHeader(functionImport);
            base.VisitFunctionImport(functionImport);
            _schemaWriter.WriteEndElement();
        }

        protected internal override void VisitFunctionImportParameter(FunctionParameter parameter)
        {
            _schemaWriter.WriteFunctionImportParameterElementHeader(parameter);
            base.VisitFunctionImportParameter(parameter);
            _schemaWriter.WriteEndElement();
        }

        protected internal override void VisitFunctionImportReturnParameter(FunctionParameter parameter)
        {
            // function imports with multiple return types are currently not supported
            // for function with single return value the return type is being written inline
        }

        protected internal override void VisitRowType(RowType rowType)
        {
            _schemaWriter.WriteRowTypeElementHeader();
            base.VisitRowType(rowType);
            _schemaWriter.WriteEndElement();
        }

        protected internal override void VisitEdmEntityType(EntityType item)
        {
            var builder = new StringBuilder();

            AppendSchemaErrors(builder, item);

            if (MetadataItemHelper.IsInvalid(item))
            {
                AppendMetadataItem(builder, item, (v, i) => v.InternalVisitEdmEntityType(i));

                _schemaWriter.WriteComment(builder.ToString());
            }
            else
            {
                _schemaWriter.WriteComment(builder.ToString());

                InternalVisitEdmEntityType(item);
            }
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

        protected override void VisitKeyProperties(EntityType entityType, IList<EdmProperty> properties)
        {
            if (properties.Any())
            {
                _schemaWriter.WriteDelaredKeyPropertiesElementHeader();

                foreach (var keyProperty in properties)
                {
                    _schemaWriter.WriteDelaredKeyPropertyRefElement(keyProperty);
                }

                _schemaWriter.WriteEndElement();
            }
        }

        protected internal override void VisitEdmProperty(EdmProperty item)
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

        protected internal override void VisitEdmAssociationType(AssociationType item)
        {
            var builder = new StringBuilder();

            AppendSchemaErrors(builder, item);

            if (MetadataItemHelper.IsInvalid(item))
            {
                AppendMetadataItem(builder, item, (v, i) => v.InternalVisitEdmAssociationType(i));

                _schemaWriter.WriteComment(builder.ToString());
            }
            else
            {
                _schemaWriter.WriteComment(builder.ToString());

                InternalVisitEdmAssociationType(item);
            }
        }

        protected override void VisitEdmAssociationEnd(RelationshipEndMember item)
        {
            _schemaWriter.WriteAssociationEndElementHeader(item);
            if (item.DeleteBehavior != OperationAction.None)
            {
                _schemaWriter.WriteOperationActionElement(XmlConstants.OnDelete, item.DeleteBehavior);
            }
            VisitMetadataItem(item);
            _schemaWriter.WriteEndElement();
        }

        protected override void VisitEdmAssociationConstraint(ReferentialConstraint item)
        {
            _schemaWriter.WriteReferentialConstraintElementHeader();
            _schemaWriter.WriteReferentialConstraintRoleElement(
                XmlConstants.PrincipalRole, item.FromRole, item.FromProperties);
            _schemaWriter.WriteReferentialConstraintRoleElement(
                XmlConstants.DependentRole, item.ToRole, item.ToProperties);
            VisitMetadataItem(item);
            _schemaWriter.WriteEndElement();
        }

        private void InternalVisitEdmEntityType(EntityType item)
        {
            _schemaWriter.WriteEntityTypeElementHeader(item);
            base.VisitEdmEntityType(item);
            _schemaWriter.WriteEndElement();
        }

        private void InternalVisitEdmAssociationType(AssociationType item)
        {
            _schemaWriter.WriteAssociationTypeElementHeader(item);
            base.VisitEdmAssociationType(item);
            _schemaWriter.WriteEndElement();
        }

        private static void AppendSchemaErrors(StringBuilder builder, MetadataItem item)
        {
            if (MetadataItemHelper.HasSchemaErrors(item))
            {
                builder.AppendLine(Strings.MetadataItemErrorsFoundDuringGeneration);

                foreach (var error in MetadataItemHelper.GetSchemaErrors(item))
                {
                    builder.AppendLine(error.ToString());
                }
            }
        }

        private void AppendMetadataItem<T>(
            StringBuilder builder, T item, Action<EdmSerializationVisitor, T> visitAction) 
            where T : MetadataItem
        {
            var settings = new XmlWriterSettings { ConformanceLevel = ConformanceLevel.Fragment };

            using (var writer = XmlWriter.Create(builder, settings))
            {
                var visitor = new EdmSerializationVisitor(_schemaWriter.Replicate(writer));
                visitAction(visitor, item);
            }
        }
    }
}
