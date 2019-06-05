// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Tools.Migrations.Design
{
    using System.Collections.Generic;

    // <summary>
    // Represents a code-based migration that has been scaffolded and is ready to be written to a file.
    // </summary>
    internal class ScaffoldedMigration
    {
        // <summary>
        // Gets or sets the unique identifier for this migration.
        // Typically used for the file name of the generated code.
        // </summary>
        public string MigrationId { get; set; }

        // <summary>
        // Gets or sets the scaffolded migration code that the user can edit.
        // </summary>
        public string UserCode { get; set; }

        // <summary>
        // Gets or sets the scaffolded migration code that should be stored in a code behind file.
        // </summary>
        public string DesignerCode { get; set; }

        // <summary>
        // Gets or sets the programming language used for this migration.
        // Typically used for the file extension of the generated code.
        // </summary>
        public string Language { get; set; }

        // <summary>
        // Gets or sets the subdirectory in the user's project that this migration should be saved in.
        // </summary>
        public string Directory { get; set; }

        // <summary>
        // Gets a dictionary of string resources to add to the migration resource file.
        // </summary>
        public IDictionary<string, object> Resources { get; } = new Dictionary<string, object>();

        // <summary>
        // Gets or sets whether the migration was re-scaffolded.
        // </summary>
        public bool IsRescaffold { get; set; }
    }
}
