// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Objects
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
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
        }

        [Fact]
        public void Change_tracking_proxy_can_be_binary_deserialized_when_running_under_full_trust()
        {
            using (var context = new ProxiesContext())
            {
                var proxy = context.MeTrackChanges.Create();
                Assert.True(proxy is IEntityWithRelationships);

                proxy.Id = 77;

                var stream = new MemoryStream();
                var formatter = new BinaryFormatter();

                formatter.Serialize(stream, proxy);
                stream.Seek(0, SeekOrigin.Begin);
                var deserialized = (MeTrackChangesS)formatter.Deserialize(stream);

                Assert.Same(proxy.GetType(), deserialized.GetType());
                Assert.Equal(77, deserialized.Id);
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

                var stream = new MemoryStream();
                var formatter = new BinaryFormatter();

                formatter.Serialize(stream, proxy);
                stream.Seek(0, SeekOrigin.Begin);
                var deserialized = (MeLazyLoadS)formatter.Deserialize(stream);

                Assert.Same(proxy.GetType(), deserialized.GetType());
                Assert.Equal(77, deserialized.Id);
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

                var stream = new MemoryStream();
                var serializer = new DataContractSerializer(
                    typeof(MeTrackChangesS), null, int.MaxValue, false, true, null, new ProxyDataContractResolver());

                serializer.WriteObject(stream, proxy);
                stream.Seek(0, SeekOrigin.Begin);
                var deserialized = (MeTrackChangesS)serializer.ReadObject(stream);

                Assert.IsType<MeTrackChangesS>(deserialized); // Resolver returns non-proxy type
                Assert.Equal(77, deserialized.Id);
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

                var stream = new MemoryStream();
                var serializer = new DataContractSerializer(
                    typeof(MeLazyLoadS), null, int.MaxValue, false, true, null, new ProxyDataContractResolver());

                serializer.WriteObject(stream, proxy);
                stream.Seek(0, SeekOrigin.Begin);
                var deserialized = (MeLazyLoadS)serializer.ReadObject(stream);

                Assert.IsType<MeLazyLoadS>(deserialized); // Resolver returns non-proxy type
                Assert.Equal(77, deserialized.Id);
            }
        }

        [Fact]
        public void Lazy_loading_proxy_can_be_data_contract_deserialized_with_known_types_when_running_under_full_trust()
        {
            using (var context = new ProxiesContext())
            {
                var proxy = context.MeLazyLoads.Create();
                Assert.False(proxy is IEntityWithRelationships);
                proxy.Id = 77;

                var otherProxy = context.MeTrackChanges.Create();

                var stream = new MemoryStream();
                var serializer = new DataContractSerializer(proxy.GetType(), new[] { proxy.GetType(), otherProxy.GetType() }, int.MaxValue, false, true, null);

                serializer.WriteObject(stream, proxy);
                stream.Seek(0, SeekOrigin.Begin);
                var deserialized = (MeLazyLoadS)serializer.ReadObject(stream);

                Assert.Same(proxy.GetType(), deserialized.GetType());
                Assert.Equal(77, deserialized.Id);
            }
        }
    }

    [Serializable]
    public class MeTrackChangesS
    {
        public virtual int Id { get; set; }
        public virtual ICollection<MeLazyLoadS> MeLazyLoad { get; set; }
    }

    [Serializable]
    public class MeLazyLoadS
    {
        public int Id { get; set; }
        public virtual MeTrackChangesS MeTrackChanges { get; set; }
    }
}
