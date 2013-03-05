// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.ComponentModel;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    public class LightweightDeleteModificationFunctionConfiguration : LightweightModificationFunctionConfiguration
    {
        private readonly Type _type;

        internal LightweightDeleteModificationFunctionConfiguration(Type type)
        {
            DebugCheck.NotNull(type);

            _type = type;
        }

        public LightweightDeleteModificationFunctionConfiguration HasName(string procedureName)
        {
            Check.NotEmpty(procedureName, "procedureName");

            Configuration.HasName(procedureName);

            return this;
        }

        public LightweightDeleteModificationFunctionConfiguration HasName(string procedureName, string schemaName)
        {
            Check.NotEmpty(procedureName, "procedureName");
            Check.NotEmpty(schemaName, "schemaName");

            Configuration.HasName(procedureName, schemaName);

            return this;
        }

        public LightweightDeleteModificationFunctionConfiguration Parameter(string propertyName, string parameterName)
        {
            Check.NotEmpty(propertyName, "propertyName");
            Check.NotEmpty(parameterName, "parameterName");

            return Parameter(_type.GetProperty(propertyName), parameterName);
        }

        public LightweightDeleteModificationFunctionConfiguration Parameter(
            PropertyInfo propertyInfo, string parameterName)
        {
            Check.NotEmpty(parameterName, "parameterName");

            if (propertyInfo != null)
            {
                Configuration.Parameter(new PropertyPath(propertyInfo), parameterName);
            }

            return this;
        }

        public LightweightDeleteModificationFunctionConfiguration RowsAffectedParameter(string parameterName)
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
