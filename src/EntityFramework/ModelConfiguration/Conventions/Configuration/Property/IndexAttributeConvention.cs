// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Linq;

    /// <summary>
    /// A convention for discovering <see cref="IndexAttribute"/> attributes on properties and generating
    /// <see cref="IndexAnnotation"/> column annotations in the model.
    /// </summary>
    public class IndexAttributeConvention : AttributeToColumnAnnotationConvention<IndexAttribute, IndexAnnotation>
    {
        /// <summary>
        /// Constructs a new instance of the convention.
        /// </summary>
        public IndexAttributeConvention()
            : base(IndexAnnotation.AnnotationName, (p, a) => new IndexAnnotation(p, a.OrderBy(i => i.ToString())))
        {
        }
    }
}
