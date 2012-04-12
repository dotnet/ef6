namespace System.Data.Entity.Core.Common
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// FieldMetadata class providing the correlation between the column ordinals and MemberMetadata.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    public struct FieldMetadata
    {
        private readonly EdmMember _fieldType;
        private readonly int _ordinal;

        /// <summary>
        /// Used to construct a field metadata object relating a column ordinal and an ImemberMetadata.
        /// </summary>
        /// <param name="ordinal">Column oridnal</param>
        /// <param name="fieldType">Metadata member</param>
        public FieldMetadata(int ordinal, EdmMember fieldType)
        {
            if (ordinal < 0)
            {
                throw EntityUtil.ArgumentOutOfRange("ordinal");
            }
            if (null == fieldType)
            {
                throw EntityUtil.ArgumentNull("fieldType");
            }

            _fieldType = fieldType;
            _ordinal = ordinal;
        }

        /// <summary>
        /// Metadata member.
        /// </summary>
        public EdmMember FieldType
        {
            get { return _fieldType; }
        }

        /// <summary>
        /// Column ordinal.
        /// </summary>
        public int Ordinal
        {
            get { return _ordinal; }
        }
    }
}
