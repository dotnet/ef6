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
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    internal class ModificationFunctionConfiguration
    {
        private readonly Dictionary<PropertyPath, Tuple<string, string>> _parameterNames
            = new Dictionary<PropertyPath, Tuple<string, string>>();

        private readonly Dictionary<PropertyInfo, string> _resultBindings
            = new Dictionary<PropertyInfo, string>();

        private string _name;
        private string _schema;
        private string _rowsAffectedParameter;

        private List<FunctionParameter> _configuredParameters;

        public ModificationFunctionConfiguration()
        {
        }

        private ModificationFunctionConfiguration(ModificationFunctionConfiguration source)
        {
            DebugCheck.NotNull(source);

            _name = source._name;
            _schema = source._schema;
            _rowsAffectedParameter = source._rowsAffectedParameter;

            source._parameterNames.Each(
                c => _parameterNames.Add(c.Key, Tuple.Create(c.Value.Item1, c.Value.Item2)));

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

            var databaseName = DatabaseName.Parse(name);

            _name = databaseName.Name;
            _schema = databaseName.Schema;
        }

        public void HasName(string name, string schema)
        {
            DebugCheck.NotEmpty(name);
            DebugCheck.NotEmpty(schema);

            _name = name;
            _schema = schema;
        }

        public string Name
        {
            get { return _name; }
        }

        public string Schema
        {
            get { return _schema; }
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

        public Dictionary<PropertyPath, Tuple<string, string>> ParameterNames
        {
            get { return _parameterNames; }
        }

        public Dictionary<PropertyInfo, string> ResultBindings
        {
            get { return _resultBindings; }
        }

        public void Parameter(
            PropertyPath propertyPath,
            string parameterName,
            string originalValueParameterName = null)
        {
            DebugCheck.NotNull(propertyPath);
            DebugCheck.NotEmpty(parameterName);

            _parameterNames[propertyPath]
                = Tuple.Create(parameterName, originalValueParameterName);
        }

        public void Result(PropertyPath propertyPath, string columnName)
        {
            DebugCheck.NotNull(propertyPath);
            DebugCheck.NotEmpty(columnName);

            _resultBindings[propertyPath.Single()] = columnName;
        }

        public virtual void Configure(StorageModificationFunctionMapping modificationFunctionMapping)
        {
            DebugCheck.NotNull(modificationFunctionMapping);

            _configuredParameters = new List<FunctionParameter>();

            ConfigureName(modificationFunctionMapping);
            ConfigureSchema(modificationFunctionMapping);
            ConfigureRowsAffectedParameter(modificationFunctionMapping);
            ConfigureParameters(modificationFunctionMapping);
            ConfigureResultBindings(modificationFunctionMapping);
        }

        private void ConfigureName(StorageModificationFunctionMapping modificationFunctionMapping)
        {
            DebugCheck.NotNull(modificationFunctionMapping);

            if (!string.IsNullOrWhiteSpace(_name))
            {
                modificationFunctionMapping.Function.StoreFunctionNameAttribute = _name;
            }
        }

        private void ConfigureSchema(StorageModificationFunctionMapping modificationFunctionMapping)
        {
            DebugCheck.NotNull(modificationFunctionMapping);

            if (!string.IsNullOrWhiteSpace(_schema))
            {
                modificationFunctionMapping.Function.Schema = _schema;
            }
        }

        private void ConfigureRowsAffectedParameter(StorageModificationFunctionMapping modificationFunctionMapping)
        {
            DebugCheck.NotNull(modificationFunctionMapping);

            if (!string.IsNullOrWhiteSpace(_rowsAffectedParameter))
            {
                if (modificationFunctionMapping.RowsAffectedParameter == null)
                {
                    throw Error.NoRowsAffectedParameter(modificationFunctionMapping.Function.FunctionName);
                }

                modificationFunctionMapping.RowsAffectedParameter.Name = _rowsAffectedParameter;

                _configuredParameters.Add(modificationFunctionMapping.RowsAffectedParameter);
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void ConfigureParameters(StorageModificationFunctionMapping modificationFunctionMapping)
        {
            foreach (var keyValue in _parameterNames)
            {
                var propertyPath = keyValue.Key;
                var parameterName = keyValue.Value.Item1;
                var originalValueParameterName = keyValue.Value.Item2;

                var parameterBindings
                    = modificationFunctionMapping
                        .ParameterBindings
                        .Where(
                            pb => // First, try and match scalar/complex/many-to-many binding 
                            (((pb.MemberPath.AssociationSetEnd == null)
                              || pb.MemberPath.AssociationSetEnd.ParentAssociationSet.ElementType.IsManyToMany())
                             && propertyPath.Equals(
                                 new PropertyPath(
                                    pb.MemberPath.Members.OfType<EdmProperty>().Select(m => m.GetClrPropertyInfo()))))
                            ||
                            // Otherwise, try and match IA FK bindings 
                            ((propertyPath.Count == 2)
                             && (pb.MemberPath.AssociationSetEnd != null)
                             && pb.MemberPath.Members.First().GetClrPropertyInfo().IsSameAs(propertyPath.Last())
                             && pb.MemberPath.AssociationSetEnd.ParentAssociationSet.AssociationSetEnds
                                    .Select(ae => ae.CorrespondingAssociationEndMember.GetClrPropertyInfo())
                                    .Where(pi => pi != null)
                                    .Any(pi => pi.IsSameAs(propertyPath.First()))))
                        .ToList();

                if (parameterBindings.Count == 1)
                {
                    var parameterBinding = parameterBindings.Single();

                    if (!string.IsNullOrWhiteSpace(originalValueParameterName))
                    {
                        if (parameterBinding.IsCurrent)
                        {
                            throw Error.ModificationFunctionParameterNotFoundOriginal(
                                propertyPath,
                                modificationFunctionMapping.Function.FunctionName);
                        }
                    }

                    parameterBinding.Parameter.Name = parameterName;

                    _configuredParameters.Add(parameterBinding.Parameter);
                }
                else if (parameterBindings.Count == 2)
                {
                    var parameterBinding = parameterBindings.Single(pb => pb.IsCurrent);

                    parameterBinding.Parameter.Name = parameterName;

                    _configuredParameters.Add(parameterBinding.Parameter);

                    if (!string.IsNullOrWhiteSpace(originalValueParameterName))
                    {
                        parameterBinding = parameterBindings.Single(pb => !pb.IsCurrent);

                        parameterBinding.Parameter.Name = originalValueParameterName;

                        _configuredParameters.Add(parameterBinding.Parameter);
                    }
                }
                else
                {
                    throw Error.ModificationFunctionParameterNotFound(
                        propertyPath,
                        modificationFunctionMapping.Function.FunctionName);
                }
            }

            var unconfiguredParameters
                = modificationFunctionMapping
                    .Function
                    .Parameters
                    .Except(_configuredParameters);

            foreach (var parameter in unconfiguredParameters)
            {
                parameter.Name
                    = modificationFunctionMapping
                        .Function
                        .Parameters
                        .Except(new[] { parameter })
                        .UniquifyName(parameter.Name);
            }
        }

        private void ConfigureResultBindings(StorageModificationFunctionMapping modificationFunctionMapping)
        {
            DebugCheck.NotNull(modificationFunctionMapping);

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
                        modificationFunctionMapping.Function.FunctionName);
                }

                resultBinding.ColumnName = columnName;
            }
        }

        public bool IsCompatibleWith(ModificationFunctionConfiguration other)
        {
            DebugCheck.NotNull(other);

            if ((_name != null)
                && (other._name != null)
                && !string.Equals(_name, other._name, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if ((_schema != null)
                && (other._schema != null)
                && !string.Equals(_schema, other._schema, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return !_parameterNames
                        .Join(
                            other._parameterNames,
                            kv1 => kv1.Key,
                            kv2 => kv2.Key,
                            (kv1, kv2) => !Equals(kv1.Value, kv2.Value))
                        .Any(j => j);
        }

        public void Merge(ModificationFunctionConfiguration modificationFunctionConfiguration, bool allowOverride)
        {
            DebugCheck.NotNull(modificationFunctionConfiguration);

            if (allowOverride || string.IsNullOrWhiteSpace(_name))
            {
                _name = modificationFunctionConfiguration.Name ?? _name;
            }

            if (allowOverride || string.IsNullOrWhiteSpace(_schema))
            {
                _schema = modificationFunctionConfiguration.Schema ?? _schema;
            }

            if (allowOverride || string.IsNullOrWhiteSpace(_rowsAffectedParameter))
            {
                _rowsAffectedParameter 
                    = modificationFunctionConfiguration.RowsAffectedParameterName ?? _rowsAffectedParameter;
            }

            foreach (var parameterName in modificationFunctionConfiguration.ParameterNames
                .Where(parameterName => allowOverride || !_parameterNames.ContainsKey(parameterName.Key)))
            {
                _parameterNames[parameterName.Key] = parameterName.Value;
            }

            foreach (var resultBinding in modificationFunctionConfiguration.ResultBindings
                .Where(resultBinding => allowOverride || !_resultBindings.ContainsKey(resultBinding.Key)))
            {
                _resultBindings[resultBinding.Key] = resultBinding.Value;
            }
        }
    }
}
