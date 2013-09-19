// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.ComponentModel;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;

    /// <summary>
    /// Allows configuration to be performed for a stored procedure that is used to modify a many to many relationship.
    /// </summary>
    /// <typeparam name="TEntityType">The type of the entity that the relationship is being configured from.</typeparam>
    /// <typeparam name="TTargetEntityType">The type of the entity that the other end of the relationship targets.</typeparam>
    public class ManyToManyModificationStoredProcedureConfiguration<TEntityType, TTargetEntityType>
        : ModificationStoredProcedureConfigurationBase
        where TEntityType : class
        where TTargetEntityType : class
    {
        internal ManyToManyModificationStoredProcedureConfiguration()
        {
        }

        /// <summary>
        /// Sets the name of the stored procedure.
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <returns> The same configuration instance so that multiple calls can be chained. </returns>
        public ManyToManyModificationStoredProcedureConfiguration<TEntityType, TTargetEntityType> HasName(string procedureName)
        {
            Check.NotEmpty(procedureName, "procedureName");

            Configuration.HasName(procedureName);

            return this;
        }

        /// <summary>
        /// Sets the name of the stored procedure.
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="schemaName">Name of the schema.</param>
        /// <returns> The same configuration instance so that multiple calls can be chained. </returns>
        public ManyToManyModificationStoredProcedureConfiguration<TEntityType, TTargetEntityType> HasName(
            string procedureName, string schemaName)
        {
            Check.NotEmpty(procedureName, "procedureName");
            Check.NotEmpty(schemaName, "schemaName");

            Configuration.HasName(procedureName, schemaName);

            return this;
        }

        /// <summary>
        /// Configures the parameter for the left key value(s).
        /// </summary>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns> The same configuration instance so that multiple calls can be chained. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ManyToManyModificationStoredProcedureConfiguration<TEntityType, TTargetEntityType> LeftKeyParameter<TProperty>(
            Expression<Func<TEntityType, TProperty>> propertyExpression, string parameterName)
            where TProperty : struct
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            Configuration.Parameter(propertyExpression.GetSimplePropertyAccess(), parameterName);

            return this;
        }

        /// <summary>
        /// Configures the parameter for the left key value(s).
        /// </summary>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns> The same configuration instance so that multiple calls can be chained. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ManyToManyModificationStoredProcedureConfiguration<TEntityType, TTargetEntityType> LeftKeyParameter<TProperty>(
            Expression<Func<TEntityType, TProperty?>> propertyExpression, string parameterName)
            where TProperty : struct
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            Configuration.Parameter(propertyExpression.GetSimplePropertyAccess(), parameterName);

            return this;
        }

        /// <summary>
        /// Configures the parameter for the left key value(s).
        /// </summary>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns> The same configuration instance so that multiple calls can be chained. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ManyToManyModificationStoredProcedureConfiguration<TEntityType, TTargetEntityType> LeftKeyParameter(
            Expression<Func<TEntityType, string>> propertyExpression, string parameterName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            Configuration.Parameter(propertyExpression.GetSimplePropertyAccess(), parameterName);

            return this;
        }

        /// <summary>
        /// Configures the parameter for the left key value(s).
        /// </summary>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns> The same configuration instance so that multiple calls can be chained. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ManyToManyModificationStoredProcedureConfiguration<TEntityType, TTargetEntityType> LeftKeyParameter(
            Expression<Func<TEntityType, byte[]>> propertyExpression, string parameterName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            Configuration.Parameter(propertyExpression.GetSimplePropertyAccess(), parameterName);

            return this;
        }

        /// <summary>
        /// Configures the parameter for the right key value(s).
        /// </summary>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns> The same configuration instance so that multiple calls can be chained. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ManyToManyModificationStoredProcedureConfiguration<TEntityType, TTargetEntityType> RightKeyParameter<TProperty>(
            Expression<Func<TTargetEntityType, TProperty>> propertyExpression, string parameterName)
            where TProperty : struct
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            Configuration.Parameter(propertyExpression.GetSimplePropertyAccess(), parameterName, rightKey: true);

            return this;
        }

        /// <summary>
        /// Configures the parameter for the right key value(s).
        /// </summary>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns> The same configuration instance so that multiple calls can be chained. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ManyToManyModificationStoredProcedureConfiguration<TEntityType, TTargetEntityType> RightKeyParameter<TProperty>(
            Expression<Func<TTargetEntityType, TProperty?>> propertyExpression, string parameterName)
            where TProperty : struct
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            Configuration.Parameter(propertyExpression.GetSimplePropertyAccess(), parameterName, rightKey: true);

            return this;
        }

        /// <summary>
        /// Configures the parameter for the right key value(s).
        /// </summary>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns> The same configuration instance so that multiple calls can be chained. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ManyToManyModificationStoredProcedureConfiguration<TEntityType, TTargetEntityType> RightKeyParameter(
            Expression<Func<TTargetEntityType, string>> propertyExpression, string parameterName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            Configuration.Parameter(propertyExpression.GetSimplePropertyAccess(), parameterName, rightKey: true);

            return this;
        }

        /// <summary>
        /// Configures the parameter for the right key value(s).
        /// </summary>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns> The same configuration instance so that multiple calls can be chained. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ManyToManyModificationStoredProcedureConfiguration<TEntityType, TTargetEntityType> RightKeyParameter(
            Expression<Func<TTargetEntityType, byte[]>> propertyExpression, string parameterName)
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotEmpty(parameterName, "parameterName");

            Configuration.Parameter(propertyExpression.GetSimplePropertyAccess(), parameterName, rightKey: true);

            return this;
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <inheritdoc /> 
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <inheritdoc /> 
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }
    }
}
