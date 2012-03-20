namespace System.Data.Entity.Migrations.Extensions
{
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;

    internal static class DbContextExtensions
    {
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public static XDocument GetModel(this DbContext context)
        {
            Contract.Requires(context != null);

            return GetModel(w => EdmxWriter.WriteEdmx(context, w));
        }

        public static XDocument GetModel(this DbModel model)
        {
            Contract.Requires(model != null);

            return GetModel(w => EdmxWriter.WriteEdmx(model, w));
        }

        private static XDocument GetModel(Action<XmlWriter> writeXml)
        {
            Contract.Requires(writeXml != null);

            using (var memoryStream = new MemoryStream())
            {
                using (var xmlWriter = XmlWriter.Create(memoryStream, new XmlWriterSettings { Indent = true }))
                {
                    writeXml(xmlWriter);
                }

                memoryStream.Position = 0;

                return XDocument.Load(memoryStream);
            }
        }
    }
}