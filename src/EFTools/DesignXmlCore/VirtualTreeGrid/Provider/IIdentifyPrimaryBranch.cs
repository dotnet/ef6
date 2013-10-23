// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid
{
    /// <summary>
    ///     Level shifting with a replacement branch representing a different
    ///     grouping state is pretty common. IIdentifyPrimaryBranch provides a standard mechanism
    ///     to find the main object for a given set of data, regardless of the
    ///     number of headers subdividing it.
    /// </summary>
    internal interface IIdentifyPrimaryTreeBranch
    {
        /// <summary>
        ///     The primary branch associated with this branch. The branch may or may not be in the tree.
        /// </summary>
        IBranch PrimaryBranch { get; }
    }
}
