// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents a pair of annotation values in a scaffolded or hand-coded <see cref="DbMigration"/>.
    /// </summary>
    /// <remarks>
    /// Code First allows for custom annotations to be associated with columns and tables in the
    /// generated model. This class represents a pair of annotation values in a migration such
    /// that when the Code First model changes the old annotation value and the new annotation
    /// value can be provided to the migration and used in SQL generation.
    /// </remarks>
    public sealed class AnnotationPair
    {
        private readonly object _oldValue;
        private readonly object _newValue;

        /// <summary>
        /// Creates a new pair of annotation values.
        /// </summary>
        /// <param name="oldValue">The old value of the annotation, which may be null if the annotation has just been created.</param>
        /// <param name="newValue">The new value of the annotation, which may be null if the annotation has been deleted.</param>
        public AnnotationPair(object oldValue, object newValue)
        {
            _oldValue = oldValue;
            _newValue = newValue;
        }

        /// <summary>
        /// Gets the old value of the annotation, which may be null if the annotation has just been created.
        /// </summary>
        public object OldValue
        {
            get { return _oldValue; }
        }

        /// <summary>
        /// Gets the new value of the annotation, which may be null if the annotation has been deleted.
        /// </summary>
        public object NewValue
        {
            get { return _newValue; }
        }

        private bool Equals(AnnotationPair other)
        {
            return Equals(_oldValue, other._oldValue) && Equals(_newValue, other._newValue);
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj is AnnotationPair && Equals((AnnotationPair)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((_oldValue != null ? _oldValue.GetHashCode() : 0) * 397) ^ (_newValue != null ? _newValue.GetHashCode() : 0);
            }
        }

        /// <summary>
        /// Returns true if both annotation pairs contain the same values, otherwise false.
        /// </summary>
        /// <param name="left">A pair of annotation values.</param>
        /// <param name="right">A pair of annotation values.</param>
        /// <returns>True if both pairs contain the same values.</returns>
        [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        public static bool operator ==(AnnotationPair left, AnnotationPair right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Returns true if the two annotation pairs contain different values, otherwise false.
        /// </summary>
        /// <param name="left">A pair of annotation values.</param>
        /// <param name="right">A pair of annotation values.</param>
        /// <returns>True if the pairs contain different values.</returns>
        [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        public static bool operator !=(AnnotationPair left, AnnotationPair right)
        {
            return !Equals(left, right);
        }
    }
}
