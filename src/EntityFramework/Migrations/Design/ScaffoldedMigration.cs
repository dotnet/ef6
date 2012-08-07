// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Design
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     Represents a code-based migration that has been scaffolded and is ready to be written to a file.
    /// </summary>
    [Serializable]
    public class ScaffoldedMigration
    {
        private string _migrationId;
        private string _userCode;
        private string _designerCode;
        private string _language;
        private string _directory;
        private readonly Dictionary<string, object> _resources = new Dictionary<string, object>();

        /// <summary>
        ///     Gets or sets the unique identifier for this migration.
        ///     Typically used for the file name of the generated code.
        /// </summary>
        public string MigrationId
        {
            get { return _migrationId; }
            set
            {
                Contract.Requires(!string.IsNullOrWhiteSpace(value));

                _migrationId = value;
            }
        }

        /// <summary>
        ///     Gets or sets the scaffolded migration code that the user can edit.
        /// </summary>
        public string UserCode
        {
            get { return _userCode; }
            set
            {
                Contract.Requires(!string.IsNullOrWhiteSpace(value));

                _userCode = value;
            }
        }

        /// <summary>
        ///     Gets or sets the scaffolded migration code that should be stored in a code behind file.
        /// </summary>
        public string DesignerCode
        {
            get { return _designerCode; }
            set
            {
                Contract.Requires(!string.IsNullOrWhiteSpace(value));

                _designerCode = value;
            }
        }

        /// <summary>
        ///     Gets or sets the programming language used for this migration.
        ///     Typically used for the file extension of the generated code.
        /// </summary>
        public string Language
        {
            get { return _language; }
            set
            {
                Contract.Requires(!string.IsNullOrWhiteSpace(value));

                _language = value;
            }
        }

        /// <summary>
        ///     Gets or sets the subdirectory in the user's project that this migration should be saved in.
        /// </summary>
        public string Directory
        {
            get { return _directory; }
            set
            {
                Contract.Requires(!string.IsNullOrWhiteSpace(value));

                _directory = value;
            }
        }

        /// <summary>
        ///     Gets a dictionary of string resources to add to the migration resource file.
        /// </summary>
        public IDictionary<string, object> Resources
        {
            get { return _resources; }
        }
    }
}
