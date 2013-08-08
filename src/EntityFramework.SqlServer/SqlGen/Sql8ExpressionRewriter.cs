// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer.SqlGen
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.SqlServer.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// Rewrites an expression tree to make it suitable for translation to SQL appropriate for SQL Server 2000
    /// In particular, it replaces expressions that are not directly supported on SQL Server 2000
    /// with alternative translations. The following expressions are translated:
    /// <list type="bullet">
    ///     <item>
    ///         <see cref="DbExceptExpression" />
    ///     </item>
    ///     <item>
    ///         <see cref="DbIntersectExpression" />
    ///     </item>
    ///     <item>
    ///         <see cref="DbSkipExpression" />
    ///     </item>
    /// </list>
    /// The other expressions are copied unmodified.
    /// The new expression belongs to a new query command tree.
    /// </summary>
    internal class Sql8ExpressionRewriter : DbExpressionRebinder
    {
        #region Entry Point

        /// <summary>
        /// The only entry point.
        /// Rewrites the given tree by replacing expressions that are not directly supported on SQL Server 2000
        /// with alterntive translations.
        /// </summary>
        /// <param name="originalTree"> The tree to rewrite </param>
        /// <returns> The new tree </returns>
        internal static DbQueryCommandTree Rewrite(DbQueryCommandTree originalTree)
        {
            DebugCheck.NotNull(originalTree);
            var rewriter = new Sql8ExpressionRewriter(originalTree.MetadataWorkspace);
            var newQuery = rewriter.VisitExpression(originalTree.Query);

#if DEBUG
            return new DbQueryCommandTree(originalTree.MetadataWorkspace, originalTree.DataSpace, newQuery);
#else
            return new DbQueryCommandTree(originalTree.MetadataWorkspace, originalTree.DataSpace, newQuery, validate: false);
#endif
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Private Constructor.
        /// </summary>
        private Sql8ExpressionRewriter(MetadataWorkspace metadata)
            : base(metadata)
        {
        }

        #endregion

        #region DbExpressionVisitor<DbExpression> Members

        /// <summary>
        /// <see
        ///     cref="TransformIntersectOrExcept(DbExpression, DbExpression, DbExpressionKind, System.Collections.Generic.IList{System.Data.Entity.Core.Common.CommandTrees.DbPropertyExpression}, string)" />
        /// </summary>
        public override DbExpression Visit(DbExceptExpression e)
        {
            Check.NotNull(e, "e");

            return TransformIntersectOrExcept(VisitExpression(e.Left), VisitExpression(e.Right), DbExpressionKind.Except);
        }

        /// <summary>
        /// <see
        ///     cref="TransformIntersectOrExcept(DbExpression, DbExpression, DbExpressionKind, System.Collections.Generic.IList{System.Data.Entity.Core.Common.CommandTrees.DbPropertyExpression}, string)" />
        /// </summary>
        public override DbExpression Visit(DbIntersectExpression e)
        {
            Check.NotNull(e, "e");

            return TransformIntersectOrExcept(VisitExpression(e.Left), VisitExpression(e.Right), DbExpressionKind.Intersect);
        }

        /// <summary>
        /// <see
        ///     cref="TransformIntersectOrExcept(DbExpression, DbExpression, DbExpressionKind, System.Collections.Generic.IList{System.Data.Entity.Core.Common.CommandTrees.DbPropertyExpression}, string)" />
        /// Logicaly, <see cref="DbSkipExpression" /> translates to:
        /// SELECT Y.x1, Y.x2, ..., Y.xn
        /// FROM (
        /// SELECT X.x1, X.x2, ..., X.xn,
        /// FROM input AS X
        /// EXCEPT
        /// SELECT TOP(count) Z.x1, Z.x2, ..., Z.xn
        /// FROM input AS Z
        /// ORDER BY sk1, sk2, ...
        /// ) AS Y
        /// ORDER BY sk1, sk2, ...
        /// Here, input refers to the input of the <see cref="DbSkipExpression" />, and count to the count property of the
        /// <see
        ///     cref="DbSkipExpression" />
        /// .
        /// The implementation of EXCEPT is non-duplicate eliminating, and does equality comparison only over the
        /// equality comparable columns of the input.
        /// This corresponds to the following expression tree:
        /// SORT
        /// |
        /// NON-DISTINCT EXCEPT  (specially translated,
        /// |
        /// | - Left:  clone of input
        /// | - Right:
        /// |
        /// Limit
        /// |
        /// | - Limit: Count
        /// | - Input
        /// |
        /// Sort
        /// |
        /// input
        /// </summary>
        public override DbExpression Visit(DbSkipExpression e)
        {
            Check.NotNull(e, "e");

            //Build the right input of the except
            DbExpression rightInput = VisitExpressionBinding(e.Input).Sort(VisitSortOrder(e.SortOrder)).Limit(VisitExpression(e.Count));

            //Build the left input for the except
            var leftInput = VisitExpression(e.Input.Expression); //Another copy of the input

            var sortOrder = VisitSortOrder(e.SortOrder); //Another copy of the sort order

            // Create a list of the sort expressions to be used for translating except
            IList<DbPropertyExpression> sortExpressions = new List<DbPropertyExpression>(e.SortOrder.Count);
            foreach (var sortClause in sortOrder)
            {
                //We only care about property expressions, not about constants
                if (sortClause.Expression.ExpressionKind
                    == DbExpressionKind.Property)
                {
                    sortExpressions.Add((DbPropertyExpression)sortClause.Expression);
                }
            }

            var exceptExpression = TransformIntersectOrExcept(
                leftInput, rightInput, DbExpressionKind.Skip, sortExpressions, e.Input.VariableName);

            DbExpression result = exceptExpression.BindAs(e.Input.VariableName).Sort(sortOrder);

            return result;
        }

        #endregion

        #region DbExpressionVisitor<DbExpression> Member Helpers

        /// <summary>
        /// This method is invoked when tranforming <see cref="DbIntersectExpression" /> and <see cref="DbExceptExpression" /> by doing comparison over all input columns.
        /// <see
        ///     cref="TransformIntersectOrExcept(DbExpression, DbExpression, DbExpressionKind, System.Collections.Generic.IList{System.Data.Entity.Core.Common.CommandTrees.DbPropertyExpression}, string)" />
        /// </summary>
        private DbExpression TransformIntersectOrExcept(DbExpression left, DbExpression right, DbExpressionKind expressionKind)
        {
            return TransformIntersectOrExcept(left, right, expressionKind, null, null);
        }

        /// <summary>
        /// This method is used for translating <see cref="DbIntersectExpression" /> and <see cref="DbExceptExpression" />,
        /// and for translating the "Except" part of <see cref="DbSkipExpression" />.
        /// into the follwoing expression:
        /// A INTERSECT B, A EXCEPT B
        /// (DISTINCT)
        /// |
        /// FILTER
        /// |
        /// | - Input: A
        /// | - Predicate:(NOT)
        /// |
        /// ANY
        /// |
        /// | - Input: B
        /// | - Predicate:  (B.b1 = A.a1 or (B.b1 is null and A.a1 is null))
        /// AND (B.b2 = A.a2 or (B.b2 is null and A.a2 is null))
        /// AND ...
        /// AND (B.bn = A.an or (B.bn is null and A.an is null)))
        /// Here, A corresponds to right and B to left.
        /// (NOT) is present when transforming Except
        /// for the purpose of translating <see cref="DbExceptExpression" /> or <see cref="DbSkipExpression" />.
        /// (DISTINCT) is present when transforming for the purpose of translating
        /// <see cref="DbExceptExpression" /> or <see cref="DbIntersectExpression" />.
        /// For <see cref="DbSkipExpression" />, the input to ANY is caped with project which projects out only
        /// the columns represented in the sortExpressionsOverLeft list and only these are used in the predicate.
        /// This is because we want to support skip over input with non-equal comarable columns and we have no way to recognize these.
        /// </summary>
        /// <param name="left"> </param>
        /// <param name="right"> </param>
        /// <param name="expressionKind"> </param>
        /// <param name="sortExpressionsOverLeft"> note that this list gets destroyed by this method </param>
        /// <param name="sortExpressionsBindingVariableName"> </param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1614:ElementParameterDocumentationMustHaveText")]
        private DbExpression TransformIntersectOrExcept(
            DbExpression left, DbExpression right, DbExpressionKind expressionKind, IList<DbPropertyExpression> sortExpressionsOverLeft,
            string sortExpressionsBindingVariableName)
        {
            var negate = (expressionKind == DbExpressionKind.Except) || (expressionKind == DbExpressionKind.Skip);
            var distinct = (expressionKind == DbExpressionKind.Except) || (expressionKind == DbExpressionKind.Intersect);

            var leftInputBinding = left.Bind();
            var rightInputBinding = right.Bind();

            IList<DbPropertyExpression> leftFlattenedProperties = new List<DbPropertyExpression>();
            IList<DbPropertyExpression> rightFlattenedProperties = new List<DbPropertyExpression>();

            FlattenProperties(leftInputBinding.Variable, leftFlattenedProperties);
            FlattenProperties(rightInputBinding.Variable, rightFlattenedProperties);

            //For Skip, we need to ignore any columns that are not in the original sort list. We can recognize these by comparing the left flattened properties and
            // the properties in the list sortExpressionsOverLeft
            // If any such columns exist, we need to add an additional project, to keep the rest of the columns from being projected, as if any among these
            // are non equal comparable, SQL Server 2000 throws.
            if (expressionKind == DbExpressionKind.Skip)
            {
                if (RemoveNonSortProperties(
                    leftFlattenedProperties, rightFlattenedProperties, sortExpressionsOverLeft, leftInputBinding.VariableName,
                    sortExpressionsBindingVariableName))
                {
                    rightInputBinding = CapWithProject(rightInputBinding, rightFlattenedProperties);
                }
            }

            Debug.Assert(
                leftFlattenedProperties.Count == rightFlattenedProperties.Count,
                "The left and the right input to INTERSECT or EXCEPT have a different number of properties");
            Debug.Assert(leftFlattenedProperties.Count != 0, "The inputs to INTERSECT or EXCEPT have no properties");

            //Build the predicate for the quantifier:
            //   (B.b1 = A.a1 or (B.b1 is null and A.a1 is null))
            //      AND (B.b2 = A.a2 or (B.b2 is null and A.a2 is null))
            //      AND ... 
            //      AND (B.bn = A.an or (B.bn is null and A.an is null)))
            DbExpression existsPredicate = null;

            for (var i = 0; i < leftFlattenedProperties.Count; i++)
            {
                //A.ai == B.bi
                DbExpression equalsExpression = leftFlattenedProperties[i].Equal(rightFlattenedProperties[i]);

                //A.ai is null AND B.bi is null
                DbExpression leftIsNullExpression = leftFlattenedProperties[i].IsNull();
                DbExpression rightIsNullExpression = rightFlattenedProperties[i].IsNull();
                DbExpression bothNullExpression = leftIsNullExpression.And(rightIsNullExpression);

                DbExpression orExpression = equalsExpression.Or(bothNullExpression);

                if (i == 0)
                {
                    existsPredicate = orExpression;
                }
                else
                {
                    existsPredicate = existsPredicate.And(orExpression);
                }
            }

            //Build the quantifier
            DbExpression quantifierExpression = rightInputBinding.Any(existsPredicate);

            DbExpression filterPredicate;

            //Negate if needed
            if (negate)
            {
                filterPredicate = quantifierExpression.Not();
            }
            else
            {
                filterPredicate = quantifierExpression;
            }

            //Build the filter
            DbExpression result = leftInputBinding.Filter(filterPredicate);

            //Apply distinct in needed
            if (distinct)
            {
                result = result.Distinct();
            }

            return result;
        }

        /// <summary>
        /// Adds the flattened properties on the input to the flattenedProperties list.
        /// </summary>
        private void FlattenProperties(DbExpression input, IList<DbPropertyExpression> flattenedProperties)
        {
            var properties = input.ResultType.GetProperties();
            Debug.Assert(properties.Any(), "No nested properties when FlattenProperties called?");

            foreach (var property in properties)
            {
                var propertyInput = input;

                var propertyExpression = propertyInput.Property(property);
                if (BuiltInTypeKind.PrimitiveType
                    == property.TypeUsage.EdmType.BuiltInTypeKind)
                {
                    flattenedProperties.Add(propertyExpression);
                }
                else
                {
                    Debug.Assert(
                        BuiltInTypeKind.EntityType == property.TypeUsage.EdmType.BuiltInTypeKind
                        || BuiltInTypeKind.RowType == property.TypeUsage.EdmType.BuiltInTypeKind,
                        "The input to FlattenProperties is not of EntityType or RowType?");

                    FlattenProperties(propertyExpression, flattenedProperties);
                }
            }
        }

        /// <summary>
        /// Helper method for
        /// <see
        ///     cref="TransformIntersectOrExcept(DbExpression, DbExpression, DbExpressionKind, System.Collections.Generic.IList{System.Data.Entity.Core.Common.CommandTrees.DbPropertyExpression}, string)" />
        /// Removes all pairs of property expressions from list1 and list2, for which the property expression in list1
        /// does not have a 'matching' property expression in list2.
        /// The lists list1 and list2 are known to not create duplicate, and the purpose of the sortList is just for this method.
        /// Thus, to optimize the match process, we remove the seen property expressions from the sort list in
        /// <see cref="HasMatchInList" />
        /// when iterating both list simultaneously.
        /// </summary>
        private static bool RemoveNonSortProperties(
            IList<DbPropertyExpression> list1, IList<DbPropertyExpression> list2, IList<DbPropertyExpression> sortList,
            string list1BindingVariableName, string sortExpressionsBindingVariableName)
        {
            var result = false;
            for (var i = list1.Count - 1; i >= 0; i--)
            {
                if (!HasMatchInList(list1[i], sortList, list1BindingVariableName, sortExpressionsBindingVariableName))
                {
                    list1.RemoveAt(i);
                    list2.RemoveAt(i);
                    result = true;
                }
            }
            return result;
        }

        /// <summary>
        /// Helper method for <see cref="RemoveNonSortProperties" />
        /// Checks whether expr has a 'match' in the given list of property expressions.
        /// If it does, the matching expression is removed form the list, to speed up future matching.
        /// </summary>
        private static bool HasMatchInList(
            DbPropertyExpression expr, IList<DbPropertyExpression> list, string exprBindingVariableName,
            string listExpressionsBindingVariableName)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (AreMatching(expr, list[i], exprBindingVariableName, listExpressionsBindingVariableName))
                {
                    // This method is used for matching element of two list without duplicates,
                    // thus if match is found, remove it from the list, to speed up future matching.
                    list.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines whether two expressions match.
        /// They match if they are  of the shape
        /// expr1 -> DbPropertyExpression(... (DbPropertyExpression(DbVariableReferenceExpression(expr1BindingVariableName), nameX), ..., name1)
        /// expr1 -> DbPropertyExpression(... (DbPropertyExpression(DbVariableReferenceExpression(expr2BindingVariableName), nameX), ..., name1),
        /// i.e. if they only differ in the name of the binding.
        /// </summary>
        private static bool AreMatching(
            DbPropertyExpression expr1, DbPropertyExpression expr2, string expr1BindingVariableName, string expr2BindingVariableName)
        {
            if (expr1.Property.Name
                != expr2.Property.Name)
            {
                return false;
            }

            if (expr1.Instance.ExpressionKind
                != expr2.Instance.ExpressionKind)
            {
                return false;
            }

            if (expr1.Instance.ExpressionKind
                == DbExpressionKind.Property)
            {
                return AreMatching(
                    (DbPropertyExpression)expr1.Instance, (DbPropertyExpression)expr2.Instance, expr1BindingVariableName,
                    expr2BindingVariableName);
            }

            var instance1 = (DbVariableReferenceExpression)expr1.Instance;
            var instance2 = (DbVariableReferenceExpression)expr2.Instance;

            return (String.Equals(instance1.VariableName, expr1BindingVariableName, StringComparison.Ordinal)
                    && String.Equals(instance2.VariableName, expr2BindingVariableName, StringComparison.Ordinal));
        }

        /// <summary>
        /// Helper method for
        /// <see
        ///     cref="TransformIntersectOrExcept(DbExpression, DbExpression, DbExpressionKind, System.Collections.Generic.IList{System.Data.Entity.Core.Common.CommandTrees.DbPropertyExpression}, string)" />
        /// Creates a
        /// <see cref="DbProjectExpression" />
        /// over the given inputBinding that projects out the given flattenedProperties.
        /// and updates the flattenedProperties to be over the newly created project.
        /// </summary>
        /// <returns>
        /// An <see cref="DbExpressionBinding" /> over the newly created <see cref="DbProjectExpression" />
        /// </returns>
        private static DbExpressionBinding CapWithProject(DbExpressionBinding inputBinding, IList<DbPropertyExpression> flattenedProperties)
        {
            var projectColumns = new List<KeyValuePair<string, DbExpression>>(flattenedProperties.Count);

            //List of all the columnNames used in the projection.
            var columnNames = new Dictionary<string, int>(flattenedProperties.Count);

            foreach (var pe in flattenedProperties)
            {
                //There may be conflicting property names, thus we may need to rename.
                var name = pe.Property.Name;
                int i;
                if (columnNames.TryGetValue(name, out i))
                {
                    string newName;
                    do
                    {
                        ++i;
                        newName = name + i.ToString(CultureInfo.InvariantCulture);
                    }
                    while (columnNames.ContainsKey(newName));

                    columnNames[name] = i;
                    name = newName;
                }

                // Add this column name to list of known names so that there are no subsequent
                // collisions
                columnNames[name] = 0;
                projectColumns.Add(new KeyValuePair<string, DbExpression>(name, pe));
            }

            //Build the project
            DbExpression rowExpr = DbExpressionBuilder.NewRow(projectColumns);
            var projectExpression = inputBinding.Project(rowExpr);

            //Create the new inputBinding
            var resultBinding = projectExpression.Bind();

            //Create the list of flattenedProperties over the new project
            flattenedProperties.Clear();
            var rowExprType = (RowType)rowExpr.ResultType.EdmType;

            foreach (var column in projectColumns)
            {
                var prop = rowExprType.Properties[column.Key];
                flattenedProperties.Add(resultBinding.Variable.Property(prop));
            }
            return resultBinding;
        }

        #endregion
    }
}
