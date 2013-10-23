// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Extensibility
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Xml.Linq;
    using EnvDTE;

    /// <summary>
    ///     A base class for the <see cref="T:Microsoft.Data.Entity.Design.Extensibility.ModelGenerationExtensionContext" />,
    ///     <see cref="T:Microsoft.Data.Entity.Design.Extensibility.PropertyExtensionContext" />,
    ///     <see cref="T:Microsoft.Data.Entity.Design.Extensibility.ModelTransformExtensionContext" /> and
    ///     <see cref="T:Microsoft.Data.Entity.Design.Extensibility.ModelConversionExtensionContext" />
    ///     classes.
    /// </summary>
    public abstract class ExtensionContext
    {
        /// <summary>The current Visual Studio project.</summary>
        /// <returns>The current Visual Studio project.</returns>
        public abstract Project Project { get; }

        /// <summary>The targeted version of the Entity Framework.</summary>
        /// <returns>The targeted version of the Entity Framework.</returns>
        public abstract Version EntityFrameworkVersion { get; }
    }

    /// <summary>Creates a unit of work that can be undone or redone with the Undo and Redo buttons in Visual Studio.</summary>
    public abstract class EntityDesignerChangeScope : IDisposable
    {
        /// <summary>
        /// Finalizer for the <see cref="EntityDesignerChangeScope"/> class
        /// </summary>
        ~EntityDesignerChangeScope()
        {
            Dispose(false);
        }

        /// <summary>Commits all operations within a change scope.</summary>
        // Indicates that all operations within the scope are completed successfully.
        // Throws InvalidOperationException if this method has already been called once.
        public abstract void Complete();

        /// <summary>
        ///     Releases all resources used by the current instance of the
        ///     <see cref="T:Microsoft.Data.Entity.Design.Extensibility.EntityDesignerChangeScope" />
        ///     class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Does nothing in this base class. Should be overridden in classes which inherit from this class
        /// and which have resources to release.
        /// </summary>
        /// <param name="disposing">True if this is called from Dispose(), false if called from the finalizer</param>
        protected virtual void Dispose(bool disposing)
        {
        }
    }

    /// <summary>Provides file and project information to Visual Studio extensions that add custom properties to objects visible in the Entity Data Model Designer or the Model Browser.</summary>
    public abstract class PropertyExtensionContext : ExtensionContext
    {
        /// <summary>The current Visual Studio project item.</summary>
        /// <returns>The current Visual Studio project item.</returns>
        public abstract ProjectItem ProjectItem { get; }

        /// <summary>
        ///     Creates an <see cref="T:Microsoft.Data.Entity.Design.Extensibility.EntityDesignerChangeScope" /> object and sets the string that will appear in the dropdown lists for the Undo and Redo buttons in Visual Studio.
        /// </summary>
        /// <param name="undoRedoDescription">The string that will appear in the dropdown lists for the Undo and Redo buttons in Visual Studio.</param>
        /// <returns>An instance of an EntityDesignerChangeScope</returns>
        // Creates a change scope that is used to add, delete and modify EDMX file content.
        // All changes made in a change scope are a single unit of work that can be undone.
        // 
        // A change scope is rooted at the specified XElement. All modifications must occur
        // to this XElement, its attributes or its descendents or their attributes
        //
        // Throws InvalidOperationException in the following situations:
        // - a change scope returned by a previous call to CreateChangeScope for this
        //   PropertyExtensionContext is still active
        // - an extension tries to add/delete/change XML content in an XML namespace 
        //   owned by MSFT or EDM
        public abstract EntityDesignerChangeScope CreateChangeScope(string undoRedoDescription);
    }

    /// <summary>Provides file and project information to Visual Studio extensions that extend the .edmx file generation process of the Entity Data Model Wizard.</summary>
    public abstract class ModelGenerationExtensionContext : ExtensionContext
    {
        /// <summary>Represents the .edmx document to be modified.</summary>
        /// <returns>Represents the .edmx document to be modified.</returns>
        // The current document that should be updated by extensions.  This is the model generated from the database, 
        // plus all modifications from previous extensions.  
        public abstract XDocument CurrentDocument { get; }

        /// <summary>Represents the original .edmx file generated by the Entity Data Model Wizard.</summary>
        /// <returns>Represents the original .edmx file generated by the Entity Data Model Wizard.</returns>
        public abstract XDocument GeneratedDocument { get; }

        /// <summary>The wizard that initiated the .edmx file generation or update process.</summary>
        /// <returns>The wizard that initiated the .edmx file generation or update process.</returns>
        public abstract WizardKind WizardKind { get; }
    }

    /// <summary>Provides file and project information to Visual Studio extensions that extend the .edmx file update process of the Update Model Wizard.</summary>
    public abstract class UpdateModelExtensionContext : ModelGenerationExtensionContext
    {
        /// <summary>The current Visual Studio project item.</summary>
        /// <returns>The current Visual Studio project item.</returns>
        public abstract ProjectItem ProjectItem { get; }

        /// <summary>Represents the .edmx file before the Update  Model Wizard has run.</summary>
        /// <returns>Represents the .edmx file before the Update  Model Wizard has run.</returns>
        public abstract XDocument OriginalDocument { get; }

        /// <summary>Represents the .edmx file after the Update  Model Wizard has run.</summary>
        /// <returns>Represents the .edmx file after the Update  Model Wizard has run.</returns>
        // The "merged" model.  Specifically, this is the model that was generated from the database, plus 
        // modifications from extensions, plus internal "update model" merge logic.
        public abstract XDocument UpdateModelDocument { get; }
    }

    /// <summary>Provides file and project information to Visual Studio extensions that extend the file loading and saving of .edmx files by the Entity Data Model Designer.</summary>
    public abstract class ModelTransformExtensionContext : ExtensionContext
    {
        /// <summary>The current Visual Studio project item.</summary>
        /// <returns>The current Visual Studio project item.</returns>
        public abstract ProjectItem ProjectItem { get; }

        /// <summary>The original .edmx file that was loaded into memory.</summary>
        /// <returns>The original .edmx file that was loaded into memory.</returns>
        // The document that was originally in memory before any serialization
        // extensions were called. Extensions should not modify this object.
        public abstract XDocument OriginalDocument { get; }

        /// <summary>The current .edmx file on which Visual Studio extensions may operate.</summary>
        /// <returns>The current .edmx file on which Visual Studio extensions may operate.</returns>
        public abstract XDocument CurrentDocument { get; set; }

        /// <summary>A list of errors that can be shown in the Visual StudioError List when .edmx files are loaded or saved by the Entity Data Model Designer.</summary>
        /// <returns>A list of errors that can be shown in the Visual StudioError List when .edmx files are loaded or saved by the Entity Data Model Designer.</returns>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Backwards compatibility, it is already part of public API")]
        public abstract List<ExtensionError> Errors { get; }
    }

    /// <summary>Provides file and project information to Visual Studio extensions that enable the loading and saving of custom file formats.</summary>
    public abstract class ModelConversionExtensionContext : ExtensionContext
    {
        /// <summary>Returns information about the custom file being processed by the Entity Data Model Designer.</summary>
        /// <returns>Information about the custom file being processed by the Entity Data Model Designer.</returns>
        public abstract FileInfo FileInfo { get; }

        /// <summary>The current Visual Studio project item.</summary>
        /// <returns>The current Visual Studio project item.</returns>
        public abstract ProjectItem ProjectItem { get; }

        /// <summary>Returns the .edmx document after it has been converted from a custom file format.</summary>
        /// <returns>The .edmx document after it has been converted from a custom file format.</returns>
        public abstract XDocument CurrentDocument { get; }

        /// <summary>Returns the original document as opened or saved by the Entity Designer.</summary>
        /// <returns>The original document as opened or saved by the Entity Designer.</returns>
        public abstract string OriginalDocument { get; set; }

        /// <summary>A list of errors that can be shown in the Visual StudioError List when loading a custom file format and converting it to a custom file format.</summary>
        /// <returns>A list of errors that can be shown in the Visual StudioError List when loading a custom file format and converting it to a custom file format.</returns>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Backwards compatibility, it is already part of public API")]
        public abstract List<ExtensionError> Errors { get; }
    }
}
