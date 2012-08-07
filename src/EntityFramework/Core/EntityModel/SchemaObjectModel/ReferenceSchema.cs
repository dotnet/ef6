// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.EntityModel.SchemaObjectModel
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Xml;

    /// <summary>
    ///     Summary description for UsingElement.
    /// </summary>
    internal class UsingElement : SchemaElement
    {
        #region Instance Fields

        #endregion

        #region Public Methods

        /// <summary>
        /// </summary>
        /// <param name="parentElement"> </param>
        internal UsingElement(Schema parentElement)
            : base(parentElement)
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// </summary>
        public virtual string Alias { get; private set; }

        /// <summary>
        /// </summary>
        public virtual string NamespaceName { get; private set; }

        /// <summary>
        /// </summary>
        public override string FQName
        {
            get { return null; }
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// </summary>
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

        /// <summary>
        /// </summary>
        /// <param name="reader"> </param>
        private void HandleNamespaceAttribute(XmlReader reader)
        {
            Debug.Assert(String.IsNullOrEmpty(NamespaceName), "Alias must be set only once");
            var returnValue = HandleDottedNameAttribute(reader, NamespaceName);
            if (returnValue.Succeeded)
            {
                NamespaceName = returnValue.Value;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="reader"> </param>
        private void HandleAliasAttribute(XmlReader reader)
        {
            Debug.Assert(String.IsNullOrEmpty(Alias), "Alias must be set only once");
            Alias = HandleUndottedNameAttribute(reader, Alias);
        }

        #endregion
    }
}
