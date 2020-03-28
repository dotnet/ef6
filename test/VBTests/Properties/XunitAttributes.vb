' Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

Imports Xunit

<Assembly: CollectionBehavior(DisableTestParallelization:=True)>
<Assembly: TestCaseOrderer("System.Data.Entity.TestHelpers.SimpleTestCaseOrderer", "EntityFramework.FunctionalTests.Transitional")>
<Assembly: TestCollectionOrderer("System.Data.Entity.TestHelpers.SimpleTestCollectionOrderer", "EntityFramework.FunctionalTests.Transitional")>
