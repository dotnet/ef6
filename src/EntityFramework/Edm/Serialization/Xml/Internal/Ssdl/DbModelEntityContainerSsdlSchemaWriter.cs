// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Edm.Serialization.Xml.Internal.Ssdl
{
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Parsing.Xml.Internal.Ssdl;
    using System.Xml;

    internal class DbModelEntityContainerSsdlSchemaWriter : XmlSchemaWriter
    {
        public const string ContainerSuffix = "Container";

        internal DbModelEntityContainerSsdlSchemaWriter(XmlWriter xmlWriter)
        {
            _xmlWriter = xmlWriter;
        }

        internal void WriteEntityContainerElementHeader(string containerName)
        {
            _xmlWriter.WriteStartElement(SsdlConstants.Element_EntityContainer);
            _xmlWriter.WriteAttributeString(SsdlConstants.Attribute_Name, containerName);
        }

        internal void WriteAssociationSetElementHeader(DbForeignKeyConstraintMetadata constraint)
        {
            _xmlWriter.WriteStartElement(SsdlConstants.Element_AssociationSet);
            _xmlWriter.WriteAttributeString(SsdlConstants.Attribute_Name, constraint.Name);
            _xmlWriter.WriteAttributeString(
                SsdlConstants.Attribute_Association, GetQualifiedTypeName(SsdlConstants.Value_Self, constraint.Name));
        }

        internal void WriteAssociationSetEndElement(DbTableMetadata end, string roleName)
        {
            _xmlWriter.WriteStartElement(SsdlConstants.Element_End);
            _xmlWriter.WriteAttributeString(SsdlConstants.Attribute_Role, roleName);
            _xmlWriter.WriteAttributeString(SsdlConstants.Attribute_EntitySet, end.Name);
            _xmlWriter.WriteEndElement();
        }

        internal void WriteEntitySetElementHeader(DbSchemaMetadata containingSchema, DbTableMetadata entitySet)
        {
            _xmlWriter.WriteStartElement(SsdlConstants.Element_EntitySet);
            _xmlWriter.WriteAttributeString(SsdlConstants.Attribute_Name, entitySet.Name);
            _xmlWriter.WriteAttributeString(
                SsdlConstants.Attribute_EntityType, GetQualifiedTypeName(SsdlConstants.Value_Self, entitySet.Name));
            if (containingSchema.DatabaseIdentifier != null)
            {
                _xmlWriter.WriteAttributeString(SsdlConstants.Attribute_Schema, containingSchema.DatabaseIdentifier);
            }
            if (entitySet.DatabaseIdentifier != null)
            {
                _xmlWriter.WriteAttributeString(SsdlConstants.Attribute_Table, entitySet.DatabaseIdentifier);
            }
        }
    }
}
