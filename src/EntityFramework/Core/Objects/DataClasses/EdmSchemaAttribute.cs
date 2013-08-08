// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.DataClasses
{
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Attribute for static types
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
    public sealed class EdmSchemaAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.Objects.DataClasses.EdmSchemaAttribute" /> class.
        /// </summary>
        public EdmSchemaAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.Objects.DataClasses.EdmSchemaAttribute" /> class with a unique value for each model referenced by the assembly.
        /// </summary>
        /// <remarks>
        /// Setting this parameter to a unique value for each model file in a Visual Basic
        /// assembly will prevent the following error:
        /// "'System.Data.Entity.Core.Objects.DataClasses.EdmSchemaAttribute' cannot be specified more than once in this project, even with identical parameter values."
        /// </remarks>
        /// <param name="assemblyGuid">A string that is a unique GUID value for the model in the assembly.</param>
        public EdmSchemaAttribute(string assemblyGuid)
        {
            Check.NotNull(assemblyGuid, "assemblyGuid");
        }
    }
}
