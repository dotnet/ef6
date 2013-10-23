// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.DatabaseGeneration.OutputGenerators
{
    using System;
    using System.Activities;
    using System.Activities.Hosting;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.DatabaseGeneration.Properties;
    using Microsoft.Data.Entity.Design.VersioningFacade;

    /// <summary>
    ///     Generates mapping specification language (MSL) based on the provided conceptual schema definition language (CSDL).
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Msl")]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Csdl")]
    public class CsdlToMsl : IGenerateActivityOutput
    {
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields",
            Justification = "Changing would require changes to GenerateActivityOutput public API")]
        private OutputGeneratorActivity _activity;
        private static string _mslUri;
        private static XNamespace _msl;

        #region Test code only

        internal static string MslNamespace
        {
            set
            {
                _mslUri = value;
                _msl = _mslUri;
            }
        }

        #endregion Test code only

        #region IGenerateActivityOutput Members

        // TODO perhaps build an in-memory "inference" model that keeps track of the assumptions we make (association/entity type names, etc.)
        /// <summary>
        ///     Generates mapping specification language (MSL) based on the provided conceptual schema definition language (CSDL).
        /// </summary>
        /// <typeparam name="T"> The type of the activity output. </typeparam>
        /// <param name="owningActivity"> The currently executing activity. </param>
        /// <param name="context"> The activity context that contains the state of the workflow. </param>
        /// <param name="inputs"> Contains the incoming CSDL. </param>
        /// <returns> Mapping specification language (MSL) of type T based on the provided conceptual schema definition language (CSDL). </returns>
        public T GenerateActivityOutput<T>(
            OutputGeneratorActivity owningActivity, NativeActivityContext context, IDictionary<string, object> inputs) where T : class
        {
            _activity = owningActivity;

            object o;
            inputs.TryGetValue(EdmConstants.csdlInputName, out o);
            var edmItemCollection = o as EdmItemCollection;
            if (edmItemCollection == null)
            {
                throw new InvalidOperationException(Resources.ErrorCouldNotFindCSDL);
            }

            var symbolResolver = context.GetExtension<SymbolResolver>();
            var edmParameterBag = symbolResolver[typeof(EdmParameterBag).Name] as EdmParameterBag;
            if (edmParameterBag == null)
            {
                throw new InvalidOperationException(Resources.ErrorNoEdmParameterBag);
            }

            // Find the TargetVersion parameter
            var targetFrameworkVersion = edmParameterBag.GetParameter<Version>(EdmParameterBag.ParameterName.TargetVersion);
            if (targetFrameworkVersion == null)
            {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture, Resources.ErrorNoParameterDefined, EdmParameterBag.ParameterName.TargetVersion));
            }

            // Find the MSL namespace parameter
            _mslUri = SchemaManager.GetMSLNamespaceName(targetFrameworkVersion);
            _msl = _mslUri;

            var csdlNamespace = edmItemCollection.GetNamespace();

            var mappingElement = ConstructMappingElement();
            var entityContainerMappingElement = ConstructEntityContainerMapping(edmItemCollection, csdlNamespace);

            entityContainerMappingElement.Add(ConstructEntitySetMappings(edmItemCollection, csdlNamespace));
            entityContainerMappingElement.Add(ConstructAssociationSetMappings(edmItemCollection, csdlNamespace, targetFrameworkVersion));

            mappingElement.Add(entityContainerMappingElement);

            var serializedMappingElement = String.Empty;
            try
            {
                serializedMappingElement = EdmExtension.SerializeXElement(mappingElement);
            }
            catch (Exception e)
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.ErrorSerializing_CsdlToMsl, e.Message), e);
            }
            return serializedMappingElement as T;
        }

        #endregion

        internal static XElement ConstructMappingElement()
        {
            return new XElement(_msl + "Mapping", new XAttribute("Space", "C-S"), new XAttribute("xmlns", _mslUri));
        }

        internal static XElement ConstructEntityContainerMapping(EdmItemCollection edm, string csdlNamespace)
        {
            var entityContainerMappingElement = new XElement(
                _msl + "EntityContainerMapping",
                new XAttribute("StorageEntityContainer", OutputGeneratorHelpers.ConstructStorageEntityContainerName(csdlNamespace)),
                new XAttribute("CdmEntityContainer", edm.GetEntityContainerName()));

            return entityContainerMappingElement;
        }

        internal static List<XElement> ConstructAssociationSetMappings(
            EdmItemCollection edm, string csdlNamespace, Version targetFrameworkVersion)
        {
            var associationSetMappings = new List<XElement>();
            foreach (var associationSet in edm.GetAllAssociationSets())
            {
                var association = associationSet.ElementType;

                if (association.IsManyToMany())
                {
                    var entityType1 = association.GetEnd1().GetEntityType();
                    var entityType2 = association.GetEnd2().GetEntityType();

                    var associationSetMapping = new XElement(
                        _msl + "AssociationSetMapping",
                        new XAttribute("Name", associationSet.Name),
                        new XAttribute("TypeName", csdlNamespace + "." + associationSet.ElementType.Name),
                        new XAttribute("StoreEntitySet", associationSet.Name));
                    var end1Property = new XElement(_msl + "EndProperty", new XAttribute("Name", association.GetEnd1().Name));
                    foreach (var property in entityType1.GetKeyProperties())
                    {
                        end1Property.Add(
                            new XElement(
                                _msl + "ScalarProperty", new XAttribute("Name", property.Name),
                                new XAttribute(
                                    "ColumnName", OutputGeneratorHelpers.GetFkName(association, association.GetEnd2(), property.Name))));
                    }
                    associationSetMapping.Add(end1Property);
                    var end2Property = new XElement(_msl + "EndProperty", new XAttribute("Name", association.GetEnd2().Name));
                    foreach (var property in entityType2.GetKeyProperties())
                    {
                        end2Property.Add(
                            new XElement(
                                _msl + "ScalarProperty", new XAttribute("Name", property.Name),
                                new XAttribute(
                                    "ColumnName", OutputGeneratorHelpers.GetFkName(association, association.GetEnd1(), property.Name))));
                    }
                    associationSetMapping.Add(end2Property);
                    associationSetMappings.Add(associationSetMapping);
                }
                else
                {
                    if (targetFrameworkVersion == EntityFrameworkVersion.Version1
                        || association.ReferentialConstraints.Count <= 0)
                    {
                        var dependentEnd = association.GetDependentEnd();
                        var principalEnd = association.GetOtherEnd(dependentEnd);
                        if (dependentEnd != null
                            && principalEnd != null)
                        {
                            var dependentEntityType = dependentEnd.GetEntityType();
                            var principalEntityType = principalEnd.GetEntityType();
                            var associationSetMapping = new XElement(
                                _msl + "AssociationSetMapping",
                                new XAttribute("Name", associationSet.Name),
                                new XAttribute("TypeName", csdlNamespace + "." + associationSet.ElementType.Name),
                                new XAttribute("StoreEntitySet", OutputGeneratorHelpers.GetStorageEntityTypeName(dependentEntityType, edm)));
                            var end1Property = new XElement(_msl + "EndProperty", new XAttribute("Name", principalEnd.Name));
                            foreach (var property in principalEntityType.GetKeyProperties())
                            {
                                var columnName = (association.ReferentialConstraints.Count > 0)
                                                     ? property.GetDependentProperty(association.ReferentialConstraints.FirstOrDefault())
                                                           .Name
                                                     : OutputGeneratorHelpers.GetFkName(association, dependentEnd, property.Name);
                                end1Property.Add(
                                    new XElement(
                                        _msl + "ScalarProperty", new XAttribute("Name", property.Name),
                                        new XAttribute("ColumnName", columnName)));
                            }
                            associationSetMapping.Add(end1Property);
                            var end2Property = new XElement(_msl + "EndProperty", new XAttribute("Name", dependentEnd.Name));
                            foreach (var property in dependentEntityType.GetKeyProperties())
                            {
                                end2Property.Add(
                                    new XElement(
                                        _msl + "ScalarProperty", new XAttribute("Name", property.Name),
                                        new XAttribute("ColumnName", property.Name)));
                            }
                            associationSetMapping.Add(end2Property);

                            // All 0..1 ends of a non PK:PK association require an IsNull=false condition
                            if (dependentEnd.RelationshipMultiplicity == RelationshipMultiplicity.ZeroOrOne
                                &&
                                !association.IsPKToPK())
                            {
                                foreach (var key in dependentEntityType.GetKeyProperties())
                                {
                                    associationSetMapping.Add(
                                        new XElement(
                                            _msl + "Condition", new XAttribute("ColumnName", key.Name), new XAttribute("IsNull", "false")));
                                }
                            }

                            if (principalEnd.RelationshipMultiplicity == RelationshipMultiplicity.ZeroOrOne)
                            {
                                foreach (var key in principalEntityType.GetKeyProperties())
                                {
                                    associationSetMapping.Add(
                                        new XElement(
                                            _msl + "Condition",
                                            new XAttribute(
                                                "ColumnName", OutputGeneratorHelpers.GetFkName(association, dependentEnd, key.Name)),
                                            new XAttribute("IsNull", "false")));
                                }
                            }
                            associationSetMappings.Add(associationSetMapping);
                        }
                    }
                }
            }
            return associationSetMappings;
        }

        internal static List<XElement> ConstructEntitySetMappings(EdmItemCollection edm, string csdlNamespace)
        {
            var entitySetMappingElements = new List<XElement>();
            foreach (var set in edm.GetAllEntitySets())
            {
                var entitySetMapping = new XElement(_msl + "EntitySetMapping", new XAttribute("Name", set.Name));
                foreach (var type in set.GetContainingTypes(edm))
                {
                    var entityTypeMapping = new XElement(
                        _msl + "EntityTypeMapping", new XAttribute("TypeName", "IsTypeOf(" + type.FullName + ")"));
                    var mappingFragment = new XElement(
                        _msl + "MappingFragment",
                        new XAttribute("StoreEntitySet", OutputGeneratorHelpers.GetStorageEntityTypeName(type, edm)));
                    foreach (var property in type.GetRootOrSelf().GetKeyProperties())
                    {
                        mappingFragment.Add(
                            new XElement(
                                _msl + "ScalarProperty", new XAttribute("Name", property.Name), new XAttribute("ColumnName", property.Name)));
                    }
                    foreach (var property in type.Properties.Except(type.GetKeyProperties()).Where(p => p.DeclaringType == type))
                    {
                        if (property.IsComplexProperty())
                        {
                            mappingFragment.Add(ConstructComplexProperty(property, property.Name, csdlNamespace));
                        }
                        else
                        {
                            mappingFragment.Add(
                                new XElement(
                                    _msl + "ScalarProperty", new XAttribute("Name", property.Name),
                                    new XAttribute("ColumnName", property.Name)));
                        }
                    }
                    entityTypeMapping.Add(mappingFragment);
                    entitySetMapping.Add(entityTypeMapping);
                }
                entitySetMappingElements.Add(entitySetMapping);
            }
            return entitySetMappingElements;
        }

        private static XElement ConstructComplexProperty(EdmProperty complexProperty, string columnNamePrefix, string csdlNamespace)
        {
            // don't add anything if the complex type associated with this property is empty.
            if (complexProperty == null
                || complexProperty.TypeUsage == null
                || !(complexProperty.TypeUsage.EdmType is ComplexType))
            {
                Debug.Fail("We should not have called ConstructComplexProperty on a property that is not a complex property");
                return null;
            }

            var complexType = complexProperty.TypeUsage.EdmType as ComplexType;
            if (complexType != null)
            {
                if (complexType.Properties.Count == 0)
                {
                    return null;
                }
            }

            var complexPropertyElement = new XElement(
                _msl + "ComplexProperty", new XAttribute("Name", complexProperty.Name),
                new XAttribute("TypeName", csdlNamespace + "." + complexProperty.TypeUsage.EdmType.Name));

            complexProperty.VisitComplexProperty(
                (namePrefix, nestedProperty) =>
                    {
                        if (nestedProperty.IsComplexProperty())
                        {
                            complexPropertyElement.Add(
                                ConstructComplexProperty(nestedProperty, columnNamePrefix + "_" + nestedProperty.Name, csdlNamespace));
                        }
                        else
                        {
                            complexPropertyElement.Add(
                                new XElement(
                                    _msl + "ScalarProperty", new XAttribute("Name", nestedProperty.Name),
                                    new XAttribute("ColumnName", columnNamePrefix + "_" + nestedProperty.Name)));
                        }
                    }, "_", false);

            if (complexPropertyElement.Nodes().Any())
            {
                return complexPropertyElement;
            }
            return null;
        }
    }
}
