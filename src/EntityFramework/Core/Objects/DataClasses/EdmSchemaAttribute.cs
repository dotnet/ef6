// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.DataClasses
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Attribute for static types
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
    public sealed class EdmSchemaAttribute : Attribute
    {
        /// <summary>
        ///     Constructor for EdmSchemaAttribute
        /// </summary>
        public EdmSchemaAttribute()
        {
        }

        /// <summary>
        ///     Setting this parameter to a unique value for each model file in a Visual Basic
        ///     assembly will prevent the following error:
        ///     "'System.Data.Entity.Core.Objects.DataClasses.EdmSchemaAttribute' cannot be specified more than once in this project, even with identical parameter values."
        /// </summary>
        public EdmSchemaAttribute(string assemblyGuid)
        {
            if (null == assemblyGuid)
            {
                throw new ArgumentNullException("assemblyGuid");
            }
        }
    }
}
