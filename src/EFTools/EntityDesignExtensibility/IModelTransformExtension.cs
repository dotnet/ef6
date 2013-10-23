// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Extensibility
{
    /// <summary>Exposes methods for extending the loading and saving processes of .edmx files.</summary>
    public interface IModelTransformExtension
    {
        /// <summary>Defines functionality for extending the process by which an .edmx file is loaded by the Entity Data Model Designer.</summary>
        /// <param name="context">Provides file and Visual Studio project information.</param>
        void OnAfterModelLoaded(ModelTransformExtensionContext context);

        /// <summary>Defines functionality for extending the process by which an .edmx file is saved by the Entity Data Model Designer.</summary>
        /// <param name="context">Provides file and project information.</param>
        void OnBeforeModelSaved(ModelTransformExtensionContext context);
    }
}
