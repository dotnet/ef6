// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.Refactoring
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Data.Entity.Design.Common;

    /// <summary>
    ///     Captures check-box setting, friendly name and other information for a preview group.
    ///     RefactoringPreviewGroup will only contain group information, will not know list of changes.
    ///     On OperationContributions class we will keep RefactoringPreviewGroup and list of changes mapping.
    /// </summary>
    internal class RefactoringPreviewGroup
    {
        private bool _enableChangeGroupUncheck;
        private bool _enableChangeUncheck;
        private readonly Dictionary<string, Guid> _languageServices;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="name">Preview group name.</param>
        public RefactoringPreviewGroup(string name)
        {
            Name = name;
            _enableChangeGroupUncheck = false;
            _enableChangeUncheck = false;
            DefaultChecked = true;
            _languageServices = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     Friendly name for the group node
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Description for the group node, will be used in tooltip
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Specify if this group will have checkbox to check and uncheck this group of changes.
        /// </summary>
        public bool EnableChangeGroupUncheck
        {
            get { return _enableChangeGroupUncheck; }
            set { _enableChangeGroupUncheck = value; }
        }

        /// <summary>
        ///     Warning message telling user what should be expected after these changes applied for this group.
        /// </summary>
        public string WarningMessage { get; set; }

        /// <summary>
        ///     If the selection for every single change inside this preview group is changable or not.
        /// </summary>
        public bool EnableChangeUncheck
        {
            get { return _enableChangeUncheck; }
            set
            {
                if (_enableChangeGroupUncheck && value)
                {
                    _enableChangeUncheck = true;
                }
                else
                {
                    _enableChangeUncheck = false;
                }
            }
        }

        /// <summary>
        ///     Default check all the changes under this preview group or not.
        /// </summary>
        public bool DefaultChecked { get; set; }

        /// <summary>
        ///     Language services can be registered with specific file extensions.
        ///     This language service is used in the preview window to display the file with the appropriate syntax highlighting.
        /// </summary>
        /// <param name="fileExtension">This is the file extension for which you are registered a language service</param>
        /// <param name="languageService">This is the guid of the language service.</param>
        public void RegisterLanguageService(string fileExtension, Guid languageService)
        {
            if (fileExtension != null
                && languageService != Guid.Empty)
            {
                Guid temp;
                if (!_languageServices.TryGetValue(fileExtension, out temp))
                {
                    _languageServices.Add(fileExtension, languageService);
                }
            }
        }

        /// <summary>
        ///     Get the language service registed for passed in file extension.
        /// </summary>
        /// <param name="fileExtension">This is the file extension for which you want to get language service for it.</param>
        /// <returns>Registed language service for this file extension.  If nothing is registed, null will be returned.</returns>
        internal Guid GetLanguageService(string fileExtension)
        {
            ArgumentValidation.CheckForEmptyString(fileExtension, "fileExtension");

            var languageServiceID = Guid.Empty;
            _languageServices.TryGetValue(fileExtension, out languageServiceID);
            return languageServiceID;
        }
    }
}
