// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.ComponentModel;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;

    public class DeleteModificationFunctionConfiguration<TEntityType> : ModificationFunctionConfiguration<TEntityType>
        where TEntityType : class
    {
        internal DeleteModificationFunctionConfiguration()
        {
        }

        public DeleteModificationFunctionConfiguration<TEntityType> HasName(string procedureName)
        {
            Check.NotEmpty(procedureName, "procedureName");

            Configuration.HasName(procedureName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public DeleteModificationFunctionConfiguration<TEntityType> Parameter<TProperty>(
            Expression<Func<TEntityType, TProperty>> propertyExpression, string parameterName)
            where TProperty : struct
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            Configuration.Parameter(propertyExpression.GetComplexPropertyAccess(), parameterName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public DeleteModificationFunctionConfiguration<TEntityType> Parameter<TProperty>(
            Expression<Func<TEntityType, TProperty?>> propertyExpression, string parameterName)
            where TProperty : struct
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            Configuration.Parameter(propertyExpression.GetComplexPropertyAccess(), parameterName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public DeleteModificationFunctionConfiguration<TEntityType> Parameter(
            Expression<Func<TEntityType, string>> propertyExpression, string parameterName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            Configuration.Parameter(propertyExpression.GetComplexPropertyAccess(), parameterName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public DeleteModificationFunctionConfiguration<TEntityType> Parameter(
            Expression<Func<TEntityType, byte[]>> propertyExpression, string parameterName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            Configuration.Parameter(propertyExpression.GetComplexPropertyAccess(), parameterName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public DeleteModificationFunctionConfiguration<TEntityType> Parameter(
            Expression<Func<TEntityType, DbGeography>> propertyExpression, string parameterName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            Configuration.Parameter(propertyExpression.GetComplexPropertyAccess(), parameterName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public DeleteModificationFunctionConfiguration<TEntityType> Parameter(
            Expression<Func<TEntityType, DbGeometry>> propertyExpression, string parameterName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            Configuration.Parameter(propertyExpression.GetComplexPropertyAccess(), parameterName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public DeleteModificationFunctionConfiguration<TEntityType> RowsAffectedParameter(string parameterName)
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
