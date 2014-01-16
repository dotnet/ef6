// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// A general purpose class for Code First conventions that read attributes from .NET types
    /// and generate table annotations based on those attributes.
    /// </summary>
    /// <typeparam name="TAttribute">The type of attribute to discover.</typeparam>
    /// <typeparam name="TAnnotation">The type of annotation that will be created.</typeparam>
    public class AttributeToTableAnnotationConvention<TAttribute, TAnnotation> : Convention
        where TAttribute : Attribute
    {
        /// <summary>
        /// Constructs a convention that will create table annotations with the given name and
        /// using the given factory delegate.
        /// </summary>
        /// <param name="annotationName">The name of the annotations to create.</param>
        /// <param name="annotationFactory">A factory for creating the annotation on each table.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public AttributeToTableAnnotationConvention(
            string annotationName, Func<Type, IList<TAttribute>, TAnnotation> annotationFactory)
        {
            Check.NotEmpty(annotationName, "annotationName");
            Check.NotNull(annotationFactory, "annotationFactory");

            var attributeProvider = DbConfiguration.DependencyResolver.GetService<AttributeProvider>();

            Types().Having(t => attributeProvider.GetAttributes(t).OfType<TAttribute>().ToList()).Configure(
                (c, a) =>
                {
                    if (a.Any())
                    {
                        c.HasTableAnnotation(annotationName, annotationFactory(c.ClrType, a));
                    }
                });
        }
    }
}
