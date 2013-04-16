// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Data.Entity.Core.Metadata.Edm;

    /// <summary>
    ///     Represents information about a parameter.
    /// </summary>
    public class ParameterModel : PropertyModel
    {
        /// <summary>
        ///     Initializes a new instance of the ParameterModel class.
        /// </summary>
        /// <param name="type"> The data type for this parameter. </param>
        public ParameterModel(PrimitiveTypeKind type)
            : this(type, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the ParameterModel class.
        /// </summary>
        /// <param name="type"> The data type for this parameter. </param>
        /// <param name="typeUsage"> Additional details about the data type. This includes details such as maximum length, nullability etc. </param>
        public ParameterModel(PrimitiveTypeKind type, TypeUsage typeUsage)
            : base(type, typeUsage)
        {
        }

        public bool IsOutParameter { get; set; }
    }
}
