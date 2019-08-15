// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    /// <summary>
    /// Place this attribute on a type that inherits from <see cref="System.Data.Entity.DbContext"/> to
    /// force all <see cref="System.DateTime"/> parameters to have the given <see cref="System.Data.DbType"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ForceDateTimeTypeAttribute : Attribute
    {
        private readonly DbType _type;

        /// <summary>
        /// Creates a new <see cref="ForceDateTimeTypeAttribute"/> instance with the given <see cref="System.Data.DbType"/>. 
        /// </summary>
        /// <param name="type"> The type to force. </param>
        public ForceDateTimeTypeAttribute(DbType type)
        {
            _type = type;
        }

        /// <summary>
        /// The <see cref="System.Data.DbType"/> that will be forced for all <see cref="System.DateTime"/> parameters.
        /// </summary>
        public DbType DateTimeType
        {
            get { return _type; }
        }
    }
}