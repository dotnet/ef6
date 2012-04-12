namespace System.Data.Entity.Core.Objects.DataClasses
{
    using System.Data.Entity.Core.Metadata.Edm;

    [Serializable]
    internal class RelationshipFixer<TSourceEntity, TTargetEntity> : IRelationshipFixer
        where TSourceEntity : class
        where TTargetEntity : class
    {
        // The following fields are serialized.  Adding or removing a serialized field is considered
        // a breaking change.  This includes changing the field type or field name of existing
        // serialized fields. If you need to make this kind of change, it may be possible, but it
        // will require some custom serialization/deserialization code.
        private readonly RelationshipMultiplicity _sourceRoleMultiplicity;
        private readonly RelationshipMultiplicity _targetRoleMultiplicity;

        internal RelationshipFixer(RelationshipMultiplicity sourceRoleMultiplicity, RelationshipMultiplicity targetRoleMultiplicity)
        {
            _sourceRoleMultiplicity = sourceRoleMultiplicity;
            _targetRoleMultiplicity = targetRoleMultiplicity;
        }

        /// <summary>
        /// Used during relationship fixup when the source end of the relationship is not
        /// yet in the relationships list, and needs to be created
        /// </summary>
        /// <param name="navigation">RelationshipNavigation to be set on new RelatedEnd</param>
        /// <param name="relationshipManager">RelationshipManager to use for creating the new end</param>
        /// <returns>Reference to the new collection or reference on the other end of the relationship</returns>
        RelatedEnd IRelationshipFixer.CreateSourceEnd(RelationshipNavigation navigation, RelationshipManager relationshipManager)
        {
            return relationshipManager.CreateRelatedEnd<TTargetEntity, TSourceEntity>(
                navigation, _targetRoleMultiplicity, _sourceRoleMultiplicity, /*existingRelatedEnd*/ null);
        }
    }
}
