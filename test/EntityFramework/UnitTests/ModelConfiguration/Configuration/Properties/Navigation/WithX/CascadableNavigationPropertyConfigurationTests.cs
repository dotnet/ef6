// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Configuration.UnitTests
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using Xunit;

    public sealed class CascadableNavigationPropertyConfigurationTests
    {
        [Fact]
        public void Will_cascade_should_set_correct_delete_action()
        {
            var associationConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo());

            new ForeignKeyNavigationPropertyConfiguration(associationConfiguration).WillCascadeOnDelete();

            Assert.Equal(EdmOperationAction.Cascade, associationConfiguration.DeleteAction);
        }

        [Fact]
        public void Will_cascade_false_should_set_correct_delete_action()
        {
            var associationConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo());

            new ForeignKeyNavigationPropertyConfiguration(associationConfiguration).WillCascadeOnDelete(false);

            Assert.Equal(EdmOperationAction.None, associationConfiguration.DeleteAction);
        }
    }
}