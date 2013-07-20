// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Xml;

    /// <summary>
    ///     Serializes an <see cref="EdmModel" /> that conforms to the restrictions of a single
    ///     CSDL schema file to an XML writer. The model to be serialized must contain a single
    ///     <see cref="Core.Metadata.Edm.EntityContainer" /> .
    /// </summary>
    public class CsdlSerializer
    {
        /// <summary>
        /// Occurs when an error is encountered serializing the model.
        /// </summary>
        public event EventHandler<DataModelErrorEventArgs> OnError;

        /// <summary>
        ///     Serialize the <see cref="EdmModel" /> to the XmlWriter.
        /// </summary>
        /// <param name="model">
        ///     The EdmModel to serialize.
        /// </param>
        /// <param name="xmlWriter"> The XmlWriter to serialize to </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public bool Serialize(EdmModel model, XmlWriter xmlWriter, string modelNamespace = null)
        {
            Check.NotNull(model, "model");
            Check.NotNull(xmlWriter, "xmlWriter");

            bool modelIsValid = true;
           
            Action<DataModelErrorEventArgs> onErrorAction =
                e =>
                {
                    modelIsValid = false;
                    if (OnError != null)
                    {
                        OnError(this, e);
                    }
                };

            if (model.NamespaceNames.Count() > 1
                || model.Containers.Count() != 1)
            {
                onErrorAction(
                    new DataModelErrorEventArgs
                    {
                        ErrorMessage = Strings.Serializer_OneNamespaceAndOneContainer,
                    });
            }

            // validate the model first
            var validator = new DataModelValidator();
            validator.OnError += (_, e) => onErrorAction(e);
            validator.Validate(model, true);

            if (modelIsValid)
            {
                new EdmSerializationVisitor(xmlWriter, model.SchemaVersion).Visit(model, modelNamespace);
                return true;
            }

            return false;
        }
    }
}
