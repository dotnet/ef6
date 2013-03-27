// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public class AssociationModificationFunctionConfiguration<TEntityType>
        where TEntityType : class
    {
        private readonly PropertyInfo _navigationPropertyInfo;
        private readonly ModificationFunctionConfiguration _configuration;

        internal AssociationModificationFunctionConfiguration(
            PropertyInfo navigationPropertyInfo, ModificationFunctionConfiguration configuration)
        {
            DebugCheck.NotNull(navigationPropertyInfo);
            DebugCheck.NotNull(configuration);

            _navigationPropertyInfo = navigationPropertyInfo;
            _configuration = configuration;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public AssociationModificationFunctionConfiguration<TEntityType> Parameter<TProperty>(
            Expression<Func<TEntityType, TProperty>> propertyExpression, string parameterName)
            where TProperty : struct
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            _configuration.Parameter(
                new PropertyPath(new[] { _navigationPropertyInfo }.Concat(propertyExpression.GetSimplePropertyAccess())),
                parameterName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public AssociationModificationFunctionConfiguration<TEntityType> Parameter<TProperty>(
            Expression<Func<TEntityType, TProperty?>> propertyExpression, string parameterName)
            where TProperty : struct
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            _configuration.Parameter(
                new PropertyPath(new[] { _navigationPropertyInfo }.Concat(propertyExpression.GetSimplePropertyAccess())),
                parameterName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public AssociationModificationFunctionConfiguration<TEntityType> Parameter(
            Expression<Func<TEntityType, string>> propertyExpression, string parameterName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            _configuration.Parameter(
                new PropertyPath(new[] { _navigationPropertyInfo }.Concat(propertyExpression.GetSimplePropertyAccess())),
                parameterName);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public AssociationModificationFunctionConfiguration<TEntityType> Parameter(
            Expression<Func<TEntityType, byte[]>> propertyExpression, string parameterName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            _configuration.Parameter(
                new PropertyPath(new[] { _navigationPropertyInfo }.Concat(propertyExpression.GetSimplePropertyAccess())),
                parameterName);

            return this;
        }
    }
}
