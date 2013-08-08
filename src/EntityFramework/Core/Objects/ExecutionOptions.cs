// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    /// <summary>
    /// Options for query execution.
    /// </summary>
    public class ExecutionOptions
    {
        internal static readonly ExecutionOptions Default = new ExecutionOptions(MergeOption.AppendOnly, false);

        /// <summary>
        /// Creates a new instance of <see cref="ExecutionOptions" />.
        /// </summary>
        /// <param name="mergeOption"> Merge option to use for entity results. </param>
        /// <param name="streaming"> Whether the query is streaming or buffering. </param>
        public ExecutionOptions(MergeOption mergeOption, bool streaming)
        {
            MergeOption = mergeOption;
            Streaming = streaming;
        }

        /// <summary>
        /// Merge option to use for entity results.
        /// </summary>
        public MergeOption MergeOption { get; private set; }

        /// <summary>
        /// Whether the query is streaming or buffering.
        /// </summary>
        public bool Streaming { get; private set; }

        /// <summary>Determines whether the specified objects are equal.</summary>
        /// <returns>true if the two objects are equal; otherwise, false.</returns>
        /// <param name="left">The left object to compare.</param>
        /// <param name="right">The right object to compare.</param>
        public static bool operator ==(ExecutionOptions left, ExecutionOptions right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null))
            {
                return false;
            }

            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether the specified objects are not equal.
        /// </summary>
        /// <param name="left">The left object to compare.</param>
        /// <param name="right">The right object to compare.</param>
        /// <returns>true if the two objects are not equal; otherwise, false.</returns>
        public static bool operator !=(ExecutionOptions left, ExecutionOptions right)
        {
            return !(left == right);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var otherOptions = obj as ExecutionOptions;
            if (ReferenceEquals(otherOptions, null))
            {
                return false;
            }

            return MergeOption == otherOptions.MergeOption &&
                   Streaming == otherOptions.Streaming;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return MergeOption.GetHashCode() ^ Streaming.GetHashCode();
        }
    }
}
