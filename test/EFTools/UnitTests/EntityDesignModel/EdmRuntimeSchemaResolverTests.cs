// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace UnitTests.EntityDesignModel
{
    using System;
    using System.IO;
    using System.Linq;
    using Microsoft.Data.Entity.Design.Model;
    using Xunit;

    public class EdmRuntimeSchemaResolverTests
    {
        private readonly string[] KnownSchemaNames =
            new[]
                {
                    "System.Data.Resources.CSDLSchema_1.xsd",
                    "System.Data.Resources.CSDLSchema_2.xsd",
                    "System.Data.Resources.CSDLSchema_3.xsd",
                    "System.Data.Resources.CSMSL_1.xsd",
                    "System.Data.Resources.CSMSL_2.xsd",
                    "System.Data.Resources.CSMSL_3.xsd",
                    "System.Data.Resources.SSDLSchema.xsd",
                    "System.Data.Resources.SSDLSchema_2.xsd",
                    "System.Data.Resources.SSDLSchema_3.xsd",
                    "System.Data.Resources.ProviderServices.ProviderManifest.xsd",
                    "System.Data.Resources.EntityStoreSchemaGenerator.xsd",
                    "System.Data.Resources.CodeGenerationSchema.xsd",
                    "System.Data.Resources.AnnotationSchema.xsd"
                };

        [Fact]
        public void Can_get_resource_stream_for_all_schemas()
        {
            var schemaResolver = new EdmRuntimeSchemaResolver();

            foreach (var schemaUri in KnownSchemaNames.Select(schemaName => new Uri("res://" + schemaName, UriKind.Absolute)))
            {
                var stream = (Stream)schemaResolver.GetEntity(schemaUri, null, null);
                Assert.NotNull(stream);
                stream.Dispose();
            }
        }

        [Fact]
        public void Can_resolve_absolute_uri_based_on_base_uri_and_relative_uri()
        {
            var schemaResolver = new EdmRuntimeSchemaResolver();

            foreach (var schemaName in KnownSchemaNames)
            {
                Assert.Equal(
                    new Uri("res://" + schemaName, UriKind.Absolute), schemaResolver.ResolveUri(null, schemaName));
            }
        }
    }
}
