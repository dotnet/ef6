// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.Internal;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    ///     Describes top-level query mapping view projection of the form:
    /// 
    ///     SELECT VALUE CASE 
    ///     WHEN Discriminator = DiscriminatorValue1 THEN EntityType1(...)
    ///     WHEN Discriminator = DiscriminatorValue2 THEN EntityType2(...)
    ///     ...
    ///     
    ///     Supports optimizing queries to leverage user supplied discriminator values
    ///     in TPH mappings rather than introducing our own. This avoids the need
    ///     to introduce a CASE statement in the store.
    /// </summary>
    internal class DiscriminatorMap
    {
        /// <summary>
        ///     Expression retrieving discriminator value from projection input.
        /// </summary>
        internal readonly DbPropertyExpression Discriminator;

        /// <summary>
        ///     Map from discriminator values to implied entity type.
        /// </summary>
        internal readonly ReadOnlyCollection<KeyValuePair<object, EntityType>> TypeMap;

        /// <summary>
        ///     Map from entity property to expression generating value for that property. Note that
        ///     the expression must be the same for all types in discriminator map.
        /// </summary>
        internal readonly ReadOnlyCollection<KeyValuePair<EdmProperty, DbExpression>> PropertyMap;

        /// <summary>
        ///     Map from entity relproperty to expression generating value for that property. Note that
        ///     the expression must be the same for all types in discriminator map.
        /// </summary>
        internal readonly ReadOnlyCollection<KeyValuePair<RelProperty, DbExpression>> RelPropertyMap;

        /// <summary>
        ///     EntitySet to which the map applies.
        /// </summary>
        internal readonly EntitySet EntitySet;

        private DiscriminatorMap(
            DbPropertyExpression discriminator,
            List<KeyValuePair<object, EntityType>> typeMap,
            Dictionary<EdmProperty, DbExpression> propertyMap,
            Dictionary<RelProperty, DbExpression> relPropertyMap,
            EntitySet entitySet)
        {
            Discriminator = discriminator;
            TypeMap = typeMap.AsReadOnly();
            PropertyMap = propertyMap.ToList().AsReadOnly();
            RelPropertyMap = relPropertyMap.ToList().AsReadOnly();
            EntitySet = entitySet;
        }

        /// <summary>
        ///     Determines whether the given query view matches the discriminator map pattern.
        /// </summary>
        internal static bool TryCreateDiscriminatorMap(EntitySet entitySet, DbExpression queryView, out DiscriminatorMap discriminatorMap)
        {
            discriminatorMap = null;

            if (queryView.ExpressionKind
                != DbExpressionKind.Project)
            {
                return false;
            }
            var project = (DbProjectExpression)queryView;

            if (project.Projection.ExpressionKind
                != DbExpressionKind.Case)
            {
                return false;
            }
            var caseExpression = (DbCaseExpression)project.Projection;
            if (project.Projection.ResultType.EdmType.BuiltInTypeKind
                != BuiltInTypeKind.EntityType)
            {
                return false;
            }

            // determine value domain by walking filter
            if (project.Input.Expression.ExpressionKind
                != DbExpressionKind.Filter)
            {
                return false;
            }
            var filterExpression = (DbFilterExpression)project.Input.Expression;

            var discriminatorDomain = new HashSet<object>();
            if (
                !ViewSimplifier.TryMatchDiscriminatorPredicate(
                    filterExpression, (equalsExp, discriminatorValue) => discriminatorDomain.Add(discriminatorValue)))
            {
                return false;
            }

            var typeMap = new List<KeyValuePair<object, EntityType>>();
            var propertyMap = new Dictionary<EdmProperty, DbExpression>();
            var relPropertyMap = new Dictionary<RelProperty, DbExpression>();
            var typeToRelPropertyMap = new Dictionary<EntityType, List<RelProperty>>();
            DbPropertyExpression discriminator = null;

            EdmProperty discriminatorProperty = null;
            for (var i = 0; i < caseExpression.When.Count; i++)
            {
                var when = caseExpression.When[i];
                var then = caseExpression.Then[i];

                var projectionVariableName = project.Input.VariableName;

                DbPropertyExpression currentDiscriminator;
                object discriminatorValue;
                if (
                    !ViewSimplifier.TryMatchPropertyEqualsValue(
                        when, projectionVariableName, out currentDiscriminator, out discriminatorValue))
                {
                    return false;
                }

                // must be the same discriminator in every case
                if (null == discriminatorProperty)
                {
                    discriminatorProperty = (EdmProperty)currentDiscriminator.Property;
                }
                else if (discriminatorProperty != currentDiscriminator.Property)
                {
                    return false;
                }
                discriminator = currentDiscriminator;

                // right hand side must be entity type constructor
                EntityType currentType;
                if (!TryMatchEntityTypeConstructor(then, propertyMap, relPropertyMap, typeToRelPropertyMap, out currentType))
                {
                    return false;
                }

                // remember type + discriminator value
                typeMap.Add(new KeyValuePair<object, EntityType>(discriminatorValue, currentType));

                // remove discriminator value from domain
                discriminatorDomain.Remove(discriminatorValue);
            }

            // make sure only one member of discriminator domain remains...
            if (1 != discriminatorDomain.Count)
            {
                return false;
            }

            // check default case
            EntityType elseType;
            if (null == caseExpression.Else
                ||
                !TryMatchEntityTypeConstructor(caseExpression.Else, propertyMap, relPropertyMap, typeToRelPropertyMap, out elseType))
            {
                return false;
            }
            typeMap.Add(new KeyValuePair<object, EntityType>(discriminatorDomain.Single(), elseType));

            // Account for cases where some type in the hierarchy specifies a rel-property, but another
            // type in the hierarchy does not
            if (!CheckForMissingRelProperties(relPropertyMap, typeToRelPropertyMap))
            {
                return false;
            }

            // since the store may right-pad strings, ensure discriminator values are unique in their trimmed
            // form
            var discriminatorValues = typeMap.Select(map => map.Key);
            var uniqueValueCount = discriminatorValues.Distinct(TrailingSpaceComparer.Instance).Count();
            var valueCount = typeMap.Count;
            if (uniqueValueCount != valueCount)
            {
                return false;
            }

            discriminatorMap = new DiscriminatorMap(discriminator, typeMap, propertyMap, relPropertyMap, entitySet);
            return true;
        }

        private static bool CheckForMissingRelProperties(
            Dictionary<RelProperty, DbExpression> relPropertyMap,
            Dictionary<EntityType, List<RelProperty>> typeToRelPropertyMap)
        {
            // Easily the lousiest implementation of this search.
            // Check to see that for each relProperty that we see in the relPropertyMap
            // (presumably because some type constructor specified it), every type for
            // which that rel-property is specified *must* also have specified it.
            // We don't need to check for equivalence here - because that's already been
            // checked
            foreach (var relProperty in relPropertyMap.Keys)
            {
                foreach (var kv in typeToRelPropertyMap)
                {
                    if (kv.Key.IsSubtypeOf(relProperty.FromEnd.TypeUsage.EdmType))
                    {
                        if (!kv.Value.Contains(relProperty))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private static bool TryMatchEntityTypeConstructor(
            DbExpression then,
            Dictionary<EdmProperty, DbExpression> propertyMap,
            Dictionary<RelProperty, DbExpression> relPropertyMap,
            Dictionary<EntityType, List<RelProperty>> typeToRelPropertyMap,
            out EntityType entityType)
        {
            if (then.ExpressionKind
                != DbExpressionKind.NewInstance)
            {
                entityType = null;
                return false;
            }
            var constructor = (DbNewInstanceExpression)then;
            entityType = (EntityType)constructor.ResultType.EdmType;

            // process arguments to constructor (must be aligned across all case statements)
            Debug.Assert(entityType.Properties.Count == constructor.Arguments.Count, "invalid new instance");
            for (var j = 0; j < entityType.Properties.Count; j++)
            {
                var property = entityType.Properties[j];
                var assignment = constructor.Arguments[j];
                DbExpression existingAssignment;
                if (propertyMap.TryGetValue(property, out existingAssignment))
                {
                    if (!ExpressionsCompatible(assignment, existingAssignment))
                    {
                        return false;
                    }
                }
                else
                {
                    propertyMap.Add(property, assignment);
                }
            }

            // Now handle the rel properties
            if (constructor.HasRelatedEntityReferences)
            {
                List<RelProperty> relPropertyList;
                if (!typeToRelPropertyMap.TryGetValue(entityType, out relPropertyList))
                {
                    relPropertyList = new List<RelProperty>();
                    typeToRelPropertyMap[entityType] = relPropertyList;
                }
                foreach (var relatedRef in constructor.RelatedEntityReferences)
                {
                    var relProperty = new RelProperty(
                        (RelationshipType)relatedRef.TargetEnd.DeclaringType,
                        relatedRef.SourceEnd, relatedRef.TargetEnd);
                    var assignment = relatedRef.TargetEntityReference;
                    DbExpression existingAssignment;
                    if (relPropertyMap.TryGetValue(relProperty, out existingAssignment))
                    {
                        if (!ExpressionsCompatible(assignment, existingAssignment))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        relPropertyMap.Add(relProperty, assignment);
                    }
                    relPropertyList.Add(relProperty);
                }
            }
            return true;
        }

        /// <summary>
        ///     Utility method determining whether two expressions appearing within the same scope
        ///     are equivalent. May return false negatives, but no false positives. In other words,
        /// 
        ///     x != y --> !ExpressionsCompatible(x, y)
        ///     
        ///     but does not guarantee
        /// 
        ///     x == y --> ExpressionsCompatible(x, y)
        /// </summary>
        private static bool ExpressionsCompatible(DbExpression x, DbExpression y)
        {
            if (x.ExpressionKind
                != y.ExpressionKind)
            {
                return false;
            }
            switch (x.ExpressionKind)
            {
                case DbExpressionKind.Property:
                    {
                        var prop1 = (DbPropertyExpression)x;
                        var prop2 = (DbPropertyExpression)y;
                        return prop1.Property == prop2.Property &&
                               ExpressionsCompatible(prop1.Instance, prop2.Instance);
                    }
                case DbExpressionKind.VariableReference:
                    return ((DbVariableReferenceExpression)x).VariableName ==
                           ((DbVariableReferenceExpression)y).VariableName;
                case DbExpressionKind.NewInstance:
                    {
                        var newX = (DbNewInstanceExpression)x;
                        var newY = (DbNewInstanceExpression)y;
                        if (!newX.ResultType.EdmType.EdmEquals(newY.ResultType.EdmType))
                        {
                            return false;
                        }
                        for (var i = 0; i < newX.Arguments.Count; i++)
                        {
                            if (!ExpressionsCompatible(newX.Arguments[i], newY.Arguments[i]))
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                case DbExpressionKind.Ref:
                    {
                        var refX = (DbRefExpression)x;
                        var refY = (DbRefExpression)y;
                        return (refX.EntitySet.EdmEquals(refY.EntitySet) &&
                                ExpressionsCompatible(refX.Argument, refY.Argument));
                    }
                default:
                    // here come the false negatives...
                    return false;
            }
        }
    }
}
