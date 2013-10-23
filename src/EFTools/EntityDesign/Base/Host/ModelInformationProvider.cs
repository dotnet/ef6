// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Base.Host
{
    using Microsoft.Data.Tools.XmlDesignerBase.Model;

    /// <summary>
    ///     The designer interacts with files through an abstract layer
    ///     called a model.  This layer may expose multiple files in
    ///     any directories on the file system, or it may even expose
    ///     virtual files that reside in a database.  The
    ///     ModelInformationProvider provides general purpose
    ///     information and events about the model.  While many of
    ///     these events are specific to a particular file, the designer
    ///     considers multiple files in the model as a single entity.
    ///     For example, if one file is checked out from source control,
    ///     all are checked out from source control.  This presents a
    ///     unified view to the user and helps in cases where some of the
    ///     files may not be shown to the user.
    ///     If this provider is not supplied in the context the designer
    ///     may display degraded error messages that do not include the
    ///     display name of the designer.
    /// </summary>
    internal abstract class ModelInformationProvider
    {
        /// <summary>
        ///     Returns the name of this designer model.  This name may
        ///     be presented to the user to identify the designer.
        ///     Generally, this is the short file name of the file being
        ///     designed:  “Page1” or “MyCustomDialog”, for example.
        /// </summary>
        internal abstract string DisplayName { get; }

        /// <summary>
        ///     Returns the primary file model for this designer.
        /// </summary>
        internal abstract XmlModel FileModel { get; }
    }
}
