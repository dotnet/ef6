// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace UnitTests.EntityDesignerVersioningFacade.ReverseEngineerDb
{
    using System;
    using Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb;
    using Xunit;

    public class UniqueIdentifierServiceTests
    {
        [Fact]
        public static void RegisterUsedIdentifier_uses_the_specified_StringComparer_correctly()
        {
            const string identifierA = "Identifier";
            const string identifierB = "IdeNTiFieR";

            var service = new UniqueIdentifierService(StringComparer.Ordinal);
            service.RegisterUsedIdentifier(identifierA);
            Assert.DoesNotThrow(() => service.RegisterUsedIdentifier(identifierB));

            service = new UniqueIdentifierService(StringComparer.OrdinalIgnoreCase);
            service.RegisterUsedIdentifier(identifierA);
            Assert.Throws<ArgumentException>(() => service.RegisterUsedIdentifier(identifierB));
        }

        [Fact]
        public static void AdjustIdentifier_with_Ordinal_comparer_and_identity_transform_returns_expected_result()
        {
            const string identifier = "Identifier";
            const string adjustedIdentifier1 = "Identifier1";
            const string adjustedIdentifier2 = "Identifier2";
            const string adjustedIdentifier3 = "Identifier3";

            var service = new UniqueIdentifierService(StringComparer.Ordinal);
            service.RegisterUsedIdentifier(identifier);

            Assert.Equal(adjustedIdentifier1, service.AdjustIdentifier(identifier));
            Assert.Equal(adjustedIdentifier2, service.AdjustIdentifier(identifier));
            Assert.Equal(adjustedIdentifier3, service.AdjustIdentifier(identifier));
        }

        [Fact]
        public static void AdjustIdentifier_with_OrdinalIgnoreCase_comparer_and_custom_transform_returns_expected_result()
        {
            const string identifier = "My.Identifier";
            const string usedIdentifier = "My_IdENtIfiEr";
            const string adjustedIdentifier1 = "My_Identifier1";
            const string adjustedIdentifier2 = "My_Identifier2";
            const string adjustedIdentifier3 = "My_Identifier3";

            Func<string, string> transform = s => s.Replace(".", "_");

            var service = new UniqueIdentifierService(StringComparer.OrdinalIgnoreCase, transform);
            service.RegisterUsedIdentifier(usedIdentifier);

            Assert.Equal(adjustedIdentifier1, service.AdjustIdentifier(identifier));
            Assert.Equal(adjustedIdentifier2, service.AdjustIdentifier(identifier));
            Assert.Equal(adjustedIdentifier3, service.AdjustIdentifier(identifier));
        }

        [Fact]
        public static void TryGetAdjustedName_correctly_retrieves_the_adjusted_identifier_associated_with_an_object()
        {
            const string identifier = "Identifier";
            const string adjustedIdentifier1 = "Identifier1";
            const string adjustedIdentifier2 = "Identifier2";
            const string adjustedIdentifier3 = "Identifier3";
            string adjustedIdentifier;

            var value1 = new object();
            var value2 = new object();
            var value3 = new object();

            var service = new UniqueIdentifierService();
            service.RegisterUsedIdentifier(identifier);

            Assert.Equal(adjustedIdentifier1, service.AdjustIdentifier(identifier, value1));
            Assert.Equal(adjustedIdentifier2, service.AdjustIdentifier(identifier, value2));
            Assert.Equal(adjustedIdentifier3, service.AdjustIdentifier(identifier, value3));
            Assert.True(service.TryGetAdjustedName(value1, out adjustedIdentifier));
            Assert.Equal(adjustedIdentifier, adjustedIdentifier1);
            Assert.True(service.TryGetAdjustedName(value2, out adjustedIdentifier));
            Assert.Equal(adjustedIdentifier, adjustedIdentifier2);
            Assert.True(service.TryGetAdjustedName(value3, out adjustedIdentifier));
            Assert.Equal(adjustedIdentifier, adjustedIdentifier3);
        }
    }
}
