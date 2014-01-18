// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.DatabaseGeneration.OutputGenerators
{
    using System;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.DatabaseGeneration.Properties;
    using Microsoft.Data.Entity.Design.VersioningFacade;

    internal static class OutputGeneratorHelpers
    {
        // <summary>
        //     Infer a Storage-layer EntityType name from a Conceptual-layer EntityType
        //     1. If this is a root type, then we will use the EntitySet name.
        //     2. If this is a derived type, then we will return the name of the EntitySet
        //     appended to the name of the EntityType.
        //     * NOTE: This is better than pluralization because this scales well for international users
        // </summary>
        internal static string GetStorageEntityTypeName(EntityType entityType, EdmItemCollection edm)
        {
            var storageEntityTypeName = String.Empty;

            // First get the EntitySet name. Unfortunately the Metadata APIs don't have the ability to
            // get an EntitySet from an EntityType, so we have to incur this perf hit.
            var rootOrSelf = entityType.GetRootOrSelf();
            foreach (var entitySet in edm.GetAllEntitySets())
            {
                if (rootOrSelf == entitySet.ElementType)
                {
                    storageEntityTypeName = entitySet.Name;
                }
            }

            Debug.Assert(!String.IsNullOrEmpty(storageEntityTypeName), "We didn't find an EntitySet for the EntityType " + entityType.Name);

            if (entityType.IsDerivedType())
            {
                storageEntityTypeName = String.Format(CultureInfo.CurrentCulture, "{0}_{1}", storageEntityTypeName, entityType.Name);
            }

            return storageEntityTypeName;
        }

        // <summary>
        //     A name derived from a *:* association will be: FK_[Association Name]_[End Name]. Note that we are
        //     getting the association name for *one* of the SSDL associations that are inferred from a CSDL *:*
        //     association. The 'principalEnd' corresponds to the end of the *:* association that will become
        //     the principal end in the 1:* association (where the * end is the newly-constructed entity corresponding
        //     to the link table)
        // </summary>
        internal static string GetStorageAssociationNameFromManyToMany(AssociationEndMember principalEnd)
        {
            var association = principalEnd.DeclaringType as AssociationType;
            Debug.Assert(
                association != null, "The DeclaringType of the AssociationEndMember " + principalEnd.Name + " should be an AssociationType");
            Debug.Assert(principalEnd != null, "The principal end cannot be null");
            var associationName = String.Empty;
            if (association != null
                && principalEnd != null)
            {
                associationName = String.Format(
                    CultureInfo.CurrentCulture, Resources.CodeViewManyToManyAssocName, association.Name, principalEnd.Name);
            }
            return associationName;
        }

        // <summary>
        //     A name derived from a *:* association will be: FK_[Association Name]_[End Name]. Note that we are
        //     getting the association name for *one* of the SSDL associations that are inferred from a CSDL *:*
        //     association. The 'principalEnd' corresponds to the end of the *:* association that will become
        //     the principal end in the 1:* association (where the * end is the newly-constructed entity corresponding
        //     to the link table)
        // </summary>
        internal static string GetStorageAssociationSetNameFromManyToMany(AssociationSet associationSet, AssociationEndMember principalEnd)
        {
            Debug.Assert(associationSet != null, "AssociationSet should not be null");
            Debug.Assert(principalEnd != null, "The principal end cannot be null");
            var associationSetName = String.Empty;
            if (associationSet != null
                && principalEnd != null)
            {
                associationSetName = String.Format(
                    CultureInfo.CurrentCulture, Resources.CodeViewManyToManyAssocName, associationSet.Name, principalEnd.Name);
            }
            return associationSetName;
        }

        // <summary>
        //     1. If there is a NavigationProperty on the dependent end, then the FK name will be: [NavProp Name]_[Property Name]
        //     2. If there isn't a NavigationProperty, then we will use the [Association Name]_[EndName]_[Property Name]
        // </summary>
        internal static string GetFkName(AssociationType association, AssociationEndMember endWithNavProp, string keyPropertyName)
        {
            var fkName = String.Empty;

            // We attempt to find a navigation property that uses the same association and points to the other end. That last
            // part is important with self-associations.
            var principalEnd = association.GetOtherEnd(endWithNavProp);
            var navigationProperty = endWithNavProp
                .GetEntityType()
                .NavigationProperties
                .Where(np => (np.RelationshipType == association && np.ToEndMember == principalEnd)).FirstOrDefault();
            if (navigationProperty != null)
            {
                // First attempt to find the NavigationProperty that points to the principal end
                fkName = String.Format(CultureInfo.CurrentCulture, "{0}_{1}", navigationProperty.Name, keyPropertyName);
            }
            else if (association != null)
            {
                // If there isn't a NavigationProperty defined, then we will use the Association Name
                fkName = String.Format(CultureInfo.CurrentCulture, "{0}_{1}_{2}", association.Name, endWithNavProp.Name, keyPropertyName);
            }

            Debug.Assert(!String.IsNullOrEmpty(fkName), "Foreign key name could not be determined for the association " + association.Name);
            return fkName;
        }

        // <summary>
        //     Construct storage entity container name from CSDL Namespace name.
        //     If the CSDL namespace name is null, then this will just return "StoreContainer".
        // </summary>
        internal static string ConstructStorageEntityContainerName(string csdlNamespaceName)
        {
            var storeContainerName = "StoreContainer";

            // Storage Entity Container Name could not contain period but CSDLNamespaceName could.
            if (false == String.IsNullOrEmpty(csdlNamespaceName))
            {
                storeContainerName = csdlNamespaceName.Replace(".", String.Empty) + storeContainerName;
            }
            return storeContainerName;
        }

        internal static void CopyExtendedPropertiesToSsdlElement(MetadataItem metadataItem, XContainer xContainer)
        {
            foreach (var extendedProperty in metadataItem.MetadataProperties.Where(mp => mp.PropertyKind == PropertyKind.Extended))
            {
                var exPropertyElement = extendedProperty.Value as XElement;
                if (exPropertyElement != null)
                {
                    // find the CopyToSSDL attribute - if it exists it can be in any EDMX namespace
                    var copyToSSDLAttribute = exPropertyElement.Attributes().FirstOrDefault(
                        attr => attr.Name.LocalName.Equals("CopyToSSDL", StringComparison.Ordinal)
                                && SchemaManager.GetEDMXNamespaceNames().Contains(attr.Name.NamespaceName));
                    if (copyToSSDLAttribute != null)
                    {
                        if ((bool?)copyToSSDLAttribute == true)
                        {
                            // CopyToSsdl is true, so let's copy this extended property
                            var exAttributeNamespace = copyToSSDLAttribute.Name.Namespace;
                            var newExPropertyElement = new XElement(exPropertyElement);
                            var newCopyToSsdlAttribute = newExPropertyElement.Attribute(exAttributeNamespace + "CopyToSSDL");
                            newCopyToSsdlAttribute.Remove();

                            var namespacePrefix = newExPropertyElement.GetPrefixOfNamespace(exAttributeNamespace);
                            if (namespacePrefix != null)
                            {
                                var xmlnsEdmxAttr = newExPropertyElement.Attribute(XNamespace.Xmlns + namespacePrefix);
                                if (xmlnsEdmxAttr != null)
                                {
                                    xmlnsEdmxAttr.Remove();
                                }
                            }
                            xContainer.Add(newExPropertyElement);
                        }
                    }
                }
            }
        }
    }
}
