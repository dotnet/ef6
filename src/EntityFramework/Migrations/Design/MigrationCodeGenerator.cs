// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Design
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// Base class for providers that generate code for code-based migrations.
    /// </summary>
    public abstract class MigrationCodeGenerator
    {
        private readonly IDictionary<string, Func<AnnotationCodeGenerator>> _annotationGenerators =
            new Dictionary<string, Func<AnnotationCodeGenerator>>();

        private readonly Func<IDbDependencyResolver> _resolver;

        /// <summary>
        /// Constructs a new <see cref="MigrationCodeGenerator"/> instance.
        /// </summary>
        protected MigrationCodeGenerator()
            : this(null)
        {
        }

        internal MigrationCodeGenerator(Func<IDbDependencyResolver> resolver)
        {
            _resolver = resolver ?? (() => DbConfiguration.DependencyResolver);
        }

        /// <summary>
        /// Generates the code that should be added to the users project.
        /// </summary>
        /// <param name="migrationId"> Unique identifier of the migration. </param>
        /// <param name="operations"> Operations to be performed by the migration. </param>
        /// <param name="sourceModel"> Source model to be stored in the migration metadata. </param>
        /// <param name="targetModel"> Target model to be stored in the migration metadata. </param>
        /// <param name="namespace"> Namespace that code should be generated in. </param>
        /// <param name="className"> Name of the class that should be generated. </param>
        /// <returns> The generated code. </returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "namespace")]
        public abstract ScaffoldedMigration Generate(
            string migrationId,
            IEnumerable<MigrationOperation> operations,
            string sourceModel,
            string targetModel,
            string @namespace,
            string className);

        /// <summary>
        /// Call this method to find and register <see cref="AnnotationCodeGenerator"/> objects for the custom annotations
        /// used in the given operations. This method must be called before calling <see cref="Generate"/> if the operations
        /// use any custom annotations.
        /// </summary>
        /// <param name="operations">The operations for which code will be generated.</param>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public virtual void RegisterAnnotationGenerators(IEnumerable<MigrationOperation> operations)
        {
            Check.NotNull(operations, "operations");

            operations = operations.ToArray();
            var annotationNames =
                // Add column; inverse is created automatically with same keys
                operations.OfType<AddColumnOperation>().SelectMany(o => o.Column.Annotations.Keys)
                    // Drop column and inverse annotations 
                    .Concat(operations.OfType<DropColumnOperation>().SelectMany(o => o.RemovedAnnotations.Keys))
                    .Concat(
                        operations.OfType<DropColumnOperation>()
                            .Where(o => o.Inverse is AddColumnOperation)
                            .SelectMany(o => ((AddColumnOperation)o.Inverse).Column.Annotations.Keys))
                    // Alter column annotations; inverse is created automatically with same keys
                    .Concat(operations.OfType<AlterColumnOperation>().SelectMany(o => o.Column.Annotations.Keys))
                    // Create table column/table annotations; inverse is created automatically with same keys
                    .Concat(operations.OfType<CreateTableOperation>().SelectMany(o => o.Annotations.Keys))
                    .Concat(operations.OfType<CreateTableOperation>().SelectMany(o => o.Columns).SelectMany(c => c.Annotations.Keys))
                    // Drop table and inverse column/table annotations
                    .Concat(
                        operations.OfType<DropTableOperation>()
                            .SelectMany(o => o.RemovedAnnotations.Keys.Concat(o.RemovedColumnAnnotations.SelectMany(c => c.Value.Keys))))
                    .Concat(
                        operations.OfType<DropTableOperation>()
                            .Where(o => o.Inverse is CreateTableOperation)
                            .SelectMany(o => ((CreateTableOperation)o.Inverse).Annotations.Keys))
                    .Concat(
                        operations.OfType<DropTableOperation>()
                            .Where(o => o.Inverse is CreateTableOperation)
                            .SelectMany(o => ((CreateTableOperation)o.Inverse).Columns).SelectMany(c => c.Annotations.Keys))
                    // Alter table annotations; inverse is same set of keys
                    .Concat(operations.OfType<AlterTableAnnotationsOperation>().SelectMany(o => o.Annotations.Keys))
                    .Concat(
                        operations.OfType<AlterTableAnnotationsOperation>().SelectMany(o => o.Columns).SelectMany(c => c.Annotations.Keys))
                    .Distinct()
                    .ToArray();

            var resolver = _resolver();
            foreach (var name in annotationNames.Where(n => !_annotationGenerators.ContainsKey(n)))
            {
                _annotationGenerators[name] = resolver.GetService<Func<AnnotationCodeGenerator>>(name);
            }
        }

        /// <summary>
        /// Gets the namespaces that must be output as "using" or "Imports" directives to handle
        /// the code generated by the given operations.
        /// </summary>
        /// <param name="operations"> The operations for which code is going to be generated. </param>
        /// <returns> An ordered list of namespace names. </returns>
        protected virtual IEnumerable<string> GetNamespaces(IEnumerable<MigrationOperation> operations)
        {
            var namespaces = GetDefaultNamespaces();

            if (operations.OfType<AddColumnOperation>().Any(
                o => o.Column.Type == PrimitiveTypeKind.Geography || o.Column.Type == PrimitiveTypeKind.Geometry))
            {
                namespaces = namespaces.Concat(new[] { "System.Data.Entity.Spatial" });
            }

            if (AnnotationGenerators.Any())
            {
                namespaces = namespaces.Concat(new[] { "System.Collections.Generic" });

                namespaces = AnnotationGenerators.Select(a => a.Value).Where(g => g != null)
                    .Aggregate(namespaces, (c, g) => c.Concat(g().GetExtraNamespaces(AnnotationGenerators.Keys)));
            }

            return namespaces.Distinct().OrderBy(n => n);
        }

        /// <summary>
        /// Gets the default namespaces that must be output as "using" or "Imports" directives for
        /// any code generated.
        /// </summary>
        /// <param name="designer"> A value indicating if this class is being generated for a code-behind file. </param>
        /// <returns> An ordered list of namespace names. </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected virtual IEnumerable<string> GetDefaultNamespaces(bool designer = false)
        {
            var namespaces
                = new List<string>
                    {
                        "System.Data.Entity.Migrations"
                    };

            if (designer)
            {
                namespaces.Add("System.CodeDom.Compiler");
                namespaces.Add("System.Data.Entity.Migrations.Infrastructure");
                namespaces.Add("System.Resources");
            }
            else
            {
                namespaces.Add("System");
            }

            return namespaces.OrderBy(n => n);
        }

        /// <summary>
        /// Gets the <see cref="AnnotationCodeGenerator"/> instances that were registered by <see cref="RegisterAnnotationGenerators"/>.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        protected virtual IDictionary<string, Func<AnnotationCodeGenerator>> AnnotationGenerators
        {
            get { return _annotationGenerators; }
        }
    }
}
