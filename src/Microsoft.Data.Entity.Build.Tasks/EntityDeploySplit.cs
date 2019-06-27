// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Metadata.Edm;
using System.Data.Entity.Design;
using System.Globalization;
using System.IO;
using System.Security;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Data.Entity.Build.Tasks.Properties;
using System.Collections;

namespace Microsoft.Data.Entity.Build.Tasks
{
    public class EntityDeploySplit : Task
    {
        private static readonly string EntityDeployRelativeDir = "EntityDeployRelativeDir";

        private ITaskItem[] _sources;
        private ITaskItem[] _edmxItemsToBeEmbedded;
        private ITaskItem[] _edmxItemsToNotBeEmbedded;

        [Required]
        public ITaskItem[] Sources
        {
            get { return _sources; }
            set { _sources = value; }
        }

        [Output]
        public ITaskItem[] EmbeddingItems
        {
            get { return _edmxItemsToBeEmbedded; }
            set { _edmxItemsToBeEmbedded = value; }
        }

        [Output]
        public ITaskItem[] NonEmbeddingItems
        {
            get { return _edmxItemsToNotBeEmbedded; }
            set { _edmxItemsToNotBeEmbedded = value; }
        }

        // Task returns true if finishes successfully
        public override bool Execute()
        {            
            List<ITaskItem> embeddedEdmxItems = new List<ITaskItem>();
            List<ITaskItem> nonEmbeddedEdmxItems = new List<ITaskItem>();

            // Loop over all EDMX files passed in as Sources
            for (int i = 0; i < Sources.Length; i++)
            {
                ITaskItem currentSource = Sources[i];
                string inputFileRelativePath = currentSource.ItemSpec;
                try
                {
                    FileInfo edmxFile = new FileInfo(inputFileRelativePath);

                    if (!edmxFile.Extension.Equals(XmlConstants.EdmxFileExtension, StringComparison.CurrentCultureIgnoreCase))
                    {
                        Log.LogError(string.Format(CultureInfo.CurrentCulture, Resources.ErrorNotAnEdmxFile, edmxFile.Name));
                        continue;
                    }

                    // Using the metadataArtifactProcessingValue split up TaskItems
                    using (StreamReader edmxInputStream = new StreamReader(edmxFile.FullName))
                    {
                        XmlElement conceptualSchemaElement;
                        XmlElement mappingElement;
                        XmlElement storageSchemaElement;
                        string metadataArtifactProcessingValue;
                        EntityDesignerUtils.ExtractConceptualMappingAndStorageNodes(
                            edmxInputStream, out conceptualSchemaElement, out mappingElement, out storageSchemaElement, out metadataArtifactProcessingValue);

                        // set up the EntityDeployRelativeDir metadata on the output item
                        // so that the metadata can be used to determine the correct output
                        // path independent on whether the input file is a link or not
                        AssignMetadata(currentSource);

                        // add to the appropriate output item list 
                        if ("EmbedInOutputAssembly".Equals(metadataArtifactProcessingValue, StringComparison.OrdinalIgnoreCase))
                        {
                            embeddedEdmxItems.Add(currentSource);
                        }
                        else
                        {
                            nonEmbeddedEdmxItems.Add(currentSource);
                        }
                    }
                }
                catch (ArgumentException ae)
                {
                    Log.LogError(string.Format(CultureInfo.CurrentCulture, Resources.ErrorProcessingInputFile, inputFileRelativePath));
                    Log.LogErrorFromException(ae, false);
                }
                catch (IOException ioe)
                {
                    Log.LogError(string.Format(CultureInfo.CurrentCulture, Resources.ErrorProcessingInputFile, inputFileRelativePath));
                    Log.LogErrorFromException(ioe, false);
                }
                catch (SecurityException se)
                {
                    Log.LogError(string.Format(CultureInfo.CurrentCulture, Resources.ErrorProcessingInputFile, inputFileRelativePath));
                    Log.LogErrorFromException(se, false);
                }
                catch (NotSupportedException nse)
                {
                    Log.LogError(string.Format(CultureInfo.CurrentCulture, Resources.ErrorProcessingInputFile, inputFileRelativePath));
                    Log.LogErrorFromException(nse, false);
                }
                catch (UnauthorizedAccessException uae)
                {
                    Log.LogError(string.Format(CultureInfo.CurrentCulture, Resources.ErrorProcessingInputFile, inputFileRelativePath));
                    Log.LogErrorFromException(uae, false);
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
                }
            }

            // set output parameters
            _edmxItemsToBeEmbedded = embeddedEdmxItems.ToArray();
            _edmxItemsToNotBeEmbedded = nonEmbeddedEdmxItems.ToArray();

            return true;
        }

        /// <summary>
        /// Assign the relative directory value to the metadata
        /// </summary>
        internal static void AssignMetadata(ITaskItem taskItem)
        {
            if (false == ContainsEntityDeployRelativeDir(taskItem))
            {
                // determine what the metadata value should be
                string entityDeployRelativeDir = ComputeEntityDeployRelativeDir(taskItem);

                // assign the metadata
                taskItem.SetMetadata(EntityDeployRelativeDir, entityDeployRelativeDir);
            }
        }

        /// <summary>
        /// If the TaskItem represents a linked file then compute the relative 
        /// directory of the link, otherwise use the normal relative directory
        /// of the file
        /// </summary>
        internal static string ComputeEntityDeployRelativeDir(ITaskItem taskItem)
        {
            string linkPath = taskItem.GetMetadata("Link");
            if (string.IsNullOrEmpty(linkPath))
            {
                // if the TaskItem has no Link metadata then return the normal RelativeDir and Filename metadata
                return taskItem.GetMetadata("RelativeDir");
            }
            else
            {
                // use the path to the link

                // make sure the relativeDir is empty or ends with Path.DirectorySeparatorChar
                int lastIndexOfDirSeparator = linkPath.LastIndexOf(Path.DirectorySeparatorChar);
                if (0 >= lastIndexOfDirSeparator)
                {
                    // no dir separator char in linkPath, or last Path.DirectorySeparatorChar char
                    // in the linkPath is the first character of the linkPath
                    return string.Empty;
                }
                else
                {
                    // entityDeployRelativeDir is up to and including the last Path.DirectorySeparatorChar char
                    return linkPath.Substring(0, lastIndexOfDirSeparator + 1);
                }
            }
        }

        /// <summary>
        /// Returns the full path to the file (including the filename and extension),
        /// computing the value for the relative directory using the same method as the 
        /// EntityDeployRelativeDir metadata
        /// </summary>
        internal static string EntityDeployMetadataRelativePath(ITaskItem taskItem)
        {
                        string entityDeployRelativeDir = String.Empty;
            if (true == ContainsEntityDeployRelativeDir(taskItem))
            {
                entityDeployRelativeDir = taskItem.GetMetadata(EntityDeployRelativeDir);
            }
            else
            {
                entityDeployRelativeDir = ComputeEntityDeployRelativeDir(taskItem);
            }
            
            string fileName = taskItem.GetMetadata("Filename") + taskItem.GetMetadata("Extension");
            return Path.Combine(entityDeployRelativeDir, fileName);
        }

        private static bool ContainsEntityDeployRelativeDir(ITaskItem taskItem)
        {
            IEnumerator enumerator = taskItem.MetadataNames.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current as string == EntityDeployRelativeDir) return true;
            }
            return false;
        }
    }
}
