// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.DependencyResolution
{
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;

    internal class ClrTypeAnnotationSerializer : IMetadataAnnotationSerializer
    {
        public string Serialize(string name, object value)
        {
            DebugCheck.NotEmpty(name);
            DebugCheck.NotNull(value);

            return ((Type)value).AssemblyQualifiedName;
        }

        public object Deserialize(string name, string value)
        {
            DebugCheck.NotEmpty(name);
            DebugCheck.NotNull(value);

            // We avoid throwing here if the type could not be loaded because we might be loading an
            // old EDMX from, for example, the MigrationHistory table, and the CLR type might no longer exist.
            // Note that the exceptions caught below can be thrown even when "throwOnError" is false.
            try
            {
                return Type.GetType(value, throwOnError: false);
            }
            catch (FileLoadException)
            {
            }
            catch (TargetInvocationException)
            {
            }
            catch (BadImageFormatException)
            {
            }

            return null;
        }
    }
}
