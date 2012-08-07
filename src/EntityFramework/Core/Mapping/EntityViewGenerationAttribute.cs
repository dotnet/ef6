// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     Attribute to mark the assemblies that contain the generated views type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class EntityViewGenerationAttribute : Attribute
    {
        #region Constructors

        /// <summary>
        ///     Constructor for EntityViewGenerationAttribute
        /// </summary>
        public EntityViewGenerationAttribute(Type viewGenerationType)
        {
            Contract.Requires(viewGenerationType != null);
            m_viewGenType = viewGenerationType;
        }

        #endregion

        #region Fields

        private readonly Type m_viewGenType;

        #endregion

        #region Properties

        public Type ViewGenerationType
        {
            get { return m_viewGenType; }
        }

        #endregion
    }
}
