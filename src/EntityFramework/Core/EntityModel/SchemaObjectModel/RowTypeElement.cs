// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.EntityModel.SchemaObjectModel
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Text;
    using System.Xml;
    using Som = System.Data.Entity.Core.EntityModel.SchemaObjectModel;

    internal class RowTypeElement : ModelFunctionTypeElement
    {
        private readonly SchemaElementLookUpTable<RowTypePropertyElement> _properties =
            new SchemaElementLookUpTable<RowTypePropertyElement>();

        #region constructor

        internal RowTypeElement(SchemaElement parentElement)
            : base(parentElement)
        {
        }

        protected override bool HandleElement(XmlReader reader)
        {
            if (CanHandleElement(reader, XmlConstants.Property))
            {
                HandlePropertyElement(reader);
                return true;
            }
            return false;
        }

        protected void HandlePropertyElement(XmlReader reader)
        {
            DebugCheck.NotNull(reader);

            var property = new RowTypePropertyElement(this);
            property.Parse(reader);
            _properties.Add(property, true, Strings.DuplicateEntityContainerMemberName);
        }

        #endregion

        internal SchemaElementLookUpTable<RowTypePropertyElement> Properties
        {
            get { return _properties; }
        }

        internal override void ResolveTopLevelNames()
        {
            foreach (var property in _properties)
            {
                property.ResolveTopLevelNames();
            }
        }

        internal override void WriteIdentity(StringBuilder builder)
        {
            builder.Append("Row[");

            var first = true;
            foreach (var property in _properties)
            {
                if (first)
                {
                    first = !first;
                }
                else
                {
                    builder.Append(", ");
                }
                property.WriteIdentity(builder);
            }
            builder.Append("]");
        }

        internal override TypeUsage GetTypeUsage()
        {
            if (_typeUsage == null)
            {
                var listOfProperties = new List<EdmProperty>();
                foreach (var property in _properties)
                {
                    var edmProperty = new EdmProperty(property.FQName, property.GetTypeUsage());
                    edmProperty.AddMetadataProperties(property.OtherContent);
                    //edmProperty.DeclaringType
                    listOfProperties.Add(edmProperty);
                }

                var rowType = new RowType(listOfProperties);
                if (Schema.DataModel
                    == SchemaDataModelOption.EntityDataModel)
                {
                    rowType.DataSpace = DataSpace.CSpace;
                }
                else
                {
                    Debug.Assert(
                        Schema.DataModel == SchemaDataModelOption.ProviderDataModel,
                        "Only DataModel == SchemaDataModelOption.ProviderDataModel is expected");
                    rowType.DataSpace = DataSpace.SSpace;
                }

                rowType.AddMetadataProperties(OtherContent);
                _typeUsage = TypeUsage.Create(rowType);
            }
            return _typeUsage;
        }

        internal override bool ResolveNameAndSetTypeUsage(
            Converter.ConversionCache convertedItemCache, Dictionary<SchemaElement, GlobalItem> newGlobalItems)
        {
            var result = true;
            if (_typeUsage == null)
            {
                foreach (var property in _properties)
                {
                    if (!property.ResolveNameAndSetTypeUsage(convertedItemCache, newGlobalItems))
                    {
                        result = false;
                    }
                }
            }
            return result;
        }

        internal override void Validate()
        {
            foreach (var property in _properties)
            {
                property.Validate();
            }

            if (_properties.Count == 0)
            {
                AddError(ErrorCode.RowTypeWithoutProperty, EdmSchemaErrorSeverity.Error, Strings.RowTypeWithoutProperty);
            }
        }
    }
}
