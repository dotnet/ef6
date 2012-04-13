namespace System.Data.Entity
{
    using System.Data.Entity.Infrastructure;
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;

    public static class DbModelExtensions
    {
        public static XDocument ToXDocument(this DbModel model)
        {
            var memoryStream = new MemoryStream();

            using (var xmlWriter = XmlWriter.Create(memoryStream, new XmlWriterSettings { Indent = true }))
            {
                EdmxWriter.WriteEdmx(model, xmlWriter);
            }

            memoryStream.Position = 0;

            return XDocument.Load(memoryStream);
        }
    }
}