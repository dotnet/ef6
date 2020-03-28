// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Objects;
    using Xunit;

    public class DbCompiledModelTests
    {
        public class GetConstructorDelegate
        {
            [Fact]
            public void Returns_delegate_using_any_constructor_with_which_EntityConnection_can_be_used()
            {
                Assert.IsType<Normal>(DbCompiledModel.GetConstructorDelegate<Normal>()(new EntityConnection()));
                Assert.IsType<WithDbConnction>(DbCompiledModel.GetConstructorDelegate<WithDbConnction>()(new EntityConnection()));
                Assert.IsType<WithIDbConnction>(DbCompiledModel.GetConstructorDelegate<WithIDbConnction>()(new EntityConnection()));
                Assert.IsType<WithIDisposable>(DbCompiledModel.GetConstructorDelegate<WithIDisposable>()(new EntityConnection()));
                Assert.IsType<WithComponent>(DbCompiledModel.GetConstructorDelegate<WithComponent>()(new EntityConnection()));
                Assert.IsType<WithMarshalByRefObject>(DbCompiledModel.GetConstructorDelegate<WithMarshalByRefObject>()(new EntityConnection()));
                Assert.IsType<WithObject>(DbCompiledModel.GetConstructorDelegate<WithObject>()(new EntityConnection()));
            }

            [Fact]
            public void Picks_best_match_constructor_for_EntityConnection_parameter()
            {
                Assert.Equal("EntityConnection", GetConstructorUsed<EntityConnectionWins>());
                Assert.Equal("DbConnection", GetConstructorUsed<DbConnectionWins>());
                Assert.Equal("IDbConnection", GetConstructorUsed<IDbConnectionWins>());
                Assert.Equal("IDisposable", GetConstructorUsed<IDisposableWins>());
                Assert.Equal("Component", GetConstructorUsed<ComponentWins>());
                Assert.Equal("MarshalByRefObject", GetConstructorUsed<MarshalByRefObjectWins>());
            }

            private static string GetConstructorUsed<TContext>() where TContext : ContextForConstruction
            {
                return ((TContext)DbCompiledModel.GetConstructorDelegate<TContext>()(new EntityConnection())).ConstructorUsed;
            }

            public class Normal : ObjectContext
            {
                public Normal(EntityConnection connection)
                {
                }
            }

            public class WithDbConnction : ObjectContext
            {
                public WithDbConnction(DbConnection connection)
                {
                }
            }

            public class WithIDbConnction : ObjectContext
            {
                public WithIDbConnction(IDbConnection connection)
                {
                }
            }

            public class WithIDisposable : ObjectContext
            {
                public WithIDisposable(IDisposable connection)
                {
                }
            }

            public class WithComponent : ObjectContext
            {
                public WithComponent(Component connection)
                {
                }
            }

            public class WithMarshalByRefObject : ObjectContext
            {
                public WithMarshalByRefObject(MarshalByRefObject connection)
                {
                }
            }

            public class WithObject : ObjectContext
            {
                public WithObject(object connection)
                {
                }
            }

            public class ContextForConstruction : ObjectContext
            {
                public string ConstructorUsed { get; set; }
            }

            public class EntityConnectionWins : ContextForConstruction
            {
                public EntityConnectionWins(EntityConnection connection)
                {
                    ConstructorUsed = "EntityConnection";
                }

                public EntityConnectionWins(DbConnection connection)
                {
                    ConstructorUsed = "DbConnection";
                }

                public EntityConnectionWins(IDbConnection connection)
                {
                    ConstructorUsed = "IDbConnection";
                }

                public EntityConnectionWins(IDisposable connection)
                {
                    ConstructorUsed = "IDisposable";
                }

                public EntityConnectionWins(Component connection)
                {
                    ConstructorUsed = "Component";
                }

                public EntityConnectionWins(MarshalByRefObject connection)
                {
                    ConstructorUsed = "MarshalByRefObject";
                }

                public EntityConnectionWins(object connection)
                {
                    ConstructorUsed = "object";
                }
            }

            public class DbConnectionWins : ContextForConstruction
            {
                public DbConnectionWins(DbConnection connection)
                {
                    ConstructorUsed = "DbConnection";
                }

                public DbConnectionWins(IDbConnection connection)
                {
                    ConstructorUsed = "IDbConnection";
                }

                public DbConnectionWins(IDisposable connection)
                {
                    ConstructorUsed = "IDisposable";
                }

                public DbConnectionWins(Component connection)
                {
                    ConstructorUsed = "Component";
                }

                public DbConnectionWins(MarshalByRefObject connection)
                {
                    ConstructorUsed = "MarshalByRefObject";
                }

                public DbConnectionWins(object connection)
                {
                    ConstructorUsed = "object";
                }
            }

            public class IDbConnectionWins : ContextForConstruction
            {
                public IDbConnectionWins(IDbConnection connection)
                {
                    ConstructorUsed = "IDbConnection";
                }

                public IDbConnectionWins(IDisposable connection)
                {
                    ConstructorUsed = "IDisposable";
                }

                public IDbConnectionWins(Component connection)
                {
                    ConstructorUsed = "Component";
                }

                public IDbConnectionWins(MarshalByRefObject connection)
                {
                    ConstructorUsed = "MarshalByRefObject";
                }

                public IDbConnectionWins(object connection)
                {
                    ConstructorUsed = "object";
                }
            }

            public class IDisposableWins : ContextForConstruction
            {
                public IDisposableWins(IDisposable connection)
                {
                    ConstructorUsed = "IDisposable";
                }

                public IDisposableWins(Component connection)
                {
                    ConstructorUsed = "Component";
                }

                public IDisposableWins(MarshalByRefObject connection)
                {
                    ConstructorUsed = "MarshalByRefObject";
                }

                public IDisposableWins(object connection)
                {
                    ConstructorUsed = "object";
                }
            }

            public class ComponentWins : ContextForConstruction
            {
                public ComponentWins(Component connection)
                {
                    ConstructorUsed = "Component";
                }

                public ComponentWins(MarshalByRefObject connection)
                {
                    ConstructorUsed = "MarshalByRefObject";
                }

                public ComponentWins(object connection)
                {
                    ConstructorUsed = "object";
                }
            }

            public class MarshalByRefObjectWins : ContextForConstruction
            {
                public MarshalByRefObjectWins(MarshalByRefObject connection)
                {
                    ConstructorUsed = "MarshalByRefObject";
                }

                public MarshalByRefObjectWins(object connection)
                {
                    ConstructorUsed = "object";
                }
            }
        }
    }
}
