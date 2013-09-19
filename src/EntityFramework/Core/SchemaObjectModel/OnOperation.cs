// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.SchemaObjectModel
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Xml;

    /// <summary>
    /// Represents an OnDelete, OnCopy, OnSecure, OnLock or OnSerialize element
    /// </summary>
    internal sealed class OnOperation : SchemaElement
    {
        public OnOperation(RelationshipEnd parentElement, Operation operation)
            : base(parentElement)
        {
            Operation = operation;
        }

        /// <summary>
        /// The operation
        /// </summary>
        public Operation Operation { get; private set; }

        /// <summary>
        /// The action
        /// </summary>
        public Action Action { get; private set; }

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
            else if (CanHandleAttribute(reader, XmlConstants.Action))
            {
                HandleActionAttribute(reader);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Handle the Action attribute
        /// </summary>
        /// <param name="reader"> reader positioned at Action attribute </param>
        private void HandleActionAttribute(XmlReader reader)
        {
            DebugCheck.NotNull(reader);

            var relationshipKind = ParentElement.ParentElement.RelationshipKind;

            switch (reader.Value.Trim())
            {
                case "None":
                    Action = Action.None;
                    break;
                case "Cascade":
                    Action = Action.Cascade;
                    break;
                default:
                    AddError(
                        ErrorCode.InvalidAction, EdmSchemaErrorSeverity.Error, reader,
                        Strings.InvalidAction(reader.Value, ParentElement.FQName));
                    break;
            }
        }

        /// <summary>
        /// the parent element.
        /// </summary>
        private new RelationshipEnd ParentElement
        {
            get { return (RelationshipEnd)base.ParentElement; }
        }
    }
}
