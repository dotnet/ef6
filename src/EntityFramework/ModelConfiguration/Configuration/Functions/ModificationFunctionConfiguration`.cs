// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.ComponentModel;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;

    public class ModificationFunctionConfiguration<TEntityType>
        where TEntityType : class
    {
        private readonly ModificationFunctionConfiguration _configuration
            = new ModificationFunctionConfiguration();

        internal ModificationFunctionConfiguration()
        {
        }

        internal ModificationFunctionConfiguration Configuration
        {
            get { return _configuration; }
        }

        public ModificationFunctionConfiguration<TEntityType> HasName(string functionName)
        {
            Check.NotEmpty(functionName, "functionName");

            _configuration.HasName(functionName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public FunctionParameterConfiguration Parameter<TProperty>(Expression<Func<TEntityType, TProperty>> propertyExpression)
            where TProperty : struct
        {
            Check.NotNull(propertyExpression, "propertyExpression");

            return _configuration.Parameter(propertyExpression.GetComplexPropertyAccess());
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public FunctionParameterConfiguration Parameter<TProperty>(Expression<Func<TEntityType, TProperty?>> propertyExpression)
            where TProperty : struct
        {
            Check.NotNull(propertyExpression, "propertyExpression");

            return _configuration.Parameter(propertyExpression.GetComplexPropertyAccess());
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public FunctionParameterConfiguration Parameter(Expression<Func<TEntityType, string>> propertyExpression)
        {
            Check.NotNull(propertyExpression, "propertyExpression");

            return _configuration.Parameter(propertyExpression.GetComplexPropertyAccess());
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public FunctionParameterConfiguration Parameter(Expression<Func<TEntityType, byte[]>> propertyExpression)
        {
            Check.NotNull(propertyExpression, "propertyExpression");

            return _configuration.Parameter(propertyExpression.GetComplexPropertyAccess());
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public FunctionParameterConfiguration Parameter(Expression<Func<TEntityType, DbGeography>> propertyExpression)
        {
            Check.NotNull(propertyExpression, "propertyExpression");

            return _configuration.Parameter(propertyExpression.GetComplexPropertyAccess());
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public FunctionParameterConfiguration Parameter(Expression<Func<TEntityType, DbGeometry>> propertyExpression)
        {
            Check.NotNull(propertyExpression, "propertyExpression");

            return _configuration.Parameter(propertyExpression.GetComplexPropertyAccess());
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
