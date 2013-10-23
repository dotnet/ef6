// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Base.Host
{
    using System;

    /// <summary>
    ///     This class defines an Escher service that provides a schema
    ///     designer given a DesignerContext.
    /// </summary>
    internal abstract class MappingDesignerFactory
    {
        /// <summary>
        ///     Creates a new designer.
        /// </summary>
        /// <param name="context">
        ///     The designer context that provides contextual information to the designer.
        /// </param>
        /// <exception cref="ArgumentNullException"> if context is null.</exception>
        internal abstract MappingDesigner CreateDesigner(MappingDesignerContext context);
    }
}
