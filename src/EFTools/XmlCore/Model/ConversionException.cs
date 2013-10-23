// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    [SuppressMessage("Microsoft.Design", "CA1064:ExceptionsShouldBePublic")]
    [Serializable]
    internal class ConversionException : Exception
    {
        internal ConversionException(string message)
            : base(message)
        {
        }

        protected ConversionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
