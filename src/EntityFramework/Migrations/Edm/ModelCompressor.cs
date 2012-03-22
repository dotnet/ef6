namespace System.Data.Entity.Migrations.Edm
{
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.IO.Compression;
    using System.Xml.Linq;

    internal class ModelCompressor
    {
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public virtual byte[] Compress(XDocument model)
        {
            Contract.Requires(model != null);

            using (var outStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(outStream, CompressionMode.Compress))
                {
                    model.Save(gzipStream);
                }

                return outStream.ToArray();
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public virtual XDocument Decompress(byte[] bytes)
        {
            Contract.Requires(bytes != null);

            using (var memoryStream = new MemoryStream(bytes))
            {
                using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    return XDocument.Load(gzipStream);
                }
            }
        }
    }
}
