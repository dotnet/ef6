// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Validation
{
    using System.Collections.Generic;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Internal.Validation;

    internal class ValidationProviderForMock : ValidationProvider
    {
        public ValidationProviderForMock(EntityValidatorBuilder builder)
            : base(builder)
        {
        }

        public EntityValidator GetEntityValidatorBase(InternalEntityEntry entityEntry)
        {
            return base.GetEntityValidator(entityEntry);
        }

        public PropertyValidator GetPropertyValidatorBase(InternalEntityEntry owningEntity, InternalMemberEntry property)
        {
            return base.GetPropertyValidator(owningEntity, property);
        }

        public PropertyValidator GetValidatorForPropertyBase(EntityValidator entityValidator, InternalMemberEntry memberEntry)
        {
            return base.GetValidatorForProperty(entityValidator, memberEntry);
        }

        public EntityValidationContext GetEntityValidationContextBase(
            InternalEntityEntry entityEntry, IDictionary<object, object> items)
        {
            return base.GetEntityValidationContext(entityEntry, items);
        }
    }
}
