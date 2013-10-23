// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using LegacyMetadata = System.Data.Metadata.Edm;

namespace Microsoft.Data.Entity.Design.VersioningFacade.LegacyCodegen
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Xml;

    internal abstract class CodeGeneratorBase
    {
        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="edmNamespace">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <param name="objectNamespace">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        public abstract void AddNamespaceMapping(string edmNamespace, string objectNamespace);

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="sourceEdmSchema">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <param name="target">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <returns>This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</returns>
        public abstract IList<EdmSchemaError> GenerateCode(XmlReader sourceEdmSchema, TextWriter target);

        protected static IList<EdmSchemaError> FromLegacySchemaErrors(IEnumerable<LegacyMetadata.EdmSchemaError> legacyEdmSchemaErrors)
        {
            return
                legacyEdmSchemaErrors == null
                    ? null
                    : legacyEdmSchemaErrors.Select(
                        e => new EdmSchemaError(
                                 e.Message, e.ErrorCode,
                                 e.Severity == LegacyMetadata.EdmSchemaErrorSeverity.Warning
                                     ? EdmSchemaErrorSeverity.Warning
                                     : EdmSchemaErrorSeverity.Error,
                                 e.SchemaLocation, e.Line, e.Column)).ToList();
        }

        public static CodeGeneratorBase Create(LanguageOption language, Version targetEntityFrameworkVersion)
        {
            Debug.Assert(
                EntityFrameworkVersion.IsValidVersion(targetEntityFrameworkVersion),
                "invalid targetEntityFrameworkVersion");

            return targetEntityFrameworkVersion == EntityFrameworkVersion.Version1
                       ? (CodeGeneratorBase)new EntityClassGenerator(language)
                       : new EntityCodeGenerator(language, targetEntityFrameworkVersion);
        }
    }
}
