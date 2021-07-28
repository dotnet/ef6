// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
[assembly: TestCaseOrderer("System.Data.Entity.TestHelpers.SimpleTestCaseOrderer", "EntityFramework.FunctionalTests.Transitional")]
[assembly: TestCollectionOrderer("System.Data.Entity.TestHelpers.SimpleTestCollectionOrderer", "EntityFramework.FunctionalTests.Transitional")]
