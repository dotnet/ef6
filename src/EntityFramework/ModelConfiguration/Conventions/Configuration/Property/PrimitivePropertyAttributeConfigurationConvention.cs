// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Config;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Linq;

    /// <summary>
    ///     Base class for conventions that process CLR attributes found on primitive properties in the model.
    /// </summary>
    /// <typeparam name="TAttribute"> The type of the attribute to look for. </typeparam>
    public abstract class PrimitivePropertyAttributeConfigurationConvention<TAttribute>
        : Convention
        where TAttribute : Attribute
    {
        private readonly AttributeProvider _attributeProvider = DbConfiguration.GetService<AttributeProvider>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PrimitivePropertyAttributeConfigurationConvention{TAttribute}"/> class.
        /// </summary>
        protected PrimitivePropertyAttributeConfigurationConvention()
        {
            Properties().Having(pi => _attributeProvider.GetAttributes(pi).OfType<TAttribute>()).Configure(
                (configuration, attributes) =>
                    {
                        foreach (var attribute in attributes)
                        {
                            Apply(configuration, attribute);
                        }
                    });
        }

        /// <summary>
        /// Applies this convention to a property that has an attribute of type TAttribute applied.
        /// </summary>
        /// <param name="configuration">The configuration for the property that has the attribute.</param>
        /// <param name="attribute">The attribute.</param>
        public abstract void Apply(ConventionPrimitivePropertyConfiguration configuration, TAttribute attribute);
    }
}
