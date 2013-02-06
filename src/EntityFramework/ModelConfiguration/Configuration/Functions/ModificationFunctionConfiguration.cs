// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;

    internal class ModificationFunctionConfiguration
    {
        private string _name;

        private readonly Dictionary<PropertyPath, FunctionParameterConfiguration> _parameterConfigurations
            = new Dictionary<PropertyPath, FunctionParameterConfiguration>();

        public ModificationFunctionConfiguration()
        {
        }

        private ModificationFunctionConfiguration(ModificationFunctionConfiguration source)
        {
            DebugCheck.NotNull(source);

            _name = source._name;

            source._parameterConfigurations.Each(
                c => _parameterConfigurations.Add(c.Key, c.Value.Clone()));
        }

        public virtual ModificationFunctionConfiguration Clone()
        {
            return new ModificationFunctionConfiguration(this);
        }

        public void HasName(string name)
        {
            DebugCheck.NotEmpty(name);

            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }

        public Dictionary<PropertyPath, FunctionParameterConfiguration> ParameterConfigurations
        {
            get { return _parameterConfigurations; }
        }

        public FunctionParameterConfiguration Parameter(PropertyPath propertyPath)
        {
            DebugCheck.NotNull(propertyPath);

            FunctionParameterConfiguration parameterConfiguration;
            if (!_parameterConfigurations.TryGetValue(propertyPath, out parameterConfiguration))
            {
                _parameterConfigurations.Add(
                    propertyPath, parameterConfiguration = new FunctionParameterConfiguration());
            }

            return parameterConfiguration;
        }

        public virtual void Configure(StorageModificationFunctionMapping modificationFunctionMapping)
        {
            DebugCheck.NotNull(modificationFunctionMapping);

            if (!string.IsNullOrWhiteSpace(_name))
            {
                modificationFunctionMapping.Function.Name = _name;
            }

            foreach (var keyValue in _parameterConfigurations)
            {
                var propertyPath = keyValue.Key;
                var parameterConfiguration = keyValue.Value;

                var parameterBinding
                    = modificationFunctionMapping
                        .ParameterBindings
                        .SingleOrDefault(
                            pb => propertyPath.Equals(
                                new PropertyPath(pb.MemberPath.Members.Select(m => m.GetClrPropertyInfo()))));

                if (parameterBinding == null)
                {
                    throw Error.ModificationFunctionParameterNotFound(
                        propertyPath,
                        modificationFunctionMapping.Function.Name);
                }

                parameterConfiguration.Configure(parameterBinding.Parameter);
            }
        }
    }
}
