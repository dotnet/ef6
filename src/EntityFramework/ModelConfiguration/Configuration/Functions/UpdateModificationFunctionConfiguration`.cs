// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Data.Entity.Spatial;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;

    public class UpdateModificationFunctionConfiguration<TEntityType> : ModificationFunctionConfiguration<TEntityType>
        where TEntityType : class
    {
        internal UpdateModificationFunctionConfiguration()
        {
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public FunctionParameterConfiguration Parameter<TProperty>(
            Expression<Func<TEntityType, TProperty>> propertyExpression, bool originalValue)
            where TProperty : struct
        {
            Check.NotNull(propertyExpression, "propertyExpression");

            return Configuration.Parameter(propertyExpression.GetComplexPropertyAccess(), originalValue);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public FunctionParameterConfiguration Parameter<TProperty>(
            Expression<Func<TEntityType, TProperty?>> propertyExpression, bool originalValue)
            where TProperty : struct
        {
            Check.NotNull(propertyExpression, "propertyExpression");

            return Configuration.Parameter(propertyExpression.GetComplexPropertyAccess(), originalValue);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public FunctionParameterConfiguration Parameter(
            Expression<Func<TEntityType, string>> propertyExpression, bool originalValue)
        {
            Check.NotNull(propertyExpression, "propertyExpression");

            return Configuration.Parameter(propertyExpression.GetComplexPropertyAccess(), originalValue);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public FunctionParameterConfiguration Parameter(
            Expression<Func<TEntityType, byte[]>> propertyExpression, bool originalValue)
        {
            Check.NotNull(propertyExpression, "propertyExpression");

            return Configuration.Parameter(propertyExpression.GetComplexPropertyAccess(), originalValue);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public FunctionParameterConfiguration Parameter(
            Expression<Func<TEntityType, DbGeography>> propertyExpression, bool originalValue)
        {
            Check.NotNull(propertyExpression, "propertyExpression");

            return Configuration.Parameter(propertyExpression.GetComplexPropertyAccess(), originalValue);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public FunctionParameterConfiguration Parameter(
            Expression<Func<TEntityType, DbGeometry>> propertyExpression, bool originalValue)
        {
            Check.NotNull(propertyExpression, "propertyExpression");

            return Configuration.Parameter(propertyExpression.GetComplexPropertyAccess(), originalValue);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public UpdateModificationFunctionConfiguration<TEntityType> BindResult<TProperty>(
            Expression<Func<TEntityType, TProperty>> propertyExpression, string columnName)
            where TProperty : struct
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(columnName, "columnName");

            Configuration.BindResult(propertyExpression.GetSimplePropertyAccess(), columnName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public UpdateModificationFunctionConfiguration<TEntityType> BindResult<TProperty>(
            Expression<Func<TEntityType, TProperty?>> propertyExpression, string columnName)
            where TProperty : struct
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(columnName, "columnName");

            Configuration.BindResult(propertyExpression.GetSimplePropertyAccess(), columnName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public UpdateModificationFunctionConfiguration<TEntityType> BindResult(
            Expression<Func<TEntityType, string>> propertyExpression, string columnName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(columnName, "columnName");

            Configuration.BindResult(propertyExpression.GetSimplePropertyAccess(), columnName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public UpdateModificationFunctionConfiguration<TEntityType> BindResult(
            Expression<Func<TEntityType, byte[]>> propertyExpression, string columnName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(columnName, "columnName");

            Configuration.BindResult(propertyExpression.GetSimplePropertyAccess(), columnName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public UpdateModificationFunctionConfiguration<TEntityType> BindResult(
            Expression<Func<TEntityType, DbGeography>> propertyExpression, string columnName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(columnName, "columnName");

            Configuration.BindResult(propertyExpression.GetSimplePropertyAccess(), columnName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public UpdateModificationFunctionConfiguration<TEntityType> BindResult(
            Expression<Func<TEntityType, DbGeometry>> propertyExpression, string columnName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(columnName, "columnName");

            Configuration.BindResult(propertyExpression.GetSimplePropertyAccess(), columnName);

            return this;
        }
    }
}
