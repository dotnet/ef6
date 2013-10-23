// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Extensibility
{
    /// <summary>Exposes methods for converting a custom file format to and from the .edmx file format that is readable by the Entity Data Model Designer.</summary>
    public interface IModelConversionExtension
    {
        /// <summary>Defines custom functionality for loading a file with a custom format and converting it to an .edmx format.</summary>
        /// <param name="context">Provides file and project information.</param>
        void OnAfterFileLoaded(ModelConversionExtensionContext context);

        /// <summary>Defines custom functionality for converting an .edmx file to a file with a custom format before the file is saved.</summary>
        void OnBeforeFileSaved(ModelConversionExtensionContext context);
    }
}
