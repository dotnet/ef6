// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Allows configuration to be performed for a stored procedure that is used to modify a relationship.
    /// </summary>
    /// <typeparam name="TEntityType">The type of the entity that the relationship is being configured from.</typeparam>
    public class AssociationModificationStoredProcedureConfiguration<TEntityType>
        where TEntityType : class
    {
        private readonly PropertyInfo _navigationPropertyInfo;
        private readonly ModificationStoredProcedureConfiguration _configuration;

        internal AssociationModificationStoredProcedureConfiguration(
            PropertyInfo navigationPropertyInfo, ModificationStoredProcedureConfiguration configuration)
        {
            DebugCheck.NotNull(navigationPropertyInfo);
            DebugCheck.NotNull(configuration);

            _navigationPropertyInfo = navigationPropertyInfo;
            _configuration = configuration;
        }

        /// <summary>Configures a parameter for this stored procedure.</summary>
        /// <returns> The same configuration instance so that multiple calls can be chained. </returns>
        /// <param name="propertyExpression"> A lambda expression representing the property to configure the parameter for. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public AssociationModificationStoredProcedureConfiguration<TEntityType> Parameter<TProperty>(
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

        /// <summary>Configures a parameter for this stored procedure.</summary>
        /// <returns> The same configuration instance so that multiple calls can be chained. </returns>
        /// <param name="propertyExpression"> A lambda expression representing the property to configure the parameter for. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public AssociationModificationStoredProcedureConfiguration<TEntityType> Parameter<TProperty>(
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

        /// <summary>Configures a parameter for this stored procedure.</summary>
        /// <returns> The same configuration instance so that multiple calls can be chained. </returns>
        /// <param name="propertyExpression"> A lambda expression representing the property to configure the parameter for. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <param name="parameterName">The name of the parameter.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public AssociationModificationStoredProcedureConfiguration<TEntityType> Parameter(
            Expression<Func<TEntityType, string>> propertyExpression, string parameterName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            _configuration.Parameter(
                new PropertyPath(new[] { _navigationPropertyInfo }.Concat(propertyExpression.GetSimplePropertyAccess())),
                parameterName);

            return this;
        }

        /// <summary>Configures a parameter for this stored procedure.</summary>
        /// <returns> The same configuration instance so that multiple calls can be chained. </returns>
        /// <param name="propertyExpression"> A lambda expression representing the property to configure the parameter for. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <param name="parameterName">The name of the parameter.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public AssociationModificationStoredProcedureConfiguration<TEntityType> Parameter(
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
