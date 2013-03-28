// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core
{
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     DataRecord interface supporting structured types and rich metadata information.
    /// </summary>
    public interface IExtendedDataRecord : IDataRecord
    {
        /// <summary>
        ///     Gets <see cref="T:System.Data.Entity.Core.Common.DataRecordInfo" /> for this
        ///     <see
        ///         cref="T:System.Data.Entity.Core.IExtendedDataRecord" />
        ///     .
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Data.Entity.Core.Common.DataRecordInfo" /> object.
        /// </returns>
        DataRecordInfo DataRecordInfo { get; }

        /// <summary>
        ///     Gets a <see cref="T:System.Data.Common.DbDataRecord" /> object with the specified index.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Data.Common.DbDataRecord" /> object.
        /// </returns>
        /// <param name="i">The index of the row.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "i")]
        DbDataRecord GetDataRecord(int i);

        /// <summary>
        ///     Returns nested readers as <see cref="T:System.Data.Common.DbDataReader" /> objects.
        /// </summary>
        /// <returns>
        ///     Nested readers as <see cref="T:System.Data.Common.DbDataReader" /> objects.
        /// </returns>
        /// <param name="i">The ordinal of the column.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "i")]
        DbDataReader GetDataReader(int i);
    }
}
