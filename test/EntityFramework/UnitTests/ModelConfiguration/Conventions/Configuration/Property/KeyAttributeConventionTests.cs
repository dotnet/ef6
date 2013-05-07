// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using Moq;
    using Xunit;

    public sealed class KeyAttributeConventionTests
    {
        [Fact]
        public void Apply_should_find_single_key()
        {
            var mockEntityTypeConfiguration = new Mock<EntityTypeConfiguration>(typeof(KeyAttributeEntity));
            var propertyInfo = typeof(KeyAttributeEntity).GetProperty("Id");

            new KeyAttributeConvention()
                .ApplyPropertyTypeConfiguration(propertyInfo, () => mockEntityTypeConfiguration.Object, new ModelConfiguration());

            mockEntityTypeConfiguration.Verify(e => e.Key(propertyInfo, It.IsAny<OverridableConfigurationParts?>()));
        }

        public class KeyAttributeEntity
        {
            [Key]
            public int Id { get; set; }
        }
    }
}
