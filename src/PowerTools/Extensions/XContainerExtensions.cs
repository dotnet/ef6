namespace Microsoft.DbContextPackage.Extensions
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Xml.Linq;

    internal static class XContainerExtensions
    {
        /// <summary>
        /// Gets the first (in document order) child element with the specified local name and one of the namespaces.
        /// </summary>
        /// <param name="container">The node containing the child elements.</param>
        /// <param name="namespaces">A collection of namespaces used when searching for the child element.</param>
        /// <param name="localName">The local (unqualified) name to match.</param>
        /// <returns>A <see cref="XElement" /> that matches the specified name and namespace, or null.</returns>
        public static XElement Element(this XContainer container, IEnumerable<XNamespace> namespaces, string localName)
        {
            Contract.Requires(container != null);
            Contract.Requires(namespaces != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(localName));

            return container.Elements()
                .FirstOrDefault(
                    e => e.Name.LocalName == localName
                        && namespaces.Contains(e.Name.Namespace));
        }
    }
}
