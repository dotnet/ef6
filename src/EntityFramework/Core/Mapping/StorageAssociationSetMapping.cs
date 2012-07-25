// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Represents the Mapping metadata for an AssociationSet in CS space.
    /// </summary>
    /// <example>
    /// For Example if conceptually you could represent the CS MSL file as following
    /// --Mapping 
    ///   --EntityContainerMapping ( CNorthwind-->SNorthwind )
    ///     --EntitySetMapping
    ///       --EntityTypeMapping
    ///         --MappingFragment
    ///       --EntityTypeMapping
    ///         --MappingFragment
    ///     --AssociationSetMapping 
    ///       --AssociationTypeMapping
    ///         --MappingFragment
    /// This class represents the metadata for the AssociationSetMapping elements in the
    /// above example. And it is possible to access the AssociationTypeMap underneath it.
    /// There will be only one TypeMap under AssociationSetMap.
    /// </example>
    internal class StorageAssociationSetMapping : StorageSetMapping
    {
        #region Constructors

        /// <summary>
        /// Construct a new AssociationSetMapping object
        /// </summary>
        /// <param name="extent">Represents the Association Set Metadata object. Will
        ///                      change this to Extent instead of MemberMetadata.</param>
        /// <param name="entityContainerMapping">The entityContainerMapping mapping that contains this Set mapping</param>
        internal StorageAssociationSetMapping(AssociationSet extent, StorageEntityContainerMapping entityContainerMapping)
            : base(extent, entityContainerMapping)
        {
        }

        #endregion

        #region Fields

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets function mapping information for this association set. May be null.
        /// </summary>
        internal StorageAssociationSetModificationFunctionMapping ModificationFunctionMapping { get; set; }

        internal EntitySetBase StoreEntitySet
        {
            get
            {
                if ((TypeMappings.Count != 0)
                    && (TypeMappings.First().MappingFragments.Count != 0))
                {
                    return TypeMappings.First().MappingFragments.First().TableSet;
                }
                return null;
            }
        }

        #endregion

        #region Methods

#if DEBUG
    /// <summary>
    /// This method is primarily for debugging purposes.
    /// Will be removed shortly.
    /// </summary>
    /// <param name="index"></param>
        internal override void Print(int index)
        {
            StorageEntityContainerMapping.GetPrettyPrintString(ref index);
            var sb = new StringBuilder();
            sb.Append("AssociationSetMapping");
            sb.Append("   ");
            sb.Append("Name:");
            sb.Append(Set.Name);
            if (QueryView != null)
            {
                sb.Append("   ");
                sb.Append("Query View:");
                sb.Append(QueryView);
            }
            Console.WriteLine(sb.ToString());
            foreach (var typeMapping in TypeMappings)
            {
                typeMapping.Print(index + 5);
            }
            if (ModificationFunctionMapping != null)
            {
                ModificationFunctionMapping.Print(index + 5);
            }
        }
#endif

        #endregion
    }
}
