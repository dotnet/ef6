// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Text;
    using Moq;
    using Moq.Protected;

    /// <summary>
    /// Helper class that uses Moq to create mocks for the InternalContext and related classes such
    /// that different database initialization strategies can be tested and the operations that these
    /// strategies perform can be recorded and validated.
    /// </summary>
    public class DatabaseInitializerTracker<TContext, TInitializer>
        where TInitializer : class, IDatabaseInitializer<TContext>
        where TContext : DbContext, new()
    {
        private readonly StringBuilder _operations = new StringBuilder();

        private readonly Mock<InternalContextForMock<TContext>> _mockInternalContext;
        private readonly Mock<DatabaseOperations> _mockDatabaseOps;
        private readonly Mock<TContext> _mockDbContext;
        private readonly Mock<TInitializer> _mockStrategy;
        private bool _databaseExists;

        internal DatabaseInitializerTracker(
            bool databaseExists, 
            bool modelCompatible = true, 
            bool hasMetadata = true,
            MigrationsChecker checker = null)
        {
            _databaseExists = databaseExists;
            _mockInternalContext = new Mock<InternalContextForMock<TContext>>
                                       {
                                           CallBase = true
                                       };
            _mockDatabaseOps = new Mock<DatabaseOperations>();
            _mockDbContext = Mock.Get((TContext)_mockInternalContext.Object.Owner);

            _mockInternalContext.Setup(c => c.DatabaseOperations).Returns(_mockDatabaseOps.Object);
            _mockInternalContext.Setup(c => c.DefaultInitializer).Returns(new CreateDatabaseIfNotExists<DbContext>());
            _mockInternalContext.Setup(c => c.CreateDatabase(It.IsAny<ObjectContext>())).Callback(
                () =>
                    {
                        _databaseExists = true;
                        _operations.Append("CreateDatabase ");
                    });

            _mockDatabaseOps.Setup(d => d.Create(It.IsAny<ObjectContext>())).Callback(
                () =>
                    {
                        _databaseExists = true;
                        _operations.Append("Create ");
                    });

            _mockDatabaseOps.Setup(d => d.Exists(It.IsAny<ObjectContext>())).Callback(() => _operations.Append("Exists ")).Returns(
                DatabaseExists);
            _mockDatabaseOps.Setup(d => d.DeleteIfExists(It.IsAny<ObjectContext>())).Callback(() => _operations.Append("DeleteIfExists ")).
                Returns(DeleteIfExists);

            _mockInternalContext.Setup(c => c.UseTempObjectContext()).Callback(() => _operations.Append("UseTempObjectContext "));
            _mockInternalContext.Setup(c => c.DisposeTempObjectContext()).Callback(() => _operations.Append("DisposeTempObjectContext "));
            _mockInternalContext.Setup(c => c.SaveMetadataToDatabase()).Callback(() => _operations.Append("SaveMetadataToDatabase "));

            _mockInternalContext.Setup(c => c.CompatibleWithModel(It.IsAny<bool>())).Callback(
                (bool throwIfNoMetadata) =>
                    {
                        if (!hasMetadata && throwIfNoMetadata)
                        {
                            throw Error.Database_NoDatabaseMetadata();
                        }
                    }).Returns(modelCompatible);

            _mockInternalContext.Setup(c => c.CreateObjectContextForDdlOps()).Returns(new Mock<ClonedObjectContext>().Object);
            _mockInternalContext.SetupGet(c => c.ProviderName).Returns("Dummy.Data.Provider");

            _mockStrategy = new Mock<TInitializer>(checker)
                                {
                                    CallBase = true
                                };
            _mockStrategy.Protected().Setup("Seed", ItExpr.IsAny<TContext>()).Callback(() => _operations.Append("Seed "));
        }

        private bool DeleteIfExists()
        {
            var exists = _databaseExists;
            _databaseExists = false;
            return exists;
        }

        private bool DatabaseExists()
        {
            return _databaseExists;
        }

        public void ExecuteStrategy()
        {
            _mockStrategy.Object.InitializeDatabase(_mockDbContext.Object);
        }

        public void RegisterStrategy()
        {
            var mockContextType = _mockDbContext.Object.GetType();
            var initMethod = typeof(Database).GetMethod("SetInitializer").MakeGenericMethod(mockContextType);
            initMethod.Invoke(null, new object[] { _mockStrategy.Object });
        }

        public string Result
        {
            get { return _operations.ToString().Trim(); }
        }

        public TContext Context
        {
            get { return _mockDbContext.Object; }
        }

        internal Mock<InternalContextForMock<TContext>> MockInternalContext
        {
            get { return _mockInternalContext; }
        }
    }
}
