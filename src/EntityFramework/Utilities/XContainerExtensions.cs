namespace System.Data.Entity.Utilities
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Xml.Linq;

    internal static class XContainerExtensions
    {
        public static XElement GetOrAddElement(this XContainer container, XName name)
        {
            Contract.Requires(container != null);

            var child = container.Element(name);

            if (child == null)
            {
                child = new XElement(name);
                container.Add(child);
            }

            return child;
        }

        public static IEnumerable<XElement> Descendants(this XContainer container, IEnumerable<XName> name)
        {
            return name.SelectMany(container.Descendants);
        }

        public static IEnumerable<XElement> Elements(this XContainer container, IEnumerable<XName> name)
        {
            return name.SelectMany(container.Elements);
        }

        public static IEnumerable<XElement> Descendants<T>(this IEnumerable<T> source, IEnumerable<XName> name)
            where T : XContainer
        {
            return name.SelectMany(n => source.SelectMany(c => c.Descendants(n)));
        }
    }
}
