// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Objects
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Spatial;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;
    using Xunit;

    public class SerializationScenarios : FunctionalTestBase
    {
        public class ProxiesContext : DbContext
        {
            static ProxiesContext()
            {
                Database.SetInitializer<ProxiesContext>(null);
            }

            public DbSet<MeLazyLoadS> MeLazyLoads { get; set; }
            public DbSet<MeTrackChangesS> MeTrackChanges { get; set; }
            public DbSet<MeSimpleEntitiesS> MeSimpleEntities { get; set; }
        }

        [Serializable]
        public class MeTrackChangesS
        {
            public MeTrackChangesS()
            {
                MeComplexTypeS = new MeComplexTypeS();
            }

            public virtual int Id { get; set; }
            public virtual ICollection<MeLazyLoadS> MeLazyLoad { get; set; }
            public virtual MeComplexTypeS MeComplexTypeS { get; set; }
            public virtual MeTrackChangesS Parent { get; set; }
            public virtual ICollection<MeTrackChangesS> Children { get; set; }
        }

        [Serializable]
        public class MeLazyLoadS
        {
            public MeLazyLoadS()
            {
                MeComplexTypeS = new MeComplexTypeS();
            }

            public int Id { get; set; }
            public virtual MeTrackChangesS MeTrackChanges { get; set; }
            public MeComplexTypeS MeComplexTypeS { get; set; }
        }

        [Serializable]
        public class MeSimpleEntitiesS
        {
            public MeSimpleEntitiesS()
            {
                MeComplexTypeS = new MeComplexTypeS();
                Children = new List<MeSimpleEntitiesS>();
            }

            public int Id { get; set; }
            public string Name { get; set; }
            public MeComplexTypeS MeComplexTypeS { get; set; }
            public DbGeometry Geometry { get; set; }
            public EnumType Enum { get; set; }
            public MeSimpleEntitiesS Parent { get; set; }
            public ICollection<MeSimpleEntitiesS> Children { get; set; }

            public enum EnumType
            {
                ZERO,
                ONE,
                TWO,
                THREE,
            }
        }
        
        [Serializable]
        public class MeComplexTypeS
        {
            public int Number { get; set; }
            public string Word { get; set; }
        }

        [Fact]
        public void Change_tracking_proxy_can_be_binary_deserialized_when_running_under_full_trust()
        {
            using (var context = new ProxiesContext())
            {
                var proxy = context.MeTrackChanges.Create();
                Assert.True(proxy is IEntityWithRelationships);

                proxy.Id = 77;
                proxy.MeComplexTypeS.Number = 88;

                var deserialized = DeserializeFromBinaryFormatter(proxy); 

                Assert.Same(proxy.GetType(), deserialized.GetType());
                Assert.Equal(77, deserialized.Id);
                Assert.Equal(88, deserialized.MeComplexTypeS.Number);
            }
        }

        [Fact]
        public void Lazy_loading_proxy_can_be_binary_deserialized_when_running_under_full_trust()
        {
            using (var context = new ProxiesContext())
            {
                var proxy = context.MeLazyLoads.Create();
                Assert.False(proxy is IEntityWithRelationships);

                proxy.Id = 77;
                proxy.MeComplexTypeS.Number = 88;

                var deserialized = DeserializeFromBinaryFormatter(proxy);

                Assert.Same(proxy.GetType(), deserialized.GetType());
                Assert.Equal(77, deserialized.Id);
                Assert.Equal(88, deserialized.MeComplexTypeS.Number);
            }
        }

        [Fact]
        public void Change_tracking_proxy_can_be_data_contract_deserialized_with_resolver_when_running_under_full_trust()
        {
            using (var context = new ProxiesContext())
            {
                var proxy = context.MeTrackChanges.Create();
                Assert.True(proxy is IEntityWithRelationships);

                proxy.Id = 77;
                proxy.MeComplexTypeS.Number = 88;

                var serializer = new DataContractSerializer(
                    typeof(MeTrackChangesS), null, int.MaxValue, false, true, null, new ProxyDataContractResolver());
                var deserialized = DeserializeWithDatacontractSerializer(proxy, serializer); 

                // Resolver returns non-proxy type
                Assert.IsType<MeTrackChangesS>(deserialized); 
                Assert.Equal(77, deserialized.Id);
                Assert.Equal(88, deserialized.MeComplexTypeS.Number);
            }
        }

        [Fact]
        public void Lazy_loading_proxy_can_be_data_contract_deserialized_with_resolver_when_running_under_full_trust()
        {
            using (var context = new ProxiesContext())
            {
                var proxy = context.MeLazyLoads.Create();
                Assert.False(proxy is IEntityWithRelationships);

                proxy.Id = 77;
                proxy.MeComplexTypeS.Number = 88;

                var serializer = new DataContractSerializer(
                    typeof(MeLazyLoadS), null, int.MaxValue, false, true, null, new ProxyDataContractResolver());
                var deserialized = DeserializeWithDatacontractSerializer(proxy, serializer);

                // Resolver returns non-proxy type
                Assert.IsType<MeLazyLoadS>(deserialized); 
                Assert.Equal(77, deserialized.Id);
                Assert.Equal(88, deserialized.MeComplexTypeS.Number);
            }
        }

        [Fact]
        public void Lazy_loading_proxy_can_be_data_contract_deserialized_with_known_types_when_running_under_full_trust()
        {
            using (var context = new ProxiesContext())
            {
                var proxy = context.MeLazyLoads.Create();
                var otherProxy = context.MeTrackChanges.Create();

                Assert.False(proxy is IEntityWithRelationships);
                proxy.Id = 77;
                proxy.MeComplexTypeS.Number = 88;
                
                var serializer = new DataContractSerializer(
                    proxy.GetType(), new[] { proxy.GetType(), otherProxy.GetType() }, int.MaxValue, false, true, null);
                var deserialized = DeserializeWithDatacontractSerializer(proxy, serializer);

                Assert.Same(proxy.GetType(), deserialized.GetType());
                Assert.Equal(77, deserialized.Id);
                Assert.Equal(88, deserialized.MeComplexTypeS.Number);
            }
        }

        [Fact]
        public void Simple_entities_can_be_binary_serialized_when_running_under_full_trust()
        {
            using (var context = new ProxiesContext())
            {
                var proxy = context.MeSimpleEntities.Create();
                Assert.False(proxy is IEntityWithRelationships);

                proxy.Id = 77;
                proxy.Name = "Entity";
                proxy.MeComplexTypeS.Number = 88;
                proxy.Geometry = DbGeometry.FromText("POINT (30 10)");
                proxy.Enum = MeSimpleEntitiesS.EnumType.ZERO;

                var deserialized = DeserializeFromBinaryFormatter(proxy);

                Assert.Same(proxy.GetType(), deserialized.GetType());
                Assert.Equal(77, deserialized.Id);
                Assert.Equal("Entity", deserialized.Name);
                Assert.Equal(88, deserialized.MeComplexTypeS.Number);
                Assert.Equal(DbGeometry.FromText("POINT (30 10)").AsText(), deserialized.Geometry.AsText());
                Assert.Equal(MeSimpleEntitiesS.EnumType.ZERO, deserialized.Enum);
            }
        }

        [Fact]
        public void Simple_entities_can_be_binary_serialized_with_non_existent_enum_values()
        {
            using (var context = new ProxiesContext())
            {
                var proxy = context.MeSimpleEntities.Create();
                Assert.False(proxy is IEntityWithRelationships);

                proxy.Id = 77;
                proxy.Enum = (MeSimpleEntitiesS.EnumType) 7;

                var deserialized = DeserializeFromBinaryFormatter(proxy);

                Assert.Same(proxy.GetType(), deserialized.GetType());
                Assert.Equal(77, deserialized.Id);
                Assert.Equal((MeSimpleEntitiesS.EnumType)7, deserialized.Enum);
            }
        }

        [Fact]
        public void Simple_entities_can_be_data_contract_deserialized_with_resolver_when_running_under_full_trust()
        {
            using (var context = new ProxiesContext())
            {
                var proxy = context.MeSimpleEntities.Create();
                Assert.False(proxy is IEntityWithRelationships);

                proxy.Id = 77;
                proxy.Name = "Entity";
                proxy.MeComplexTypeS.Number = 88;
                proxy.Geometry = DbGeometry.FromText("POINT (30 10)");
                proxy.Enum = MeSimpleEntitiesS.EnumType.ZERO;

                var serializer = new DataContractSerializer(
                    typeof(MeSimpleEntitiesS), null, int.MaxValue, false, true, null, new ProxyDataContractResolver());
                var deserialized = DeserializeWithDatacontractSerializer(proxy, serializer);

                // Resolver returns non-proxy type
                Assert.IsType<MeSimpleEntitiesS>(deserialized); 
                Assert.Equal(77, deserialized.Id);
                Assert.Equal("Entity", deserialized.Name);
                Assert.Equal(88, deserialized.MeComplexTypeS.Number);
                Assert.Equal(DbGeometry.FromText("POINT (30 10)").AsText(), deserialized.Geometry.AsText());
                Assert.Equal(MeSimpleEntitiesS.EnumType.ZERO, deserialized.Enum);
            }
        }

        [Fact]
        public void Stored_change_tracking_proxy_can_be_binary_deserialized_when_running_under_full_trust()
        {
            using (var context = new ProxiesContext())
            {
                // needed to create the dynamically generated proxy
                var proxy = context.MeTrackChanges.Create();
                var base64String = "AAEAAAD/////AQAAAAAAAAAMAgAAAHRFbnRpdHlGcmFtZXdvcmtEeW5hbWljUHJveGllcy1FbnRpdHlGcmFtZXdvcmsuRnVuY3Rpb25hbFRlc3RzLCBWZXJzaW9uPTEuMC4wLjAsIEN1bHR1cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49bnVsbAwDAAAAUkVudGl0eUZyYW1ld29yaywgVmVyc2lvbj02LjAuMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPWI3N2E1YzU2MTkzNGUwODkMBAAAAGJFbnRpdHlGcmFtZXdvcmsuRnVuY3Rpb25hbFRlc3RzLCBWZXJzaW9uPTAuMC4wLjAsIEN1bHR1cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49YjAzZjVmN2YxMWQ1MGEzYQUBAAAAclN5c3RlbS5EYXRhLkVudGl0eS5EeW5hbWljUHJveGllcy5NZVRyYWNrQ2hhbmdlc1NfNzhCN0FGQUYyNjkzREQ3QzM3MzlDNDU0QjkxMDA4Rjk4NkVEREY1QjdDRjlDNDVFN0U5MzUyMzZDNzc4OTVBRAYAAAAUX3JlbGF0aW9uc2hpcE1hbmFnZXIjTWVUcmFja0NoYW5nZXNTKzxJZD5rX19CYWNraW5nRmllbGQrTWVUcmFja0NoYW5nZXNTKzxNZUxhenlMb2FkPmtfX0JhY2tpbmdGaWVsZC9NZVRyYWNrQ2hhbmdlc1MrPE1lQ29tcGxleFR5cGVTPmtfX0JhY2tpbmdGaWVsZCdNZVRyYWNrQ2hhbmdlc1MrPFBhcmVudD5rX19CYWNraW5nRmllbGQpTWVUcmFja0NoYW5nZXNTKzxDaGlsZHJlbj5rX19CYWNraW5nRmllbGQEAAQEBAQ/U3lzdGVtLkRhdGEuRW50aXR5LkNvcmUuT2JqZWN0cy5EYXRhQ2xhc3Nlcy5SZWxhdGlvbnNoaXBNYW5hZ2VyAwAAAAjjAVN5c3RlbS5EYXRhLkVudGl0eS5Db3JlLk9iamVjdHMuRGF0YUNsYXNzZXMuRW50aXR5Q29sbGVjdGlvbmAxW1tTeXN0ZW0uRGF0YS5FbnRpdHkuT2JqZWN0cy5TZXJpYWxpemF0aW9uU2NlbmFyaW9zK01lTGF6eUxvYWRTLCBFbnRpdHlGcmFtZXdvcmsuRnVuY3Rpb25hbFRlc3RzLCBWZXJzaW9uPTAuMC4wLjAsIEN1bHR1cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49YjAzZjVmN2YxMWQ1MGEzYV1dAwAAAEBTeXN0ZW0uRGF0YS5FbnRpdHkuT2JqZWN0cy5TZXJpYWxpemF0aW9uU2NlbmFyaW9zK01lQ29tcGxleFR5cGVTBAAAAEFTeXN0ZW0uRGF0YS5FbnRpdHkuT2JqZWN0cy5TZXJpYWxpemF0aW9uU2NlbmFyaW9zK01lVHJhY2tDaGFuZ2VzUwQAAADnAVN5c3RlbS5EYXRhLkVudGl0eS5Db3JlLk9iamVjdHMuRGF0YUNsYXNzZXMuRW50aXR5Q29sbGVjdGlvbmAxW1tTeXN0ZW0uRGF0YS5FbnRpdHkuT2JqZWN0cy5TZXJpYWxpemF0aW9uU2NlbmFyaW9zK01lVHJhY2tDaGFuZ2VzUywgRW50aXR5RnJhbWV3b3JrLkZ1bmN0aW9uYWxUZXN0cywgVmVyc2lvbj0wLjAuMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPWIwM2Y1ZjdmMTFkNTBhM2FdXQMAAAACAAAACQUAAABNAAAACQYAAAAJBwAAAAoJCAAAAAUFAAAAP1N5c3RlbS5EYXRhLkVudGl0eS5Db3JlLk9iamVjdHMuRGF0YUNsYXNzZXMuUmVsYXRpb25zaGlwTWFuYWdlcgIAAAAGX293bmVyDl9yZWxhdGlvbnNoaXBzBANyU3lzdGVtLkRhdGEuRW50aXR5LkR5bmFtaWNQcm94aWVzLk1lVHJhY2tDaGFuZ2VzU183OEI3QUZBRjI2OTNERDdDMzczOUM0NTRCOTEwMDhGOTg2RURERjVCN0NGOUM0NUU3RTkzNTIzNkM3Nzg5NUFEAgAAAK8BU3lzdGVtLkNvbGxlY3Rpb25zLkdlbmVyaWMuTGlzdGAxW1tTeXN0ZW0uRGF0YS5FbnRpdHkuQ29yZS5PYmplY3RzLkRhdGFDbGFzc2VzLlJlbGF0ZWRFbmQsIEVudGl0eUZyYW1ld29yaywgVmVyc2lvbj02LjAuMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPWI3N2E1YzU2MTkzNGUwODldXQMAAAAJAQAAAAkKAAAADAsAAABOU3lzdGVtLkNvcmUsIFZlcnNpb249NC4wLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1iNzdhNWM1NjE5MzRlMDg5BQYAAADjAVN5c3RlbS5EYXRhLkVudGl0eS5Db3JlLk9iamVjdHMuRGF0YUNsYXNzZXMuRW50aXR5Q29sbGVjdGlvbmAxW1tTeXN0ZW0uRGF0YS5FbnRpdHkuT2JqZWN0cy5TZXJpYWxpemF0aW9uU2NlbmFyaW9zK01lTGF6eUxvYWRTLCBFbnRpdHlGcmFtZXdvcmsuRnVuY3Rpb25hbFRlc3RzLCBWZXJzaW9uPTAuMC4wLjAsIEN1bHR1cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49YjAzZjVmN2YxMWQ1MGEzYV1dBgAAABBfcmVsYXRlZEVudGl0aWVzCV9pc0xvYWRlZBFSZWxhdGVkRW5kK19vd25lchZSZWxhdGVkRW5kK19uYXZpZ2F0aW9uHVJlbGF0ZWRFbmQrX3JlbGF0aW9uc2hpcEZpeGVyFFJlbGF0ZWRFbmQrX2lzTG9hZGVkBAAEBAQAyQFTeXN0ZW0uQ29sbGVjdGlvbnMuR2VuZXJpYy5IYXNoU2V0YDFbW1N5c3RlbS5EYXRhLkVudGl0eS5PYmplY3RzLlNlcmlhbGl6YXRpb25TY2VuYXJpb3MrTWVMYXp5TG9hZFMsIEVudGl0eUZyYW1ld29yay5GdW5jdGlvbmFsVGVzdHMsIFZlcnNpb249MC4wLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1iMDNmNWY3ZjExZDUwYTNhXV0LAAAAAXJTeXN0ZW0uRGF0YS5FbnRpdHkuRHluYW1pY1Byb3hpZXMuTWVUcmFja0NoYW5nZXNTXzc4QjdBRkFGMjY5M0REN0MzNzM5QzQ1NEI5MTAwOEY5ODZFRERGNUI3Q0Y5QzQ1RTdFOTM1MjM2Qzc3ODk1QUQCAAAAQlN5c3RlbS5EYXRhLkVudGl0eS5Db3JlLk9iamVjdHMuRGF0YUNsYXNzZXMuUmVsYXRpb25zaGlwTmF2aWdhdGlvbgMAAACMA1N5c3RlbS5EYXRhLkVudGl0eS5Db3JlLk9iamVjdHMuRGF0YUNsYXNzZXMuUmVsYXRpb25zaGlwRml4ZXJgMltbU3lzdGVtLkRhdGEuRW50aXR5Lk9iamVjdHMuU2VyaWFsaXphdGlvblNjZW5hcmlvcytNZVRyYWNrQ2hhbmdlc1MsIEVudGl0eUZyYW1ld29yay5GdW5jdGlvbmFsVGVzdHMsIFZlcnNpb249MC4wLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1iMDNmNWY3ZjExZDUwYTNhXSxbU3lzdGVtLkRhdGEuRW50aXR5Lk9iamVjdHMuU2VyaWFsaXphdGlvblNjZW5hcmlvcytNZUxhenlMb2FkUywgRW50aXR5RnJhbWV3b3JrLkZ1bmN0aW9uYWxUZXN0cywgVmVyc2lvbj0wLjAuMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPWIwM2Y1ZjdmMTFkNTBhM2FdXQMAAAABAwAAAAoACQEAAAAJDQAAAAkOAAAAAAUHAAAAQFN5c3RlbS5EYXRhLkVudGl0eS5PYmplY3RzLlNlcmlhbGl6YXRpb25TY2VuYXJpb3MrTWVDb21wbGV4VHlwZVMCAAAAFzxOdW1iZXI+a19fQmFja2luZ0ZpZWxkFTxXb3JkPmtfX0JhY2tpbmdGaWVsZAABCAQAAABYAAAACgUIAAAA5wFTeXN0ZW0uRGF0YS5FbnRpdHkuQ29yZS5PYmplY3RzLkRhdGFDbGFzc2VzLkVudGl0eUNvbGxlY3Rpb25gMVtbU3lzdGVtLkRhdGEuRW50aXR5Lk9iamVjdHMuU2VyaWFsaXphdGlvblNjZW5hcmlvcytNZVRyYWNrQ2hhbmdlc1MsIEVudGl0eUZyYW1ld29yay5GdW5jdGlvbmFsVGVzdHMsIFZlcnNpb249MC4wLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1iMDNmNWY3ZjExZDUwYTNhXV0GAAAAEF9yZWxhdGVkRW50aXRpZXMJX2lzTG9hZGVkEVJlbGF0ZWRFbmQrX293bmVyFlJlbGF0ZWRFbmQrX25hdmlnYXRpb24dUmVsYXRlZEVuZCtfcmVsYXRpb25zaGlwRml4ZXIUUmVsYXRlZEVuZCtfaXNMb2FkZWQEAAQEBADNAVN5c3RlbS5Db2xsZWN0aW9ucy5HZW5lcmljLkhhc2hTZXRgMVtbU3lzdGVtLkRhdGEuRW50aXR5Lk9iamVjdHMuU2VyaWFsaXphdGlvblNjZW5hcmlvcytNZVRyYWNrQ2hhbmdlc1MsIEVudGl0eUZyYW1ld29yay5GdW5jdGlvbmFsVGVzdHMsIFZlcnNpb249MC4wLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1iMDNmNWY3ZjExZDUwYTNhXV0LAAAAAXJTeXN0ZW0uRGF0YS5FbnRpdHkuRHluYW1pY1Byb3hpZXMuTWVUcmFja0NoYW5nZXNTXzc4QjdBRkFGMjY5M0REN0MzNzM5QzQ1NEI5MTAwOEY5ODZFRERGNUI3Q0Y5QzQ1RTdFOTM1MjM2Qzc3ODk1QUQCAAAAQlN5c3RlbS5EYXRhLkVudGl0eS5Db3JlLk9iamVjdHMuRGF0YUNsYXNzZXMuUmVsYXRpb25zaGlwTmF2aWdhdGlvbgMAAACQA1N5c3RlbS5EYXRhLkVudGl0eS5Db3JlLk9iamVjdHMuRGF0YUNsYXNzZXMuUmVsYXRpb25zaGlwRml4ZXJgMltbU3lzdGVtLkRhdGEuRW50aXR5Lk9iamVjdHMuU2VyaWFsaXphdGlvblNjZW5hcmlvcytNZVRyYWNrQ2hhbmdlc1MsIEVudGl0eUZyYW1ld29yay5GdW5jdGlvbmFsVGVzdHMsIFZlcnNpb249MC4wLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1iMDNmNWY3ZjExZDUwYTNhXSxbU3lzdGVtLkRhdGEuRW50aXR5Lk9iamVjdHMuU2VyaWFsaXphdGlvblNjZW5hcmlvcytNZVRyYWNrQ2hhbmdlc1MsIEVudGl0eUZyYW1ld29yay5GdW5jdGlvbmFsVGVzdHMsIFZlcnNpb249MC4wLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1iMDNmNWY3ZjExZDUwYTNhXV0DAAAAAQMAAAAKAAkBAAAACRAAAAAJEQAAAAAECgAAAK8BU3lzdGVtLkNvbGxlY3Rpb25zLkdlbmVyaWMuTGlzdGAxW1tTeXN0ZW0uRGF0YS5FbnRpdHkuQ29yZS5PYmplY3RzLkRhdGFDbGFzc2VzLlJlbGF0ZWRFbmQsIEVudGl0eUZyYW1ld29yaywgVmVyc2lvbj02LjAuMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPWI3N2E1YzU2MTkzNGUwODldXQMAAAAGX2l0ZW1zBV9zaXplCF92ZXJzaW9uBAAAOFN5c3RlbS5EYXRhLkVudGl0eS5Db3JlLk9iamVjdHMuRGF0YUNsYXNzZXMuUmVsYXRlZEVuZFtdAwAAAAgICRIAAAACAAAAAgAAAAUNAAAAQlN5c3RlbS5EYXRhLkVudGl0eS5Db3JlLk9iamVjdHMuRGF0YUNsYXNzZXMuUmVsYXRpb25zaGlwTmF2aWdhdGlvbgMAAAARX3JlbGF0aW9uc2hpcE5hbWUFX2Zyb20DX3RvAQEBAwAAAAYTAAAANVN5c3RlbS5EYXRhLkVudGl0eS5PYmplY3RzLk1lVHJhY2tDaGFuZ2VzU19NZUxhenlMb2FkBhQAAAAhTWVUcmFja0NoYW5nZXNTX01lTGF6eUxvYWRfU291cmNlBhUAAAAhTWVUcmFja0NoYW5nZXNTX01lTGF6eUxvYWRfVGFyZ2V0BQ4AAACMA1N5c3RlbS5EYXRhLkVudGl0eS5Db3JlLk9iamVjdHMuRGF0YUNsYXNzZXMuUmVsYXRpb25zaGlwRml4ZXJgMltbU3lzdGVtLkRhdGEuRW50aXR5Lk9iamVjdHMuU2VyaWFsaXphdGlvblNjZW5hcmlvcytNZVRyYWNrQ2hhbmdlc1MsIEVudGl0eUZyYW1ld29yay5GdW5jdGlvbmFsVGVzdHMsIFZlcnNpb249MC4wLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1iMDNmNWY3ZjExZDUwYTNhXSxbU3lzdGVtLkRhdGEuRW50aXR5Lk9iamVjdHMuU2VyaWFsaXphdGlvblNjZW5hcmlvcytNZUxhenlMb2FkUywgRW50aXR5RnJhbWV3b3JrLkZ1bmN0aW9uYWxUZXN0cywgVmVyc2lvbj0wLjAuMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPWIwM2Y1ZjdmMTFkNTBhM2FdXQIAAAAXX3NvdXJjZVJvbGVNdWx0aXBsaWNpdHkXX3RhcmdldFJvbGVNdWx0aXBsaWNpdHkEBD1TeXN0ZW0uRGF0YS5FbnRpdHkuQ29yZS5NZXRhZGF0YS5FZG0uUmVsYXRpb25zaGlwTXVsdGlwbGljaXR5AwAAAD1TeXN0ZW0uRGF0YS5FbnRpdHkuQ29yZS5NZXRhZGF0YS5FZG0uUmVsYXRpb25zaGlwTXVsdGlwbGljaXR5AwAAAAMAAAAF6v///z1TeXN0ZW0uRGF0YS5FbnRpdHkuQ29yZS5NZXRhZGF0YS5FZG0uUmVsYXRpb25zaGlwTXVsdGlwbGljaXR5AQAAAAd2YWx1ZV9fAAgDAAAAAAAAAAHp////6v///wIAAAABEAAAAA0AAAAGGAAAADNTeXN0ZW0uRGF0YS5FbnRpdHkuT2JqZWN0cy5NZVRyYWNrQ2hhbmdlc1NfQ2hpbGRyZW4GGQAAAB9NZVRyYWNrQ2hhbmdlc1NfQ2hpbGRyZW5fU291cmNlBhoAAAAfTWVUcmFja0NoYW5nZXNTX0NoaWxkcmVuX1RhcmdldAURAAAAkANTeXN0ZW0uRGF0YS5FbnRpdHkuQ29yZS5PYmplY3RzLkRhdGFDbGFzc2VzLlJlbGF0aW9uc2hpcEZpeGVyYDJbW1N5c3RlbS5EYXRhLkVudGl0eS5PYmplY3RzLlNlcmlhbGl6YXRpb25TY2VuYXJpb3MrTWVUcmFja0NoYW5nZXNTLCBFbnRpdHlGcmFtZXdvcmsuRnVuY3Rpb25hbFRlc3RzLCBWZXJzaW9uPTAuMC4wLjAsIEN1bHR1cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49YjAzZjVmN2YxMWQ1MGEzYV0sW1N5c3RlbS5EYXRhLkVudGl0eS5PYmplY3RzLlNlcmlhbGl6YXRpb25TY2VuYXJpb3MrTWVUcmFja0NoYW5nZXNTLCBFbnRpdHlGcmFtZXdvcmsuRnVuY3Rpb25hbFRlc3RzLCBWZXJzaW9uPTAuMC4wLjAsIEN1bHR1cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49YjAzZjVmN2YxMWQ1MGEzYV1dAgAAABdfc291cmNlUm9sZU11bHRpcGxpY2l0eRdfdGFyZ2V0Um9sZU11bHRpcGxpY2l0eQQEPVN5c3RlbS5EYXRhLkVudGl0eS5Db3JlLk1ldGFkYXRhLkVkbS5SZWxhdGlvbnNoaXBNdWx0aXBsaWNpdHkDAAAAPVN5c3RlbS5EYXRhLkVudGl0eS5Db3JlLk1ldGFkYXRhLkVkbS5SZWxhdGlvbnNoaXBNdWx0aXBsaWNpdHkDAAAAAwAAAAHl////6v///wAAAAAB5P///+r///8CAAAABxIAAAAAAQAAAAQAAAAENlN5c3RlbS5EYXRhLkVudGl0eS5Db3JlLk9iamVjdHMuRGF0YUNsYXNzZXMuUmVsYXRlZEVuZAMAAAAJCAAAAAkGAAAADQIL";
                var deserialized = DeserializeStringWithFormatter<MeTrackChangesS>(base64String);

                Assert.Same(proxy.GetType(), deserialized.GetType());
                Assert.Equal(77, deserialized.Id);
                Assert.Equal(88, deserialized.MeComplexTypeS.Number);
            }
        }

        [Fact]
        public void Stored_lazy_loading_proxy_can_be_binary_deserialized_when_running_under_full_trust()
        {
            using (var context = new ProxiesContext())
            {
                // needed to create the dynamically generated proxy
                var proxy = context.MeLazyLoads.Create(); 
                var base64String = "AAEAAAD/////AQAAAAAAAAAMAgAAAHRFbnRpdHlGcmFtZXdvcmtEeW5hbWljUHJveGllcy1FbnRpdHlGcmFtZXdvcmsuRnVuY3Rpb25hbFRlc3RzLCBWZXJzaW9uPTEuMC4wLjAsIEN1bHR1cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49bnVsbAwDAAAAYkVudGl0eUZyYW1ld29yay5GdW5jdGlvbmFsVGVzdHMsIFZlcnNpb249MC4wLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1iMDNmNWY3ZjExZDUwYTNhBQEAAABuU3lzdGVtLkRhdGEuRW50aXR5LkR5bmFtaWNQcm94aWVzLk1lTGF6eUxvYWRTX0UwNjM1MjMzMUNBMTk4MTg0NkEzNDkwQkUwODg2NTU3QjFFNjUzQzhDM0JEQjJFOEM0N0Y2MzA0OEM1OUQ3OUMDAAAAH01lTGF6eUxvYWRTKzxJZD5rX19CYWNraW5nRmllbGQrTWVMYXp5TG9hZFMrPE1lVHJhY2tDaGFuZ2VzPmtfX0JhY2tpbmdGaWVsZCtNZUxhenlMb2FkUys8TWVDb21wbGV4VHlwZVM+a19fQmFja2luZ0ZpZWxkAAQECEFTeXN0ZW0uRGF0YS5FbnRpdHkuT2JqZWN0cy5TZXJpYWxpemF0aW9uU2NlbmFyaW9zK01lVHJhY2tDaGFuZ2VzUwMAAABAU3lzdGVtLkRhdGEuRW50aXR5Lk9iamVjdHMuU2VyaWFsaXphdGlvblNjZW5hcmlvcytNZUNvbXBsZXhUeXBlUwMAAAACAAAATQAAAAoJBAAAAAUEAAAAQFN5c3RlbS5EYXRhLkVudGl0eS5PYmplY3RzLlNlcmlhbGl6YXRpb25TY2VuYXJpb3MrTWVDb21wbGV4VHlwZVMCAAAAFzxOdW1iZXI+a19fQmFja2luZ0ZpZWxkFTxXb3JkPmtfX0JhY2tpbmdGaWVsZAABCAMAAABYAAAACgs=";
                var deserialized = DeserializeStringWithFormatter<MeLazyLoadS>(base64String);

                Assert.Same(proxy.GetType(), deserialized.GetType());
                Assert.Equal(77, deserialized.Id);
                Assert.Equal(88, deserialized.MeComplexTypeS.Number);
            }
        }


        
        [Fact]
        public void Stored_change_tracking_proxy_can_be_data_contract_deserialized_with_resolver_when_running_under_full_trust()
        {
            var serializer = new DataContractSerializer(
                typeof(MeTrackChangesS), null, int.MaxValue, false, true, null, new ProxyDataContractResolver());
            var base64String = "PFNlcmlhbGl6YXRpb25TY2VuYXJpb3MuTWVUcmFja0NoYW5nZXNTIHo6SWQ9IjEiIGk6dHlwZT0iU2VyaWFsaXphdGlvblNjZW5hcmlvcy5NZVRyYWNrQ2hhbmdlc1MiIHhtbG5zPSJodHRwOi8vc2NoZW1hcy5kYXRhY29udHJhY3Qub3JnLzIwMDQvMDcvU3lzdGVtLkRhdGEuRW50aXR5Lk9iamVjdHMiIHhtbG5zOmk9Imh0dHA6Ly93d3cudzMub3JnLzIwMDEvWE1MU2NoZW1hLWluc3RhbmNlIiB4bWxuczp6PSJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tLzIwMDMvMTAvU2VyaWFsaXphdGlvbi8iPjxfeDAwM0NfQ2hpbGRyZW5feDAwM0Vfa19fQmFja2luZ0ZpZWxkIHo6SWQ9IjIiIHo6U2l6ZT0iMCIvPjxfeDAwM0NfSWRfeDAwM0Vfa19fQmFja2luZ0ZpZWxkPjc3PC9feDAwM0NfSWRfeDAwM0Vfa19fQmFja2luZ0ZpZWxkPjxfeDAwM0NfTWVDb21wbGV4VHlwZVNfeDAwM0Vfa19fQmFja2luZ0ZpZWxkIHo6SWQ9IjMiPjxfeDAwM0NfTnVtYmVyX3gwMDNFX2tfX0JhY2tpbmdGaWVsZD44ODwvX3gwMDNDX051bWJlcl94MDAzRV9rX19CYWNraW5nRmllbGQ+PF94MDAzQ19Xb3JkX3gwMDNFX2tfX0JhY2tpbmdGaWVsZCBpOm5pbD0idHJ1ZSIvPjwvX3gwMDNDX01lQ29tcGxleFR5cGVTX3gwMDNFX2tfX0JhY2tpbmdGaWVsZD48X3gwMDNDX01lTGF6eUxvYWRfeDAwM0Vfa19fQmFja2luZ0ZpZWxkIHo6SWQ9IjQiIHo6U2l6ZT0iMCIvPjxfeDAwM0NfUGFyZW50X3gwMDNFX2tfX0JhY2tpbmdGaWVsZCBpOm5pbD0idHJ1ZSIvPjxfcmVsYXRpb25zaGlwTWFuYWdlciB6OklkPSI1IiB4bWxucz0iaHR0cDovL3NjaGVtYXMuZGF0YWNvbnRyYWN0Lm9yZy8yMDA0LzA3L1N5c3RlbS5EYXRhLkVudGl0eS5EeW5hbWljUHJveGllcyIgeG1sbnM6YT0iaHR0cDovL3NjaGVtYXMuZGF0YWNvbnRyYWN0Lm9yZy8yMDA0LzA3L1N5c3RlbS5EYXRhLkVudGl0eS5Db3JlLk9iamVjdHMuRGF0YUNsYXNzZXMiPjxhOl9vd25lciB6OlJlZj0iMSIgaTpuaWw9InRydWUiLz48YTpfcmVsYXRpb25zaGlwcyB6OklkPSI2IiB6OlNpemU9IjIiPjxhOlJlbGF0ZWRFbmQgejpSZWY9IjQiIGk6bmlsPSJ0cnVlIi8+PGE6UmVsYXRlZEVuZCB6OlJlZj0iMiIgaTpuaWw9InRydWUiLz48L2E6X3JlbGF0aW9uc2hpcHM+PC9fcmVsYXRpb25zaGlwTWFuYWdlcj48L1NlcmlhbGl6YXRpb25TY2VuYXJpb3MuTWVUcmFja0NoYW5nZXNTPg==";
            var deserialized = DeserializeStringWithDatacontractSerializer<MeTrackChangesS>(base64String, serializer);            

            // Resolver returns non-proxy type
            Assert.IsType<MeTrackChangesS>(deserialized); 
            Assert.Equal(77, deserialized.Id);
            Assert.Equal(88, deserialized.MeComplexTypeS.Number);
        }

        [Fact]
        public void Stored_lazy_loading_proxy_can_be_data_contract_deserialized_with_resolver_when_running_under_full_trust()
        {
            var serializer = new DataContractSerializer(
                typeof(MeLazyLoadS), null, int.MaxValue, false, true, null, new ProxyDataContractResolver());
            var base64String = "PFNlcmlhbGl6YXRpb25TY2VuYXJpb3MuTWVMYXp5TG9hZFMgejpJZD0iMSIgaTp0eXBlPSJTZXJpYWxpemF0aW9uU2NlbmFyaW9zLk1lTGF6eUxvYWRTIiB4bWxucz0iaHR0cDovL3NjaGVtYXMuZGF0YWNvbnRyYWN0Lm9yZy8yMDA0LzA3L1N5c3RlbS5EYXRhLkVudGl0eS5PYmplY3RzIiB4bWxuczppPSJodHRwOi8vd3d3LnczLm9yZy8yMDAxL1hNTFNjaGVtYS1pbnN0YW5jZSIgeG1sbnM6ej0iaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS8yMDAzLzEwL1NlcmlhbGl6YXRpb24vIj48X3gwMDNDX0lkX3gwMDNFX2tfX0JhY2tpbmdGaWVsZD43NzwvX3gwMDNDX0lkX3gwMDNFX2tfX0JhY2tpbmdGaWVsZD48X3gwMDNDX01lQ29tcGxleFR5cGVTX3gwMDNFX2tfX0JhY2tpbmdGaWVsZCB6OklkPSIyIj48X3gwMDNDX051bWJlcl94MDAzRV9rX19CYWNraW5nRmllbGQ+ODg8L194MDAzQ19OdW1iZXJfeDAwM0Vfa19fQmFja2luZ0ZpZWxkPjxfeDAwM0NfV29yZF94MDAzRV9rX19CYWNraW5nRmllbGQgaTpuaWw9InRydWUiLz48L194MDAzQ19NZUNvbXBsZXhUeXBlU194MDAzRV9rX19CYWNraW5nRmllbGQ+PF94MDAzQ19NZVRyYWNrQ2hhbmdlc194MDAzRV9rX19CYWNraW5nRmllbGQgaTpuaWw9InRydWUiLz48L1NlcmlhbGl6YXRpb25TY2VuYXJpb3MuTWVMYXp5TG9hZFM+";
            var deserialized = DeserializeStringWithDatacontractSerializer<MeLazyLoadS>(base64String, serializer);

            // Resolver returns non-proxy type
            Assert.IsType<MeLazyLoadS>(deserialized); 
            Assert.Equal(77, deserialized.Id);
            Assert.Equal(88, deserialized.MeComplexTypeS.Number);
        }

        [Fact]
        public void Stored_lazy_loading_proxy_can_be_data_contract_deserialized_with_known_types_when_running_under_full_trust()
        {
            using (var context = new ProxiesContext())
            {
                var proxy = context.MeLazyLoads.Create();
                var otherProxy = context.MeTrackChanges.Create();
                var serializer = new DataContractSerializer(proxy.GetType(), new[] { proxy.GetType(), otherProxy.GetType() }, int.MaxValue, false, true, null);
                var base64String = "PE1lTGF6eUxvYWRTX0UwNjM1MjMzMUNBMTk4MTg0NkEzNDkwQkUwODg2NTU3QjFFNjUzQzhDM0JEQjJFOEM0N0Y2MzA0OEM1OUQ3OUMgejpJZD0iMSIgeG1sbnM9Imh0dHA6Ly9zY2hlbWFzLmRhdGFjb250cmFjdC5vcmcvMjAwNC8wNy9TeXN0ZW0uRGF0YS5FbnRpdHkuRHluYW1pY1Byb3hpZXMiIHhtbG5zOmk9Imh0dHA6Ly93d3cudzMub3JnLzIwMDEvWE1MU2NoZW1hLWluc3RhbmNlIiB4bWxuczp6PSJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tLzIwMDMvMTAvU2VyaWFsaXphdGlvbi8iPjxfeDAwM0NfSWRfeDAwM0Vfa19fQmFja2luZ0ZpZWxkIHhtbG5zPSJodHRwOi8vc2NoZW1hcy5kYXRhY29udHJhY3Qub3JnLzIwMDQvMDcvU3lzdGVtLkRhdGEuRW50aXR5Lk9iamVjdHMiPjc3PC9feDAwM0NfSWRfeDAwM0Vfa19fQmFja2luZ0ZpZWxkPjxfeDAwM0NfTWVDb21wbGV4VHlwZVNfeDAwM0Vfa19fQmFja2luZ0ZpZWxkIHo6SWQ9IjIiIHhtbG5zPSJodHRwOi8vc2NoZW1hcy5kYXRhY29udHJhY3Qub3JnLzIwMDQvMDcvU3lzdGVtLkRhdGEuRW50aXR5Lk9iamVjdHMiPjxfeDAwM0NfTnVtYmVyX3gwMDNFX2tfX0JhY2tpbmdGaWVsZD44ODwvX3gwMDNDX051bWJlcl94MDAzRV9rX19CYWNraW5nRmllbGQ+PF94MDAzQ19Xb3JkX3gwMDNFX2tfX0JhY2tpbmdGaWVsZCBpOm5pbD0idHJ1ZSIvPjwvX3gwMDNDX01lQ29tcGxleFR5cGVTX3gwMDNFX2tfX0JhY2tpbmdGaWVsZD48X3gwMDNDX01lVHJhY2tDaGFuZ2VzX3gwMDNFX2tfX0JhY2tpbmdGaWVsZCBpOm5pbD0idHJ1ZSIgeG1sbnM9Imh0dHA6Ly9zY2hlbWFzLmRhdGFjb250cmFjdC5vcmcvMjAwNC8wNy9TeXN0ZW0uRGF0YS5FbnRpdHkuT2JqZWN0cyIvPjwvTWVMYXp5TG9hZFNfRTA2MzUyMzMxQ0ExOTgxODQ2QTM0OTBCRTA4ODY1NTdCMUU2NTNDOEMzQkRCMkU4QzQ3RjYzMDQ4QzU5RDc5Qz4=";
                var deserialized = DeserializeStringWithDatacontractSerializer<MeLazyLoadS>(base64String, serializer);

                Assert.Same(proxy.GetType(), deserialized.GetType());
                Assert.Equal(77, deserialized.Id);
                Assert.Equal(88, deserialized.MeComplexTypeS.Number);
            }
        }

        [Fact]
        public void Graph_serialization_preserves_related_entities_deserialized_with_data_contract_deserializer()
        {
            using (var context = new ProxiesContext())
            {
                var proxy = context.MeLazyLoads.Create();
                var otherProxy = context.MeTrackChanges.Create();

                proxy.Id = 77;
                otherProxy.Id = 99;
                proxy.MeTrackChanges = otherProxy;
                
                var serializer = new DataContractSerializer(
                    typeof(MeLazyLoadS), null, int.MaxValue, false, true, null, new ProxyDataContractResolver());
                var deserialized = DeserializeWithDatacontractSerializer(proxy, serializer);

                // Resolver returns non-proxy type
                Assert.IsType<MeLazyLoadS>(deserialized); 
                Assert.Equal(77, deserialized.Id);
                Assert.IsType<MeTrackChangesS>(deserialized.MeTrackChanges);
                Assert.Equal(99, deserialized.MeTrackChanges.Id);                
            }
        }

        [Fact]
        public void Graph_serialization_preserves_related_entities_deserialized_with_binary_deserializer()
        {
            using (var context = new ProxiesContext())
            {
                var proxy = context.MeLazyLoads.Create();
                var otherProxy = context.MeTrackChanges.Create();               

                proxy.Id = 77;
                otherProxy.Id = 99;
                proxy.MeTrackChanges = otherProxy;

                var deserialized = DeserializeFromBinaryFormatter(proxy);

                Assert.Same(proxy.GetType(), deserialized.GetType());
                Assert.Equal(77, deserialized.Id);
                Assert.Same(otherProxy.GetType(), deserialized.MeTrackChanges.GetType());
                Assert.Equal(99, deserialized.MeTrackChanges.Id);     
            }
        }

        [Fact]
        public void Graph_serialization_works_with_change_tracking_proxies_containing_ICollections_and_cycles()
        {
            using (var context = new ProxiesContext())
            {
                var proxy = context.MeTrackChanges.Create();
                var otherProxy = context.MeLazyLoads.Create();

                proxy.Id = 77;
                otherProxy.Id = 99;
                proxy.MeLazyLoad.Add(otherProxy);
                otherProxy.MeTrackChanges = proxy;

                var deserialized = DeserializeFromBinaryFormatter(proxy);

                Assert.Same(proxy.GetType(), deserialized.GetType());
                Assert.Equal(77, deserialized.Id);
                Assert.Equal(1, deserialized.MeLazyLoad.Count);

                foreach (var meLazyLoadS in deserialized.MeLazyLoad)
                {
                    Assert.Same(otherProxy.GetType(), meLazyLoadS.GetType());
                    Assert.Equal(99, meLazyLoadS.Id);
                    Assert.Equal(proxy.Id, meLazyLoadS.MeTrackChanges.Id);
                }   
            }
        }

        [Fact]
        public void Graph_serialization_works_with_simple_entity_cycle()
        {
            using (var context = new ProxiesContext())
            {
                var children = new MeSimpleEntitiesS[2];
                MeSimpleEntitiesS[] deserializedChildren;
                var childProxy1 = context.MeSimpleEntities.Create();
                var childProxy2 = context.MeSimpleEntities.Create();
                var parentProxy = context.MeSimpleEntities.Create();                

                children[0] = childProxy1;
                children[1] = childProxy2;
                childProxy1.Id = 77;
                childProxy2.Id = 88;
                parentProxy.Id = 99;
                childProxy1.Parent = parentProxy;
                childProxy2.Parent = parentProxy;
                parentProxy.Children.Add(childProxy1);
                parentProxy.Children.Add(childProxy2);

                var deserialized = DeserializeFromBinaryFormatter(childProxy1);

                Assert.Same(childProxy1.GetType(), deserialized.GetType());
                Assert.Equal(77, deserialized.Id);
                Assert.Equal(99, deserialized.Parent.Id);
                Assert.Equal(2, deserialized.Parent.Children.Count);
                Assert.True(deserialized.Parent.Children.Contains(deserialized));
                
                deserializedChildren = deserialized.Parent.Children.ToArray();

                for (int i = 0; i < children.Length; i++)
                {                 
                    Assert.Same(children[i].GetType(), deserializedChildren[i].GetType());
                    Assert.Equal(children[i].Id, deserializedChildren[i].Id);
                    Assert.Equal(deserializedChildren[i].Parent, deserialized.Parent);
                } 
            }
        }

        [Fact]
        public void Graph_serialization_works_with_simple_entity_cycle_with_lazy_loading_diabled()
        {
            using (var context = new ProxiesContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                var children = new MeSimpleEntitiesS[2];
                MeSimpleEntitiesS[] deserializedChildren;
                var childProxy1 = context.MeSimpleEntities.Create();
                var childProxy2 = context.MeSimpleEntities.Create();
                var parentProxy = context.MeSimpleEntities.Create();                

                children[0] = childProxy1;
                children[1] = childProxy2;
                childProxy1.Id = 77;
                childProxy2.Id = 88;
                parentProxy.Id = 99;
                childProxy1.Parent = parentProxy;
                childProxy2.Parent = parentProxy;
                parentProxy.Children.Add(childProxy1);
                parentProxy.Children.Add(childProxy2);

                var deserialized = DeserializeFromBinaryFormatter(childProxy1);

                Assert.Same(childProxy1.GetType(), deserialized.GetType());
                Assert.Equal(77, deserialized.Id);
                Assert.Equal(99, deserialized.Parent.Id);
                Assert.Equal(2, deserialized.Parent.Children.Count);
                Assert.True(deserialized.Parent.Children.Contains(deserialized));
                
                deserializedChildren = deserialized.Parent.Children.ToArray();

                for (int i = 0; i < children.Length; i++)
                {                 
                    Assert.Same(children[i].GetType(), deserializedChildren[i].GetType());
                    Assert.Equal(children[i].Id, deserializedChildren[i].Id);
                    Assert.Equal(deserializedChildren[i].Parent, deserialized.Parent);
                } 
            }
        }

        [Fact]
        public void Graph_serialization_works_with_change_tracking_cycle()
        {
            using (var context = new ProxiesContext())
            {
                var children = new MeTrackChangesS[2];
                MeTrackChangesS[] deserializedChildren;
                var childProxy1 = context.MeTrackChanges.Create();
                var childProxy2 = context.MeTrackChanges.Create();
                var parentProxy = context.MeTrackChanges.Create();

                children[0] = childProxy1;
                children[1] = childProxy2;
                childProxy1.Id = 77;
                childProxy2.Id = 88;
                parentProxy.Id = 99;
                childProxy1.Parent = parentProxy;
                childProxy2.Parent = parentProxy;
                parentProxy.Children.Add(childProxy1);
                parentProxy.Children.Add(childProxy2);

                var deserialized = DeserializeFromBinaryFormatter(childProxy1);

                Assert.Same(childProxy1.GetType(), deserialized.GetType());
                Assert.Equal(77, deserialized.Id);
                Assert.Equal(99, deserialized.Parent.Id);
                Assert.Equal(2, deserialized.Parent.Children.Count);
                Assert.True(deserialized.Parent.Children.Contains(deserialized));

                deserializedChildren = deserialized.Parent.Children.ToArray();

                for (int i = 0; i < children.Length; i++)
                {
                    Assert.Same(children[i].GetType(), deserializedChildren[i].GetType());
                    Assert.Equal(children[i].Id, deserializedChildren[i].Id);
                    Assert.Equal(deserializedChildren[i].Parent, deserialized.Parent);
                } 
            }
        }
        
        [Fact]
        public void Graph_serialization_works_with_change_tracking_cycle_and_lazy_loading_disabled()
        {
            using (var context = new ProxiesContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                var children = new MeTrackChangesS[2];
                MeTrackChangesS[] deserializedChildren;
                var childProxy1 = context.MeTrackChanges.Create();
                var childProxy2 = context.MeTrackChanges.Create();
                var parentProxy = context.MeTrackChanges.Create();

                children[0] = childProxy1;
                children[1] = childProxy2;
                childProxy1.Id = 77;
                childProxy2.Id = 88;
                parentProxy.Id = 99;
                childProxy1.Parent = parentProxy;
                childProxy2.Parent = parentProxy;
                parentProxy.Children.Add(childProxy1);
                parentProxy.Children.Add(childProxy2);

                var deserialized = DeserializeFromBinaryFormatter(childProxy1);

                Assert.Same(childProxy1.GetType(), deserialized.GetType());
                Assert.Equal(77, deserialized.Id);
                Assert.Equal(99, deserialized.Parent.Id);
                Assert.Equal(2, deserialized.Parent.Children.Count);
                Assert.True(deserialized.Parent.Children.Contains(deserialized));

                deserializedChildren = deserialized.Parent.Children.ToArray();

                for (int i = 0; i < children.Length; i++)
                {
                    Assert.Same(children[i].GetType(), deserializedChildren[i].GetType());
                    Assert.Equal(children[i].Id, deserializedChildren[i].Id);
                    Assert.Equal(deserializedChildren[i].Parent, deserialized.Parent);
                } 
            }
        }

        //Helpers
        private MemoryStream BuildStreamFromBase64String(string base64String)
        {
            var stream = new MemoryStream();
            var binaryData = System.Convert.FromBase64String(base64String);

            stream.Write(binaryData, 0, binaryData.Length);
            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }

        private T DeserializeStringWithFormatter<T>(string base64String)
        {
            var formatter = new BinaryFormatter();
            var stream = BuildStreamFromBase64String(base64String);

            return (T)formatter.Deserialize(stream);           
        }
        
        private T DeserializeStringWithDatacontractSerializer<T>(string base64String, DataContractSerializer serializer)
        {
            var stream = BuildStreamFromBase64String(base64String);
            
            return (T)serializer.ReadObject(stream);
        }

        private string BuildStringFromStream(MemoryStream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            byte[] binaryData = new Byte[stream.Length];
            stream.Read(binaryData, 0, (int)stream.Length);
            string base64String = System.Convert.ToBase64String(binaryData, 0, binaryData.Length);
            stream.Seek(0, SeekOrigin.Begin);

            return base64String;
        }

        private T DeserializeFromBinaryFormatter<T>(T proxy)
        {
            var stream = new MemoryStream();
            var formatter = new BinaryFormatter();

            formatter.Serialize(stream, proxy);
            stream.Seek(0, SeekOrigin.Begin);

            return (T)formatter.Deserialize(stream);
        }

        private T DeserializeWithDatacontractSerializer<T>(T proxy, DataContractSerializer serializer)
        {
            var stream = new MemoryStream();

            serializer.WriteObject(stream, proxy);
            stream.Seek(0, SeekOrigin.Begin);         
            
            return (T)serializer.ReadObject(stream);
        }
    }
}
