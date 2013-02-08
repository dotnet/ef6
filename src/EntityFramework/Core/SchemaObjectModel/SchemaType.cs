// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.SchemaObjectModel
{
    /// <summary>
    ///     Summary description for SchemaType.
    /// </summary>
    internal abstract class SchemaType : SchemaElement
    {
        #region Public Properties

        /// <summary>
        ///     Gets the Namespace that this type is in.
        /// </summary>
        /// <value> </value>
        public string Namespace
        {
            get { return Schema.Namespace; }
        }

        /// <summary>
        /// </summary>
        public override string Identity
        {
            get { return Namespace + "." + Name; }
        }

        /// <summary>
        /// </summary>
        public override string FQName
        {
            get { return Namespace + "." + Name; }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// </summary>
        /// <param name="parentElement"> </param>
        internal SchemaType(Schema parentElement)
            : base(parentElement)
        {
        }

        #endregion
    }
}
