// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestHelpers
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics;
    using System.Linq;

    public sealed class IndexAnnotationEqualityComparer : IEqualityComparer<object>
    {
        public new bool Equals(object x, object y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null)
                || ReferenceEquals(y, null)
                || x.GetType() != typeof(IndexAnnotation)
                || y.GetType() != typeof(IndexAnnotation))
            {
                return false;
            }

            return ((IndexAnnotation)x).Indexes.OrderBy(e => e.Name)
                .SequenceEqual(((IndexAnnotation)y).Indexes.OrderBy(e => e.Name), new IndexAttributeEqualityComparer());
        }

        public int GetHashCode(object obj)
        {
            var annotation = obj as IndexAnnotation;
            Debug.Assert(annotation != null);

            return annotation.Indexes.OrderBy(e => e.Name).Aggregate(0, (h, v) => (h * 397) ^ v.GetHashCode());
        }
    }
}
