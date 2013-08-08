// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Convention to set precision to 18 and scale to 2 for decimal properties.
    /// </summary>
    public class DecimalPropertyConvention : IConceptualModelConvention<EdmProperty>
    {
        private readonly byte _precision;
        private readonly byte _scale;

        /// <summary>
        /// Initializes a new instance of <see cref="DecimalPropertyConvention"/> with the default precision and scale.
        /// </summary>
        public DecimalPropertyConvention()
            : this(18, 2)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DecimalPropertyConvention"/> with the specified precision and scale.
        /// </summary>
        /// <param name="precision"> Precision </param>
        /// <param name="scale"> Scale </param>
        public DecimalPropertyConvention(byte precision, byte scale)
        {
            _precision = precision;
            _scale = scale;
        }

        /// <inheritdoc />
        public virtual void Apply(EdmProperty item, DbModel model)
        {
            Check.NotNull(item, "item");
            Check.NotNull(model, "model");

            if (item.PrimitiveType == PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Decimal))
            {
                if (item.Precision == null)
                {
                    item.Precision = _precision;
                }

                if (item.Scale == null)
                {
                    item.Scale = _scale;
                }
            }
        }
    }
}
