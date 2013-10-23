// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Converters
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine;

    internal class DbGenTemplateFileListConverter : ExtensibleFileListConverter
    {
        protected override string SubDirPath
        {
            get { return DatabaseGenerationEngine._dbGenFolderName; }
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            var standardValues = new List<string>();
            standardValues.AddRange(DatabaseGenerationEngine.TemplateFileManager.VSFiles.Select(fi => MacroizeFilePath(fi.FullName)));
            standardValues.AddRange(DatabaseGenerationEngine.TemplateFileManager.UserFiles.Select(fi => MacroizeFilePath(fi.FullName)));
            return new StandardValuesCollection(standardValues);
        }
    }
}
