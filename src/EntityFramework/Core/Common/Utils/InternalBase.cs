// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Common.Utils
{
    using System.Text;

    // A basic class from which all classes derive so that ToString can be
    // more controlled
    internal abstract class InternalBase
    {
        // effects: Modify builder to contain a compact string representation
        // of this
        internal abstract void ToCompactString(StringBuilder builder);

        // effects: Modify builder to contain a verbose string representation
        // of this
        internal virtual void ToFullString(StringBuilder builder)
        {
            ToCompactString(builder);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            ToCompactString(builder);
            return builder.ToString();
        }

        internal virtual string ToFullString()
        {
            var builder = new StringBuilder();
            ToFullString(builder);
            return builder.ToString();
        }
    }
}
