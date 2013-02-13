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

        public ManyToManyModificationFunctionConfiguration<TEntityType, TTargetEntityType> HasName(string functionName)
        {
            Check.NotEmpty(functionName, "functionName");

            _configuration.HasName(functionName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public FunctionParameterConfiguration LeftKeyParameter<TProperty>(Expression<Func<TEntityType, TProperty>> propertyExpression)
            where TProperty : struct
        {
            Check.NotNull(propertyExpression, "propertyExpression");

            return _configuration.Parameter(propertyExpression.GetSimplePropertyAccess());
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public FunctionParameterConfiguration LeftKeyParameter<TProperty>(Expression<Func<TEntityType, TProperty?>> propertyExpression)
            where TProperty : struct
        {
            Check.NotNull(propertyExpression, "propertyExpression");

            return _configuration.Parameter(propertyExpression.GetSimplePropertyAccess());
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public FunctionParameterConfiguration LeftKeyParameter(Expression<Func<TEntityType, string>> propertyExpression)
        {
            Check.NotNull(propertyExpression, "propertyExpression");

            return _configuration.Parameter(propertyExpression.GetSimplePropertyAccess());
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public FunctionParameterConfiguration LeftKeyParameter(Expression<Func<TEntityType, byte[]>> propertyExpression)
        {
            Check.NotNull(propertyExpression, "propertyExpression");

            return _configuration.Parameter(propertyExpression.GetSimplePropertyAccess());
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public FunctionParameterConfiguration LeftKeyParameter(Expression<Func<TEntityType, DbGeography>> propertyExpression)
        {
            Check.NotNull(propertyExpression, "propertyExpression");

            return _configuration.Parameter(propertyExpression.GetSimplePropertyAccess());
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public FunctionParameterConfiguration LeftKeyParameter(Expression<Func<TEntityType, DbGeometry>> propertyExpression)
        {
            Check.NotNull(propertyExpression, "propertyExpression");

            return _configuration.Parameter(propertyExpression.GetSimplePropertyAccess());
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public FunctionParameterConfiguration RightKeyParameter<TProperty>(
            Expression<Func<TTargetEntityType, TProperty>> propertyExpression)
            where TProperty : struct
        {
            Check.NotNull(propertyExpression, "propertyExpression");

            return _configuration.Parameter(propertyExpression.GetSimplePropertyAccess());
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public FunctionParameterConfiguration RightKeyParameter<TProperty>(
            Expression<Func<TTargetEntityType, TProperty?>> propertyExpression)
            where TProperty : struct
        {
            Check.NotNull(propertyExpression, "propertyExpression");

            return _configuration.Parameter(propertyExpression.GetSimplePropertyAccess());
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public FunctionParameterConfiguration RightKeyParameter(Expression<Func<TTargetEntityType, string>> propertyExpression)
        {
            Check.NotNull(propertyExpression, "propertyExpression");

            return _configuration.Parameter(propertyExpression.GetSimplePropertyAccess());
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public FunctionParameterConfiguration RightKeyParameter(Expression<Func<TTargetEntityType, byte[]>> propertyExpression)
        {
            Check.NotNull(propertyExpression, "propertyExpression");

            return _configuration.Parameter(propertyExpression.GetSimplePropertyAccess());
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public FunctionParameterConfiguration RightKeyParameter(Expression<Func<TTargetEntityType, DbGeography>> propertyExpression)
        {
            Check.NotNull(propertyExpression, "propertyExpression");

            return _configuration.Parameter(propertyExpression.GetSimplePropertyAccess());
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public FunctionParameterConfiguration RightKeyParameter(Expression<Func<TTargetEntityType, DbGeometry>> propertyExpression)
        {
            Check.NotNull(propertyExpression, "propertyExpression");

            return _configuration.Parameter(propertyExpression.GetSimplePropertyAccess());
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
