// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Data.Entity.Spatial;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;

    public class InsertModificationFunctionConfiguration<TEntityType> : ModificationFunctionConfiguration<TEntityType>
        where TEntityType : class
    {
        internal InsertModificationFunctionConfiguration()
        {
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public InsertModificationFunctionConfiguration<TEntityType> BindResult<TProperty>(
            Expression<Func<TEntityType, TProperty>> propertyExpression, string columnName)
            where TProperty : struct
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(columnName, "columnName");

            Configuration.BindResult(propertyExpression.GetSimplePropertyAccess(), columnName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public InsertModificationFunctionConfiguration<TEntityType> BindResult<TProperty>(
            Expression<Func<TEntityType, TProperty?>> propertyExpression, string columnName)
            where TProperty : struct
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(columnName, "columnName");

            Configuration.BindResult(propertyExpression.GetSimplePropertyAccess(), columnName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public InsertModificationFunctionConfiguration<TEntityType> BindResult(
            Expression<Func<TEntityType, string>> propertyExpression, string columnName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(columnName, "columnName");

            Configuration.BindResult(propertyExpression.GetSimplePropertyAccess(), columnName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public InsertModificationFunctionConfiguration<TEntityType> BindResult(
            Expression<Func<TEntityType, byte[]>> propertyExpression, string columnName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(columnName, "columnName");

            Configuration.BindResult(propertyExpression.GetSimplePropertyAccess(), columnName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public InsertModificationFunctionConfiguration<TEntityType> BindResult(
            Expression<Func<TEntityType, DbGeography>> propertyExpression, string columnName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(columnName, "columnName");

            Configuration.BindResult(propertyExpression.GetSimplePropertyAccess(), columnName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public InsertModificationFunctionConfiguration<TEntityType> BindResult(
            Expression<Func<TEntityType, DbGeometry>> propertyExpression, string columnName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(columnName, "columnName");

            Configuration.BindResult(propertyExpression.GetSimplePropertyAccess(), columnName);

            return this;
        }
    }
}
