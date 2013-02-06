// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;

    public class FunctionParameterConfiguration
    {
        private string _parameterName;

        internal FunctionParameterConfiguration()
        {
        }

        private FunctionParameterConfiguration(FunctionParameterConfiguration source)
        {
            DebugCheck.NotNull(source);

            _parameterName = source.ParameterName;
        }

        internal string ParameterName
        {
            get { return _parameterName; }
        }

        public void HasName(string parameterName)
        {
            Check.NotEmpty(parameterName, "parameterName");

            _parameterName = parameterName;
        }

        internal virtual FunctionParameterConfiguration Clone()
        {
            return new FunctionParameterConfiguration(this);
        }

        internal void Configure(FunctionParameter functionParameter)
        {
            DebugCheck.NotNull(functionParameter);

            if (!string.IsNullOrWhiteSpace(ParameterName))
            {
                functionParameter.Name = ParameterName;
            }
        }
    }
}
