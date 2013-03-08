// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive
{
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    ///     Dummy class used to ignore the configuration of properties that don't exist.
    /// </summary>
    internal class MissingPropertyConfiguration : LightweightPropertyConfiguration
    {
        public override LightweightPropertyConfiguration HasColumnName(string columnName)
        {
            return this;
        }

        public override LightweightPropertyConfiguration HasColumnOrder(int columnOrder)
        {
            return this;
        }

        public override LightweightPropertyConfiguration HasColumnType(string columnType)
        {
            return this;
        }

        public override LightweightPropertyConfiguration HasParameterName(string parameterName)
        {
            return this;
        }

        public override LightweightPropertyConfiguration IsConcurrencyToken()
        {
            return this;
        }

        public override LightweightPropertyConfiguration IsConcurrencyToken(bool concurrencyToken)
        {
            return this;
        }

        public override LightweightPropertyConfiguration HasDatabaseGeneratedOption(DatabaseGeneratedOption databaseGeneratedOption)
        {
            return this;
        }

        public override LightweightPropertyConfiguration IsOptional()
        {
            return this;
        }

        public override LightweightPropertyConfiguration IsRequired()
        {
            return this;
        }

        public override LightweightPropertyConfiguration IsUnicode()
        {
            return this;
        }

        public override LightweightPropertyConfiguration IsUnicode(bool unicode)
        {
            return this;
        }

        public override LightweightPropertyConfiguration IsFixedLength()
        {
            return this;
        }

        public override LightweightPropertyConfiguration IsVariableLength()
        {
            return this;
        }

        public override LightweightPropertyConfiguration HasMaxLength(int value)
        {
            return this;
        }

        public override LightweightPropertyConfiguration IsMaxLength()
        {
            return this;
        }

        public override LightweightPropertyConfiguration HasPrecision(byte value)
        {
            return this;
        }

        public override LightweightPropertyConfiguration HasPrecision(byte precision, byte scale)
        {
            return this;
        }

        public override LightweightPropertyConfiguration IsRowVersion()
        {
            return this;
        }

        public override LightweightPropertyConfiguration IsKey()
        {
            return this;
        }
    }
}
