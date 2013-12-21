// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Design
{
    using System.Collections.Generic;
    using System.Data.Entity.Migrations.Utilities;
    using System.Data.Entity.Utilities;
    using System.Linq;

    /// <summary>
    /// Inherit from this class to create a service that allows for code generation of custom annotations as part of 
    /// scaffolding Migrations. The derived class should be registered using <see cref="DbConfiguration.SetAnnotationCodeGenerator"/>.
    /// </summary>
    /// <remarks>
    /// Note that an <see cref="AnnotationCodeGenerator" /> is not needed if the annotation uses a simple string value,
    /// or if calling ToString on the annotation object is sufficient for use in the scaffolded Migration.
    /// </remarks>
    public abstract class AnnotationCodeGenerator
    {
        /// <summary>
        /// Override this method to return additional namespaces that should be included in the code generated for the
        /// scaffolded migration. The default implementation returns an empty enumeration.
        /// </summary>
        /// <param name="annotationNames">The names of the annotations that are being included in the generated code.</param>
        /// <returns>A list of additional namespaces to include.</returns>
        public virtual IEnumerable<string> GetExtraNamespaces(IEnumerable<string> annotationNames)
        {
            Check.NotNull(annotationNames, "annotationNames");

            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Implement this method to generate code for the given annotation value.
        /// </summary>
        /// <param name="annotationName">The name of the annotation for which a value is being generated.</param>
        /// <param name="annotation">The annotation value.</param>
        /// <param name="writer">The writer to which generated code should be written.</param>
        public abstract void Generate(string annotationName, object annotation, IndentedTextWriter writer);
    }
}
