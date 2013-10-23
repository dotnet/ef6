// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Extensibility
{
    using System.Diagnostics.CodeAnalysis;
    using System.Xml.Linq;

    /// <summary>Exposes methods for adding properties to objects that are visible to a user in the Entity Data Model Designer or the Model Browser.</summary>
    public interface IEntityDesignerExtendedProperty
    {
        /// <summary>Creates a new property for an object that is selected in the Entity Data Model Designer or the Model Browser.</summary>
        /// <returns>
        ///     An object whose public properties are displayed in the Visual StudioProperties window. For more information, see
        ///     <see
        ///         cref="T:System.Windows.Forms.PropertyGrid" />
        ///     .
        /// </returns>
        /// <param name="xElement">The element in the .edmx file that defines the object that is selected in the Entity Data Model Designer or the Model Browser</param>
        /// <param name="context">Provides file and project information.</param>
        /// <remarks>
        ///     Called when the selected object changes in the ADO.NET Entity Designer. An implementation should
        ///     return a new instance of a class whose public properties should be shown in the VS property window.
        ///     An implementation may return "null" to not show the property.
        ///     Any exceptions thrown by an implementation of CreateProperty() are shown to the
        ///     user in a standard dialog box.
        ///     Extensions are responsible for localizing exception messages.
        /// </remarks>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "This is a desirable name")]
        object CreateProperty(XElement xElement, PropertyExtensionContext context);
    }
}
