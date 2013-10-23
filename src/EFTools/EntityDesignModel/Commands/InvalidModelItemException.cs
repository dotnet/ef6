// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    [SuppressMessage("Microsoft.Design", "CA1064:ExceptionsShouldBePublic")]
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    [Serializable]
    internal class InvalidModelItemException : Exception
    {
        internal InvalidModelItemException()
            : base(Resources.Command_InvalidModelItem)
        {
        }

        protected InvalidModelItemException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
