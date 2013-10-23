// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.EntityDesigner.Rules;
    using Microsoft.Data.Entity.Design.EntityDesigner.View;

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public partial class MicrosoftDataEntityDesignDomainModel
    {
        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <returns>This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</returns>
        protected override Type[] GetCustomDomainModelTypes()
        {
            return new[]
                {
                    typeof(EntityDesignerViewModel_AddRule),
                    typeof(EntityDesignerDiagram_AddRule),
                    typeof(Association_AddRule),
                    typeof(AssociationConnector_AddRule),
                    typeof(AssociationConnector_ChangeRule),
                    typeof(AssociationConnector_DeleteRule),
                    typeof(Inheritance_AddRule),
                    typeof(Inheritance_DeleteRule),
                    typeof(InheritanceConnector_AddRule),
                    typeof(InheritanceConnector_ChangeRule),
                    typeof(InheritanceConnector_DeleteRule),
                    typeof(EntityType_AddRule),
                    typeof(EntityType_ChangeRule),
                    typeof(EntityTypeShape_AddRule),
                    typeof(EntityTypeShape_ChangeRule),
                    typeof(EntityTypeShape_DeleteRule),
                    typeof(Property_AddRule),
                    typeof(Property_ChangeRule),
                    typeof(ScalarProperty_ChangeRule),
                    typeof(NavigationProperty_AddRule),
                    typeof(NavigationProperty_ChangeRule),
                    typeof(EntityTypeElementListCompartment)
                };
        }
    }
}
