using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml;
using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Mapping {
    /// <summary>
    /// Mapping metadata for complex member maps.
    /// </summary>
    internal class ObjectComplexPropertyMapping : ObjectPropertyMapping
    {
        #region Constructors

        /// <summary>
        /// Constrcut a new member mapping metadata object
        /// </summary>
        /// <param name="edmProperty"></param>
        /// <param name="clrProperty"></param>
        internal ObjectComplexPropertyMapping(EdmProperty edmProperty, EdmProperty clrProperty)
            : base(edmProperty, clrProperty)
        {
        }
        #endregion

        #region Fields

        #endregion

        #region Properties
        /// <summary>
        /// return the member mapping kind
        /// </summary>
        internal override MemberMappingKind MemberMappingKind
        {
            get
            {
                return MemberMappingKind.ComplexPropertyMapping;
            }
        }
        #endregion
    }
}
