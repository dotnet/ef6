// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal.ConfigFile
{
    using System.Linq;
    using Xunit;

    public class ParameterCollectionTests : TestBase
    {
        [Fact]
        public void ParameterCollection_converts_valid_types()
        {
            var coll = new ParameterCollection();
            var p1 = coll.NewElement();
            p1.ValueString = "Test";
            p1.TypeName = "System.String";
            var p2 = coll.NewElement();
            p2.ValueString = "true";
            p2.TypeName = "System.Boolean";

            var parameters = coll.GetTypedParameterValues();
            Assert.Equal(2, parameters.Count());
            Assert.Equal("Test", parameters.ElementAt(0));
            Assert.Equal(true, parameters.ElementAt(1));
        }

        [Fact]
        public void ParameterCollection_throws_converting_to_invalid_type()
        {
            var coll = new ParameterCollection();
            var p1 = coll.NewElement();
            p1.ValueString = "MyValue";
            p1.TypeName = "Not.A.Type";

            Assert.True(Assert.Throws<TypeLoadException>(() => coll.GetTypedParameterValues()).Message.Contains("Not.A.Type"));
        }

        [Fact]
        public void ParameterCollection_throws_converting_to_incompatible_type()
        {
            var coll = new ParameterCollection();
            var p1 = coll.NewElement();
            p1.ValueString = "MyValue";
            p1.TypeName = "System.Int32";

            Assert.Throws<FormatException>(() => coll.GetTypedParameterValues());
        }
    }
}
