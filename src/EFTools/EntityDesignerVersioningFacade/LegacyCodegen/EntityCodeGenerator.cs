// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Sded = System.Data.Entity.Design;

namespace Microsoft.Data.Entity.Design.VersioningFacade.LegacyCodegen
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.IO;
    using System.Xml;

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    internal sealed class EntityCodeGenerator : CodeGeneratorBase
    {
        private readonly Sded.EntityCodeGenerator _entityCodeGenerator;
        private readonly Version _targetEntityFrameworkVersion;

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="languageOption">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <param name="targetEntityFrameworkVersion">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        public EntityCodeGenerator(LanguageOption languageOption, Version targetEntityFrameworkVersion)
        {
            _entityCodeGenerator = new Sded.EntityCodeGenerator((Sded.LanguageOption)languageOption);
            _targetEntityFrameworkVersion = targetEntityFrameworkVersion;
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="edmNamespace">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <param name="objectNamespace">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        public override void AddNamespaceMapping(string edmNamespace, string objectNamespace)
        {
            _entityCodeGenerator.EdmToObjectNamespaceMap.Add(edmNamespace, objectNamespace);
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="sourceEdmSchema">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <param name="target">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <returns>This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</returns>
        public override IList<EdmSchemaError> GenerateCode(XmlReader sourceEdmSchema, TextWriter target)
        {
            return FromLegacySchemaErrors(_entityCodeGenerator.GenerateCode(sourceEdmSchema, target, _targetEntityFrameworkVersion));
        }
    }
}
