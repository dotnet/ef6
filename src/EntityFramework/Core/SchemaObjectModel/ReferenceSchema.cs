// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.SchemaObjectModel
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Xml;

    // <summary>
    // Summary description for UsingElement.
    // </summary>
    internal class UsingElement : SchemaElement
    {
        #region Instance Fields

        #endregion

        #region Public Methods

        internal UsingElement(Schema parentElement)
            : base(parentElement)
        {
        }

        #endregion

        #region Public Properties

        public virtual string Alias { get; private set; }

        public virtual string NamespaceName { get; private set; }

        public override string FQName
        {
            get { return null; }
        }

        #endregion

        #region Protected Properties

        protected override bool ProhibitAttribute(string namespaceUri, string localName)
        {
            if (base.ProhibitAttribute(namespaceUri, localName))
            {
                return true;
            }

            if (namespaceUri == null
                && localName == XmlConstants.Name)
            {
                return false;
            }
            return false;
        }

        protected override bool HandleAttribute(XmlReader reader)
        {
            if (base.HandleAttribute(reader))
            {
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.Namespace))
            {
                HandleNamespaceAttribute(reader);
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.Alias))
            {
                HandleAliasAttribute(reader);
                return true;
            }

            return false;
        }

        #endregion

        #region Private Methods

        private void HandleNamespaceAttribute(XmlReader reader)
        {
            Debug.Assert(String.IsNullOrEmpty(NamespaceName), "Alias must be set only once");
            var returnValue = HandleDottedNameAttribute(reader, NamespaceName);
            if (returnValue.Succeeded)
            {
                NamespaceName = returnValue.Value;
            }
        }

        private void HandleAliasAttribute(XmlReader reader)
        {
            Debug.Assert(String.IsNullOrEmpty(Alias), "Alias must be set only once");
            Alias = HandleUndottedNameAttribute(reader, Alias);
        }

        #endregion
    }
}
