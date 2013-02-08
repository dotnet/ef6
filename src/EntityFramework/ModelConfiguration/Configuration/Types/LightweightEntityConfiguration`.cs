// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Types
{
    using System.Collections.Generic;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    ///     Allows configuration to be performed for an entity type in a model.
    ///     This configuration functionality is available via lightweight conventions.
    /// </summary>
    /// <typeparam name="T"> A type inherited by the entity type. </typeparam>
    public class LightweightEntityConfiguration<T> : LightweightEntityConfiguration
        where T : class
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LightweightEntityConfiguration{T}" /> class.
        /// </summary>
        /// <param name="type">
        ///     The <see cref="Type" /> of this entity type.
        /// </param>
        /// <param name="configuration"> The configuration object that this instance wraps. </param>
        public LightweightEntityConfiguration(Type type, Func<EntityTypeConfiguration> configuration)
            : base(type, configuration)
        {
            Check.NotNull(type, "type");
            Check.NotNull(configuration, "configuration");

            if (!typeof(T).IsAssignableFrom(type))
            {
                throw Error.LightweightEntityConfiguration_TypeMismatch(type, typeof(T));
            }
        }

        /// <summary>
        ///     Configures the entity set name to be used for this entity type.
        ///     The entity set name can only be configured for the base type in each set.
        /// </summary>
        /// <param name="entitySetName"> The name of the entity set. </param>
        /// <returns>
        ///     The same <see cref="LightweightEntityConfiguration{T}" /> instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        ///     Calling this will have no effect once it has been configured.
        /// </remarks>
        public new LightweightEntityConfiguration<T> HasEntitySetName(string entitySetName)
        {
            base.HasEntitySetName(entitySetName);

            return this;
        }

        /// <summary>
        ///     Excludes a property from the model so that it will not be mapped to the database.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property to be ignored. </typeparam>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public void Ignore<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
        {
            Check.NotNull(propertyExpression, "propertyExpression");

            Ignore(propertyExpression.GetSimplePropertyAccess().Single());
        }

        /// <summary>
        ///     Configures a property that is defined on this type.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured. </typeparam>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to configure the property. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public LightweightPropertyConfiguration Property<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
        {
            Check.NotNull(propertyExpression, "propertyExpression");

            return Property(propertyExpression.GetComplexPropertyAccess());
        }

        /// <summary>
        ///     Configures the primary key property for this entity type.
        /// </summary>
        /// <param name="propertyName"> The name of the property to be used as the primary key. </param>
        /// <returns>
        ///     The same <see cref="LightweightEntityConfiguration{T}" /> instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        ///     Calling this will have no effect once it has been configured of if the
        ///     property does not exist.
        /// </remarks>
        public new LightweightEntityConfiguration<T> HasKey(string propertyName)
        {
            base.HasKey(propertyName);

            return this;
        }

        /// <summary>
        ///     Configures the primary key property for this entity type.
        /// </summary>
        /// <param name="propertyInfo"> The property to be used as the primary key. </param>
        /// <returns>
        ///     The same <see cref="LightweightEntityConfiguration{T}" /> instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        ///     Calling this will have no effect once it has been configured of if the
        ///     property does not exist.
        /// </remarks>
        public new LightweightEntityConfiguration<T> HasKey(PropertyInfo propertyInfo)
        {
            base.HasKey(propertyInfo);

            return this;
        }

        /// <summary>
        ///     Configures the primary key property(s) for this entity type.
        /// </summary>
        /// <param name="propertyNames"> The names of the properties to be used as the primary key. </param>
        /// <returns>
        ///     The same <see cref="LightweightEntityConfiguration{T}" /> instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        ///     Calling this will have no effect once it has been configured or if any
        ///     property does not exist.
        /// </remarks>
        public new LightweightEntityConfiguration<T> HasKey(IEnumerable<string> propertyNames)
        {
            base.HasKey(propertyNames);

            return this;
        }

        /// <summary>
        ///     Configures the primary key property(s) for this entity type.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the key. </typeparam>
        /// <param name="keyExpression"> A lambda expression representing the property to be used as the primary key. C#: t => t.Id VB.Net: Function(t) t.Id If the primary key is made up of multiple properties then specify an anonymous type including the properties. C#: t => new { t.Id1, t.Id2 } VB.Net: Function(t) New With { t.Id1, t.Id2 } </param>
        /// <returns>
        ///     The same <see cref="LightweightEntityConfiguration{T}" /> instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        ///     Calling this will have no effect once it has been configured.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public LightweightEntityConfiguration<T> HasKey<TProperty>(Expression<Func<T, TProperty>> keyExpression)
        {
            Check.NotNull(keyExpression, "keyExpression");

            return HasKey(keyExpression.GetSimplePropertyAccessList().Select(p => p.Single()));
        }

        /// <summary>
        ///     Configures the primary key property(s) for this entity type.
        /// </summary>
        /// <param name="keyProperties"> The properties to be used as the primary key. </param>
        /// <returns>
        ///     The same <see cref="LightweightEntityConfiguration{T}" /> instance so that multiple calls can be chained.
        /// </returns>
        /// <remarks>
        ///     Calling this will have no effect once it has been configured or if any
        ///     property does not exist.
        /// </remarks>
        public new LightweightEntityConfiguration<T> HasKey(IEnumerable<PropertyInfo> keyProperties)
        {
            base.HasKey(keyProperties);

            return this;
        }
    }
}
