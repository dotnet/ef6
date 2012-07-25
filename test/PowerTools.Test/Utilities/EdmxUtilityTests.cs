// ﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace Microsoft.DbContextPackage.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Data.Mapping;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.DbContextPackage.Extensions;
    using Microsoft.DbContextPackage.Resources;
    using Xunit;

    public class EdmxUtilityTests
    {
        [Fact]
        public void GetMappingCollection_returns_mapping()
        {
            var edmxContents = @"<?xml version='1.0'?>
<Edmx Version='3.0' xmlns='http://schemas.microsoft.com/ado/2009/11/edmx'>
  <Runtime>
    <ConceptualModels>
      <Schema Namespace='Model' Alias='Self' annotation:UseStrongSpatialTypes='false' xmlns:annotation='http://schemas.microsoft.com/ado/2009/02/edm/annotation' xmlns='http://schemas.microsoft.com/ado/2009/11/edm'>
        <EntityContainer Name='DatabaseEntities' annotation:LazyLoadingEnabled='true'>
          <EntitySet Name='Entities' EntityType='Model.Entity' />
        </EntityContainer>
        <EntityType Name='Entity'>
          <Key>
            <PropertyRef Name='Id' />
          </Key>
          <Property Name='Id' Type='Int32' Nullable='false' annotation:StoreGeneratedPattern='Identity' />
        </EntityType>
      </Schema>
    </ConceptualModels>
    <Mappings>
      <Mapping Space='C-S' xmlns='http://schemas.microsoft.com/ado/2009/11/mapping/cs'>
        <EntityContainerMapping StorageEntityContainer='ModelStoreContainer' CdmEntityContainer='DatabaseEntities'>
          <EntitySetMapping Name='Entities'>
            <EntityTypeMapping TypeName='Model.Entity'>
              <MappingFragment StoreEntitySet='Entities'>
                <ScalarProperty Name='Id' ColumnName='Id' />
              </MappingFragment>
            </EntityTypeMapping>
          </EntitySetMapping>
        </EntityContainerMapping>
      </Mapping>
    </Mappings>
    <StorageModels>
      <Schema Namespace='Model.Store' Alias='Self' Provider='System.Data.SqlClient' ProviderManifestToken='2008' xmlns:store='http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator' xmlns='http://schemas.microsoft.com/ado/2009/11/edm/ssdl'>
        <EntityContainer Name='ModelStoreContainer'>
          <EntitySet Name='Entities' EntityType='Model.Store.Entities' store:Type='Tables' Schema='dbo' />
        </EntityContainer>
        <EntityType Name='Entities'>
          <Key>
            <PropertyRef Name='Id' />
          </Key>
          <Property Name='Id' Type='int' Nullable='false' StoreGeneratedPattern='Identity' />
        </EntityType>
      </Schema>
    </StorageModels>
  </Runtime>
</Edmx>";
            StorageMappingItemCollection mappingCollection;

            using (var edmx = new TempFile(edmxContents))
            {
                mappingCollection = new EdmxUtility(edmx.FileName)
                    .GetMappingCollection();
            }

            Assert.True(mappingCollection.Contains("DatabaseEntities"));
        }

        [Fact]
        public void GetMappingCollection_returns_mapping_for_v2_schema()
        {
            var edmxContents = @"<?xml version='1.0' ?>
<Edmx Version='2.0' xmlns='http://schemas.microsoft.com/ado/2008/10/edmx'>
  <Runtime>
    <ConceptualModels>
      <Schema Namespace='Model' Alias='Self' xmlns:annotation='http://schemas.microsoft.com/ado/2009/02/edm/annotation' xmlns='http://schemas.microsoft.com/ado/2008/09/edm'>
        <EntityContainer Name='DatabaseEntities' annotation:LazyLoadingEnabled='true'>
          <EntitySet Name='Entities' EntityType='Model.Entity' />
        </EntityContainer>
        <EntityType Name='Entity'>
          <Key>
            <PropertyRef Name='Id' />
          </Key>
          <Property Name='Id' Type='Int32' Nullable='false' annotation:StoreGeneratedPattern='Identity' />
        </EntityType>
      </Schema>
    </ConceptualModels>
    <Mappings>
      <Mapping Space='C-S' xmlns='http://schemas.microsoft.com/ado/2008/09/mapping/cs'>
        <EntityContainerMapping StorageEntityContainer='ModelStoreContainer' CdmEntityContainer='DatabaseEntities'>
          <EntitySetMapping Name='Entities'>
            <EntityTypeMapping TypeName='Model.Entity'>
              <MappingFragment StoreEntitySet='Entities'>
                <ScalarProperty Name='Id' ColumnName='Id' />
              </MappingFragment>
            </EntityTypeMapping>
          </EntitySetMapping>
        </EntityContainerMapping>
      </Mapping>
    </Mappings>
    <StorageModels>
      <Schema Namespace='Model.Store' Alias='Self' Provider='System.Data.SqlClient' ProviderManifestToken='2008' xmlns:store='http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator' xmlns='http://schemas.microsoft.com/ado/2009/02/edm/ssdl'>
        <EntityContainer Name='ModelStoreContainer'>
          <EntitySet Name='Entities' EntityType='Model.Store.Entities' store:Type='Tables' Schema='dbo' />
        </EntityContainer>
        <EntityType Name='Entities'>
          <Key>
            <PropertyRef Name='Id' />
          </Key>
          <Property Name='Id' Type='int' Nullable='false' StoreGeneratedPattern='Identity' />
        </EntityType>
      </Schema>
    </StorageModels>
  </Runtime>
</Edmx>";
            StorageMappingItemCollection mappingCollection;

            using (var edmx = new TempFile(edmxContents))
            {
                mappingCollection = new EdmxUtility(edmx.FileName)
                    .GetMappingCollection();
            }

            Assert.True(mappingCollection.Contains("DatabaseEntities"));
        }

        [Fact]
        public void GetMappingCollection_throws_on_schema_errors()
        {
            var edmxContents = @"<?xml version='1.0'?>
<Edmx Version='3.0' xmlns='http://schemas.microsoft.com/ado/2009/11/edmx'>
  <Runtime>
    <ConceptualModels>
      <Schema xmlns='http://schemas.microsoft.com/ado/2009/11/edm' />
    </ConceptualModels>
  </Runtime>
</Edmx>";

            using (var edmx = new TempFile(edmxContents))
            {
                var ex = Assert.Throws<EdmSchemaErrorException>(
                    () => new EdmxUtility(edmx.FileName).GetMappingCollection());

                Assert.Equal(
                    Strings.EdmSchemaError(
                        Path.GetFileName(edmx.FileName),
                        "ConceptualModels"),
                    ex.Message);
            }
        }

        private sealed class TempFile : IDisposable
        {
            private readonly string _fileName;
            private bool _disposed;

            public TempFile(string contents)
            {
                _fileName = Path.GetTempFileName();
                File.WriteAllText(_fileName, contents);
            }

            ~TempFile()
            {
                Dispose(false);
            }

            public string FileName
            {
                get
                {
                    HandleDisposed();

                    return _fileName;
                }
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (!_disposed)
                {
                    if (!string.IsNullOrWhiteSpace(_fileName))
                    {
                        File.Delete(_fileName);
                    }

                    _disposed = true;
                }
            }

            private void HandleDisposed()
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(null);
                }
            }
        }
    }
}
