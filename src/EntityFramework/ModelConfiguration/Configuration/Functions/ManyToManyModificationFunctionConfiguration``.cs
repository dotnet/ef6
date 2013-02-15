// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.ComponentModel;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;

    public class ManyToManyModificationFunctionConfiguration<TEntityType, TTargetEntityType>
        where TEntityType : class
        where TTargetEntityType : class
    {
        private readonly ModificationFunctionConfiguration _configuration
            = new ModificationFunctionConfiguration();

        internal ManyToManyModificationFunctionConfiguration()
        {
        }

        internal ModificationFunctionConfiguration Configuration
        {
            get { return _configuration; }
        }

        public ManyToManyModificationFunctionConfiguration<TEntityType, TTargetEntityType> HasName(string procedureName)
        {
            Check.NotEmpty(procedureName, "procedureName");

            _configuration.HasName(procedureName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ManyToManyModificationFunctionConfiguration<TEntityType, TTargetEntityType> LeftKeyParameter<TProperty>(
            Expression<Func<TEntityType, TProperty>> propertyExpression, string parameterName)
            where TProperty : struct
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            _configuration.Parameter(propertyExpression.GetSimplePropertyAccess(), parameterName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ManyToManyModificationFunctionConfiguration<TEntityType, TTargetEntityType> LeftKeyParameter<TProperty>(
            Expression<Func<TEntityType, TProperty?>> propertyExpression, string parameterName)
            where TProperty : struct
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            _configuration.Parameter(propertyExpression.GetSimplePropertyAccess(), parameterName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ManyToManyModificationFunctionConfiguration<TEntityType, TTargetEntityType> LeftKeyParameter(
            Expression<Func<TEntityType, string>> propertyExpression, string parameterName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            _configuration.Parameter(propertyExpression.GetSimplePropertyAccess(), parameterName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ManyToManyModificationFunctionConfiguration<TEntityType, TTargetEntityType> LeftKeyParameter(
            Expression<Func<TEntityType, byte[]>> propertyExpression, string parameterName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            _configuration.Parameter(propertyExpression.GetSimplePropertyAccess(), parameterName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ManyToManyModificationFunctionConfiguration<TEntityType, TTargetEntityType> LeftKeyParameter(
            Expression<Func<TEntityType, DbGeography>> propertyExpression, string parameterName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            _configuration.Parameter(propertyExpression.GetSimplePropertyAccess(), parameterName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ManyToManyModificationFunctionConfiguration<TEntityType, TTargetEntityType> LeftKeyParameter(
            Expression<Func<TEntityType, DbGeometry>> propertyExpression, string parameterName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            _configuration.Parameter(propertyExpression.GetSimplePropertyAccess(), parameterName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ManyToManyModificationFunctionConfiguration<TEntityType, TTargetEntityType> RightKeyParameter<TProperty>(
            Expression<Func<TTargetEntityType, TProperty>> propertyExpression, string parameterName)
            where TProperty : struct
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            _configuration.Parameter(propertyExpression.GetSimplePropertyAccess(), parameterName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ManyToManyModificationFunctionConfiguration<TEntityType, TTargetEntityType> RightKeyParameter<TProperty>(
            Expression<Func<TTargetEntityType, TProperty?>> propertyExpression, string parameterName)
            where TProperty : struct
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            _configuration.Parameter(propertyExpression.GetSimplePropertyAccess(), parameterName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ManyToManyModificationFunctionConfiguration<TEntityType, TTargetEntityType> RightKeyParameter(
            Expression<Func<TTargetEntityType, string>> propertyExpression, string parameterName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            _configuration.Parameter(propertyExpression.GetSimplePropertyAccess(), parameterName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ManyToManyModificationFunctionConfiguration<TEntityType, TTargetEntityType> RightKeyParameter(
            Expression<Func<TTargetEntityType, byte[]>> propertyExpression, string parameterName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            _configuration.Parameter(propertyExpression.GetSimplePropertyAccess(), parameterName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ManyToManyModificationFunctionConfiguration<TEntityType, TTargetEntityType> RightKeyParameter(
            Expression<Func<TTargetEntityType, DbGeography>> propertyExpression, string parameterName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            _configuration.Parameter(propertyExpression.GetSimplePropertyAccess(), parameterName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ManyToManyModificationFunctionConfiguration<TEntityType, TTargetEntityType> RightKeyParameter(
            Expression<Func<TTargetEntityType, DbGeometry>> propertyExpression, string parameterName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            _configuration.Parameter(propertyExpression.GetSimplePropertyAccess(), parameterName);

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
