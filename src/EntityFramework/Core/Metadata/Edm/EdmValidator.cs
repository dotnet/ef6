// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class for representing the validator
    /// </summary>
    internal class EdmValidator
    {
        /// <summary>
        /// Gets or Sets whether the validator should skip readonly items
        /// </summary>
        internal bool SkipReadOnlyItems { get; set; }

        /// <summary>
        /// Validate a collection of items in a batch
        /// </summary>
        /// <param name="items"> A collection of items to validate </param>
        /// <param name="ospaceErrors"> List of validation errors that were previously collected by the caller. if it encounters more errors, it adds them to this list of errors </param>
        public void Validate<T>(IEnumerable<T> items, List<EdmItemError> ospaceErrors)
            where T : EdmType // O-Space only supports EdmType
        {
            Check.NotNull(items, "items");
            Check.NotNull(items, "items");

            var validatedItems = new HashSet<MetadataItem>();

            foreach (MetadataItem item in items)
            {
                // Just call the internal helper method for each item
                InternalValidate(item, ospaceErrors, validatedItems);
            }
        }

        /// <summary>
        /// Event hook to perform preprocessing on the validation error before it gets added to a list of errors
        /// </summary>
        /// <param name="e"> The event args for this event </param>
        protected virtual void OnValidationError(ValidationErrorEventArgs e)
        {
        }

        /// <summary>
        /// Invoke the event hook Add an error to the list
        /// </summary>
        /// <param name="errors"> The list of errors to add to </param>
        /// <param name="newError"> The new error to add </param>
        private void AddError(List<EdmItemError> errors, EdmItemError newError)
        {
            // Create an event args object and call the event hook, the derived class may have changed
            // the validation error to some other object, in which case we add the validation error object
            // coming from the event args
            var e = new ValidationErrorEventArgs(newError);
            OnValidationError(e);
            errors.Add(e.ValidationError);
        }

        /// <summary>
        /// Allows derived classes to perform additional validation
        /// </summary>
        /// <param name="item"> The item to perform additional validation </param>
        /// <returns> A collection of errors </returns>
        protected virtual IEnumerable<EdmItemError> CustomValidate(MetadataItem item)
        {
            return null;
        }

        /// <summary>
        /// Validate an item object
        /// </summary>
        /// <param name="item"> The item to validate </param>
        /// <param name="errors"> An error collection for adding validation errors </param>
        /// <param name="validatedItems"> A dictionary keeping track of items that have been validated </param>
        private void InternalValidate(MetadataItem item, List<EdmItemError> errors, HashSet<MetadataItem> validatedItems)
        {
            DebugCheck.NotNull(item);

            // If the item has already been validated or we need to skip readonly items, then skip
            if ((item.IsReadOnly && SkipReadOnlyItems)
                || validatedItems.Contains(item))
            {
                return;
            }

            // Add this item to the dictionary so we won't validate this again.  Note that we only do this
            // in this function because every other function should eventually delegate to here
            validatedItems.Add(item);

            // Check to make sure the item has an identity
            if (string.IsNullOrEmpty(item.Identity))
            {
                AddError(errors, new EdmItemError(Strings.Validator_EmptyIdentity));
            }

            switch (item.BuiltInTypeKind)
            {
                case BuiltInTypeKind.CollectionType:
                    ValidateCollectionType((CollectionType)item, errors, validatedItems);
                    break;
                case BuiltInTypeKind.ComplexType:
                    ValidateComplexType((ComplexType)item, errors, validatedItems);
                    break;
                case BuiltInTypeKind.EntityType:
                    ValidateEntityType((EntityType)item, errors, validatedItems);
                    break;
                case BuiltInTypeKind.Facet:
                    ValidateFacet((Facet)item, errors, validatedItems);
                    break;
                case BuiltInTypeKind.MetadataProperty:
                    ValidateMetadataProperty((MetadataProperty)item, errors, validatedItems);
                    break;
                case BuiltInTypeKind.NavigationProperty:
                    ValidateNavigationProperty((NavigationProperty)item, errors, validatedItems);
                    break;
                case BuiltInTypeKind.PrimitiveType:
                    ValidatePrimitiveType((PrimitiveType)item, errors, validatedItems);
                    break;
                case BuiltInTypeKind.EdmProperty:
                    ValidateEdmProperty((EdmProperty)item, errors, validatedItems);
                    break;
                case BuiltInTypeKind.RefType:
                    ValidateRefType((RefType)item, errors, validatedItems);
                    break;
                case BuiltInTypeKind.TypeUsage:
                    ValidateTypeUsage((TypeUsage)item, errors, validatedItems);
                    break;

                    // Abstract classes
                case BuiltInTypeKind.EntityTypeBase:
                case BuiltInTypeKind.EdmType:
                case BuiltInTypeKind.MetadataItem:
                case BuiltInTypeKind.EdmMember:
                case BuiltInTypeKind.RelationshipEndMember:
                case BuiltInTypeKind.RelationshipType:
                case BuiltInTypeKind.SimpleType:
                case BuiltInTypeKind.StructuralType:
                    Debug.Assert(
                        false,
                        "An instance with a built in type kind refering to the abstract type " + item.BuiltInTypeKind + " is encountered");
                    break;

                default:
                    //Debug.Assert(false, String.Format(CultureInfo.InvariantCulture, "Validate not implemented for {0}", item.BuiltInTypeKind));
                    break;
            }

            // Performs other custom validation
            var customErrors = CustomValidate(item);
            if (customErrors != null)
            {
                errors.AddRange(customErrors);
            }
        }

        /// <summary>
        /// Validate an CollectionType object
        /// </summary>
        /// <param name="item"> The CollectionType object to validate </param>
        /// <param name="errors"> An error collection for adding validation errors </param>
        /// <param name="validatedItems"> A dictionary keeping track of items that have been validated </param>
        private void ValidateCollectionType(CollectionType item, List<EdmItemError> errors, HashSet<MetadataItem> validatedItems)
        {
            ValidateEdmType(item, errors, validatedItems);

            // Check that it doesn't have a base type
            if (item.BaseType != null)
            {
                AddError(errors, new EdmItemError(Strings.Validator_CollectionTypesCannotHaveBaseType));
            }

            if (item.TypeUsage == null)
            {
                AddError(errors, new EdmItemError(Strings.Validator_CollectionHasNoTypeUsage));
            }
            else
            {
                // Just validate the element type, there is nothing on the collection itself to validate
                InternalValidate(item.TypeUsage, errors, validatedItems);
            }
        }

        /// <summary>
        /// Validate an ComplexType object
        /// </summary>
        /// <param name="item"> The ComplexType object to validate </param>
        /// <param name="errors"> An error collection for adding validation errors </param>
        /// <param name="validatedItems"> A dictionary keeping track of items that have been validated </param>
        private void ValidateComplexType(ComplexType item, List<EdmItemError> errors, HashSet<MetadataItem> validatedItems)
        {
            ValidateStructuralType(item, errors, validatedItems);
        }

        /// <summary>
        /// Validate an EdmType object
        /// </summary>
        /// <param name="item"> The EdmType object to validate </param>
        /// <param name="errors"> An error collection for adding validation errors </param>
        /// <param name="validatedItems"> A dictionary keeping track of items that have been validated </param>
        [SuppressMessage("Microsoft.Performance", "CA1820:TestForEmptyStringsUsingStringLength")]
        private void ValidateEdmType(EdmType item, List<EdmItemError> errors, HashSet<MetadataItem> validatedItems)
        {
            ValidateItem(item, errors, validatedItems);

            // Check that this type has a name and namespace
            if (string.IsNullOrEmpty(item.Name))
            {
                AddError(errors, new EdmItemError(Strings.Validator_TypeHasNoName));
            }
            if (null == item.NamespaceName
                ||
                item.DataSpace != DataSpace.OSpace && string.Empty == item.NamespaceName)
            {
                AddError(errors, new EdmItemError(Strings.Validator_TypeHasNoNamespace));
            }

            // We don't need to verify that the base type chain eventually gets to null because
            // the CLR doesn't allow loops in class hierarchies.
            if (item.BaseType != null)
            {
                // Validate the base type
                InternalValidate(item.BaseType, errors, validatedItems);
            }
        }

        /// <summary>
        /// Validate an EntityType object
        /// </summary>
        /// <param name="item"> The EntityType object to validate </param>
        /// <param name="errors"> An error collection for adding validation errors </param>
        /// <param name="validatedItems"> A dictionary keeping track of items that have been validated </param>
        private void ValidateEntityType(EntityType item, List<EdmItemError> errors, HashSet<MetadataItem> validatedItems)
        {
            // check the base EntityType has Keys
            if (item.BaseType == null)
            {
                // Check that there is at least one key member
                if (item.KeyMembers.Count < 1)
                {
                    AddError(errors, new EdmItemError(Strings.Validator_NoKeyMembers(item.FullName)));
                }
                else
                {
                    foreach (EdmProperty keyProperty in item.KeyMembers)
                    {
                        if (keyProperty.Nullable)
                        {
                            AddError(errors, new EdmItemError(Strings.Validator_NullableEntityKeyProperty(keyProperty.Name, item.FullName)));
                        }
                    }
                }
            }

            // Continue to process the entity to see if there are other errors. This allows the user to 
            // fix as much as possible at the same time.
            ValidateStructuralType(item, errors, validatedItems);
        }

        /// <summary>
        /// Validate an Facet object
        /// </summary>
        /// <param name="item"> The Facet object to validate </param>
        /// <param name="errors"> An error collection for adding validation errors </param>
        /// <param name="validatedItems"> A dictionary keeping track of items that have been validated </param>
        private void ValidateFacet(Facet item, List<EdmItemError> errors, HashSet<MetadataItem> validatedItems)
        {
            ValidateItem(item, errors, validatedItems);

            // Check that this facet has a name
            if (string.IsNullOrEmpty(item.Name))
            {
                AddError(errors, new EdmItemError(Strings.Validator_FacetHasNoName));
            }

            // Validate the type
            if (item.FacetType == null)
            {
                AddError(errors, new EdmItemError(Strings.Validator_FacetTypeIsNull));
            }
            else
            {
                InternalValidate(item.FacetType, errors, validatedItems);
            }
        }

        /// <summary>
        /// Validate an MetadataItem object
        /// </summary>
        /// <param name="item"> The MetadataItem object to validate </param>
        /// <param name="errors"> An error collection for adding validation errors </param>
        /// <param name="validatedItems"> A dictionary keeping track of items that have been validated </param>
        private void ValidateItem(MetadataItem item, List<EdmItemError> errors, HashSet<MetadataItem> validatedItems)
        {
            // In here, we look at RawMetadataProperties because it dynamically add MetadataProperties when you access the
            // normal MetadataProperties property. This avoids needless validation and infinite recursion
            if (item.RawMetadataProperties != null)
            {
                foreach (var itemAttribute in item.MetadataProperties)
                {
                    InternalValidate(itemAttribute, errors, validatedItems);
                }
            }
        }

        /// <summary>
        /// Validate an EdmMember object
        /// </summary>
        /// <param name="item"> The item object to validate </param>
        /// <param name="errors"> An error collection for adding validation errors </param>
        /// <param name="validatedItems"> A dictionary keeping track of items that have been validated </param>
        private void ValidateEdmMember(EdmMember item, List<EdmItemError> errors, HashSet<MetadataItem> validatedItems)
        {
            ValidateItem(item, errors, validatedItems);

            // Check that this member has a name
            if (string.IsNullOrEmpty(item.Name))
            {
                AddError(errors, new EdmItemError(Strings.Validator_MemberHasNoName));
            }

            if (item.DeclaringType == null)
            {
                AddError(errors, new EdmItemError(Strings.Validator_MemberHasNullDeclaringType));
            }
            else
            {
                InternalValidate(item.DeclaringType, errors, validatedItems);
            }

            if (item.TypeUsage == null)
            {
                AddError(errors, new EdmItemError(Strings.Validator_MemberHasNullTypeUsage));
            }
            else
            {
                InternalValidate(item.TypeUsage, errors, validatedItems);
            }
        }

        /// <summary>
        /// Validate an MetadataProperty object
        /// </summary>
        /// <param name="item"> The MetadataProperty object to validate </param>
        /// <param name="errors"> An error collection for adding validation errors </param>
        /// <param name="validatedItems"> A dictionary keeping track of items that have been validated </param>
        private void ValidateMetadataProperty(MetadataProperty item, List<EdmItemError> errors, HashSet<MetadataItem> validatedItems)
        {
            // Validate only for user added item attributes, for system attributes, we can skip validation
            if (item.PropertyKind
                == PropertyKind.Extended)
            {
                ValidateItem(item, errors, validatedItems);

                // Check that this member has a name
                if (string.IsNullOrEmpty(item.Name))
                {
                    AddError(errors, new EdmItemError(Strings.Validator_MetadataPropertyHasNoName));
                }

                if (item.TypeUsage == null)
                {
                    AddError(errors, new EdmItemError(Strings.Validator_ItemAttributeHasNullTypeUsage));
                }
                else
                {
                    InternalValidate(item.TypeUsage, errors, validatedItems);
                }
            }
        }

        /// <summary>
        /// Validate an NavigationProperty object
        /// </summary>
        /// <param name="item"> The NavigationProperty object to validate </param>
        /// <param name="errors"> An error collection for adding validation errors </param>
        /// <param name="validatedItems"> A dictionary keeping track of items that have been validated </param>
        private void ValidateNavigationProperty(NavigationProperty item, List<EdmItemError> errors, HashSet<MetadataItem> validatedItems)
        {
            // Continue to process the property to see if there are other errors. This allows the user to fix as much as possible at the same time.
            ValidateEdmMember(item, errors, validatedItems);
        }

        /// <summary>
        /// Validate an GetPrimitiveType object
        /// </summary>
        /// <param name="item"> The GetPrimitiveType object to validate </param>
        /// <param name="errors"> An error collection for adding validation errors </param>
        /// <param name="validatedItems"> A dictionary keeping track of items that have been validated </param>
        private void ValidatePrimitiveType(PrimitiveType item, List<EdmItemError> errors, HashSet<MetadataItem> validatedItems)
        {
            ValidateSimpleType(item, errors, validatedItems);
        }

        /// <summary>
        /// Validate an EdmProperty object
        /// </summary>
        /// <param name="item"> The EdmProperty object to validate </param>
        /// <param name="errors"> An error collection for adding validation errors </param>
        /// <param name="validatedItems"> A dictionary keeping track of items that have been validated </param>
        private void ValidateEdmProperty(EdmProperty item, List<EdmItemError> errors, HashSet<MetadataItem> validatedItems)
        {
            ValidateEdmMember(item, errors, validatedItems);
        }

        /// <summary>
        /// Validate an RefType object
        /// </summary>
        /// <param name="item"> The RefType object to validate </param>
        /// <param name="errors"> An error collection for adding validation errors </param>
        /// <param name="validatedItems"> A dictionary keeping track of items that have been validated </param>
        private void ValidateRefType(RefType item, List<EdmItemError> errors, HashSet<MetadataItem> validatedItems)
        {
            ValidateEdmType(item, errors, validatedItems);

            // Check that it doesn't have a base type
            if (item.BaseType != null)
            {
                AddError(errors, new EdmItemError(Strings.Validator_RefTypesCannotHaveBaseType));
            }

            // Just validate the element type, there is nothing on the collection itself to validate
            if (item.ElementType == null)
            {
                AddError(errors, new EdmItemError(Strings.Validator_RefTypeHasNullEntityType));
            }
            else
            {
                InternalValidate(item.ElementType, errors, validatedItems);
            }
        }

        /// <summary>
        /// Validate an SimpleType object
        /// </summary>
        /// <param name="item"> The SimpleType object to validate </param>
        /// <param name="errors"> An error collection for adding validation errors </param>
        /// <param name="validatedItems"> A dictionary keeping track of items that have been validated </param>
        private void ValidateSimpleType(SimpleType item, List<EdmItemError> errors, HashSet<MetadataItem> validatedItems)
        {
            ValidateEdmType(item, errors, validatedItems);
        }

        /// <summary>
        /// Validate an StructuralType object
        /// </summary>
        /// <param name="item"> The StructuralType object to validate </param>
        /// <param name="errors"> An error collection for adding validation errors </param>
        /// <param name="validatedItems"> A dictionary keeping track of items that have been validated </param>
        private void ValidateStructuralType(StructuralType item, List<EdmItemError> errors, HashSet<MetadataItem> validatedItems)
        {
            ValidateEdmType(item, errors, validatedItems);

            // Just validate each member, the collection already guaranteed that there aren't any nulls in the collection
            var allMembers = new Dictionary<string, EdmMember>();
            foreach (var member in item.Members)
            {
                // Check if the base type already has a member of the same name
                EdmMember baseMember = null;
                if (allMembers.TryGetValue(member.Name, out baseMember))
                {
                    AddError(errors, new EdmItemError(Strings.Validator_BaseTypeHasMemberOfSameName));
                }
                else
                {
                    allMembers.Add(member.Name, member);
                }

                InternalValidate(member, errors, validatedItems);
            }
        }

        /// <summary>
        /// Validate an TypeUsage object
        /// </summary>
        /// <param name="item"> The TypeUsage object to validate </param>
        /// <param name="errors"> An error collection for adding validation errors </param>
        /// <param name="validatedItems"> A dictionary keeping track of items that have been validated </param>
        private void ValidateTypeUsage(TypeUsage item, List<EdmItemError> errors, HashSet<MetadataItem> validatedItems)
        {
            ValidateItem(item, errors, validatedItems);

            if (item.EdmType == null)
            {
                AddError(errors, new EdmItemError(Strings.Validator_TypeUsageHasNullEdmType));
            }
            else
            {
                InternalValidate(item.EdmType, errors, validatedItems);
            }

            foreach (var facet in item.Facets)
            {
                InternalValidate(facet, errors, validatedItems);
            }
        }
    }
}
