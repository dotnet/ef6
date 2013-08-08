// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.SchemaObjectModel
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Xml;

    /// <summary>
    /// class representing the Schema element in the schema
    /// </summary>
    internal sealed class ModelFunction : Function
    {
        private readonly TypeUsageBuilder _typeUsageBuilder;

        #region Public Methods

        /// <summary>
        /// ctor for a schema function
        /// </summary>
        public ModelFunction(Schema parentElement)
            :
                base(parentElement)
        {
            _isComposable = true;
            _typeUsageBuilder = new TypeUsageBuilder(this);
        }

        #endregion

        public override SchemaType Type
        {
            get { return _type; }
        }

        internal TypeUsage TypeUsage
        {
            get
            {
                if (_typeUsageBuilder.TypeUsage == null)
                {
                    return null;
                }
                else if (CollectionKind != CollectionKind.None)
                {
                    return TypeUsage.Create(new CollectionType(_typeUsageBuilder.TypeUsage));
                }
                else
                {
                    return _typeUsageBuilder.TypeUsage;
                }
            }
        }

        internal void ValidateAndSetTypeUsage(ScalarType scalar)
        {
            _typeUsageBuilder.ValidateAndSetTypeUsage(scalar, false);
        }

        internal void ValidateAndSetTypeUsage(EdmType edmType)
        {
            _typeUsageBuilder.ValidateAndSetTypeUsage(edmType, false);
        }

        #region Protected Properties

        protected override bool HandleElement(XmlReader reader)
        {
            if (base.HandleElement(reader))
            {
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.DefiningExpression))
            {
                HandleDefiningExpressionElment(reader);
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.Parameter))
            {
                HandleParameterElement(reader);
                return true;
            }

            return false;
        }

        protected override void HandleReturnTypeAttribute(XmlReader reader)
        {
            base.HandleReturnTypeAttribute(reader);
            _isComposable = true;
        }

        protected override bool HandleAttribute(XmlReader reader)
        {
            if (base.HandleAttribute(reader))
            {
                return true;
            }
            else if (_typeUsageBuilder.HandleAttribute(reader))
            {
                return true;
            }

            return false;
        }

        internal override void ResolveTopLevelNames()
        {
            if (null != UnresolvedReturnType)
            {
                if (Schema.ResolveTypeName(this, UnresolvedReturnType, out _type))
                {
                    if (_type is ScalarType)
                    {
                        _typeUsageBuilder.ValidateAndSetTypeUsage(_type as ScalarType, false);
                    }
                }
            }

            foreach (var parameter in Parameters)
            {
                parameter.ResolveTopLevelNames();
            }

            if (ReturnTypeList != null)
            {
                Debug.Assert(
                    ReturnTypeList.Count == 1,
                    "returnTypeList should always be non-empty.  Multiple ReturnTypes are only possible on FunctionImports.");
                ReturnTypeList[0].ResolveTopLevelNames();
            }
        }

        #endregion

        private void HandleDefiningExpressionElment(XmlReader reader)
        {
            DebugCheck.NotNull(reader);

            var commandText = new FunctionCommandText(this);
            commandText.Parse(reader);
            _commandText = commandText;
        }

        internal override void Validate()
        {
            base.Validate();

            ValidationHelper.ValidateFacets(this, _type, _typeUsageBuilder);

            if (_isRefType)
            {
                ValidationHelper.ValidateRefType(this, _type);
            }
        }
    }
}
