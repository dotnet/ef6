// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data.Entity.Design;
using System.Data.Metadata.Edm;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Data.Entity.Build.Tasks.Properties;

namespace Microsoft.Data.Entity.Build.Tasks
{
    /// <summary>
    /// Extracts the CSDL, MSL and SSDL resources from the EDMX file(s) and copies them
    /// as files to the OutputPath
    /// </summary>
    public class EntityDeploy : Task
    {
        private string _outputPath;
        private ITaskItem[] _sources;
        private List<ITaskItem> _newResources = new List<ITaskItem>();

        [Required]
        public string OutputPath
        {
            get { return _outputPath; }
            set { _outputPath = value; }
        }

        [Required]
        public ITaskItem[] Sources
        {
            get { return _sources; }
            set { _sources = value; }
        }

        /// <summary>
        /// Output property that contains the list of all embedded resources
        /// Used only when targeting v3.5 toolset.  v4+ toolsets should use the EntityClean task's ResourceOutputPath parameter.
        /// </summary>
        /// <remarks>exists for v3.5 backward compatibility - not used in v4.0 and newer</remarks>
        [Obsolete("Used only for v3.5 backward compatibility."), Output]
        public ITaskItem[] EntityDataModelEmbeddedResources
        {
            get
            {
                return _newResources.ToArray();
            }
        }

        // Task returns true if finishes successfully
        public override bool Execute()
        {
            // Check usage
            if (string.IsNullOrEmpty(OutputPath))
            {
                // "OutputPath" is the name of the parameter and hence not localized
                Log.LogError(string.Format(CultureInfo.CurrentCulture, Resources.Usage, typeof(EntityDeploy).FullName, "OutputPath"));
                return false;
            }

            // Create the output files in outputDir
            DirectoryInfo outputDir = new DirectoryInfo(OutputPath);
            return OutputFiles(outputDir);
        }

        // returns true if finished successfully
        private bool OutputFiles(DirectoryInfo topLevelOutputDir)
        {
            bool allFilesOutputSuccessfully = true;

            // Loop over all EDMX files passed in as Sources
            Log.LogMessage(string.Format(CultureInfo.CurrentCulture, Resources.ProcessingEdmxFiles, Sources.Length));
            for (int i = 0; i < Sources.Length; i++)
            {
                string inputFileRelativePath = Sources[i].ItemSpec;
                try
                {
                    FileInfo edmxFile = new FileInfo(inputFileRelativePath);

                    if (!edmxFile.Extension.Equals(XmlConstants.EdmxFileExtension, StringComparison.CurrentCultureIgnoreCase))
                    {
                        Log.LogError(string.Format(CultureInfo.CurrentCulture, Resources.ErrorNotAnEdmxFile, edmxFile.Name));
                        continue;
                    }

                    Log.LogMessage(string.Format(CultureInfo.CurrentCulture, Resources.StartingProcessingFile, inputFileRelativePath));

                    // Find the C/M/S nodes within the EDMX file and check for errors
                    using (StreamReader edmxInputStream = new StreamReader(edmxFile.FullName))
                    {
                        XmlElement conceptualSchemaElement;
                        XmlElement mappingElement;
                        XmlElement storageSchemaElement;
                        string metadataArtifactProcessingValue;
                        EntityDesignerUtils.ExtractConceptualMappingAndStorageNodes(
                            edmxInputStream, out conceptualSchemaElement, out mappingElement, out storageSchemaElement, out metadataArtifactProcessingValue);

                        bool produceOutput = true;
                        if (null == conceptualSchemaElement)
                        {
                            Log.LogError(string.Format(CultureInfo.CurrentCulture, Resources.CouldNotFindConceptualSchema, edmxFile.FullName));
                            produceOutput = false;
                            allFilesOutputSuccessfully = false;
                        }

                        if (null == storageSchemaElement)
                        {
                            Log.LogError(string.Format(CultureInfo.CurrentCulture, Resources.CouldNotFindStorageSchema, edmxFile.FullName));
                            produceOutput = false;
                            allFilesOutputSuccessfully = false;
                        }

                        if (null == mappingElement)
                        {
                            Log.LogError(string.Format(CultureInfo.CurrentCulture, Resources.CouldNotFindMapping, edmxFile.FullName));
                            produceOutput = false;
                            allFilesOutputSuccessfully = false;
                        }

                        // Output the set of C/M/S files corresponding to this EDMX file
                        if (produceOutput)
                        {
                            // if the given edmx file is a link we should output the files using 
                            // the directory relative to the link rather than to the original file
                            string inputFileRelativePathFromMetadata = EntityDeploySplit.EntityDeployMetadataRelativePath(Sources[i]);
                            if (!OutputCMS(inputFileRelativePathFromMetadata, topLevelOutputDir, conceptualSchemaElement, mappingElement, storageSchemaElement))
                            {
                                allFilesOutputSuccessfully = false;
                            }
                        }
                    }
                }
                catch (ArgumentException ae)
                {
                    Log.LogError(string.Format(CultureInfo.CurrentCulture, Resources.ErrorProcessingInputFile, inputFileRelativePath));
                    Log.LogErrorFromException(ae, false);
                    allFilesOutputSuccessfully = false;
                }
                catch (IOException ioe)
                {
                    Log.LogError(string.Format(CultureInfo.CurrentCulture, Resources.ErrorProcessingInputFile, inputFileRelativePath));
                    Log.LogErrorFromException(ioe, false);
                    allFilesOutputSuccessfully = false;
                }
                catch(SecurityException se)
                {
                    Log.LogError(string.Format(CultureInfo.CurrentCulture, Resources.ErrorProcessingInputFile, inputFileRelativePath));
                    Log.LogErrorFromException(se, false);
                    allFilesOutputSuccessfully = false;
                }
                catch (NotSupportedException nse)
                {
                    Log.LogError(string.Format(CultureInfo.CurrentCulture, Resources.ErrorProcessingInputFile, inputFileRelativePath));
                    Log.LogErrorFromException(nse, false);
                    allFilesOutputSuccessfully = false;
                }
                catch (UnauthorizedAccessException uae)
                {
                    Log.LogError(string.Format(CultureInfo.CurrentCulture, Resources.ErrorProcessingInputFile, inputFileRelativePath));
                    Log.LogErrorFromException(uae, false);
                    allFilesOutputSuccessfully = false;
                }
                catch (XmlException xe)
                {
                    Log.LogError(String.Empty,
                        String.Empty,
                        String.Empty,
                        inputFileRelativePath,
                        xe.LineNumber,
                        xe.LinePosition,
                        xe.LineNumber,
                        xe.LinePosition, 
                        xe.Message);
                    allFilesOutputSuccessfully = false;
                }

                Log.LogMessage(string.Format(CultureInfo.CurrentCulture, Resources.FinishedProcessingFile, inputFileRelativePath));
            }

            Log.LogMessage(string.Format(CultureInfo.CurrentCulture, Resources.FinishedProcessingEdmxFiles, Sources.Length));
            return allFilesOutputSuccessfully;
        }

        /// <summary>
        /// Outputs an individual set of CSDL, MSL and SSDL files
        /// </summary>
        /// <param name="edmxFile">input EDMX file</param>
        /// <param name="outputDir">directory where output files should be placed</param>
        /// <param name="conceptualSchemaNode">CSDL Schema element as XmlNode</param>
        /// <param name="mappingNode">Mapping element as XmlNode</param>
        /// <param name="storageSchemaNode">SSDL Schema element as XmlNode</param>
        /// <returns>returns true if finished successfully</returns>
        private bool OutputCMS(string inputFileRelativePath, DirectoryInfo topLevelOutputDir,
            XmlElement conceptualSchemaElement, XmlElement mappingElement, XmlElement storageSchemaElement)
        {

            // create the directory where this specific set of C/M/S files will be placed
            FileInfo outputFile = new FileInfo(Path.Combine(topLevelOutputDir.FullName, inputFileRelativePath));
            DirectoryInfo outputDir = outputFile.Directory;
            if (!outputDir.Exists)
            {
                outputDir.Create();
            }

            string outputDirPath = outputDir.FullName + "\\";
            string outputFileNamePrefix = outputFile.Name.Replace(XmlConstants.EdmxFileExtension, String.Empty);
            string csdlFileName = outputFileNamePrefix + XmlConstants.CSpaceSchemaExtension;
            string mslFileName = outputFileNamePrefix + XmlConstants.CSSpaceSchemaExtension;
            string ssdlFileName = outputFileNamePrefix + XmlConstants.SSpaceSchemaExtension;

            bool csdlProducedSuccessfully = OutputXml(outputDirPath + csdlFileName, conceptualSchemaElement);
            bool mslProducedSuccessfully = OutputXml(outputDirPath + mslFileName, mappingElement);
            bool ssdlProducedSuccessfully = OutputXml(outputDirPath + ssdlFileName, storageSchemaElement);

            if (csdlProducedSuccessfully)
            {
                _newResources.Add(CreateTaskItem(outputDirPath + csdlFileName, csdlFileName, inputFileRelativePath));
            }

            if (ssdlProducedSuccessfully)
            {
                _newResources.Add(CreateTaskItem(outputDirPath + ssdlFileName, ssdlFileName, inputFileRelativePath));
            }

            if (mslProducedSuccessfully)
            {
                _newResources.Add(CreateTaskItem(outputDirPath + mslFileName, mslFileName, inputFileRelativePath));
            }

            return (csdlProducedSuccessfully && mslProducedSuccessfully && ssdlProducedSuccessfully);
        }

        private TaskItem CreateTaskItem(string fileName, string logicalName, string relativePathOfEDMXFile)
        {
            TaskItem ti = new TaskItem(fileName);

            int lastSlash = relativePathOfEDMXFile.LastIndexOf(Path.DirectorySeparatorChar);
            if (lastSlash > -1)
            {
                logicalName = relativePathOfEDMXFile.Substring(0, lastSlash + 1) + logicalName;
            }
            ti.SetMetadata("LogicalName", logicalName.Replace(Path.DirectorySeparatorChar, '.'));
            return ti;
        }

        // returns true if finished successfully
        private bool OutputXml(string outputPath, XmlElement xmlElement)
        {
            FileInfo file = new FileInfo(outputPath);
            Stream outputStream = null;
            try
            {
                if (file.Exists)
                {
                    outputStream = new FileStream(outputPath, FileMode.Truncate, FileAccess.Write);
                }
                else
                {
                    outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
                }

                EntityDesignerUtils.OutputXmlElementToStream(xmlElement, outputStream);
            }
            catch (IOException ex)
            {
                Log.LogError(string.Format(CultureInfo.CurrentCulture, Resources.ErrorWritingFile, file.FullName));
                Log.LogErrorFromException(ex, false);
                return false;
            }
            finally
            {
                if (outputStream != null) { outputStream.Close(); }
            }

            return true;
        }
    }
}
