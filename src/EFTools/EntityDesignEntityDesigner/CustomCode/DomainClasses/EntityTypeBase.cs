// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using DslResources = Microsoft.Data.Entity.Design.EntityDesigner.Properties.Resources;
using Model = Microsoft.Data.Entity.Design.Model.Entity;

namespace Microsoft.Data.Entity.Design.EntityDesigner.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Model.Entity;

    abstract partial class EntityTypeBase
    {
        public string EntitySetName
        {
            get
            {
                if (EntityDesignerViewModel != null)
                {
                    var modelEntityType = EntityDesignerViewModel.ModelXRef.GetExisting(this) as Model.Entity.EntityType;
                    if (modelEntityType != null)
                    {
                        var entitySet = modelEntityType.EntitySet;
                        if (entitySet != null)
                        {
                            return entitySet.DisplayName;
                        }
                    }
                }

                return "";
            }
        }

        public string GetBaseTypeNameValue()
        {
            // We need to look at the model to get the base-type name; the view-model might not have it.
            // In multiple diagram scenario, the base entity-type might not exist in the current diagram.
            var modelEntityType = EntityDesignerViewModel.ModelXRef.GetExisting(this) as ConceptualEntityType;
            if (modelEntityType != null
                && modelEntityType.BaseType != null
                && modelEntityType.BaseType.Target != null)
            {
                return modelEntityType.BaseType.Target.DisplayName.Trim();
            }
            return String.Empty;
        }

        public bool GetHasBaseTypeValue()
        {
            return (String.IsNullOrEmpty(GetBaseTypeNameValue()) == false);
        }

        /// <summary>
        ///     Walks entity properties and returns a comma-separated string with key property names
        /// </summary>
        /// <param name="separator"></param>
        /// <returns></returns>
        public string GetKeyNamesSeparatedBy(string separator)
        {
            var separatedKeys = String.Empty;

            try
            {
                var keyList = new List<String>();
                foreach (var property in GetKeyProperties())
                {
                    keyList.Add(property.Name);
                }

                separatedKeys = String.Join(separator, keyList.ToArray());
            }
            catch (InvalidOperationException)
            {
                // Detected circular inheritance, nothing to do!
            }

            return separatedKeys;
        }

        /// <summary>
        ///     Recursively walks the base type hierarchy and returns properties that are keys.
        ///     Also checks for circular inheritance and throws an InvalidOperationException if found.
        /// </summary>
        /// <returns></returns>
        internal List<Property> GetKeyProperties()
        {
            var circularPath = String.Empty;
            if (HasCircularInheritance(out circularPath))
            {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        DslResources.Error_CircularEntityInheritanceFound,
                        Name, circularPath));
            }

            var keyProperties = new List<Property>();
            if (BaseType != null)
            {
                keyProperties.AddRange(BaseType.GetKeyProperties());
            }
            else
            {
                keyProperties.AddRange(GetLocalKeyProperties());
            }
            return keyProperties;
        }

        /// <summary>
        ///     Returns a list of key properties that are defined locally in this Entity.
        /// </summary>
        /// <returns></returns>
        internal List<Property> GetLocalKeyProperties()
        {
            var keyProperties = new List<Property>();
            foreach (var property in Properties)
            {
                var scalarProperty = property as ScalarProperty;
                if (scalarProperty != null
                    && scalarProperty.EntityKey)
                {
                    keyProperties.Add(property);
                }
            }
            return keyProperties;
        }

        /// <summary>
        ///     Detects if this entity has a circular inheritance
        /// </summary>
        /// <param name="circularPathFound"></param>
        /// <returns></returns>
        internal bool HasCircularInheritance(out string circularPathFound)
        {
            var hasCircularInheritance = false;

            var visited = new Dictionary<EntityTypeBase, int>();
            visited.Add(this, 0);

            var circularPath = Name;
            circularPathFound = circularPath;
            var baseType = BaseType;

            while (baseType != null)
            {
                circularPath += " -> " + baseType.Name;
                if (visited.ContainsKey(baseType))
                {
                    circularPathFound = circularPath;
                    hasCircularInheritance = true;
                    break;
                }
                visited.Add(baseType, 0);
                baseType = baseType.BaseType;
            }

            return hasCircularInheritance;
        }

        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        ///     Returns all properties and navigation properties names (recursively including baseType properties)
        /// </summary>
        /// <returns></returns>
        internal List<string> GetPropertiesNames()
        {
            List<string> names = null;
            if (BaseType != null)
            {
                names = BaseType.GetPropertiesNames();
            }
            else
            {
                names = new List<string>();
            }

            foreach (var property in Properties)
            {
                names.Add(property.Name);
            }
            foreach (var navProp in NavigationProperties)
            {
                names.Add(navProp.Name);
            }

            return names;
        }
    }
}
