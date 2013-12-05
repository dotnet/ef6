// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// A general purpose class for Code First conventions that read attributes from .NET properties
    /// and generate column annotations based on those attributes.
    /// </summary>
    /// <typeparam name="TAttribute">The type of attribute to discover.</typeparam>
    /// <typeparam name="TAnnotation">The type of annotation that will be created.</typeparam>
    public class AttributeToColumnAnnotationConvention<TAttribute, TAnnotation> : Convention
        where TAttribute : Attribute
    {
        /// <summary>
        /// Constructs a convention that will create column annotations with the given name and
        /// using the given factory delegate.
        /// </summary>
        /// <param name="annotationName">The name of the annotations to create.</param>
        /// <param name="annotationFactory">A factory for creating the annotation on each column.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public AttributeToColumnAnnotationConvention(
            string annotationName, Func<PropertyInfo, IList<TAttribute>, TAnnotation> annotationFactory)
        {
            Check.NotEmpty(annotationName, "annotationName");
            Check.NotNull(annotationFactory, "annotationFactory");

            var attributeProvider = DbConfiguration.DependencyResolver.GetService<AttributeProvider>();

            Properties().Having(pi => attributeProvider.GetAttributes(pi).OfType<TAttribute>().ToList()).Configure(
                (c, a) =>
                {
                    if (a.Any())
                    {
                        c.HasAnnotation(annotationName, annotationFactory(c.ClrPropertyInfo, a));
                    }
                });
        }
    }
}
