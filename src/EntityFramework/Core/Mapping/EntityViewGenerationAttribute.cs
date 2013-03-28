// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     Attribute to mark the assemblies that contain the generated views type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class EntityViewGenerationAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Data.Entity.Core.Mapping.EntityViewGenerationAttribute" /> class.
        /// </summary>
        /// <param name="viewGenerationType">The view type.</param>
        public EntityViewGenerationAttribute(Type viewGenerationType)
        {
            Check.NotNull(viewGenerationType, "viewGenerationType");
            m_viewGenType = viewGenerationType;
        }

        private readonly Type m_viewGenType;

        /// <summary>Gets the T:System.Type of the view.</summary>
        /// <returns>The T:System.Type of the view.</returns>
        public Type ViewGenerationType
        {
            get { return m_viewGenType; }
        }
    }
}
