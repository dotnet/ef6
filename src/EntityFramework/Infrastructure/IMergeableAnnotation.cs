// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Types used as custom annotations can implement this interface to indicate that an attempt to use
    /// multiple annotations with the same name on a given table or column may be possible by merging
    /// the multiple annotations into one.
    /// </summary>
    /// <remarks>
    /// Normally there can only be one custom annotation with a given name on a given table or
    /// column. If a table or column ends up with multiple annotations, for example, because
    /// multiple CLR properties map to the same column, then an exception will be thrown.
    /// However, if the annotation type implements this interface, then the two annotations will be
    /// checked for compatibility using the <see cref="IsCompatibleWith"/> method and, if compatible,
    /// will be merged into one using the <see cref="MergeWith"/> method.
    /// </remarks>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Mergeable")]
    public interface IMergeableAnnotation
    {
        /// <summary>
        /// Returns true if this annotation does not conflict with the given annotation such that
        /// the two can be combined together using the <see cref="MergeWith"/> method.
        /// </summary>
        /// <param name="other">The annotation to compare.</param>
        /// <returns>A CompatibilityResult indicating whether or not this annotation is compatible with the other.</returns>
        CompatibilityResult IsCompatibleWith(object other);

        /// <summary>
        /// Merges this annotation with the given annotation and returns a new merged annotation. This method is
        /// only expected to succeed if <see cref="IsCompatibleWith"/> returns true.
        /// </summary>
        /// <param name="other">The annotation to merge with this one.</param>
        /// <returns>A new merged annotation.</returns>
        object MergeWith(object other);
    }
}
