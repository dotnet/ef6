// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Generic;
    using System.Linq;
    using Moq;

    /// <summary>
    /// This is an implementation of the abstract <see cref="InternalPropertyValues" /> class that
    /// is based on a simple dictionary of property values.  Instances of this class are used to
    /// test the functionality of the abstract class in unit tests.
    /// It was possible to do everything here with Moq, but it was easier and clearer in this case
    /// to create a simple implementation instead.
    /// </summary>
    /// <typeparam name="T"> The type of object that the dictionary contains. </typeparam>
    internal class TestInternalPropertyValues<T> : InternalPropertyValues
    {
        private readonly ISet<string> _propertyNames;

        private readonly IDictionary<string, Mock<IPropertyValuesItem>> _propertyValues =
            new Dictionary<string, Mock<IPropertyValuesItem>>();

        public TestInternalPropertyValues(
            IDictionary<string, object> properties = null, IEnumerable<string> complexProperties = null, bool isEntityValues = false)
            : base(new Mock<InternalContextForMock>().Object, typeof(T), isEntityValues)
        {
            var names = new HashSet<string>();

            if (properties != null)
            {
                foreach (var property in properties)
                {
                    var name = property.Key;
                    names.Add(name);

                    var itemMock = new Mock<IPropertyValuesItem>();
                    itemMock.SetupGet(i => i.Name).Returns(name);
                    itemMock.SetupProperty(i => i.Value, property.Value);
                    itemMock.SetupGet(i => i.IsComplex).Returns(complexProperties != null && complexProperties.Contains(name));
                    itemMock.SetupGet(i => i.Type).Returns(property.Value != null ? property.Value.GetType() : typeof(object));

                    _propertyValues[name] = itemMock;
                }
            }

            _propertyNames = new ReadOnlySet<string>(names);
        }

        public Mock<InternalContextForMock> MockInternalContext
        {
            get { return Mock.Get((InternalContextForMock)InternalContext); }
        }

        public Mock<IPropertyValuesItem> GetMockItem(string propertyName)
        {
            return _propertyValues[propertyName];
        }

        protected override IPropertyValuesItem GetItemImpl(string propertyName)
        {
            return _propertyValues[propertyName].Object;
        }

        public override ISet<string> PropertyNames
        {
            get { return _propertyNames; }
        }
    }
}
