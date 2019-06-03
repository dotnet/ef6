// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Data.Entity.Build.Tasks.Properties;

namespace Microsoft.Data.Entity.Build.Tasks
{
    /// <summary>
    /// Used to strip off the ResourceOutputPath from the 
    /// Item's Identity to produce the correct LogicalName
    /// </summary>
    public class EntityDeploySetLogicalNames : Task
    {
        private string _resourceOutputPath;
        private ITaskItem[] _sources;
        private List<ITaskItem> _newResources = new List<ITaskItem>();

        [Required]
        public string ResourceOutputPath
        {
            get { return _resourceOutputPath; }
            set { _resourceOutputPath = value; }
        }

        [Required]
        public ITaskItem[] Sources
        {
            get { return _sources; }
            set { _sources = value; }
        }

        [Output]
        public ITaskItem[] ResourcesToEmbed
        {
            get { return _newResources.ToArray(); }
        }

        // Task returns true if finishes successfully
        public override bool Execute()
        {
            // Check usage
            if (string.IsNullOrEmpty(ResourceOutputPath))
            {
                // "ResourceOutputPath" is the name of the parameter and hence not localized
                Log.LogError(string.Format(CultureInfo.CurrentCulture, Resources.Usage, typeof(EntityDeploySetLogicalNames).FullName, "ResourceOutputPath"));
                return false;
            }

            int resourceOutputPathLen = ResourceOutputPath.Length;

            // Loop over all EDMX files passed in as Sources
            for (int i = 0; i < Sources.Length; i++)
            {
                ITaskItem currentSource = Sources[i];
                string inputFileIdentity = currentSource.GetMetadata("Identity");

                if (inputFileIdentity.StartsWith(ResourceOutputPath, StringComparison.CurrentCultureIgnoreCase))
                {
                    TaskItem ti = new TaskItem(currentSource.ItemSpec);
                    string logicalName = inputFileIdentity.Substring(resourceOutputPathLen).Replace(Path.DirectorySeparatorChar, '.');
                    Log.LogMessage(MessageImportance.Low, string.Format(CultureInfo.CurrentCulture, Resources.SettingLogicalName, inputFileIdentity, logicalName));
                    ti.SetMetadata("LogicalName", logicalName);
                    _newResources.Add(ti);
                }
                else
                {
                    Log.LogError(string.Format(CultureInfo.CurrentCulture, Resources.SetLogicalNamesIncorrectPath, inputFileIdentity, ResourceOutputPath));
                }
            }

            return true;
        }
    }
}
