// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Extensions
{
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Xml.Linq;

    internal static class XContainerExtensions
    {
        public static XElement GetOrCreateElement(
            this XContainer container, string elementName, params XAttribute[] attributes)
        {
            DebugCheck.NotNull(container);
            DebugCheck.NotEmpty(elementName);

            var element = container.Element(elementName);
            if (element == null)
            {
                element = new XElement(elementName, attributes);
                container.Add(element);
            }
            return element;
        }

        public static XElement GetOrCreateElementWithSpecificAttribute(
            this XContainer container, string elementName, XAttribute requiredAttribute, params XAttribute[] attributes)
        {
            DebugCheck.NotNull(container);
            DebugCheck.NotEmpty(elementName);

            var element = container.Elements(elementName)
                .FirstOrDefault(e => e.Attributes(requiredAttribute.Name).Any(a => a.Value == requiredAttribute.Value));

            if (element == null)
            {
                element = new XElement(elementName, new[] { requiredAttribute }.Concat(attributes));
                container.Add(element);
            }
            return element;
        }
    }
}
