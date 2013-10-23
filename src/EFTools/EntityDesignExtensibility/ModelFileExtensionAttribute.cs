// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Extensibility
{
    using System;
    using System.ComponentModel.Composition;

    /// <summary>Specifies a custom file extension that can be loaded or saved by the Entity Data Model Designer.</summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ModelFileExtensionAttribute : Attribute
    {
        private readonly string _fileExtension;

        /// <summary>
        ///     Creates a new instance of the <see cref="T:Microsoft.Data.Entity.Design.Extensibility.ModelFileExtensionAttribute" /> class.
        /// </summary>
        /// <param name="fileExtension">The file extension for custom files that can be loaded and saved by the Entity Data Model Designer.</param>
        public ModelFileExtensionAttribute(string fileExtension)
        {
            _fileExtension = fileExtension;
        }

        /// <summary>The file extension for custom files that can be loaded and saved by the Entity Data Model Designer.</summary>
        /// <returns>The file extension for custom files that can be loaded and saved by the Entity Data Model Designer.</returns>
        // Specifies the file extension for which the model conversion extension class will
        // be called. Can optionally include a leading “.”.
        public string FileExtension
        {
            get { return _fileExtension; }
        }
    }
}
