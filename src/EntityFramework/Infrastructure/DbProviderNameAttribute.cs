// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Generic;
    using System.Data.Entity.Resources;
    using System.Linq;

    /// <summary>
    ///     Indicates that the class is specific to a provider.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class DbProviderNameAttribute : Attribute
    {
        private readonly string _name;

        /// <summary>Initializes a new instance of the <see cref="T:System.Data.Entity.Infrastructure.DbProviderNameAttribute" /> class.</summary>
        /// <param name="name">The name of the provider.</param>
        public DbProviderNameAttribute(string name)
        {
            _name = name;
        }

        /// <summary>Gets or sets the name of the provider.</summary>
        /// <returns>The name of the provider.</returns>
        public string Name
        {
            get { return _name; }
        }

        internal static IEnumerable<DbProviderNameAttribute> GetFromType(Type type)
        {
            var providerInvariantNameAttributes = type.GetCustomAttributes(inherit: false)
                                                      .OfType<DbProviderNameAttribute>();

            if (!providerInvariantNameAttributes.Any())
            {
                throw new InvalidOperationException(Strings.DbProviderNameAttributeNotFound(type));
            }

            return providerInvariantNameAttributes;
        }
    }
}
