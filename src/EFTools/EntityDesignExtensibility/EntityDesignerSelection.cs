// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Extensibility
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     An enumeration used to specify which object types that, when selected in the Entity Data Model Designer or the Model Browser, cause the
    ///     <see
    ///         cref="M:Microsoft.Data.Entity.Design.Extensibility.IEntityDesignerExtendedProperty.CreateProperty(System.Xml.Linq.XElement,Microsoft.Data.Entity.Design.Extensibility.PropertyExtensionContext)" />
    ///     method of the annotated class to be called.
    /// </summary>
    [Flags]
    [SuppressMessage("Microsoft.Naming", "CA1714:FlagsEnumsShouldHavePluralNames")]
    public enum EntityDesignerSelection
    {
        /// <summary>
        ///     Specifies that the
        ///     <see
        ///         cref="M:Microsoft.Data.Entity.Design.Extensibility.IEntityDesignerExtendedProperty.CreateProperty(System.Xml.Linq.XElement,Microsoft.Data.Entity.Design.Extensibility.PropertyExtensionContext)" />
        ///     method should be called when the Entity Data Model Designer surface is selected in the Entity Data Model Designer.
        /// </summary>
        DesignerSurface = 0x00001,

        /// <summary>
        ///     Specifies that the
        ///     <see
        ///         cref="M:Microsoft.Data.Entity.Design.Extensibility.IEntityDesignerExtendedProperty.CreateProperty(System.Xml.Linq.XElement,Microsoft.Data.Entity.Design.Extensibility.PropertyExtensionContext)" />
        ///     method should be called when a conceptual model entity set is selected in the Model Browser.
        /// </summary>
        ConceptualModelEntitySet = 0x00002,

        /// <summary>
        ///     Specifies that the
        ///     <see
        ///         cref="M:Microsoft.Data.Entity.Design.Extensibility.IEntityDesignerExtendedProperty.CreateProperty(System.Xml.Linq.XElement,Microsoft.Data.Entity.Design.Extensibility.PropertyExtensionContext)" />
        ///     method should be called when a conceptual model association set is selected in the Model Browser.
        /// </summary>
        ConceptualModelAssociationSet = 0x00004,

        /// <summary>
        ///     Specifies that the
        ///     <see
        ///         cref="M:Microsoft.Data.Entity.Design.Extensibility.IEntityDesignerExtendedProperty.CreateProperty(System.Xml.Linq.XElement,Microsoft.Data.Entity.Design.Extensibility.PropertyExtensionContext)" />
        ///     method should be called when a conceptual model entity container is selected in the Model Browser.
        /// </summary>
        ConceptualModelEntityContainer = 0x00008,

        /// <summary>
        ///     Specifies that the
        ///     <see
        ///         cref="M:Microsoft.Data.Entity.Design.Extensibility.IEntityDesignerExtendedProperty.CreateProperty(System.Xml.Linq.XElement,Microsoft.Data.Entity.Design.Extensibility.PropertyExtensionContext)" />
        ///     method should be called when a conceptual model entity type is selected in the Entity Data Model Designer or the Model Browser.
        /// </summary>
        ConceptualModelEntityType = 0x00010,

        /// <summary>
        ///     Specifies that the
        ///     <see
        ///         cref="M:Microsoft.Data.Entity.Design.Extensibility.IEntityDesignerExtendedProperty.CreateProperty(System.Xml.Linq.XElement,Microsoft.Data.Entity.Design.Extensibility.PropertyExtensionContext)" />
        ///     method should be called when a conceptual model property is selected in the Entity Data Model Designer or the Model Browser.
        /// </summary>
        ConceptualModelProperty = 0x00020,

        /// <summary>
        ///     Specifies that the
        ///     <see
        ///         cref="M:Microsoft.Data.Entity.Design.Extensibility.IEntityDesignerExtendedProperty.CreateProperty(System.Xml.Linq.XElement,Microsoft.Data.Entity.Design.Extensibility.PropertyExtensionContext)" />
        ///     method should be called when a conceptual model navigation property is selected in the Entity Data Model Designer or the Model Browser.
        /// </summary>
        ConceptualModelNavigationProperty = 0x00040,

        /// <summary>
        ///     Specifies that the
        ///     <see
        ///         cref="M:Microsoft.Data.Entity.Design.Extensibility.IEntityDesignerExtendedProperty.CreateProperty(System.Xml.Linq.XElement,Microsoft.Data.Entity.Design.Extensibility.PropertyExtensionContext)" />
        ///     method should be called when a conceptual model association is selected in the Entity Data Model Designer or the Model Browser.
        /// </summary>
        ConceptualModelAssociation = 0x00080,

        /// <summary>
        ///     Specifies that the
        ///     <see
        ///         cref="M:Microsoft.Data.Entity.Design.Extensibility.IEntityDesignerExtendedProperty.CreateProperty(System.Xml.Linq.XElement,Microsoft.Data.Entity.Design.Extensibility.PropertyExtensionContext)" />
        ///     method should be called when a conceptual model complex type is selected in the Model Browser.
        /// </summary>
        ConceptualModelComplexType = 0x00100,

        /// <summary>
        ///     Specifies that the
        ///     <see
        ///         cref="M:Microsoft.Data.Entity.Design.Extensibility.IEntityDesignerExtendedProperty.CreateProperty(System.Xml.Linq.XElement,Microsoft.Data.Entity.Design.Extensibility.PropertyExtensionContext)" />
        ///     method should be called when a conceptual model complex property is selected in the Entity Data Model Designer or the Model Browser.
        /// </summary>
        ConceptualModelComplexProperty = 0x00200,

        /// <summary>
        ///     Specifies that the
        ///     <see
        ///         cref="M:Microsoft.Data.Entity.Design.Extensibility.IEntityDesignerExtendedProperty.CreateProperty(System.Xml.Linq.XElement,Microsoft.Data.Entity.Design.Extensibility.PropertyExtensionContext)" />
        ///     method should be called when a conceptual model function import is selected in the Model Browser.
        /// </summary>
        ConceptualModelFunctionImport = 0x00400,

        /// <summary>
        ///     Specifies that the
        ///     <see
        ///         cref="M:Microsoft.Data.Entity.Design.Extensibility.IEntityDesignerExtendedProperty.CreateProperty(System.Xml.Linq.XElement,Microsoft.Data.Entity.Design.Extensibility.PropertyExtensionContext)" />
        ///     method should be called when a conceptual model function import parameter is selected in the Model Browser.
        /// </summary>
        ConceptualModelFunctionImportParameter = 0x00800,

        /// <summary>
        ///     Specifies that the
        ///     <see
        ///         cref="M:Microsoft.Data.Entity.Design.Extensibility.IEntityDesignerExtendedProperty.CreateProperty(System.Xml.Linq.XElement,Microsoft.Data.Entity.Design.Extensibility.PropertyExtensionContext)" />
        ///     method should be called when a storage model entity container is selected in the Model Browser.
        /// </summary>
        StorageModelEntityContainer = 0x01000,

        /// <summary>
        ///     Specifies that the
        ///     <see
        ///         cref="M:Microsoft.Data.Entity.Design.Extensibility.IEntityDesignerExtendedProperty.CreateProperty(System.Xml.Linq.XElement,Microsoft.Data.Entity.Design.Extensibility.PropertyExtensionContext)" />
        ///     method should be called when a storage model entity type is selected in the Model Browser.
        /// </summary>
        StorageModelEntityType = 0x02000,

        /// <summary>
        ///     Specifies that the
        ///     <see
        ///         cref="M:Microsoft.Data.Entity.Design.Extensibility.IEntityDesignerExtendedProperty.CreateProperty(System.Xml.Linq.XElement,Microsoft.Data.Entity.Design.Extensibility.PropertyExtensionContext)" />
        ///     method should be called when a storage model property is selected in the Model Browser.
        /// </summary>
        StorageModelProperty = 0x04000,

        /// <summary>
        ///     Specifies that the
        ///     <see
        ///         cref="M:Microsoft.Data.Entity.Design.Extensibility.IEntityDesignerExtendedProperty.CreateProperty(System.Xml.Linq.XElement,Microsoft.Data.Entity.Design.Extensibility.PropertyExtensionContext)" />
        ///     method should be called when a storage model association is selected in the Model Browser.
        /// </summary>
        StorageModelAssociation = 0x08000,

        /// <summary>
        ///     Specifies that the
        ///     <see
        ///         cref="M:Microsoft.Data.Entity.Design.Extensibility.IEntityDesignerExtendedProperty.CreateProperty(System.Xml.Linq.XElement,Microsoft.Data.Entity.Design.Extensibility.PropertyExtensionContext)" />
        ///     method should be called when a storage model function is selected in the Model Browser.
        /// </summary>
        StorageModelFunction = 0x10000,

        /// <summary>
        ///     Specifies that the
        ///     <see
        ///         cref="M:Microsoft.Data.Entity.Design.Extensibility.IEntityDesignerExtendedProperty.CreateProperty(System.Xml.Linq.XElement,Microsoft.Data.Entity.Design.Extensibility.PropertyExtensionContext)" />
        ///     method should be called when a storage model function parameter is selected in the Model Browser.
        /// </summary>
        StorageModelFunctionParameter = 0x20000,
    }
}
