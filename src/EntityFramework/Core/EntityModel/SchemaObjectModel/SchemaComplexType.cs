// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.EntityModel.SchemaObjectModel
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Xml;

    /// <summary>
    /// Summary description for NestedType.
    /// </summary>
    internal sealed class SchemaComplexType : StructuredType
    {
        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentElement"></param>
        internal SchemaComplexType(Schema parentElement)
            : base(parentElement)
        {
            if (Schema.DataModel
                == SchemaDataModelOption.EntityDataModel)
            {
                OtherContent.Add(Schema.SchemaSource);
            }
        }

        #endregion

        #region Public Properties

        #endregion

        #region Protected Methods

        /// <summary>
        /// 
        /// </summary>
        internal override void ResolveTopLevelNames()
        {
            base.ResolveTopLevelNames();

            if (BaseType != null)
            {
                if (!(BaseType is SchemaComplexType))
                {
                    AddError(
                        ErrorCode.InvalidBaseType, EdmSchemaErrorSeverity.Error,
                        Strings.InvalidBaseTypeForNestedType(BaseType.FQName, FQName));
                }
            }
        }

        protected override bool HandleElement(XmlReader reader)
        {
            if (base.HandleElement(reader))
            {
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.ValueAnnotation))
            {
                // EF does not support this EDM 3.0 element, so ignore it.
                SkipElement(reader);
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.TypeAnnotation))
            {
                // EF does not support this EDM 3.0 element, so ignore it.
                SkipElement(reader);
                return true;
            }
            return false;
        }

        #endregion
    }
}
