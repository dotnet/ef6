// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;

    // <summary>
    // Encapsulates the result (represented as a Ref to the resulting Entity) of navigating from
    // the specified source end of a relationship to the specified target end. This class is intended
    // for use only with <see cref="DbNewInstanceExpression" />, where an 'owning' instance of that class
    // represents the source Entity involved in the relationship navigation.
    // Instances of DbRelatedEntityRef may be specified when creating a <see cref="DbNewInstanceExpression" /> that
    // constructs an Entity, allowing information about Entities that are related to the newly constructed Entity to be captured.
    // </summary>
    internal sealed class DbRelatedEntityRef
    {
        private readonly RelationshipEndMember _sourceEnd;
        private readonly RelationshipEndMember _targetEnd;
        private readonly DbExpression _targetEntityRef;

        internal DbRelatedEntityRef(RelationshipEndMember sourceEnd, RelationshipEndMember targetEnd, DbExpression targetEntityRef)
        {
            // Validate that the specified relationship ends are:
            // 1. Non-null
            // 2. From the same metadata workspace as that used by the command tree
            DebugCheck.NotNull(sourceEnd);
            DebugCheck.NotNull(targetEnd);

            // Validate that the specified target entity ref is:
            // 1. Non-null
            DebugCheck.NotNull(targetEntityRef);

            // Validate that the specified source and target ends are:
            // 1. Declared by the same relationship type
            if (!ReferenceEquals(sourceEnd.DeclaringType, targetEnd.DeclaringType))
            {
                throw new ArgumentException(Strings.Cqt_RelatedEntityRef_TargetEndFromDifferentRelationship, "targetEnd");
            }
            // 2. Not the same end
            if (ReferenceEquals(sourceEnd, targetEnd))
            {
                throw new ArgumentException(Strings.Cqt_RelatedEntityRef_TargetEndSameAsSourceEnd, "targetEnd");
            }

            // Validate that the specified target end has multiplicity of at most one
            if (targetEnd.RelationshipMultiplicity != RelationshipMultiplicity.One
                &&
                targetEnd.RelationshipMultiplicity != RelationshipMultiplicity.ZeroOrOne)
            {
                throw new ArgumentException(Strings.Cqt_RelatedEntityRef_TargetEndMustBeAtMostOne, "targetEnd");
            }

            // Validate that the specified target entity ref actually has a ref result type
            if (!TypeSemantics.IsReferenceType(targetEntityRef.ResultType))
            {
                throw new ArgumentException(Strings.Cqt_RelatedEntityRef_TargetEntityNotRef, "targetEntityRef");
            }

            // Validate that the specified target entity is of a type that can be reached by navigating to the specified relationship end
            var endType = TypeHelpers.GetEdmType<RefType>(targetEnd.TypeUsage).ElementType;
            var targetType = TypeHelpers.GetEdmType<RefType>(targetEntityRef.ResultType).ElementType;

            if (!endType.EdmEquals(targetType)
                && !TypeSemantics.IsSubTypeOf(targetType, endType))
            {
                throw new ArgumentException(Strings.Cqt_RelatedEntityRef_TargetEntityNotCompatible, "targetEntityRef");
            }

            // Validation succeeded, initialize state
            _targetEntityRef = targetEntityRef;
            _targetEnd = targetEnd;
            _sourceEnd = sourceEnd;
        }

        // <summary>
        // Retrieves the 'source' end of the relationship navigation satisfied by this related entity Ref
        // </summary>
        internal RelationshipEndMember SourceEnd
        {
            get { return _sourceEnd; }
        }

        // <summary>
        // Retrieves the 'target' end of the relationship navigation satisfied by this related entity Ref
        // </summary>
        internal RelationshipEndMember TargetEnd
        {
            get { return _targetEnd; }
        }

        // <summary>
        // Retrieves the entity Ref that is the result of navigating from the source to the target end of this related entity Ref
        // </summary>
        internal DbExpression TargetEntityReference
        {
            get { return _targetEntityRef; }
        }
    }
}
