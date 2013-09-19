// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Infrastructure
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal class DynamicToFunctionModificationCommandConverter : DefaultExpressionVisitor
    {
        private readonly EntityTypeModificationFunctionMapping _entityTypeModificationFunctionMapping;
        private readonly AssociationSetModificationFunctionMapping _associationSetModificationFunctionMapping;
        private readonly EntityContainerMapping _entityContainerMapping;

        private ModificationFunctionMapping _currentFunctionMapping;
        private EdmProperty _currentProperty;
        private List<EdmProperty> _storeGeneratedKeys;
        private int _nextStoreGeneratedKey;

        public DynamicToFunctionModificationCommandConverter(
            EntityTypeModificationFunctionMapping entityTypeModificationFunctionMapping,
            EntityContainerMapping entityContainerMapping)
        {
            DebugCheck.NotNull(entityTypeModificationFunctionMapping);
            DebugCheck.NotNull(entityContainerMapping);

            _entityTypeModificationFunctionMapping = entityTypeModificationFunctionMapping;
            _entityContainerMapping = entityContainerMapping;
        }

        public DynamicToFunctionModificationCommandConverter(
            AssociationSetModificationFunctionMapping associationSetModificationFunctionMapping,
            EntityContainerMapping entityContainerMapping)
        {
            DebugCheck.NotNull(associationSetModificationFunctionMapping);
            DebugCheck.NotNull(entityContainerMapping);

            _associationSetModificationFunctionMapping = associationSetModificationFunctionMapping;
            _entityContainerMapping = entityContainerMapping;
        }

        public IEnumerable<TCommandTree> Convert<TCommandTree>(
            IEnumerable<TCommandTree> modificationCommandTrees)
            where TCommandTree : DbModificationCommandTree
        {
            DebugCheck.NotNull(modificationCommandTrees);

            _currentFunctionMapping = null;
            _currentProperty = null;
            _storeGeneratedKeys = null;
            _nextStoreGeneratedKey = 0;

            return modificationCommandTrees
                .Select(modificationCommandTree => ConvertInternal((dynamic)modificationCommandTree))
                .Cast<TCommandTree>();
        }

        private DbModificationCommandTree ConvertInternal(DbInsertCommandTree commandTree)
        {
            DebugCheck.NotNull(commandTree);

            if (_currentFunctionMapping == null)
            {
                _currentFunctionMapping
                    = _entityTypeModificationFunctionMapping != null
                        ? _entityTypeModificationFunctionMapping.InsertFunctionMapping
                        : _associationSetModificationFunctionMapping.InsertFunctionMapping;

                var firstTable
                    = ((DbScanExpression)commandTree.Target.Expression).Target.ElementType;

                _storeGeneratedKeys
                    = firstTable.KeyProperties
                        .Where(p => p.IsStoreGeneratedIdentity)
                        .ToList();
            }

            _nextStoreGeneratedKey = 0;

            return
                new DbInsertCommandTree(
                    commandTree.MetadataWorkspace,
                    commandTree.DataSpace,
                    commandTree.Target,
                    VisitSetClauses(commandTree.SetClauses),
                    commandTree.Returning != null ? commandTree.Returning.Accept(this) : null);
        }

        private DbModificationCommandTree ConvertInternal(DbUpdateCommandTree commandTree)
        {
            DebugCheck.NotNull(commandTree);

            _currentFunctionMapping = _entityTypeModificationFunctionMapping.UpdateFunctionMapping;

            return
                new DbUpdateCommandTree(
                    commandTree.MetadataWorkspace,
                    commandTree.DataSpace,
                    commandTree.Target,
                    commandTree.Predicate.Accept(this),
                    VisitSetClauses(commandTree.SetClauses),
                    commandTree.Returning != null ? commandTree.Returning.Accept(this) : null);
        }

        private DbModificationCommandTree ConvertInternal(DbDeleteCommandTree commandTree)
        {
            DebugCheck.NotNull(commandTree);

            _currentFunctionMapping
                = _entityTypeModificationFunctionMapping != null
                    ? _entityTypeModificationFunctionMapping.DeleteFunctionMapping
                    : _associationSetModificationFunctionMapping.DeleteFunctionMapping;

            return
                new DbDeleteCommandTree(
                    commandTree.MetadataWorkspace,
                    commandTree.DataSpace,
                    commandTree.Target,
                    commandTree.Predicate.Accept(this));
        }

        private ReadOnlyCollection<DbModificationClause> VisitSetClauses(IList<DbModificationClause> setClauses)
        {
            DebugCheck.NotNull(setClauses);

            return new ReadOnlyCollection<DbModificationClause>(
                setClauses
                    .Cast<DbSetClause>()
                    .Select(
                        s => new DbSetClause(
                            s.Property.Accept(this),
                            s.Value.Accept(this)))
                    .Cast<DbModificationClause>()
                    .ToList());
        }

        public override DbExpression Visit(DbComparisonExpression expression)
        {
            var equalityPredicate = (DbComparisonExpression)base.Visit(expression);

            var propertyExpression = (DbPropertyExpression)equalityPredicate.Left;
            var property = (EdmProperty)propertyExpression.Property;

            if (property.Nullable)
            {
                // Rewrite to IS NULL

                var nullPredicate
                    = propertyExpression.IsNull().And(equalityPredicate.Right.IsNull());

                return equalityPredicate.Or(nullPredicate);
            }

            return equalityPredicate;
        }

        public override DbExpression Visit(DbPropertyExpression expression)
        {
            DebugCheck.NotNull(expression);

            _currentProperty = (EdmProperty)expression.Property;

            return base.Visit(expression);
        }

        public override DbExpression Visit(DbConstantExpression expression)
        {
            DebugCheck.NotNull(expression);

            if (_currentProperty != null)
            {
                var parameter = GetParameter(_currentProperty);

                if (parameter != null)
                {
                    return new DbParameterReferenceExpression(parameter.Item1.TypeUsage, parameter.Item1.Name);
                }
            }

            return base.Visit(expression);
        }

        public override DbExpression Visit(DbAndExpression expression)
        {
            DebugCheck.NotNull(expression);

            var newLeft = VisitExpression(expression.Left);
            var newRight = VisitExpression(expression.Right);

            if ((newLeft != null)
                && (newRight != null))
            {
                return newLeft.And(newRight);
            }

            return newLeft ?? newRight;
        }

        public override DbExpression Visit(DbIsNullExpression expression)
        {
            DebugCheck.NotNull(expression);

            var propertyExpression
                = expression.Argument as DbPropertyExpression;

            if (propertyExpression != null)
            {
                var parameter
                    = GetParameter((EdmProperty)propertyExpression.Property, originalValue: true);

                if (parameter != null)
                {
                    if (parameter.Item2)
                    {
                        // Current value, remove condition
                        return null;
                    }

                    var parameterReferenceExpression
                        = new DbParameterReferenceExpression(parameter.Item1.TypeUsage, parameter.Item1.Name);

                    var equalityPredicate
                        = propertyExpression.Equal(parameterReferenceExpression);

                    var nullPredicate
                        = propertyExpression.IsNull().And(parameterReferenceExpression.IsNull());

                    return equalityPredicate.Or(nullPredicate);
                }
            }

            return base.Visit(expression);
        }

        public override DbExpression Visit(DbNullExpression expression)
        {
            DebugCheck.NotNull(expression);

            if (_currentProperty != null)
            {
                var parameter = GetParameter(_currentProperty);

                if (parameter != null)
                {
                    return new DbParameterReferenceExpression(parameter.Item1.TypeUsage, parameter.Item1.Name);
                }
            }

            return base.Visit(expression);
        }

        public override DbExpression Visit(DbNewInstanceExpression expression)
        {
            DebugCheck.NotNull(expression);

            // Update the returning new instance expression with the column
            // names from the sproc result binding.
            var arguments
                = (from propertyExpression in expression.Arguments.Cast<DbPropertyExpression>()
                    let resultBinding
                        = _currentFunctionMapping
                            .ResultBindings
                            .Single(
                                rb => (from esm in _entityContainerMapping.EntitySetMappings
                                    from etm in esm.EntityTypeMappings
                                    from mf in etm.MappingFragments
                                    from pm in mf.Properties.OfType<ScalarPropertyMapping>()
                                    where
                                        pm.ColumnProperty.EdmEquals(propertyExpression.Property)
                                        && pm.ColumnProperty.DeclaringType.EdmEquals(propertyExpression.Property.DeclaringType)
                                    select pm.EdmProperty)
                                    .Contains(rb.Property))
                    select new KeyValuePair<string, DbExpression>(resultBinding.ColumnName, propertyExpression))
                    .ToList();

            return DbExpressionBuilder.NewRow(arguments);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private Tuple<FunctionParameter, bool> GetParameter(EdmProperty column, bool originalValue = false)
        {
            DebugCheck.NotNull(column);

            var columnMappings
                = (from esm in _entityContainerMapping.EntitySetMappings
                    from etm in esm.EntityTypeMappings
                    from mf in etm.MappingFragments
                    from cm in mf.FlattenedProperties
                    where cm.ColumnProperty.EdmEquals(column)
                          && cm.ColumnProperty.DeclaringType.EdmEquals(column.DeclaringType)
                    select cm)
                    .ToList();

            var parameterBindings
                = _currentFunctionMapping
                    .ParameterBindings
                    .Where(
                        pb => columnMappings
                            .Any(cm => pb.MemberPath.Members.Reverse().SequenceEqual(cm.PropertyPath)))
                    .ToList();

            if (!parameterBindings.Any())
            {
                var iaColumnMappings
                    = (from asm in _entityContainerMapping.AssociationSetMappings
                        from tm in asm.TypeMappings
                        from mf in tm.MappingFragments
                        from epm in mf.Properties.OfType<EndPropertyMapping>()
                        from pm in epm.PropertyMappings
                        where pm.ColumnProperty.EdmEquals(column)
                              && pm.ColumnProperty.DeclaringType.EdmEquals(column.DeclaringType)
                        select new EdmMember[]
                               {
                                   pm.EdmProperty,
                                   epm.EndMember
                               })
                        .ToList();

                parameterBindings
                    = _currentFunctionMapping
                        .ParameterBindings
                        .Where(
                            pb => iaColumnMappings
                                .Any(epm => pb.MemberPath.Members.SequenceEqual(epm)))
                        .ToList();
            }

            if ((parameterBindings.Count == 0)
                && column.IsPrimaryKeyColumn)
            {
                // Store generated key: Introduce a fake parameter which can
                // be replaced by a local variable in the sproc body.

                return
                    Tuple.Create(
                        new FunctionParameter(
                            _storeGeneratedKeys[_nextStoreGeneratedKey++].Name,
                            column.TypeUsage,
                            ParameterMode.In), true);
            }

            if (parameterBindings.Count == 1)
            {
                return Tuple.Create(parameterBindings[0].Parameter, parameterBindings[0].IsCurrent);
            }

            if (parameterBindings.Count == 0)
            {
                return null;
            }

            Debug.Assert(parameterBindings.Count == 2);

            var parameterBinding
                = originalValue
                    ? parameterBindings.Single(pb => !pb.IsCurrent)
                    : parameterBindings.Single(pb => pb.IsCurrent);

            return Tuple.Create(parameterBinding.Parameter, parameterBinding.IsCurrent);
        }
    }
}
