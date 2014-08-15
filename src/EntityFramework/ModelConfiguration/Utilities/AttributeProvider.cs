// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Utilities
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;

    internal class AttributeProvider
    {
        private readonly ConcurrentDictionary<PropertyInfo, IEnumerable<Attribute>> _discoveredAttributes =
            new ConcurrentDictionary<PropertyInfo, IEnumerable<Attribute>>();

        public virtual IEnumerable<Attribute> GetAttributes(MemberInfo memberInfo)
        {
            DebugCheck.NotNull(memberInfo);

            var type = memberInfo as Type;

            if (type != null)
            {
                return GetAttributes(type);
            }

            return GetAttributes((PropertyInfo)memberInfo);
        }

        public virtual IEnumerable<Attribute> GetAttributes(Type type)
        {
            DebugCheck.NotNull(type);

            var attrs = new List<Attribute>(GetTypeDescriptor(type).GetAttributes().Cast<Attribute>());

            // Data Services workaround
            foreach (var attribute in type.GetCustomAttributes<Attribute>(inherit: true)
                                          .Where(
                                              a =>
                                              a.GetType().FullName.Equals(
                                                  "System.Data.Services.Common.EntityPropertyMappingAttribute", StringComparison.Ordinal) &&
                                              !attrs.Contains(a)))
            {
                attrs.Add(attribute);
            }

            return attrs;
        }

        public virtual IEnumerable<Attribute> GetAttributes(PropertyInfo propertyInfo)
        {
            DebugCheck.NotNull(propertyInfo);

            return _discoveredAttributes.GetOrAdd(
                propertyInfo, pi =>
                   {
                       // PERF: this code is part of a critical section, consider its performance when refactoring
                       var typeDescriptor = GetTypeDescriptor(pi.DeclaringType);
                       var propertyCollection = typeDescriptor.GetProperties();
                       var propertyDescriptor = propertyCollection[pi.Name];

                       var propertyAttributes
                           = (propertyDescriptor != null)
                                   ? propertyDescriptor.Attributes.Cast<Attribute>()
                           // Fallback to standard reflection (non-public properties)
                                   : pi.GetCustomAttributes<Attribute>(inherit: true);

                       // Get the attributes for the property's type and exclude them
                       var propertyTypeAttributes = (ICollection<Attribute>)GetAttributes(pi.PropertyType);
                       if (propertyTypeAttributes.Count > 0)
                       {
                           propertyAttributes = propertyAttributes.Except(propertyTypeAttributes);
                       }
                       return propertyAttributes.ToList();
                   });
        }

        private static ICustomTypeDescriptor GetTypeDescriptor(Type type)
        {
            DebugCheck.NotNull(type);

            return new AssociatedMetadataTypeTypeDescriptionProvider(type).GetTypeDescriptor(type);
        }

        public virtual void ClearCache()
        {
            _discoveredAttributes.Clear();
        }
    }
}
