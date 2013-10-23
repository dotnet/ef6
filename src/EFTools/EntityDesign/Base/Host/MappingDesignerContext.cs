// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Base.Host
{
    using Microsoft.Data.Tools.XmlDesignerBase.Model;

    /// <summary>
    ///     The MappingDesignerContext provides contextual services to the
    ///     designer.  All IO through the designer is handled through the
    ///     provider objects offered through this class.
    ///     Any provider may be null.  If null, the features the designer
    ///     offers that require the provider will be not-enabled.  If enough
    ///     providers are null the designer will do nothing.
    /// </summary>
    internal class MappingDesignerContext
    {
        /// <summary>
        ///     Provides access to the given provider to the designer.  If this
        ///     value is null those features of the designer that rely on this
        ///     provider will not be available.
        /// </summary>
        internal ModelInformationProvider ModelInformationProvider { get; set; }

        /// <summary>
        ///     Provides access to the given provider to the designer.  If this
        ///     value is null those features of the designer that rely on this
        ///     provider will not be available.
        /// </summary>
        internal XmlModelProvider XmlModelProvider { get; set; }
    }
}
