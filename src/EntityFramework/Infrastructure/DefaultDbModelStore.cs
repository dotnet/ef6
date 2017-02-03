// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Utilities;
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;

    /// <summary>
    /// Loads or saves models from/into .edmx files at a specified location.
    /// </summary>
    public class DefaultDbModelStore : DbModelStore
    {
        private const string FileExtension = ".edmx";

        private readonly string _directory;

        /// <summary>
        /// Initializes a new DefaultDbModelStore instance.
        /// </summary>
        /// <param name="directory">The parent directory for the .edmx files.</param>
        public DefaultDbModelStore(string directory)
        {
            Check.NotEmpty(directory, "directory");

            _directory = directory;
        }

        /// <summary>
        /// Gets the location of the .edmx files.
        /// </summary>
        public string Directory
        {
            get { return _directory; }
        }

        /// <summary>
        /// Loads a model from the store.
        /// </summary>
        /// <param name="contextType">The type of context representing the model.</param>
        /// <returns>The loaded metadata model.</returns>
        public override DbCompiledModel TryLoad(Type contextType)
        {
            return LoadXml(
                contextType,
                reader =>
                {
                    var defaultSchema = GetDefaultSchema(contextType);
                    return EdmxReader.Read(reader, defaultSchema);
                });
        }

        /// <summary>
        /// Retrieves an edmx XDocument version of the model from the store.
        /// </summary>
        /// <param name="contextType">The type of context representing the model.</param>
        /// <returns>The loaded XDocument edmx.</returns>
        public override XDocument TryGetEdmx(Type contextType)
        {
            return LoadXml(contextType, XDocument.Load);
        }

        internal T LoadXml<T>(Type contextType, Func<XmlReader, T> xmlReaderDelegate)
        {
            var filePath = GetFilePath(contextType);

            if (!File.Exists(filePath))
            {
                return default(T);
            }

            if (!FileIsValid(contextType, filePath))
            {
                File.Delete(filePath);
                return default(T);
            }

            using (var reader = XmlReader.Create(filePath))
            {
                return xmlReaderDelegate(reader);
            }
        }

        /// <summary>
        /// Saves a model to the store.
        /// </summary>
        /// <param name="contextType">The type of context representing the model.</param>
        /// <param name="model">The metadata model to save.</param>
        public override void Save(Type contextType, DbModel model)
        {
            using (var writer = XmlWriter.Create(GetFilePath(contextType), 
                new XmlWriterSettings
                    {
                        Indent = true
                    }))
            {
                EdmxWriter.WriteEdmx(model, writer);
            }
        }

        /// <summary>
        /// Gets the path of the .edmx file corresponding to the specified context type.
        /// </summary>
        /// <param name="contextType">A context type.</param>
        /// <returns>The .edmx file path.</returns>
        protected virtual string GetFilePath(Type contextType)
        {
            var fileName = contextType.FullName + FileExtension;

            return Path.Combine(_directory, fileName);
        }

        /// <summary>
        /// Validates the model store is valid.
        /// The default implementation verifies that the .edmx file was last 
        /// written after the context assembly was last written.
        /// </summary>
        /// <param name="contextType">The type of context representing the model.</param>
        /// <param name="filePath">The path of the stored model.</param>
        /// <returns>Whether the edmx file should be invalidated.</returns>
        protected virtual bool FileIsValid(Type contextType, string filePath)
        {
            var contextCreated =
                File.GetLastWriteTimeUtc(contextType.Assembly.Location);
            var storeCreated = File.GetLastWriteTimeUtc(filePath);
            return storeCreated >= contextCreated;
        }
    }
}
