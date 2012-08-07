// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer.Utilities
{
    using System.Collections.Generic;
    using Xunit;

    public class IDictionaryExtensionsTests
    {
        [Fact]
        public void Add_creates_a_new_list_and_adds_value_if_list_for_key_does_not_exist()
        {
            var map = new Dictionary<int, IList<string>>();
            map.Add(1, "Sooty");

            Assert.Contains("Sooty", map[1]);
        }

        [Fact]
        public void Add_adds_value_to_existing_list_if_it_exists()
        {
            var map = new Dictionary<int, IList<string>>();
            map.Add(1, "Sooty");
            map.Add(1, "Sweep");

            Assert.Contains("Sooty", map[1]);
            Assert.Contains("Sweep", map[1]);
        }
    }
}
