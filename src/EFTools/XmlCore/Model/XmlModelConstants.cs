// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    internal static class XmlModelConstants
    {
        /// <summary>
        ///     Refers to merge elements that can update the model, but cannot be updated.
        /// </summary>
        internal static readonly string MergeMode_OneWay = "OneWay";

        /// <summary>
        ///     Refers to merge elements that can update the model and are updated from the model.
        /// </summary>
        internal static readonly string MergeMode_TwoWay = "TwoWay";

        /// <summary>
        ///     Refers to merge elements that fully replace their corresponding model nodes and
        ///     their children with the merge element's children. They cannot be updated by the model.
        /// </summary>
        internal static readonly string MergeMode_OneWayReplace = "OneWayReplace";

        /// <summary>
        ///     Refers to elements that will not be touched by any part of the merge process.
        /// </summary>
        internal static readonly string MergeMode_None = "None";
    }
}
