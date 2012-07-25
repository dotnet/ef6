// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Metadata.Edm
{
    /// <summary>
    /// DataSpace
    /// </summary>
    public enum DataSpace
    {
        /// <summary>
        /// OSpace indicates the item in the clr space
        /// </summary>
        OSpace = 0,

        /// <summary>
        /// CSpace indicates the item in the CSpace - edm primitive types + 
        /// types defined in csdl
        /// </summary>
        CSpace = 1,

        /// <summary>
        /// SSpace indicates the item in the SSpace
        /// </summary>
        SSpace = 2,

        /// <summary>
        /// Mapping between OSpace and CSpace
        /// </summary>
        OCSpace = 3,

        /// <summary>
        /// Mapping between CSpace and SSpace
        /// </summary>
        CSSpace = 4
    }
}
