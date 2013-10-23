// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Sded = System.Data.Entity.Design;

namespace Microsoft.Data.Entity.Design.VersioningFacade.LegacyCodegen
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.IO;
    using System.Xml;

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    internal sealed class EntityClassGenerator : CodeGeneratorBase
    {
        private readonly Sded.EntityClassGenerator _entityClassGenerator;

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="languageOption">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        public EntityClassGenerator(LanguageOption languageOption)
        {
            _entityClassGenerator = new Sded.EntityClassGenerator((Sded.LanguageOption)languageOption);
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="edmNamespace">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <param name="objectNamespace">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        public override void AddNamespaceMapping(string edmNamespace, string objectNamespace)
        {
            _entityClassGenerator.EdmToObjectNamespaceMap.Add(edmNamespace, objectNamespace);
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="sourceEdmSchema">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <param name="target">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <returns>This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</returns>
        public override IList<EdmSchemaError> GenerateCode(XmlReader sourceEdmSchema, TextWriter target)
        {
            return FromLegacySchemaErrors(_entityClassGenerator.GenerateCode(sourceEdmSchema, target));
        }
    }
}
