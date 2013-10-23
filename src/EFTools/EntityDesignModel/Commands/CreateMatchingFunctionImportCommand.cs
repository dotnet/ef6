// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using Microsoft.Data.Entity.Design.Model.Database;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Tools.XmlDesignerBase;
    using ComplexType = Microsoft.Data.Entity.Design.Model.Entity.ComplexType;

    /// <summary>
    ///     Use this command to create a FunctionImport in the C-Side representing a non-composable Function in
    ///     the S-Side given an IDataSchemaProcedure representing the information gathered about the underlying
    ///     sproc from the database.
    ///     We will also generate a FunctionImportMapping to map the C-side to the S-side.
    ///     Note: we assume the Function has already been created.
    /// </summary>
    internal class CreateMatchingFunctionImportCommand : Command
    {
        private FunctionImport _generatedFunctionImport; // output

        // _schemaProcedure is the IDataSchemaProcedure directly returned from IDataSchemaServer.GetProcedureOrFunction()
        // if it contains zero columns we will assume the return type is <None>
        // if it contains 1 column we will assume the return type is Collection(<primitive_type>)
        // if it returns 2 or more columns we will generate a new ComplexType and arrange for the return type to be that ComplexType
        private IRawDataSchemaProcedure _schemaProcedure;
        private readonly bool _shouldCreateComposableFunctionImport; // by default do not create FunctionImports for composable Functions

        /// <summary>
        ///     Returns the FunctionImport created by this command
        /// </summary>
        internal FunctionImport FunctionImport
        {
            get { return _generatedFunctionImport; }
        }

        internal string OverrideNameValue { get; set; }

        internal string OverrideEntitySetValue { get; set; }

        internal string OverrideReturnTypeValue { get; set; }

        internal CreateMatchingFunctionImportCommand(IDataSchemaProcedure schemaProcedure)
        {
            Initialize(schemaProcedure);
        }

        internal CreateMatchingFunctionImportCommand(IDataSchemaProcedure schemaProcedure, bool shouldCreateMatchingFunctionImport)
        {
            Initialize(schemaProcedure);
            _shouldCreateComposableFunctionImport = shouldCreateMatchingFunctionImport;
        }

        internal CreateMatchingFunctionImportCommand(
            IRawDataSchemaProcedure schemaProcedure, Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
            Initialize(schemaProcedure);
        }

        private void Initialize(IRawDataSchemaProcedure schemaProcedure)
        {
            _schemaProcedure = schemaProcedure;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "InvokeInternal")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "schemaProcedure")]
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            var artifact = cpc.EditingContext.GetEFArtifactService().Artifact;
            if (null == artifact)
            {
                Debug.Fail("null artifact not allowed");
                return;
            }

            // safety check, this should never be hit
            Debug.Assert(_schemaProcedure != null, "InvokeInternal is called when _schemaProcedure is null");
            if (null == _schemaProcedure)
            {
                throw new InvalidOperationException("InvokeInternal is called when _schemaProcedure is null.");
            }

            var cModel = artifact.ConceptualModel();
            if (null == cModel)
            {
                Debug.Fail("ConceptualEntityModel not allowed");
                return;
            }

            var cContainer = cModel.FirstEntityContainer as ConceptualEntityContainer;
            if (null == cContainer)
            {
                Debug.Fail("ConceptualEntityContainer not allowed");
                return;
            }

            var sModel = artifact.StorageModel();
            if (null == sModel)
            {
                Debug.Fail("null StorageEntityModel not allowed");
                return;
            }

            // determine matching Function
            var funcObj = DatabaseObject.CreateFromSchemaProcedure(_schemaProcedure);
            var function = ModelHelper.FindFunction(sModel, funcObj);
            if (null == function)
            {
                // in some error scenarios where the model has not been properly created we can be asked to create a FunctionImport for a Function which does not exist
                // if so just return without creating
                return;
            }

            // do not produce FunctionImports for composable Functions unless _shouldCreateComposableFunctionImport is true
            if (false == _shouldCreateComposableFunctionImport
                && function.IsComposable.Value)
            {
                return;
            }

            // determine FunctionImport name and make sure it is unique
            var functionImportName = OverrideNameValue;
            if (String.IsNullOrWhiteSpace(functionImportName))
            {
                if (null == function.LocalName
                    || string.IsNullOrEmpty(function.LocalName.Value))
                {
                    Debug.Fail("null or empty LocalName attribute for matching Function " + function);
                    return;
                }
                functionImportName = ModelHelper.GetUniqueName(typeof(FunctionImport), cContainer, function.LocalName.Value);
            }
            else
            {
#if DEBUG
                string errorMessage;
                var isUnique = ModelHelper.IsUniqueName(typeof(FunctionImport), cContainer, functionImportName, false, out errorMessage);
                Debug.Assert(isUnique, "If we gave CreateMatchingFunctionImportCommand a name, it should have been unique");
#endif
            }

            object returnType = null;
            ComplexType existingComplexTypeReturnType = null;
            if (OverrideReturnTypeValue == null)
            {
                // get return type of the Function
                returnType = ConstructReturnType(_schemaProcedure, cModel, sModel, functionImportName);
                if (null == returnType)
                {
                    Debug.Fail("cannot determine return type for schemaProcedure " + _schemaProcedure);
                    return;
                }
            }
            else
            {
                if (OverrideReturnTypeValue.Equals(ModelConstants.NoneValue, StringComparison.Ordinal))
                {
                    returnType = Resources.NoneDisplayValueUsedForUX;
                }
                else
                {
                    var rawValue = ModelHelper.UnwrapCollectionAroundFunctionImportReturnType(OverrideReturnTypeValue);

                    // Here we attempt to find the corresponding ReturnType for the given ReturnTypeOverride.
                    // The ReturnTypeOverride will be specified as the actual XAttribute value of the return type
                    if (OverrideEntitySetValue != null)
                    {
                        if (ModelHelper.FindEntitySet(cpc.Artifact.ConceptualModel(), OverrideEntitySetValue) != null)
                        {
                            // ReturnType is an EntityType
                            returnType = ModelHelper.FindEntityTypeViaSymbol(cpc.Artifact.ConceptualModel(), rawValue);
                        }
                    }
                    else if (!ModelHelper.AllPrimitiveTypes(artifact.SchemaVersion).Contains(rawValue))
                    {
                        // ReturnType is a ComplexType 
                        existingComplexTypeReturnType = ModelHelper.FindComplexType(cpc.Artifact.ConceptualModel(), rawValue);
                        returnType = existingComplexTypeReturnType;
                    }
                    else
                    {
                        returnType = rawValue;
                    }
                }
            }

            // Composable functions that do not return collections (e.g. scalar valued functions) are not supported
            // and should not be imported to the conceptual model
            if (Resources.NoneDisplayValueUsedForUX.Equals(returnType)
                && function.IsComposable.Value)
            {
                return;
            }

            // list of commands to be executed
            IList<Command> commands = new List<Command>();

            // if return type is the name of a ComplexType then create a new matching ComplexType
            CreateComplexTypeCommand createComplexTypeCommand = null;
            if (OverrideReturnTypeValue == null
                && returnType is string
                && false == Resources.NoneDisplayValueUsedForUX.Equals(returnType))
            {
                createComplexTypeCommand = AddCreateComplexTypeCommands(sModel, returnType as string, _schemaProcedure.RawColumns, commands);
            }

            // if we created a ComplexType above then pass that as a pre-req to the CreateFunctionImport command,
            // otherwise just create the FunctionImport without the pre-req
            CreateFunctionImportCommand cmdFuncImp;
            if (createComplexTypeCommand == null)
            {
                if (returnType is EdmType)
                {
                    // For the case where the FunctionImport should have a return type which is not a Complex Type but
                    // simply a C-side primitive type we have to pass the _name_ of the C-side primitive type to
                    // CreateFunctionImportCommand, rather than the type itself
                    returnType = (returnType as EdmType).Name;
                }
                cmdFuncImp = new CreateFunctionImportCommand(cContainer, function, functionImportName, returnType);
            }
            else
            {
                cmdFuncImp = new CreateFunctionImportCommand(cContainer, function, functionImportName, createComplexTypeCommand);
            }

            commands.Add(cmdFuncImp);

            // now create the FunctionImportMapping to map the S-side Function to the C-side FunctionImport
            if (null != artifact.MappingModel()
                && null != artifact.MappingModel().FirstEntityContainerMapping)
            {
                var cmdFuncImpMapping = new CreateFunctionImportMappingCommand(
                    artifact.MappingModel().FirstEntityContainerMapping, function, cmdFuncImp.Id);
                cmdFuncImpMapping.AddPreReqCommand(cmdFuncImp);
                commands.Add(cmdFuncImpMapping);

                IDictionary<string, string> mapPropertyNameToColumnName = null;
                if (_schemaProcedure != null)
                {
                    mapPropertyNameToColumnName =
                        ModelHelper.ConstructComplexTypePropertyNameToColumnNameMapping(
                            _schemaProcedure.RawColumns.Select(c => c.Name).ToList());
                }

                // Create explicit function-import result type mapping if the return type is a complex type.
                if (createComplexTypeCommand != null)
                {
                    commands.Add(
                        new CreateFunctionImportTypeMappingCommand(cmdFuncImpMapping, createComplexTypeCommand)
                            {
                                CreateDefaultScalarProperties = true,
                                PropertyNameToColumnNameMap = mapPropertyNameToColumnName
                            });
                }
                else if (OverrideReturnTypeValue != null
                         && existingComplexTypeReturnType != null)
                {
                    commands.Add(
                        new CreateFunctionImportTypeMappingCommand(cmdFuncImpMapping, existingComplexTypeReturnType)
                            {
                                CreateDefaultScalarProperties = true,
                                PropertyNameToColumnNameMap = mapPropertyNameToColumnName
                            });
                }
            }

            // now invoke the list of commands
            if (null != commands)
            {
                var cp = new CommandProcessor(cpc, commands);
                cp.Invoke();

                // assign the generated FunctionImport so this command can be used as input for others
                _generatedFunctionImport = cmdFuncImp.FunctionImport;
            }
        }

        /// <summary>
        ///     Construct a return type to be used for the FunctionImport
        /// </summary>
        private static object ConstructReturnType(
            IRawDataSchemaProcedure schemaProcedure, ConceptualEntityModel cModel,
            StorageEntityModel sModel, string functionImportResultBaseName)
        {
            if (null == schemaProcedure)
            {
                Debug.Fail("null SchemaProcedure not allowed");
                return null;
            }
            else
            {
                var colCount = schemaProcedure.RawColumns.Count;
                if (0 == colCount)
                {
                    // zero columns is equivalent to no return type
                    return Resources.NoneDisplayValueUsedForUX;
                }
                else if (1 == colCount)
                {
                    // if 1 columns return a collection of scalars
                    var col = schemaProcedure.RawColumns[0];
                    var primType = ModelHelper.GetPrimitiveType(sModel, col.NativeDataType, col.ProviderDataType);
                    if (null == primType)
                    {
                        Debug.Fail(
                            "Could not find primitive type for column with NativeDataType = " + col.NativeDataType + ", ProviderDataType = "
                            + col.ProviderDataType);
                        return null;
                    }
                    else
                    {
                        return primType.GetEdmPrimitiveType();
                    }
                }
                else
                {
                    // if more than 1 column return a new ComplexType name (this will cause a new ComplexType with that name to be created)
                    var proposedName = String.Format(CultureInfo.CurrentCulture, "{0}_Result", functionImportResultBaseName);
                    var complexTypeName = ModelHelper.GetUniqueName(typeof(ComplexType), cModel, proposedName);
                    return complexTypeName;
                }
            }
        }

        /// <summary>
        ///     Add a list of commands to create a ComplexType matching the list of columns passed in.
        ///     Return the initial CreateComplexTypeCommand.
        /// </summary>
        /// <param name="sModel"></param>
        /// <param name="complexTypeName"></param>
        /// <param name="columns"></param>
        /// <param name="commands"></param>
        internal static CreateComplexTypeCommand AddCreateComplexTypeCommands(
            StorageEntityModel sModel, string complexTypeName, IList<IRawDataSchemaColumn> columns, IList<Command> commands)
        {
            Debug.Assert(false == String.IsNullOrEmpty(complexTypeName), "The passed in complexTypeName is null or empty");
            Debug.Assert(null != columns, "The passed in schema columns are null");
            Debug.Assert(null != commands, "We require a pre-created list of commands to add to");

            CreateComplexTypeCommand cmdNewComplexType = null;
            if (!string.IsNullOrEmpty(complexTypeName)
                && null != columns
                && null != commands)
            {
                cmdNewComplexType = new CreateComplexTypeCommand(complexTypeName, true);
                commands.Add(cmdNewComplexType);

                // now add commands to add appropriate properties to the ComplexType
                foreach (var column in columns)
                {
                    // adds commands to create and set the facets for a new complex type property to match the column
                    AddCreateComplexTypePropertyCommands(sModel, column, cmdNewComplexType, null, commands);
                }
            }

            return cmdNewComplexType;
        }

        /// <summary>
        ///     Add commands to create and set the facets for a new complex type property to match the column.
        /// </summary>
        internal static void AddCreateComplexTypePropertyCommands(
            StorageEntityModel storageModel, IRawDataSchemaColumn column, CreateComplexTypeCommand cmdNewComplexType,
            ComplexType complexType, IList<Command> commands)
        {
            // Assert if both cmdNewComplexType and complexType are null or if both are not null.
            Debug.Assert(
                ((cmdNewComplexType != null && complexType == null) || (cmdNewComplexType == null && complexType != null)),
                "Both cmdNewComplexType and complexType are null or both are not null. cmdNewComplexType is null : "
                + (cmdNewComplexType == null).ToString() + ", complexType is null : " + (complexType == null).ToString());

            if ((cmdNewComplexType != null && complexType == null)
                || (cmdNewComplexType == null && complexType != null))
            {
                // Skip creating the complex type property for a column if the column type is unknown or not supported ( providerDataType == -1 ).
                if (column.ProviderDataType != -1)
                {
                    var primitiveType = ModelHelper.GetPrimitiveType(storageModel, column.NativeDataType, column.ProviderDataType);
                    // We only create complex type property if primitive type is known.
                    if (primitiveType != null)
                    {
                        CreateComplexTypePropertyCommand cmdNewComplexTypeProperty = null;
                        // if complex type is not created yet.
                        if (cmdNewComplexType != null)
                        {
                            cmdNewComplexTypeProperty = new CreateComplexTypePropertyCommand(
                                // Automatically "fix" the property Name if it contains bad character. 
                                // We need to do this since we don't let the user to change the property name from the Function Import dialog.
                                ModelHelper.CreateValidSimpleIdentifier(column.Name),
                                cmdNewComplexType,
                                primitiveType.GetEdmPrimitiveType().Name,
                                column.IsNullable);
                            commands.Add(cmdNewComplexTypeProperty);
                        }
                        else
                        {
                            cmdNewComplexTypeProperty = new CreateComplexTypePropertyCommand(
                                // Automatically "fix" the property Name if it contains bad character. 
                                // We need to do this since we don't let the user to change the property name from the Function Import dialog.
                                ModelHelper.CreateValidSimpleIdentifier(column.Name),
                                complexType,
                                primitiveType.GetEdmPrimitiveType().Name,
                                column.IsNullable);
                            commands.Add(cmdNewComplexTypeProperty);
                        }

                        // We only update the facets that are displayed in Function Import dialog return type view list:
                        // - Nullable.
                        // - Max Size.
                        // - Precision.
                        // - Scale.
                        var cmdSetPropertyFacets = new SetPropertyFacetsCommand(
                            cmdNewComplexTypeProperty
                            , null // Default value
                            , ModelHelper.GetMaxLengthFacetValue(column.Size)
                            , null // Fixed Length
                            , DefaultableValueUIntOrNone.GetFromNullableUInt(column.Precision)
                            , DefaultableValueUIntOrNone.GetFromNullableUInt(column.Scale)
                            , null // unicode
                            , null // collation
                            , null // concurrency mode 
                            );
                        commands.Add(cmdSetPropertyFacets);
                    }
                }
            }
        }

        /// <summary>
        ///     This method will queue commands to sync complex type properties and the schema columns.
        ///     Add the commands which change the complex type properties.
        ///     - Iterate complex type properties list.
        ///     - If no column with the same name found, add DeletePropertyCommands.
        ///     - If match is found, update the properties types and facets by queueing ChangePropertyTypeCommand and SetPropertyFacetsCommand
        ///     - Iterate schema column properties list and add the column that have not been added.
        /// </summary>
        /// <param name="complexType">The complex type that will be updated.</param>
        /// <param name="complexTypePropertiesMap">The complex type properties map.</param>
        /// <param name="columns">The DataSchema columns that are used as the source for the update.</param>
        /// <param name="commands">The list of commands to which these commands will be added</param>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        internal static void AddChangeComplexTypePropertiesCommands(
            ComplexType complexType, IDictionary<string, Property> complexTypePropertiesMap, IList<IRawDataSchemaColumn> columns,
            IList<Command> commands)
        {
            Debug.Assert(complexTypePropertiesMap != null, "Parameter complexTypePropertiesMap is null");
            if (complexTypePropertiesMap != null)
            {
                var columnsDictionary = columns.ToDictionary(c => c.Name);

                var createdProperties = new HashSet<string>();
                var storageModel = complexType.Artifact.StorageModel();

                // Iterate current properties decide whether to delete, create, update or skip.
                foreach (var propertyName in complexTypePropertiesMap.Keys)
                {
                    // If the column is not found in columns dictionary delete it.
                    if (!columnsDictionary.ContainsKey(propertyName))
                    {
                        commands.Add(complexTypePropertiesMap[propertyName].GetDeleteCommand());
                    }
                        // Match is found between schema column and complex type property.
                    else
                    {
                        var complexTypeProperty = complexTypePropertiesMap[propertyName];
                        var schemaColumn = columnsDictionary[propertyName];
                        // Special case if the property is a complex property, we just need to delete it.
                        if (complexTypeProperty is ComplexConceptualProperty)
                        {
                            // Delete the complex property 
                            commands.Add(complexTypeProperty.GetDeleteCommand());
                            // Create a new complex property
                            AddCreateComplexTypePropertyCommands(storageModel, schemaColumn, null, complexType, commands);
                            // Add the property to the "created-list" so we don't create the property in the second pass.
                            createdProperties.Add(propertyName);
                        }
                            // If ProviderDataType == -1 (Unsupported type) we should skip the sync operation
                        else if (schemaColumn.ProviderDataType != -1)
                        {
                            // Update the ComplexType's property to look like the schemaColumn
                            var primitiveType = ModelHelper.GetPrimitiveType(
                                storageModel, schemaColumn.NativeDataType, schemaColumn.ProviderDataType);
                            // PrimitiveType is null if there is no compatible EF type for the schema column type.
                            if (primitiveType != null)
                            {
                                // sync the property type
                                commands.Add(new ChangePropertyTypeCommand(complexTypeProperty, primitiveType.GetEdmPrimitiveType().Name));
                                // sync the property facets.
                                // We only update the facets that are displayed in Function Import dialog return type view list:
                                // - Nullable.
                                // - Max Size.
                                // - Precision.
                                // - Scale.
                                commands.Add(new ChangePropertyTypeNullableCommand(complexTypeProperty, schemaColumn.IsNullable));
                                commands.Add(
                                    new SetPropertyFacetsCommand(
                                        complexTypeProperty
                                        , null // Default value
                                        , ModelHelper.GetMaxLengthFacetValue(schemaColumn.Size) // Size or MaxLength
                                        , null // Fixed Length
                                        , DefaultableValueUIntOrNone.GetFromNullableUInt(schemaColumn.Precision)
                                        , DefaultableValueUIntOrNone.GetFromNullableUInt(schemaColumn.Scale)
                                        , null // unicode
                                        , null // collation
                                        , null // concurrency mode 
                                        ));
                            }
                        }
                    }
                }

                // Second pass: Iterate columns list to add columns that are not in complex type properties
                foreach (var columnName in columnsDictionary.Keys)
                {
                    if (false == complexTypePropertiesMap.ContainsKey(columnName)
                        && false == createdProperties.Contains(columnName))
                    {
                        AddCreateComplexTypePropertyCommands(storageModel, columnsDictionary[columnName], null, complexType, commands);
                    }
                }
            }
        }
    }
}
