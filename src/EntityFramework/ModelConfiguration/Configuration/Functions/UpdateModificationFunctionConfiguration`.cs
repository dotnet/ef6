// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.ComponentModel;
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

        public UpdateModificationFunctionConfiguration<TEntityType> HasName(string procedureName)
        {
            Check.NotEmpty(procedureName, "procedureName");

            Configuration.HasName(procedureName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public UpdateModificationFunctionConfiguration<TEntityType> Parameter<TProperty>(
            Expression<Func<TEntityType, TProperty>> propertyExpression, string parameterName)
            where TProperty : struct
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            Configuration.Parameter(propertyExpression.GetComplexPropertyAccess(), parameterName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public UpdateModificationFunctionConfiguration<TEntityType> Parameter<TProperty>(
            Expression<Func<TEntityType, TProperty?>> propertyExpression, string parameterName)
            where TProperty : struct
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            Configuration.Parameter(propertyExpression.GetComplexPropertyAccess(), parameterName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public UpdateModificationFunctionConfiguration<TEntityType> Parameter(
            Expression<Func<TEntityType, string>> propertyExpression, string parameterName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            Configuration.Parameter(propertyExpression.GetComplexPropertyAccess(), parameterName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public UpdateModificationFunctionConfiguration<TEntityType> Parameter(
            Expression<Func<TEntityType, byte[]>> propertyExpression, string parameterName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            Configuration.Parameter(propertyExpression.GetComplexPropertyAccess(), parameterName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public UpdateModificationFunctionConfiguration<TEntityType> Parameter(
            Expression<Func<TEntityType, DbGeography>> propertyExpression, string parameterName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            Configuration.Parameter(propertyExpression.GetComplexPropertyAccess(), parameterName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public UpdateModificationFunctionConfiguration<TEntityType> Parameter(
            Expression<Func<TEntityType, DbGeometry>> propertyExpression, string parameterName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            Configuration.Parameter(propertyExpression.GetComplexPropertyAccess(), parameterName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public UpdateModificationFunctionConfiguration<TEntityType> Parameter<TProperty>(
            Expression<Func<TEntityType, TProperty>> propertyExpression, string currentValueParameterName, string originalValueParameterName)
            where TProperty : struct
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(currentValueParameterName, "currentValueParameterName");
            Check.NotEmpty(originalValueParameterName, "originalValueParameterName");

            Configuration.Parameter(
                propertyExpression.GetComplexPropertyAccess(), currentValueParameterName, originalValueParameterName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public UpdateModificationFunctionConfiguration<TEntityType> Parameter<TProperty>(
            Expression<Func<TEntityType, TProperty?>> propertyExpression, string currentValueParameterName,
            string originalValueParameterName)
            where TProperty : struct
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(currentValueParameterName, "currentValueParameterName");
            Check.NotEmpty(originalValueParameterName, "originalValueParameterName");

            Configuration.Parameter(
                propertyExpression.GetComplexPropertyAccess(), currentValueParameterName, originalValueParameterName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public UpdateModificationFunctionConfiguration<TEntityType> Parameter(
            Expression<Func<TEntityType, string>> propertyExpression, string currentValueParameterName, string originalValueParameterName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(currentValueParameterName, "currentValueParameterName");
            Check.NotEmpty(originalValueParameterName, "originalValueParameterName");

            Configuration.Parameter(
                propertyExpression.GetComplexPropertyAccess(), currentValueParameterName, originalValueParameterName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public UpdateModificationFunctionConfiguration<TEntityType> Parameter(
            Expression<Func<TEntityType, byte[]>> propertyExpression, string currentValueParameterName, string originalValueParameterName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(currentValueParameterName, "currentValueParameterName");
            Check.NotEmpty(originalValueParameterName, "originalValueParameterName");

            Configuration.Parameter(
                propertyExpression.GetComplexPropertyAccess(), currentValueParameterName, originalValueParameterName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public UpdateModificationFunctionConfiguration<TEntityType> Parameter(
            Expression<Func<TEntityType, DbGeography>> propertyExpression, string currentValueParameterName,
            string originalValueParameterName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(currentValueParameterName, "currentValueParameterName");
            Check.NotEmpty(originalValueParameterName, "originalValueParameterName");

            Configuration.Parameter(
                propertyExpression.GetComplexPropertyAccess(), currentValueParameterName, originalValueParameterName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public UpdateModificationFunctionConfiguration<TEntityType> Parameter(
            Expression<Func<TEntityType, DbGeometry>> propertyExpression, string currentValueParameterName,
            string originalValueParameterName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(currentValueParameterName, "currentValueParameterName");
            Check.NotEmpty(originalValueParameterName, "originalValueParameterName");

            Configuration.Parameter(
                propertyExpression.GetComplexPropertyAccess(), currentValueParameterName, originalValueParameterName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public UpdateModificationFunctionConfiguration<TEntityType> Result<TProperty>(
            Expression<Func<TEntityType, TProperty>> propertyExpression, string columnName)
            where TProperty : struct
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(columnName, "columnName");

            Configuration.Result(propertyExpression.GetSimplePropertyAccess(), columnName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public UpdateModificationFunctionConfiguration<TEntityType> Result<TProperty>(
            Expression<Func<TEntityType, TProperty?>> propertyExpression, string columnName)
            where TProperty : struct
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(columnName, "columnName");

            Configuration.Result(propertyExpression.GetSimplePropertyAccess(), columnName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public UpdateModificationFunctionConfiguration<TEntityType> Result(
            Expression<Func<TEntityType, string>> propertyExpression, string columnName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(columnName, "columnName");

            Configuration.Result(propertyExpression.GetSimplePropertyAccess(), columnName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public UpdateModificationFunctionConfiguration<TEntityType> Result(
            Expression<Func<TEntityType, byte[]>> propertyExpression, string columnName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(columnName, "columnName");

            Configuration.Result(propertyExpression.GetSimplePropertyAccess(), columnName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public UpdateModificationFunctionConfiguration<TEntityType> Result(
            Expression<Func<TEntityType, DbGeography>> propertyExpression, string columnName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(columnName, "columnName");

            Configuration.Result(propertyExpression.GetSimplePropertyAccess(), columnName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public UpdateModificationFunctionConfiguration<TEntityType> Result(
            Expression<Func<TEntityType, DbGeometry>> propertyExpression, string columnName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(columnName, "columnName");

            Configuration.Result(propertyExpression.GetSimplePropertyAccess(), columnName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public UpdateModificationFunctionConfiguration<TEntityType> RowsAffectedParameter(string parameterName)
        {
            Check.NotEmpty(parameterName, "parameterName");

            Configuration.RowsAffectedParameter(parameterName);

            return this;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }
    }
}
