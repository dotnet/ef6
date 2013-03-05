// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.ComponentModel;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    public class LightweightUpdateModificationFunctionConfiguration : LightweightModificationFunctionConfiguration
    {
        private readonly Type _type;

        internal LightweightUpdateModificationFunctionConfiguration(Type type)
        {
            DebugCheck.NotNull(type);

            _type = type;
        }

        public LightweightUpdateModificationFunctionConfiguration HasName(string procedureName)
        {
            Check.NotEmpty(procedureName, "procedureName");

            Configuration.HasName(procedureName);

            return this;
        }

        public LightweightUpdateModificationFunctionConfiguration HasName(string procedureName, string schemaName)
        {
            Check.NotEmpty(procedureName, "procedureName");
            Check.NotEmpty(schemaName, "schemaName");

            Configuration.HasName(procedureName, schemaName);

            return this;
        }

        public LightweightUpdateModificationFunctionConfiguration Parameter(string propertyName, string parameterName)
        {
            Check.NotEmpty(propertyName, "propertyName");
            Check.NotEmpty(parameterName, "parameterName");

            return Parameter(_type.GetProperty(propertyName), parameterName);
        }

        public LightweightUpdateModificationFunctionConfiguration Parameter(
            PropertyInfo propertyInfo, string parameterName)
        {
            Check.NotEmpty(parameterName, "parameterName");

            if (propertyInfo != null)
            {
                Configuration.Parameter(new PropertyPath(propertyInfo), parameterName);
            }

            return this;
        }

        public LightweightUpdateModificationFunctionConfiguration Parameter(
            string propertyName, string currentValueParameterName, string originalValueParameterName)
        {
            Check.NotEmpty(propertyName, "propertyName");
            Check.NotEmpty(currentValueParameterName, "currentValueParameterName");
            Check.NotEmpty(originalValueParameterName, "originalValueParameterName");

            return Parameter(_type.GetProperty(propertyName), currentValueParameterName, originalValueParameterName);
        }

        public LightweightUpdateModificationFunctionConfiguration Parameter(
            PropertyInfo propertyInfo, string currentValueParameterName, string originalValueParameterName)
        {
            Check.NotEmpty(currentValueParameterName, "currentValueParameterName");
            Check.NotEmpty(originalValueParameterName, "originalValueParameterName");

            if (propertyInfo != null)
            {
                Configuration.Parameter(
                    new PropertyPath(propertyInfo),
                    currentValueParameterName,
                    originalValueParameterName);
            }

            return this;
        }

        public LightweightUpdateModificationFunctionConfiguration Result(string propertyName, string columnName)
        {
            Check.NotEmpty(propertyName, "propertyName");
            Check.NotEmpty(columnName, "columnName");

            Configuration.Result(new PropertyPath(_type.GetProperty(propertyName)), columnName);

            return this;
        }

        public LightweightUpdateModificationFunctionConfiguration Result(PropertyInfo propertyInfo, string columnName)
        {
            Check.NotNull(propertyInfo, "propertyInfo");
            Check.NotEmpty(columnName, "columnName");

            Configuration.Result(new PropertyPath(propertyInfo), columnName);

            return this;
        }

        public LightweightUpdateModificationFunctionConfiguration RowsAffectedParameter(string parameterName)
        {
            Check.NotEmpty(parameterName, "parameterName");

            Configuration.RowsAffectedParameter(parameterName);

            return this;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }
    }
}
