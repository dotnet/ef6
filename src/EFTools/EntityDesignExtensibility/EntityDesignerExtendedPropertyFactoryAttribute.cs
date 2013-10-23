// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Extensibility
{
    using System;
    using System.ComponentModel.Composition;

    /// <summary>
    ///     Specifies objects in the Entity Data Model Designer or the Model Browser that, when selected by a user, cause the
    ///     <see
    ///         cref="M:Microsoft.Data.Entity.Design.Extensibility.IEntityDesignerExtendedProperty.CreateProperty(System.Xml.Linq.XElement,Microsoft.Data.Entity.Design.Extensibility.PropertyExtensionContext)" />
    ///     method of the annotated class to be called.
    /// </summary>
    // Attribute used by Managed Extensibility Framework extensions to specify the scope of operations, based on the user's
    // selection in the Entity Designer
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class EntityDesignerExtendedPropertyAttribute : Attribute
    {
        private readonly EntityDesignerSelection _entityDesignerSelection;

        /// <summary>
        ///     Instantiates a new instance of the
        ///     <see
        ///         cref="T:Microsoft.Data.Entity.Design.Extensibility.EntityDesignerExtendedPropertyAttribute" />
        ///     class.
        /// </summary>
        /// <param name="entityDesignerSelection">
        ///     The object in the Entity Data Model Designer or the Model Browser that, when selected by a user, triggers the call of the
        ///     <see
        ///         cref="M:Microsoft.Data.Entity.Design.Extensibility.IEntityDesignerExtendedProperty.CreateProperty" />
        ///     method.
        /// </param>
        public EntityDesignerExtendedPropertyAttribute(EntityDesignerSelection entityDesignerSelection)
        {
            _entityDesignerSelection = entityDesignerSelection;
        }

        /// <summary>
        ///     The object in the Entity Data Model Designer or the Model Browser that, when selected by a user, triggers the call of the
        ///     <see
        ///         cref="M:Microsoft.Data.Entity.Design.Extensibility.IEntityDesignerExtendedProperty.CreateProperty" />
        ///     method.
        /// </summary>
        /// <returns>
        ///     The object in the Entity Data Model Designer that, when selected, triggers the call of the
        ///     <see
        ///         cref="M:Microsoft.Data.Entity.Design.Extensibility.IEntityDesignerExtendedProperty.CreateProperty" />
        ///     method.
        /// </returns>
        public EntityDesignerSelection EntityDesignerSelection
        {
            get { return _entityDesignerSelection; }
        }
    }
}
