namespace System.Data.Entity.Core {

    using System.Data.Entity.Core.Common;
    using System.Data.Common;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// DataRecord interface supporting structured types and rich metadata information.
    /// </summary>
    public interface IExtendedDataRecord : IDataRecord {

        /// <summary>
        /// DataRecordInfo property describing the contents of the record.
        /// </summary>
        DataRecordInfo DataRecordInfo { get;}

        /// <summary>
        /// Used to return a nested DbDataRecord.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "i")]
        DbDataRecord GetDataRecord(int i);

        /// <summary>
        /// Used to return a nested result
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "i")]
        DbDataReader GetDataReader(int i);
    }
}