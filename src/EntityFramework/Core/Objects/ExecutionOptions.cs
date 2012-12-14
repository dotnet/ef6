// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    /// <summary>
    ///     Options for query execution.
    /// </summary>
    public class ExecutionOptions
    {
        internal static readonly ExecutionOptions Default = new ExecutionOptions(MergeOption.AppendOnly, false);

        /// <summary>
        ///     Creates a new instance of <see cref="ExecutionOptions"/>.
        /// </summary>
        /// <param name="mergeOption"> Merge option to use for entity results. </param>
        /// <param name="streaming"> Whether the query is streaming or buffering. </param>
        public ExecutionOptions(MergeOption mergeOption, bool streaming)
        {
            MergeOption = mergeOption;
            Streaming = streaming;
        }

       public MergeOption MergeOption { get; private set; }
       public bool Streaming { get; private set; }
    }
}
