// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Infrastructure;

    /// <summary>
    /// This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// 
    /// Represents an error because migrations does not support the given type of project.
    /// </summary>
    [Serializable]
    public sealed class ProjectTypeNotSupportedException : MigrationsException
    {
        /// <summary>
        /// This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// 
        /// Initializes a new instance of the <see cref="ProjectTypeNotSupportedException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ProjectTypeNotSupportedException(string message)
            : base(message)
        {
        }
    }
}
