// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;

    internal class ModificationFunctionConfiguration
    {
        internal sealed class ParameterKey
        {
            private readonly PropertyPath _propertyPath;
            private readonly bool _originalValue;

            public ParameterKey(PropertyPath propertyPath, bool originalValue)
            {
                DebugCheck.NotNull(propertyPath);

                _propertyPath = propertyPath;
                _originalValue = originalValue;
            }

            public PropertyPath PropertyPath
            {
                get { return _propertyPath; }
            }

            public bool IsOriginalValue
            {
                get { return _originalValue; }
            }

            private bool Equals(ParameterKey other)
            {
                return _propertyPath.Equals(other._propertyPath)
                       && _originalValue.Equals(other._originalValue);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                var parameterKey = obj as ParameterKey;

                return (parameterKey != null) && Equals(parameterKey);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (_propertyPath.GetHashCode() * 397) ^ _originalValue.GetHashCode();
                }
            }
        }

        private readonly Dictionary<ParameterKey, FunctionParameterConfiguration> _parameterConfigurations
            = new Dictionary<ParameterKey, FunctionParameterConfiguration>();

        private readonly Dictionary<PropertyInfo, string> _resultBindings
            = new Dictionary<PropertyInfo, string>();

        private string _name;
        private string _rowsAffectedParameter;

        public ModificationFunctionConfiguration()
        {
        }

        private ModificationFunctionConfiguration(ModificationFunctionConfiguration source)
        {
            DebugCheck.NotNull(source);

            _name = source._name;
            _rowsAffectedParameter = source._rowsAffectedParameter;

            source._parameterConfigurations.Each(
                c => _parameterConfigurations.Add(c.Key, c.Value.Clone()));

            source._resultBindings.Each(
                r => _resultBindings.Add(r.Key, r.Value));
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

        public void RowsAffectedParameter(string name)
        {
            DebugCheck.NotEmpty(name);

            _rowsAffectedParameter = name;
        }

        public string RowsAffectedParameterName
        {
            get { return _rowsAffectedParameter; }
        }

        public Dictionary<ParameterKey, FunctionParameterConfiguration> ParameterConfigurations
        {
            get { return _parameterConfigurations; }
        }

        public Dictionary<PropertyInfo, string> ResultBindings
        {
            get { return _resultBindings; }
        }

        public FunctionParameterConfiguration Parameter(PropertyPath propertyPath, bool originalValue = false)
        {
            DebugCheck.NotNull(propertyPath);

            var parameterKey = new ParameterKey(propertyPath, originalValue);

            FunctionParameterConfiguration parameterConfiguration;
            if (!_parameterConfigurations.TryGetValue(parameterKey, out parameterConfiguration))
            {
                _parameterConfigurations.Add(
                    parameterKey, parameterConfiguration = new FunctionParameterConfiguration());
            }

            return parameterConfiguration;
        }

        public void BindResult(PropertyPath propertyPath, string columnName)
        {
            DebugCheck.NotNull(propertyPath);
            DebugCheck.NotEmpty(columnName);

            _resultBindings[propertyPath.Single()] = columnName;
        }

        public virtual void Configure(StorageModificationFunctionMapping modificationFunctionMapping)
        {
            DebugCheck.NotNull(modificationFunctionMapping);

            ConfigureName(modificationFunctionMapping);
            ConfigureRowsAffectedParameter(modificationFunctionMapping);
            ConfigureParameters(modificationFunctionMapping);
            ConfigureResultBindings(modificationFunctionMapping);
        }

        private void ConfigureName(StorageModificationFunctionMapping modificationFunctionMapping)
        {
            if (!string.IsNullOrWhiteSpace(_name))
            {
                modificationFunctionMapping.Function.Name = _name;
            }
        }

        private void ConfigureRowsAffectedParameter(StorageModificationFunctionMapping modificationFunctionMapping)
        {
            if (!string.IsNullOrWhiteSpace(_rowsAffectedParameter))
            {
                if (modificationFunctionMapping.RowsAffectedParameter == null)
                {
                    throw Error.NoRowsAffectedParameter(modificationFunctionMapping.Function.Name);
                }

                modificationFunctionMapping.RowsAffectedParameter.Name = _rowsAffectedParameter;
            }
        }

        private void ConfigureParameters(StorageModificationFunctionMapping modificationFunctionMapping)
        {
            foreach (var keyValue in _parameterConfigurations)
            {
                var parameterKey = keyValue.Key;
                var parameterConfiguration = keyValue.Value;

                var parameterBindings
                    = modificationFunctionMapping
                        .ParameterBindings
                        .Where(
                            pb => parameterKey.PropertyPath.Equals(
                                new PropertyPath(
                                      pb.MemberPath.Members
                                        .OfType<EdmProperty>()
                                        .Select(m => m.GetClrPropertyInfo()))))
                        .ToList();

                var parameterBinding
                    = parameterBindings
                          .SingleOrDefault(pb => pb.IsCurrent != parameterKey.IsOriginalValue)
                      ?? parameterBindings
                             .SingleOrDefault(pb => !parameterKey.IsOriginalValue);

                if (parameterBinding == null)
                {
                    throw !parameterKey.IsOriginalValue
                              ? Error.ModificationFunctionParameterNotFound(
                                  parameterKey.PropertyPath,
                                  modificationFunctionMapping.Function.Name)
                              : Error.ModificationFunctionParameterNotFoundOriginal(
                                  parameterKey.PropertyPath,
                                  modificationFunctionMapping.Function.Name);
                }

                parameterConfiguration.Configure(parameterBinding.Parameter);
            }
        }

        private void ConfigureResultBindings(StorageModificationFunctionMapping modificationFunctionMapping)
        {
            foreach (var keyValue in _resultBindings)
            {
                var propertyInfo = keyValue.Key;
                var columnName = keyValue.Value;

                var resultBinding
                    = (modificationFunctionMapping
                           .ResultBindings ?? Enumerable.Empty<StorageModificationFunctionResultBinding>())
                        .SingleOrDefault(rb => propertyInfo.IsSameAs(rb.Property.GetClrPropertyInfo()));

                if (resultBinding == null)
                {
                    throw Error.ResultBindingNotFound(
                        propertyInfo.Name,
                        modificationFunctionMapping.Function.Name);
                }

                resultBinding.ColumnName = columnName;
            }
        }

        public bool IsCompatibleWith(ModificationFunctionConfiguration other)
        {
            if ((_name != null)
                && (other._name != null)
                && !string.Equals(_name, other._name, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return !_parameterConfigurations
                        .Join(
                            other._parameterConfigurations,
                            kv1 => kv1.Key,
                            kv2 => kv2.Key,
                            (kv1, kv2) => !Equals(kv1.Value, kv2.Value))
                        .Any(j => j);
        }
    }
}
