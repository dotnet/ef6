namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;

    /// <summary>
    /// Mapping metadata for all OC member maps.
    /// </summary>
    internal class ObjectPropertyMapping : ObjectMemberMapping
    {
        #region Constructors

        /// <summary>
        /// Constrcut a new member mapping metadata object
        /// </summary>
        /// <param name="edmProperty"></param>
        /// <param name="clrProperty"></param>
        internal ObjectPropertyMapping(EdmProperty edmProperty, EdmProperty clrProperty)
            :
                base(edmProperty, clrProperty)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// The PropertyMetadata object that represents the Clr member for which mapping is being specified
        /// </summary>
        internal EdmProperty ClrProperty
        {
            get { return (EdmProperty)ClrMember; }
        }

        /// <summary>
        /// return the member mapping kind
        /// </summary>
        internal override MemberMappingKind MemberMappingKind
        {
            get { return MemberMappingKind.ScalarPropertyMapping; }
        }

        #endregion
    }
}
