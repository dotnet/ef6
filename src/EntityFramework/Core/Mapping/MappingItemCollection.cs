// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Class for representing a collection of mapping items in Edm space.
    /// </summary>
    public abstract class MappingItemCollection : ItemCollection
    {
        /// <summary>
        ///     The default constructor for ItemCollection
        /// </summary>
        internal MappingItemCollection(DataSpace dataSpace)
            : base(dataSpace)
        {
        }

        /// <summary>
        ///     Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <param name="identity"> identity of the type </param>
        /// <param name="typeSpace"> The dataspace that the type for which map needs to be returned belongs to </param>
        /// <param name="map"> </param>
        /// <returns> Returns false if no match found. </returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1614:ElementParameterDocumentationMustHaveText")]
        internal virtual bool TryGetMap(string identity, DataSpace typeSpace, out Map map)
        {
            //will only be implemented by Mapping Item Collections
            throw Error.NotSupported();
        }

        /// <summary>
        ///     Search for a Mapping metadata with the specified type key.
        /// </summary>
        internal virtual Map GetMap(GlobalItem item)
        {
            DebugCheck.NotNull(item);

            //will only be implemented by Mapping Item Collections
            throw Error.NotSupported();
        }

        /// <summary>
        ///     Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <returns> Returns false if no match found. </returns>
        internal virtual bool TryGetMap(GlobalItem item, out Map map)
        {
            //will only be implemented by Mapping Item Collections
            throw Error.NotSupported();
        }

        /// <summary>
        ///     Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <param name="identity"> identity of the type </param>
        /// <param name="typeSpace"> The dataspace that the type for which map needs to be returned belongs to </param>
        /// <param name="ignoreCase"> true for case-insensitive lookup </param>
        /// <exception cref="ArgumentException">Thrown if mapping space is not valid</exception>
        internal virtual Map GetMap(string identity, DataSpace typeSpace, bool ignoreCase)
        {
            DebugCheck.NotNull(identity);

            //will only be implemented by Mapping Item Collections
            throw Error.NotSupported();
        }

        /// <summary>
        ///     Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <param name="identity"> identity of the type </param>
        /// <param name="typeSpace"> The dataspace that the type for which map needs to be returned belongs to </param>
        /// <param name="ignoreCase"> true for case-insensitive lookup </param>
        /// <param name="map"> </param>
        /// <returns> Returns false if no match found. </returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1614:ElementParameterDocumentationMustHaveText")]
        internal virtual bool TryGetMap(string identity, DataSpace typeSpace, bool ignoreCase, out Map map)
        {
            //will only be implemented by Mapping Item Collections
            throw Error.NotSupported();
        }

        /// <summary>
        ///     Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <param name="identity"> identity of the type </param>
        /// <param name="typeSpace"> The dataspace that the type for which map needs to be returned belongs to </param>
        /// <exception cref="ArgumentException">Thrown if mapping space is not valid</exception>
        internal virtual Map GetMap(string identity, DataSpace typeSpace)
        {
            DebugCheck.NotNull(identity);

            //will only be implemented by Mapping Item Collections
            throw Error.NotSupported();
        }
    }
}
