// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Common.EntitySql
{
    /// <summary>
    /// Represents eSQL error context.
    /// </summary>
    internal class ErrorContext
    {
        /// <summary>
        /// Represents the position of the error in the input stream.
        /// </summary>
        internal int InputPosition = -1;

        /// <summary>
        /// Represents the additional/contextual information related to the error position/cause.
        /// </summary>
        internal string ErrorContextInfo;

        /// <summary>
        /// Defines how ErrorContextInfo should be interpreted.
        /// </summary>
        internal bool UseContextInfoAsResourceIdentifier = true;

        /// <summary>
        /// Represents a referece to the original command text.
        /// </summary>
        internal string CommandText;
    }
}
