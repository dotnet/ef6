// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb.SchemaDiscovery
{
    using System;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    internal abstract class FunctionDetailsRowView
    {
        protected readonly object[] Values;

        protected FunctionDetailsRowView(object[] values)
        {
            Debug.Assert(values != null);

            Values = values;
        }

        public abstract string Catalog { get; }

        public abstract string Schema { get; }

        public abstract string ProcedureName { get; }

        public abstract string ReturnType { get; }

        public abstract bool IsIsAggregate { get; }

        public abstract bool IsBuiltIn { get; }

        public abstract bool IsComposable { get; }

        public abstract bool IsNiladic { get; }

        public abstract bool IsTvf { get; }

        public abstract string ParameterName { get; }

        public abstract bool IsParameterNameNull { get; }

        public abstract string ParameterType { get; }

        public abstract bool IsParameterTypeNull { get; }

        public abstract string ProcParameterMode { get; }

        public abstract bool IsParameterModeNull { get; }

        protected static T ConvertDBNull<T>(object value)
        {
            Debug.Assert(value != null, "value != null");

            return Convert.IsDBNull(value) ? default(T) : (T)value;
        }

        public bool TryGetParameterMode(out ParameterMode parameterMode)
        {
            if (!IsParameterModeNull)
            {
                switch (ProcParameterMode)
                {
                    case "IN":
                        parameterMode = ParameterMode.In;
                        return true;
                    case "OUT":
                        parameterMode = ParameterMode.Out;
                        return true;
                    case "INOUT":
                        parameterMode = ParameterMode.InOut;
                        return true;
                }
            }

            parameterMode = (ParameterMode)(-1);
            return false;
        }

        public string GetMostQualifiedFunctionName()
        {
            var name = string.Empty;
            if (Catalog != null)
            {
                name = Catalog;
            }

            if (Schema != null)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    name += ".";
                }
                name += Schema;
            }

            if (!string.IsNullOrEmpty(name))
            {
                name += ".";
            }

            // ProcedureName is not allowed to be null
            name += ProcedureName;

            return name;
        }
    }
}
