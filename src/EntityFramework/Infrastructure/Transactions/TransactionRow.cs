// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    /// <summary>
    /// Rrepresents a transaction
    /// </summary>
    public class TransactionRow
    {
        /// <summary>
        /// A unique id assigned to a transaction object.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The local time when the transaction was started.
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var other = obj as TransactionRow;
            return other != null
                   && Id == other.Id;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
