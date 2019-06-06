// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Entity.Design;
using System.Data.Metadata.Edm;
using System.Globalization;
using System.IO;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Data.Entity.Build.Tasks.Properties;

namespace Microsoft.Data.Entity.Build.Tasks
{
    public class EntityClean: Task
    {
        private ITaskItem[] _sources; 
        private string _resourceOutputPath;
        private string _outputPath;

        [Required]
        public ITaskItem[] Sources
        {
            get { return _sources; }
            set { _sources = value; }
        }

        /// <remarks>
        /// [Required] - this (required) property doesn't exist in v3.5 and prevents targeting v3.5 framework and tools from v4
        /// the v3.5 Microsoft.Data.Entity.targets won't use the property
        /// the v4.0 Microsoft.Data.Entity.targets will always supply the property
        /// other target files are not expected to directly use Microsoft.Data.Entity.Build.Tasks.dll
        /// </remarks>
        public string ResourceOutputPath
        {
            get { return _resourceOutputPath; }
            set { _resourceOutputPath = value; }
        }

        [Required]
        public string OutputPath
        {
            get { return _outputPath; }
            set { _outputPath = value; }
        }

        public override bool Execute()
        {
            bool allFilesCleaned = true;

            // the source's itemspec will mirror the metadata locations
            // in the bin directory exactly.
            foreach (ITaskItem source in this.Sources)
            {
                // combine the output path and the edmx file location to create a location that points to where the
                // output files exist
                string relativePath = EntityDeploySplit.EntityDeployMetadataRelativePath(source);
                string resourceOutputModelPath = (null != this.ResourceOutputPath) ? Path.Combine(this.ResourceOutputPath, relativePath) : null;
                string outputModelPath = Path.Combine(this.OutputPath, relativePath);
                List<FileInfo> filesToDelete = new List<FileInfo>();
                try
                {
                    // translate the 'fake' model path into the assumed locations of the csdl, ssdl, and msl locations
                    // under either ResourceOutputPath or OutputPath - file will be deleted if present under either one
                    // if resourceOutputModelPath is null then follow the v3.5 behavior
                    if (null != resourceOutputModelPath)
                    {
                        filesToDelete.Add(new FileInfo(Path.ChangeExtension(resourceOutputModelPath, XmlConstants.CSpaceSchemaExtension)));
                        filesToDelete.Add(new FileInfo(Path.ChangeExtension(resourceOutputModelPath, XmlConstants.SSpaceSchemaExtension)));
                        filesToDelete.Add(new FileInfo(Path.ChangeExtension(resourceOutputModelPath, XmlConstants.CSSpaceSchemaExtension)));
                    }

                    filesToDelete.Add(new FileInfo(Path.ChangeExtension(outputModelPath, XmlConstants.CSpaceSchemaExtension)));
                    filesToDelete.Add(new FileInfo(Path.ChangeExtension(outputModelPath, XmlConstants.SSpaceSchemaExtension)));
                    filesToDelete.Add(new FileInfo(Path.ChangeExtension(outputModelPath, XmlConstants.CSSpaceSchemaExtension)));
                }
                catch (Exception ex)
                {
                    // there are lots of possible exceptions here (Security, NotSupported, Argument, Unauthorized, etc.)
                    Log.LogError(string.Format(CultureInfo.CurrentCulture, Resources.ErrorProcessingInputFile, source.ItemSpec));
                    Log.LogErrorFromException(ex, false);
                    allFilesCleaned = false;
                    continue;
                }

                // if each CSDL, SSDL, and MSL file exists, then delete it
                foreach (FileInfo fileToDelete in filesToDelete)
                {
                    if (fileToDelete.Exists)
                    {
                        try
                        {
                            fileToDelete.Delete();
                            Log.LogMessage(MessageImportance.Low, string.Format(CultureInfo.CurrentCulture, Resources.FinishedCleaningFile, fileToDelete.FullName));
                        }
                        catch (Exception e)
                        {
                            Log.LogError(string.Format(CultureInfo.CurrentCulture, Resources.ErrorCleaningFile, fileToDelete.FullName));
                            Log.LogErrorFromException(e, false);
                            allFilesCleaned = false;
                        }
                    }
                }

                Log.LogMessage(string.Format(CultureInfo.CurrentCulture, Resources.FinishedCleaningEdmxFile, source.ItemSpec));
            }

            if (allFilesCleaned)
            {
                Log.LogMessage(string.Format(CultureInfo.CurrentCulture, Resources.FinishedCleaningAllFiles, this.Sources.Length));
            }

            return allFilesCleaned;
        }
    }
}
