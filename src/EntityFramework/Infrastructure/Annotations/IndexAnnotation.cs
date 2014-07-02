// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Annotations
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Instances of this class are used as custom annotations for representing database indexes in an
    /// Entity Framework model.
    /// </summary>
    /// <remarks>
    /// An index annotation is added to a Code First model when an <see cref="IndexAttribute"/> is placed on
    /// a mapped property of that model. This is used by Entity Framework Migrations to create indexes on
    /// mapped database columns. Note that multiple index attributes on a property will be merged into a
    /// single annotation for the column. Similarly, index attributes on multiple properties that map to the
    /// same column will be merged into a single annotation for the column. This means that one index
    /// annotation can represent multiple indexes. Within an annotation there can be only one index with any
    /// given name.
    /// </remarks>
    public class IndexAnnotation : IMergeableAnnotation
    {
        /// <summary>
        /// The name used when this annotation is stored in Entity Framework metadata or serialized into
        /// an SSDL/EDMX file.
        /// </summary>
        public const string AnnotationName = "Index";
        
        private readonly IList<IndexAttribute> _indexes = new List<IndexAttribute>();

        /// <summary>
        /// Creates a new annotation for the given index.
        /// </summary>
        /// <param name="indexAttribute">An index attributes representing an index.</param>
        public IndexAnnotation(IndexAttribute indexAttribute)
        {
            Check.NotNull(indexAttribute, "indexAttribute");

            _indexes.Add(indexAttribute);
        }

        /// <summary>
        /// Creates a new annotation for the given collection of indexes.
        /// </summary>
        /// <param name="indexAttributes">Index attributes representing one or more indexes.</param>
        public IndexAnnotation(IEnumerable<IndexAttribute> indexAttributes)
        {
            Check.NotNull(indexAttributes, "indexAttributes");

            MergeLists(_indexes, indexAttributes, null);
        }

        internal IndexAnnotation(PropertyInfo propertyInfo, IEnumerable<IndexAttribute> indexAttributes)
        {
            Check.NotNull(indexAttributes, "indexAttributes");

            MergeLists(_indexes, indexAttributes, propertyInfo);
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        private static void MergeLists(
            ICollection<IndexAttribute> existingIndexes,
            IEnumerable<IndexAttribute> newIndexes,
            PropertyInfo propertyInfo)
        {
            foreach (var index in newIndexes)
            {
                if (index == null)
                {
                    throw new ArgumentNullException("indexAttribute");
                }

                var matches = existingIndexes.Where(
                    i => ((index.Identity.HasValue && i.Identity == index.Identity) || i.Name == index.Name)
                        && !(i.Identity != index.Identity && i.Name == null && index.Name == null));

                if (matches.Count() > 1)
                {
                    throw Error.ConflictingIndexAttributeMatches(index.Identity, index.Name);
                }

                var existingIndex = matches.SingleOrDefault();

                if (existingIndex == null)
                {
                    existingIndexes.Add(index);
                }
                else
                {
                    var isCompatible = index.IsCompatibleWith(existingIndex);
                    if (isCompatible)
                    {
                        existingIndexes.Remove(existingIndex);
                        existingIndexes.Add(index.MergeWith(existingIndex));
                    }
                    else
                    {
                        var errorMessage = Environment.NewLine + "\t" + isCompatible.ErrorMessage;
                        throw new InvalidOperationException(
                            propertyInfo == null
                                ? Strings.ConflictingIndexAttribute(existingIndex.Name, errorMessage)
                                : Strings.ConflictingIndexAttributesOnProperty(
                                    propertyInfo.Name, propertyInfo.ReflectedType.Name, existingIndex.Name, errorMessage));
                    }
                }
            }
        }

        /// <summary>
        /// Gets the indexes represented by this annotation.
        /// </summary>
        public virtual IEnumerable<IndexAttribute> Indexes
        {
            get { return _indexes; }
        }

        /// <summary>
        /// Returns true if this annotation does not conflict with the given annotation such that
        /// the two can be combined together using the <see cref="MergeWith"/> method.
        /// </summary>
        /// <remarks>
        /// Each index annotation contains at most one <see cref="IndexAttribute"/> with a given name.
        /// Two annotations are considered compatible if each IndexAttribute with a given name is only
        /// contained in one annotation or the other, or if both annotations contain an IndexAttribute
        /// with the given name.
        /// </remarks>
        /// <param name="other">The annotation to compare.</param>
        /// <returns>A CompatibilityResult indicating whether or not this annotation is compatible with the other.</returns>
        public virtual CompatibilityResult IsCompatibleWith(object other)
        {
            if (ReferenceEquals(this, other)
                || other == null)
            {
                return new CompatibilityResult(true, null);
            }

            var otherAnnotation = other as IndexAnnotation;
            if (otherAnnotation == null)
            {
                return new CompatibilityResult(false, Strings.IncompatibleTypes(other.GetType().Name, typeof(IndexAnnotation).Name));
            }

            foreach (var newIndex in otherAnnotation._indexes)
            {
                var matches = _indexes.Where(
                    i => ((newIndex.Identity != null && i.Identity == newIndex.Identity) || i.Name == newIndex.Name)
                        && !(i.Identity != newIndex.Identity && i.Name == null && newIndex.Name == null));

                if (matches.Count() > 1)
                {
                    throw Error.ConflictingIndexAttributeMatches(newIndex.Identity, newIndex.Name);
                }

                var existing = matches.SingleOrDefault();

                if (existing != null)
                {
                    var isCompatible = existing.IsCompatibleWith(newIndex);
                    if (!isCompatible)
                    {
                        return isCompatible;
                    }
                }
            }

            return new CompatibilityResult(true, null);
        }

        /// <summary>
        /// Merges this annotation with the given annotation and returns a new annotation containing the merged indexes.
        /// </summary>
        /// <remarks>
        /// Each index annotation contains at most one <see cref="IndexAttribute"/> with a given name.
        /// The merged annotation will contain IndexAttributes from both this and the other annotation.
        /// If both annotations contain an IndexAttribute with the same name, then the merged annotation
        /// will contain one IndexAttribute with that name.
        /// </remarks>
        /// <param name="other">The annotation to merge with this one.</param>
        /// <returns>A new annotation with indexes from both annotations merged.</returns>
        /// <exception cref="InvalidOperationException">
        /// The other annotation contains indexes that are not compatible with indexes in this annotation.
        /// </exception>
        public virtual object MergeWith(object other)
        {
            if (ReferenceEquals(this, other)
                || other == null)
            {
                return this;
            }

            var otherAnnotation = other as IndexAnnotation;
            if (otherAnnotation == null)
            {
                throw new ArgumentException(Strings.IncompatibleTypes(other.GetType().Name, typeof(IndexAnnotation).Name));
            }

            var merged = _indexes.ToList();
            MergeLists(merged, otherAnnotation._indexes, null);
            return new IndexAnnotation(merged);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "IndexAnnotation: " + new IndexAnnotationSerializer().Serialize(AnnotationName, this);
        }
    }
}
